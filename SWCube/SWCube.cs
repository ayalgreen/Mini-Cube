//#define OLD
//print on each frame tick
//#define TICKMON
//inter frame timing
#define FRAMEMON
//#define DEBUGG
//movement filter debugger
//#define MOVEMON

using System;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Collections;
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
        double MAX_AXIS_DIFF_UNLOCK = 0.001;//0.0001;
        int sFPS = 1000;
        String CONNECT_MESSAGE = "Solid";
        String GET_QUAT_MESSAGE = "getQuat000";
        String GET_ZOOM_MESSAGE = "getZoom000";
        String GET_PAN_MESSAGE = "getPan0000";
        String DISCONNECT_MESSAGE = "Disconnect0";
        String CONNECTED_REPLY = "Connected";

        //Delegates
        public delegate void SimpleDelegate();

        //solid vars
        public SldWorks _swApp;
        MathUtility swMathUtility;
        MathTransform orientation;
        MathVector translation;
        public Mouse theMouse;

        private int mSWCookie;
        int solidFrameInterval;
        System.Threading.Timer solidFrameTimerT;
        int serverConnectInterval;
        System.Threading.Timer serverConnectTimer;
        System.Windows.Forms.Timer solidFrameTimer;
        bool solidDoc = false;
        static Mutex solidFrameMutex = new Mutex();
        static Mutex ServerConnectMutex = new Mutex();
        bool mouseSelected = false;
        bool rotationCenterChanged = false;

        //static vars
        Quaternion displayQuat;
        Quaternion lastDisplayQuat;
        int zoom = 1;
        double[] panSpeed = {0, 0, 0};
        bool panning = false;
        double[] rotationCenter = {0, 0, 0};
        double[,] rotCenterTransCorrection;
        Queue quatQueue;
        int queueBufferSize = 3; //TODO: get from server?
        int queueSize = 0;
        bool mpuStable = false;
        Control controlThread;

        //comm vars
        TcpClient client;
        NetworkStream clientStream;
        bool clientConnected = false;

        //debug vars
        Stopwatch stopWatch;
        double[] times;
        double[] thetaDiffs = new double[10];
        double[] vectorDiffs = new double[10];
        int quatReadingNum = 0;

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
            orientation = swMathUtility.CreateTransform(new double[1]);
            translation = swMathUtility.CreateVector(new double[1]);
            controlThread = new Control();
            controlThread.CreateControl();
            /*_swApp.SendMsgToUser2("Press OK when server is up!",
                (int)swMessageBoxIcon_e.swMbInformation,
                (int)swMessageBoxBtn_e.swMbOk);*/
            ConnectClient();
            solidFrameTimerT = new System.Threading.Timer(SolidFrameT, null, Timeout.Infinite, Timeout.Infinite);
            serverConnectTimer = new System.Threading.Timer(ServerConnectT, null, Timeout.Infinite, Timeout.Infinite);
            rotCenterTransCorrection = TransToTransformation(0, 0, 0);
            StartFrameTimer();
            return true;
        }

        public bool DisconnectFromSW()
        {
            //TODO: make this stop-mutex protected and stop timer!
            //TODO: dispose of control
            //solidFrameTimerT.Stop();
            return DisconnectServer();

        }

        public bool DisconnectServer()
        {
            try
            {
                clientStream.Close();
                client.Close();
                clientConnected = false;
            }
            catch
            {
                Debug.WriteLine("Error closing server!");
            }
            return true;
        }

        private void StartFrameTimer()
        {
            //TODO add stopwatch to make sure not rnning at too high paste
            solidFrameInterval = 1;//(int)(1000 / sFPS);
            quatQueue = new Queue();
#if (OLD)
            solidFrameTimer = new System.Windows.Forms.Timer();
            solidFrameTimer.Tick += new EventHandler(SolidFrameOld);
            solidFrameTimer.Interval = solidFrameInterval;
            solidFrameTimer.Start();
#else
            solidFrameTimerT.Change(solidFrameInterval, solidFrameInterval);// = new System.Threading.Timer(SolidFrameT, null, );
#endif            
            Debug.WriteLine("Frame timer started");
        }

        private void StopFrameTimer()
        {
#if (OLD)
            solidFrameTimer.Stop();
#else
            solidFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite);
#endif            
            Debug.WriteLine("Frame timer stopped");
        }

        private void StartServerTimer()
        {
            serverConnectInterval = 500;//(int)(1000 / sFPS);
            serverConnectTimer.Change(serverConnectInterval, serverConnectInterval);// = new System.Threading.Timer(SolidFrameT, null, );
            Debug.WriteLine("Server timer started");
        }

        private void StopServerTimer()
        {
            serverConnectTimer.Change(Timeout.Infinite, Timeout.Infinite);     
            Debug.WriteLine("Server timer stopped");
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
                //TODO: shouldn't be while(true)
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

        //worker thread method for connecting to server
        private void ServerConnectT(object myObject)//Vector3D a, Double theta)
        {
            if (ServerConnectMutex.WaitOne(0))
            {
#if (TICKMON)
                Debug.WriteLine("Server connect Timer Tick");
#endif
                if (clientConnected)
                {
                    StopServerTimer();
                    StartFrameTimer();
                    ServerConnectMutex.ReleaseMutex();
                    return;
                }

                ConnectClient();
                //controlThread.Invoke(new EventHandler(ConnectClient));

                ServerConnectMutex.ReleaseMutex();
            }
#if (DEBUGG)
            else
            {
                Debug.WriteLine("ServerConnectMutex Drop!");
            }
#endif
        }



        //worker thread method for setting up things to update the frame
        private void SolidFrameT(object myObject)//Vector3D a, Double theta)
        {
            if (solidFrameMutex.WaitOne(0))
            {
#if (TICKMON)
                Debug.WriteLine("Solid Timer Tick");
#endif
                if (!clientConnected)
                {
                    StopFrameTimer();
                    StartServerTimer();
                    solidFrameMutex.ReleaseMutex();
                    return;
                }
#if (FRAMEMON)
                stopWatch = new Stopwatch();
                times = new double[8];
                stopWatch.Start();
#endif
                GetCorrectedQuat();
                GetZoom();
                GetPanSpeed();
#if (FRAMEMON)
                times[0] = stopWatch.ElapsedMilliseconds;
#endif
                //TODO: release if no movement. but if no mouse, try to get a mouse!
                if ((mouseSelected) && (!mpuStable || !MovementFilter()) )
                {
                    solidFrameMutex.ReleaseMutex();
                    return;
                }
                lastDisplayQuat = displayQuat;
                //added for queue:
                quatQueue.Clear();
                queueSize = 0;

#if (FRAMEMON)
                times[1] = stopWatch.ElapsedMilliseconds;
#endif
                controlThread.Invoke(new EventHandler(SolidFrame));
#if (FRAMEMON)
                times[7] = stopWatch.ElapsedMilliseconds;
                Debug.WriteLine("solid: {0} {1} {2} {3} {4} {5} {6} {7} total: {8}", times[0], times[1] - times[0], times[2] - times[1],
                times[3] - times[2], times[4] - times[3], times[5] - times[4], times[6] - times[5], times[7] - times[6], times[7]);

                stopWatch.Stop();
#endif
                solidFrameMutex.ReleaseMutex();            
            }
#if (DEBUGG)
            else
            {
                Debug.WriteLine("SolidFrameMutex Drop!");
            }
#endif
        }

        //method for updating the solid cam view
        private void SolidFrame(object myObject, EventArgs myEventArgs)
        {
#if (FRAMEMON)
            times[2] = stopWatch.ElapsedMilliseconds;
#endif
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
                        if (mouseSelected == false)
                        {
                            theMouse = view.GetMouse();
                            theMouse.MouseSelectNotify += MouseSelect;
                            theMouse.MouseLBtnUpNotify += ClearSelection;
                            mouseSelected = true;
                            /*swDocumentTypes_e type = (swDocumentTypes_e)doc.GetType();
                            switch (type)
                            {
                                case swDocumentTypes_e.swDocASSEMBLY:
                                    ((AssemblyDoc)doc).ClearSelectionsNotify += ClearSelection;
                                    break;
                                case swDocumentTypes_e.swDocDRAWING:
                                    ((DrawingDoc)doc).ClearSelectionsNotify += ClearSelection;
                                    break;
                                case swDocumentTypes_e.swDocPART:
                                    ((PartDoc)doc).ClearSelectionsNotify += ClearSelection;
                                    break;
                            } */                           
                        }
                        //TODO: translate :(
                        //15-23 ms no need to translate just yet!
                        //MathTransform translate = view.Translation3;
                        //TODO: rescale :(
                        //no need to rescale yet either
                        //double scale = view.Scale2;
                        //double[,] rotation = QuatToRotation(displayQuat);
                        
                        /*Debug.WriteLine("{0} {1} {2} {3}", data[0], data[3], data[6], data[9]);
                        Debug.WriteLine("{0} {1} {2} {3}", data[1], data[4], data[7], data[10]);
                        Debug.WriteLine("{0} {1} {2} {3}", data[2], data[5], data[8], data[11]);
                        Debug.WriteLine("{0} {1} {2} {3}", data[13], data[14], data[15], data[12]);*/

                        double[,] transformation = QuatToTransformation(displayQuat);
                        double[,] translate = TransToTransformation(-rotationCenter[0], -rotationCenter[1], -rotationCenter[2]);
                        transformation = MatMult(transformation, translate);
                        translate = TransToTransformation(rotationCenter[0], rotationCenter[1], rotationCenter[2]);
                        transformation = MatMult(translate, transformation);

                        if (rotationCenterChanged)
                        {
                            double[] data = view.Orientation3.ArrayData;
                            rotCenterTransCorrection = TransToTransformation(data[9]-transformation[0, 3],
                                                        data[10] - transformation[1, 3], data[11] - transformation[2, 3]);
                            rotationCenterChanged = false;
                        }
                        //TODO: a break here causes debugger meltdown (particularly, if trying to 'reconnect' after resuming. WHY?
                        transformation = MatMult(rotCenterTransCorrection, transformation);

                        double[] tempArr = TransformationToArray(transformation);

#if (FRAMEMON)
                        times[3] = stopWatch.ElapsedMilliseconds;
#endif                        
                        orientation.ArrayData = tempArr;
                        //TODO calculating the translation makes it MUCH slower.
                        view.Orientation3 = orientation;

                        double[] tempArr2 = {panSpeed[0], panSpeed[1], panSpeed[2]};
                        double[] currArray = view.Translation3.ArrayData;
                        tempArr2[0] += currArray[0];
                        tempArr2[1] += currArray[1];
                        tempArr2[2] += currArray[2];

                        translation.ArrayData = tempArr2;
                        view.Translation3 = translation;
                                             
                        //old code
                        //view.RotateAboutCenter(0, 0);
#if (FRAMEMON)
                        times[4] = stopWatch.ElapsedMilliseconds;
#endif
                        view.GraphicsRedraw(new int[] { });
#if (FRAMEMON)
                        times[5] = stopWatch.ElapsedMilliseconds;
#endif
                    }
                    //no active view
                    catch (Exception ex)
                    {
                        solidDoc = false;
                        mouseSelected = false;
                        Debug.WriteLine("Unable to rotate Solid Camera!\n" + ex.ToString());
                    }
                }
            }
            //no _swApp
            catch (Exception ex)
            {
                Debug.WriteLine("Oh no! Something went wrong with Solid!\n" + ex.ToString());
            }
#if (FRAMEMON)
            times[6] = stopWatch.ElapsedMilliseconds;
#endif
        }

        //method for getting the corrected current quat (and mpu state) from server
        public void GetCorrectedQuat()
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(GET_QUAT_MESSAGE);
            try
            {
                clientStream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error writing to server!");
                DisconnectServer();
                return;
            }
            int readBytes = 0;
            while (readBytes < 17)
            {
                //TODO: wait to complete the data
                data = new Byte[17];
                // Read batch of the TcpServer response bytes.
                Int32 bytes;
                try
                {
                    bytes = clientStream.Read(data, readBytes, data.Length - readBytes);
                    readBytes += bytes;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error reading from server!");
                    DisconnectServer();
                    return;
                }                
            }
            mpuStable = BitConverter.ToBoolean(data, 0);
            float X = BitConverter.ToSingle(data, 1);
            float Y = BitConverter.ToSingle(data, 5);
            float Z = BitConverter.ToSingle(data, 9);
            float W = BitConverter.ToSingle(data, 13);
            displayQuat = new Quaternion(X, Y, Z, W);
            displayQuat.Invert();
        }

        //method for getting the current zoom from server
        public void GetZoom()
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(GET_ZOOM_MESSAGE);
            try
            {
                clientStream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error writing to server!");
                DisconnectServer();
                return;
            }
            int readBytes = 0;
            while (readBytes < 4)
            {
                //TODO: wait to complete the data
                data = new Byte[4];
                // Read batch of the TcpServer response bytes.
                Int32 bytes;
                try
                {
                    bytes = clientStream.Read(data, readBytes, data.Length - readBytes);
                    readBytes += bytes;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error reading from server!");
                    DisconnectServer();
                    return;
                }
            }
            zoom = BitConverter.ToInt32(data, 0);
        }

        //method for getting the current panning speed
        public void GetPanSpeed()
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(GET_PAN_MESSAGE);
            try
            {
                clientStream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error writing to server!");
                DisconnectServer();
                return;
            }
            int readBytes = 0;
            while (readBytes < 12)
            {
                //TODO: wait to complete the data
                data = new Byte[12];
                // Read batch of the TcpServer response bytes.
                Int32 bytes;
                try
                {
                    bytes = clientStream.Read(data, readBytes, data.Length - readBytes);
                    readBytes += bytes;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error reading from server!");
                    DisconnectServer();
                    return;
                }
            }
            float X = BitConverter.ToSingle(data, 0);
            float Y = BitConverter.ToSingle(data, 4);
            float Z = BitConverter.ToSingle(data, 8);
            double panNorm = Math.Sqrt(X * X + Y * Y + Z * Z);
            //Debug.WriteLine("{0}, {1}: {2}", X, Y, panNorm);
            if (panNorm > 0)
            {
                panning = true;
                panSpeed[0] = X / (1000 * 20);
                panSpeed[1] = Y / (1000 * 20);
                panSpeed[2] = Z / (1000 * 20);
            }
            else
            {
                panning = false;
                panSpeed[0] = 0;
                panSpeed[1] = 0;
                panSpeed[2] = 0;
            }

        }


        //TODO: make a good filter.
        //function that checks whether an actual movement of the cube was made
        private bool MovementFilter()
        {
            if (panning)
            {
                return true;
            }
            if (queueSize < queueBufferSize-1)
            {
                quatQueue.Enqueue(displayQuat);
                queueSize++;
                return false;
            }

            double diffTheta = lastDisplayQuat.Angle - displayQuat.Angle;
            Vector3D diffVector = Vector3D.Subtract(lastDisplayQuat.Axis, displayQuat.Axis);
#if (MOVEMON)
            thetaDiffs[quatReadingNum] = diffTheta;
            vectorDiffs[quatReadingNum] = diffVector.Length;
            quatReadingNum++;
            if (quatReadingNum >= 10)
            {
                Debug.WriteLine("Quaternion Readings: ({0}, {1}), ({2}, {3}), ({4}, {5}), ({6}, {7}), ({8}, {9}), ({10}, {11}), ({12}, {13}), ({14}, {15}), ({16}, {17}), ({18}, {19})",
                    thetaDiffs[0], vectorDiffs[0], thetaDiffs[1], vectorDiffs[1], thetaDiffs[2], vectorDiffs[2], thetaDiffs[3], vectorDiffs[3], thetaDiffs[4], vectorDiffs[4],
                     thetaDiffs[5], vectorDiffs[5], thetaDiffs[6], vectorDiffs[6], thetaDiffs[7], vectorDiffs[7], thetaDiffs[8], vectorDiffs[8], thetaDiffs[9], vectorDiffs[9]);

                thetaDiffs = new double[10];
                vectorDiffs = new double[10];
                quatReadingNum = 0;
            }
#endif
            if (!(diffTheta > queueBufferSize * MAX_THETA_DIFF_UNLOCK || diffVector.Length > queueBufferSize * MAX_AXIS_DIFF_UNLOCK))
            {
                ////TODO: fix cube so there is no drift to begin with and so this can be removed?
                //avoid jumping due to drifting
                //lastDisplayQuat = displayQuat;
                quatQueue.Enqueue(displayQuat);
                lastDisplayQuat = (Quaternion)quatQueue.Dequeue();                
                return false;
            }
#if (MOVEMON)
            Debug.WriteLine("Tak! angle diff: {0}; Vector diff: {1}", (diffTheta > queueBufferSize * MAX_THETA_DIFF_UNLOCK), (diffVector.Length > queueBufferSize * MAX_AXIS_DIFF_UNLOCK));
#endif
            return true;
        }

        public int MouseSelect(int Ix, int Iy, double X, double Y, double Z)
        {
            rotationCenter[0] = X;
            rotationCenter[1] = Y;
            rotationCenter[2] = Z;
            rotationCenterChanged = true;
            return 1;
        }

        public int ClearSelection(int Ix, int Iy, int wParam)
        {
            rotationCenter[0] = 0;
            rotationCenter[1] = 0;
            rotationCenter[2] = 0;
            rotationCenterChanged = true;

            return 1;
        }

#region conversions
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

        public double[] TransformationToArray(double[,] transformation)
        {
            double[] tempArr = new double[16];
            //new X axis
            tempArr[0] = transformation[0, 0];
            tempArr[1] = transformation[1, 0];
            tempArr[2] = transformation[2, 0];
            //new Y axis
            tempArr[3] = transformation[0, 1];
            tempArr[4] = transformation[1, 1];
            tempArr[5] = transformation[2, 1];
            //new Z axis
            tempArr[6] = transformation[0, 2];
            tempArr[7] = transformation[1, 2];
            tempArr[8] = transformation[2, 2];
            //translation - doesn't mater for orientation!
            tempArr[9] = transformation[0, 3];
            tempArr[10] = transformation[1, 3];
            tempArr[11] = transformation[2, 3];
            //scale - doesn't mater for orientation!
            tempArr[12] = transformation[3, 3];
            //who knows
            tempArr[13] = 0;
            tempArr[14] = 0;
            tempArr[15] = 0;
            return tempArr;
        }

        public double[,] QuatToTransformation(Quaternion a)
        {
            double[,] transformation = new double[4, 4];
            transformation[0, 0] = 1 - (2 * a.Y * a.Y + 2 * a.Z * a.Z);
            transformation[0, 1] = 2 * a.X * a.Y + 2 * a.Z * a.W;
            transformation[0, 2] = 2 * a.X * a.Z - 2 * a.Y * a.W;

            transformation[0, 3] = 0;

            transformation[1, 0] = 2 * a.X * a.Y - 2 * a.Z * a.W;
            transformation[1, 1] = 1 - (2 * a.X * a.X + 2 * a.Z * a.Z);
            transformation[1, 2] = 2 * a.Y * a.Z + 2 * a.X * a.W;

            transformation[1, 3] = 0;

            transformation[2, 0] = 2 * a.X * a.Z + 2 * a.Y * a.W;
            transformation[2, 1] = 2 * a.Y * a.Z - 2 * a.X * a.W;
            transformation[2, 2] = 1 - (2 * a.X * a.X + 2 * a.Y * a.Y);

            transformation[2, 3] = 0;

            transformation[3, 0] = 0;
            transformation[3, 1] = 0;
            transformation[3, 2] = 0;
            transformation[3, 3] = 1;

            return transformation;
        }

        public double[,] TransToTransformation(double x, double y, double z)
        {
            double[,] transformation = new double[4, 4];
            transformation[0, 0] = 1;
            transformation[0, 1] = 0;
            transformation[0, 2] = 0;

            transformation[0, 3] = x;

            transformation[1, 0] = 0;
            transformation[1, 1] = 1;
            transformation[1, 2] = 0;

            transformation[1, 3] = y;

            transformation[2, 0] = 0;
            transformation[2, 1] = 0;
            transformation[2, 2] = 1;

            transformation[2, 3] = z;

            transformation[3, 0] = 0;
            transformation[3, 1] = 0;
            transformation[3, 2] = 0;
            transformation[3, 3] = 1;

            return transformation;
        }

        public double[,] MatMult(double[,] mat1, double[,] mat2, int dim=4)
        {
            double[,] newMat = new double[dim, dim];
            for (int i=0; i<dim; i++)
            {
                for (int j=0; j<dim; j++)
                {
                    double sum = 0;
                    for (int k=0; k<dim; k++)
                    {
                        sum += mat1[i, k] * mat2[k, j];
                    }
                    newMat[i, j] = sum;
                }
            }
            return newMat;
        }
#endregion
        
#region old...
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

            if (!mpuStable || !MovementFilterOld())
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

        //TODO: make a good filter.
        //function that checks whether an actual movement of the cube was made
        private bool MovementFilterOld()
        {
            double diffTheta = lastDisplayQuat.Angle - displayQuat.Angle;
            Vector3D diffVector = Vector3D.Subtract(lastDisplayQuat.Axis, displayQuat.Axis);
            if (!(diffTheta > MAX_THETA_DIFF_UNLOCK || diffVector.Length > MAX_AXIS_DIFF_UNLOCK))
            {
                ////TODO: fix cube so there is no drift to begin with and so this can be removed?
                //avoid jumping due to drifting
                lastDisplayQuat = displayQuat;
                return false;
            }
            return true;
        }
#endregion

    }
}

