﻿//(C) Copyright 2012 by Autodesk, Inc. 

//Permission to use, copy, modify, and distribute this software
//in object code form for any purpose and without fee is hereby
//granted, provided that the above copyright notice appears in
//all copies and that both that copyright notice and the limited
//warranty and restricted rights notice below appear in all
//supporting documentation.

//AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
//AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
//MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK,
//INC. DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL
//BE UNINTERRUPTED OR ERROR FREE.

//Use, duplication, or disclosure by the U.S. Government is
//subject to restrictions set forth in FAR 52.227-19 (Commercial
//Computer Software - Restricted Rights) and DFAR 252.227-7013(c)
//(1)(ii)(Rights in Technical Data and Computer Software), as
//applicable.


//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;

#define INV
//#define SOLID
using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;
using Inventor;
using SolidWorks.Interop.sldworks;
//using SolidWorks.Interop.swconst;
using System.Threading;
//using InTheHand;
//using InTheHand.Net.Ports;
using InTheHand.Net.Sockets;

using InTheHand.Net.Bluetooth;
using System.Diagnostics;
using System.Collections.Generic;

namespace MiniCube
{
    public partial class CubeForm : Form
    {
        //Constants
        //TODO: add try to any port operation?
        int BAUD_RATE = 38400;
        string serialComPort = "COM9";
        int FPS = 60;
        double MAX_THETA_DIFF_LOCK = 0.05;
        double MAX_AXIS_DIFF_LOCK = 0.0005;
        double MAX_THETA_DIFF_UNLOCK = 0.01;
        double MAX_AXIS_DIFF_UNLOCK = 0.0001;
        string CUBE_BT_MODULE = "Cube";
        string BT_PIN = "1234";

        //delegates
        public delegate void SimpleDelegate();
        public delegate void TimerDelegate(object myObject);
        public delegate void DataProcessDelegate(byte[] buffer);
        public delegate void CameraLockDelegate(Vector3D a, double theta);

        //inventor vars
        Inventor.Application _invApp;
        bool _inventorStartedByForm = false;
        bool inventorRunning = false;
        int inventorFrameInterval;
        System.Threading.Timer inventorFrameTimerT;
        bool inventorFrameTimerTEnabled = false;
        static Mutex inventorFrameMutex = new Mutex();

        //solid vars
        SldWorks _swApp;
        MathTransform view;
        bool _solidStartedByForm = false;
        bool solidRunning = false;
        int solidFrameInterval;
        System.Threading.Timer solidFrameTimerT;
        bool solidFrameTimerTEnabled = false;
        static Mutex solidFrameMutex = new Mutex();


        //TODO cam dist adjust
        double camDist = 10;

        //comm vars
        BluetoothDeviceInfo bluetoothDevice;
        Guid mUUID = new Guid("00001101-0000-1000-8000-00805F9B34FB");      
        SerialPort serialPort1 = new SerialPort();
   
        //comm protocol vars
        char[] teapotPacket = new char[14];  // InvenSense Teapot packet
        int serialCount = 0;                 // current packet byte position
        bool synced = false;
        bool noSync = true;
        int pingTimerInterval;
        System.Threading.Timer pingTimerT;
        char[] pingBuff = { 'r' };
        char[] calBuff = { 'c', 'f', 'c', 'f', 'c', 'f', 'c', 'f' };
        char lastPacketID = (char)0;
        static Mutex serialPortDataMutex = new Mutex();
        Object synchronizerLock = new Object();
        Object packetAnalyzerLock = new Object();
        ReaderWriterLockSlim closeLock = new ReaderWriterLockSlim();
        //static vars
        Quaternion quat = new Quaternion(1, 0, 0, 0);
        Quaternion oldQuat = new Quaternion(1, 0, 0, 0);
        Quaternion lastLockedQuat = new Quaternion(1, 0, 0, 0);
        Quaternion invertedQuat = new Quaternion(-1, 0, 0, 0); //Vect= (-1, 0, 0),  Angle= 180
        double[] q = new Double[4];
        double[] gravity = new Double[3];
        double[] euler = new Double[3];
        double[] ypr = new Double[3];
        bool formClose = false;
        bool mpuStable = true;
        bool foundCube = false;
        string path = @"./Cubecnfg";

        //temp vars
        int dbgcounter = 0;
        bool temp1 = false;
        bool temp2 = false;
        double scale = 0;
        MathUtility swMathUtility;
        MathVector X;
        MathVector Y;
        MathVector Z;
        MathVector transVect;
        double[] x = new double[] { 0, 0, 0 };
        double[] y = new double[] { 0, 0, 0 };
        double[] z = new double[] { 0, 0, 0 };
        double[] transArr = new double[] { 0, 0, 0 };
        double[] orientationMat = new double[16];
        int rotationSelect = 0;
        

        public CubeForm()
        {
            InitializeComponent();
            Console.WriteLine("Initializing...");
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(CloseHandler);
#if (INV)
            Console.WriteLine("Inventor...");
            StartInventor();
#endif
#if (SOLID)
            Console.WriteLine("Solid...");
            StartSolid();
#endif
            ////TODO: make software calibration work
            invertedQuat.Invert();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxPorts.Items.Add(port);
            }
            Console.WriteLine("Load config...");
            loadConfig();
            Console.WriteLine("Set timers...");
            SetTimers();
            Console.WriteLine("Open port...");
            OpenPort();
            Console.WriteLine("Done.");

            //OpenBluetooth();
        }

        private void SetTimers()
        {
#if (INV)
            inventorFrameInterval = (int)(1000 / FPS);
            inventorFrameTimerT = new System.Threading.Timer(InventorFrameT, null, Timeout.Infinite, Timeout.Infinite);
#endif
#if (SOLID)
            solidFrameInterval = (int)(1000 / FPS);
            solidFrameTimerT = new System.Threading.Timer(SolidFrameT, null, Timeout.Infinite, Timeout.Infinite);
#endif

            pingTimerInterval = 2000;

            pingTimerT = new System.Threading.Timer(PingT, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void loadConfig()
        {
            if (System.IO.File.Exists(path))
            {
                using (StreamReader sr = System.IO.File.OpenText(path))
                {
                    string s = "";
                    s = sr.ReadLine();
                    if (comboBoxPorts.Items.Contains(s))
                    {
                        comboBoxPorts.Text = s;
                        serialComPort = s;
                    }
                }
            }
        }

        private void StartInventor()
        {
            try
            {
                _invApp = (Inventor.Application)Marshal.GetActiveObject("Inventor.Application");
                inventorRunning = true;
            }
            catch (Exception ex)
            {
                try
                {
                    Type invAppType = Type.GetTypeFromProgID("Inventor.Application");

                    _invApp = (Inventor.Application)System.Activator.CreateInstance(invAppType);
                    _invApp.Visible = true;

                    //Note: if the Inventor session is left running after this
                    //form is closed, there will still an be and Inventor.exe 
                    //running. We will use this Boolean to test in Form1.Designer.cs 
                    //in the dispose method whether or not the Inventor App should
                    //be shut down when the form is closed.
                    _inventorStartedByForm = true;
                    inventorRunning = true;

                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.ToString());
                    MessageBox.Show("Unable to get or start Inventor");
                }
            }
        }

        private void StartSolid()
        {
            try
            {
                _swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                solidRunning = true;
            }
            catch (Exception ex)
            {
                try
                {
                    Type swAppType = Type.GetTypeFromProgID("SldWorks.Application");

                    _swApp = (SldWorks)System.Activator.CreateInstance(swAppType);
                    _swApp.Visible = true;

                    //Note: if the Inventor session is left running after this
                    //form is closed, there will still an be and Inventor.exe 
                    //running. We will use this Boolean to test in Form1.Designer.cs 
                    //in the dispose method whether or not the Inventor App should
                    //be shut down when the form is closed.
                    _solidStartedByForm = true;
                    solidRunning = true;
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.ToString());
                    MessageBox.Show("Unable to get or start Solid");
                    return;
                }
            }
            swMathUtility = (MathUtility)_swApp.GetMathUtility();
            X = swMathUtility.CreateVector(new double[3]);
            Y = swMathUtility.CreateVector(new double[3]);
            Z = swMathUtility.CreateVector(new double[3]);
            transVect = swMathUtility.CreateVector(new double[3]);
        }

        private void OpenPort()
        {
            serialPort1 = new SerialPort();
            serialPort1.ReadTimeout = 200;
            serialPort1.WriteTimeout = 200;
            serialPort1.PortName = serialComPort;
            serialPort1.BaudRate = BAUD_RATE;
            serialPort1.ParityReplace = (byte)0;
            serialPort1.DtrEnable = true;
            //event handler to be run on *secondary thread*
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(SerialPort1DataReceived);
            foreach (string port in SerialPort.GetPortNames())
            {
                if (port == serialPort1.PortName)
                {
                    try
                    {
                        serialPort1.Open();
                        pingTimerT.Change(pingTimerInterval, pingTimerInterval);
                    }
                    //if can't open port
                    catch (Exception ex)
                    {
                        MessageBox.Show("Oh no! bad port!\n" + ex.ToString());
                    }
                    return;
                }
            }
           
        }

        //TODO: automatic Bluetooth stuff
        /* Bluetooth
        private void OpenBluetooth()
        {
            Thread bluetoothScanner = new Thread(new ThreadStart(BluetoothScan));
            bluetoothScanner.Start();
        }

        private void BluetoothScan()
        {
            try
            {
                BluetoothClient client = new BluetoothClient();
                //Synchroniously look for devices
                BluetoothDeviceInfo[] allDevices = client.DiscoverDevicesInRange();
                List<BluetoothDeviceInfo> devices = new List<BluetoothDeviceInfo>();
                foreach (BluetoothDeviceInfo device in allDevices)
                {
                    if (device.DeviceName == CUBE_BT_MODULE)
                    {
                        devices.Add(device);
                    }
                }
                if (devices.Count > 0)
                {
                    foreach (BluetoothDeviceInfo device in devices)
                    {
                        if (PairDevice(device))
                        {
                            //should this actually be on a separate thread?
                            bluetoothDevice = device;
                            Thread bluetoothConnector = new Thread(new ThreadStart(BluetoothConnect));
                            bluetoothConnector.Start();
                            break;
                        }
                    }

                }
            }
            catch (System.PlatformNotSupportedException ex)
            {
                Console.WriteLine("Can't find bluetooth module! Please make sure Bluetooth module is connected and activated");
            }
            finally
            {

            }
        }

        private void BluetoothConnect()
        {
            BluetoothClient client = new BluetoothClient();
            client.BeginConnect(bluetoothDevice.DeviceAddress, mUUID, BluetoothClienctConnectCallback, client);
        }


        private bool PairDevice(BluetoothDeviceInfo device)
        {
            if (!device.Authenticated)
            {
                if (!BluetoothSecurity.PairRequest(device.DeviceAddress, BT_PIN))
                {
                    return false;
                }
            }
            return true;
        }

        //does this need to be called repeatedly?
        void BluetoothClienctConnectCallback(IAsyncResult result)
        {
            BluetoothClient client = (BluetoothClient)result.AsyncState;
            client.EndConnect(result);
            foundCube = true;

            Stream stream = client.GetStream();
            stream.ReadTimeout = 1000;
            while (true)
            {
                byte[] buffer = new byte[1024];
                stream.Read(buffer, 0, buffer.Length);
                //TODO: form close lock
                if (formClose)
                {
                    break;
                }           
                BeginInvoke(new DataProcessDelegate(Synchronizer), buffer);
                //return;
            }
        }
        */

        //Method for reading from serial port and passing on to InvokedOnData (as handler on form-thread)
        private void SerialPort1DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (closeLock.TryEnterReadLock(0))
            {
                try
                {
                    if (serialPortDataMutex.WaitOne(0))
                    {
                        //TODO: only iterates once.  change to if?
                        while (serialPort1.BytesToRead > 0)
                        {

                            //Console.WriteLine("buffer length {0}", serialPort1.BytesToRead);
                            //TODO only if port isn't closed!
                            byte[] buffer = new byte[serialPort1.BytesToRead];
                            serialPort1.Read(buffer, 0, buffer.Length);
                            //TODO: which is better?
                            new DataProcessDelegate(Synchronizer).BeginInvoke(buffer, null, null);
                            //old run using control.beginInvoke
                            //BeginInvoke(new DataProcessDelegate(Synchronizer), buffer);
                            serialPortDataMutex.ReleaseMutex();
                            return;
                        }

                        serialPortDataMutex.ReleaseMutex();
                    }
                    //for debugging purposes
                    /*
                    {
                        Console.WriteLine("serial port data mutex block {0}", dbgcounter);
                        dbgcounter++;
                    }     
                    */
                }
                finally
                {
                    closeLock.ExitReadLock();
                } 
            }                  
        }

        //form-thread method for parsing received data and then calling DisplayFromPort()
        private void Synchronizer(byte[] buffer)
        {
            if (closeLock.TryEnterReadLock(0))
            {
                try
                {
                    if (Monitor.TryEnter(synchronizerLock, 0))
                    {
                        try
                        {
                            foreach (byte b in buffer)
                            {
                                int ch = b;
                                if (!synced && ch != '$')
                                {
                                    //TODO: after long (50 sec) debug break in Stable() (after clicking calibrate) gets deadlocked on this
                                    Console.Write((char)ch);
                                    noSync = true;
                                    continue;  // initial synchronization - also used to resync/realign if needed
                                }
                                //noSync doesn't become true after each packet
                                if (noSync)
                                {
                                    Console.WriteLine("Synced!");
                                    /*old code:
                                    //if regained sync after calibration, should wait for stabilization, adjust heading, and change view.
                                    if (mpuCalibrating)
                                    {
                                        mpuCalibrating = false;
                                        mpuStable = false;
                                        makeCorrection = true;
                                        mpuStabilizeTimer.Start();
                                    }*/
                                }
                                synced = true;
                                noSync = false;

                                if ((serialCount == 1 && ch != 2)
                                    || (serialCount == 12 && ch != '\r')
                                    || (serialCount == 13 && ch != '\n'))
                                {
                                    serialCount = 0;
                                    synced = false;
                                    continue;
                                }
                                //TODO: only needed as long as close sequence/reconnect may alter serialCount
                                if (serialCount > 0 || ch == '$')
                                {
                                    teapotPacket[serialCount++] = (char)ch;
                                    //congrats! we have a new packet. 
                                    if (serialCount == 14)
                                    {
                                        //restart packet byte position
                                        serialCount = 0;
                                        //synced has to be false for serial count 0, so that messages can be displayed
                                        synced = false;
                                        //try our best not to lose sync
                                        new SimpleDelegate(PacketAnalyzer).BeginInvoke(null, null);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            Monitor.Exit(synchronizerLock);
                        }
                    }
                    //for debugging purposes
                    /*
                    else
                    {
                        Console.WriteLine("synchronizer mutex block");
                    }*/
                }
                finally
                {
                    closeLock.ExitReadLock();
                }

            }
            
        }

        private void PacketAnalyzer()
        {
            if (closeLock.TryEnterReadLock(0))
            {
                try
                {
                    if (Monitor.TryEnter(packetAnalyzerLock, 0))
                    {
                        try
                        {
                            //needed for simple auto lock/unlock mechanism, as each packet is currently sent twice
                            if (teapotPacket[11] != lastPacketID)
                            {
                                lastPacketID = teapotPacket[11];
                            }
                            else
                            {
                                return;
                            }

                            // get quaternion from data packet
                            q[0] = ((teapotPacket[2] << 8) | teapotPacket[3]) / 16384.0f;
                            q[1] = ((teapotPacket[4] << 8) | teapotPacket[5]) / 16384.0f;
                            q[2] = ((teapotPacket[6] << 8) | teapotPacket[7]) / 16384.0f;
                            q[3] = ((teapotPacket[8] << 8) | teapotPacket[9]) / 16384.0f;
                            for (int i = 0; i < 4; i++) if (q[i] >= 2) q[i] = -4 + q[i];

                            // set our quaternion to new data
                            // adjusted to Inventor Coordinate System
                            oldQuat = quat;
                            quat = new Quaternion(q[0], -q[2], q[3], q[1]);
                            //quat = new Quaternion(q[0], q[1], q[2], q[3]);

                            double diffTheta = oldQuat.Angle - quat.Angle;
                            Vector3D diffVector = Vector3D.Subtract(oldQuat.Axis, quat.Axis);

                            //activate frame timer if detected movement
                            if (diffTheta > MAX_THETA_DIFF_LOCK || diffVector.Length > MAX_AXIS_DIFF_LOCK)
                            {
                                ////if (!inventorFrameTimer.Enabled) inventorFrameTimer.Start();
#if (INV)
                                if (!inventorFrameTimerTEnabled)
                                {
                                    if (inventorFrameTimerT.Change(inventorFrameInterval, inventorFrameInterval)) inventorFrameTimerTEnabled = true;
                                }
#endif
#if (SOLID)
                                if (!solidFrameTimerTEnabled)
                                {
                                    if (solidFrameTimerT.Change(solidFrameInterval, solidFrameInterval)) solidFrameTimerTEnabled = true;
                                }
#endif
                            }
                            //TODO: decide whats better.
                            /*else
                            {
                                if (inventorFrameTimer.Enabled) inventorFrameTimer.Stop(); //move this within the FMS timer?
                            }*/
                        }
                        finally
                        {
                            Monitor.Exit(packetAnalyzerLock);
                        }
                    }
                    //for debugging porposes
                    /*
                    else
                    {
                        Console.WriteLine("packet analyzer mutex block");
                    }
                    */
                }
                finally
                {
                    closeLock.ExitReadLock();
                }
            }
            
        }
        
        //method for updating the inventor cam view
        private void InventorFrameT(object myObject)//Vector3D a, Double theta)
        {
            if (inventorFrameMutex.WaitOne(0))
            {
                //no update over "noise", no update during calibration
                //TODO: how is this not symmetrical??
                temp1 = MovementFilter();
                if (!temp1 || !mpuStable)
                {
                    inventorFrameMutex.ReleaseMutex();
                    return;
                }

                lastLockedQuat = quat;

                Quaternion tempQuat = Quaternion.Multiply(invertedQuat, quat);
                Vector3D a = tempQuat.Axis;
                switch (rotationSelect)
                {
                    case 1:
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        a = tempQuat.Axis;
                        double[] tempAxis = RotateQuaternion(a.X, a.Y, a.Z, invertedQuat.Axis, invertedQuat.Angle);
                        a.X = tempAxis[0];
                        a.Y = tempAxis[1];
                        a.Z = tempAxis[2];
                        break;
                    case 2:
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        a = tempQuat.Axis;
                        break;
                    default:
                        break;
                }

                double theta = tempQuat.Angle;
                theta *= Math.PI / 180;
                //move object instead of the camera
                theta = -theta;

                double[] camPos = RotateQuaternion(0, 0, camDist, a, theta);
                double[] camUp = RotateQuaternion(0, 1, 0, a, theta);

                //avoid exceptions if possible before actually updating the frame
                if (inventorRunning)
                {
                    try
                    {
                        //avoid exceptions if possible
                        if (_invApp.ActiveView != null)
                        {
                            try
                            {
                                //Stopwatch stopWatch = new Stopwatch();
                                //stopWatch.Start();                            
                                Inventor.Camera cam = _invApp.ActiveView.Camera;
                                TransientGeometry tg = _invApp.TransientGeometry;
                                cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);
                                cam.Target = tg.CreatePoint();
                                cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                                cam.ApplyWithoutTransition();
                                //stopWatch.Stop();
                                //Console.WriteLine(stopWatch.ElapsedMilliseconds);
                            }
                            //no active view
                            catch (Exception ex)
                            {
                                MessageBox.Show("Unable to rotate Inventor Camera!\n" + ex.ToString());
                            }
                        }
                    }
                    //no _invApp
                    catch (Exception ex)
                    {
                        inventorRunning = false;
                        MessageBox.Show("Oh no! Something went wrong with Inventor!\n" + ex.ToString());
                    }
                }
                inventorFrameMutex.ReleaseMutex();
            }
            //for debugging purposes
            /*
            else
            {
                Console.WriteLine("inventor frame mutex block");
            }
            */
           
        }


        //method for updating the solid cam view
        //TODO:possibly merge with inventor frame.
        private void SolidFrameT(object myObject)//Vector3D a, Double theta)
        {
            if (solidFrameMutex.WaitOne(0))
            {
                //no update over "noise", no update during calibration
                //TODO: how is this not symmetrical??
                temp2 = MovementFilter();
                if (!temp2 || !mpuStable)
                {
                    solidFrameMutex.ReleaseMutex();
                    return;
                }

                lastLockedQuat = quat;
                Quaternion tempQuat = Quaternion.Multiply(invertedQuat, quat);
                //tempQuat = Quaternion.Multiply(tempQuat, correctionQuat);
                Vector3D a = tempQuat.Axis;
                double theta = tempQuat.Angle;
                //double theta = quat.Angle;
                theta *= Math.PI / 180;
                //move object instead of the camera
                theta = -theta;

                double[] camPos = RotateQuaternion(0, 0, camDist, a, theta);
                double[] camUp = RotateQuaternion(0, 1, 0, a, theta);

                //avoid exceptions if possible before actually updating the frame
                if (solidRunning)
                {
                    try
                    {
                        //avoid exceptions if possible
                        if (_swApp.ActiveDoc != null)
                        {
                            IModelDoc doc = _swApp.ActiveDoc;
                            if (doc.ActiveView != null)
                            {
                                IModelView view = doc.ActiveView;
                                try
                                {

                                    //TODO: make  solid rotate!
                                    tempQuat.Invert();
                                    double[] rotation = QuatToRotation(tempQuat);
                                    arrAssign(ref x, rotation[0], rotation[3], rotation[6]);
                                    arrAssign(ref y, rotation[1], rotation[4], rotation[7]);
                                    arrAssign(ref z, rotation[2], rotation[5], rotation[8]);
                                    //arrAssign(ref transArr, 0, 0, 0);



                                    X = swMathUtility.CreateVector(x);
                                    Y = swMathUtility.CreateVector(y);
                                    Z = swMathUtility.CreateVector(z);
                                    transVect = swMathUtility.CreateVector(transArr);

                                    x = X.ArrayData;
                                    y = Y.ArrayData;
                                    z = Z.ArrayData;
                                    transArr = transVect.ArrayData;
                                    double scale = view.Scale;
                                    MathTransform transform = view.Transform;
                                    MathTransform orientation = swMathUtility.CreateTransform(new double[1]);
                                    orientation.SetData(X, Y, Z, transVect, scale);
                                    view.Orientation3 = orientation;
                                    view.RotateAboutCenter(0, 0);

                                    //orientation.IGetData2(ref X, ref Y, ref Z, ref transform, ref scale);
                                    //orientation.ISetData();

                                    //double[] orientationMat = new double[13] {1, 0, 0,
                                    //                                          0, 1, 0,
                                    //                                          0, 0, 1,
                                    //                                          0, 0, 0,
                                    //                                          1 };


                                    //IMathUtility.ComposeTransform(swVectorX, swVectorY, swVectorZ, xyzOrigin.ConvertToVector, 1#)

                                    /*
                                    //Stopwatch stopWatch = new Stopwatch();
                                    //stopWatch.Start();                            
                                    ICamera cam = view.Camera;
                                    TransientGeometry tg = _invApp.TransientGeometry;
                                    cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);
                                    cam.Target = tg.CreatePoint();
                                    cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                                    doc.GraphicsRedraw();
                                    //stopWatch.Stop();
                                    //Console.WriteLine(stopWatch.ElapsedMilliseconds);*/
                                }
                                //no active view
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Unable to rotate Solid Camera!\n" + ex.ToString());
                                }
                            }                            
                        }
                    }
                    //no _swApp
                    catch (Exception ex)
                    {
                        solidRunning = false;
                        MessageBox.Show("Oh no! Something went wrong with Solid!\n" + ex.ToString());
                    }
                }
                solidFrameMutex.ReleaseMutex();
            }
            //for debugging purposes
            /*
            else
            {
                Console.WriteLine("solid frame mutex block");
            }
            */

        }

        private void arrAssign(ref double[] arr, double a0, double a1, double a2)
        {
            arr[0] = a0;
            arr[1] = a1;
            arr[2] = a2;
        }

        private double[] QuatToRotation(Quaternion a)
        {
            double[] rotation = new double[9];
            rotation[0] = 1 - (2 * a.Y * a.Y + 2 * a.Z * a.Z);
            rotation[1] = 2 * a.X * a.Y + 2 * a.Z * a.W;
            rotation[2] = 2 * a.X * a.Z - 2 * a.Y * a.W;

            rotation[3] = 2 * a.X * a.Y - 2 * a.Z * a.W;
            rotation[4] = 1 - (2 * a.X * a.X + 2 * a.Z * a.Z);
            rotation[5] = 2 * a.Y * a.Z + 2 * a.X * a.W;

            rotation[6] = 2 * a.X * a.Z + 2 * a.Y * a.W;
            rotation[7] = 2 * a.Y * a.Z - 2 * a.X * a.W;
            rotation[8] = 1 - (2 * a.X * a.X + 2 * a.Y * a.Y);

            return rotation;
        }

        //TODO: make a good filter.
        //function that checks whether a an actual movement of the cube was made
        private bool MovementFilter()
        {
            double diffTheta = lastLockedQuat.Angle - quat.Angle;
            Vector3D diffVector = Vector3D.Subtract(lastLockedQuat.Axis, quat.Axis);
            if (!(diffTheta > MAX_THETA_DIFF_UNLOCK || diffVector.Length > MAX_AXIS_DIFF_UNLOCK))
            {
                ////TODO: re-enable.
                ////TODO: recalibrate to avoid drift?
                //avoid jumping due to drifting
                lastLockedQuat = quat;
                //inventorFrameTimer.Stop();
                return false;
            }
            return true;
        }

        //equation due to https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation, specifically:
        //https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation#Quaternion-derived_rotation_matrix
        private double[] RotateQuaternion(double x, double y, double z, Vector3D a, double theta)
        {
            double[] vect = new double[3];
            double c = Math.Cos(theta);
            double s = Math.Sin(theta);
            vect[0] = x * (c + a.X * a.X * (1 - c)) + y * (a.X * a.Y * (1 - c) - a.Z * s) + z * (a.X * a.Z * (1 - c) + a.Y * s);
            vect[1] = x * (a.Y * a.X * (1 - c) + a.Z * s) + y * (c + a.Y * a.Y * (1 - c)) + z * (a.Y * a.Z * (1 - c) - a.X * s);
            vect[2] = x * (a.Z * a.X * (1 - c) - a.Y * s) + y * (a.Z * a.Y * (1 - c) + a.X * s) + z * (c + a.Z * a.Z * (1 - c));
            
            return vect;
        }

        //a ping timer is started upon port opening.
        //a ping checks for activeness of inventor (shuts down otherwise)
        //and for serial port (shuts down itself and frame clock otherwise)
        //and also send a ping over serial.
        private void PingT(object myObject)
        {
#if (INV)
            if (!inventorRunning)
            {
                try
                {
                    _invApp = (Inventor.Application)Marshal.GetActiveObject("Inventor.Application");
                    inventorRunning = true;
                }
                catch (Exception ex)
                {
                    inventorRunning = false;
                }
            }
#endif
#if (SOLID)
            if (!solidRunning)
            {
                try
                {
                    _swApp = (SldWorks)Marshal.GetActiveObject("SldWorks.Application");
                    solidRunning = true;
                }
                catch (Exception ex)
                {
                    solidRunning = false;
                }
            }
#endif
            if (!solidRunning && !inventorRunning)
            {
                this.BeginInvoke(new SimpleDelegate(delegate
                {
                    this.Close();
                }));         
            }
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(pingBuff, 0, 1);
                }
                catch (Exception ex)
                {
                    //TODO: "close" port if error
                    pingTimerT.Change(Timeout.Infinite, Timeout.Infinite);
#if (INV)
                    if (inventorFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite)) inventorFrameTimerTEnabled = false;
#endif
#if (SOLID)
                    if (solidFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite)) solidFrameTimerTEnabled = false;
#endif

                    MessageBox.Show("Oh no! can't write to port!\n" + ex.ToString());

                }

            }
            else
            {
                pingTimerT.Change(Timeout.Infinite, Timeout.Infinite);
#if (INV)
                if (inventorFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite)) inventorFrameTimerTEnabled = false;
#endif
#if (SOLID)
                if (solidFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite)) solidFrameTimerTEnabled = false;
#endif
            }


        }


        //Hopefully, it's enough that the DataRecieved uses BeginInvoke.
        //TODO: make sure there really isn't any deadlock by using Invoke for closing
        private void CloseHandler(object sender, FormClosingEventArgs e)
        {
            this.Invoke(new SimpleDelegate(CloseSequence));
            formClose = true;
        }

        private void CloseSequence()
        {
            //TODO: finish close mutex usage.
            if (closeLock.TryEnterReadLock(0))
            {
                try
                {
                    //timers were definitely already created at this stage

                    pingTimerT.Change(Timeout.Infinite, Timeout.Infinite);
#if (INV)
                    if (inventorFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite)) inventorFrameTimerTEnabled = false;
#endif
#if (SOLID)
                    if (solidFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite)) solidFrameTimerTEnabled = false;
#endif
                    serialPort1.Close();
                    Console.WriteLine("port closed");
                    synced = false;
                    serialCount = 0;
                    using (StreamWriter sw = System.IO.File.CreateText(path))
                    {
                        sw.WriteLine(serialComPort);
                    }
                }
                finally
                {
                    closeLock.ExitReadLock();
                }
            }

    }


        private void buttonCalibrate_Click(object sender, EventArgs e)
        {
            new SimpleDelegate(Calibrate).BeginInvoke(null, null);
        }

        private void Calibrate()
        {
            if (!serialPort1.IsOpen)
            {
                return;
            }
            serialPort1.Write(calBuff, 0, 8);
            mpuStable = false;
            /*//wait for calibration sync loss
            while (!noSync) ;
            //wait for re-sync  
            while (noSync) ;
            Thread.Sleep(3000);*/
            //Quaternion tempQuat = new Quaternion(quat.Axis, -quat.Angle); 
            //correctionQuat = Quaternion.Multiply(tempQuat, adjustmentQuat);
            ///correctionQuat = new Quaternion(quat.Axis, -quat.Angle);
            ////TODO: make software calibration work
            invertedQuat = new Quaternion(quat.Axis, quat.Angle);
            invertedQuat.Invert();
            mpuStable = true;
#if (INV)
            new TimerDelegate(InventorFrameT).BeginInvoke(null, null, null);
#endif
        }


        private void comboBoxPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialComPort = comboBoxPorts.Text;
        }

        private void buttonReconnect_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new SimpleDelegate(delegate
            {
                try
                {
                    serialPort1.Close();
                    Console.WriteLine("port closed");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Oh no! can't close port!\n" + ex.ToString());
                }
                synced = false;
                serialCount = 0;
                OpenPort();
            }));
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rotationSelect = Int32.Parse(comboBox1.Text);
        }
    }
}

//TODO mutiple Cubes, Cube sleep, Autodetect.

