using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Threading;
using System.Diagnostics;

namespace MiniCube
{
    public partial class DebugForm : Form
    {
        CubeForm cube;
        private bool go;
        private Semaphore goSemaphore = new Semaphore(0, 1);
        private bool breakDisplay = false;
        private bool userInput = false;
        private String displayText = "";
        private Quaternion userQuat = new Quaternion();
        public delegate void QuatDelegate(Quaternion passedQuat);
        public delegate void CamDelegate(double[] camPos, double[] camUp);
        public delegate void SimpleDelegate();
        static Mutex frameMutex = new Mutex();
        private bool atomicCalc = false;
        private int[] atomicCalcArray = new int[] {2, 5};
        private bool unsupportedCalc = false;
        private int[] unsupportedCalcArray = new int[] {4};
        int rotationSelect = 0;


        public DebugForm(CubeForm cubeF)
        {
            InitializeComponent();
            cube = cubeF;
            rotationSelect = Int32.Parse(comboBox1.Text);
            if (atomicCalcArray.Contains(Int32.Parse(comboBox1.Text)))
            {
                atomicCalc = true;
            }
            else
            {
                atomicCalc = false;
            }
            UpdateDisplayCal();
            this.Show();
        }


        public void Frame(Quaternion quat, Quaternion invertedQuat, Quaternion worldQuat, double camDist)
        {
            if (frameMutex.WaitOne(0))
            {
                //will this work???
                //quat = new Quaternion(quat.X, -quat.Y, -quat.Z, quat.W);
                if (userInput)
                {
                    quat = userQuat;
                }
                
                //TODO can throw exceptions
                this.BeginInvoke(new QuatDelegate(UpdateDisplayCurrQuat), (quat));

                double[,] currRotation = cube.QuatToRotation(quat);
                double[,] invCalRotation = cube.QuatToRotation(invertedQuat);

                double[] tempAxis;

                Quaternion tempQuat = new Quaternion();
                Vector3D a = tempQuat.Axis;
                double theta = tempQuat.Angle;
                theta *= Math.PI / 180;

                //-theta so we move object instead of the camera
                theta = -theta;
                double[] camPos = cube.RotateQuaternion(0, 0, -camDist, a, theta);
                double[] camUp = cube.RotateQuaternion(0, 1, 0, a, theta);

                switch (rotationSelect)
                {
                    //quat = (C^-1)(RC^-1)C = (C^-1)R
                    case 1:
                        if (breakDisplay)
                        {
                            displayText = "Current Quat";
                            ShowQuat(quat, camDist);
                            goSemaphore.WaitOne(0);
                            goSemaphore.WaitOne();
                        }
                        if (breakDisplay)
                        {
                            displayText = "Inv Cal Quat";
                            ShowQuat(invertedQuat, camDist);
                            goSemaphore.WaitOne(0);
                            goSemaphore.WaitOne();
                        }
                        //seems like this give the REAL WORLD (according to the sensor. not it's relative change, but not the virtual either)change from calibration
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        if (breakDisplay)
                        {
                            displayText = "tempQuat = InvCalQuat*Quat";
                            ShowQuat(tempQuat, camDist);
                            goSemaphore.WaitOne(0);
                            goSemaphore.WaitOne();
                        }
                        a = tempQuat.Axis;
                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;
                        camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = cube.RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    //quat = (C^-1)R
                    //should be like 1
                    case 2:
                        double[,] relativeRotation2 = cube.MatMultiply(invCalRotation, currRotation);
                        camPos = cube.MatVectMultiply(relativeRotation2, new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(relativeRotation2, new double[3] { 0, 1, 0 });
                        break;
                    //quat = C(C^-1(R))(C^-1)=R^C^-1
                    case 3:
                        if (breakDisplay)
                        {
                            displayText = "Current Quat";
                            ShowQuat(quat, camDist);
                            goSemaphore.WaitOne(0);
                            goSemaphore.WaitOne();
                        }
                        if (breakDisplay)
                        {
                            displayText = "Inv Cal Quat";
                            ShowQuat(invertedQuat, camDist);
                            goSemaphore.WaitOne(0);
                            goSemaphore.WaitOne();
                        }
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        if (breakDisplay)
                        {
                            displayText = "tempQuat = InvCalQuat*Quat";
                            ShowQuat(tempQuat, camDist);
                            goSemaphore.WaitOne(0);
                            goSemaphore.WaitOne();
                        }
                        a = tempQuat.Axis;
                        tempAxis = cube.RotateQuaternion(a.X, a.Y, a.Z, invertedQuat.Axis,
                                                       invertedQuat.Angle * Math.PI / 180);
                        a.X = tempAxis[0];
                        a.Y = tempAxis[1];
                        a.Z = tempAxis[2];
                        theta = tempQuat.Angle;
                        if (breakDisplay)
                        {
                            displayText = "tempQuat is routated about invertedQuat";
                            tempQuat = new Quaternion(a, theta);
                            ShowQuat(tempQuat, camDist);
                            goSemaphore.WaitOne(0);
                            goSemaphore.WaitOne();
                        }
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;
                        camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = cube.RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    //quat=R^C^-1
                    //should be like 3
                    case 4:
                        tempQuat = Quaternion.Multiply(invertedQuat, quat);
                        a = tempQuat.Axis;
                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;
                        camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = cube.RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    //quat = RC^-1
                    //should be like 3
                    case 5:
                        double[,] relativeRotation = cube.MatMultiply(currRotation, invCalRotation);
                        camPos = cube.MatVectMultiply(relativeRotation, new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(relativeRotation, new double[3] { 0, 1, 0 });
                        break;
                    //should be: (W)(C^-1)(RC^-1)(C)(W^-1) = W(C^-1)R(W^-1)
                    case 6:
                        //tempQuat = (C^-1)R
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        //tempQuat = W(C^-1)R
                        tempQuat = Quaternion.Multiply(tempQuat, worldQuat);
                        Quaternion invWorldQuat = new Quaternion(worldQuat.X, worldQuat.Y, worldQuat.Z, worldQuat.W);
                        invWorldQuat.Invert();
                        //tempQuat = W(C^-1)R(W^-1)
                        tempQuat = Quaternion.Multiply(invWorldQuat,tempQuat);
                        a = tempQuat.Axis;
                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;
                        camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = cube.RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    default:
                        break;
                }
                this.BeginInvoke(new CamDelegate(UpdateDisplayCam), camPos, camUp);
                cube.ShowInvFrame(camPos, camUp);
                if (breakDisplay)
                {
                    if (atomicCalc)
                    {
                        displayText = "Atomic operation: Final Rotation";
                    }
                    else
                    {
                        displayText = "Final Rotation";
                    }
                    this.BeginInvoke(new SimpleDelegate(UpdateDisplayLabel));
                    goSemaphore.WaitOne(0);
                    goSemaphore.WaitOne();
                }

                frameMutex.ReleaseMutex();                
            } 
            else
            {
                Debug.WriteLine("Inventor Frame Mutex Block!");
            }       
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rotationSelect = Int32.Parse(comboBox1.Text);
        }


        private void goButton_Click(object sender, EventArgs e)
        {
            //TODO prevent exceptions due to releasing too many semaphores, and take care of semaphore counting
            go = true;
            goSemaphore.WaitOne(0);
            goSemaphore.Release();
        }


        private void breakDisplayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            breakDisplay = !breakDisplay;
        }


        private void buttonSetInvCalQuat_Click(object sender, EventArgs e)
        {
            double x = 0;
            double y = 0;
            double z = 0;
            double theta = 0;
            try
            {
                x = Double.Parse(textBoxInvCalX.Text);
                y = Double.Parse(textBoxInvCalY.Text);
                z = Double.Parse(textBoxInvCalZ.Text);
                theta = Double.Parse(textBoxInvCalTheta.Text);
            }
            catch (FormatException ex) {}
            Vector3D tempAxis = new Vector3D(x, y, z);
            cube.SetInvQuat(tempAxis, theta);
            UpdateDisplayCal();
        }


        private void buttonSetCalQuat_Click(object sender, EventArgs e)
        {
            double x = 0;
            double y = 0;
            double z = 0;
            double theta = 0;
            try
            {
                x = Double.Parse(textBoxCalX.Text);
                y = Double.Parse(textBoxCalY.Text);
                z = Double.Parse(textBoxCalZ.Text);
                theta = Double.Parse(textBoxCalTheta.Text);
            }
            catch (FormatException ex) { }
            Vector3D tempAxis = new Vector3D(x, y, z);
            cube.SetCalQuat(tempAxis, theta);
            UpdateDisplayCal();
        }

        private void ShowQuat(Quaternion tempQuat, double camDist)
        {
            this.BeginInvoke(new QuatDelegate(UpdateDisplayTempQuat), tempQuat);
            Vector3D a = tempQuat.Axis;
            double theta = tempQuat.Angle;
            theta *= Math.PI / 180;
            //move object instead of the camera
            theta = -theta;
            double[] camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
            double[] camUp = cube.RotateQuaternion(0, 1, 0, a, theta);
            this.BeginInvoke(new CamDelegate(UpdateDisplayCam), camPos, camUp);
            cube.ShowInvFrame(camPos, camUp);
            this.BeginInvoke(new SimpleDelegate(UpdateDisplayLabel));
        }


        public void UpdateDisplayCal()
        {
            Quaternion temp = cube.GetInvQuat();
            textBoxInvCalX.Text = Convert.ToString(temp.Axis.X);
            textBoxInvCalY.Text = Convert.ToString(temp.Axis.Y);
            textBoxInvCalZ.Text = Convert.ToString(temp.Axis.Z);
            textBoxInvCalTheta.Text = Convert.ToString(temp.Angle);

            temp = cube.GetCalQuat();
            textBoxCalX.Text = Convert.ToString(temp.Axis.X);
            textBoxCalY.Text = Convert.ToString(temp.Axis.Y);
            textBoxCalZ.Text = Convert.ToString(temp.Axis.Z);
            textBoxCalTheta.Text = Convert.ToString(temp.Angle);
        }

        public void UpdateDisplayCurrQuat(Quaternion tempQuat)
        {
            textBoxCurrX.Text = Convert.ToString(tempQuat.Axis.X);
            textBoxCurrY.Text = Convert.ToString(tempQuat.Axis.Y);
            textBoxCurrZ.Text = Convert.ToString(tempQuat.Axis.Z);
            textBoxCurrTheta.Text = Convert.ToString(tempQuat.Angle);
        }

        public void UpdateDisplayCam(double[] camPos, double[] camUp)
        {
            textBoxCamPosX.Text = Convert.ToString(camPos[0]);
            textBoxCamPosY.Text = Convert.ToString(camPos[1]);
            textBoxCamPosZ.Text = Convert.ToString(camPos[2]);
            textBoxCamUpX.Text = Convert.ToString(camUp[0]);
            textBoxCamUpY.Text = Convert.ToString(camUp[1]);
            textBoxCamUpZ.Text = Convert.ToString(camUp[2]);
        }

        public void UpdateDisplayTempQuat(Quaternion tempQuat)
        {
            textBoxDisplayX.Text = Convert.ToString(tempQuat.Axis.X);
            textBoxDisplayY.Text = Convert.ToString(tempQuat.Axis.Y);
            textBoxDisplayZ.Text = Convert.ToString(tempQuat.Axis.Z);
            textBoxDisplayTheta.Text = Convert.ToString(tempQuat.Angle);
        }

        private void UpdateDisplayLabel()
        {
            displayTextLabel.Text = displayText;
        }

        private void userInputCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            userInput = !userInput;
            buttonSetCurrQuat.Enabled = userInput;
        }

        private void buttonSetCurrQuat_Click(object sender, EventArgs e)
        {
            double x = 0;
            double y = 0;
            double z = 0;
            double theta = 0;
            try
            {
                x = Double.Parse(textBoxCurrX.Text);
                y = Double.Parse(textBoxCurrY.Text);
                z = Double.Parse(textBoxCurrZ.Text);
                theta = Double.Parse(textBoxCurrTheta.Text);
            }
            catch (FormatException ex) { }
            Vector3D tempAxis = new Vector3D(x, y, z);
            try
            {
                userQuat = new Quaternion(tempAxis, theta);
            }
            catch (InvalidOperationException ex) { }            
            UpdateDisplayCurrQuat(userQuat);
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            rotationSelect = Int32.Parse(comboBox1.Text);
            if (atomicCalcArray.Contains(Int32.Parse(comboBox1.Text)))
            {
                atomicCalc = true;
            }
            else
            {
                atomicCalc = false;
            }
        }
    }
}
