﻿


//#define OLD

using System;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
//using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swcommands;
//using SolidWorksTools;

namespace SWCube
{

    public class SW_Cube : ISwAddin
    {
        //Constants
        double MAX_THETA_DIFF_UNLOCK = 0.01;
        double MAX_AXIS_DIFF_UNLOCK = 0.0001;
        int sFPS = 1000;
        String CONNECT_MESSAGE = "Solid";
        String GET_QUAT_MESSAGE = "getQuat000";
        String DISCONNECT_MESSAGE = "Disconnect0";
        String CONNECTED_REPLY = "Connected";

        //Delegates
        public delegate void SimpleDelegate();

        //solid vars
        public SldWorks _swApp;
        MathUtility swMathUtility;
        
        private int mSWCookie;
        int solidFrameInterval;
        System.Threading.Timer solidFrameTimerT;
        System.Windows.Forms.Timer solidFrameTimer;
        bool solidDoc = false;
        static Mutex solidFrameMutex = new Mutex();

        //static vars
        Quaternion displayQuat;
        Quaternion lastDisplayQuat;
        bool mpuStable = false;
        Control controlThread;

        //comm vars
        TcpClient client;
        NetworkStream clientStream;
        bool clientConnected = false;

        //debug vars
        Stopwatch stopWatch;
        double[] times;

        [ComRegisterFunction()]
        private static void ComRegister(Type t)
        {
            string keyPath = String.Format(@"SOFTWARE\SolidWorks\AddIns\{0:b}", t.GUID);
            var baseReg = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64);
            //32 registry. goes to HKEY_LOCALMACHINE/SOFTWARE/WOW6432Node on 64 bit machines.
            //using (Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.CreateSubKey(keyPath))
            using (Microsoft.Win32.RegistryKey rk = baseReg.CreateSubKey(keyPath))
            {
                rk.SetValue(null, 1); // Load at startup
                rk.SetValue("Title", "SW Cube"); // Title
                rk.SetValue("Description", "All your pixels belong to us!"); // Description
            }
        }

        [ComUnregisterFunction()]
        private static void ComUnregister(Type t)
        {
            string keyPath = String.Format(@"SOFTWARE\SolidWorks\AddIns\{0:b}", t.GUID);
            var baseReg = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64);
            baseReg.DeleteSubKeyTree(keyPath);
        }

        public bool ConnectToSW(object ThisSW, int Cookie)
        {
            _swApp = (SldWorks)ThisSW;
            mSWCookie = Cookie;
            // Set-up add-in call back info
            bool result = _swApp.SetAddinCallbackInfo(0, this, Cookie);
            swMathUtility = (MathUtility)_swApp.GetMathUtility();
            controlThread = new Control();
            controlThread.CreateControl();
            _swApp.SendMsgToUser2("Press OK when server is up!",
                (int)swMessageBoxIcon_e.swMbInformation,
                (int)swMessageBoxBtn_e.swMbOk);
            //TODO: add timer to repeatedly trying to connect.
            ConnectClient();
            StartTimer();
            return true;
        }

        public bool DisconnectFromSW()
        {
            //TODO: make this stop-mutex protected and stop timer!
            //TODO: dispose of control
            //solidFrameTimerT.Stop();
            clientStream.Close();
            client.Close();
            return true;
        }

        private void StartTimer()
        {
            //TODO add stopwatch to make sure not rnning at too high paste
            solidFrameInterval = 1;//(int)(1000 / sFPS);
#if (OLD)
            solidFrameTimer = new System.Windows.Forms.Timer();
            solidFrameTimer.Tick += new EventHandler(SolidFrameOld);
            solidFrameTimer.Interval = solidFrameInterval;
            solidFrameTimer.Start();
#else
            solidFrameTimerT = new System.Threading.Timer(SolidFrameT, null, solidFrameInterval, solidFrameInterval);
#endif            
            Debug.WriteLine("Timer started");
        }

        private void ConnectClient()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 8090);
                Debug.WriteLine("Cube Client Connected!");
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(CONNECT_MESSAGE);
                clientStream = client.GetStream();
                clientStream.Write(data, 0, data.Length);
                while (true)
                {
                    //TODO: wait to complete the data
                    data = new Byte[256];

                    // String to store the response ASCII representation.
                    String responseData = String.Empty;

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = clientStream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    if (responseData == "Connected")
                    {
                        clientConnected = true;
                        break;
                    }
                    else
                    {
                        Debug.WriteLine("Error connecting! got: " + responseData);
                    }
                }

            }
            catch (ArgumentNullException e)
            {
                Debug.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Debug.WriteLine("SocketException: {0}", e);
            }
        }
        
        //worker thread method for setting up things to update the frame
        private void SolidFrameT(object myObject)//Vector3D a, Double theta)
        {
            if (solidFrameMutex.WaitOne(0))
            {
                Debug.WriteLine("Solid Timer Tick");
                if (!clientConnected)
                {
                    solidFrameMutex.ReleaseMutex();
                    return;
                }
                stopWatch = new Stopwatch();
                times = new double[8];
                stopWatch.Start();
                GetCorrectedQuat();
                times[0] = stopWatch.ElapsedMilliseconds;
                if (!mpuStable || !MovementFilter())
                {
                    solidFrameMutex.ReleaseMutex();
                    return;
                }
                lastDisplayQuat = displayQuat;

                times[1] = stopWatch.ElapsedMilliseconds;
                controlThread.Invoke(new EventHandler(SolidFrame));
                times[7] = stopWatch.ElapsedMilliseconds;
                Debug.WriteLine("solid: {0} {1} {2} {3} {4} {5} {6} {7} total: {8}", times[0], times[1] - times[0], times[2] - times[1],
                times[3] - times[2], times[4] - times[3], times[5] - times[4], times[6] - times[5], times[7] - times[6], times[7]);

                stopWatch.Stop();
                solidFrameMutex.ReleaseMutex();            
            }
            else
            {
                Debug.WriteLine("SolidFrameMutex Drop!");
            }
        }

        //method for updating the solid cam view
        private void SolidFrame(object myObject, EventArgs myEventArgs)
        {
            times[2] = stopWatch.ElapsedMilliseconds;
            try
            {
                if (!solidDoc)
                {
                    if (_swApp.ActiveDoc != null)
                    {
                        solidDoc = true;
                    }
                }
                //avoiding exceptions if possible                        
                if (solidDoc)
                {
                    IModelDoc doc = _swApp.ActiveDoc;
                    try
                    {
                        IModelView view = doc.ActiveView;
                        //TODO: translate :(
                        //15-23 ms no need to translate just yet!
                        //MathTransform translate = view.Translation3;
                        //TODO: rescale :(
                        //no need to rescale yet either
                        //double scale = view.Scale2;
                        double[,] rotation = QuatToRotation(displayQuat);
                        double[] tempArr = new double[16];
                        //new X axis
                        tempArr[0] = rotation[0, 0];
                        tempArr[1] = rotation[1, 0];
                        tempArr[2] = rotation[2, 0];
                        //new Y axis
                        tempArr[3] = rotation[0, 1];
                        tempArr[4] = rotation[1, 1];
                        tempArr[5] = rotation[2, 1];
                        //new Z axis
                        tempArr[6] = rotation[0, 2];
                        tempArr[7] = rotation[1, 2];
                        tempArr[8] = rotation[2, 2];
                        //translation - doesn't mater for orientation!
                        tempArr[9] = 0;
                        tempArr[10] = 0;
                        tempArr[11] = 0;
                        //scale - doesn't mater for orientation!
                        tempArr[12] = 1;
                        tempArr[13] = 0;
                        tempArr[14] = 0;
                        tempArr[15] = 0;

                        times[3] = stopWatch.ElapsedMilliseconds;
                        MathTransform orientation = swMathUtility.CreateTransform(new double[1]); 
                        orientation.ArrayData = tempArr;
                        view.Orientation3 = orientation;
                        //view.RotateAboutCenter(0, 0);
                        times[4] = stopWatch.ElapsedMilliseconds;
                        view.GraphicsRedraw(new int[] { });
                        times[5] = stopWatch.ElapsedMilliseconds;
                    }
                    //no active view
                    catch (Exception ex)
                    {
                        solidDoc = false;
                        Debug.WriteLine("Unable to rotate Solid Camera!\n" + ex.ToString());
                    }
                }
            }
            //no _swApp
            catch (Exception ex)
            {
                Debug.WriteLine("Oh no! Something went wrong with Solid!\n" + ex.ToString());
            }
            times[6] = stopWatch.ElapsedMilliseconds;
        }

        //comtlete method for updating the solid cam view - non threaded
        private void SolidFrameOld(object myObject, EventArgs myEventArgs)
        {
            Debug.WriteLine("Solid Timer Tick");
            if (!clientConnected)
            {
                return;
            }

            Stopwatch stopWatch = new Stopwatch();
            double[] times = new double[8];
            stopWatch.Start();

            GetCorrectedQuat();

            if (!mpuStable || !MovementFilter())
            {
                return;
            }
            lastDisplayQuat = displayQuat;

            //0 ms
            times[0] = stopWatch.ElapsedMilliseconds;
            try
            {
                if (!solidDoc)
                {
                    //5-19 ms
                    if (_swApp.ActiveDoc != null)
                    {
                        solidDoc = true;
                    }
                }
                //avoiding exceptions if possible                        
                if (solidDoc)
                {
                    times[1] = stopWatch.ElapsedMilliseconds;
                    //5-14 ms
                    IModelDoc doc = _swApp.ActiveDoc;
                    try
                    {
                        times[2] = stopWatch.ElapsedMilliseconds;
                        //4-6 ms somehow solid won't allow this to happen at once
                        IModelView view = doc.ActiveView;
                        times[3] = stopWatch.ElapsedMilliseconds;
                        double[,] rotation = QuatToRotation(displayQuat);
                        //TODO: translate :(
                        //15-23 ms no need to translate just yet!
                        //MathTransform translate = view.Translation3;
                        //TODO: rescale :(
                        //no need to rescale yet either
                        //double scale = view.Scale2;
                        times[4] = stopWatch.ElapsedMilliseconds;
                        double[] tempArr = new double[16];
                        //new X axis
                        tempArr[0] = rotation[0, 0];
                        tempArr[1] = rotation[1, 0];
                        tempArr[2] = rotation[2, 0];
                        //new Y axis
                        tempArr[3] = rotation[0, 1];
                        tempArr[4] = rotation[1, 1];
                        tempArr[5] = rotation[2, 1];
                        //new Z axis
                        tempArr[6] = rotation[0, 2];
                        tempArr[7] = rotation[1, 2];
                        tempArr[8] = rotation[2, 2];
                        //translation - doesn't mater for orientation!
                        tempArr[9] = 0;
                        tempArr[10] = 0;
                        tempArr[11] = 0;
                        //scale - doesn't mater for orientation!
                        tempArr[12] = 1;
                        //?
                        tempArr[13] = 0;
                        tempArr[14] = 0;
                        tempArr[15] = 0;
                        //? ms
                        MathTransform orientation = swMathUtility.CreateTransform(new double[1]);
                        orientation.ArrayData = tempArr;
                        times[5] = stopWatch.ElapsedMilliseconds;
                        //? ms
                        view.Orientation3 = orientation;
                        times[6] = stopWatch.ElapsedMilliseconds;
                        //? ms
                        //view.RotateAboutCenter(0, 0);
                        view.GraphicsRedraw(new int[] { });
                        times[7] = stopWatch.ElapsedMilliseconds;
                        Debug.WriteLine("solid: {0} {1} {2} {3} {4} {5} {6} {7} total: {8}", times[0], times[1] - times[0], times[2] - times[1],
                times[3] - times[2], times[4] - times[3], times[5] - times[4], times[6] - times[5], times[7] - times[6], times[7]);

                    }
                    //no active view
                    catch (Exception ex)
                    {
                        solidDoc = false;
                        Debug.WriteLine("Unable to rotate Solid Camera!\n" + ex.ToString());
                    }
                }
            }
            //no _swApp
            catch (Exception ex)
            {
                Debug.WriteLine("Oh no! Something went wrong with Solid!\n" + ex.ToString());
            }
            stopWatch.Stop();

        }

        //method for getting the corrected current quat (and mpu state) from server
        public void GetCorrectedQuat()
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(GET_QUAT_MESSAGE);
            clientStream.Write(data, 0, data.Length);
            int readBytes = 0;
            while (readBytes < 17)
            {
                //TODO: wait to complete the data
                data = new Byte[17];
                // Read batch of the TcpServer response bytes.
                Int32 bytes = clientStream.Read(data, readBytes, data.Length-readBytes);
                readBytes += bytes;
            }
            mpuStable = BitConverter.ToBoolean(data, 0);
            float X = BitConverter.ToSingle(data, 1);
            float Y = BitConverter.ToSingle(data, 5);
            float Z = BitConverter.ToSingle(data, 9);
            float W = BitConverter.ToSingle(data, 13);
            displayQuat = new Quaternion(X, Y, Z, W);
            displayQuat.Invert();
        }


        //TODO: make a good filter.
        //function that checks whether an actual movement of the cube was made
        private bool MovementFilter()
        {
            double diffTheta = lastDisplayQuat.Angle - displayQuat.Angle;
            Vector3D diffVector = Vector3D.Subtract(lastDisplayQuat.Axis, displayQuat.Axis);
            if (!(diffTheta > MAX_THETA_DIFF_UNLOCK || diffVector.Length > MAX_AXIS_DIFF_UNLOCK))
            {
                ////TODO: fix cube so can remove?
                //avoid jumping due to drifting
                lastDisplayQuat = displayQuat;
                return false;
            }            
            return true;
        }


        public double[,] QuatToRotation(Quaternion a)
        {
            double[,] rotation = new double[3, 3];
            rotation[0, 0] = 1 - (2 * a.Y * a.Y + 2 * a.Z * a.Z);
            rotation[0, 1] = 2 * a.X * a.Y + 2 * a.Z * a.W;
            rotation[0, 2] = 2 * a.X * a.Z - 2 * a.Y * a.W;

            rotation[1, 0] = 2 * a.X * a.Y - 2 * a.Z * a.W;
            rotation[1, 1] = 1 - (2 * a.X * a.X + 2 * a.Z * a.Z);
            rotation[1, 2] = 2 * a.Y * a.Z + 2 * a.X * a.W;

            rotation[2, 0] = 2 * a.X * a.Z + 2 * a.Y * a.W;
            rotation[2, 1] = 2 * a.Y * a.Z - 2 * a.X * a.W;
            rotation[2, 2] = 1 - (2 * a.X * a.X + 2 * a.Y * a.Y);

            return rotation;
        }

    }
}
