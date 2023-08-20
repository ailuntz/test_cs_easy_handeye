using pho.api.csharp;
using System;

internal class Program
{
    static void printDeviceInfo(ref PhoXiDeviceInformation deviceInfo)
    {
        Console.WriteLine("  Name:                    " + deviceInfo.Name);
        Console.WriteLine("  Hardware Identification: " + deviceInfo.HWIdentification);
        Console.WriteLine("  Type:                    " + deviceInfo.Type);
        Console.WriteLine("  Firmware version:        " + deviceInfo.FirmwareVersion);
        Console.WriteLine("  Variant:                 " + deviceInfo.Variant);
        Console.WriteLine("  IsFileCamera:            " + (deviceInfo.IsFileCamera ? "Yes" : "No"));
        Console.WriteLine("  Feaure-Alpha:            " + (deviceInfo.CheckFeature("Alpha") ? "Yes" : "No"));
        Console.WriteLine("  Feaure-Color:            " + (deviceInfo.CheckFeature("Color") ? "Yes" : "No"));
        Console.WriteLine("  Status:                  " +
                            (deviceInfo.Status.Attached ? "Attached to PhoXi Control. " : "Not Attached to PhoXi Control. ") +
                            (deviceInfo.Status.Ready ? "Ready to connect" : "Occupied") + "\n");
    }

    static void Main(string[] args)
    {
        PhoXiFactory factory = new PhoXiFactory();
        //Wait for the PhoXi Control
        while (!factory.isPhoXiControlRunning())
        {
            System.Threading.Thread.Sleep(100);
        }

        Console.WriteLine("PhoXi Control version: {0}\n", factory.GetPhoXiControlVersion());
        Console.WriteLine("PhoXi API version: {0}\n", factory.GetAPIVersion());

        PhoXiDeviceInformation[] deviceList = factory.GetDeviceList();
        Console.WriteLine("PhoXi Factory found {0}  devices by GetDeviceList call.\n", deviceList.Length);
        for (var i = 0; i < deviceList.Length; i++)
        {
            Console.WriteLine("Device: {0}", i);
            printDeviceInfo(ref deviceList[i]);
        }

        int selectedIndex;
        while (true)
        {
            Console.Write("Please enter device Index from the list: ");
            string consoleLine = Console.ReadLine();
            if (!int.TryParse(consoleLine, out selectedIndex) || selectedIndex >= deviceList.Length)
            {
                Console.WriteLine("Incorrect input!");
                continue;
            }

            break;
        }

        ref var selectedDeviceInformation = ref deviceList[selectedIndex];

        Console.WriteLine("Selected device: " + selectedDeviceInformation.Name);
        Console.WriteLine("Do you want to:");
        Console.WriteLine("  0: Exit application");
        Console.WriteLine("  1: Reboot device");

        int selectedOperation;
        while (true)
        {
            Console.Write("Please enter your choice: ");
            string consoleLine = Console.ReadLine();
            if (!int.TryParse(consoleLine, out selectedOperation) || selectedOperation > 1)
            {
                Console.WriteLine("Incorrect input!");
                continue;
            }

            break;
        }

        switch (selectedOperation)
        {
            default:
            case 0:
                {
                    return;
                }
            case 1:
                {
                    if (selectedDeviceInformation.IsFileCamera)
                    {
                        Console.WriteLine("File camera can not be rebooted!");
                        break;
                    }

                    //Function call does not wait for the device to reboot, reboot command is sent only, which means the device will reboot after some time.
                    if (factory.Reboot(selectedDeviceInformation.HWIdentification))
                    {
                        Console.WriteLine("Reboot command sent to the device {0}. Device will reboot shortly.", selectedDeviceInformation.Name);
                    }
                    else
                    {
                        Console.WriteLine("Failed to send reboot command to the device {0}!", selectedDeviceInformation.Name);
                    }

                    break;
                }
        }

        Console.WriteLine("Press any key to exit...");
        Console.Read();
    }
}
