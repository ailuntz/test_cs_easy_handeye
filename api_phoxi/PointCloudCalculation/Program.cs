using pho.api.csharp;
using System;

internal class Program
{
    const ulong channels = 3; // XYZ organisation
    static void Main(string[] args)
    {
        PhoXiFactory Factory = new PhoXiFactory();
        //Check if the PhoXi Control is running
        if (!Factory.isPhoXiControlRunning()) return;
        Console.WriteLine("PhoXi Control is running");
        //Get List of available devices on the network

        PhoXiDeviceInformation[] DeviceList = Factory.GetDeviceList();
        Console.WriteLine("PhoXi Factory found {0} devices by GetDeviceList call.", DeviceList.Length);
        Console.WriteLine();
        for (int i = 0; i < DeviceList.Length; i++)
        {
            Console.WriteLine("Device: {0}", i);
            Console.WriteLine("  Name:                    " + (String)DeviceList[i].Name);
            Console.WriteLine("  Hardware Identification: " + (String)DeviceList[i].HWIdentification);
            Console.WriteLine("  Type:                    " + (String)DeviceList[i].Type);
            Console.WriteLine("  Firmware version:        " + (String)DeviceList[i].FirmwareVersion);
            Console.WriteLine("  Variant:                 " + (String)DeviceList[i].Variant);
            Console.WriteLine("  IsFileCamera:            " + (DeviceList[i].IsFileCamera ? "Yes" : "No"));
            Console.WriteLine("  Feaure-Alpha:            " + (DeviceList[i].CheckFeature("Alpha") ? "Yes" : "No"));
            Console.WriteLine("  Feaure-Color:            " + (DeviceList[i].CheckFeature("Color") ? "Yes" : "No"));
            Console.WriteLine("  Status:                  " + (DeviceList[i].Status.Attached ? "Attached to PhoXi Control. " : "Not Attached to PhoXi Control. ") + (DeviceList[i].Status.Ready ? "Ready to connect" : "Occupied"));
            Console.WriteLine();
        }

        //Try to connect Device opened in PhoXi Control, if Any
        PhoXi PhoXiDevice = Factory.CreateAndConnectFirstAttached();
        if (PhoXiDevice != null)
        {
            Console.WriteLine("You have already PhoXi device opened in PhoXi Control, the API Example is connected to device: " + (String)PhoXiDevice.HardwareIdentification);
        }
        else
        {
            Console.WriteLine("You have no PhoXi device opened in PhoXi Control, the API Example will try to connect to last device in device list");
            if (DeviceList.Length > 0)
            {
                PhoXiDevice = Factory.CreateAndConnectFirstAttached();
            }
        }
        if (PhoXiDevice == null)
        {
            Console.WriteLine("No device is connected!");
            return;
        }

        if (PhoXiDevice.isConnected())
        {
            if (!PhoXiDevice.isAcquiring())
            {
                PhoXiDevice.StartAcquisition();
            }

            if (PhoXiDevice.isAcquiring())
            {
                int FrameID = PhoXiDevice.TriggerFrame();

                if (FrameID < 0)
                {
                    //If negative number is returned trigger was unsuccessful
                    Console.WriteLine("Trigger was unsuccessful! code={0}", FrameID);
                    return;
                }
                Frame frame = PhoXiDevice.GetFrame();

                //The reprojection map is constant for resolution, it is not necessary
                // to obtain it if the resolution has not changed. 
                //MotionCam has valid reprojection map only for the Regular Grid mode.
                ReprojectionMap32f reprojection = PhoXiDevice.ReprojectionMap.Map;
                DepthMap32f depth = frame.DepthMap;
                //Get point cloud as array of floats
                float[] calculatedPointCloud = CalculatePointCloud(reprojection, depth);

                //Check if calculated point cloud equals the one from frame
                if (calculatedPointCloud.Length != (int)(frame.PointCloud.GetElementsCount() * channels))
                {
                    Console.WriteLine("Calculated point cloud size does not match the one from frame!");
                    PhoXiDevice.Disconnect();
                    return;
                }
                int size = calculatedPointCloud.Length;
                float[] framePointCloudData = frame.PointCloud.GetDataCopyXYZXYZ();
                for (int i = 0; i < size; ++i)
                {
                    if (framePointCloudData[i] != calculatedPointCloud[i])
                    {
                        Console.WriteLine("Calculated point cloud does not match the one from frame!");
                        break;
                    }
                }
            }
            Console.WriteLine("Calculated point cloud is the same as the one from frame.");
        }

        PhoXiDevice.Disconnect();
    }

    // Optimized point cloud calculus based on native data structures
    static float[] CalculatePointCloud(ReprojectionMap32f reprojection, DepthMap32f depth)
    {
        ulong size = reprojection.GetElementsCount();
        float[] pointCloud = new float[size * channels];
        float[] reprojectionData = reprojection.GetDataCopyXYZXYZ();
        float[] depthData = depth.GetDataCopy();
        for (ulong i = 0; i < size; ++i)
        {
            for (ulong ch = 0; ch < channels; ++ch)
            {
                pointCloud[i * channels + ch] = depthData[i] * reprojectionData[i * channels + ch];
            }
        }
        return pointCloud;
    }

    // Generic point clound calculus based on C# data structures
    static float[] CalculatePointCloudNotOptimized(ReprojectionMap32f reprojection, DepthMap32f depth)
    {
        const int channels = 3;
        int size = channels * depth.Size.Height * depth.Size.Width;
        float[] pointCloud = new float[size];
        for (int i = 0; i < depth.Size.Height; ++i)
        {
            for (int j = 0; j < depth.Size.Width; ++j)
            {
                int index = channels * (i * depth.Size.Width + j);
                pointCloud[index + 0] = depth[i, j] * reprojection[i, j].x;
                pointCloud[index + 1] = depth[i, j] * reprojection[i, j].y;
                pointCloud[index + 2] = depth[i, j] * reprojection[i, j].z;
            }
        }
        return pointCloud;
    }
}




