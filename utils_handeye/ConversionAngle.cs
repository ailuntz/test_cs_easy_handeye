/*
# -*- coding: utf-8 -*-
///
Calibrate the Camera with Zhang Zhengyou Method.
Picture File Folder: "./pic/RGB_camera_calib_img/", Without Distortion. 

By Ailuntz, 2023.07.04, Ailuntz@icloud.com
///
*/

using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using OpenCvSharp.Internal.Vectors;
using static HandEyeCalibration.Definetype;
using static HandEyeCalibration.ConversionMatrix;
using Quaternion = HandEyeCalibration.Definetype.Quaternion;

namespace HandEyeCalibration
{
    public partial class ConversionAngle
    {


    //将弧度值转化为旋转矩阵(需要确定旋转顺序，默认xyz)
    public static double[,] RotationMatrix(double rx, double ry, double rz)
    {
        double[,] rxMatrix = new double[3, 3] { { 1, 0, 0 }, { 0, Math.Cos(rx), -Math.Sin(rx) }, { 0, Math.Sin(rx), Math.Cos(rx) } };
        double[,] ryMatrix = new double[3, 3] { { Math.Cos(ry), 0, Math.Sin(ry) }, { 0, 1, 0 }, { -Math.Sin(ry), 0, Math.Cos(ry) } };
        double[,] rzMatrix = new double[3, 3] { { Math.Cos(rz), -Math.Sin(rz), 0 }, { Math.Sin(rz), Math.Cos(rz), 0 }, { 0, 0, 1 } };

        double[,] result = MultiplyMatrices(rzMatrix, ryMatrix);
        result = MultiplyMatrices(result, rxMatrix);

        return result;
    }
    

    //将欧拉角转换为四元数
    public static Quaternion EulerToQuaternion(double yaw , double pitch, double roll,string AngleType = "rad",string RotationType = "xyz")
    {
        //从欧拉角（roll、yaw、pitch）计算旋转矩阵，必须清楚姿态绕XYZ轴的旋转顺序，zyx，xyz,yzx。。。
        //若x,y,z 依次旋转，否则旋转矩阵R = Rx * Ry * Rz
        //这里采用xyz顺序
        if (AngleType == "deg")
        {
            yaw /= (180 / Math.PI);		//度转弧度
            pitch /= (180 / Math.PI);		//度转弧度
            roll /= (180 / Math.PI);		//度转弧度
        }
        else if (AngleType == "rad")
        {
            yaw = yaw;
            pitch = pitch;
            roll = roll;
        }
        else
        {
            Console.WriteLine("Type Error");
        }

        double cy = Math.Cos(yaw * 0.5);
        double sy = Math.Sin(yaw * 0.5);
        double cp = Math.Cos(pitch * 0.5);
        double sp = Math.Sin(pitch * 0.5);
        double cr = Math.Cos(roll* 0.5);
        double sr = Math.Sin(roll * 0.5);

        Quaternion q = new Quaternion();

        if (RotationType == "xyz")
        {
            //xyz方式
            q.w = cr * cp * cy - sr * sp * sy;//0.4829633
            q.x = cr * cp * sy + sr * sp * cy;//0.1294106
            q.y = cr * sp * cy - sr * cp * sy;//0.2241435
            q.z = cr * sp * sy + sr * cp * cy;//0.836516
        }else if (RotationType == "xzy")
        {
            //xzy方式
            q.w = cr * cp * cy + sr * sp * sy;//0.4281246
            q.x = cr * cp * sy - sr * sp * cy;//-0.2582827
            q.y = cr * sp * cy - sr * cp * sy;//0.2241435
            q.z = cr * sp * sy + sr * cp * cy;//0.836516
        }
        else if (RotationType == "yxz")
        {
            //yxz方式
            q.w = cr * cp * cy + sr * sp * sy;//0.4829633
            q.x = cr * cp * sy + sr * sp * cy;//0.1294106 sr * cp * cy - cr * sp * sy;//0.8658649
            q.y = cr * sp * cy - sr * cp * sy;//0.2241435 cr * sp * cy + sr * cp * sy;//-0.016656
            q.z = sr * cp * cy - cr * sp * sy;//0.8658649 cr * cp * sy - sr * sp * cy;//-0.2582827
        }
        else if (RotationType == "yzx")
        {
            //yzx方式
            q.w = cr * cp * cy - sr * sp * sy;//0.4829633
            q.x = cr * cp * sy + sr * sp * cy;//0.1294106
            q.y = cr * sp * cy + sr * cp * sy;//-0.016656sr * cp * cy + cr * sp * sy;//0.8365160
            q.z = sr * cp * cy - cr * sp * sy;//0.8658649cr * cp * sy + sr * sp * cy;//0.1294106
        }
        else if (RotationType == "zxy")
        {
            //zxy方式
            q.w = cr * cp * cy - sr * sp * sy;//0.4829633cr * cp * cy + sr * sp * sy;//0.4281246
            q.x = cr * cp * sy - sr * sp * cy;//-0.2582827cr * sp * cy + sr * cp * sy;//-0.016656
            q.y = cr * sp * cy + sr * cp * sy;//-0.016656cr * cp * sy - sr * sp * cy;
            q.z = sr * cp * cy + cr * sp * sy;//0.8365160sr * cp * cy - cr * sp * sy;
        }
        else if (RotationType == "zyx")
        {
            //zyx方式
            q.w = cr * cp * cy - sr * sp * sy;//0.4829633cr * cp * cy - sr * sp * sy;
            q.x = cr * cp * sy - sr * sp * cy;//-0.2582827cr * sp * cy + sr * cp * sy;
            q.y = cr * sp * cy + sr * cp * sy;//-0.016656cr * cp * sy + sr * sp * cy;
            q.z = sr * cp * cy - cr * sp * sy;//0.8658649sr * cp * cy - cr * sp * sy;
        }
        else
        {
            Console.WriteLine("RotationType Error");
        }

        return q;
    }

    //将旋转矩阵转换为旋转向量
    public static Mat RotationmatrixToEuler(Mat Rotationmatrix)
    {
        //eulerAngle /= (180 / CV_PI);		//度转弧度
        Mat Euler = new Mat(1, 3, MatType.CV_64FC1);
        Cv2.Rodrigues(Rotationmatrix, Euler);
        return Euler;
    }

    /// <summary>返回一个量值为x、符号为y的值。
    /// </summary>
    /// <param name="x">其大小在结果中使用的数字</param>
    /// <param name="y">其符号在结果中使用的数字</param>
    /// <returns>具有x的大小和y的符号的值</returns>
    public static double CopySign(double x, double y)
    {
        //Implements Math.CopySign from newer .NET versions
        if ((x > 0.0 && y > 0.0) || (x < 0.0 && y < 0.0))
            return x;

        return -x;
    }

    //将四元数转换为欧拉角
    public static EulerAngles QuaternionToEulerAngles(Quaternion q)
    {
        EulerAngles angles = new();
        //zyx方式
        // roll / x
        float sinr_cosp = (float)(2 * (q.w * q.x + q.y * q.z));
        float cosr_cosp = (float)(1 - 2 * (q.x * q.x + q.y * q.y));
        angles.rx = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // pitch / y
        float sinp = (float)(2 * (q.w * q.y - q.z * q.x));
        if (Math.Abs(sinp) >= 1)
        {
            angles.ry = (float)CopySign(Math.PI / 2, sinp);
        }
        else
        {
            angles.ry = (float)Math.Asin(sinp);
        }
        // yaw / z
        float siny_cosp = (float)(2 * (q.w * q.z + q.x * q.y));
        float cosy_cosp = (float)(1 - 2 * (q.y * q.y + q.z * q.z));
        angles.rz = (float)Math.Atan2(siny_cosp, cosy_cosp);

        return angles;
    }

   /// <summary>将四元数转换为旋转矩阵<para />
   /// <param name="a">归一化的四元数: q = q0 + q1 * i + q2 * j + q3 * k;<para /></param>
   /// <param name="b"><para /></param>
   /// <returns>返回3*3旋转向量</returns>
   /// </summary>
    public static double[,] QuaterniontoRotationVector(double w0, double q1, double q2, double q3)
    {
                double[,] matrix = new double[3, 3];
                matrix[0, 0] = (2 * (w0 * w0 + q1 * q1)) - 1;
                matrix[0, 1] = 2 * (q1 * q2 - w0 * q3);
                matrix[0, 2] = 2 * (q1 * q3 + w0 * q2);
                matrix[1, 0] = 2 * (q1 * q2 + w0 * q3);
                matrix[1, 1] = 2 * (w0 * w0 + q2 * q2) - 1;
                matrix[1, 2] = 2 * (q2 * q3 - w0 * q1);
                matrix[2, 0] = 2 * (q1 * q3 - w0 * q2);
                matrix[2, 1] = 2 * (q2 * q3 + w0 * q1);
                matrix[2, 2] = 2 * (w0 * w0 + q3 * q3) - 1;    
                return matrix;
    }

    //待测试
    /// <summary>将旋转矩阵转换为四元数<para />
    /// <param name="a">3*3旋转矩阵<para /></param>
    /// <param name="b"><para /></param>
    /// <returns>返回归一化的四元数: q = q0 + q1 * i + q2 * j + q3 * k;</returns>
    /// </summary>
    public static double[] RotationVectorToQuaternion(double[,] R)
    {
        double[] q = new double[4];
        double tr = R[0, 0] + R[1, 1] + R[2, 2];
        if (tr > 0)
        {
            double S = Math.Sqrt(tr + 1.0) * 2; // S=4*qw 
            q[0] = 0.25 * S;
            q[1] = (R[2, 1] - R[1, 2]) / S;
            q[2] = (R[0, 2] - R[2, 0]) / S;
            q[3] = (R[1, 0] - R[0, 1]) / S;
        }
        else if ((R[0, 0] > R[1, 1]) && (R[0, 0] > R[2, 2]))
        {
            double S = Math.Sqrt(1.0 + R[0, 0] - R[1, 1] - R[2, 2]) * 2; // S=4*qx 
            q[0] = (R[2, 1] - R[1, 2]) / S;
            q[1] = 0.25 * S;
            q[2] = (R[0, 1] + R[1, 0]) / S;
            q[3] = (R[0, 2] + R[2, 0]) / S;
        }
        else if (R[1, 1] > R[2, 2])
        {
            double S = Math.Sqrt(1.0 + R[1, 1] - R[0, 0] - R[2, 2]) * 2; // S=4*qy
            q[0] = (R[0, 2] - R[2, 0]) / S;
            q[1] = (R[0, 1] + R[1, 0]) / S;
            q[2] = 0.25 * S;
            q[3] = (R[1, 2] + R[2, 1]) / S;
        }
        else
        {
            double S = Math.Sqrt(1.0 + R[2, 2] - R[0, 0] - R[1, 1]) * 2; // S=4*qz
            q[0] = (R[1, 0] - R[0, 1]) / S;
            q[1] = (R[0, 2] + R[2, 0]) / S;
            q[2] = (R[1, 2] + R[2, 1]) / S;
            q[3] = 0.25 * S;
        }
        return q;
    }


    /// <summary>欧拉角转换为旋转矩阵<para />
    /// <param name="a">指定欧拉角的排列顺序；（机械臂的位姿类型有xyz,zyx,zyz几种，需要区分）<para /></param>
    /// <param name="b"><para />欧拉角（1*3矩阵）, 角度值</param>
    /// <returns>返回3*3旋转矩阵</returns>
    /// </summary>
    public static Mat EulerAngleToRotateMatrix(EulerAngles eulerAngle, string seq)
    {
        //从欧拉角（roll、yaw、pitch）计算旋转矩阵，必须清楚姿态绕XYZ轴的旋转顺序，zyx，xyz,yzx。。。
        //若x,y,z 依次旋转，否则旋转矩阵R = Rx * Ry * Rz
        //eulerAngle /= (180 / CV_PI);		//度转弧度
        double rx = eulerAngle.rx, ry = eulerAngle.ry, rz = eulerAngle.rz;
        double rxs = Math.Sin(rx), rxc = Math.Cos(rx);
        double rys = Math.Sin(ry), ryc =  Math.Cos(ry);
        double rzs = Math.Sin(rz), rzc =  Math.Cos(rz);

        //XYZ方向的旋转矩阵
        double[,] RotXarry = new double[3, 3]{ { 1, 0, 0 }, { 0, rxc, -rxs }, { 0, rxs, rxc } };
        Mat RotX = new Mat(3, 3, MatType.CV_64FC1, RotXarry);

        double[,] RotYarry = new double[3, 3]{ { ryc, 0, rys }, { 0, 1, 0 }, { -rys, 0, ryc } };
        Mat RotY = new Mat(3, 3, MatType.CV_64FC1, RotYarry);

        double[,] RotZarry = new double[3, 3]{ { rzc, -rzs, 0 }, { rzs, rzc, 0 }, { 0, 0, 1 } };
        Mat RotZ = new Mat(3, 3, MatType.CV_64FC1, RotZarry);
        //按顺序合成后的旋转矩阵
        Mat rotMat = new Mat();

        if (seq == "zyx") rotMat = RotX * RotY * RotZ;
        else if (seq == "yzx") rotMat = RotX * RotZ * RotY;
        else if (seq == "zxy") rotMat = RotY * RotX * RotZ;
        else if (seq == "yxz") rotMat = RotZ * RotX * RotY;
        else if (seq == "xyz") rotMat = RotZ * RotY * RotX;
        else if (seq == "xzy") rotMat = RotY * RotZ * RotX;
        else
        {
            Console.WriteLine("Euler Angle Sequence string is wrong...");
        }
        if (!IsRotatedMatrix(rotMat))  //欧拉角特定姿态进入死锁状态
        {
            Console.WriteLine("Euler Angle convert to RotatedMatrix failed...");
            Environment.Exit(0);
        }
        return rotMat;
    }


    /// <summary>旋转矩阵转换为欧拉角<para />
    /// <param name="a">指定欧拉角的排列顺序；（机械臂的位姿类型有xyz,zyx,zyz几种，需要区分）<para /></param>
    /// <param name="b"><para />旋转矩阵（3*3矩阵）</param>
    /// <returns>返回欧拉角（1*3矩阵）, 角度值</returns>
    /// </summary>
    public static EulerAngles RotateMatrixToEulerAngle(Mat rotMatSource, string seq)
    {
        //从旋转矩阵计算欧拉角（roll、yaw、pitch），必须清楚姿态绕XYZ轴的旋转顺序，zyx，xyz,yzx。。。
        //若x,y,z 依次旋转，否则旋转矩阵R = Rx * Ry * Rz
        //eulerAngle /= (180 / CV_PI);		//度转弧度
        Mat rotMat = rotMatSource.Inv();		//旋转矩阵的逆矩阵
        double rx = 0, ry = 0, rz = 0;
        double r11 = rotMat.At<double>(0, 0);
        double r12 = rotMat.At<double>(0, 1);
        double r13 = rotMat.At<double>(0, 2);
        double r21 = rotMat.At<double>(1, 0);
        double r22 = rotMat.At<double>(1, 1);
        double r23 = rotMat.At<double>(1, 2);
        double r31 = rotMat.At<double>(2, 0);
        double r32 = rotMat.At<double>(2, 1);
        double r33 = rotMat.At<double>(2, 2);

        //需要提前转换需要的数据
        bool invflag = false;
        if (seq == "yzx")
        {
            seq = "xyz";
        }else if (seq == "zxy")
        {
            seq = "yzx";
        }
        else if (seq == "yxz")
        {
            seq = "zxy";
            invflag = true;
             //输出要取反
        }else if (seq == "xyz")
        {
            seq = "xzy";
            invflag = true;
            //输出要取反         
        }else if (seq == "xzy")
        {
            seq = "yxz";  
        }
        //zyx=zyx
        if (seq == "zyx")
        {
            ry = Math.Asin(-r13);
            if (ry < Math.PI / 2)
            {
                if (ry > -Math.PI / 2)
                {
                    rx = Math.Atan2(r23, r33);
                    rz = Math.Atan2(r12, r11);
                }
                else
                {
                    // Not a unique solution.
                    rz = 0;
                    rx = -Math.Atan2(-r21, r22);
                }
            }
            else
            {
                // Not a unique solution.
                rz = 0;
                rx = Math.Atan2(-r21, r22);
            }
        }
        //yzx=zxy
        else if (seq == "yzx")
        {
            rx = Math.Asin(r23);
            if (rx < Math.PI / 2)
            {
                if (rx > -Math.PI / 2)
                {
                    ry = Math.Atan2(-r13, r33);
                    rz = Math.Atan2(-r21, r22);
                }
                else
                {
                    // Not a unique solution.
                    rz = 0;
                    ry = Math.Atan2(r31, r11);
                }
            }
            else
            {
                // Not a unique solution.
                rz = 0;
                ry = -Math.Atan2(r31, r11);
            }
        }
        //zxy=-yxz
        else if (seq == "zxy")
        {
            rx = Math.Asin(r32);
            if (rx < Math.PI / 2)
            {
                if (rx > -Math.PI / 2)
                {
                    ry = Math.Atan2(-r31, r33);
                    rz = Math.Atan2(-r12, r22);
                }
                else
                {
                    // Not a unique solution.
                    rz = 0;
                    ry = Math.Atan2(r21, r11);
                }
            }
            else
            {
                // Not a unique solution.
                rz = 0;
                ry = -Math.Atan2(r21, r11);
            }
        }
        //yxz=xzy
        else if (seq == "yxz")
        {
            rz = Math.Asin(-r21);
            if (rz < Math.PI / 2)
            {
                if (rz > -Math.PI / 2)
                {
                    rx = Math.Atan2(r23, r22);
                    ry = Math.Atan2(r31, r11);
                }
                else
                {
                    // Not a unique solution.
                    ry = 0;
                    rx = -Math.Atan2(-r13, r33);
                }
            }
            else
            {
                // Not a unique solution.
                ry = 0;
                rx = Math.Atan2(-r13, r33);
            }
        }
        //xyz=yzx
        else if (seq == "xyz")
        {
            rz = Math.Asin(r12);
            if (rz < Math.PI / 2)
            {
                if (rz > -Math.PI / 2)
                {
                    rx = Math.Atan2(-r32, r22);
                    ry = Math.Atan2(-r13, r11);
                }
                else
                {
                    // Not a unique solution.
                    ry = 0;
                    rx = Math.Atan2(r23, r33);
                }
            }
            else
            {
                // Not a unique solution.
                ry = 0;
                rx = -Math.Atan2(r23, r33);
            }
        }
        //xzy=-xyz
        else if (seq == "xzy")
        {
            ry = Math.Asin(-r31);
            if (ry < Math.PI / 2)
            {
                if (ry > -Math.PI / 2)
                {
                    rx = Math.Atan2(r32, r33);
                    rz = Math.Atan2(r21, r11);
                }
                else
                {
                    // Not a unique solution.
                    rz = 0;
                    rx = -Math.Atan2(-r12, r22);
                }
            }
            else
            {
                // Not a unique solution.
                rz = 0;
                rx = Math.Atan2(-r12, r22);
            }
        }
        else
        {
            Console.WriteLine("Invalid sequence: %s", seq);
        }
        EulerAngles eulerAngles = new EulerAngles();
        eulerAngles.rx = rx;
        eulerAngles.ry = ry;
        eulerAngles.rz = rz;

        if (invflag&&seq == "zxy")
        {
            eulerAngles.rx = -eulerAngles.rx;
            eulerAngles.ry = -eulerAngles.ry;
            eulerAngles.rz = -eulerAngles.rz;
        }else if (invflag&&seq == "xzy")
        {
            eulerAngles.rx = -eulerAngles.rx;
            eulerAngles.ry = -eulerAngles.ry;
            eulerAngles.rz = -eulerAngles.rz;
        }


        return eulerAngles;
    }

    }
}