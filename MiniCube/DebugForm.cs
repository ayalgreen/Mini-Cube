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
        public delegate void SimpleDelegate();
        static Mutex frameMutex = new Mutex();


        public DebugForm(CubeForm cubeF)
        {
            InitializeComponent();
            cube = cubeF;
            cube.setRotationSelect(Int32.Parse(comboBox1.Text));
            this.Show();
        }


        public void Frame(Quaternion quat, Quaternion invertedQuat, Quaternion unInvertedQuat, double camDist)
        {
            if (frameMutex.WaitOne(0))
            {
                if (userInput)
                {
                    quat = userQuat;
                }
                if (breakDisplay)
                {
                    displayText = "Broken Display Mode!";
                    this.BeginInvoke(new QuatDelegate(UpdateDisplayQuat),(quat));
                    this.BeginInvoke(new SimpleDelegate(UpdateDisplayCal));
                    goSemaphore.Release();
                    goSemaphore.WaitOne();
                }

                double[,] currRotation = cube.QuatToRotation(quat);
                double[,] invCalRotation = cube.QuatToRotation(invertedQuat);
                double[,] calRotation = cube.QuatToRotation(unInvertedQuat);

                double[,] relativeRotation = cube.MatMultiply(currRotation, invCalRotation);
                double[,] relativeRotation2 = cube.MatMultiply(invCalRotation, currRotation);
                Quaternion tempQuat1 = Quaternion.Multiply(quat, invertedQuat);
                double[,] relativeRotation3 = cube.QuatToRotation(tempQuat1);
                double[,] finalRot1 = cube.MatMultiply(relativeRotation, calRotation);
                finalRot1 = cube.MatMultiply(invCalRotation, finalRot1);
                double[,] finalRot2 = cube.MatMultiply(invCalRotation, currRotation);
                Quaternion halfAngle = new Quaternion(unInvertedQuat.Axis, unInvertedQuat.Angle / 2);
                double[,] halfRotation = cube.QuatToRotation(halfAngle);
                Quaternion invHalfAngle = new Quaternion(unInvertedQuat.Axis, unInvertedQuat.Angle / 2);
                double[,] invHalfRotation = cube.QuatToRotation(invHalfAngle);
                invHalfAngle.Invert();
                double[,] finalRot3 = cube.QuatToRotation(Quaternion.Multiply(invertedQuat, quat));
                double[,] testRotation;

                double[] tempAxis;

                Quaternion tempQuat = new Quaternion();
                Vector3D a = tempQuat.Axis;
                double theta = tempQuat.Angle;
                theta *= Math.PI / 180;

                //-theta so we move object instead of the camera
                theta = -theta;
                double[] camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
                double[] camUp = cube.RotateQuaternion(0, 1, 0, a, theta);

                switch (cube.getRotationSelect())
                {
                    case 1:
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        a = tempQuat.Axis;

                        tempAxis = cube.RotateQuaternion(a.X, a.Y, a.Z, invertedQuat.Axis, invertedQuat.Angle * Math.PI / 180);
                        a.X = tempAxis[0];
                        a.Y = tempAxis[1];
                        a.Z = tempAxis[2];

                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;
                        camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = cube.RotateQuaternion(0, 1, 0, a, theta);

                        break;
                    case 2:
                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        a = tempQuat.Axis;

                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;
                        camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = cube.RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    case 3:

                        camPos = cube.MatVectMultiply(finalRot3, new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(finalRot3, new double[3] { 0, 1, 0 });

                        break;
                    case 4:
                        camPos = cube.MatVectMultiply(finalRot2, new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(finalRot2, new double[3] { 0, 1, 0 });

                        break;

                    case 5:

                        camPos = cube.MatVectMultiply(cube.MatInverse(finalRot3), new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(cube.MatInverse(finalRot3), new double[3] { 0, 1, 0 });

                        break;
                    case 6:
                        camPos = cube.MatVectMultiply(cube.MatInverse(finalRot2), new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(cube.MatInverse(finalRot2), new double[3] { 0, 1, 0 });

                        break;
                    case 7:
                        if (breakDisplay)
                        {
                            displayText = "Atomic calculation: Final result.";
                            goSemaphore.WaitOne();
                        }
                        camPos = cube.MatVectMultiply(cube.MatInverse(relativeRotation), new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(cube.MatInverse(relativeRotation), new double[3] { 0, 1, 0 });
                        break;

                    case 8:
                        if (breakDisplay)
                        {
                            displayText = "Atomic calculation: Final result.";
                            goSemaphore.WaitOne();
                        }
                        camPos = cube.MatVectMultiply(relativeRotation, new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(relativeRotation, new double[3] { 0, 1, 0 });

                        break;
                    case 9:
                        tempQuat = Quaternion.Multiply(invHalfAngle, quat);
                        tempQuat = Quaternion.Multiply(quat, halfAngle);

                        a = tempQuat.Axis;

                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;
                        camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = cube.RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    case 10:
                        tempQuat = Quaternion.Multiply(halfAngle, quat);
                        tempQuat = Quaternion.Multiply(quat, invHalfAngle);

                        a = tempQuat.Axis;

                        theta = tempQuat.Angle;
                        theta *= Math.PI / 180;
                        //move object instead of the camera
                        theta = -theta;
                        camPos = cube.RotateQuaternion(0, 0, camDist, a, theta);
                        camUp = cube.RotateQuaternion(0, 1, 0, a, theta);
                        break;
                    case 11:
                        testRotation = cube.MatMultiply(relativeRotation, invCalRotation);
                        testRotation = cube.MatMultiply(calRotation, testRotation);

                        camPos = cube.MatVectMultiply(testRotation, new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(testRotation, new double[3] { 0, 1, 0 });

                        break;
                    case 12:
                        testRotation = cube.MatMultiply(invCalRotation, relativeRotation);
                        camPos = cube.MatVectMultiply(testRotation, new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(testRotation, new double[3] { 0, 1, 0 });
                        break;
                    case 13:
                        testRotation = cube.MatMultiply(relativeRotation, invHalfRotation);
                        testRotation = cube.MatMultiply(halfRotation, testRotation);

                        camPos = cube.MatVectMultiply(testRotation, new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(testRotation, new double[3] { 0, 1, 0 });
                        break;
                    case 14:
                        testRotation = cube.MatMultiply(relativeRotation, halfRotation);
                        testRotation = cube.MatMultiply(invHalfRotation, testRotation);

                        camPos = cube.MatVectMultiply(testRotation, new double[3] { 0, 0, camDist });
                        camUp = cube.MatVectMultiply(testRotation, new double[3] { 0, 1, 0 });
                        break;
                    case 15:

                        tempQuat = Quaternion.Multiply(quat, invertedQuat);
                        a = tempQuat.Axis;
                        tempAxis = cube.RotateQuaternion(a.X, a.Y, a.Z, unInvertedQuat.Axis,
                                                       unInvertedQuat.Angle * Math.PI / 180);
                        a.X = tempAxis[0];
                        a.Y = tempAxis[1];
                        a.Z = tempAxis[2];

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
                cube.ShowInvFrame(camPos, camUp);

                frameMutex.ReleaseMutex();                
            }           
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            cube.setRotationSelect(Int32.Parse(comboBox1.Text));
        }


        private void goButton_Click(object sender, EventArgs e)
        {
            //TODO prevent exceptions due to releasing too many semaphores, and take care of semaphore counting
            go = true;
            displayTextLabel.Text = displayText;
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

        public void UpdateDisplayQuat(Quaternion tempQuat)
        {
            textBoxCurrX.Text = Convert.ToString(tempQuat.Axis.X);
            textBoxCurrY.Text = Convert.ToString(tempQuat.Axis.Y);
            textBoxCurrZ.Text = Convert.ToString(tempQuat.Axis.Z);
            textBoxCurrTheta.Text = Convert.ToString(tempQuat.Angle);
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
            UpdateDisplayQuat(userQuat);
        }
    }
}
