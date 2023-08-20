using pho.api.csharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class Program
{
    // static AutoResetEvent stopWaitHandle = new AutoResetEvent(false);
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
            Console.WriteLine("Your device is connected");
            if (PhoXiDevice.isAcquiring())
            {
                PhoXiDevice.StopAcquisition();
            }
            Console.WriteLine("Starting Software trigger mode");
            PhoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Software;
            PhoXiDevice.ClearBuffer();
            PhoXiDevice.StartAcquisition();
            if (PhoXiDevice.isAcquiring())
            {
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("Triggering the {0}-th frame", i);
                    int FrameID = PhoXiDevice.TriggerFrame();
                    if (FrameID < 0)
                    {
                        //If negative number is returned trigger was unsuccessful
                        Console.WriteLine("Trigger was unsuccessful! code={0}", FrameID);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Frame was triggered, Frame Id: {0}", FrameID);
                    }
                    Console.WriteLine("Waiting for frame {0}", i);
                    Frame MyFrame = PhoXiDevice.GetSpecificFrame(FrameID, PhoXiTimeout.Value.Infinity);




                    if (MyFrame != null)
                    {
                        Console.WriteLine("Frame retrieved");
                        Console.WriteLine("  Frame params: ");
                        Console.WriteLine("    Frame Index: {0}", MyFrame.Info.FrameIndex);
                        Console.WriteLine("    Frame Timestamp: {0}", MyFrame.Info.FrameTimestamp);
                        Console.WriteLine("    Frame Duration: {0}", MyFrame.Info.FrameDuration);
                        Console.WriteLine("    Frame Resolution: {0} x {1}", MyFrame.GetResolution().Width, MyFrame.GetResolution().Height);
                        Console.WriteLine("    Sensor Position: {0}; {1}; {2}", MyFrame.Info.SensorPosition.x, MyFrame.Info.SensorPosition.y, MyFrame.Info.SensorPosition.z);
                        Console.WriteLine("    Total scan count: {0}", MyFrame.Info.TotalScanCount);
                        if (!MyFrame.Empty())
                        {
                            Console.WriteLine("  Frame data: ");
                            if (!MyFrame.PointCloud.Empty())
                            {
                                Console.WriteLine("    PointCloud: {0} x {1} Type: {2}", MyFrame.PointCloud.Size.Width, MyFrame.PointCloud.Size.Height, PointCloud32f.GetElementName());
                            }
                            if (!MyFrame.NormalMap.Empty())
                            {
                                Console.WriteLine("    NormalMap: {0} x {1} Type: {2}", MyFrame.NormalMap.Size.Width, MyFrame.NormalMap.Size.Height, NormalMap32f.GetElementName());
                            }
                            if (!MyFrame.DepthMap.Empty())
                            {
                                Console.WriteLine("    DepthMap: {0} x {1} Type: {2}", MyFrame.DepthMap.Size.Width, MyFrame.DepthMap.Size.Height, DepthMap32f.GetElementName());
                            }
                            if (!MyFrame.ConfidenceMap.Empty())
                            {
                                Console.WriteLine("    ConfidenceMap: {0} x {1} Type: {2}", MyFrame.ConfidenceMap.Size.Width, MyFrame.ConfidenceMap.Size.Height, ConfidenceMap32f.GetElementName());
                            }
                            if (!MyFrame.Texture.Empty())
                            {
                                Console.WriteLine("    Texture: {0} x {1} Type: {2}", MyFrame.Texture.Size.Width, MyFrame.Texture.Size.Height, Texture32f.GetElementName());
                            }
                            if (!MyFrame.TextureRGB.Empty())
                            {
                                Console.WriteLine("    TextureRGB: {0} x {1} Type: {2}", MyFrame.TextureRGB.Size.Width, MyFrame.TextureRGB.Size.Height, Texture32f.GetElementName());
                            }
                            if (!MyFrame.ColorCameraImage.Empty())
                            {
                                Console.WriteLine("    ColorCameraImage: {0} x {1} Type: {2}", MyFrame.ColorCameraImage.Size.Width, MyFrame.ColorCameraImage.Size.Height, Texture32f.GetElementName());
                            }
                        }
                        else
                        {
                            Console.WriteLine("Frame is empty.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve the frame!");
                    }
                }
            }

            PhoXiDevice.StopAcquisition();
            Console.WriteLine("Starting Freerun mode");
            PhoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Freerun;
            PhoXiDevice.StartAcquisition();

            if (PhoXiDevice.isAcquiring())
            {
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine("Waiting for frame {0}", i);
                    Frame MyFrame = PhoXiDevice.GetFrame(PhoXiTimeout.Value.Infinity);
                    if (MyFrame != null)
                    {
                        Console.WriteLine("Frame retrieved");
                        Console.WriteLine("  Frame params: ");
                        Console.WriteLine("    Frame Index: {0}", MyFrame.Info.FrameIndex);
                        Console.WriteLine("    Frame Timestamp: {0}", MyFrame.Info.FrameTimestamp);
                        Console.WriteLine("    Frame Duration: {0}", MyFrame.Info.FrameDuration);
                        Console.WriteLine("    Frame Resolution: {0} x {1}", MyFrame.GetResolution().Width, MyFrame.GetResolution().Height);
                        Console.WriteLine("    Sensor Position: {0}; {1}; {2}", MyFrame.Info.SensorPosition.x, MyFrame.Info.SensorPosition.y, MyFrame.Info.SensorPosition.z);
                        Console.WriteLine("    Total scan count: {0}", MyFrame.Info.TotalScanCount);
                        if (!MyFrame.Empty())
                        {
                            Console.WriteLine("  Frame data: ");
                            if (MyFrame.PointCloud.Empty())
                            {
                                Console.WriteLine("    PointCloud: {0} x {1} Type: {2}", MyFrame.PointCloud.Size.Width, MyFrame.PointCloud.Size.Height, PointCloud32f.GetElementName());
                            }
                            if (!MyFrame.NormalMap.Empty())
                            {
                                Console.WriteLine("    NormalMap: {0} x {1} Type: {2}", MyFrame.NormalMap.Size.Width, MyFrame.NormalMap.Size.Height, NormalMap32f.GetElementName());
                            }
                            if (!MyFrame.DepthMap.Empty())
                            {
                                Console.WriteLine("    DepthMap: {0} x {1} Type: {2}", MyFrame.DepthMap.Size.Width, MyFrame.DepthMap.Size.Height, DepthMap32f.GetElementName());
                            }
                            if (!MyFrame.ConfidenceMap.Empty())
                            {
                                Console.WriteLine("    ConfidenceMap: {0} x {1} Type: {2}", MyFrame.ConfidenceMap.Size.Width, MyFrame.ConfidenceMap.Size.Height, ConfidenceMap32f.GetElementName());
                            }
                            if (!MyFrame.Texture.Empty())
                            {
                                Console.WriteLine("    Texture: {0} x {1} Type: {2}", MyFrame.Texture.Size.Width, MyFrame.Texture.Size.Height, Texture32f.GetElementName());
                            }
                            if (!MyFrame.TextureRGB.Empty())
                            {
                                Console.WriteLine("    TextureRGB: {0} x {1} Type: {2}", MyFrame.TextureRGB.Size.Width, MyFrame.TextureRGB.Size.Height, Texture32f.GetElementName());
                            }
                            if (!MyFrame.ColorCameraImage.Empty())
                            {
                                Console.WriteLine("    ColorCameraImage: {0} x {1} Type: {2}", MyFrame.ColorCameraImage.Size.Width, MyFrame.ColorCameraImage.Size.Height, Texture32f.GetElementName());
                            }
                        }
                        else
                        {
                            Console.WriteLine("Frame is empty.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve the frame!");
                    }
                }
            }
            PhoXiDevice.StopAcquisition();

            // Grab frame via asynchronous delegate
            Console.WriteLine("Starting Software trigger mode with async grab");
            PhoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Software;
            PhoXiDevice.StartAcquisition();

            if (PhoXiDevice.isAcquiring())
            {
                PhoXiDevice.AsyncGetFrameEvent += PhoXiDevice_NewFrameArrived;
                PhoXiDevice.EnableAsyncGetFrame();
                int FrameID = PhoXiDevice.TriggerFrame();
                if (FrameID < 0)
                {
                    //If negative number is returned trigger was unsuccessful
                    Console.WriteLine("Trigger was unsuccessful! code={0}", FrameID);
                }
                else
                {
                    //   stopWaitHandle.WaitOne();
                    PhoXiDevice.DisableAsyncGetFrame();
                }
            }
            PhoXiDevice.StopAcquisition();
        }
        PhoXiDevice.Disconnect();

    }

    private static void PhoXiDevice_NewFrameArrived(Frame MyFrame)
    {
        if (MyFrame != null)
        {
            Console.WriteLine("Frame retrieved");
            Console.WriteLine("  Frame params: ");
            Console.WriteLine("    Frame Index: {0}", MyFrame.Info.FrameIndex);
            Console.WriteLine("    Frame Timestamp: {0}", MyFrame.Info.FrameTimestamp);
            Console.WriteLine("    Frame Duration: {0}", MyFrame.Info.FrameDuration);
            Console.WriteLine("    Frame Resolution: {0} x {1}", MyFrame.GetResolution().Width, MyFrame.GetResolution().Height);
            Console.WriteLine("    Sensor Position: {0}; {1}; {2}", MyFrame.Info.SensorPosition.x, MyFrame.Info.SensorPosition.y, MyFrame.Info.SensorPosition.z);
            Console.WriteLine("    Total scan count: {0}", MyFrame.Info.TotalScanCount);
            // stopWaitHandle.Set();
        }
    }
}



