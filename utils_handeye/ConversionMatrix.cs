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
    public partial class ConversionMatrix
    {

    //#将Mat对象转换为数组
    public static double[,] ConvertMattoArray(Mat _mat, bool _isFloat = true)


        {
                double[,] result = new double[_mat.Rows, _mat.Cols];
                for (int i = 0; i < _mat.Rows; i++)
                {
                    for (int j = 0; j < _mat.Cols; j++)
                    {
                        if (_isFloat)
                            result[i, j] = _mat.At<float>(i, j);
                        else
                            result[i, j] = _mat.At<double>(i, j);
                    }
                }
                return result;
        }
   
    //#将数组转换为Mat对象
    public static Mat ArraytoConvertMat(double[,] _ary)
        {
                Mat result = new Mat(_ary.GetLength(0), _ary.GetLength(1), MatType.CV_64FC1,_ary);

                return result;
        }

    /// <summary>矩阵相乘
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns>a*b</returns>
    public static double[,] MultiplyMatrices(double[,] matrix1, double[,] matrix2)
    {
        int rows1 = matrix1.GetLength(0);
        int cols1 = matrix1.GetLength(1);
        int cols2 = matrix2.GetLength(1);

        double[,] result = new double[rows1, cols2];

        for (int i = 0; i < rows1; i++)
        {
            for (int j = 0; j < cols2; j++)
            {
                for (int k = 0; k < cols1; k++)
                {
                    result[i, j] += matrix1[i, k] * matrix2[k, j];
                }
            }
        }

        return result;
    }

    /// <summary>矩阵相加
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns>a+b</returns>
    public static double[,] MatrixPlus(double[,] A, double[,] B)
    {
        int m, n;
        m = A.GetLength(0);
        n = A.GetLength(1);
        if (m != B.GetLength(0) || n != B.GetLength(1)) return null;
        double[,] C = new double[m, n];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                C[i, j] = A[i, j] + B[i, j];
            }
        }
        return C;
    }   



   /// <summary>将旋转矩阵与平移向量合成为齐次矩阵<para />
   /// <param name="a">3*3旋转矩阵<para /></param>
   /// <param name="b">3*1平移矩阵<para /></param>
   /// <returns>返回4*4齐次矩阵</returns>
   /// </summary>
    public static  Mat R_T2HomogeneousMatrix(Mat R,Mat T)
    {
        double[,] HomoMtrarry = new double[4, 4];
        Mat HomoMtr = new Mat(4, 4, MatType.CV_64FC1, HomoMtrarry);

        double[,] R1arry = new double[4, 3];
        for (int row = 0; row < 4; row++)
            for (int col = 0; col < 3; col++){
                R1arry[row, col] = R.At<double>(row, col);
                if (row == 4)  R1arry[row, col]= 0;
                };
        Mat R1 = new Mat(4, 3, MatType.CV_64FC1, R1arry);

        double[,] T1arry = new double[4, 1];
        for (int row = 0; row < 4; row++)
            for (int col = 0; col < 1; col++){
                T1arry[row, col] = T.At<double>(row, col);
                if (row == 4)  T1arry[row, col]= 0;
                }
        Mat T1 = new Mat(4, 1, MatType.CV_64FC1, T1arry);

        Cv2.HConcat(R1, T1, HomoMtr);		//矩阵拼接
        return HomoMtr;
    }


    /// <summary>齐次矩阵分解为旋转矩阵与平移矩阵<para />
    /// <param name="a">4*4齐次矩阵<para /></param>
    /// <param name="b"><para /></param>
    /// <returns>输出旋转矩阵</returns>
    /// </summary>
    public static  Mat HomogeneousMtr2R(Mat HomoMtr, Mat R, Mat T)
    {
        double[,] R1arry = new double[3, 3];
        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++){
                R1arry[row, col] = HomoMtr.At<double>(row, col);
                };
        Mat R1 = new Mat(3, 3, MatType.CV_64FC1, R1arry);
        return  R1;
    }


    /// <summary>齐次矩阵分解为旋转矩阵与平移矩阵<para />
    /// <param name="a">4*4齐次矩阵<para /></param>
    /// <param name="b"><para /></param>
    /// <returns>输出平移矩阵</returns>
    /// </summary>
    public static  Mat HomogeneousMtr2T(Mat HomoMtr, Mat R, Mat T)
    {
        
        double[,] T1arry = new double[3, 1];
        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 1; col++){
                T1arry[row, col] = HomoMtr.At<double>(row, col);
                }
        Mat T1 = new Mat(3, 1, MatType.CV_64FC1, T1arry);
        return  T1;
    }


    /// <summary>检查是否是旋转矩阵<para />
    /// <param name="a"><para /></param>
    /// <param name="b"><para /></param>
    /// <returns>return  true : 是旋转矩阵， false : 不是旋转矩阵</returns>
    /// </summary>
    public static  bool IsRotatedMatrix(Mat R)		
    {//旋转矩阵的转置矩阵是它的逆矩阵，逆矩阵 * 矩阵 = 单位矩阵
        Mat tempR = R.AdjustROI(0,0,3,3);	//无论输入是几阶矩阵，均提取它的三阶矩阵
        Mat TransposeRarry = tempR.Transpose();
        Mat shouldBeIdentity = TransposeRarry * tempR;//是旋转矩阵则乘积为单位矩阵
        Mat I = Mat.Eye(3, 3, shouldBeIdentity.Type());
        bool notCompare=true;
        for (int row = 0; row < shouldBeIdentity.Rows; row++)
            for (int col = 0; col < shouldBeIdentity.Cols; col++){
                if (Math.Abs(I.At<double>(row, col) - shouldBeIdentity.At<double>(row, col)) < 1e-6)
                    notCompare = false;
                }
        return notCompare;
    }


    }
}