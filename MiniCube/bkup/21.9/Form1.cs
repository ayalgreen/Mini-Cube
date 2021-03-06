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

using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;
using Inventor;
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
        int BAUD_RATE = 38400;
        //int BAUD_RATE = 9600;
        //TODO auto serial find
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
        public delegate void DataProcessDelegate(byte[] buffer);
        public delegate void CameraLockDelegate(Vector3D a, double theta);

        //inventor vars
        Inventor.Application _invApp;
        bool _startedByForm = false;
        bool inventorRunning = false;
        System.Windows.Forms.Timer inventorFrameTimer;
        //TODO decide what to do with dist
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
        System.Windows.Forms.Timer pingTimer = new System.Windows.Forms.Timer();
        //TODO: This.. 
        System.Windows.Forms.Timer noDataRecivedTimer = new System.Windows.Forms.Timer();
        char[] pingBuff = { 'r' };
        char[] calBuff = { 'c', 'f', 'c', 'f', 'c', 'f', 'c', 'f' };
        char lastPacketID = (char)0;

        //static vars
        Quaternion quat = new Quaternion(1, 0, 0, 0);
        Quaternion[] oldQuats ={ new Quaternion(1, 0, 0, 0) };
        Quaternion lastLockedQuat = new Quaternion(1, 0, 0, 0);
        Quaternion correctionQuat = new Quaternion(-1, 0, 0, 0); //Vect= (-1, 0, 0),  Angle= 180
        double[] q = new Double[4];
        double[] gravity = new Double[3];
        double[] euler = new Double[3];
        double[] ypr = new Double[3];
        bool formClose = false;
        bool mpuStable = true;
        bool mpuCalibrating = false;
        bool makeCorrection = false;
        bool foundCube = false;
        System.Windows.Forms.Timer mpuStabilizeTimer;
        string path = @"./Cubecnfg";


        public CubeForm()
        {
            InitializeComponent();
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(CloseHandler);
            StartInventor();

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxPorts.Items.Add(port);
            }
            loadConfig();
            
            SetTimers();
           
            OpenPort();

            //OpenBluetooth();
        }

        private void SetTimers()
        {
            inventorFrameTimer = new System.Windows.Forms.Timer();
            inventorFrameTimer.Tick += new EventHandler(InventorFrame);
            inventorFrameTimer.Interval = (int)(1000 / FPS);

            mpuStabilizeTimer = new System.Windows.Forms.Timer();
            mpuStabilizeTimer.Tick += new EventHandler(Stable);
            mpuStabilizeTimer.Interval = 3000;

            pingTimer = new System.Windows.Forms.Timer();
            pingTimer.Tick += new EventHandler(Ping);
            pingTimer.Interval = 2000;
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
                    _startedByForm = true;
                    inventorRunning = true;

                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.ToString());
                    MessageBox.Show("Unable to get or start Inventor");
                }
            }
        }

        private void OpenPort()
        {
            serialPort1 = new SerialPort();
            serialPort1.PortName = serialComPort;
            serialPort1.BaudRate = BAUD_RATE;
            serialPort1.ParityReplace = (byte)0;
            serialPort1.DtrEnable = true;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(SerialPort1DataReceived);
            foreach (string port in SerialPort.GetPortNames())
            {
                if (port == serialPort1.PortName)
                {
                    serialPort1.Open();
                    pingTimer.Start();
                    return;
                }
            }
           
        }

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
                            //TODO: handle multiple Cubes
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

            //TODO using??
            Stream stream = client.GetStream();
            stream.ReadTimeout = 1000;
            while (true)
            {
                byte[] buffer = new byte[1024];
                stream.Read(buffer, 0, buffer.Length);
                if (formClose)
                {
                    break;
                }
                //TODO:need to make sure Handle was created
                BeginInvoke(new DataProcessDelegate(Synchronizer), buffer);
                //return;
            }
        }


        //Method for reading from serial port and passing on to InvokedOnData (as handler on form-thread)
        private void SerialPort1DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            while (serialPort1.BytesToRead > 0)
            {
                byte[] buffer = new byte[serialPort1.BytesToRead];
                serialPort1.Read(buffer, 0, buffer.Length);
                //avoid exception in case form had already closed
                if (formClose)
                {
                    return;
                }
                //TODO:need to make sure Handle was created
                BeginInvoke(new DataProcessDelegate(Synchronizer), buffer);     
                return;
            }
        }

        //form-thread method for parsing received data and then calling DisplayFromPort()
        private void Synchronizer(byte[] buffer)
        {
            foreach (byte b in buffer)
            {
                int ch = b;         
                if (!synced && ch != '$')
                {
                    //TODO: after long (50 sec) debug break in Stable() (after clicking calibrate) gets deadlocked on this
                    Console.Write((char)ch);////
                    noSync = true;////
                    continue;  // initial synchronization - also used to resync/realign if needed
                }
                if (noSync)////
                {
                    Console.WriteLine("Synced!");
                    //if regained sync after calibration, should wait for stabilization, adjust heading, and change view.
                    if (mpuCalibrating)
                    {
                        mpuCalibrating = false;
                        mpuStable = false;
                        makeCorrection = true;
                        mpuStabilizeTimer.Start();
                    }////
                }
                synced = true;
                noSync = false;////

                if ((serialCount == 1 && ch != 2)
                    || (serialCount == 12 && ch != '\r')
                    || (serialCount == 13 && ch != '\n'))
                {
                    serialCount = 0;
                    synced = false;         
                    continue; 
                }
                //is this "if" really needed?
                if (serialCount > 0 || ch == '$')
                {
                    teapotPacket[serialCount++] = (char)ch;
                    if (serialCount == 14)
                    {
                        //restart packet byte position
                        serialCount = 0;
                        //synced has to be false for serial count 0, so that messages can be displayed
                        synced = false;////
                        PacketAnalyzer();
                    }
                }
            }
        }

        private void PacketAnalyzer()
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
            oldQuats[0] = quat;
            quat = new Quaternion(q[0], -q[2], q[3], q[1]);

            int j = 0;
            double diffTheta = oldQuats[j].Angle - quat.Angle;
            Vector3D diffVector = Vector3D.Subtract(oldQuats[j].Axis, quat.Axis);
            
            if (diffTheta > MAX_THETA_DIFF_LOCK || diffVector.Length > MAX_AXIS_DIFF_LOCK)
            {
                if (!inventorFrameTimer.Enabled) inventorFrameTimer.Start();
            }
            //TODO: decide whats better.
            /*else
            {
                if (inventorFrameTimer.Enabled) inventorFrameTimer.Stop(); //move this within the FMS timer?
            }*/
        }
        
        //method for updating the inventor cam view
        private void InventorFrame(object myObject, EventArgs myEventArgs)//Vector3D a, Double theta)
        {
            double diffTheta = lastLockedQuat.Angle - quat.Angle;
            Vector3D diffVector = Vector3D.Subtract(lastLockedQuat.Axis, quat.Axis);
            //TODO: decide whats better.
            if (!(diffTheta > MAX_THETA_DIFF_UNLOCK || diffVector.Length > MAX_AXIS_DIFF_UNLOCK))
            {
                inventorFrameTimer.Stop();
                return;
            }
            if (!mpuStable)
            {
                return;
            }
            lastLockedQuat = quat;
            Quaternion tempQuat = Quaternion.Multiply(correctionQuat, quat);
            ////Vector3D a = quat.Axis;
            Vector3D a = tempQuat.Axis;
            double theta = tempQuat.Angle;
            theta *= Math.PI / 180;
            //makes the object to move instead of the camera
            theta = -theta;

            double[] camPos = RotateQuaternion(0, 0, -camDist, a, theta);
            double[] camUp = RotateQuaternion(0, 1, 0, a, theta);

            //avoid exceptions if possible
            //disregarding program close!
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

        
        private void Stable(object myObject, EventArgs myEventArgs)
        {
            mpuStabilizeTimer.Stop();
            mpuStable = true;
            if (makeCorrection)
            {
                makeCorrection = false;
                correctionQuat = new Quaternion(quat.Axis, -quat.Angle);
            }
            BeginInvoke(new EventHandler(InventorFrame));
        }

        //a ping timer is started upon port opening.
        //a ping checks for activeness of inventor (shuts down otherwise)
        //and for serial port (shuts down itself and frame clock otherwise)
        //and also send a ping over serial.
        private void Ping(object sender, EventArgs e)
        {
            try
            {
                _invApp = (Inventor.Application)Marshal.GetActiveObject("Inventor.Application");
            }
            catch (Exception ex)
            {
                inventorRunning = false;                
            }
            if (!inventorRunning)
            {
                this.Close();
            }
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(pingBuff, 0, 1);
            }
            else
            {
                pingTimer.Stop();
                inventorFrameTimer.Stop();
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
            pingTimer.Stop();
            inventorFrameTimer.Stop();
            serialPort1.Close();
            Console.WriteLine("port closed");
            synced = false;
            serialCount = 0;
            using (StreamWriter sw = System.IO.File.CreateText(path))
            {
                sw.WriteLine(serialComPort);
            }
    }


        private void buttonCalibrate_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(calBuff, 0, 8);
                mpuCalibrating = true;
            }
        }


        private void comboBoxPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialComPort = comboBoxPorts.Text;
        }

        private void buttonReconnect_Click(object sender, EventArgs e)
        {
            this.BeginInvoke(new EventHandler(delegate
            {
                serialPort1.Close();
                Console.WriteLine("port closed");
                synced = false;
                serialCount = 0;
                OpenPort();
            }));
        }
    }
}

