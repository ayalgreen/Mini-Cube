//#define OLD

using System;
using System.Runtime.InteropServices;
using Inventor;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
//using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace InvCubeAddin
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("b4b574a8-a3e7-4f54-a4d7-3ed8791f3f64")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
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

        //Inventor vars
        private Inventor.Application _invApp;
        private TransientGeometry tg;
        private Inventor.ButtonDefinition buttonDef;

        //solid vars

        int invFrameInterval;
        System.Threading.Timer invFrameTimerT;
        System.Windows.Forms.Timer invFrameTimer;
        bool invView = false;
        static Mutex invFrameMutex = new Mutex();

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


        public StandardAddInServer()
        {
        }

        #region ApplicationAddInServer Members

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            // This method is called by Inventor when it loads the addin.
            // The AddInSiteObject provides access to the Inventor Application object.
            // The FirstTime flag indicates if the addin is loaded for the first time.

            // Initialize AddIn members.
            controlThread = new Control();
            controlThread.CreateControl();
            Debug.WriteLine("INVCube Started!");
            _invApp = addInSiteObject.Application;
            tg = _invApp.TransientGeometry;
            ConnectClient();
            StartTimer();
        }

        public void Deactivate()
        {
            // This method is called by Inventor when the AddIn is unloaded.
            // The AddIn will be unloaded either manually by the user or
            // when the Inventor session is terminated

            //TODO: make this stop-mutex protected and stop timer!
            //TODO: dispose of control
            // Release objects.
            _invApp = null;
            //make sure we didn't close already
            clientStream.Close();
            client.Close();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExecuteCommand(int commandID)
        {
            // Note:this method is now obsolete, you should use the 
            // ControlDefinition functionality for implementing commands.
        }

        public object Automation
        {
            // This property is provided to allow the AddIn to expose an API 
            // of its own to other programs. Typically, this  would be done by
            // implementing the AddIn's API interface in a class and returning 
            // that class object through this property.

            get
            {
                // TODO: Add ApplicationAddInServer.Automation getter implementation
                return null;
            }
        }

        #endregion

        private void StartTimer()
        {
            //TODO add stopwatch to make sure not rnning at too high paste
            invFrameInterval = 1;//(int)(1000 / sFPS);
#if (OLD)
            solidFrameTimer = new System.Windows.Forms.Timer();
            solidFrameTimer.Tick += new EventHandler(SolidFrameOld);
            solidFrameTimer.Interval = solidFrameInterval;
            solidFrameTimer.Start();
#else
            invFrameTimerT = new System.Threading.Timer(InvFrameT, null, invFrameInterval, invFrameInterval);
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
        private void InvFrameT(object myObject)
        {
            if (invFrameMutex.WaitOne(0))
            {
                Debug.WriteLine("Timer Tick");
                if (!clientConnected)
                {
                    invFrameMutex.ReleaseMutex();
                    return;
                }
                stopWatch = new Stopwatch();
                times = new double[8];
                stopWatch.Start();
                GetCorrectedQuat();
                times[0] = stopWatch.ElapsedMilliseconds;
                if (!mpuStable || !MovementFilter())
                {
                    invFrameMutex.ReleaseMutex();
                    return;
                }
                lastDisplayQuat = displayQuat;

                times[1] = stopWatch.ElapsedMilliseconds;
                controlThread.Invoke(new EventHandler(InvFrame));
                times[7] = stopWatch.ElapsedMilliseconds;
                Debug.WriteLine("solid: {0} {1} {2} {3} {4} {5} {6} {7} total: {8}", times[0], times[1] - times[0], times[2] - times[1],
                times[3] - times[2], times[4] - times[3], times[5] - times[4], times[6] - times[5], times[7] - times[6], times[7]);

                stopWatch.Stop();
                invFrameMutex.ReleaseMutex();
            }
            else
            {
                Debug.WriteLine("InvFrameMutex Drop!");
            }
        }

        //displaying an inventor frame (non debugger).
        public void InvFrame(object myObject, EventArgs myEventArgs)
        {
            Vector3D a = displayQuat.Axis;
            double theta = displayQuat.Angle;
            theta *= Math.PI / 180;
            //move object instead of the camera
            theta = -theta;

            double[] camUp = RotateQuaternion(0, 1, 0, a, theta);
            try
            {
                //TODO: this doesn't necesarily throw exception when inventor is off
                if (!invView)
                {
                    if (_invApp.ActiveView != null)
                    {
                        invView = true;
                    }
                }
                //avoiding exceptions if possible                        
                if (invView)
                {
                    try
                    {
                        times[0] = stopWatch.ElapsedMilliseconds;
                        Inventor.Camera cam = _invApp.ActiveView.Camera;
                        times[1] = stopWatch.ElapsedMilliseconds;
                        double[] eyeArr = new double[3] { 0, 0, 0 };
                        cam.Eye.GetPointData(ref eyeArr);
                        times[2] = stopWatch.ElapsedMilliseconds;
                        double[] targetArr = new double[3] { 0, 0, 0 };
                        cam.Target.GetPointData(ref targetArr);
                        times[3] = stopWatch.ElapsedMilliseconds;
                        double[] camVector = new double[3]
                                {eyeArr[0]-targetArr[0], eyeArr[1] - targetArr[1], eyeArr[2] - targetArr[2] };//
                        double camDist = Math.Sqrt(camVector[0] * camVector[0] + camVector[1] * camVector[1] + camVector[2] * camVector[2]);
                        /*i think the algorithm should be:
                         * get rotation for Z axis to targetpoint (call it A)
                         * rotate Z axis (probably a point on it with same dist as target vector size)
                         * rotate said point by A (potentially conjugated by the quat. not sure about this?)
                         * rotate cam vector by a similar procedure(?)
                         */
                        double[] camPos = RotateQuaternion(0, 0, camDist, a, theta);
                        cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);
                        times[4] = stopWatch.ElapsedMilliseconds;
                        cam.Target = tg.CreatePoint();
                        times[5] = stopWatch.ElapsedMilliseconds;
                        cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                        times[6] = stopWatch.ElapsedMilliseconds;
                        cam.ApplyWithoutTransition();
                        times[7] = stopWatch.ElapsedMilliseconds;
                    }
                    #region Obsolete Code
                    /*centers, doesn't change scale!
                    { 
                        Inventor.Camera cam = _invApp.ActiveView.Camera;
                        TransientGeometry tg = _invApp.TransientGeometry;
                        double[] eyeArr = new double[3] { 0, 0, 0 }; //
                        cam.Eye.GetPointData(ref eyeArr); //
                        double[] targetArr = new double[3] { 0, 0, 0 }; //
                        cam.Target.GetPointData(ref targetArr); //
                        double[] camVector = new double[3] 
                                {eyeArr[0]-targetArr[0], eyeArr[1] - targetArr[1], eyeArr[2] - targetArr[2] };//
                        camDist = Math.Sqrt(camVector[0]* camVector[0] + camVector[1] * camVector[1] + camVector[2] * camVector[2]);
                        camPos = RotateQuaternion(0, 0, camDist, a, theta);
                        cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);                                
                        cam.Target = tg.CreatePoint();
                        cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                        cam.ApplyWithoutTransition();
                    }*/
                    /* centers and rescales!
                    {
                        times[1] = stopWatch.ElapsedMilliseconds;
                        //Stopwatch stopWatch = new Stopwatch();
                        //stopWatch.Start();                            
                        Inventor.Camera cam = _invApp.ActiveView.Camera;
                        times[2] = stopWatch.ElapsedMilliseconds;
                        TransientGeometry tg = _invApp.TransientGeometry;
                        times[3] = stopWatch.ElapsedMilliseconds;
                        cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);
                        times[4] = stopWatch.ElapsedMilliseconds;
                        cam.Target = tg.CreatePoint();
                        times[5] = stopWatch.ElapsedMilliseconds;
                        cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                        times[6] = stopWatch.ElapsedMilliseconds;
                        cam.ApplyWithoutTransition();
                        times[7] = stopWatch.ElapsedMilliseconds;
                    }*/
                    #endregion
                    //no active view
                    catch (Exception ex)
                    {
                        invView = false;
                        Debug.WriteLine("Unable to rotate Inventor Camera!\n" + ex.ToString());
                    }
                }
            }
            //no _invApp
            catch (Exception ex)
            {
                Debug.WriteLine("Oh no! Something went wrong with Inventor!\n" + ex.ToString());
            }
        }

        //worker thread method for setting up things to update the frame
        private void InventorFrameOld(object myObject)
        {
            Debug.WriteLine("Timer Tick");
            if (!clientConnected)
            {
                return;
            }
            stopWatch = new Stopwatch();
            times = new double[8];
            stopWatch.Start();
            GetCorrectedQuat();
            times[0] = stopWatch.ElapsedMilliseconds;
            if (!mpuStable || !MovementFilter())
            {
                return;
            }
            lastDisplayQuat = displayQuat;

            times[1] = stopWatch.ElapsedMilliseconds;

            Vector3D a = displayQuat.Axis;
            double theta = displayQuat.Angle;
            theta *= Math.PI / 180;
            //move object instead of the camera
            theta = -theta;

            double[] camUp = RotateQuaternion(0, 1, 0, a, theta);
            try
            {
                //TODO: this doesn't necesarily throw exception when inventor is off
                //avoid exceptions if possible
                if (_invApp.ActiveView != null)
                {
                    try
                    {
                        times[0] = stopWatch.ElapsedMilliseconds;
                        Inventor.Camera cam = _invApp.ActiveView.Camera;
                        times[1] = stopWatch.ElapsedMilliseconds;
                        double[] eyeArr = new double[3] { 0, 0, 0 };
                        cam.Eye.GetPointData(ref eyeArr);
                        times[2] = stopWatch.ElapsedMilliseconds;
                        double[] targetArr = new double[3] { 0, 0, 0 };
                        cam.Target.GetPointData(ref targetArr);
                        times[3] = stopWatch.ElapsedMilliseconds;
                        double[] camVector = new double[3]
                                {eyeArr[0]-targetArr[0], eyeArr[1] - targetArr[1], eyeArr[2] - targetArr[2] };//
                        double camDist = Math.Sqrt(camVector[0] * camVector[0] + camVector[1] * camVector[1] + camVector[2] * camVector[2]);
                        /*i think the algorithm should be:
                         * get rotation for Z axis to targetpoint (call it A)
                         * rotate Z axis (probably a point on it with same dist as target vector size)
                         * rotate said point by A (potentially conjugated by the quat. not sure about this?)
                         * rotate cam vector by a similar procedure(?)
                         */
                        double[] camPos = RotateQuaternion(0, 0, camDist, a, theta);
                        cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);
                        times[4] = stopWatch.ElapsedMilliseconds;
                        cam.Target = tg.CreatePoint();
                        times[5] = stopWatch.ElapsedMilliseconds;
                        cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                        times[6] = stopWatch.ElapsedMilliseconds;
                        cam.ApplyWithoutTransition();
                        times[7] = stopWatch.ElapsedMilliseconds;
                    }
                    #region Obsolete Code
                    /*centers, doesn't change scale!
                    { 
                        Inventor.Camera cam = _invApp.ActiveView.Camera;
                        TransientGeometry tg = _invApp.TransientGeometry;
                        double[] eyeArr = new double[3] { 0, 0, 0 }; //
                        cam.Eye.GetPointData(ref eyeArr); //
                        double[] targetArr = new double[3] { 0, 0, 0 }; //
                        cam.Target.GetPointData(ref targetArr); //
                        double[] camVector = new double[3] 
                                {eyeArr[0]-targetArr[0], eyeArr[1] - targetArr[1], eyeArr[2] - targetArr[2] };//
                        camDist = Math.Sqrt(camVector[0]* camVector[0] + camVector[1] * camVector[1] + camVector[2] * camVector[2]);
                        camPos = RotateQuaternion(0, 0, camDist, a, theta);
                        cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);                                
                        cam.Target = tg.CreatePoint();
                        cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                        cam.ApplyWithoutTransition();
                    }*/
                    /* centers and rescales!
                    {
                        times[1] = stopWatch.ElapsedMilliseconds;
                        //Stopwatch stopWatch = new Stopwatch();
                        //stopWatch.Start();                            
                        Inventor.Camera cam = _invApp.ActiveView.Camera;
                        times[2] = stopWatch.ElapsedMilliseconds;
                        TransientGeometry tg = _invApp.TransientGeometry;
                        times[3] = stopWatch.ElapsedMilliseconds;
                        cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);
                        times[4] = stopWatch.ElapsedMilliseconds;
                        cam.Target = tg.CreatePoint();
                        times[5] = stopWatch.ElapsedMilliseconds;
                        cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                        times[6] = stopWatch.ElapsedMilliseconds;
                        cam.ApplyWithoutTransition();
                        times[7] = stopWatch.ElapsedMilliseconds;
                    }*/
                    #endregion
                    //no active view
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Unable to rotate Inventor Camera!\n" + ex.ToString());
                    }
                }
            }
            //no _invApp
            catch (Exception ex)
            {
                Debug.WriteLine("Oh no! Something went wrong with Inventor!\n" + ex.ToString());
            }

            times[7] = stopWatch.ElapsedMilliseconds;
            Debug.WriteLine("solid: {0} {1} {2} {3} {4} {5} {6} {7} total: {8}", times[0], times[1] - times[0], times[2] - times[1],
            times[3] - times[2], times[4] - times[3], times[5] - times[4], times[6] - times[5], times[7] - times[6], times[7]);
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
                Int32 bytes = clientStream.Read(data, readBytes, data.Length - readBytes);
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

        public double[] RotateQuaternion(double x, double y, double z, Vector3D a, double theta)
        {
            double[] vect = new double[3];
            double c = Math.Cos(theta);
            double s = Math.Sin(theta);
            vect[0] = x * (c + a.X * a.X * (1 - c)) + y * (a.X * a.Y * (1 - c) - a.Z * s) + z * (a.X * a.Z * (1 - c) + a.Y * s);
            vect[1] = x * (a.Y * a.X * (1 - c) + a.Z * s) + y * (c + a.Y * a.Y * (1 - c)) + z * (a.Y * a.Z * (1 - c) - a.X * s);
            vect[2] = x * (a.Z * a.X * (1 - c) - a.Y * s) + y * (a.Z * a.Y * (1 - c) + a.X * s) + z * (c + a.Z * a.Z * (1 - c));

            return vect;
        }


    }
}
