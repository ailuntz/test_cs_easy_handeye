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
        private PhoXi _phoXiDevice;
        private PhoXiFactory _factory;

        public void ConnectPhoXiDeviceBySerialExample()
        {
            _factory = new PhoXiFactory();
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

        public void ChangeGeneralMotionCamExample()
        {
            Console.WriteLine("\nChange General Settings Example");

            //Retrieving the GeneralSettings
            var currentGeneralSettings = _phoXiDevice.MotionCam;
            //Check if the CurrentGeneralSettings have been retrieved succesfully
            if (!_phoXiDevice.MotionCamFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.MotionCamFeature.GetLastErrorMessage());
            }

            var backupGeneralSettings = currentGeneralSettings;

            PrintMotionCamGeneral(currentGeneralSettings);

            //OperationMode values: Camera / Scanner / Mode2D
            currentGeneralSettings.OperationMode = PhoXiOperationMode.Value.Scanner;

            //LaserPower values: possible from 0 - 4095, recommended 800 - 4095
            currentGeneralSettings.LaserPower = 1650;

            //MaximumFPS values: 0 - 60
            currentGeneralSettings.MaximumFPS = 1.12;

            //HardwareTrigger values: 0 / 1 or false / true (0 is OFF, 1 is ON)
            currentGeneralSettings.HardwareTrigger = true;

            //HardwareTriggerSignal values Falling / Rising / Both
            currentGeneralSettings.HardwareTriggerSignal = PhoXiHardwareTriggerSignal.Value.Falling;

            //Set GeneralSettings to the Device
            _phoXiDevice.MotionCam = currentGeneralSettings;
            //Check if the CurrentGeneralSettings has been set succesfully
            if (!_phoXiDevice.MotionCamFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.MotionCamFeature.GetLastErrorMessage());
            }
            Console.WriteLine("GeneralSettings have been changed to the following: ");
            PrintMotionCamGeneral(currentGeneralSettings);

            //Restore previous values
            _phoXiDevice.MotionCam = backupGeneralSettings;
        }

        public void ChangeMotionCamCameraModeExample()
        {
            Console.WriteLine("\nChange CameraMode Settings Example");

            //Make sure to have CameraMode selected
            var currentGeneralSettings = _phoXiDevice.MotionCam;
            currentGeneralSettings.OperationMode = PhoXiOperationMode.Value.Camera;
            _phoXiDevice.MotionCam = currentGeneralSettings;
            //Check if the CurrentGeneralSettins have been set succesfully
            if (!_phoXiDevice.MotionCamFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.MotionCamFeature.GetLastErrorMessage());
            }

            //Retrieving the Current Resolution from CapturingMode
            var currentResolution = _phoXiDevice.CapturingMode.Resolution;
            //Check if the CurrentResolution has been retrieved succesfully
            if (!_phoXiDevice.CapturingModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CapturingModeFeature.GetLastErrorMessage());
            }

            //Printing the Current Resolution
            Console.WriteLine("Current CameraMode Settings are the following:");
            PrintResolution(currentResolution);

            //Retrieving the CameraMode settings
            var currentCameraMode = _phoXiDevice.MotionCamCameraMode;
            //Check if the MotionCamCameraMode has been retrieved succesfully
            if (!_phoXiDevice.MotionCamCameraModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.MotionCamCameraModeFeature.GetLastErrorMessage());
            }

            var backupCameraMode = currentCameraMode;

            PrintMotionCamCameraMode(currentCameraMode);

            //Get all supported values
            var supportedSinglePatternExposures = _phoXiDevice.SupportedSinglePatternExposures;
            //Check if the SetSupportedSinglePatternExposures have been retrieved succesfully
            if (!_phoXiDevice.SupportedSinglePatternExposuresFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.SupportedSinglePatternExposuresFeature.GetLastErrorMessage());
            }
            currentCameraMode.Exposure = supportedSinglePatternExposures[0];

            //SamplingTopology values: Standard
            currentCameraMode.SamplingTopology = PhoXiSamplingTopology.Value.Standard;

            //OutputTopology values: Raw / Irregular grid / Regular grid
            currentCameraMode.OutputTopology = PhoXiOutputTopology.Value.Raw;

            //CodingStrategy values: Normal / Interreflections
            currentCameraMode.CodingStrategy = PhoXiCodingStrategy.Value.Normal;

            //Set the CameraMode settings to the Device
            _phoXiDevice.MotionCamCameraMode = currentCameraMode;
            //Check if the MotionCamCameraMode has been set succesfully
            if (!_phoXiDevice.MotionCamCameraModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.MotionCamCameraModeFeature.GetLastErrorMessage());
            }

            Console.WriteLine("CameraMode Settings have been changed to the following: ");
            PrintMotionCamCameraMode(currentCameraMode);

            //Restore previous values
            _phoXiDevice.MotionCamCameraMode = backupCameraMode;
        }

        public void ChangeMotionCamScannerModeExample()
        {
            Console.WriteLine("\nChange ScannerMode Settings Example");

            //Make sure to have ScannerMode selected
            var currentGeneralSettings = _phoXiDevice.MotionCam;
            currentGeneralSettings.OperationMode = PhoXiOperationMode.Value.Scanner;
            _phoXiDevice.MotionCam = currentGeneralSettings;
            //Check if the CurrentGeneralSettings have been set succesfully
            if (!_phoXiDevice.MotionCamFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.MotionCamFeature.GetLastErrorMessage());
            }

            //Retrieving the Current Resolution from CapturingMode
            var currentResolution = _phoXiDevice.CapturingMode.Resolution;
            //Check if the CurrentResolution has been retrieved succesfully
            if (!_phoXiDevice.CapturingModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CapturingModeFeature.GetLastErrorMessage());
            }

            //Printing the Current Resolution
            Console.WriteLine("Current ScannerMode Settings are the following:");
            PrintResolution(currentResolution);

            //Retrieving the ScannerMode settings
            var currentScannerMode = _phoXiDevice.MotionCamScannerMode;
            //Check if the MotionCamScannerMode has been retrieved succesfully
            if (!_phoXiDevice.MotionCamScannerModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.MotionCamScannerModeFeature.GetLastErrorMessage());
            }

            var backupScannerMode = currentScannerMode;

            PrintMotionCamScannerMode(currentScannerMode);

            //ShutterMultiplier values: 0-10
            currentScannerMode.ShutterMultiplier = 2;

            //ScanMultiplier values: 0-10
            currentScannerMode.ScanMultiplier = 2;

            //CodingStrategy values: Normal / Interreflections
            currentScannerMode.CodingStrategy = PhoXiCodingStrategy.Value.Normal;

            //CodingQuality values: Fast / High / Ultra
            currentScannerMode.CodingQuality = PhoXiCodingQuality.Value.Fast;

            //TextureSource values: LED / Computed / Laser / Focus
            currentScannerMode.TextureSource = PhoXiTextureSource.Value.LED;

            //Get all supported values
            var supportedSinglePatternExposures = _phoXiDevice.SupportedSinglePatternExposures;
            //Check if the SetSupportedSinglePatternExposures have been retrieved succesfully
            if (!_phoXiDevice.SupportedSinglePatternExposuresFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.SupportedSinglePatternExposuresFeature.GetLastErrorMessage());
            }
            currentScannerMode.Exposure = supportedSinglePatternExposures[0];

            //Set the ScannerMode settings to the Device
            _phoXiDevice.MotionCamScannerMode = currentScannerMode;
            //Check if the MotionCamCameraMode has been set succesfully
            if (!_phoXiDevice.MotionCamScannerModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.MotionCamScannerModeFeature.GetLastErrorMessage());
            }

            Console.WriteLine("ScannerMode Settings have been changed to the following: ");
            PrintMotionCamScannerMode(currentScannerMode);

            //Restore previous values
            _phoXiDevice.MotionCamScannerMode = backupScannerMode;
        }

        public void ChangeMotionCam2DModeExample()
        {
            Console.WriteLine("\nChange 2D Mode Settings Example");

            //Make sure to have Mode2D selected
            var currentGeneralSettings = _phoXiDevice.MotionCam;
            currentGeneralSettings.OperationMode = PhoXiOperationMode.Value.Mode2D;
            _phoXiDevice.MotionCam = currentGeneralSettings;
            //Check if the CurrentGeneralSettings have been set succesfully
            if (!_phoXiDevice.MotionCamFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.MotionCamFeature.GetLastErrorMessage());
            }

            //Retrieving the Current Resolution from CapturingMode
            var currentResolution = _phoXiDevice.CapturingMode.Resolution;
            //Check if the CurrentResolution has been retrieved succesfully
            if (!_phoXiDevice.CapturingModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CapturingModeFeature.GetLastErrorMessage());
            }

            //Printing the Current Resolution
            Console.WriteLine("Current 2D Mode Settings are the following:");
            PrintResolution(currentResolution);
        }

        public void ChangeProcessingSettingsExample()
        {
            Console.WriteLine("\nChange Processing Settings Example");
            //Retrieving the Current Processing Settings
            var currentProcessingSettings = _phoXiDevice.ProcessingSettings;

            //Check if the currentProcessingSettings have been retrieved succesfully
            if (!_phoXiDevice.ProcessingSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ProcessingSettingsFeature.GetLastErrorMessage());
            }

            var backupProcessingSettings = currentProcessingSettings;


            //Printing the Current Processing Settings
            Console.WriteLine("Current Processing Settings are the following: ");
            PrintProcessingSettings(currentProcessingSettings);

            var roi3D = currentProcessingSettings.ROI3D;
            //3D ROI Camera Space min/max
            //These variables are double so any value for a double are acceptable
            //values from -4000mm to 4000mm (maximum range of the PhoXi XL 3D scanner is 3780mm)
            var cameraSpace = roi3D.CameraSpace;
            var csMin = cameraSpace.min;
            csMin.x = 10;
            csMin.y = 10;
            csMin.z = 10;
            cameraSpace.min = csMin;
            var csMax = cameraSpace.max;
            csMax.x = 2000;
            csMax.y = 2000;
            csMax.z = 2000;
            cameraSpace.max = csMax;
            roi3D.CameraSpace = cameraSpace;

            //3D ROI PointCloudSpace min/max
            //These variables are double so any value for a double are acceptable
            //Values from -4000mm to 4000mm (maximum range of the PhoXi XL 3D scanner is 3780mm)
            var pointCloudSpace = roi3D.PointCloudSpace;
            var pcsMin = pointCloudSpace.min;
            pcsMin.x = 10;
            pcsMin.y = 10;
            pcsMin.z = 10;
            pointCloudSpace.min = pcsMin;
            var pcsMax = pointCloudSpace.max;
            pcsMax.x = 2000;
            pcsMax.y = 2000;
            pcsMax.z = 2000;
            pointCloudSpace.max = pcsMax;
            roi3D.PointCloudSpace = pointCloudSpace;
            currentProcessingSettings.ROI3D = roi3D;

            var normalAngle = currentProcessingSettings.NormalAngle;
            //MaxCameraAngle values: 0-90
            normalAngle.MaxCameraAngle = 80;

            //MaxProjectionAngle values: 0-90
            normalAngle.MaxProjectorAngle = 80;

            //MinHalfwayAngle values: 0-90
            normalAngle.MinHalfwayAngle = 10;

            //MaxHalfwayAngle values: 0-90
            normalAngle.MaxHalfwayAngle = 10;
            currentProcessingSettings.NormalAngle = normalAngle;

            //MaxInaccuracy(Confidence) values: 0-100
            currentProcessingSettings.Confidence = 2.0;

            //CalibrationVolumeCut values: 0 / 1 or false / true (0 is OFF, 1 is ON)
            currentProcessingSettings.CalibrationVolumeOnly = false;

            //SurfaceSmoothness values: 1 is Sharp, 2 is Normal, 3 is Smooth
            currentProcessingSettings.SurfaceSmoothness = PhoXiSurfaceSmoothness.Value.Sharp;

            //NormalsEstimationRadius values: 1-4
            currentProcessingSettings.NormalsEstimationRadius = 2;

            //Send settings
            _phoXiDevice.ProcessingSettings = currentProcessingSettings;

            //Check if the CurrentProcessingSettings have been retrieved succesfully
            if (!_phoXiDevice.ProcessingSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ProcessingSettingsFeature.GetLastErrorMessage());
            }
            Console.WriteLine("Processing Settings  have been changed to the following:");
            PrintProcessingSettings(currentProcessingSettings);

            //Restore previous values
            _phoXiDevice.ProcessingSettings = backupProcessingSettings;
        }

        public void ChangeCoordinatesSettingsExample()
        {
            Console.WriteLine("\nChange Coordinates Settings Example");
            //Retrieving the Current Coordinates Settings
            var currentCoordinatesSettings = _phoXiDevice.CoordinatesSettings;

            //Check if the currentCoordinatesSettings have been retrieved succesfully
            if (!_phoXiDevice.CoordinatesSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CoordinatesSettingsFeature.GetLastErrorMessage());
            }

            var backupCoordinatesSettings = currentCoordinatesSettings;

            //Printing the Current Coordinates Settings
            Console.WriteLine("Current Coordinates Settings are the following: ");
            PrintCoordinatesSettings(currentCoordinatesSettings);

            var customTransformation = currentCoordinatesSettings.CustomTransformation;
            //Transformation from Camera Space to Mounting Space for PhoXi 3D scanner model L
            var rotation = customTransformation.Rotation;
            rotation[0, 0] = -0.986429;
            rotation[0, 1] = 0;
            rotation[0, 2] = 0.164187;
            rotation[1, 0] = 0;
            rotation[1, 1] = -1;
            rotation[1, 2] = 0;
            rotation[2, 0] = 0.164187;
            rotation[2, 1] = 0;
            rotation[2, 2] = 0.986429;
            customTransformation.Rotation = rotation;

            var translation = customTransformation.Translation;
            translation.x = -271.97;
            translation.y = 29.9;
            translation.z = 36.14;
            customTransformation.Translation = translation;
            currentCoordinatesSettings.CustomTransformation = customTransformation;

            //CoordinateSpace values: 1 is CameraSpace, 2 is MarkerSpace, 3 is RobotSpace, 4 is CustomSpace
            currentCoordinatesSettings.CoordinateSpace = PhoXiCoordinateSpace.Value.CameraSpace;

            //Recognize Markers values: false is OFF, true is ON
            currentCoordinatesSettings.RecognizeMarkers = true;

            //Pattern Scale values: 0.0 - 1.0 (scale 1.0 x 1.0 is normal size)
            var markersSettings = currentCoordinatesSettings.MarkersSettings;
            var markerScale = markersSettings.MarkerScale;
            markerScale.Width = 0.5;
            markerScale.Height = 0.5;
            markersSettings.MarkerScale = markerScale;
            currentCoordinatesSettings.MarkersSettings = markersSettings;

            //Send settings
            _phoXiDevice.CoordinatesSettings = currentCoordinatesSettings;

            //Check if the CurrentCoordinatesSettings have been retrieved succesfully
            if (!_phoXiDevice.CoordinatesSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CoordinatesSettingsFeature.GetLastErrorMessage());
            }
            Console.WriteLine("Changed Coordinates Settings are the following:");
            PrintCoordinatesSettings(currentCoordinatesSettings);

            //Restore previous values
            _phoXiDevice.CoordinatesSettings = backupCoordinatesSettings;
        }

        public void CalibrationSettingsExample()
        {
            //Retrieving the CalibrationSettings
            var calibrationSettings = _phoXiDevice.CalibrationSettings;
            //Check if the current CalibrationSettings have been retrieved succesfully
            if (!_phoXiDevice.CalibrationSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CalibrationSettingsFeature.GetLastErrorMessage());
            }
            PrintCalibrationSettings(calibrationSettings, "Projector");
        }

        public void ColorCameraCalibrationSettingsExample()
        {
            //Retrieving the ColorCameraCalibrationSettings
            var calibrationSettings = _phoXiDevice.ColorCameraCalibrationSettings;
            //Check if the current ColorCameraCalibrationSettings have been retrieved succesfully
            if (!_phoXiDevice.ColorCameraCalibrationSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorCameraCalibrationSettingsFeature.GetLastErrorMessage());
            }
            PrintAdditionalCalibrationSettings(calibrationSettings, "ColorCamera");
        }

        public void CorrectDisconnectExample()
        {
            //The whole API is designed on C++ standards, using smart pointers and constructor/destructor logic
            //All resources will be closed automatically, but the device state will not be affected. it will remain connected in PhoXi Control and if in freerun, it will remain Scanning
            //To Stop the device, just
            _phoXiDevice.StopAcquisition();
            //If you want to disconnect and logout the device from PhoXi Control, so it will then be available for other devices, call
            Console.WriteLine("\nDo you want to logout the device? Enter 0 for no, enter 1 for yes: ");
            var consoleLine = Console.ReadLine();
            var entry = 0;
            if (!int.TryParse(consoleLine, out entry)) return;
            _phoXiDevice.Disconnect(entry == 1);
            //The call PhoXiDevice without Logout will be called automatically by destructor
        }

        public void PrintMotionCamGeneral(PhoXiMotionCam motionCam)
        {
            Console.WriteLine("  MotionCam: ");
            Console.WriteLine("    OperationMode: {0}",
                Enum.GetName(typeof(PhoXiOperationMode.Value), (int)motionCam.OperationMode));
            Console.WriteLine("    LaserPower: {0}", motionCam.LaserPower);
            Console.WriteLine("    MaximumFPS: {0}", motionCam.MaximumFPS);
            Console.WriteLine("    Hardware Trigger: {0}", motionCam.HardwareTrigger);
            Console.WriteLine("    Hardware Trigger Signal: {0}",
                Enum.GetName(typeof(PhoXiHardwareTriggerSignal.Value), (int)motionCam.HardwareTriggerSignal));
        }

        public void PrintMotionCamCameraMode(PhoXiMotionCamCameraMode cameraMode)
        {
            Console.WriteLine("  CameraMode: ");
            Console.WriteLine("    Exposure: {0}", cameraMode.Exposure);
            Console.WriteLine("    SamplingTopology: {0}",
                Enum.GetName(typeof(PhoXiSamplingTopology.Value), (int)cameraMode.SamplingTopology));
            Console.WriteLine("    OutputTopology: {0}",
                Enum.GetName(typeof(PhoXiOutputTopology.Value), (int)cameraMode.OutputTopology));
            Console.WriteLine("    CodingStrategy: {0}",
                Enum.GetName(typeof(PhoXiCodingStrategy.Value), (int)cameraMode.CodingStrategy));
        }

        public void PrintMotionCamScannerMode(PhoXiMotionCamScannerMode scannerMode)
        {
            Console.WriteLine("  ScannerMode: ");
            Console.WriteLine("    ShutterMultiplier: {0}", scannerMode.ShutterMultiplier);
            Console.WriteLine("    ScanMultiplier: {0}", scannerMode.ScanMultiplier);
            Console.WriteLine("    CodingStrategy: {0}",
                Enum.GetName(typeof(PhoXiCodingStrategy.Value), (int)scannerMode.CodingStrategy));
            Console.WriteLine("    CodingQuality: {0}",
                Enum.GetName(typeof(PhoXiCodingQuality.Value), (int)scannerMode.CodingQuality));
            Console.WriteLine("    TextureSource: {0}",
                Enum.GetName(typeof(PhoXiTextureSource.Value), (int)scannerMode.TextureSource));
            Console.WriteLine("    Exposure: {0}", scannerMode.Exposure);
        }

        public void PrintProcessingSettings(PhoXiProcessingSettings processingSettings)
        {
            Console.WriteLine("  ProcessingSettings: ");
            PrintVector("MinCameraSpace(in DataCutting)", processingSettings.ROI3D.CameraSpace.min);
            PrintVector("MaxCameraSpace(in DataCutting)", processingSettings.ROI3D.CameraSpace.max);
            PrintVector("MinPointCloudSpace (in DataCutting)", processingSettings.ROI3D.PointCloudSpace.min);
            PrintVector("MaxPointCloudSpace (in DataCutting)", processingSettings.ROI3D.PointCloudSpace.max);

            Console.WriteLine("    MaxCameraAngle: {0}", processingSettings.NormalAngle.MaxCameraAngle);
            Console.WriteLine("    MaxProjectionAngle: {0}", processingSettings.NormalAngle.MaxProjectorAngle);
            Console.WriteLine("    MinHalfwayAngle: {0}", processingSettings.NormalAngle.MinHalfwayAngle);
            Console.WriteLine("    MaxHalfwayAngle: {0}", processingSettings.NormalAngle.MaxHalfwayAngle);
            Console.WriteLine("    Confidence (MaxInaccuracy): {0}", processingSettings.Confidence);
            Console.WriteLine("    SurfaceSmoothness: {0}",
                Enum.GetName(typeof(PhoXiSurfaceSmoothness.Value), (int)processingSettings.SurfaceSmoothness));
            Console.WriteLine("    NormalsEstimationRadius: {0}", processingSettings.NormalsEstimationRadius);
        }

        public void PrintCoordinatesSettings(PhoXiCoordinatesSettings coordinatesSettings)
        {
            Console.WriteLine("  CoordinatesSettings: ");
            PrintMatrix("CustomRotationMatrix", coordinatesSettings.CustomTransformation.Rotation);
            PrintVector("CustomTranslationVector", coordinatesSettings.CustomTransformation.Translation);
            PrintMatrix("RobotRotationMatrix", coordinatesSettings.RobotTransformation.Rotation);
            PrintVector("RobotTranslationVector", coordinatesSettings.RobotTransformation.Translation);
            Console.WriteLine("    CoordinateSpace: {0}",
                Enum.GetName(typeof(PhoXiCoordinateSpace.Value), (int)coordinatesSettings.CoordinateSpace));
            Console.WriteLine("    MarkerScale: {0} x {1}",
                coordinatesSettings.MarkersSettings.MarkerScale.Width,
                coordinatesSettings.MarkersSettings.MarkerScale.Height);
            Console.WriteLine("    RecognizeMarkers: {0}", coordinatesSettings.RecognizeMarkers);

        }

        public void PrintCalibrationSettings(PhoXiCalibrationSettings calibrationSettings, string source)
        {
            Console.WriteLine("Source: {0}", source);
            Console.WriteLine("  CalibrationSettings: ");
            Console.WriteLine("    FocusLength: {0}", calibrationSettings.FocusLength);
            Console.WriteLine("    PixelSize: {0} x {1}",
                calibrationSettings.PixelSize.Width,
                calibrationSettings.PixelSize.Height);
            PrintMatrix("CameraMatrix", calibrationSettings.CameraMatrix);
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

        public void PrintVector(string name, Point3_64f vector)
        {
            Console.WriteLine("    {0}: [{1}; {2}; {3}]",
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
                Console.WriteLine("    {0}: ", name);
                Console.WriteLine("      [{0}, {1}, {2}]",
                    (double)matrix[0, 0],
                    (double)matrix[0, 1],
                    (double)matrix[0, 2]);
                Console.WriteLine("      [{0}, {1}, {2}]",
                    (double)matrix[1, 0],
                    (double)matrix[1, 1],
                    (double)matrix[1, 2]);
                Console.WriteLine("      [{0}, {1}, {2}]",
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
                Console.WriteLine("    {0}: ", name);
                Console.WriteLine("      [{0}, {1}, {2}]",
                    (double)matrix[0, 0],
                    (double)matrix[0, 1],
                    (double)matrix[0, 2]);
                Console.WriteLine("      [{0}, {1}, {2}]",
                    (double)matrix[1, 0],
                    (double)matrix[1, 1],
                    (double)matrix[1, 2]);
                Console.WriteLine("      [{0}, {1}, {2}]",
                    (double)matrix[2, 0],
                    (double)matrix[2, 1],
                    (double)matrix[2, 2]);
            }
        }

        public PhoXiExamples()
        {
            try
            {
                ConnectPhoXiDeviceBySerialExample();
                ChangeMotionCamCameraModeExample();
                ChangeMotionCamScannerModeExample();
                ChangeMotionCam2DModeExample();
                ChangeProcessingSettingsExample();
                ChangeCoordinatesSettingsExample();
                CalibrationSettingsExample();
                ColorCameraCalibrationSettingsExample();
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