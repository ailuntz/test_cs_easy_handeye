using pho.api.csharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    public class PhoXiExamples
    {
        private PhoXiDeviceInformation[] _deviceList;
        private PhoXi _phoXiDevice;
        private PhoXiFactory _factory;
        private Frame _sampleFrame;
        private const string FileCameraFolder = @"";
        private const string OutputFolder = @"";

        public void GetAvailableDevicesExample()
        {
            _factory = new PhoXiFactory();
            //Wait for the PhoXi Control
            while (!_factory.isPhoXiControlRunning())
            {
                System.Threading.Thread.Sleep(100);
            }

            Console.WriteLine("PhoXi Control version: {0}\n", _factory.GetPhoXiControlVersion());
            Console.WriteLine("PhoXi API version: {0}\n", _factory.GetAPIVersion());

            _deviceList = _factory.GetDeviceList();
            Console.WriteLine("PhoXi Factory found {0}  devices by GetDeviceList call.\n", _deviceList.Length);
            for (var i = 0; i < _deviceList.Length; i++)
            {
                Console.WriteLine("Device: {0}", i);
                Console.WriteLine("  Name:                    " + _deviceList[i].Name);
                Console.WriteLine("  Hardware Identification: " + _deviceList[i].HWIdentification);
                Console.WriteLine("  Type:                    " + _deviceList[i].Type);
                Console.WriteLine("  Firmware version:        " + _deviceList[i].FirmwareVersion);
                Console.WriteLine("  Variant:                 " + _deviceList[i].Variant);
                Console.WriteLine("  IsFileCamera:            " + (_deviceList[i].IsFileCamera ? "Yes" : "No"));
                Console.WriteLine("  Feaure-Alpha:            " + (_deviceList[i].CheckFeature("Alpha") ? "Yes" : "No"));
                Console.WriteLine("  Feaure-Color:            " + (_deviceList[i].CheckFeature("Color") ? "Yes" : "No"));
                Console.WriteLine("  Status:                  " +
                                  (_deviceList[i].Status.Attached
                                      ? "Attached to PhoXi Control. "
                                      : "Not Attached to PhoXi Control. ") +
                                  (_deviceList[i].Status.Ready ? "Ready to connect" : "Occupied") + "\n");
            }
        }

        public void ConnectPhoXiDeviceExample()
        {
            //You can connect to any device connected to local network (with compatible ip4 settings)
            //The connection can be made in multiple ways
            while (true)
            {
                Console.WriteLine("Please enter the number of the way to connect to your device from this possibilities:");
                Console.WriteLine("  1. Connect by Hardware Identification Number");
                Console.WriteLine("  2. Connect by Index listed from GetDeviceList call");
                Console.WriteLine("  3. Connect first device Attached to PhoXi Control - if Any");
                Console.WriteLine("  4. Connect to file camera in folder: {0}\n", FileCameraFolder);
                Console.WriteLine("  5. Refresh GetDeviceList\n");
                Console.Write("Please enter the choice: ");

                var consoleLine = Console.ReadLine();
                var index = 0;
                if (!int.TryParse(consoleLine, out index))
                {
                    Console.WriteLine("Incorrect input!");
                    continue;
                }

                switch (index)
                {
                    case 1:
                        ConnectPhoXiDeviceBySerialExample();
                        break;
                    case 2:
                        ConnectPhoXiDeviceByPhoXiDeviceInformationEntryExample();
                        break;
                    case 3:
                        ConnectFirstAttachedPhoXiDeviceExample();
                        break;
                    case 4:
                        ConnectPhoXiFileCameraExample();
                        break;
                    case 5:
                        GetAvailableDevicesExample();
                        break;
                    default:
                        continue;
                }

                if (_phoXiDevice != null && _phoXiDevice.isConnected())
                {
                    Console.WriteLine("You are connected to {0} with Hardware Identification {1}",
                        Enum.GetName(typeof(PhoXiDeviceType.Value), (int)_phoXiDevice.GetType()),
                        _phoXiDevice.HardwareIdentification);
                    break;
                }
            }
        }

        public void ConnectPhoXiDeviceBySerialExample()
        {
            Console.Write("\nPlease enter the Hardware Identification Number: ");
            var hardwareIdentification = Console.ReadLine();
            PhoXiTimeout timeout = PhoXiTimeout.Value.ZeroTimeout;
            _phoXiDevice = _factory.CreateAndConnect(hardwareIdentification, timeout);
            if (_phoXiDevice)
            {
                Console.WriteLine("Connection to the device " + hardwareIdentification + " was Successful!");
            }
            else
            {
                Console.WriteLine("Connection to the device " + hardwareIdentification + " was Unsuccessful!");
            }
        }

        public void ConnectPhoXiDeviceByPhoXiDeviceInformationEntryExample()
        {
            Console.Write("\nPlease enter the Index listed from GetDeviceList call: ");
            var consoleLine = Console.ReadLine();
            var index = 0;

            if (!int.TryParse(consoleLine, out index))
            {
                Console.WriteLine("Incorrect input!");
                return;
            }

            if (index >= _deviceList.Length)
            {
                Console.WriteLine("Bad Index, or not number!");
                return;
            }

            _phoXiDevice = _factory.Create(_deviceList[index]);
            if (_phoXiDevice == null)
            {
                Console.WriteLine("Device {0} was not created", _deviceList[index].HWIdentification);
                return;
            }

            if (_phoXiDevice.Connect())
            {
                Console.WriteLine("Connection to the device " + _deviceList[index].HWIdentification +
                                  " was Successful!");
            }
            else
            {
                Console.WriteLine("Connection to the device " + _deviceList[index].HWIdentification +
                                  " was Unsuccessful!");
            }
        }

        public void ConnectFirstAttachedPhoXiDeviceExample()
        {
            _phoXiDevice = _factory.CreateAndConnectFirstAttached();
            if (_phoXiDevice != null)
            {
                Console.WriteLine("Connection to the device " + _phoXiDevice.HardwareIdentification + " was Successful!");
            }
            else
            {
                Console.WriteLine("There is no attached device, or the device is not ready!");
            }
        }

        public void ConnectPhoXiFileCameraExample()
        {
            var prawFolder = new[] { FileCameraFolder };
            const string name = "TestFileCamera";
            var fileCameraName = _factory.AttachFileCamera(name, prawFolder);

            if (fileCameraName == string.Empty)
            {
                Console.WriteLine("Could not create file camera! Check whether praw files are in the specified folder: {0}", prawFolder[0]);
                return;
            }

            _phoXiDevice = _factory.CreateAndConnect(fileCameraName, PhoXiTimeout.Value.Infinity);
            if (_phoXiDevice.isConnected())
            {
                Console.WriteLine("Connection to the device {0} was Successful!", _phoXiDevice.HardwareIdentification);
                // In file camera you can't change settings thus we stop the program flow
                CorrectDisconnectExample();
            }
            else
            {
                Console.WriteLine("There is no attached device, or the device is not ready!");
            }
        }

        public void BasicDeviceStateExample()
        {
            //Check if the device is connected
            if (_phoXiDevice == null || !_phoXiDevice.isConnected())
            {
                Console.WriteLine("Device is not created or not connected!");
                return;
            }

            Console.WriteLine("  Status:");
            Console.WriteLine("    " + (_phoXiDevice.isConnected()
                                  ? "Device is connected"
                                  : "Device is not connected (Error)"));
            Console.WriteLine("    " + (_phoXiDevice.isAcquiring()
                                  ? "Device is in acquisition mode"
                                  : "Device is not in acquisition mode"));

            //We will go trough all current Device features
            //You can ask the feature if it is implemented and if it is possible to Get or Set the feature value

            // HardwareIdentification
            if (_phoXiDevice.HardwareIdentificationFeature.isEnabled() &&
                _phoXiDevice.HardwareIdentificationFeature.CanGet())
            {
                var hwIdentification = _phoXiDevice.HardwareIdentification;
                if (!_phoXiDevice.HardwareIdentificationFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.HardwareIdentificationFeature.GetLastErrorMessage());

                Console.WriteLine("HardwareIdentification: {0}", hwIdentification);
            }

            //PhoXiCapturingMode
            if (_phoXiDevice.CapturingModeFeature.isEnabled() && _phoXiDevice.CapturingModeFeature.CanGet())
            {
                var capturingMode = _phoXiDevice.CapturingMode;
                //You can ask the feature, if the last performed operation was successful
                if (!_phoXiDevice.CapturingModeFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.CapturingModeFeature.GetLastErrorMessage());

                var resolution = capturingMode.Resolution;
                //you can also access the resolution by PhoXiDevice.Resolution;
                Console.WriteLine("  CapturingMode: ");
                Console.WriteLine("    Resolution:");
                Console.WriteLine("      Width: {0}", resolution.Width);
                Console.WriteLine("      Height: {0}",
                    _phoXiDevice.Resolution.Height /*You can also directly access the value inside*/);
            }

            //PhoXiCapturingModes
            if (_phoXiDevice.SupportedCapturingModesFeature.isEnabled() &&
                _phoXiDevice.SupportedCapturingModesFeature.CanGet())
            {
                var supportedCapturingModes = _phoXiDevice.SupportedCapturingModes;
                if (!_phoXiDevice.SupportedCapturingModesFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.SupportedCapturingModesFeature.GetLastErrorMessage());

                Console.WriteLine("SupportedCapturingModes: ");
                foreach (var mode in supportedCapturingModes)
                {
                    Console.WriteLine("    Resolution:");
                    Console.WriteLine("      Width: {0}", mode.Resolution.Width);
                    Console.WriteLine("      Height: {0}", mode.Resolution.Height);
                }
            }

            //PhoXiTriggerMode
            if (_phoXiDevice.TriggerModeFeature.isEnabled() && _phoXiDevice.TriggerModeFeature.CanGet())
            {
                var triggerMode = _phoXiDevice.TriggerMode;
                if (!_phoXiDevice.TriggerModeFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.TriggerModeFeature.GetLastErrorMessage());

                Console.WriteLine("  TriggerMode: " + triggerMode);
            }

            //PhoXiTimeout
            if (_phoXiDevice.TimeoutFeature.isEnabled() && _phoXiDevice.TimeoutFeature.CanGet())
            {
                var timeout = _phoXiDevice.Timeout;
                if (!_phoXiDevice.TimeoutFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.TimeoutFeature.GetLastErrorMessage());

                Console.WriteLine("  Timeout: " + timeout);
            }

            //PhoXiCapturingSettings
            if (_phoXiDevice.CapturingSettingsFeature.isEnabled() && _phoXiDevice.CapturingSettingsFeature.CanGet())
            {
                var capturingSettings = _phoXiDevice.CapturingSettings;
                if (!_phoXiDevice.CapturingSettingsFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.CapturingSettingsFeature.GetLastErrorMessage());

                PrintCapturingSettings(capturingSettings);
            }

            //PhoXiProcessingSettings
            if (_phoXiDevice.ProcessingSettingsFeature.isEnabled() && _phoXiDevice.ProcessingSettingsFeature.CanGet())
            {
                var processingSettings = _phoXiDevice.ProcessingSettings;
                if (!_phoXiDevice.ProcessingSettingsFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.ProcessingSettingsFeature.GetLastErrorMessage());

                PrintProcessingSettings(processingSettings);
            }

            //PhoXiCoordinatesSettings
            if (_phoXiDevice.CoordinatesSettingsFeature.isEnabled() && _phoXiDevice.CoordinatesSettingsFeature.CanGet())
            {
                var coordinatesSettings = _phoXiDevice.CoordinatesSettings;
                if (!_phoXiDevice.CoordinatesSettingsFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.CoordinatesSettingsFeature.GetLastErrorMessage());

                PrintCoordinatesSettings(coordinatesSettings);
            }

            //PhoXiCalibrationSettings
            if (_phoXiDevice.CalibrationSettingsFeature.isEnabled() && _phoXiDevice.CalibrationSettingsFeature.CanGet())
            {
                var calibrationSettings = _phoXiDevice.CalibrationSettings;
                if (!_phoXiDevice.CalibrationSettingsFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.CalibrationSettingsFeature.GetLastErrorMessage());

                PrintCalibrationSettings(calibrationSettings, "Projector");
            }

            //PhoXiAdditionalCameraCalibration
            if (_phoXiDevice.ColorCameraCalibrationSettingsFeature.isEnabled() && _phoXiDevice.ColorCameraCalibrationSettingsFeature.CanGet())
            {
                var calibrationSettings = _phoXiDevice.ColorCameraCalibrationSettings;
                if (!_phoXiDevice.ColorCameraCalibrationSettingsFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.ColorCameraCalibrationSettingsFeature.GetLastErrorMessage());

                PrintAdditionalCalibrationSettings(calibrationSettings, "ColorCamera");
            }

            //PhoXiScanningVolume
            if (_phoXiDevice.ScanningVolumeFeature.isEnabled() && _phoXiDevice.ScanningVolumeFeature.CanGet())
            {
                var scanningVolume = _phoXiDevice.ScanningVolume;
                if (!_phoXiDevice.ScanningVolumeFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.ScanningVolumeFeature.GetLastErrorMessage());

                if (_phoXiDevice.GetType() == PhoXiDeviceType.Value.MotionCam3D)
                {
                    // Scanning volume for the motion camera devices is available after triggering the first frame
                    int frameId = _phoXiDevice.TriggerFrame();
                    if (frameId >= 0)
                    {
                        Frame frame = _phoXiDevice.GetSpecificFrame(frameId);
                        frame.Empty();
                    }
                }

                PrintScanningVolume(scanningVolume);
            }

            //FrameOutputSettings
            if (_phoXiDevice.OutputSettingsFeature.isEnabled() && _phoXiDevice.OutputSettingsFeature.CanGet())
            {
                var outputSettings = _phoXiDevice.OutputSettings;
                if (!_phoXiDevice.OutputSettingsFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.OutputSettingsFeature.GetLastErrorMessage());

                Console.WriteLine("  OutputSettings: ");
                Console.WriteLine("    SendConfidenceMap: " + (outputSettings.SendConfidenceMap ? "Yes" : "No"));
                Console.WriteLine("    SendDepthMap: " + (outputSettings.SendDepthMap ? "Yes" : "No"));
                Console.WriteLine("    SendNormalMap: " + (outputSettings.SendNormalMap ? "Yes" : "No"));
                Console.WriteLine("    SendPointCloud: " + (outputSettings.SendPointCloud ? "Yes" : "No"));
                Console.WriteLine("    SendEventMap: " + (outputSettings.SendEventMap ? "Yes" : "No"));
                Console.WriteLine("    SendTexture: " + (outputSettings.SendTexture ? "Yes" : "No"));
                Console.WriteLine("    SendColorCameraImage: " + (outputSettings.SendColorCameraImage ? "Yes" : "No"));
            }

            //PhoXiSize
            if (_phoXiDevice.CameraBinningFeature.isEnabled() && _phoXiDevice.CameraBinningFeature.CanGet())
            {
                var cameraBinning = _phoXiDevice.CameraBinning;
                //You can ask the feature, if the last performed operation was successful
                if (!_phoXiDevice.CameraBinningFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.CameraBinningFeature.GetLastErrorMessage());

                //you can also access the resolution by PhoXiDevice.Resolution;
                Console.WriteLine("  CameraBinning: ");
                Console.WriteLine("    Width: {0}", cameraBinning.Width);
                Console.WriteLine("    Height: {0}",
                    _phoXiDevice.CameraBinning.Height /*You can also directly access the value inside*/);
            }
        }
        public void BasicDeviceInfo()
        {
            //Check if the device is connected
            if (!_phoXiDevice)
            {
                Console.WriteLine("Device is not created!");
                return;
            }

            Console.WriteLine("  Info:");
            Console.WriteLine("    " + (_phoXiDevice.Info().ConnectedToPhoXiControl()
                                 ? "Device is connected"
                                 : "Device is not connected (Error)"));

            Console.WriteLine("    " + "name: " + _phoXiDevice.Info().Name);
            Console.WriteLine("    " + "HWIdentification: " + _phoXiDevice.Info().HWIdentification);
            Console.WriteLine("    " + "FirmwareVersion: " + _phoXiDevice.Info().FirmwareVersion);
            Console.WriteLine("    " + "Variant: " + _phoXiDevice.Info().Variant);
            Console.WriteLine("    " + "Features: " + _phoXiDevice.Info().Features);
            Console.WriteLine("    " + (_phoXiDevice.Info().IsFileCamera
                                 ? "Device is file camera"
                                 : "Device is not file camera"));
            Console.WriteLine("    " + "GetTypeHWIdentification: " + _phoXiDevice.Info().GetTypeHWIdentification());
        }

        public void FreerunExample()
        {
            //Check if the device is connected
            if (!_phoXiDevice || !_phoXiDevice.isConnected())
            {
                Console.WriteLine("Device is not created or not connected!");
                return;
            }
            //If it is not in Freerun mode, we need to switch the modes
            if (_phoXiDevice.TriggerMode != PhoXiTriggerMode.Value.Freerun)
            {
                Console.WriteLine("Device is not in Freerun mode");
                if (_phoXiDevice.isAcquiring())
                {
                    Console.WriteLine("Stopping acquisition");
                    //If the device is in Acquisition mode, we need to stop the acquisition
                    if (!_phoXiDevice.StopAcquisition())
                    {
                        throw new Exception("Error in StopAcquistion");
                    }
                }
                Console.WriteLine("Switching to Freerun mode ");
                //Switching the mode is as easy as assigning of a value, it will call the appropriate calls in the background
                _phoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Freerun;
                //Just check if did everything run smoothly
                if (!_phoXiDevice.TriggerModeFeature.isLastOperationSuccessful()) throw new Exception(_phoXiDevice.TriggerModeFeature.GetLastErrorMessage());
            }
            //Start the device acquisition, if necessary
            if (!_phoXiDevice.isAcquiring())
            {
                if (!_phoXiDevice.StartAcquisition())
                {
                    throw new Exception("Error in StartAcquisition");
                }
            }
            //We can clear the current Acquisition buffer -- This will not clear Frames that arrives to the PC after the Clear command is performed
            var clearedFrames = _phoXiDevice.ClearBuffer();
            Console.WriteLine("{0} were cleared from the cyclic buffer", clearedFrames);

            //While we checked the state of the StartAcquisition call, this check is not necessary, but it is a good practice
            if (!_phoXiDevice.isAcquiring())
            {
                Console.WriteLine("Device is not acquiring");
                return;
            }
            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine("Waiting for frame {0}", i);
                //Get the frame
                var myFrame = _phoXiDevice.GetFrame(/*You can specify Timeout here - default is the Timeout stored in Timeout Feature . Infinity by default*/);
                if (myFrame != null)
                {
                    PrintFrameInfo(myFrame);
                    PrintFrameData(myFrame);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve the MyFrame!");
                }
            }
        }

        public void SoftwareTriggerExample()
        {
            //Check if the device is connected
            if (_phoXiDevice == null || !_phoXiDevice.isConnected())
            {
                Console.WriteLine("Device is not created or not connected!");
                return;
            }

            //If it is not in Software trigger mode, we need to switch the modes
            if (_phoXiDevice.TriggerMode != PhoXiTriggerMode.Value.Software)
            {
                Console.WriteLine("Device is not in Software trigger mode");
                if (_phoXiDevice.isAcquiring())
                {
                    Console.WriteLine("Stopping acquisition");
                    //If the device is in Acquisition mode, we need to stop the acquisition
                    if (!_phoXiDevice.StopAcquisition())
                    {
                        throw new Exception("Error in StopAcquistion");
                    }
                }
                Console.WriteLine("Switching to Software trigger mode ");
                //Switching the mode is as easy as assigning of a value, it will call the appropriate calls in the background
                _phoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Software;
                //Just check if did everything run smoothly
                if (!_phoXiDevice.TriggerModeFeature.isLastOperationSuccessful()) throw new Exception(_phoXiDevice.TriggerModeFeature.GetLastErrorMessage());
            }
            //Start the device acquisition, if necessary
            if (!_phoXiDevice.isAcquiring())
            {
                if (!_phoXiDevice.StartAcquisition())
                {
                    throw new Exception("Error in StartAcquisition");
                }
            }
            //We can clear the current Acquisition buffer -- This will not clear Frames that arrives to the PC after the Clear command is performed
            var clearedFrames = _phoXiDevice.ClearBuffer();
            Console.WriteLine("{0} frames were cleared from the cyclic buffer", clearedFrames);

            //While we checked the state of the StartAcquisition call, this check is not necessary, but it is a good practice
            if (!_phoXiDevice.isAcquiring())
            {
                Console.WriteLine("Device is not acquiring");
                return;
            }

            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine("Triggering the {0}-th frame", i);
                int FrameID = _phoXiDevice.TriggerFrame(/*If false is passed here, the device will reject the frame if it is not ready to be triggered, if true us supplied, it will wait for the trigger*/);
                if (FrameID < 0)
                {
                    //If negative number is returned trigger was unsuccessful
                    Console.WriteLine("Trigger was unsuccessful!, code={0}", FrameID);
                    continue;
                }

                Console.WriteLine("Frame was triggered, Frame Id: {0}", FrameID);
                Console.WriteLine("Waiting for frame {0}", i);
                //Wait for a frame with specific FrameID. There is a possibility, that frame triggered before the trigger will arrive after the trigger call, and will be retrieved before requested frame
                //  Because of this, the TriggerFrame call returns the requested frame ID, so it can than be retrieved from the Frame structure. This call is doing that internally in background
                var myFrame = _phoXiDevice.GetSpecificFrame(FrameID/*, You can specify Timeout here - default is the Timeout stored in Timeout Feature . Infinity by default*/);
                if (myFrame != null)
                {
                    PrintFrameInfo(myFrame);
                    PrintFrameData(myFrame);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve the MyFrame!");
                }
            }
        }

        public void ChangeSettingsExample()
        {
            //Check if the device is connected
            if (_phoXiDevice == null || !_phoXiDevice.isConnected())
            {
                Console.WriteLine("Device is not created or not connected!");
                return;
            }

            //Check if the feature is supported and if it we have required access permissions
            //  These checks are not necessary, these have in mind multiple different devices in the future
            if (!_phoXiDevice.CapturingSettingsFeature.isEnabled() || !_phoXiDevice.CapturingSettingsFeature.CanSet() ||
                !_phoXiDevice.CapturingSettingsFeature.CanGet())
            {
                Console.WriteLine(
                    "Settings used in example are not supported by the Device Hardware, or are Read only on the specific device");
                return;
            }

            Console.WriteLine("Settings change example");

            //For purpose of this example, we will change the trigger mode to Software Trigger, it is not necessary for the exhibition of desired functionality
            if (_phoXiDevice.TriggerMode != PhoXiTriggerMode.Value.Software)
            {
                if (_phoXiDevice.isAcquiring())
                {
                    if (!_phoXiDevice.StopAcquisition())
                    {
                        throw new Exception("Error in StopAcquistion");
                    }
                }

                _phoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Software;
                //Just check if did everything run smoothly
                if (!_phoXiDevice.TriggerModeFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.TriggerModeFeature.GetLastErrorMessage());
            }

            //Start the device acquisition, if necessary
            if (!_phoXiDevice.isAcquiring())
            {
                if (_phoXiDevice.StartAcquisition())
                {
                    throw new Exception("Error in StartAcquisition");
                }
            }

            var currentShutterMultiplier = _phoXiDevice.CapturingSettings.ShutterMultiplier;
            var newCapturingSettings = _phoXiDevice.CapturingSettings;

            newCapturingSettings.ShutterMultiplier = currentShutterMultiplier + 1;
            //To change the setting, just assign a new value
            _phoXiDevice.CapturingSettings = newCapturingSettings;

            //You can check if the operation succeed
            if (!_phoXiDevice.CapturingSettingsFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.CapturingSettingsFeature.GetLastErrorMessage());

            //Get the current Output configuration
            var currentOutputSettings = _phoXiDevice.OutputSettings;
            var newOutputSettings = currentOutputSettings;
            newOutputSettings.SendPointCloud = true;
            newOutputSettings.SendNormalMap = true;
            newOutputSettings.SendDepthMap = true;
            newOutputSettings.SendConfidenceMap = true;
            newOutputSettings.SendTexture = true;
            newOutputSettings.SendEventMap = true;
            newOutputSettings.SendColorCameraImage = true;
            //Send all outputs
            _phoXiDevice.OutputSettings = newOutputSettings;

            //Trigger the frame
            int FrameID = _phoXiDevice.TriggerFrame();
            //Check if the frame was successfully triggered
            if (FrameID < 0)
            {
                string msg = "Software trigger failed! code=" + FrameID.ToString();
                throw new Exception(msg);
            }
            //Retrieve the frame
            var myFrame = _phoXiDevice.GetSpecificFrame(FrameID, PhoXiTimeout.Value.Infinity);
            if (myFrame != null)
            {
                //Save the frame for next example
                _sampleFrame = myFrame;
                Console.WriteLine("Saved scan {0} for data handling example", FrameID);
            }
            else
            {
                Console.WriteLine("Could not save frame {0} for data handling example!", FrameID);
            }

            //Change the setting back
            _phoXiDevice.OutputSettings = currentOutputSettings;
            newCapturingSettings.ShutterMultiplier = currentShutterMultiplier;
            _phoXiDevice.CapturingSettings = newCapturingSettings;

            if (!_phoXiDevice.CapturingSettingsFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.CapturingSettingsFeature.GetLastErrorMessage());

            //Try to change device resolution
            if (_phoXiDevice.SupportedCapturingModesFeature.isEnabled() &&
                _phoXiDevice.SupportedCapturingModesFeature.CanGet() &&
                _phoXiDevice.CapturingModeFeature.isEnabled() &&
                _phoXiDevice.CapturingModeFeature.CanSet() &&
                _phoXiDevice.CapturingModeFeature.CanGet())
            {
                //Retrieve current capturing mode
                var currentCapturingMode = _phoXiDevice.CapturingMode;
                if (!_phoXiDevice.CapturingModeFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.CapturingModeFeature.GetLastErrorMessage());

                //Get all supported modes
                var supportedCapturingModes = _phoXiDevice.SupportedCapturingModes;
                if (!_phoXiDevice.SupportedCapturingModesFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.SupportedCapturingModesFeature.GetLastErrorMessage());

                //Cycle trough all other Supported modes, change the settings and grab a frame
                foreach (var mode in supportedCapturingModes)
                {
                    if (!(mode == currentCapturingMode))
                    {
                        _phoXiDevice.CapturingMode = mode;
                        if (!_phoXiDevice.CapturingModeFeature.isLastOperationSuccessful())
                            throw new Exception(_phoXiDevice.CapturingModeFeature.GetLastErrorMessage());
                        //Trigger Frame
                        FrameID = _phoXiDevice.TriggerFrame();
                        if (FrameID < 0)
                        {
                            string msg = "Software trigger failed! code=" + FrameID.ToString();
                            throw new Exception(msg);
                        }
                        myFrame = _phoXiDevice.GetSpecificFrame(FrameID);
                        if (myFrame)
                        {
                            Console.WriteLine("Arrived Frame Resolution: {0} x {1}", myFrame.GetResolution().Width,
                                myFrame.GetResolution().Height);
                        }
                    }
                }

                //Change the mode back
                _phoXiDevice.CapturingMode = currentCapturingMode;
                if (!_phoXiDevice.CapturingModeFeature.isLastOperationSuccessful())
                    throw new Exception(_phoXiDevice.CapturingModeFeature.GetLastErrorMessage());
            }
        }

        public void ChangeProfileExample()
        {

            Console.WriteLine("\nChange Profile Example");

            //Check if the device is connected
            if (_phoXiDevice == null || !_phoXiDevice.isConnected())
            {
                Console.WriteLine("Device is not created or not connected!");
                return;
            }

            //Check if the feature is supported and if it we have required access permissions
            //  These checks are not necessary, these have in mind multiple different devices in the future
            if ((!_phoXiDevice.ProfilesFeature.isEnabled() || !_phoXiDevice.ProfilesFeature.CanGet()) &&
                (!_phoXiDevice.ActiveProfileFeature.isEnabled() || !_phoXiDevice.ActiveProfileFeature.CanGet() || !_phoXiDevice.ActiveProfileFeature.CanSet()))
            {
                Console.WriteLine(
                    "Settings used in example are not supported by the Device Hardware, or are Read only on the specific device");
                return;
            }

            //Retrieving the current profile
            var actualprofile = _phoXiDevice.ActiveProfile;
            Console.WriteLine("Current profile is the following: " + actualprofile + "\n");

            //Retrieving all profiles on device
            var profiles = _phoXiDevice.Profiles;

            //select profiles with different Name
            var differentProfiles = profiles.Where(profile => profile.Name != actualprofile).ToList();
            //set the profile to first with different name
            if (differentProfiles.Count != 0)
                _phoXiDevice.ActiveProfile = differentProfiles[0].Name;

            //Check if profile has been changed successfully
            if (!_phoXiDevice.ProfilesFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ProfilesFeature.GetLastErrorMessage());
            }

            Console.WriteLine("Changed profile is the following: " + _phoXiDevice.ActiveProfile + "\n");

            _phoXiDevice.ActiveProfile = actualprofile;

            //Check if profile has been changed back successfully
            if (!_phoXiDevice.ProfilesFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.ProfilesFeature.GetLastErrorMessage());

            Console.WriteLine("Changed profile back is the following: " + _phoXiDevice.ActiveProfile + "\n");
        }

        /*unsafe*/
        public void DataHandlingExample()
        {
            //Check if we have SampleFrame Data
            if (_sampleFrame == null || _sampleFrame.Empty() || _sampleFrame.PointCloud.Empty())
            {
                Console.WriteLine("Input frame or point cloud is empty");
                return;
            }

            //We will count the number of measured points
            var measuredPoints = 0;
            PointCloud32f pointCloud = _sampleFrame.PointCloud;
            for (var y = 0; y < pointCloud.Size.Height; y++)
            {
                for (var x = 0; x < pointCloud.Size.Width; x++)
                {
                    var currentPoint = pointCloud[y, x];
                    if (Math.Abs(currentPoint.x) < 0.01 && Math.Abs(currentPoint.y) < 0.01 &&
                        Math.Abs(currentPoint.z) < 0.01)
                    {
                        measuredPoints++;
                    }
                }
            }

            Console.WriteLine("Your sample Point cloud has {0} measured points", measuredPoints);
            //You can get data copy . it will reorganize the data into [X, X, ..., X][Y, Y, ..., Y][Z, Z, ..., Z] format
            //var pointCloudCopy = _sampleFrame.PointCloud.GetDataCopy();

            /* You can use this access if defined as unsafe and compiled in with /unsafe
                //or you can get direct memory pointer in native data structure [XYZ, XYZ, ..., XYZ]
                float* RawPointer = SampleFrame.PointCloud.GetFloatPtr();
            */

            //Data from SampleFrame, or all other frames that are returned by the device are copied from the Cyclic buffer and will remain in the memory until the Frame will go out of scope
            //You can specifically call SampleFrame.PointCloud.Clear() to release some of the data

            //You can store the Frame as a ply structure
            //If you don't specify Output folder the PLY file will be saved where FullAPIExample_CSharp.exe is
            var outputFolder = OutputFolder == string.Empty ? string.Empty : OutputFolder + "\\";
            var sampleFramePly = outputFolder + "SampleFrame.ply";
            if (_sampleFrame.SaveAsPly(sampleFramePly, true, true))
            {
                Console.WriteLine("Saved sample frame as PLY to: {0}", sampleFramePly);
            }
            else
            {
                Console.WriteLine("Could not save sample frame as PLY to {0} !", sampleFramePly);
            }
            //You can save scans to any format, you only need to specify path + file name
            //API will look at extension and save the scan in the correct format
            //You can define which options to save (PointCloud, DepthMap, ...) in PhoXi Control application -> Saving options
            //Override of this method has a 2nd parameter: FrameId
            //Use this option to save other scans than the last one
            //Absolute path is prefered
            //If you don't specify Output folder the file will be saved to %APPDATA%\PhotoneoPhoXiControl\ folder
            //Next override of this method has a 3rd parameter: JsonOptions
            //Use this options in case of the GUI set of options are not suitable.
            var sampleFrameTiffFormat = outputFolder + "OtherSampleFrame.tif";
            var tiffExtension = Path.GetExtension(sampleFrameTiffFormat);
            if (_phoXiDevice.SaveLastOutput(sampleFrameTiffFormat))
            {
                Console.WriteLine("Saved sample frame as {0} to: {1}", tiffExtension, sampleFrameTiffFormat);
            }
            else
            {
                Console.WriteLine("Could not save sample frame as {0} to {1} !", tiffExtension, sampleFrameTiffFormat);
            }
            // Overide saving options
            var sampleFramePrawFormat = outputFolder + "OtherSampleFrame.praw";
            var prawExtension = Path.GetExtension(sampleFramePrawFormat);
            String jsonOptions = "{ \"UseCompression\": true }";
            if (_phoXiDevice.SaveLastOutput(sampleFramePrawFormat, -1, jsonOptions))
            {
                Console.WriteLine("Saved sample frame as {0} to: {1}", prawExtension, sampleFramePrawFormat);
            }
            else
            {
                Console.WriteLine("Could not save sample frame as {0} to {1} !", prawExtension, sampleFramePrawFormat);
            }
        }
        public void CorrectDisconnectExample()
        {
            //The whole API is designed on C++ standards, using smart pointers and constructor/destructor logic
            //All resources will be closed automatically, but the device state will not be affected. it will remain connected in PhoXi Control and if in freerun, it will remain Scanning
            //To Stop the device, just
            _phoXiDevice.StopAcquisition();
            //If you want to disconnect and logout the device from PhoXi Control, so it will then be available for other devices, call
            Console.Write("Do you want to logout the device? Enter 0 for no, enter 1 for yes: ");
            var consoleLine = Console.ReadLine();
            var entry = 0;
            if (!int.TryParse(consoleLine, out entry)) return;
            _phoXiDevice.Disconnect(entry == 1);
            //The call PhoXiDevice without Logout will be called automatically by destructor
        }

        public void PrintFrameInfo(Frame frame)
        {
            var frameInfo = frame.Info;
            Console.WriteLine("  Frame params: ");
            Console.WriteLine("    Frame Index: {0}", frameInfo.FrameIndex);
            Console.WriteLine("    Frame Timestamp: {0} s", frameInfo.FrameTimestamp);
            Console.WriteLine("    Frame Acquisition duration: {0} ms", frameInfo.FrameDuration);
            Console.WriteLine("    Frame Computation duration: {0} ms", frameInfo.FrameComputationDuration);
            Console.WriteLine("    Frame Transfer duration: {0} ms", frameInfo.FrameTransferDuration);
            Console.WriteLine("    Sensor Position: [{0}; {1}; {2}]",
                (double)frameInfo.SensorPosition.x,
                (double)frameInfo.SensorPosition.y,
                (double)frameInfo.SensorPosition.z);
            Console.WriteLine("    Total scan count: {0}", frameInfo.TotalScanCount);
        }

        public void PrintFrameData(Frame frame)
        {
            if (frame.Empty())
            {
                Console.WriteLine("Frame is empty");
                return;
            }

            Console.WriteLine("  Frame data: ");
            if (!frame.PointCloud.Empty())
            {
                Console.WriteLine("    PointCloud:    ({0} x {1}) Type: {2}",
                    frame.PointCloud.Size.Width,
                    frame.PointCloud.Size.Height,
                    PointCloud32f.GetElementName());
            }

            if (!frame.NormalMap.Empty())
            {
                Console.WriteLine("    NormalMap:     ({0} x {1}) Type: {2}",
                    frame.NormalMap.Size.Width,
                    frame.NormalMap.Size.Height,
                    NormalMap32f.GetElementName());
            }

            if (!frame.DepthMap.Empty())
            {
                Console.WriteLine("    DepthMap:     ({0} x {1}) Type: {2}",
                    frame.DepthMap.Size.Width,
                    frame.DepthMap.Size.Height,
                    DepthMap32f.GetElementName());
            }

            if (!frame.ConfidenceMap.Empty())
            {
                Console.WriteLine("    ConfidenceMap:     ({0} x {1}) Type: {2}",
                    frame.ConfidenceMap.Size.Width,
                    frame.ConfidenceMap.Size.Height,
                    ConfidenceMap32f.GetElementName());
            }

            if (!frame.EventMap.Empty())
            {
                Console.WriteLine("    EventMap:     ({0} x {1}) Type: {2}",
                    frame.EventMap.Size.Width,
                    frame.EventMap.Size.Height,
                    EventMap32f.GetElementName());
            }

            if (!frame.Texture.Empty())
            {
                Console.WriteLine("    Texture:     ({0} x {1}) Type: {2}",
                    frame.Texture.Size.Width,
                    frame.Texture.Size.Height,
                    Texture32f.GetElementName());
            }

            if (!frame.TextureRGB.Empty())
            {
                Console.WriteLine("    TextureRGB:     ({0} x {1}) Type: {2}",
                    frame.TextureRGB.Size.Width,
                    frame.TextureRGB.Size.Height,
                    Texture32f.GetElementName());
            }

            if (!frame.ColorCameraImage.Empty())
            {
                Console.WriteLine("    ColorCameraImage:     ({0} x {1}) Type: {2}",
                    frame.ColorCameraImage.Size.Width,
                    frame.ColorCameraImage.Size.Height,
                    Texture32f.GetElementName());
            }
        }

        public void PrintCapturingSettings(PhoXiCapturingSettings capturingSettings)
        {
            Console.WriteLine("  CapturingSettings: ");
            Console.WriteLine("    ShutterMultiplier: {0}", capturingSettings.ShutterMultiplier);
            Console.WriteLine("    ScanMultiplier: {0}", capturingSettings.ScanMultiplier);
            Console.WriteLine("    CameraOnlyMode: {0}", capturingSettings.CameraOnlyMode);
            Console.WriteLine("    AmbientLightSuppression: {0}", capturingSettings.AmbientLightSuppression);
            Console.WriteLine("    CodingStrategy: {0}",
                Enum.GetName(typeof(PhoXiCodingStrategy.Value), (int)capturingSettings.CodingStrategy));
            Console.WriteLine("    CodingQuality: {0}",
                Enum.GetName(typeof(PhoXiCodingQuality.Value), (int)capturingSettings.CodingQuality));
            Console.WriteLine("    TextureSource: {0}",
                Enum.GetName(typeof(PhoXiTextureSource.Value), (int)capturingSettings.TextureSource));
            Console.WriteLine("    SinglePatternExposure: {0}", capturingSettings.SinglePatternExposure);
            Console.WriteLine("    MaximumFPS: {0}", capturingSettings.MaximumFPS);
            Console.WriteLine("    LaserPower: {0}", capturingSettings.LaserPower);
            Console.WriteLine("    LEDPower: {0}", capturingSettings.LEDPower);
            Console.WriteLine("    ProjectionOffsetLeft: {0}", capturingSettings.ProjectionOffsetLeft);
            Console.WriteLine("    ProjectionOffsetRight: {0}", capturingSettings.ProjectionOffsetLeft);
            Console.WriteLine("    HardwareTrigger: {0}", capturingSettings.HardwareTrigger);
            Console.WriteLine("    HardwareTriggerSignal: {0}",
                Enum.GetName(typeof(PhoXiHardwareTriggerSignal.Value), (int)capturingSettings.HardwareTriggerSignal));
        }

        public void PrintProcessingSettings(PhoXiProcessingSettings processingSettings)
        {
            Console.WriteLine("  ProcessingSettings: ");
            Console.WriteLine("    Confidence (MaxInaccuracy): {0}", processingSettings.Confidence);
            Console.WriteLine("    CalibrationVolumeOnly: {0}", processingSettings.CalibrationVolumeOnly);
            PrintVector("    MinCameraSpace(in DataCutting)", processingSettings.ROI3D.CameraSpace.min);
            PrintVector("    MaxCameraSpace(in DataCutting)", processingSettings.ROI3D.CameraSpace.max);
            PrintVector("    MinPointCloudSpace (in DataCutting)", processingSettings.ROI3D.PointCloudSpace.min);
            PrintVector("    MaxPointCloudSpace (in DataCutting)", processingSettings.ROI3D.PointCloudSpace.max);
            Console.WriteLine("    MaxCameraAngle: {0}", processingSettings.NormalAngle.MaxCameraAngle);
            Console.WriteLine("    MaxProjectionAngle: {0}", processingSettings.NormalAngle.MaxProjectorAngle);
            Console.WriteLine("    MinHalfwayAngle: {0}", processingSettings.NormalAngle.MinHalfwayAngle);
            Console.WriteLine("    MaxHalfwayAngle: {0}", processingSettings.NormalAngle.MaxHalfwayAngle);
            Console.WriteLine("    SurfaceSmoothness: {0}",
                Enum.GetName(typeof(PhoXiSurfaceSmoothness.Value), (int)processingSettings.SurfaceSmoothness));
            Console.WriteLine("    NormalsEstimationRadius: {0}", processingSettings.NormalsEstimationRadius);
            Console.WriteLine("    InterreflectionsFiltering: {0}", processingSettings.InterreflectionsFiltering);
        }

        public void PrintCoordinatesSettings(PhoXiCoordinatesSettings coordinatesSettings)
        {
            Console.WriteLine("  CoordinatesSettings: ");
            PrintMatrix("    CustomRotationMatrix", coordinatesSettings.CustomTransformation.Rotation);
            PrintVector("    CustomTranslationVector", coordinatesSettings.CustomTransformation.Translation);
            PrintMatrix("    RobotRotationMatrix", coordinatesSettings.CustomTransformation.Rotation);
            PrintVector("    RobotTranslationVector", coordinatesSettings.RobotTransformation.Translation);
            Console.WriteLine("    CoordinateSpace: {0}",
                Enum.GetName(typeof(PhoXiCoordinateSpace.Value), (int)coordinatesSettings.CoordinateSpace));
            Console.WriteLine("    RecognizeMarkers: {0}", coordinatesSettings.RecognizeMarkers);
            Console.WriteLine("    MarkerScale: {0} x {1}",
                coordinatesSettings.MarkersSettings.MarkerScale.Width,
                coordinatesSettings.MarkersSettings.MarkerScale.Height);
        }

        public void PrintCalibrationSettings(PhoXiCalibrationSettings calibrationSettings, string source)
        {
            Console.WriteLine("Source: {0}", source);
            Console.WriteLine("  CalibrationSettings: ");
            Console.WriteLine("    FocusLength: {0}", calibrationSettings.FocusLength);
            Console.WriteLine("    PixelSize: {0} x {1}",
                calibrationSettings.PixelSize.Width,
                calibrationSettings.PixelSize.Height);
            PrintMatrix("    CameraMatrix", calibrationSettings.CameraMatrix);
            Console.WriteLine("    DistortionCoefficients: ");
            Console.WriteLine("      Format is the following: ");
            Console.WriteLine("      (k1, k2, p1, p2[, k3[, k4, k5, k6[, s1, s2, s3, s4[, tx, ty]]]])");

            var distCoeffs = calibrationSettings.GetDistortionCoefficients();
            if (distCoeffs.Length == 0)
            {
                Console.WriteLine("Distortion coefficients are empty");
            }
            else
            {

                var currentDistCoeffs = "(" + distCoeffs[0];
                var brackets = 0;
                for (var i = 1; i < distCoeffs.Length; ++i)
                {
                    if (i == 4 || i == 5 || i == 8 || i == 12 || i == 14)
                    {
                        currentDistCoeffs += "[";
                        ++brackets;
                    }

                    currentDistCoeffs += ", " + distCoeffs[i];
                }

                for (var j = 0; j < brackets; ++j)
                {
                    currentDistCoeffs += "]";
                }

                currentDistCoeffs += ")";
                Console.WriteLine("      {0}", currentDistCoeffs);
            }
        }

        public void PrintCoordinateTransformation(PhoXiCoordinateTransformation transformation)
        {
            PrintMatrix("RotationMatrix", transformation.Rotation);
            PrintVector("TranslationVector", transformation.Translation);
        }

        public void PrintAdditionalCalibrationSettings(PhoXiAdditionalCameraCalibration calibrationSettings, string source)
        {
            Console.WriteLine("\nAdditional camera calibration settings: {0}", source);
            PrintCalibrationSettings(calibrationSettings.CalibrationSettings, source);
            PrintResolution(calibrationSettings.CameraResolution);
            PrintCoordinateTransformation(calibrationSettings.CoordinateTransformation);
        }

        public void PrintResolution(PhoXiSize resolution)
        {
            Console.WriteLine("    Resolution: ({0}x{1})",
                resolution.Width,
                resolution.Height);
        }

        public void PrintScanningVolume(PhoXiScanningVolume scanningVolume)
        {
            Console.WriteLine("  ScanningVolume: ");
            Console.WriteLine("    CuttingPlanes: ");
            var cuttingPlanes = scanningVolume.CuttingPlanes;
            for (var i = 0; i < cuttingPlanes.Length; ++i)
            {
                Console.WriteLine("      Plane[{0}]: ", i);
                PrintVector("        normal = ", cuttingPlanes[i].normal);
                Console.WriteLine("        d = {0}", cuttingPlanes[i].d);
            }

            Console.WriteLine("    ProjectionGeometry: ");
            var projectionGeometry = scanningVolume.ProjectionGeometry;
            PrintVector("      Origin = ", projectionGeometry.Origin);
            PrintVector("      TopLeftTangentialVector = ", projectionGeometry.TopLeftTangentialVector);
            PrintVector("      TopRightTangentialVector = ", projectionGeometry.TopRightTangentialVector);
            PrintVector("      BottomLeftTangentialVector = ", projectionGeometry.BottomLeftTangentialVector);
            PrintVector("      BottomRightTangentialVector = ", projectionGeometry.BottomRightTangentialVector);
            Console.WriteLine("      TopContourPoints: ");
            for (var i = 0; i < projectionGeometry.TopContourPoints.Length; ++i)
            {
                PrintVector("        [" + i + "] = ", projectionGeometry.TopContourPoints[i]);
            }
            Console.WriteLine("      BottomContourPoints: ");
            for (var i = 0; i < projectionGeometry.BottomContourPoints.Length; ++i)
            {
                PrintVector("        [" + i + "] = ", projectionGeometry.BottomContourPoints[i]);
            }

            var mesh = scanningVolume.Mesh;
            if (mesh.IsValid())
            {
                Console.WriteLine("    Scanning volume: ");
                Console.WriteLine("      Count of cross sections: {0}", mesh.Vertices.Length / mesh.PointsPerSection);
                Console.WriteLine("      Count of point in section:  {0}", mesh.PointsPerSection);
                Console.WriteLine("      Vertices:");
                for (var i = 0; i < mesh.Vertices.Length; ++i)
                {
                    PrintVector("        [" + i + "] = ", mesh.Vertices[i]);
                }
                Console.WriteLine("      Indices:");
                for (var i = 0; i < mesh.Indices.Length; ++i)
                {
                    PrintVector("        [" + i + "] = ", mesh.Vertices[mesh.Indices[i]]);
                }
            }
        }

        public void PrintVector(string name, Point3_64f vector)
        {
            Console.WriteLine("{0}: [{1}; {2}; {3}]",
                name,
                (double)vector.x,
                (double)vector.y,
                (double)vector.z);
        }

        public void PrintMatrix(string name, CameraMatrix64f matrix)
        {
            if (matrix.Empty())
            {
                Console.WriteLine("{0}: [empty]", name);
            }
            else
            {
                Console.WriteLine("{0}: ", name);
                Console.WriteLine("      [{0}, {1}, {2}",
                    (double)matrix[0, 0],
                    (double)matrix[0, 1],
                    (double)matrix[0, 2]);
                Console.WriteLine("      [{0}, {1}, {2}",
                    (double)matrix[1, 0],
                    (double)matrix[1, 1],
                    (double)matrix[1, 2]);
                Console.WriteLine("      [{0}, {1}, {2}",
                    (double)matrix[2, 0],
                    (double)matrix[2, 1],
                    (double)matrix[2, 2]);
            }
        }

        public void PrintMatrix(string name, RotationMatrix64f matrix)
        {
            if (matrix.Empty())
            {
                Console.WriteLine("{0}: [empty]", name);
            }
            else
            {
                Console.WriteLine("{0}: ", name);
                Console.WriteLine("      [{0}, {1}, {2}",
                    (double)matrix[0, 0],
                    (double)matrix[0, 1],
                    (double)matrix[0, 2]);
                Console.WriteLine("      [{0}, {1}, {2}",
                    (double)matrix[1, 0],
                    (double)matrix[1, 1],
                    (double)matrix[1, 2]);
                Console.WriteLine("      [{0}, {1}, {2}",
                    (double)matrix[2, 0],
                    (double)matrix[2, 1],
                    (double)matrix[2, 2]);
            }
        }

        public PhoXiExamples()
        {
            try
            {
                GetAvailableDevicesExample();
                ConnectPhoXiDeviceExample();
                BasicDeviceStateExample();
                BasicDeviceInfo();
                FreerunExample();
                SoftwareTriggerExample();
                ChangeSettingsExample();
                ChangeProfileExample();
                DataHandlingExample();
                CorrectDisconnectExample();
            }
            catch (Exception internalException)
            {
                Console.WriteLine(internalException.Message);
            }
        }
    };
    static void Main(string[] args)
    {
        var examples = new PhoXiExamples();
    }
}
