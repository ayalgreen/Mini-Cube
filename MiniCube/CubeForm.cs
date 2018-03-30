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

//#define BT
//#define DEBUGGER    
//#define DEBUGG      //show mutex blocks
//#define QUATREADMON
#define CONNECTMON //monitor the opening/aborting of port connection
#define SYNCMON
#define SERVER
#define MOUSEPAN


using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using Inventor;
using System.Runtime.InteropServices;
using System.Threading;
//using InTheHand;
//using InTheHand.Net.Ports;
using InTheHand.Net.Bluetooth;
using System.Collections.Generic;
using InTheHand.Net.Sockets;
using System.Diagnostics;

namespace MiniCube
{
    public partial class CubeForm : Form
    {
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% Variables %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        #region vars
        //Constants
        const int MAX_ClICK_COUNTER = 0xFF;
        const int BAUD_RATE = 38400;
        //inventor FPS. obsolete (for debugger mode)
        int iFPS = 60;
        const int BTTimeoutSeconds = 6;
        const double MAX_THETA_DIFF_LOCK = 0.05;
        const double MAX_AXIS_DIFF_LOCK = 0.0005;
        const double MAX_THETA_DIFF_UNLOCK = 0.01;
        const double MAX_AXIS_DIFF_UNLOCK = 0.0001;
        const string CUBE_BT_MODULE = "Cube";
        const string BT_PIN = "1234";
        const string path = @"./Cubecnfg";
        public const int NUM_BUTTONS = 6;
        public const int TOP = 0;
        public const int BOTTOM = 1;
        public const int LEFT = 2;
        public const int RIGHT = 3;
        public const int FRONT = 4;
        public const int BACK = 5;
        int[] range3 = new int[3] { 0, 1, 2 };

        //delegates
        public delegate void SimpleDelegate();
        public delegate void TimerDelegate(object myObject);
        public delegate void DataProcessDelegate(byte[] buffer);
        public delegate void CameraLockDelegate(Vector3D a, double theta);

        //TCP server for web apps and plugins 
        Server server;
        bool serverStarted = false;

        //comm vars
        string serialComPort = "";
        SerialPort serialPort1 = new SerialPort();
        bool portError = false;
        System.Threading.Timer BTTimer;
        int BTTimerInterval = 1000;
        Thread portOpenerThread;
        bool oldProtocol = false;

#if (BT)        
        BluetoothDeviceInfo bluetoothDevice;
        Guid mUUID = new Guid("00001101-0000-1000-8000-00805F9B34FB");        
#endif

        //comm protocol vars
        char[] packet = new char[26];  // cube packet
        int serialCount = 0;                 // current packet byte position
        bool packetStarted = false;
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
        Quaternion invCalQuat = new Quaternion(0, 0, 0, 1); //Vect= (0, 1, 0),  Angle=0. Identity.
        Quaternion worldQuat = new Quaternion(0, 0, 0, 1); //Vect= (0, 1, 0),  Angle=0. Identity.
        float[] panSpeed = { 0, 0, 0 };
        int zoom = 0;
        //receive array from cube
        double[] q = new Double[4];
        bool mpuStable = true;
        bool cubeConnected = false;
        bool calAlgNum2 = false;
        bool closed = false;

        //debug vars
        int dbgcounter = 0;
        Quaternion unInvertedQuat = new Quaternion(0, 0, 0, 1);
        Stopwatch quatReadingsWatch = new Stopwatch();
        double[] quatReadingsTimes = new double[10];
        int quatReading = 0;
        int quatReading2 = 0;
        DebugForm debugger;
        String closeMutexOwner = "";
        bool formClose;
        bool noFullSync = true;
        bool noSync = true;

        //button click variable
        //in order to keep track of button clicks in a way that the clients
        //don't miss them, button clicks and release will be monitored by
        //counters which increase the appropriate event
        int[] buttonClicks = { 0, 0, 0, 0, 0, 0 };
        int[] buttonReleases = { 0, 0, 0, 0, 0, 0 };
        //int[] newButtonClicks = { 0, 0, 0, 0, 0, 0 };
        //int[] newButtonReleases = { 0, 0, 0, 0, 0, 0 };

#if (MOUSEPAN)
        //bind the mouse movement to affect panning
        bool mousePan = false;
        Thread mouseWatcherThread;
        bool mousePanStarted = false;
#endif

        //cradle
        bool cradleConnected = false;
        Thread portOpenerThread2;
        string serialComPortCradle = "";
        SerialPort serialPort2 = new SerialPort();
        bool portError2 = false;
        bool allInCradle = false;
        static Mutex serialPort2DataMutex = new Mutex();
        Object synchronizer2Lock = new Object();
        Object packetAnalyzer2Lock = new Object();
        char[] packet2 = new char[26];  // cube packet
        int serialCount2 = 0;                 // current packet byte position
        bool packetStarted2 = false;
        bool noFullSync2 = true;
        bool noSync2 = true;
        //inventor vars - obsolete. only for debugger mode
        /*
        Inventor.Application _invApp;
        TransientGeometry tg;
        bool _inventorStartedByForm = false;
        bool inventorRunning = false;
        int inventorFrameInterval;
        System.Threading.Timer inventorFrameTimerT;
        bool inventorFrameTimerTEnabled = false;
        static Mutex inventorFrameMutex = new Mutex();
        double camDist = 100;
        bool inventorMovement = false;*/

        #endregion
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% Setup %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

        #region Setup

        public CubeForm()
        {
            InitializeComponent();
            Console.WriteLine("Initializing...");
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(CloseHandler);

#if (SERVER)
            server = new Server(this);
            serverStarted = true;
#endif

#if (DEBUGGER)
            StartInventor();
            Debug.WriteLine("Enter Debugg Mode!");
            debugger = new DebugForm(this);
#endif
            //TODO: make software calibration work
            invCalQuat.Invert();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxPorts.Items.Add(port);
                comboBoxPortsCradle.Items.Add(port);
            }
#if (MOUSEPAN)
            mouseWatcherThread = new Thread(this.MouseWatcher);
            this.MouseClick += MouseClickAction;
#endif
            Debug.WriteLine("Load config...");
            LoadConfig();
            Debug.WriteLine("Set timers...");
            SetTimers();
            Debug.WriteLine("Opening port...");
            OpenPort();
            //OpenBluetooth();
        }

        private void SetTimers()
        {

#if (DEBUGGER)
            inventorFrameInterval = (int)(1000 / iFPS);
            inventorFrameTimerT = new System.Threading.Timer(InventorFrameDebug, null, Timeout.Infinite, Timeout.Infinite);
#endif
            pingTimerInterval = 2000;

            pingTimerT = new System.Threading.Timer(PingT, null, Timeout.Infinite, Timeout.Infinite);
            BTTimer = new System.Threading.Timer(CubeSearcher, null, Timeout.Infinite, Timeout.Infinite);

        }

        private void LoadConfig()
        {
            if (System.IO.File.Exists(path))
            {
                using (StreamReader sr = System.IO.File.OpenText(path))
                {
                    string s = "";
                    s = sr.ReadLine();
                    if (s == null)
                    {
                        return;
                    }
                    if (comboBoxPorts.Items.Contains(s))
                    {
                        comboBoxPorts.Text = s;
                        serialComPort = s;
                    }
                    s = sr.ReadLine();
                    if (s == null)
                    {
                        return;
                    }
                    if (comboBoxPortsCradle.Items.Contains(s))
                    {
                        comboBoxPortsCradle.Text = s;
                        serialComPortCradle = s;
                    }
                }
            }
        }

        //method for opening a port must be run on UI thread!
        private void OpenPort()
        {
            buttonReconnect.Enabled = false;
            buttonReconnect.Text = "Opening";
            this.Icon = Properties.Resources.off;
            cubeConnected = false;
            serialPort1 = new SerialPort();
            serialPort1.ReadTimeout = 200;
            serialPort1.WriteTimeout = 200;
            serialPort1.PortName = serialComPort;
            serialPort1.BaudRate = BAUD_RATE;
            serialPort1.ParityReplace = (byte)0;
            serialPort1.DtrEnable = true;
            //event handler to be run on *secondary thread*
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(SerialPort1DataReceived);
            //select the port whose  port name is the required
            foreach (string port in SerialPort.GetPortNames())
            {
                if (port == serialPort1.PortName)
                {
                    portError = false;
                    Thread newThread = new Thread(this.OpenPortExecutioner);
                    newThread.Start();
                    return;
                }
            }
        }

        private void OpenCradlePort()
        {
            buttonReconnectCradle.Enabled = false;
            buttonReconnectCradle.Text = "Opening";
            //this.Icon = Properties.Resources.off;
            cradleConnected = false;
            serialPort2 = new SerialPort();
            serialPort2.ReadTimeout = 200;
            serialPort2.WriteTimeout = 200;
            serialPort2.PortName = serialComPortCradle;
            serialPort2.BaudRate = BAUD_RATE;
            serialPort2.ParityReplace = (byte)0;
            serialPort2.DtrEnable = true;
            //event handler to be run on *secondary thread*
            serialPort2.DataReceived += new SerialDataReceivedEventHandler(SerialPort2DataReceived);
            //select the port whose  port name is the required
            foreach (string port in SerialPort.GetPortNames())
            {
                if (port == serialPort2.PortName)
                {
                    portError2 = false;
                    Thread newThread = new Thread(this.OpenPort2Executioner);
                    newThread.Start();
                    return;
                }
            }
        }

        //helper function for a timedout OpenPort
        public void OpenPortExecutioner()
        {
            if (portOpenerThread != null)
            {
                if (portOpenerThread.IsAlive)
                {
                    Debug.WriteLine("Stopping previous port open execution");
                    portOpenerThread.Abort();
                }
            }
            //wait in case port was recently closed. 200ms did it on my PC..
            Thread.Sleep(500);
#if (CONNECTMON)
            Stopwatch connectWatch = new Stopwatch();
            connectWatch.Start();
            long[] times = new long[5];
#endif
            //this doesnt terminate previous thread
            portOpenerThread = new Thread(this.OpenPortExecution);
            portOpenerThread.Start();
#if (CONNECTMON)
            times[0] = connectWatch.ElapsedMilliseconds;
#endif
            //timeout for the connection 'try'
            if (!portOpenerThread.Join(TimeSpan.FromSeconds(BTTimeoutSeconds)))
            {
#if (CONNECTMON)
                times[1] = connectWatch.ElapsedMilliseconds;
#endif

                portOpenerThread.Abort();
#if (CONNECTMON)
                times[2] = connectWatch.ElapsedMilliseconds;
#endif
                //moved to whithin catch clause
                //portError = true;
                //Debug.WriteLine("could not open port " + serialPort1.PortName);
            }
            if (this.IsHandleCreated)
            {
#if (CONNECTMON)
                times[3] = connectWatch.ElapsedMilliseconds;
#endif
                this.BeginInvoke(new SimpleDelegate(delegate
                {
                    buttonReconnect.Text = "Reconnect";
                    buttonReconnect.Enabled = true;
                }));
#if (CONNECTMON)
                times[4] = connectWatch.ElapsedMilliseconds;
                Debug.WriteLine("connect/abort times: {0}, {1}, {2}, {3}, {4}", times[0], times[1] - times[0], times[2] - times[1], times[3] - times[2], times[4] - times[3]);
#endif
            }
            if (!cubeConnected)
            {
                //TODO finish cube open timer
                BTTimer.Change(BTTimerInterval, BTTimerInterval);
            }
        }

        //threaded version for allowing timeout
        public void OpenPortExecution()
        {
            try
            {
                serialPort1.Open();
                if (serialPort1.IsOpen)
                {
                    Debug.WriteLine("Port opened.");
                    //TODO move to BT section
                    cubeConnected = true;
                }
                else
                {
                    Debug.WriteLine("Could not open port (no exception)");
                }
                pingTimerT.Change(pingTimerInterval, pingTimerInterval);
            }
            //if can't open port
            catch (Exception ex)
            {
                portError = true;
                //MessageBox.Show("Could not open port " + serialPort1.PortName);
                Debug.WriteLine("Could not open port " + serialPort1.PortName + ": " + ex.Message);
                return;
            }
            quatReadingsWatch.Start();
        }

        //helper function for a timedout OpenPort
        public void OpenPort2Executioner()
        {
            if (portOpenerThread2 != null)
            {
                if (portOpenerThread2.IsAlive)
                {
                    Debug.WriteLine("Stopping previous port open execution");
                    portOpenerThread2.Abort();
                }
            }
            //wait in case port was recently closed. 200ms did it on my PC..
            Thread.Sleep(500);
#if (CONNECTMON)
            Stopwatch connectWatch = new Stopwatch();
            connectWatch.Start();
            long[] times = new long[5];
#endif
            //this doesnt terminate previous thread
            portOpenerThread2 = new Thread(this.OpenPortExecution2);
            portOpenerThread2.Start();
#if (CONNECTMON)
            times[0] = connectWatch.ElapsedMilliseconds;
#endif
            //timeout for the connection 'try'
            if (!portOpenerThread2.Join(TimeSpan.FromSeconds(100*BTTimeoutSeconds)))
            {
#if (CONNECTMON)
                times[1] = connectWatch.ElapsedMilliseconds;
#endif

                portOpenerThread2.Abort();
#if (CONNECTMON)
                times[2] = connectWatch.ElapsedMilliseconds;
#endif
                //moved to whithin catch clause
                //portError = true;
                //Debug.WriteLine("could not open port " + serialPort1.PortName);
            }
            if (this.IsHandleCreated)
            {
#if (CONNECTMON)
                times[3] = connectWatch.ElapsedMilliseconds;
#endif
                this.BeginInvoke(new SimpleDelegate(delegate
                {
                    buttonReconnectCradle.Text = "Reconnect";
                    buttonReconnectCradle.Enabled = true;
                }));
#if (CONNECTMON)
                times[4] = connectWatch.ElapsedMilliseconds;
                Debug.WriteLine("cradle connect/abort times: {0}, {1}, {2}, {3}, {4}", times[0], times[1] - times[0], times[2] - times[1], times[3] - times[2], times[4] - times[3]);
#endif
            }
            if (!cradleConnected)
            {
                //TODO finish cradle open timer
                BTTimer.Change(BTTimerInterval, BTTimerInterval);
            }
        }

        //threaded version for allowing timeout
        public void OpenPortExecution2()
        {
            try
            {
                serialPort2.Open();
                if (serialPort2.IsOpen)
                {
                    Debug.WriteLine("Cradle port opened.");
                    //TODO move to BT section
                    cradleConnected = true;
                }
                else
                {
                    Debug.WriteLine("Could not open port (no exception)");
                }
                //pingTimerT.Change(pingTimerInterval, pingTimerInterval);
            }
            //if can't open port
            catch (Exception ex)
            {
                portError2 = true;
                //MessageBox.Show("Could not open port " + serialPort1.PortName);
                Debug.WriteLine("Could not open port " + serialPort2.PortName + ": " + ex.Message);
                return;
            }
        }


        //method for closing a port must be run on UI thread!
        private void ClosePort()
        {
            buttonReconnect.Enabled = false;
            buttonReconnect.Text = "Closing";
            this.Icon = Properties.Resources.off;
            cubeConnected = false;
            portError = false;
            Thread newThread = new Thread(this.ClosePortExecution);
            newThread.Start();
            //timeout for the connection 'try'
            if (!newThread.Join(TimeSpan.FromSeconds(3)))
            {
                portError = true;
                Debug.WriteLine("could not close port " + serialPort1.PortName);
            }
            if (portError)
            {
                MessageBox.Show("Oh no! Error closing port!\n");
            }
        }


        //threaded version for allowing timeouts
        public void ClosePortExecution()
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    Debug.WriteLine("port closed");
                }
            }
            //if can't close port
            catch (Exception ex)
            {
                portError = true;
            }
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new SimpleDelegate(delegate
                {
                    buttonReconnect.Text = "Reconnect";
                    buttonReconnect.Enabled = true;
                }));
            }
        }


        //method for closing a port must be run on UI thread!
        private void ClosePort2()
        {
            buttonReconnectCradle.Enabled = false;
            buttonReconnectCradle.Text = "Closing";
            //this.Icon = Properties.Resources.off;
            cradleConnected = false;
            portError2 = false;
            Thread newThread = new Thread(this.ClosePortExecution2);
            newThread.Start();
            //timeout for the connection 'try'
            if (!newThread.Join(TimeSpan.FromSeconds(3)))
            {
                portError2 = true;
                Debug.WriteLine("could not close port " + serialPort1.PortName);
            }
            if (portError2)
            {
                MessageBox.Show("Oh no! Error closing port!\n");
            }
        }


        //threaded version for allowing timeouts
        public void ClosePortExecution2()
        {
            try
            {
                if (serialPort2.IsOpen)
                {
                    serialPort2.Close();
                    Debug.WriteLine("cradle port closed");
                }
            }
            //if can't close port
            catch (Exception ex)
            {
                portError2 = true;
            }
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new SimpleDelegate(delegate
                {
                    buttonReconnectCradle.Text = "Reconnect";
                    buttonReconnectCradle.Enabled = true;
                }));
            }
        }

        private void CubeSearcher(object myObject)
        {

        }
        //TODO: automatic Bluetooth stuff
#if (BT)
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
                Debug.WriteLine("Can't find bluetooth module! Please make sure Bluetooth module is connected and activated");
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
            cubeConnected = true;

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
#endif
        #endregion
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% Comm Protocol %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        #region Cube Comm Protocol
        //Method for reading from serial port and passing on to InvokedOnData (as handler on form-thread)
        private void SerialPort1DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (closeLock.TryEnterReadLock(0))
            {
                closeMutexOwner = "SerialPort1DataReceived";
                try
                {
                    if (serialPortDataMutex.WaitOne(0))
                    {
                        //TODO: only iterates once.  change to if?
                        while (serialPort1.BytesToRead > 0)
                        {

                            //Debug.WriteLine("buffer length {0}", serialPort1.BytesToRead);
                            //TODO only if port isn't closed!
                            byte[] buffer = new byte[serialPort1.BytesToRead];
                            serialPort1.Read(buffer, 0, buffer.Length);
                            //new run, using delegate.beginInvoke is bad!! causes mutex blocks and losss of sync on laptop.
                            //new DataProcessDelegate(Synchronizer).BeginInvoke(buffer, null, null);
                            //old run using control.beginInvoke
                            BeginInvoke(new DataProcessDelegate(Synchronizer), buffer);
                            serialPortDataMutex.ReleaseMutex();
                            return;
                        }

                        serialPortDataMutex.ReleaseMutex();
                    }
#if (DEBUGG)
                    else
                    {
                        Debug.WriteLine("serial port data mutex block {0}", dbgcounter);
                        dbgcounter++;
                    }
#endif
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error on serial port data received!" + ex.Message);
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
                closeMutexOwner = "Synchronizer";
                try
                {
                    if (Monitor.TryEnter(synchronizerLock, 0))
                    {
                        try
                        {
                            foreach (byte b in buffer)
                            {
                                int ch = b;
                                if (!packetStarted && ch != '$' && ch != '&') //$ is old 14 byte protocol, & is for new protocol
                                {
                                    //TODO: after long (50 sec) debug break in Stable() (after clicking calibrate) gets deadlocked on this
#if (SYNCMON)
                                    Debug.Write((char)ch);
#endif
                                    noSync = true;
                                    noFullSync = true;

                                    continue;  // initial synchronization - also used to resync/realign if needed
                                }

                                if (noSync)
                                {
#if (SYNCMON)
                                    Debug.WriteLine("Sync Started..");
#endif
                                    /*old code: (if brought back, nosync must be more than a debug var!!)
                                    //if regained sync after calibration, should wait for stabilization, adjust heading, and change view.
                                    if (mpuCalibrating)
                                    {
                                        mpuCalibrating = false;
                                        mpuStable = false;
                                        makeCorrection = true;
                                        mpuStabilizeTimer.Start();
                                    }*/
                                    if (ch == '$')
                                    {
                                        oldProtocol = true;
                                    }
                                    else  if (ch == '&')
                                    {
                                        oldProtocol = false;
                                    }
                                    noSync = false;
                                }

                                packetStarted = true;
                                if (oldProtocol)
                                {
                                    if ((serialCount == 1 && ch != 2)
                                    || (serialCount == 12 && ch != '\r')
                                    || (serialCount == 13 && ch != '\n'))
                                    {
                                        serialCount = 0;
                                        packetStarted = false;

                                        noSync = true;
                                        noFullSync = true;

                                        continue;
                                    }

                                    //TODO: only needed as long as close sequence/reconnect may alter serialCount
                                    if (serialCount > 0 || ch == '$')
                                    {
                                        packet[serialCount++] = (char)ch;
                                        //congrats! we have a new packet. 
                                        if (serialCount == 14)
                                        {

                                            if (noFullSync)
                                            {
                                                if (this.IsHandleCreated)
                                                {
                                                    this.BeginInvoke(new SimpleDelegate(delegate
                                                    {
                                                        this.Icon = Properties.Resources.on;
                                                    }));
                                                }
#if (SYNCMON)
                                                Debug.WriteLine("Sync complete!");
#endif
                                                noFullSync = false;
                                            }

                                            //restart packet byte position
                                            serialCount = 0;
                                            //synced has to be false for serial count 0, so that messages can be displayed
                                            packetStarted = false;
                                            //try our best not to lose sync
                                            new SimpleDelegate(PacketAnalyzer).BeginInvoke(null, null);
                                        }
                                    }
                                }
                                //new protocol
                                else
                                {
                                    if ((serialCount == 1 && ch != 2)
                                    || (serialCount == 24 && ch != '\r')
                                    || (serialCount == 25 && ch != '\n'))
                                    {
                                        serialCount = 0;
                                        packetStarted = false;

                                        noSync = true;
                                        noFullSync = true;

                                        continue;
                                    }

                                    //TODO: only needed as long as close sequence/reconnect may alter serialCount
                                    if (serialCount > 0 || ch == '&')
                                    {
                                        packet[serialCount++] = (char)ch;
                                        //congrats! we have a new packet. 
                                        if (serialCount == 26)
                                        {

                                            if (noFullSync)
                                            {
                                                if (this.IsHandleCreated)
                                                {
                                                    this.BeginInvoke(new SimpleDelegate(delegate
                                                    {
                                                        this.Icon = Properties.Resources.on;
                                                    }));
                                                }
#if (SYNCMON)
                                                Debug.WriteLine("Sync complete!");
#endif
                                                noFullSync = false;
                                            }

                                            //restart packet byte position
                                            serialCount = 0;
                                            //synced has to be false for serial count 0, so that messages can be displayed
                                            packetStarted = false;
                                            //try our best not to lose sync
                                            new SimpleDelegate(PacketAnalyzer).BeginInvoke(null, null);
                                        }
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
                        Debug.WriteLine("synchronizer mutex block");
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
                closeMutexOwner = "PacketAnalyzer";
                try
                {
                    if (Monitor.TryEnter(packetAnalyzerLock, 0))
                    {
                        try
                        {
                            if (oldProtocol)
                            {
                                //needed for simple auto lock/unlock mechanism, as each packet is currently sent twice
                                if (packet[11] != lastPacketID)
                                {
                                    lastPacketID = packet[11];
                                }
                                else
                                {
                                    return;
                                }
                            }

                            // get quaternion from data packet
                            q[0] = ((packet[2] << 8) | packet[3]) / 16384.0f;
                            q[1] = ((packet[4] << 8) | packet[5]) / 16384.0f;
                            q[2] = ((packet[6] << 8) | packet[7]) / 16384.0f;
                            q[3] = ((packet[8] << 8) | packet[9]) / 16384.0f;
                            for (int i = 0; i < 4; i++) if (q[i] >= 2) q[i] = -4 + q[i];

                            // set our quaternion to new data
                            // adjusted to Inventor Coordinate System
                            oldQuat = quat;
                            quat = new Quaternion(q[0], q[2], -q[3], q[1]);
                            //this was before debugging
                            //quat = new Quaternion(q[0], -q[2], q[3], q[1]);
#if (QUATREADMON)
                            //checking quat update speed
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
                                Debug.WriteLine("quat reading times: {0} {1} {2} {3} {4} {5} {6} {7} {8}", (qrt[1] - qrt[0]) / 10, 
                                   (qrt[2] - qrt[1]) / 10, (qrt[3] - qrt[2]) / 10, (qrt[4] - qrt[3]) / 10, (qrt[5] - qrt[4]) / 10, (qrt[6] - qrt[5]) / 10, 
                                   (qrt[7] - qrt[6]) / 10, (qrt[8] - qrt[7]) / 10, (qrt[9] - qrt[8]) / 10);
                            }
#endif

                            double diffTheta = oldQuat.Angle - quat.Angle;
                            Vector3D diffVector = Vector3D.Subtract(oldQuat.Axis, quat.Axis);

                            if (!oldProtocol)
                            {
                                for (int i = 0; i < NUM_BUTTONS; i++)
                                {
                                    buttonClicks[i] = packet[12 + i];
                                    buttonReleases[i] = packet[18 + i];
                                }
                            }
                            

#if (DEBUGGER)
                            //activate frame timer if detected movement
                            if (diffTheta > MAX_THETA_DIFF_LOCK || diffVector.Length > MAX_AXIS_DIFF_LOCK)
                            {

                                if (!inventorFrameTimerTEnabled)
                                {
                                    if (inventorFrameTimerT.Change(inventorFrameInterval, inventorFrameInterval)) inventorFrameTimerTEnabled = true;
                                }
                            }
#endif
                        }
                        finally
                        {
                            Monitor.Exit(packetAnalyzerLock);
                        }
                    }
#if (DEBUGG)
                    else
                    {
                        Debug.WriteLine("packet analyzer mutex block");
                    }
#endif
                }
                finally
                {
                    closeLock.ExitReadLock();
                }
            }

        }


        private void SerialPort2DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (closeLock.TryEnterReadLock(0))
            {
                closeMutexOwner = "SerialPort2DataReceived";
                try
                {
                    if (serialPort2DataMutex.WaitOne(0))
                    {
                        //TODO: only iterates once.  change to if?
                        while (serialPort2.BytesToRead > 0)
                        {

                            //Debug.WriteLine("buffer length {0}", serialPort1.BytesToRead);
                            //TODO only if port isn't closed!
                            byte[] buffer = new byte[serialPort2.BytesToRead];
                            serialPort2.Read(buffer, 0, buffer.Length);
                            //new run, using delegate.beginInvoke is bad!! causes mutex blocks and losss of sync on laptop.
                            //new DataProcessDelegate(Synchronizer).BeginInvoke(buffer, null, null);
                            //old run using control.beginInvoke
                            BeginInvoke(new DataProcessDelegate(Synchronizer2), buffer);
                            serialPort2DataMutex.ReleaseMutex();
                            return;
                        }

                        serialPort2DataMutex.ReleaseMutex();
                    }
#if (DEBUGG)
                    else
                    {
                        Debug.WriteLine("serial port data mutex block {0}", dbgcounter);
                        dbgcounter++;
                    }
#endif
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error on serial port data received!" + ex.Message);
                }
                finally
                {
                    closeLock.ExitReadLock();
                }
            }
        }

        //form-thread method for parsing received data and then calling DisplayFromPort()
        private void Synchronizer2(byte[] buffer)
        {
            if (closeLock.TryEnterReadLock(0))
            {
                closeMutexOwner = "Synchronizer2";
                try
                {
                    if (Monitor.TryEnter(synchronizer2Lock, 0))
                    {
                        try
                        {
                            foreach (byte b in buffer)
                            {
                                int ch = b;
                                if (!packetStarted2 && ch != '&') //only new protocol!
                                {
                                    //TODO: after long (50 sec) debug break in Stable() (after clicking calibrate) gets deadlocked on this
#if (SYNCMON)
                                    Debug.Write((char)ch);
#endif
                                    noSync2 = true;
                                    noFullSync2 = true;

                                    continue;  // initial synchronization - also used to resync/realign if needed
                                }

                                if (noSync2)
                                {
#if (SYNCMON)
                                    Debug.WriteLine("Sync Started..");
#endif
                                    /*old code: (if brought back, nosync must be more than a debug var!!)
                                    //if regained sync after calibration, should wait for stabilization, adjust heading, and change view.
                                    if (mpuCalibrating)
                                    {
                                        mpuCalibrating = false;
                                        mpuStable = false;
                                        makeCorrection = true;
                                        mpuStabilizeTimer.Start();
                                    }*/
                                    noSync2 = false;
                                }

                                packetStarted2 = true;

                                if ((serialCount2 == 1 && ch != 2)
                                    || (serialCount2 == 24 && ch != '\r')
                                    || (serialCount2 == 25 && ch != '\n'))
                                {
                                    serialCount2 = 0;
                                    packetStarted2 = false;

                                    noSync2 = true;
                                    noFullSync2 = true;

                                    continue;
                                }

                                //TODO: only needed as long as close sequence/reconnect may alter serialCount
                                if (serialCount2 > 0 || ch == '&')
                                {
                                    packet2[serialCount2++] = (char)ch;
                                    //congrats! we have a new packet. 
                                    if (serialCount2 == 26)
                                    {

                                        if (noFullSync2)
                                        {
                                            if (this.IsHandleCreated)
                                            {
                                                this.BeginInvoke(new SimpleDelegate(delegate
                                                {
                                                    this.Icon = Properties.Resources.on;
                                                }));
                                            }
#if (SYNCMON)
                                            Debug.WriteLine("Cradle sync complete!");
#endif
                                            noFullSync2 = false;
                                        }

                                        //restart packet byte position
                                        serialCount2 = 0;
                                        //synced has to be false for serial count 0, so that messages can be displayed
                                        packetStarted2 = false;
                                        //try our best not to lose sync
                                        new SimpleDelegate(PacketAnalyzer2).BeginInvoke(null, null);
                                    }
                                }





                            }
                        }
                        finally
                        {
                            Monitor.Exit(synchronizer2Lock);
                        }
                    }
#if (DEBUGG)
                    else
                    {
                        Debug.WriteLine("synchronizer2 mutex block");
                    }
#endif
                }
                finally
                {
                    closeLock.ExitReadLock();
                }

            }

        }

        private void PacketAnalyzer2()
        {
            if (closeLock.TryEnterReadLock(0))
            {
                closeMutexOwner = "PacketAnalyzer2";
                try
                {
                    if (Monitor.TryEnter(packetAnalyzer2Lock, 0))
                    {
                        try
                        {
                            if (allInCradle)
                            {
                                // get quaternion from data packet
                                q[0] = ((packet[2] << 8) | packet[3]) / 16384.0f;
                                q[1] = ((packet[4] << 8) | packet[5]) / 16384.0f;
                                q[2] = ((packet[6] << 8) | packet[7]) / 16384.0f;
                                q[3] = ((packet[8] << 8) | packet[9]) / 16384.0f;
                                for (int i = 0; i < 4; i++) if (q[i] >= 2) q[i] = -4 + q[i];

                                // set our quaternion to new data
                                // adjusted to Inventor Coordinate System
                                oldQuat = quat;
                                quat = new Quaternion(q[0], q[2], -q[3], q[1]);
                                //this was before debugging
                                //quat = new Quaternion(q[0], -q[2], q[3], q[1]);
#if (QUATREADMON)
                            //checking quat update speed
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
                                Debug.WriteLine("quat reading times: {0} {1} {2} {3} {4} {5} {6} {7} {8}", (qrt[1] - qrt[0]) / 10, 
                                   (qrt[2] - qrt[1]) / 10, (qrt[3] - qrt[2]) / 10, (qrt[4] - qrt[3]) / 10, (qrt[5] - qrt[4]) / 10, (qrt[6] - qrt[5]) / 10, 
                                   (qrt[7] - qrt[6]) / 10, (qrt[8] - qrt[7]) / 10, (qrt[9] - qrt[8]) / 10);
                            }
#endif

                                double diffTheta = oldQuat.Angle - quat.Angle;
                                Vector3D diffVector = Vector3D.Subtract(oldQuat.Axis, quat.Axis);

                                for (int i = 0; i < NUM_BUTTONS; i++)
                                {
                                    buttonClicks[i] = packet[12 + i];
                                    buttonReleases[i] = packet[18 + i];
                                }



#if (DEBUGGER)
                            //activate frame timer if detected movement
                            if (diffTheta > MAX_THETA_DIFF_LOCK || diffVector.Length > MAX_AXIS_DIFF_LOCK)
                            {

                                if (!inventorFrameTimerTEnabled)
                                {
                                    if (inventorFrameTimerT.Change(inventorFrameInterval, inventorFrameInterval)) inventorFrameTimerTEnabled = true;
                                }
                            }
#endif
                            }
                        }
                        finally
                        {
                            Monitor.Exit(packetAnalyzer2Lock);
                        }
                    }
#if (DEBUGG)
                    else
                    {
                        Debug.WriteLine("packet analyzer mutex block");
                    }
#endif
                }
                finally
                {
                    closeLock.ExitReadLock();
                }
            }

        }
        #endregion
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%% Server interaction and Debug %%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        #region Server and Debug
        //method for getting the corrected current quat
        public Quaternion GetCorrectedQuat()
        {
            //tempQuat = R(C^-1)
            Quaternion tempQuat = Quaternion.Multiply(invCalQuat, quat);
            if (calAlgNum2)
            {
                //World view correction:
                //tempQuat = (C^-1)R
                tempQuat = Quaternion.Multiply(quat, invCalQuat);
                //tempQuat = W(C^-1)R
                tempQuat = Quaternion.Multiply(tempQuat, worldQuat);
                Quaternion invWorldQuat = new Quaternion(worldQuat.X, worldQuat.Y, worldQuat.Z, worldQuat.W);
                invWorldQuat.Invert();
                //tempQuat = W(C^-1)R(W^-1)
                tempQuat = Quaternion.Multiply(invWorldQuat, tempQuat);
            }
            return tempQuat;
        }


        //method for getting the corrected current quat as float array (for server)
        public float[] GetCorrectedQuatFloats()
        {
            Quaternion tempQuat = GetCorrectedQuat();
            return new float[4] { (float)tempQuat.X, (float)tempQuat.Y, (float)tempQuat.Z, (float)tempQuat.W };
        }

        //method for getting the panning speed as int array (for server)
        public float[] GetPanSpeed()
        {
            return panSpeed;
        }

        //method for getting the current zoom (for server)
        public int GetZoom()
        {
            return zoom;
        }

        //method for getting the current button clicks (for server)
        public int[] GetButtonClicks()
        {
            return buttonClicks;
        }

        //method for getting the current button releases (for server)
        public int[] GetButtonReleases()
        {
            return buttonReleases;
        }

        private void checkBoxMousePan_CheckedChanged(object sender, EventArgs e)
        {
            mousePan = !mousePan;
            if (mousePan)
            {
                if (mousePanStarted)
                {
                    mouseWatcherThread.Resume();
                }
                else
                {
                    mouseWatcherThread.Start();
                    mousePanStarted = true;
                }
            }
            else
            {
                mouseWatcherThread.Suspend();
            }
        }

        private void buttonTop_Click(object sender, EventArgs e)
        {
            buttonClicks[TOP]++;
            if (buttonClicks[TOP] == MAX_ClICK_COUNTER)
            {
                buttonClicks[TOP] = 0;
            }
            buttonReleases[TOP]++;
            if (buttonReleases[TOP] == MAX_ClICK_COUNTER)
            {
                buttonReleases[TOP] = 0;
            }
        }

        private void buttonBottom_Click(object sender, EventArgs e)
        {
            buttonClicks[BOTTOM]++;
            if (buttonClicks[BOTTOM] == MAX_ClICK_COUNTER)
            {
                buttonClicks[BOTTOM] = 0;
            }
            buttonReleases[BOTTOM]++;
            if (buttonReleases[BOTTOM] == MAX_ClICK_COUNTER)
            {
                buttonReleases[BOTTOM] = 0;
            }
        }

        private void buttonLeft_Click(object sender, EventArgs e)
        {
            buttonClicks[LEFT]++;
            if (buttonClicks[LEFT] == MAX_ClICK_COUNTER)
            {
                buttonClicks[LEFT] = 0;
            }
            buttonReleases[LEFT]++;
            if (buttonReleases[LEFT] == MAX_ClICK_COUNTER)
            {
                buttonReleases[LEFT] = 0;
            }
        }

        private void buttonRight_Click(object sender, EventArgs e)
        {
            buttonClicks[RIGHT]++;
            if (buttonClicks[RIGHT] == MAX_ClICK_COUNTER)
            {
                buttonClicks[RIGHT] = 0;
            }
            buttonReleases[RIGHT]++;
            if (buttonReleases[RIGHT] == MAX_ClICK_COUNTER)
            {
                buttonReleases[RIGHT] = 0;
            }
        }

        private void buttonFront_Click(object sender, EventArgs e)
        {
            buttonClicks[FRONT]++;
            if (buttonClicks[FRONT] == MAX_ClICK_COUNTER)
            {
                buttonClicks[FRONT] = 0;
            }
            buttonReleases[FRONT]++;
            if (buttonReleases[FRONT] == MAX_ClICK_COUNTER)
            {
                buttonReleases[FRONT] = 0;
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            buttonClicks[BACK]++;
            if (buttonClicks[BACK] == MAX_ClICK_COUNTER)
            {
                buttonClicks[BACK] = 0;
            }
            buttonReleases[BACK]++;
            if (buttonReleases[BACK] == MAX_ClICK_COUNTER)
            {
                buttonReleases[BACK] = 0;
            }
        }
        #endregion

        #region old debugger mode

#if (DEBUGGER)
        //method for updating the inventor cam view via the debugger
        private void InventorFrameDebug(object myObject)
        {
            debugger.Frame(quat, invCalQuat, worldQuat, camDist);            
        }
#endif
        //for debugger use - display according to pos and up
/*        public bool InvFrameDisplay(double[] camPos, double[] camUp)
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
                        //this doesn't necesarily throw exception when inventor is off - obsolete
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
#if (DEBUGG)
                Debug.WriteLine("inventor: {0} {1} {2} {3} {4} {5} total: {6}", times[0], times[1] - times[0], times[2] - times[1], times[3] - times[2], times[4] - times[3], times[5] - times[4], times[5]);
#endif
            }
#if (DEBUGG)
            else
            {
                Debug.WriteLine("inventor frame mutex block");
                return false;
            }
#endif
            return true;
        }*/


        //make a good filter. - obsolete (doesn't sit in server)
        //function that checks whether an actual movement of the cube was made
        public bool MovementFilter()
        {
            double diffTheta = lastLockedQuat.Angle - quat.Angle;
            Vector3D diffVector = Vector3D.Subtract(lastLockedQuat.Axis, quat.Axis);
            if (!(diffTheta > MAX_THETA_DIFF_UNLOCK || diffVector.Length > MAX_AXIS_DIFF_UNLOCK))
            {
                ////TODO: recalibrate to prevent stationary drift of cube over time
                //avoid jumping due to drifting
                lastLockedQuat = quat;
                return false;
            }
            return true;
        }

        public bool MPUStable()
        {
            return mpuStable;
        }
        #endregion
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% Helper Functions %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        #region Helper Functions
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
        public double[,] MatMultiply(double[,] a, double[,] b)
        {
            double[,] c = new double[3, 3];
            double currSum = 0;
            foreach (int i in range3)
            {
                foreach (int j in range3)
                {
                    foreach (int k in range3)
                    {
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
                foreach (int j in range3)
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
        #endregion
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% Program Flow %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        #region Program flow
        //a ping timer is started upon port opening.
        //a ping checks for activeness of inventor (shuts down otherwise)
        //and for serial port (shuts down itself and frame clock otherwise)
        //and also send a ping over serial.
        private void PingT(object myObject)
        {
#if (DEBUGGER)
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

            if (!inventorRunning)
            {
                this.BeginInvoke(new SimpleDelegate(delegate
                {
                    this.Close();
                }));         
            }
#endif

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
#if (DEBUGGER)
                    if (inventorFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite)) inventorFrameTimerTEnabled = false;
#endif
                    MessageBox.Show("Oh no! can't write to port!\n" + ex.ToString());

                }
            }
            else
            {
                pingTimerT.Change(Timeout.Infinite, Timeout.Infinite);
#if (DEBUGGER)
                if (inventorFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite)) inventorFrameTimerTEnabled = false;
#endif
            }
        }

        //Hopefully, it's enough that the DataRecieved uses BeginInvoke.
        //TODO: make sure there really isn't any deadlock by using Invoke for closing
        private void CloseHandler(object sender, FormClosingEventArgs e)
        {
            this.Invoke(new SimpleDelegate(CloseSequence));
        }

        private void CloseSequence()
        {
            //TODO: finish close mutex usage. (open/close ports)
            closeLock.EnterWriteLock();
            closeMutexOwner = "CloseSequence";
            closed = true;
#if (MOUSEPAN)
            if (mousePanStarted && !mousePan)
            {
                mouseWatcherThread.Resume();
            }
#endif            
            try
            {
                //timers were definitely already created at this stage
                pingTimerT.Change(Timeout.Infinite, Timeout.Infinite);
                BTTimer.Change(Timeout.Infinite, Timeout.Infinite);
#if (DEBUGGER)
                if (inventorFrameTimerT.Change(Timeout.Infinite, Timeout.Infinite)) inventorFrameTimerTEnabled = false;
#endif
#if (SERVER)
                server.stopServer();
#endif
                //must wait for close before open!
                //can take a bit of time, but seems to work eventually *most* of the time.
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    Debug.WriteLine("port closed");
                }

                packetStarted = false;
                serialCount = 0;
                //TODO when should confing be written?
                using (StreamWriter sw = System.IO.File.CreateText(path))
                {
                    sw.WriteLine(serialComPort);
                }
            }
            finally
            {
                closeLock.ExitWriteLock();
            }
        }

        private void comboBoxPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialComPort = comboBoxPorts.Text;
        }

        private void comboBoxPorts_DropDown(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBoxPorts.Items.Clear();
            foreach (string port in ports)
            {
                comboBoxPorts.Items.Add(port);
            }
        }

        private void comboBoxPortsCradle_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialComPortCradle = comboBoxPortsCradle.Text;
        }

        private void comboBoxPortsCradle_DropDown(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBoxPortsCradle.Items.Clear();
            foreach (string port in ports)
            {
                comboBoxPortsCradle.Items.Add(port);
            }
        }

        private void buttonReconnect_Click(object sender, EventArgs e)
        {
            buttonReconnect.Enabled = false;
            this.Icon = Properties.Resources.off;
            cubeConnected = false;
            this.BeginInvoke(new SimpleDelegate(delegate
            {
                //TODO: change to port close executioner
                try
                {
                    if (serialPort1.IsOpen)
                    {
                        //TODO: can get stuck here! if tryin to close after connected cube is turned off
                        buttonReconnect.Text = "Closing";
                        serialPort1.Close();
                        Debug.WriteLine("port closed properly");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Oh no! can't close port!\n" + ex.ToString());
                }
                packetStarted = false;
                serialCount = 0;
                OpenPort();
            }));
        }

        private void buttonReconnectCradle_Click(object sender, EventArgs e)
        {
            buttonReconnectCradle.Enabled = false;
            //this.Icon = Properties.Resources.off;
            cradleConnected = false;
            this.BeginInvoke(new SimpleDelegate(delegate
            {
                //TODO: change to port close executioner
                try
                {
                    if (serialPort2.IsOpen)
                    {
                        //TODO: can get stuck here! if tryin to close after connected cube is turned off
                        buttonReconnectCradle.Text = "Closing";
                        serialPort2.Close();
                        Debug.WriteLine("cradle port closed properly");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Oh no! can't close port!\n" + ex.ToString());
                }
                packetStarted2 = false;
                serialCount2 = 0;
                OpenCradlePort();
            }));
        }

        private void DisableReconnect()
        {
            buttonReconnect.Enabled = false;
        }

        private void EnableReconnect()
        {
            buttonReconnect.Enabled = true;
        }

        private void MouseWatcher()
        {
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            while (true && !closed)
            {
                if ((X != Cursor.Position.X) || (Y != Cursor.Position.Y))
                {
                    this.MouseMove(Cursor.Position.X - X, Cursor.Position.Y - Y);
                    X = Cursor.Position.X;
                    Y = Cursor.Position.Y;
                }
                Thread.Sleep(1);

            }
        }

        private void MouseMove(int xChange, int yChange)
        {
            //Debug.WriteLine("moved: {0}, {1}", xChange, yChange);
            //Debug.WriteLine("speed: {0}, {1}", panSpeed[0], panSpeed[1]);
            panSpeed[0] += xChange;
            panSpeed[1] += yChange;
            float panNorm = (float)Math.Sqrt(panSpeed[0] * panSpeed[0] + panSpeed[1] * panSpeed[1]);
            if (panNorm > 45)
            {
                panSpeed[0] *= 45 / panNorm;
                panSpeed[1] *= 45 / panNorm;
            }

        }

        private void MouseClickAction(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine("reset! {0}, {1}", panSpeed[0], panSpeed[1]);
            panSpeed[0] = 0;
            panSpeed[1] = 0;
            panSpeed[2] = 0;
        }

        #endregion
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%        
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%% Calibration %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        #region Calibration
        private void buttonCalibrate_Click(object sender, EventArgs e)
        {
            new SimpleDelegate(Calibrate).BeginInvoke(null, null);
        }

        private void buttonCalReset_Click(object sender, EventArgs e)
        {
            unInvertedQuat = new Quaternion();
            invCalQuat = new Quaternion();
#if (DEBUGGER)
            new TimerDelegate(InventorFrameT).BeginInvoke(null, null, null);
            debugger.BeginInvoke(new SimpleDelegate(debugger.UpdateDisplayCal));
#endif
        }

        private void Calibrate()
        {
            if (!serialPort1.IsOpen)
            {
                return;
            }
            unInvertedQuat = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
            invCalQuat = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
            invCalQuat.Invert();
#if (DEBUGGER)
            new TimerDelegate(InventorFrameT).BeginInvoke(null, null, null);
            debugger.BeginInvoke(new SimpleDelegate(debugger.UpdateDisplayCal));
#endif
        }

        public void SetInvQuat(Vector3D tempAxis, double tempTheta)
        {
            try
            {
                invCalQuat = new Quaternion(tempAxis, tempTheta);
                unInvertedQuat = new Quaternion(tempAxis, tempTheta);
                unInvertedQuat.Invert();
            }
            catch (InvalidOperationException ex)
            {
                invCalQuat = new Quaternion();
                unInvertedQuat = new Quaternion();
                unInvertedQuat.Invert();
            }
        }

        public void SetCalQuat(Vector3D tempAxis, double tempTheta)
        {
            try
            {
                invCalQuat = new Quaternion(tempAxis, tempTheta);
                invCalQuat.Invert();
                unInvertedQuat = new Quaternion(tempAxis, tempTheta);
            }
            catch (InvalidOperationException ex)
            {
                invCalQuat = new Quaternion();
                invCalQuat.Invert();
                unInvertedQuat = new Quaternion();
            }
        }

        public Quaternion GetInvQuat()
        {
            return invCalQuat;
        }

        public Quaternion GetCalQuat()
        {
            return unInvertedQuat;
        }

        //TODO: make world calibration work
        private void WorldCalibrate()
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
            worldQuat = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
            mpuStable = true;
#if (DEBUGGER)
            new TimerDelegate(InventorFrameT).BeginInvoke(null, null, null);
            debugger.BeginInvoke(new SimpleDelegate(debugger.UpdateDisplayCal));
#endif
        }

        public Quaternion GetWorldQuat()
        {
            return worldQuat;
        }

        private void buttonSetVirWorld_Click(object sender, EventArgs e)
        {
            new SimpleDelegate(WorldCalibrate).BeginInvoke(null, null);
        }

        private void buttonResetVirWorld_Click(object sender, EventArgs e)
        {
            worldQuat = new Quaternion();
        }

        private void checkBoxCalNum2_CheckedChanged(object sender, EventArgs e)
        {
            calAlgNum2 = !calAlgNum2;
        }

        private void buttonStopServer_Click(object sender, EventArgs e)
        {
            server.stopServer();
            serverStarted = false;
        }

        private void buttonStartServer_Click(object sender, EventArgs e)
        {
            server = new Server(this);
            serverStarted = true;
        }

        private void checkBoxAllInOne_CheckedChanged(object sender, EventArgs e)
        {
            allInCradle = !allInCradle;
        }


        #endregion
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%        
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% Obsolete %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        #region Obsolete
#if (DEBUGGER)
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

        //method for updating the inventor cam view
        private void InventorFrameT(object myObject)
        {
            //no update over "noise", no update during calibration
            if (!MovementFilter() || !mpuStable)
            {
                return;
            }
            lastLockedQuat = quat;
            
            Quaternion tempQuat = GetCorrectedQuat();
            Vector3D a = tempQuat.Axis;
            double theta = tempQuat.Angle;
            theta *= Math.PI / 180;
            //move object instead of the camera
            theta = -theta;

            double[] camPos = RotateQuaternion(0, 0, -camDist, a, theta);
            double[] camUp = RotateQuaternion(0, 1, 0, a, theta);
            
            InvFrameDisplay(a, theta, camUp);
        }

        //displaying an inventor frame (non debugger).
        public bool InvFrameDisplay(Vector3D a, double theta, double[] camUp)
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
                                camDist = Math.Sqrt(camVector[0] * camVector[0] + camVector[1] * camVector[1] + camVector[2] * camVector[2]);
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
#if (DEBUGG)
                Debug.WriteLine("inventor: {0} {1} {2} {3} {4} {5} {6} {7} total: {8}", times[0], times[1] - times[0], times[2] - times[1], times[3] - times[2], times[4] - times[3], times[5] - times[4], times[6] - times[5], times[7] - times[6], times[7]);
#endif
            }
#if (DEBUGG)
            else
            {
                Debug.WriteLine("inventor frame mutex block");
                return false;
            }
#endif
            return true;
        }
#endif

        #endregion

    }

}

//TODO mutiple Cubes, Cube sleep, Autodetect.

