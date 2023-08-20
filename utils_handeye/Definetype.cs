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

namespace HandEyeCalibration
{

    public partial class Definetype
    {
    //四元数定义
    public class Quaternion
    {
        public double w;
        public double x;
        public double y;
        public double z;
    }
   
    //欧拉角度定义
    public class EulerAngles
    {
        public double rx; // x
        public double ry; // y
        public double rz; // z
        //从欧拉角（roll、yaw、pitch）计算旋转矩阵，必须清楚姿态绕XYZ轴的旋转顺序，zyx，xyz,yzx。。。
        //若x,y,z 依次旋转，否则旋转矩阵R = Rx * Ry * Rz
    }


    //Robotpose3D类来存储机器人坐标
    public class Robotpose3D
        {
            public double X { get; set; }

            public double Y { get; set; }

            public double Z { get; set; }

            public double W { get; set; }

            public double Q1 { get; set; }

            public double Q2 { get; set; }

            public double Q3 { get; set; }

            public Robotpose3D()
            {

            }

            public Robotpose3D(double _x, double _y, double _z, double _w0, double _q1, double _q2, double _q3) : this()
            {
                X = _x;
                Y = _y;
                Z = _z;
                W = _w0;
                Q1 = _q1;
                Q2 = _q2;
                Q3 = _q3;
            }
    }

    //Campose3D类来存储机器人坐标
    public class Campose3D
        {
            public double X { get; set; }

            public double Y { get; set; }

            public double Z { get; set; }

            public double W { get; set; }

            public double Q1 { get; set; }

            public double Q2 { get; set; }

            public double Q3 { get; set; }

            public Campose3D()
            {

            }

            public Campose3D(double _x, double _y, double _z, double _w0, double _q1, double _q2, double _q3) : this()
            {
                X = _x;
                Y = _y;
                Z = _z;
                W = _w0;
                Q1 = _q1;
                Q2 = _q2;
                Q3 = _q3;
            }
    
    }

    //#将数组对象转换为Robotpose3D
    public static Robotpose3D[] ConvertArray2Robotpose3D(double[,] Arrayq)
        {
                Robotpose3D[] result = new Robotpose3D[Arrayq.Length];
                for (int i = 0; i < Arrayq.Length; i++)
                { 
                        result[i].W = Arrayq[i,0];
                        result[i].Q1 = Arrayq[i,1];
                        result[i].Q2 = Arrayq[i,2];
                        result[i].Q3 = Arrayq[i,3];
                        result[i].X = Arrayq[i,4];
                        result[i].Y = Arrayq[i,5];
                        result[i].Z = Arrayq[i,6];
                }
                return result;
        }
    
    //#将数组对象转换为Campose3D
    public static Campose3D[] ConvertArray2Campose3D(double[,] Arrayq)
        {
                Campose3D[] result = new Campose3D[Arrayq.Length];
                for (int i = 0; i < Arrayq.Length; i++)
                { 
                        result[i].W = Arrayq[i,0];
                        result[i].Q1 = Arrayq[i,1];
                        result[i].Q2 = Arrayq[i,2];
                        result[i].Q3 = Arrayq[i,3];
                        result[i].X = Arrayq[i,4];
                        result[i].Y = Arrayq[i,5];
                        result[i].Z = Arrayq[i,6];
                }
                return result;
        }
    

    }
}

