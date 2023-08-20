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
using Size = OpenCvSharp.Size;
using Point = OpenCvSharp.Point;

namespace HandEyeCalibration
{
    public partial class utilsopencv
    {

    /// <summary>opencv在图片中插入图片</summary>
    public static Mat MatCopy(Mat image)
        {
            Mat logo = Cv2.ImRead("/Users/ailuntz/Documents/github/test_c_easy_handeye/ConsoleApp1/logo/IMG_2240.JPG", ImreadModes.AnyColor);
            Cv2.Resize(logo, logo, new Size(100, 100));
            Rect rectroi = new Rect(image.Cols - logo.Cols, image.Rows - logo.Rows, logo.Cols, logo.Rows);
            Mat imageroi = new Mat(image, rectroi);
            logo.CopyTo(imageroi);
            //ROI 实际上就是一个cv::Mat 对象，它与它的父图像指向同一个数据缓冲区，并且在头
            //部指明了ROI 的坐标。

            return image;
        }

    /// <summary>opencv在图像中加入椒盐噪声
    ///（salt-and-pepper noise）。顾名思义，椒盐噪声是一个专门的噪声类型
    /// 它随机选择一些像素，把它们的颜色替换成白色或黑色。
    /// 如果通信时出错，部分像素的值在传输时丢失，就会产生这种噪声。
    /// 这里只是随机选择一些像素，把它们设置为白色。
    /// </summary>
    public static Mat SaltPepperNoisy(Mat image)
    {
        Random random = new Random();
        int n = random.Next(0, 100);
        int j, k;
        for (int i = 0; i < n; i++)
        {
            j = random.Next(0, image.Rows - 1);
            k = random.Next(0, image.Cols - 1);
            //如果是单通道灰色图像，char black = (char)0;
            //char white = (char)0;
            //如果是彩色图片，因为彩图是3通道，所以要Vec3b white = new Vec3b(255, 255, 255);
            Vec3b white = new Vec3b(255, 255, 255);
            image.Set(j, k, white);
        }
        return image;
    }

    /// <summary>opencv操作像素点
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public  static Mat  CtrlPixel(Mat image)
    {
        for (int i = 0; i < image.Rows; i++)
        {
            for (int j = 0; j < image.Cols; j++)
            {
                //如果是灰色图像
                //byte color = (byte)Math.Abs(mat.Get<byte>(i, j) - 50);//读取原来的通道值并减50
                //mat.Set(i, j, color);
                Vec3b color = new Vec3b();
                color.Item0 = (byte)Math.Abs((image.Get<Vec3b>(i, j).Item0 - 50));
                color.Item1 = (byte)Math.Abs((image.Get<Vec3b>(i, j).Item1 - 50));
                color.Item2 = (byte)Math.Abs((image.Get<Vec3b>(i, j).Item2 - 50));
                image.Set(i, j, color);
            }
        }
        return image;
    }
    
    /// <summary>opencv基础操作
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public  static Mat  Opecvbase()
    {
        /*
        imread()参数说明
        cv2.IMREAD_UNCHANGED = -1, //返回原通道原深度图像
        cv2.IMREAD_GRAYSCALE = 0, //返回单通道（灰度），8位图像
        cv2.IMREAD_COLOR = 1, //返回三通道，8位图像，为默认参数
        cv2.IMREAD_ANYDEPTH = 2, //返回单通道图像。如果原图像深度为16/32 位，则返回原深度，否则转换为8位
        cv2.IMREAD_ANYCOLOR = 4, //返回原通道，8位图像。

        Cv2.Flip(image, result, FlipMode.Y);//Flip 翻转图片
        Cv2.Blur(mat, mat, new OpenCvSharp.Size(5, 5));
        Cv2.GaussianBlur(mat, mat, new OpenCvSharp.Size(3, 5), 0);
        Cv2.BilateralFilter(mat, mat, 5, 10, 2);
        •Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);//转灰度图
        Scalarscalar = Cv2.Mean(gray);//计算灰度图平均值
        Cv2.Threshold(gray, gray, scalar.Val0, 255, ThresholdTypes.Binary);//二值化
        */
        //读取图片
        string imgName = "";
        Cv2.NamedWindow("imgname0", WindowFlags.FullScreen);

        Mat img= Cv2.ImRead(@"/Users/ailuntz/Documents/github/test_c_easy_handeye/ConsoleApp1/棋盘格图片/IR_camera_calib_img/scan_0002_IMG_Texture_8Bit.png", ImreadModes.AnyColor);
        Cv2.Resize(img, img, new Size(640, 480));
        MatCopy(img);
        SaltPepperNoisy(img);
        Cv2.MoveWindow("imgname0", 100,100); 

        Cv2.Circle(img, 200, 200, 200, 0, 3);
        Point org = new Point(100, 100);
        Cv2.PutText(img, "this is AILUNTZ", org, HersheyFonts.HersheyPlain, 2, Scalar.Red, 2, LineTypes.AntiAlias);
        
        Cv2.ImShow("imgname0", img);
        Cv2.WaitKey(0);
        Cv2.WaitKey(0);       
        Cv2.DestroyAllWindows();
        GC.Collect();

        Cv2.DestroyAllWindows();

        Cv2.WaitKey(1);
        Cv2.WaitKey(1);
        Cv2.WaitKey(1);
        return img;
    }
    




    }
}