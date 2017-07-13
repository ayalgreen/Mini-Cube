//(C) Copyright 2012 by Autodesk, Inc. 

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

#define DEBUGGER    //works instead of inventor frame!
#define DEBUGG      //show mutex blocks
#define INV
//#define SOLID
//#define WEB


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
using System.Net.Sockets;
using System.Net;
//using InTheHand;
//using InTheHand.Net.Ports;
using InTheHand.Net.Sockets;

using InTheHand.Net.Bluetooth;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace MiniCube
{
    public partial class CubeForm : Form
    {
        //Constants
        //TODO: add try to any port operation?
        int BAUD_RATE = 38400;
        string serialComPort = "COM9";
        int iFPS = 60;
        int sFPS = 60;
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
        TransientGeometry tg;
        bool _inventorStartedByForm = false;
        bool inventorRunning = false;
        int inventorFrameInterval;
        System.Threading.Timer inventorFrameTimerT;
        bool inventorFrameTimerTEnabled = false;
        static Mutex inventorFrameMutex = new Mutex();
        double camDist = 100;

        //solid vars
        SldWorks _swApp;
        MathUtility swMathUtility;
        MathTransform orientation;
        bool _solidStartedByForm = false;
        bool solidRunning = false;
        bool solidDoc = false;
        int solidFrameInterval;
        System.Threading.Timer solidFrameTimerT;
        bool solidFrameTimerTEnabled = false;
        static Mutex solidFrameMutex = new Mutex();

        //server forr web apps
        Server server;
        bool webStarted = false;

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
        Quaternion quat = new Quaternion(0, 0, 0, 1);
        Quaternion oldQuat = new Quaternion(0, 0, 0, 1);
        Quaternion lastLockedQuat = new Quaternion(0, 0, 0, 1);
        Quaternion invertedQuat = new Quaternion(0, 0, 0, 1); //Vect= (0, 1, 0),  Angle=0. Identity.
        //before debugging:
        //Quaternion invertedQuat = new Quaternion(-1, 0, 0, 0); //Vect= (-1, 0, 0),  Angle= 180
        double[] q = new Double[4];
        double[] gravity = new Double[3];
        double[] euler = new Double[3];
        double[] ypr = new Double[3];
        bool formClose = false;
        bool mpuStable = true;
        bool foundCube = false;
        string path = @"./Cubecnfg";
        int[] range3 = new int[3] { 0, 1, 2 };

        //temp vars
        int dbgcounter = 0;
        bool temp1 = false;
        bool temp2 = false;
        int rotationSelect = 0;
        Quaternion unInvertedQuat = new Quaternion(0, 0, 0, 1);
        //before debugging
        //Quaternion unInvertedQuat = new Quaternion(-1, 0, 0, 0);
        Stopwatch quatReadingsWatch = new Stopwatch();
        double [] quatReadingsTimes = new double[10];
        int quatReading = 0;
        int quatReading2 = 0;
        DebugForm debugger;



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

#if (WEB)
            server = new Server(this);
            webStarted = true;
#endif

#if (DEBUGGER)
            Console.WriteLine("Enter Debugg Mode!");
            debugger = new DebugForm(this);
#endif

            ////TODO: make software calibration work
            invertedQuat.Invert();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxPorts.Items.Add(port);
            }
            Console.WriteLine("Load config...");
            LoadConfig();
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
            inventorFrameInterval = (int)(1000 / iFPS);
#if (DEBUGGER)
            inventorFrameTimerT = new System.Threading.Timer(InventorFrameDebug, null, Timeout.Infinite, Timeout.Infinite);
#else
            inventorFrameTimerT = new System.Threading.Timer(InventorFrameT, null, Timeout.Infinite, Timeout.Infinite);
#endif

#endif
#if (SOLID)
            solidFrameInterval = (int)(1000 / sFPS);
            solidFrameTimerT = new System.Threading.Timer(SolidFrameT, null, Timeout.Infinite, Timeout.Infinite);
#endif

            pingTimerInterval = 2000;

            pingTimerT = new System.Threading.Timer(PingT, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void LoadConfig()
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
                tg = _invApp.TransientGeometry;
                inventorRunning = true;
            }
            catch (Exception ex)
            {
                try
                {
                    Type invAppType = Type.GetTypeFromProgID("Inventor.Application");

                    _invApp = (Inventor.Application)System.Activator.CreateInstance(invAppType);
                    tg = _invApp.TransientGeometry;
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
            orientation = swMathUtility.CreateTransform(new double[1]);
        }

        //TODO remove this useless ass server
        private void StartServer()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            server.Start();

            Console.WriteLine("Server has started on 127.0.0.1:8080");
            Console.WriteLine("Waiting for a connection...");

            TcpClient client = server.AcceptTcpClient();

            Console.Write("A client connected.");

            NetworkStream stream = client.GetStream();

            //enter to an infinite cycle to be able to handle every change in stream
            while (true)
            {
                while (!stream.DataAvailable) ;

                byte[] bytes = new byte[client.Available];
                stream.Read(bytes, 0, bytes.Length);

                //translate bytes of request to string
                string request = Encoding.UTF8.GetString(bytes);
                if (new Regex("^GET").IsMatch(request))
                {
                    byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + System.Environment.NewLine
                        + "Connection: Upgrade" + System.Environment.NewLine
                        + "Upgrade: websocket" + System.Environment.NewLine
                        + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                            SHA1.Create().ComputeHash(
                                Encoding.UTF8.GetBytes(
                                    new Regex("Sec-WebSocket-Key: (.*)").Match(request).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                )
                            )
                        ) + System.Environment.NewLine + System.Environment.NewLine);

                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    byte[] response = Encoding.UTF8.GetBytes("Data received");
                    response[0] = 0x81; // denotes this is the final message and it is in text
                    response[1] = (byte)(response.Length - 2); // payload size = message - header size
                    stream.Write(response, 0, response.Length);
                }
            }
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
                    quatReadingsWatch.Start();
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
#if (DEBUGG)
                    else
                    {
                        Console.WriteLine("serial port data mutex block {0}", dbgcounter);
                        dbgcounter++;
                    }
#endif
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
#if (DEBUGG)                    
                    else
                    {
                        Console.WriteLine("synchronizer mutex block");
                    }
#endif
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
                            quat = new Quaternion(q[0], q[2], -q[3], q[1]);
                            //this was before debugging
                            //quat = new Quaternion(q[0], -q[2], q[3], q[1]);
                            /*//checking quat update speed
                            quatReading2++;
                            if (quatReading2 >= 10)
                            {
                                quatReadingsTimes[quatReading] = quatReadingsWatch.ElapsedMilliseconds;
                                quatReading++;
                                quatReading2 = 0;
                            }
                            
                            if (quatReading >= 10)
                            {
                                quatReading = 0;
                                double[] qrt = quatReadingsTimes;
                                Console.WriteLine("quat readings: {0} {1} {2} {3} {4} {5} {6} {7} {8}", qrt[1] - qrt[0], 
                                   qrt[2] - qrt[1], qrt[3] - qrt[2], qrt[4] - qrt[3], qrt[5] - qrt[4], qrt[6] - qrt[5], 
                                   qrt[7] - qrt[6], qrt[8] - qrt[7], qrt[9] - qrt[8]);
                            }*/

                            double diffTheta = oldQuat.Angle - quat.Angle;
                            Vector3D diffVector = Vector3D.Subtract(oldQuat.Axis, quat.Axis);

                            //activate frame timer if detected movement
                            if (diffTheta > MAX_THETA_DIFF_LOCK || diffVector.Length > MAX_AXIS_DIFF_LOCK)
                            {
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
#if (DEBUGG)
                    else
                    {
                        Console.WriteLine("packet analyzer mutex block");
                    }
#endif
                }
                finally
                {
                    closeLock.ExitReadLock();
                }
            }
            
        }

        //method for getting the corrected current quat (used for webserver now)
        //TODO: use in all apps
        public float[] GetCorrectedQuatFloats()
        {
            Quaternion tempQuat = Quaternion.Multiply(invertedQuat, quat);
            //Console.WriteLine("passing quat values: {0}, {1}, {2}, {3}", (float)tempQuat.X, (float)tempQuat.Y, (float)tempQuat.Z, (float)tempQuat.W);
            return new float[4] { (float)tempQuat.X, (float)tempQuat.Y, (float)tempQuat.Z, (float)tempQuat.W};
        }
        

        /*old
        private void InventorFrameT(object myObject)//Vector3D a, Double theta)
        {
            Stopwatch stopWatch = new Stopwatch();
            double[] times = new double[8];
            stopWatch.Start();
            if (inventorFrameMutex.WaitOne(0))
            {
                //no update over "noise", no update during calibration
                temp1 = MovementFilter();
                if (!temp1 || !mpuStable)
                {
                    inventorFrameMutex.ReleaseMutex();
                    return;
                }

                lastLockedQuat = quat;
                double[,] currRotation = QuatToRotation(quat);
                double[,] invCalRotation = QuatToRotation(invertedQuat);
                double[,] calRotation = QuatToRotation(unInvertedQuat);

                double[,] relativeRotation = MatMultiply(currRotation, invCalRotation);
                double[,] relativeRotation2 = MatMultiply(invCalRotation, currRotation);
                Quaternion tempQuat1 = Quaternion.Multiply(quat, invertedQuat);
                double[,] relativeRotation3 = QuatToRotation(tempQuat1);
                double[,] finalRot1 = MatMultiply(relativeRotation, calRotation);
                finalRot1 = MatMultiply(invCalRotation, finalRot1);
                double[,] finalRot2 = MatMultiply(invCalRotation, currRotation);
                Quaternion halfAngle = new Quaternion(unInvertedQuat.Axis, unInvertedQuat.Angle / 2);
                double[,] halfRotation = QuatToRotation(halfAngle);
                Quaternion invHalfAngle = new Quaternion(unInvertedQuat.Axis, unInvertedQuat.Angle / 2);
                double[,] invHalfRotation = QuatToRotation(invHalfAngle);
                invHalfAngle.Invert();
                double[,] testRotation;


                //just like option 8 - relative movement!
                Quaternion tempQuat = Quaternion.Multiply(invertedQuat, quat);
                double[,] finalRot3 = QuatToRotation(tempQuat);
                Vector3D a = tempQuat.Axis;

                double theta = tempQuat.Angle;
                theta *= Math.PI / 180;
                //move object instead of the camera
                theta = -theta;

                double[] camPos = RotateQuaternion(0, 0, camDist, a, theta);
                double[] camUp = RotateQuaternion(0, 1, 0, a, theta);

                double[] tempAxis;
                switch (rotationSelect)
                {
                    case 1:
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        a = tempQuat.Axis;
                        tempAxis = RotateQuaternion(a.X, a.Y, a.Z, invertedQuat.Axis, invertedQuat.Angle);
                        a.X = tempAxis[0];
                        a.Y = tempAxis[1];
                        a.Z = tempAxis[2];

                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;

                        camPos = RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = RotateQuaternion(0, 1, 0, a, theta);

                        break;
                    case 2:
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        a = tempQuat.Axis;

                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;

                        camPos = RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    case 3:

                        camPos = MatVectMultiply(finalRot3, new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(finalRot3, new double[3] { 0, 1, 0 });

                        break;
                    case 4:
                        camPos = MatVectMultiply(finalRot2, new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(finalRot2, new double[3] { 0, 1, 0 });

                        break;

                    case 5:

                        camPos = MatVectMultiply(MatInverse(finalRot3), new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(MatInverse(finalRot3), new double[3] { 0, 1, 0 });

                        break;
                    case 6:
                        camPos = MatVectMultiply(MatInverse(finalRot2), new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(MatInverse(finalRot2), new double[3] { 0, 1, 0 });

                        break;
                    case 7:
                        camPos = MatVectMultiply(MatInverse(relativeRotation), new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(MatInverse(relativeRotation), new double[3] { 0, 1, 0 });

                        break;

                    case 8:
                        camPos = MatVectMultiply(relativeRotation, new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(relativeRotation, new double[3] { 0, 1, 0 });

                        break;
                    case 9:
                        tempQuat = Quaternion.Multiply(invHalfAngle, quat);
                        tempQuat = Quaternion.Multiply(quat, halfAngle);

                        a = tempQuat.Axis;

                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;

                        camPos = RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    case 10:
                        tempQuat = Quaternion.Multiply(halfAngle, quat);
                        tempQuat = Quaternion.Multiply(quat, invHalfAngle);

                        a = tempQuat.Axis;

                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;

                        camPos = RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    case 11:
                        testRotation = MatMultiply(relativeRotation, invCalRotation);
                        testRotation = MatMultiply(calRotation, testRotation);

                        camPos = MatVectMultiply(testRotation, new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(testRotation, new double[3] { 0, 1, 0 });

                        break;
                    case 12:
                        testRotation = MatMultiply(invCalRotation, relativeRotation);
                        camPos = MatVectMultiply(testRotation, new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(testRotation, new double[3] { 0, 1, 0 });
                        break;
                    case 13:
                        testRotation = MatMultiply(relativeRotation, invHalfRotation);
                        testRotation = MatMultiply(halfRotation, testRotation);

                        camPos = MatVectMultiply(testRotation, new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(testRotation, new double[3] { 0, 1, 0 });
                        break;
                    case 14:
                        testRotation = MatMultiply(relativeRotation, halfRotation);
                        testRotation = MatMultiply(invHalfRotation, testRotation);

                        camPos = MatVectMultiply(testRotation, new double[3] { 0, 0, camDist });
                        camUp = MatVectMultiply(testRotation, new double[3] { 0, 1, 0 });
                        break;
                    case 15:
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        a = tempQuat.Axis;
                        tempAxis = RotateQuaternion(a.X, a.Y, a.Z, unInvertedQuat.Axis, unInvertedQuat.Angle);
                        a.X = tempAxis[0];
                        a.Y = tempAxis[1];
                        a.Z = tempAxis[2];

                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;

                        camPos = RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = RotateQuaternion(0, 1, 0, a, theta);
                        break;

                    default:
                        break;
                }


                times[0] = stopWatch.ElapsedMilliseconds;
                //avoid exceptions if possible before actually updating the frame
                if (inventorRunning)
                {
                    try
                    {
                        //TODO: this doesn't necesarily throw exception when inventor is off
                        //avoid exceptions if possible
                        if (_invApp.ActiveView != null)
                        {
                            try
                            {
                                Inventor.Camera cam = _invApp.ActiveView.Camera;
                                TransientGeometry tg = _invApp.TransientGeometry;
                                double[] eyeArr = new double[3] { 0, 0, 0 }; //
                                cam.Eye.GetPointData(ref eyeArr); //
                                double[] targetArr = new double[3] { 0, 0, 0 }; //
                                cam.Target.GetPointData(ref targetArr); //
                                double[] camVector = new double[3]
                                        {eyeArr[0]-targetArr[0], eyeArr[1] - targetArr[1], eyeArr[2] - targetArr[2] };//
                                camDist = Math.Sqrt(camVector[0] * camVector[0] + camVector[1] * camVector[1] + camVector[2] * camVector[2]);
                                /*i think the algorithm should be:
                                 * get rotation for Z axis to targetpoint (call it A)
                                 * rotate Z axis (probably a point on it with same dist as target vector size)
                                 * rotate said point by A (potentially conjugated by the quat. not sure about this?)
                                 * rotate cam vector by a similar procedure(?)
                                 *
                                camPos = RotateQuaternion(0, 0, camDist, a, theta);
                                cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);
                                cam.Target = tg.CreatePoint();
                                cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                                cam.ApplyWithoutTransition();
                            }
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
        }*
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
stopWatch.Stop();
//Console.WriteLine("inventor: {0} {1} {2} {3} {4} {5} {6} {7} total: {8}", times[0], times[1] - times[0], times[2] - times[1], times[3] - times[2], times[4] - times[3], times[5] - times[4], times[6] - times[5], times[7] - times[6], times[7]);
}
//for debugging purposes

/*else
{
Console.WriteLine("inventor frame mutex block");
}*
}*/


        private void InventorFrameDebug(object myObject)
        {
            debugger.Frame(quat, invertedQuat, unInvertedQuat, camDist);
            
        }


        //method for updating the inventor cam view
        private void InventorFrameT(object myObject)//Vector3D a, Double theta)
        {
            //no update over "noise", no update during calibration
            temp1 = MovementFilter();
            if (!temp1 || !mpuStable)
            {
                return;
            }
            lastLockedQuat = quat;
            
            //just like option 8 - relative movement!
            Quaternion tempQuat = Quaternion.Multiply(invertedQuat, quat);
            Vector3D a = tempQuat.Axis;

            double theta = tempQuat.Angle;
            theta *= Math.PI / 180;
            //move object instead of the camera
            theta = -theta;

            double[] camPos = RotateQuaternion(0, 0, camDist, a, theta);
            double[] camUp = RotateQuaternion(0, 1, 0, a, theta);
            
            ShowInvFrame(a, theta, camUp);
        }

        //normal use - displaying a frame.
        public bool ShowInvFrame(Vector3D a, double theta, double[] camUp)
        {
            Stopwatch stopWatch = new Stopwatch();
            double[] times = new double[8];
            stopWatch.Start();
            if (inventorFrameMutex.WaitOne(0))
            {
                times[0] = stopWatch.ElapsedMilliseconds;
                //avoid exceptions if possible before actually updating the frame
                if (inventorRunning)
                {
                    try
                    {
                        //TODO: this doesn't necesarily throw exception when inventor is off
                        //avoid exceptions if possible
                        if (_invApp.ActiveView != null)
                        {
                            try
                            {
                                Inventor.Camera cam = _invApp.ActiveView.Camera;
                                double[] eyeArr = new double[3] { 0, 0, 0 }; //
                                cam.Eye.GetPointData(ref eyeArr); //
                                double[] targetArr = new double[3] { 0, 0, 0 }; //
                                cam.Target.GetPointData(ref targetArr); //
                                double[] camVector = new double[3]
                                        {eyeArr[0]-targetArr[0], eyeArr[1] - targetArr[1], eyeArr[2] - targetArr[2] };//
                                camDist = Math.Sqrt(camVector[0] * camVector[0] + camVector[1] * camVector[1] + camVector[2] * camVector[2]);
                                /*i think the algorithm should be:
                                 * get rotation for Z axis to targetpoint (call it A)
                                 * rotate Z axis (probably a point on it with same dist as target vector size)
                                 * rotate said point by A (potentially conjugated by the quat. not sure about this?)
                                 * rotate cam vector by a similar procedure(?)
                                 */
                                double[] camPos = RotateQuaternion(0, 0, camDist, a, theta);
                                cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);
                                cam.Target = tg.CreatePoint();
                                cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                                cam.ApplyWithoutTransition();                                
                            }
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
                stopWatch.Stop();
                //Console.WriteLine("inventor: {0} {1} {2} {3} {4} {5} {6} {7} total: {8}", times[0], times[1] - times[0], times[2] - times[1], times[3] - times[2], times[4] - times[3], times[5] - times[4], times[6] - times[5], times[7] - times[6], times[7]);
            }
#if (DEBUGG)
            else
            {
                Console.WriteLine("inventor frame mutex block");
                return false;
            }
#endif
            return true;
        }

        //for debugg use - display according to pos and up
        public bool ShowInvFrame(double[] camPos, double[] camUp)
        {
            Stopwatch stopWatch = new Stopwatch();
            double[] times = new double[8];
            stopWatch.Start();
            if (inventorFrameMutex.WaitOne(0))
            {
                //avoid exceptions if possible before actually updating the frame
                if (inventorRunning)
                {
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
                                cam.Eye = tg.CreatePoint(camPos[0], camPos[1], camPos[2]);
                                times[2] = stopWatch.ElapsedMilliseconds;
                                cam.Target = tg.CreatePoint();
                                times[3] = stopWatch.ElapsedMilliseconds;
                                cam.UpVector = tg.CreateUnitVector(camUp[0], camUp[1], camUp[2]);
                                times[4] = stopWatch.ElapsedMilliseconds;
                                cam.ApplyWithoutTransition();
                                times[5] = stopWatch.ElapsedMilliseconds;
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
                stopWatch.Stop();
                Console.WriteLine("inventor: {0} {1} {2} {3} {4} {5} total: {6}", times[0], times[1] - times[0], times[2] - times[1], times[3] - times[2], times[4] - times[3], times[5] - times[4], times[5]);
            }
#if (DEBUGG)
            else
            {
                Console.WriteLine("inventor frame mutex block");
                return false;
            }
#endif
            return true;
        }


        //method for updating the solid cam view
        //TODO:possibly merge with inventor frame.
        private void SolidFrameT(object myObject)//Vector3D a, Double theta)
        {
            Stopwatch stopWatch = new Stopwatch();
            double[] times = new double[8];
            stopWatch.Start();

            if (solidFrameMutex.WaitOne(0))
            {
                //no update over "noise", no update during calibration
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
                //0 ms
                times[0] = stopWatch.ElapsedMilliseconds;
                //avoid exceptions if possible before actually updating the frame
                if (solidRunning)
                {
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
                                //TODO: make  solid rotate!
                                tempQuat.Invert();
                                double[,] rotation = QuatToRotation(tempQuat);
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
                                view.RotateAboutCenter(0, 0);
                                //view.GraphicsRedraw(new int[] { });
                                times[7] = stopWatch.ElapsedMilliseconds;

                            }
                            //no active view
                            catch (Exception ex)
                            {
                                solidDoc = false;
                                //MessageBox.Show("Unable to rotate Solid Camera!\n" + ex.ToString());
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
                stopWatch.Stop();
                //Console.WriteLine("solid: {0} {1} {2} {3} {4} {5} {6} {7} total: {8}", times[0], times[1]-times[0], times[2]-times[1], 
                //    times[3]-times[2], times[4]-times[3], times[5]-times[4], times[6]-times[5], times[7] - times[6], times[7]);
            }
#if (DEBUGG)
            else
            {
                Console.WriteLine("solid frame mutex block");
            }
#endif
        }

        //TODO: make a good filter.
        //function that checks whether a an actual movement of the cube was made
        private bool MovementFilter()
        {
            double diffTheta = lastLockedQuat.Angle - quat.Angle;
            Vector3D diffVector = Vector3D.Subtract(lastLockedQuat.Axis, quat.Axis);
            if (!(diffTheta > MAX_THETA_DIFF_UNLOCK || diffVector.Length > MAX_AXIS_DIFF_UNLOCK))
            {
                ////TODO: dis/re-enable timers?
                ////TODO: recalibrate to prevent stationary drift of cube over time?
                //avoid jumping due to drifting
                lastLockedQuat = quat;
                //inventorFrameTimer.Stop();
                return false;
            }
            return true;
        }

        private void arrAssign(ref double[] arr, double a0, double a1, double a2)
        {
            arr[0] = a0;
            arr[1] = a1;
            arr[2] = a2;
        }

        //according to http://www.opengl-tutorial.org/assets/faq_quaternions/index.html#Q54
        //NOT according to wikipedia https://en.wikipedia.org/wiki/Rotation_matrix#Quaternion (wtf?)
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

        //returns AB
        public double[,] MatMultiply(double[,] a, double [,] b)
        {
            double[,] c = new double[3, 3];
            double currSum = 0;
            foreach (int i in range3)
            {
                foreach(int j in range3)
                {
                    foreach (int k in range3) {
                        currSum += a[i, k] * b[k, j];
                    }
                    c[i, j] = currSum;
                    currSum = 0;
                }
            }
            return c;
        }

        //returns AB
        public double[] MatVectMultiply(double[,] a, double[] b)
        {
            double[] c = new double[3];
            double currSum = 0;
            foreach (int i in range3)
            {
                foreach (int j in range3)
                {
                    currSum += a[i, j] * b[j];
                }
                c[i] = currSum;
                currSum = 0;
            }

            return c;
        }

        //inverting a rotation is just its transpose
        public double[,] MatInverse(double[,] a)
        {
            double[,] transMat = new double[3, 3];
            foreach (int i in range3)
            {
                foreach(int j in range3)
                {
                    transMat[i, j] = a[j, i];
                }
            }
            return transMat;
        }
 
        //equation due to https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation#Quaternion-derived_rotation_matrix
        //seems to also invert the matrix! (wtf?)
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
            if (!solidRunning && !inventorRunning && !webStarted)
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
            //if (closeLock.TryEnterReadLock(0)) {
            closeLock.EnterWriteLock();
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
#if (WEB)
                server.stopServer();                    
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
                //closeLock.ExitReadLock();
                closeLock.ExitWriteLock();
            }

            //}

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
            //what does it do now??
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
            //invertedQuat = new Quaternion(quat.Axis, quat.Angle);
            unInvertedQuat = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
            invertedQuat = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
            invertedQuat.Invert();
            mpuStable = true;
#if (INV)
            new TimerDelegate(InventorFrameT).BeginInvoke(null, null, null);
#endif
#if (DEBUGGER)
            debugger.BeginInvoke(new SimpleDelegate(debugger.UpdateDisplayCal));
#endif
        }

        public void SetInvQuat(Vector3D tempAxis, double tempTheta)
        {
            try
            {
                invertedQuat = new Quaternion(tempAxis, tempTheta);
                unInvertedQuat = new Quaternion(tempAxis, tempTheta);
                unInvertedQuat.Invert();
            }
            catch (InvalidOperationException ex)
            {
                invertedQuat = new Quaternion();
                unInvertedQuat = new Quaternion();
                unInvertedQuat.Invert();
            }
        }

        public void SetCalQuat(Vector3D tempAxis, double tempTheta)
        {
            try
            {
                invertedQuat = new Quaternion(tempAxis, tempTheta);
                invertedQuat.Invert();
                unInvertedQuat = new Quaternion(tempAxis, tempTheta);
            }
            catch (InvalidOperationException ex)
            {
                invertedQuat = new Quaternion();
                invertedQuat.Invert();
                unInvertedQuat = new Quaternion();
            }


        }

        public Quaternion GetInvQuat()
        {
            return invertedQuat;
        }

        public Quaternion GetCalQuat()
        {
            return unInvertedQuat;
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


        public void setRotationSelect(int selection)
        {
            rotationSelect = selection;
        }

        public int getRotationSelect()
        {
            return rotationSelect;
        }


    }
}

//TODO mutiple Cubes, Cube sleep, Autodetect.

