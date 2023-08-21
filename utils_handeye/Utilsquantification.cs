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
    public class Srcquantification
    {
        public double Srcmin;
        public double Srcmax;
    }

    public class Dstquantification
    {
        public double Dstmin;
        public double Dstmax;
    }

    ///<summary>
    ///Srcquantification Srcquantificationtemp = new Srcquantification();
    ///Srcquantificationtemp.Srcmax = 3.402823466e+38F;
    ///Srcquantificationtemp.Srcmin = 1.175494351e-38F;
    ///Dstquantification Dstquantificationtemp = new Dstquantification();
    ///Dstquantificationtemp.Dstmax = 127;
    ///Dstquantificationtemp.Dstmin = -128;
    /// </summary>
    public partial class Utilsquantification
    {
        /*
        Srcquantification Srcquantificationtemp = new Srcquantification();

        Srcquantificationtemp.Srcmax = 8.6;
            Srcquantificationtemp.Srcmin = 2.1;
            Dstquantification Dstquantificationtemp = new Dstquantification();

        Dstquantificationtemp.Dstmax = 127;
            Dstquantificationtemp.Dstmin = -128;

            Utilsquantification.Quantification(Srcquantificationtemp, Dstquantificationtemp, out float Scalin, out float zero);

        float[] src = { 2.1f, 8.6f };
        sbyte[] dst = new sbyte[src.Length];

        Utilsquantification.Quantificationout(Scalin, zero, src,out dst);
        //*/



        public static void Quantificationout(float Scalin, float zero, float[] srcdata, out sbyte[] dstdata)
        {
            dstdata = new sbyte[srcdata.Length];
            int len = srcdata.Length;
            for (int i = 0; i < len; i++)
            {
                dstdata[i] = (sbyte)Math.Round(srcdata[i] / Scalin + zero);
            }


        }

        public static void Quantification(Srcquantification srcdata, Dstquantification dstdata , out float Scalin, out float zero)
        {
            Scalin = SValue(srcdata, dstdata);
            zero = ZValue(srcdata, dstdata);

            Console.WriteLine("s = {0}", Scalin);
            Console.WriteLine("z = {0}", zero);
        }

        public static float SValue(Srcquantification srcdata, Dstquantification dstdata)
        {
            float rMax = (float)srcdata.Srcmax;
            float rMin = (float)srcdata.Srcmin;


            sbyte qMax = 127;
            sbyte qMin = -128;

            float s = (rMax - rMin) / (qMax - qMin);
            return s;
        }

        public static float ZValue(Srcquantification srcdata, Dstquantification dstdata)
        {
            float rMax = (float)srcdata.Srcmax;
            sbyte qMax = 127;
            float s = SValue(srcdata, dstdata);
            float z = (float)Math.Round(qMax - (rMax / s));
            return z;
        }

        public static float FindFloat32Max(float[] r)
        {
            int len = r.Length;
            float tmp = 0.0f;
            for (int i = 0; i < len; i++)
            {
                if (r[i] > tmp)
                {
                    tmp = r[i];
                }
            }
            return tmp;
        }

        public static float FindFloat32Min(float[] r)
        {
            int len = r.Length;
            float tmp = r[0];
            for (int i = 0; i < len; i++)
            {
                if (r[i] < tmp)
                {
                    tmp = r[i];
                }
            }
            return tmp;
        }


    }
}