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

    //public static void Main(String[] args)
    public partial class Test
    {

        /*方法测试temp
        //bool temp1 = pnp();
        //bool temp = Cameracalibration();
        */

        /*基础功能测试
        Console.WriteLine("Hello, World!");
        Mat img = new Mat ("/Users/ailuntz/Documents/github/test_c_easy_handeye/截屏2023-07-27 03.13.54.png");
        Cv2.ImShow ("Beauty",img);
        Cv2.WaitKey();
        */

        /*方法测试
        EulerAngles e = new();
        e.roll = 0.14;
        e.pitch = 1.21;
        e.yaw = 2.1;

        Vector3 v = new() { X = 0.14F, Y = 1.21F, Z = 2.1F };
      
        // 将欧拉角转换为四元数:
        Quaternion q = EulerToQuaternion(e.yaw,e.pitch,e.roll);
       
        // 将相同的四元数转换回欧拉角:
        EulerAngles n = QuaternionToEulerAngles(q);

        // 验证转换
        Console.WriteLine($"Q: {q.x} {q.y} {q.z} {q.w}");
        Console.WriteLine($"E: {n.roll} {n.pitch} {n.yaw}");
        
        Quaternion vq = VectorToQuaternion(v);
        Vector3 vn = VectorToEulerAngles(vq);

        Console.WriteLine($"Q: {vq.x} {vq.y} {vq.z} {vq.w}");
        Console.WriteLine($"E: {vn.X} {vn.Y} {vn.Z}");
        */

        //调用HandEyeCalibration函数
       (double[,], double[,]) resultHandEye = EyeHandCalibration.HandtoEyeCalibration(_gripper2base_data, _gTarget2cam_data);
        Console.WriteLine(resultHandEye);

    }
}