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
using static HandEyeCalibration.ConversionAngle;
using Quaternion = HandEyeCalibration.Definetype.Quaternion;

namespace HandEyeCalibration
{

    public partial class HandEyeCalibration
    {
        

    /// <summary>眼在手外标定<para />
    /// <param name="a">InputArrayOfArrays 	R_gripper2base，= R_base2tool<para /></param>
    /// <param name="b">InputArrayOfArrays 	t_gripper2base, = T_base2tool<para /></param>
    /// <param name="b">InputArrayOfArrays 	R_target2cam， = R_cal2cam<para /></param>
    /// <param name="b">InputArrayOfArrays 	T_target2cam， = T_cal2cam<para /></param>
    /// <param name="b">handtoeye or eyeinhand<para /></param>
    /// <returns>R_cam2gripper,	= R_cam2base</returns>
    /// <returns>T_cam2gripper,	= T_cam2base</returns>
    /// </summary>
    /// 
    /// <summary>眼在手上标定<para />
    /// <param name="a">InputArrayOfArrays 	R_gripper2base，= R_tool2base<para /></param>
    /// <param name="b">InputArrayOfArrays 	t_gripper2base, = T_tool2base<para /></param>
    /// <param name="b">InputArrayOfArrays 	R_target2cam， = R_cal2cam<para /></param>
    /// <param name="b">InputArrayOfArrays 	T_target2cam， = T_cal2cam<para /></param>
    /// <returns>R_cam2gripper,	= R_cam2tool</returns>
    /// <returns>T_cam2gripper,	= T_cam2tool</returns>
    /// </summary>
    public static (double[,], double[,]) HandtoEyeCalibration(Robotpose3D[] _rtgripper2base, Campose3D[] _rtTarget2cam,string seq)
        {
                try
                {
                    if(_rtgripper2base.Length != _rtTarget2cam.Length)
                    {
                        Console.WriteLine("The number of data is not equal!");
                        return (null, null);
                    }

                    var l = _rtgripper2base.Length;
                    Mat[] R_base2gripper = new Mat[l];
                    Mat[] t_base2gripper = new Mat[l];

                    Mat[] R_target2cam = new Mat[l];
                    Mat[] t_target2cam = new Mat[l];

                    for (int i = 0; i < l; i++)
                    {

                        #region tcp位姿提取
                        //将四元数转换为旋转矩阵
                        var R_gripper2base = QuaterniontoRotationVector(_rtgripper2base[i].W, _rtgripper2base[i].Q1, _rtgripper2base[i].Q2, _rtgripper2base[i].Q3);
                        //齐次变换矩阵
                        double[,] RT_gripper2base = new double[4, 4];
                        for (int row = 0; row < 3; row++)
                            for (int col = 0; col < 3; col++)
                                RT_gripper2base[row, col] = R_gripper2base[row, col];
                        RT_gripper2base[0, 3] = _rtgripper2base[i].X;
                        RT_gripper2base[1, 3] = _rtgripper2base[i].Y;
                        RT_gripper2base[2, 3] = _rtgripper2base[i].Z;
                        RT_gripper2base[3, 3] = 1;

                        // 眼对手配置。 反转gripper2base 成 base2gripper 找到 cam2base
                        Mat mat = new Mat(4, 4, MatType.CV_64FC1, RT_gripper2base);
                        var inv = mat;

                        if(seq == "eyeinhand")
                        {
                            //眼在手上
                             inv = mat;

                        }else if(seq == "handtoeye")
                        {
                            //眼在手外
                            var invExpr = mat.Inv();
                            inv = invExpr.ToMat();

                        }

                        // 提取旋转矩阵和平移向量
                        R_base2gripper[i] = inv.SubMat(0, 3, 0, 3);
                        t_base2gripper[i] = inv.SubMat(0, 3, 3, 4);
                        #endregion


                        #region 标定板位姿提取

                        double[,] tmp_qua_R_t2c = QuaterniontoRotationVector(_rtTarget2cam[i].W, _rtTarget2cam[i].Q1, _rtTarget2cam[i].Q2, _rtTarget2cam[i].Q3);
                        //
                        Mat tmp_R_t2c = ArraytoConvertMat(tmp_qua_R_t2c);

                        /////测试
                        if (seq == "eyeinhand")
                        {
                        //眼在手上
                        }
                        else if (seq == "handtoeye")
                        {
                            //眼在手外
                            var tmp_R_t2cinvtemp = tmp_R_t2c.Inv();
                            tmp_R_t2c = tmp_R_t2cinvtemp.ToMat();
                        }
                        //////


                    double[,] tmp_arry_t_t2c = new double[3, 1] { { _rtTarget2cam[i].X }, { _rtTarget2cam[i].Y }, { _rtTarget2cam[i].Z } };
                        Mat tmp_t_t2c = ArraytoConvertMat(tmp_arry_t_t2c);
                        //
                        R_target2cam[i] = tmp_R_t2c;
                        t_target2cam[i] = tmp_t_t2c;

                        #endregion

                    }



                    using Mat R_camtobase = new Mat();
                    using Mat T_camtobase = new Mat();

                    using Mat R_cam2tool = new Mat();
                    using Mat T_cam2tool = new Mat();

                    if(seq =="handtoeye"){
                    // 估计相机在机器人底座框架中的位置
                    // 将HandEyeCalibrationMethod.TSAI替换为HandEyeCalibrationMethod.PARK可以更改使用的方法。
                    Cv2.CalibrateHandEye(R_base2gripper, t_base2gripper, R_target2cam, t_target2cam, R_camtobase, T_camtobase, HandEyeCalibrationMethod.TSAI);
                    }
                    else if (seq =="eyeinhand"){
                    // 将HandEyeCalibrationMethod.TSAI替换为HandEyeCalibrationMethod.PARK可以更改使用的方法。
                    Cv2.CalibrateHandEye(R_base2gripper, t_base2gripper, R_target2cam, t_target2cam, R_cam2tool, T_cam2tool, HandEyeCalibrationMethod.TSAI);
                    }


                    var rotationMatrix = ConvertMattoArray((seq =="handtoeye")?R_camtobase:R_cam2tool, false);
                    var translationMatrix = ConvertMattoArray((seq =="handtoeye")?T_camtobase:T_cam2tool, false);


                    return (rotationMatrix, translationMatrix);

                }
                catch (Exception ex)
                {
                    return (null, null);
                }
        }
    
    // 计算图像坐标
    public static double[,]  CalibrationCalculate(double[,] handEyeMatrix, double[] robotCoordinate)
        {

                    /* 已知的手眼标定矩阵
                    double[,] handEyeMatrix = {
                        {0.9786, -0.1609, 0.1333, 100},
                        {0.2056, 0.9752, -0.0803, 50},
                        {-0.0003, 0.0837, 0.9965, 200},
                        {0, 0, 0, 1}
                    };
                    // 机械手坐标
                    double[] robotCoordinate = { 300, 200, 100, 1 };*/

                    // 转换为OpenCvSharp的矩阵类型
                    Mat handEyeMat = new Mat(4, 4, MatType.CV_64FC1, handEyeMatrix);
                    Mat robotMat = new Mat(4, 1, MatType.CV_64FC1, robotCoordinate);

                    // 计算图像坐标
                    Mat imageMat = handEyeMat.Mul(robotMat);

                    // 输出结果
                    Console.WriteLine($"图像坐标：({imageMat.At<double>(0, 0)}, {imageMat.At<double>(1, 0)})");

                    return ConvertMattoArray(imageMat,true) ;
        }

    }
}