using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Media3D;


namespace MiniCube
{
    public partial class DebugForm : Form
    {
        CubeForm cube;
        private bool go;
        private bool breakDisplay = false;
        String displayText = "";

        public DebugForm(CubeForm cubeF)
        {
            InitializeComponent();
            cube = cubeF;
            cube.setRotationSelect(Int32.Parse(comboBox1.Text));
            this.Show();
        }

        public void Frame(Quaternion quat, Quaternion invertedQuat, Quaternion unInvertedQuat, double camDist)
        {
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
                    camPos = cube.MatVectMultiply(cube.MatInverse(relativeRotation), new double[3] { 0, 0, camDist });
                    camUp = cube.MatVectMultiply(cube.MatInverse(relativeRotation), new double[3] { 0, 1, 0 });

                    break;

                case 8:
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
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            cube.setRotationSelect(Int32.Parse(comboBox1.Text));
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            go = true;
        }

        private void breakDisplayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            breakDisplay = !breakDisplay;
        }
    }
}
