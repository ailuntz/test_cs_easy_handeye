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
using Size = OpenCvSharp.Size;

namespace HandEyeCalibration
{
    public partial class Cameracalibration
    {

    /// <summary>相机标定<para />
    /// <param name="a"><para /></param>
    /// <param name="b"><para /></param>
    /// <returns>R_target2cam ， T_target2cam 是标定板相对于相机的齐次矩阵，
    ///进行相机标定时用calibrateCamera得到, 
    ///可以通过solvePnP得到</returns>
    /// </summary>
    public static bool Cameracalibrationtest()
    {   
        //图片路径
        string save_dir = "./Checkerboard_picture/IR_dedistortion";
        string img_dir = "./Checkerboard_picture/IR_camera_calib_img";
        List<string> img_paths = new List<string>();
        string[] extensions = { "jpg", "png", "jpeg" };
        foreach (string extension in extensions)
        {
            img_paths.AddRange(Directory.GetFiles(img_dir, $"*.{extension}"));
        }
        Debug.Assert(img_paths.Count == 0, "No images for calibration found!");

        //棋盘格模板规格
        Size patternSize = new Size(9, 6);//棋盘格模板规格
        float  size_grid = 0.15F;//棋盘格每个格子的大小

        int h = patternSize.Height; 
        int w = patternSize.Width; 

        // Cp_int：int形式的角点，以‘int’形式保存世界空间角点的坐标，如(0,0,0), (1,0,0), (2,0,0) ...., (10,7,0)
        Point2f[] cp_int = new Point2f[w*h];
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                cp_int[i * w + j] = new Point2f();
                cp_int[i * w + j].X = j;
                cp_int[i * w + j].Y = i;
            }
        }
        // Cp_world：世界空间中的角点，保存世界空间中角点的坐标
        Point3f[] cp_world = new Point3f[w * h];
        for (int i = 0; i < w * h; i++)
        { 
            cp_world[i] = new Point3f();
            //cp_world[i].X = (float)Math.Round((cp_int[i].X * size_grid),5);
            cp_world[i].X = cp_int[i].X * size_grid;
            //cp_world[i].Y = (float)Math.Round((cp_int[i].Y * size_grid),5);
            cp_world[i].Y = cp_int[i].Y * size_grid;
            cp_world[i].Z = 0;
        }

        OutputArray corners = new Mat();
        //x, y, z, rx, ry, rz = -3.04639859, -2.52416742, 29.2139115, 0.00724597, 0.61002365, 0.01765365
        //Cv2.NamedWindow("imgname0", WindowFlags.FullScreen);
        //Mat gray_imgsingle= Cv2.ImRead(@"/Users/ailuntz/Documents/github/test_c_easy_handeye/ConsoleApp1/Checkerboard_picture/IR_camera_calib_img/scan_0002_IMG_Texture_8Bit.png", ImreadModes.Grayscale);
        //Cv2.Resize(img, img, new Size(640, 480));
        //Mat roiimage = image.Clone(new Rect(0, 0, 640, 480));
        //Cv2.MoveWindow("imgname0", 100,100); 
        //Cv2.ImShow("imgname0", gray_imgsingle);
        //Cv2.WaitKey(1);
        //Cv2.WaitKey(1);  
        List<List<Point3f>> points_world = new(); // the points in world space
        List<List<Point2f>> points_pixel = new(); // the points in pixel space (relevant to points_world)
        foreach (string img_path in img_paths)
        {
            Mat img = Cv2.ImRead(img_path);
            Mat gray_img = new Mat();
            Cv2.CvtColor(img, gray_img, ColorConversionCodes.BGR2GRAY);
            //找到角点，cp_img：像素空间中的角点
            List < Point3f > points_world_temp = new(); // the points in world space
            List < Point2f > points_pixel_temp = new(); // the points in world space          
            
            //Cv2.FindCirclesGrid (gray_img, patternSize, corners, FindCirclesGridFlags.AsymmetricGrid);
            bool ret = Cv2.FindChessboardCorners(gray_img, new Size(w, h), out Point2f[] cp_img, ChessboardFlags.AdaptiveThresh | ChessboardFlags.FastCheck | ChessboardFlags.NormalizeImage);
            
            //亚像素精确化
            IEnumerable<Point2f> cornersSubPix = new List<Point2f>();
            Cv2.CornerSubPix(gray_img, cp_img, new Size(5, 5), new Size(-1, -1), TermCriteria.Both(30, 0.1));

            // 如果ret为True，则保存
            if (ret)
            {
                // Cv2.CornerSubPix(gray_img, cp_img, new Size(11, 11), new Size(-1, -1), criteria);
                
                //埋了一个坑、未考虑到角点的对齐问题
                for (int i = 0; i < cp_img.Length; i++)
                {
                    points_world_temp.Add(cp_world[i]);
                    points_pixel_temp.Add(cp_img[i]);
                }

                points_world.Add(points_world_temp);
                points_pixel.Add(points_pixel_temp);

                bool visualization = true;
                // 查看角点检测结果
                if (visualization)
                {
                    Cv2.DrawChessboardCorners(gray_img, new Size(w, h), cp_img, ret);
                    Cv2.ImShow("FoundCorners", gray_img);
                    Cv2.WaitKey(500);
                }
            }
        }


        Mat cameraMatrix = new Mat(3, 3, MatType.CV_32FC1);
        Mat distCoeffs = new Mat(1, 5, MatType.CV_32FC1);

        List<Mat> points_world_list = new();
        List<Mat> points_pixel_list = new();       


        for (int i = 0; i < points_world.Count; i++)
        {
            Point3f[] points_world_single_arry = points_world[i].ToArray();
            Mat m = new Mat(points_world_single_arry.Length, 1, MatType.CV_32FC3, points_world_single_arry);
            points_world_list.Add(m);

            Point2f[] points_pixel_single_arry = points_pixel[i].ToArray();
            Mat n = new Mat(points_pixel_single_arry.Length, 1, MatType.CV_32FC2, points_pixel_single_arry);
            points_pixel_list.Add(n);

        }
        Size sizetemp  = new Size(1544,2064);

        var rms = Cv2.CalibrateCamera(points_world_list, points_pixel_list, sizetemp, cameraMatrix, distCoeffs, out Mat[] rvecs, out Mat[] tvecs, CalibrationFlags.FixIntrinsic);
        //double[,] tempcameraMatrix= ConvertMat2Array(cameraMatrix);
        //Console.WriteLine("intrinsic matrix: \n{0}", tempcameraMatrix);
        double[,] tempcameraMatrix= ConvertMattoArray(cameraMatrix,false);
        double[,] tempdistCoeffs= ConvertMattoArray(distCoeffs,false);
        double[,] temprvecs= ConvertMattoArray(rvecs[0],false);
        double[,] temptvecs= ConvertMattoArray(tvecs[0],false);


        // 计算重投影的误差//重投影用的是单个图片的角点坐标
        double tot_error = 0;
        Mat reprojected_points = new Mat(points_world[0].Count, 1, MatType.CV_32FC2);
        double[,]? reprojected_points_temp = null;
        for (int i = 0; i < points_world.Count; i++)
        {
            Cv2.ProjectPoints(points_world_list[i], rvecs[i], tvecs[i], cameraMatrix, distCoeffs, reprojected_points);
            double error = Cv2.Norm(points_pixel_list[i], reprojected_points, NormTypes.L2)/((points_world.Count)*2);
            tot_error += error ;
        }
        double mean_error = Math.Sqrt(tot_error / points_world.Count);
        Console.WriteLine("Mean reprojection error: " + mean_error);

        // 校正图像
        foreach (string img_path in img_paths)
        {
            string img_name = Path.GetFileName(img_path);
            Mat img = Cv2.ImRead(img_path);
            Mat newcameramtx = Cv2.GetOptimalNewCameraMatrix(cameraMatrix, distCoeffs, img.Size(), 1, img.Size(), out _, false);
            Mat dst = new Mat();
            Cv2.Undistort(img, dst, cameraMatrix, distCoeffs, newcameramtx);
            // 剪裁图像
            // int x = roi.X;
            // int y = roi.Y;
            // int width = roi.Width;
            // int height = roi.Height;
            // dst = dst[new Rect(x, y, width, height)];
            Cv2.ImWrite(Path.Combine(save_dir, img_name), dst);
        }
        Console.WriteLine("已将去失真图像保存到: " + save_dir);
        
        
        //3d-2d
        //根据已有的相机内参计算出相机与标定板之间的外参数
        //rvec与tvec是从世界坐标系（标定板坐标系）到摄像基坐标转换
        //既可以输入棋盘格世界系坐标，也可以输入物体世界系坐标:)
        using var rvecMat = new Mat();
        using var tvecMat = new Mat();
        Cv2.SolvePnP(points_world_list[0], points_pixel_list[0], cameraMatrix, distCoeffs, rvecMat, tvecMat);
        //Console.WriteLine($"rvec: {rvecMat}");
        //Console.WriteLine($"tvec: {tvecMat}");

        return false;
    }

    private static IEnumerable<Point3f> Create3DChessboardCorners(Size boardSize, float squareSize)
    {
        for (int y = 0; y < boardSize.Height; y++)
        {
            for (int x = 0; x < boardSize.Width; x++)
            {
                yield return new Point3f(x * squareSize, y * squareSize, 0);
            }
        }
    }

    private static IEnumerable<Point3d> Generate3DPoints()
    {
        double x, y, z;

        x = .5; y = .5; z = -.5;
        yield return new Point3d(x, y, z);

        x = .5; y = .5; z = .5;
        yield return new Point3d(x, y, z);

        x = -.5; y = .5; z = .5;
        yield return new Point3d(x, y, z);

        x = -.5; y = .5; z = -.5;
        yield return new Point3d(x, y, z);

        x = .5; y = -.5; z = -.5;
        yield return new Point3d(x, y, z);

        x = -.5; y = -.5; z = -.5;
        yield return new Point3d(x, y, z);

        x = -.5; y = -.5; z = .5;
        yield return new Point3d(x, y, z);
    }

    public static void staticRodrigues()
    {
        const double angle = 45;
        double cos = Math.Cos(angle * Math.PI / 180);
        double sin = Math.Sin(angle * Math.PI / 180);
        var matrix = new double[3, 3]
        {
            {cos, -sin, 0},
            {sin, cos, 0},
            {0, 0, 1}
        };

        Cv2.Rodrigues(matrix, out var vector, out var jacobian);
    }

    public static bool pnp()
    {
        //pnp示例
         List<Point3f> threeDim = new List<Point3f>()
            {
                new Point3f(100f, 200f, 300f),
             new Point3f(400f, 500f, 600f),
             new Point3f(700f, 800f, 900f),
             new Point3f(600f, 800f, 200f),            
            };
         List<Point2f> twoDim = new List<Point2f>()
            {
                 new Point2f(100f, 200f),
            new Point2f(400f, 500f),
            new Point2f(700f, 800f), 
            new Point2f(900f, 100f),              
            };

        List<Point3f> threeDim1 = new List<Point3f>()
            {
            new Point3f(0f, 0f, 0f),
            new Point3f(0.15f, 0f, 0f),
            new Point3f(0.3f, 0f, 0f),
            new Point3f(0.45f, 0, 0), 
            new Point3f(0.6f, 0f, 0f),
            new Point3f(0.75f, 0f, 0f),

            new Point3f(0.15f, 0.15f, 0f),
            new Point3f(0.3f, 0.15f, 0f),
            new Point3f(0.45f, 0.15f, 0f),
            new Point3f(0.6f, 0.15f, 0), 
            new Point3f(0.75f, 0.15f, 0f),
            new Point3f(0.9f, 0.15f, 0f),   

            new Point3f(0.3f, 0.3f, 0f),
            new Point3f(0.45f, 0.3f, 0f),
            new Point3f(0.6f, 0.3f, 0f),
            new Point3f(0.75f, 0.3f, 0), 
            new Point3f(0.9f, 0.3f, 0f),
            new Point3f(1.05f, 0.3f, 0f),      

            new Point3f(0.6f, 0.6f, 0f),
            new Point3f(0.75f, 0.6f, 0f),
            new Point3f(0.9f, 0.6f, 0f),
            new Point3f(1.05f, 0.6f, 0), 
            new Point3f(1.2f, 0.75f, 0f),
            new Point3f(0f, 0.75f, 0f),  


            };
        List<Point2f> twoDim1 = new List<Point2f>()
            {
            new Point2f(539.17645f, 531.7228f),
            new Point2f(645.4736f, 532.5014f),
            new Point2f(757.936f, 533.9747f), 
            new Point2f(875.05743f, 534.67975f),    
            new Point2f(993.37836f, 536.9847f), 
            new Point2f(1116.8296f,  538.0309f),  

            new Point2f(640.4984f, 652.91705f),
            new Point2f(752.2385f, 655.26953f),
            new Point2f(868.24963f, 659.66187f), 
            new Point2f(988.7275f, 662.99f),    
            new Point2f(1110.1676f,  666.6372f), 
            new Point2f(1236.792f,  669.0209f),  

            new Point2f(746.5611f, 778.94775f),
            new Point2f(863.6078f, 784.0315f),
            new Point2f(982.7124f, 788.63025f), 
            new Point2f(1105.7122f,  794.5766f),    
            new Point2f(1232.5756f,  799.5878f), 
            new Point2f(1362.2954f,  805.44574f),  

            new Point2f(858.4394f, 909.5691f),
            new Point2f(974.8172f, 916.6481f),
            new Point2f(1099.8346f,  924.3709f), 
            new Point2f(1226.7301f,  932.2268f),    
            new Point2f(1355.9869f,  939.3499f), 
            new Point2f(1488.7744f,  946.61743f),             
            };
        //double[] dist = new double[5] { -0.05697294087771295, -3.9202766326545206, 0.01943979076345387, -0.0470540904605256, 20.76880076547274};
        //double[] camD = new double[9] { 3203.55222583, 0, 1322.20951231, 0, 2962.74235065, 441.90418957, 0, 0, 1 };
        //double[] dist = new double[5] { -0.05283023496591613,-0.0050814901063073814, 0.018183325854066592, -0.010662561001730558, -8.144552855009733E-05};
        //double[] camD = new double[9] { 491.72758694010935, 0, 797.6379898593872, 0, 392.19513760544055, 966.4119947968791, 0, 0, 1 };   
        
        double[] camD = new double[9] { 2, 0, 99.5, 0, 21.83, 39.5, 0, 0, 1 };
        //double[] rvcs1 = new double[5][-0.5654701219693429,-0.7034601005986886,-0.21492416715183246];
        //double[] tvcs1 = new double[5][-0.26027545299901345, -0.6232079850934842, 0.3227915980396588];

        Mat camera_matrix = new Mat(3, 3, MatType.CV_64FC1, camD, 0);          
        var objPtsMat = InputArray.Create<Point3f>(threeDim, MatType.CV_32FC3);
        var imgPtsMat = InputArray.Create<Point2f>(twoDim, MatType.CV_32FC2);

        //var distMat = Mat.FromArray(dist);
        var distMat = Mat.Zeros(5, 0, MatType.CV_64FC1);
        var rvecMat = new Mat();
        var tvecMat = new Mat();                    
        Mat rvec = new Mat();                 
        // 平面时 旋转及平移向量 SolvePnPFlags.DLS  贼大// Iterative接近CalibrateCamera   //  p3p平面报错  //                  
        Cv2.SolvePnP(objPtsMat, imgPtsMat, camera_matrix, distMat, rvecMat, tvecMat, false, SolvePnPFlags.Iterative);
       
        double[,] R1d = new double[3,3];
        R1d = ConvertMattoArray(rvecMat,false);

        Cv2.Rodrigues(rvecMat, rvec);
        double[,] R = new double[3,3];
        R = ConvertMattoArray(rvec,false);
        double[,] T = new double[3,1];
        T = ConvertMattoArray(tvecMat,false);
        //OutInfo = Calculate(R, T);                 
        //double[,] objPoint = new double[3, 1] { { 1.2 }, { 0.75 }, { 0 } };
        double[,] objPoint = new double[3, 1] { { 300 }, { 400 }, { 500 } }; 
        double[,] pCamR = MultiplyMatrices(R, objPoint);
        double[,] pCamRT = MatrixPlus(pCamR, T); //回到相机坐标系        
        double Xc = pCamRT[0, 0];
        double Yc =  pCamRT[1, 0];
        double Zc =  pCamRT[2, 0];
        double picPointX = Xc / Zc;
        double picPointY = Yc / Zc;
        //double[,] camD2 = new double[3,3] { { 491.72758694010935, 0, 392.19513760544055 }, { 0, 797.6379898593872, 0 }, { 0, 0, 1 } };//相机参数
        Mat camD2 = camera_matrix.Inv();
        double[,] camD3 = new double[3,3] { { 2851.83556, 0, 959.5 }, { 0, 2851.83556, 539.5 }, { 0, 0, 1 } };//相机参数
        //double[,] camD3 = ConvertMat2Array(camD2,false);
        double[,] matrixXn = new double[3, 1] { { picPointX }, { picPointY }, { 0 } };
        double[,] picelePoints = MultiplyMatrices(camD3, matrixXn);//3*1的矩阵，像素坐标值为前两个数字。     
        return true;                            
    }


    }
}