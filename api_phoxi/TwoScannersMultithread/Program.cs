using pho.api.csharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class Program
{
    //for all acquire threads to store frames
    static public List<Frame> Frames;
    //used to sync access to frames
    static public Mutex FramesAccessMutex;
    //notify ProcessFrameThread, that new frame is ready
    static public SemaphoreSlim FrameReady;

    //params for acquire threads
    public class ThreadParams
    {
        public PhoXi Device;
        //check if work is done - notified from ProcessFrameThread
        public bool Stop;
        //controls thread notifies acquire thread
        public SemaphoreSlim Trigger;
    }
    static void Main(string[] args)
    {
        PhoXiFactory Factory = new PhoXiFactory();
        //Check if the PhoXi Control is running
        if (!Factory.isPhoXiControlRunning()) return;
        Console.WriteLine("PhoXi Control is running");

        //Get List of available devices on the network
        PrintDeviceList(Factory.GetDeviceList());

        //Get number of devices you want to connect to
        Console.WriteLine("Enter number of devices you want to use: ");
        int NumberOfDevices = 0;
        try
        {
            NumberOfDevices = Int32.Parse(Console.ReadLine());
        }
        catch
        {
            Console.WriteLine("Can not parse input as string");
            return;
        }
        PhoXi[] PhoXiDevices = new PhoXi[NumberOfDevices];
        for (int i = 0; i < NumberOfDevices; ++i)
        {
            try
            {
                Console.WriteLine("Device {0} : Enter the device hardware identification: ", i);
                PhoXiDevices[i] = Factory.CreateAndConnect(Console.ReadLine(), 10000);//timeout 10 seconds
                if (PhoXiDevices[i] == null)
                {
                    Console.WriteLine("Can not connect to the device, try enter name again");
                    i--;
                }
            }
            catch
            {
                Console.WriteLine("Can not connect to the device");
                return;
            }
        }

        //init static members
        Frames = new List<Frame>();
        FramesAccessMutex = new Mutex();
        FrameReady = new SemaphoreSlim(0);

        //prepare parameters for each thread
        ThreadParams[] threadParams = new ThreadParams[NumberOfDevices];
        SemaphoreSlim[] SemaphoreTriggers = new SemaphoreSlim[NumberOfDevices];
        for (int i = 0; i < NumberOfDevices; ++i)
        {
            threadParams[i] = new ThreadParams();
            SemaphoreTriggers[i] = new SemaphoreSlim(0);
            threadParams[i].Device = PhoXiDevices[i];
            threadParams[i].Stop = false;
            threadParams[i].Trigger = SemaphoreTriggers[i];
        }

        Thread[] AcquireThreads = new Thread[NumberOfDevices];
        int threadIndex = 0;
        //Create acquire threads
        foreach (var ThreadParam in threadParams)
        {
            AcquireThreads[threadIndex++] = new Thread(() => AcquiringThreadFunction(ThreadParam));
        }
        foreach (var Thread in AcquireThreads)
        {
            Thread.Start();
        }
        //Create control thread
        Thread ControlThread = new Thread(() => ControlThreadFunction(threadParams));
        ControlThread.Start();
        //Create process frames thread
        Thread ProcessFrameThread = new Thread(() => ProcessFrameThreadFunction(threadParams));
        ProcessFrameThread.Start();

        //Wait for work is done

        //Join all threads
        ControlThread.Join();
        ProcessFrameThread.Join();
        foreach (var Thread in AcquireThreads)
        {
            Thread.Join();
        }
        //Disconnect all devices
        foreach (var Device in PhoXiDevices)
        {
            Device.Disconnect();
            Device.Dispose();
        }
        Factory.Dispose();
        return;
    }

    public static void ControlThreadFunction(ThreadParams[] Param)
    {
        //Trigger for NumberOfDevices * 5 frames (for example: 10 frames if there are 2 devices)
        for (int i = 0; i < 5; ++i)
        {
            for (int j = 0; j < Param.Length; ++j)
            {
                //Triger frame on device "j"
                Param[j].Trigger.Release();

                //Wait 1000 millisecond between frames
                Thread.Sleep(1000);
            }
        }
    }

    public static void ProcessFrameThreadFunction(ThreadParams[] Param)
    {
        //Work is done when all frames are received and processed
        int NumberOfFrames = Param.Length * 5;
        for (int i = 0; i < NumberOfFrames; ++i)
        {
            //Wait for a frame
            FrameReady.Wait();
            //Lock mutex so no one else can access list of frames
            FramesAccessMutex.WaitOne();
            //Read first frame
            var frame = Frames.First<Frame>();
            //Remove frame from list
            Frames.Remove(frame);
            //Release mutex so other threads can access Frames
            FramesAccessMutex.ReleaseMutex();
            //Process frame
            PhoXiDevice_NewFrameArrived(frame);
        }
        //Notify all threads to stop
        for (int i = 0; i < Param.Length; ++i)
        {
            Param[i].Stop = true;
            Param[i].Trigger.Release();
        }
    }

    public static void AcquiringThreadFunction(ThreadParams Param)
    {
        if (!Param.Device.isAcquiring())
        {
            Param.Device.StartAcquisition();
        }
        //Trigger until notified to stop
        while (!Param.Stop)
        {
            Param.Trigger.Wait();
            //Stop if notified
            if (Param.Stop)
                break;

            //Do not wait for acquisition, nor grabbing end, custom message is device name - fastest way to triger frame
            int i = Param.Device.TriggerFrame(false, false, Param.Device.HardwareIdentification);
            //Check if frame has been trigered sucessfully
            if (i < 0)
            {
                Console.WriteLine("Frame lost on device{0}", Param.Device.HardwareIdentification);
            }
            //Get specific frame with timeout of 5000 milliseconds
            var frame = Param.Device.GetSpecificFrame(i, 5000);
            //Lock mutex to add frame to List
            FramesAccessMutex.WaitOne();
            Frames.Add(frame);
            FramesAccessMutex.ReleaseMutex();
            //Notify that new frame is ready
            FrameReady.Release();
        }
    }
    private static void PhoXiDevice_NewFrameArrived(Frame MyFrame)
    {
        if (MyFrame != null)
        {
            Console.WriteLine("Frame retrieved");
            Console.WriteLine("  Frame params: ");
            Console.WriteLine("    Frame Index: {0}", MyFrame.Info.FrameIndex);
            Console.WriteLine("    Frame Timestamp: {0} ms", MyFrame.Info.FrameTimestamp);
            Console.WriteLine("    Frame Duration: {0} ms", MyFrame.Info.FrameDuration);
            Console.WriteLine("    Frame Resolution: {0} x {1}", MyFrame.GetResolution().Width, MyFrame.GetResolution().Height);
            Console.WriteLine("    Sensor Position: {0}; {1}; {2}", MyFrame.Info.SensorPosition.x, MyFrame.Info.SensorPosition.y, MyFrame.Info.SensorPosition.z);
            Console.WriteLine("    Total scan count: {0}", MyFrame.Info.TotalScanCount);
            Console.WriteLine("    Custom Message: {0}", MyFrame.CustomMessage);
        }
    }

    static void PrintDeviceList(PhoXiDeviceInformation[] DeviceList)
    {
        Console.WriteLine("PhoXi Factory found {0} devices by GetDeviceList call.", DeviceList.Length);
        Console.WriteLine();
        for (int i = 0; i < DeviceList.Length; i++)
        {
            Console.WriteLine("Device: {0}", i);
            Console.WriteLine("  Hardware Identification: " + (String)DeviceList[i].HWIdentification);
            Console.WriteLine("  Name:                    " + (String)DeviceList[i].Name);
            Console.WriteLine("  Type:                    " + (String)DeviceList[i].Type);
            Console.WriteLine("  Firmware version:        " + (String)DeviceList[i].FirmwareVersion);
            Console.WriteLine("  Variant:                 " + (String)DeviceList[i].Variant);
            Console.WriteLine("  IsFileCamera:            " + (DeviceList[i].IsFileCamera ? "Yes" : "No"));
            Console.WriteLine("  Status:                  " + (DeviceList[i].Status.Attached ? "Attached to PhoXi Control. " : "Not Attached to PhoXi Control. ") + (DeviceList[i].Status.Ready ? "Ready to connect" : "Occupied"));
            Console.WriteLine();
        }
    }
}



