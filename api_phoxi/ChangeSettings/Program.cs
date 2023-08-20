/*
* Photoneo's API Example - ChangeSettingsExample
* Defines the entry point for the console application.
* Demonstrates the extended functionality of PhoXi devices. This Example shows how to change the settings of the device.
* Contains the usage of retrieving all parameters and how to change these parameters.
* Points out the correct way to disconnect the device from PhoXiControl.
*/

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
            Console.WriteLine("\nPlease enter the Hardware Identification Number (for example 'YYYY-MM-###-LC#'): ");
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

        public void ChangeCapturingSettingsExample()
        {
            Console.WriteLine("\nChange Capturing Settings Example");
            //Retrieving the Current Capturing Settings
            var backupCapturingSettings = _phoXiDevice.CapturingSettings;
            //Changed current capture settings
            var changedCapturingSettings = _phoXiDevice.CapturingSettings;

            //Check if the currentCapturingSettings have been retrieved succesfully
            if (!_phoXiDevice.CapturingSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CapturingSettingsFeature.GetLastErrorMessage());
            }

            //Retrieving the Current Resolution
            var capturingResolution = _phoXiDevice.Resolution;
            //Check if the CurrentResolution has been retrieved succesfully
            if (!_phoXiDevice.ResolutionFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ResolutionFeature.GetLastErrorMessage());
            }

            //Printing the Current Capturing Settings
            Console.WriteLine("Current Capturing Settings are the following: ");
            PrintCapturingSettings(backupCapturingSettings, capturingResolution);

            //ShutterMultiplier values: 1-20
            changedCapturingSettings.ShutterMultiplier = 3;

            //ScanMultiplier values: 1-50
            changedCapturingSettings.ScanMultiplier = 8;

            //Resolution values: 0 is 2064x1544, 1 is 1032x772
            //Get all supported modes
            var supportedCapturingModes = _phoXiDevice.SupportedCapturingModes;
            //Check if the SupportedCapturingModes have been retrieved succesfully
            if (!_phoXiDevice.SupportedCapturingModesFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.SupportedCapturingModesFeature.GetLastErrorMessage());
            }
            _phoXiDevice.CapturingMode = supportedCapturingModes[0];
            //Check if the CapturingMode has been changed succesfully
            if (!_phoXiDevice.CapturingModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CapturingModeFeature.GetLastErrorMessage());
            }

            //CameraOnlyMode values: false is OFF, true is ON
            changedCapturingSettings.CameraOnlyMode = false;

            //AmbientLightSuppression values: false is OFF, true is ON
            changedCapturingSettings.AmbientLightSuppression = false;

            //CodingStrategy values: 1 is Normal, 2 is Interreflections
            changedCapturingSettings.CodingStrategy = 2;

            //CodingQuality values: 1 is Fast, 2 is High, 3 is Ultra
            changedCapturingSettings.CodingQuality = 1;

            //TextureSource values: 1 is Computed, 2 is LED, 3 is Laser, 4 is Focus
            changedCapturingSettings.TextureSource = 3;

            //SinglePatternExposure values: 10.24 / 14.336 / 20.48 / 24.576 / 30.72 / 34.816 / 40.96 / 49.152 / 75.776 / 79.872 / 90.112 / 100.352
            //Get all supported values
            var supportedSinglePatternExposures = _phoXiDevice.SupportedSinglePatternExposures;
            //Check if the SetSupportedSinglePatternExposures have been retrieved succesfully
            if (!_phoXiDevice.SupportedSinglePatternExposuresFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.SupportedSinglePatternExposuresFeature.GetLastErrorMessage());
            }
            changedCapturingSettings.SinglePatternExposure = supportedSinglePatternExposures[0];

            //MaximumFPS values: 0 - 60
            changedCapturingSettings.MaximumFPS = 1.12;

            //LaserPower values: possible from 0 - 4095, recommended 800 - 4095
            changedCapturingSettings.LaserPower = 1650;

            //LEDPower values: possible from 0 - 4095
            changedCapturingSettings.LEDPower = 1500;

            //ProjectionOffsetLeft values: possible from 0 - 512
            changedCapturingSettings.ProjectionOffsetLeft = 20;

            //ProjectionOffsetRight values: possible from 0 - 512
            changedCapturingSettings.ProjectionOffsetRight = 20;

            //HardwareTrigger values: 0 / 1 or false / true (0 is OFF, 1 is ON)
            changedCapturingSettings.HardwareTrigger = false;

            //HardwareTriggerSignal values: Falling / Rising / Both
            changedCapturingSettings.HardwareTriggerSignal = PhoXiHardwareTriggerSignal.Value.Falling;

            //Send settings
            _phoXiDevice.CapturingSettings = changedCapturingSettings;

            //Retrieving the Changed Capturing Settings
            var changedResolution = _phoXiDevice.Resolution;
            //Getting Current Resolution
            if (_phoXiDevice.CapturingModeFeature.isEnabled() && _phoXiDevice.CapturingModeFeature.CanGet())
            {
                var capturingMode = _phoXiDevice.CapturingMode;
                //You can ask the feature, if the last performed operation was successful
                if (!_phoXiDevice.CapturingModeFeature.isLastOperationSuccessful())
                {
                    throw new Exception(_phoXiDevice.CapturingModeFeature.GetLastErrorMessage());
                }
            }
            Console.WriteLine("Capturing Settings have been changed to the following: ");
            PrintCapturingSettings(changedCapturingSettings, changedResolution);

            //Restore previous values
            _phoXiDevice.CapturingSettings = backupCapturingSettings;
        }

        public void ChangeProcessingSettingsExample()
        {
            Console.WriteLine("\nChange Processing Settings Example");
            //Retrieving the Current Processing Settings
            var backupProcessingSettings = _phoXiDevice.ProcessingSettings;
            //Changed current Processing Settings
            var changedProcessingSettings = _phoXiDevice.ProcessingSettings;

            //Check if the currentProcessingSettings have been retrieved succesfully
            if (!_phoXiDevice.ProcessingSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ProcessingSettingsFeature.GetLastErrorMessage());
            }

            //Printing the Current Processing Settings
            Console.WriteLine("Current Processing Settings are the following: ");
            PrintProcessingSettings(backupProcessingSettings);

            var roi3D = changedProcessingSettings.ROI3D;
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
            changedProcessingSettings.ROI3D = roi3D;

            var normalAngle = changedProcessingSettings.NormalAngle;
            //MaxCameraAngle values: 0-90
            normalAngle.MaxCameraAngle = 80;

            //MaxProjectionAngle values: 0-90
            normalAngle.MaxProjectorAngle = 80;

            //MinHalfwayAngle values: 0-90
            normalAngle.MinHalfwayAngle = 10;

            //MaxHalfwayAngle values: 0-90
            normalAngle.MaxHalfwayAngle = 0;
            changedProcessingSettings.NormalAngle = normalAngle;

            //MaxInaccuracy(Confidence) values: 0-100
            changedProcessingSettings.Confidence = 2.0;

            //CalibrationVolumeCut values: 0 / 1 or false / true (0 is OFF, 1 is ON)
            changedProcessingSettings.CalibrationVolumeOnly = false;

            //SurfaceSmoothness values: 1 is Sharp, 2 is Normal, 3 is Smooth
            changedProcessingSettings.SurfaceSmoothness = 1;

            //NormalsEstimationRadius values: 1-4
            changedProcessingSettings.NormalsEstimationRadius = 2;

            //InterreflectionsFiltering values: 0 / 1 or false / true (0 is OFF, 1 is ON)
            changedProcessingSettings.InterreflectionsFiltering = false;

            //InterreflectionFilterStrength values: 0.01-0.99
            changedProcessingSettings.InterreflectionFilterStrength = 0.33;

            //PatternDecompositionReach values: Local, Small, Medium, Large
            changedProcessingSettings.PatternDecompositionReach = PhoXiPatternDecompositionReach.Value.Small;

            //SignalContrastThreshold values: 0.0-4095.0
            changedProcessingSettings.SignalContrastThreshold = 3.14;

            //Send settings
            _phoXiDevice.ProcessingSettings = changedProcessingSettings;

            //Check if the CurrentProcessingSettings have been retrieved succesfully
            if (!_phoXiDevice.ProcessingSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ProcessingSettingsFeature.GetLastErrorMessage());
            }
            Console.WriteLine("Processing Settings  have been changed to the following:");
            PrintProcessingSettings(changedProcessingSettings);

            //Restore previous values
            _phoXiDevice.ProcessingSettings = backupProcessingSettings;
        }

        public void ChangeCoordinatesSettingsExample()
        {
            Console.WriteLine("\nChange Coordinates Settings Example");
            //Retrieving the Current Coordinates Settings
            var backupCoordinatesSettings = _phoXiDevice.CoordinatesSettings;
            //Changed current capture settings
            var changedCoordinatesSettings = _phoXiDevice.CoordinatesSettings;

            //Check if the currentCoordinatesSettings have been retrieved succesfully
            if (!_phoXiDevice.CoordinatesSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CoordinatesSettingsFeature.GetLastErrorMessage());
            }

            //Printing the Current Coordinates Settings
            Console.WriteLine("Current Coordinates Settings are the following: ");
            PrintCoordinatesSettings(backupCoordinatesSettings);

            var customTransformation = changedCoordinatesSettings.CustomTransformation;
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
            changedCoordinatesSettings.CustomTransformation = customTransformation;

            //CoordinateSpace values: 1 is CameraSpace, 2 is MarkerSpace, 3 is RobotSpace, 4 is CustomSpace
            changedCoordinatesSettings.CoordinateSpace = 1;

            //Recognize Markers values: false is OFF, true is ON
            changedCoordinatesSettings.RecognizeMarkers = true;

            //Pattern Scale values: 0.0 - 1.0 (scale 1.0 x 1.0 is normal size)
            var markersSettings = changedCoordinatesSettings.MarkersSettings;
            var markerScale = markersSettings.MarkerScale;
            markerScale.Width = 0.5;
            markerScale.Height = 0.5;
            markersSettings.MarkerScale = markerScale;
            changedCoordinatesSettings.MarkersSettings = markersSettings;

            //Send settings
            _phoXiDevice.CoordinatesSettings = changedCoordinatesSettings;

            //Check if the CurrentCoordinatesSettings have been retrieved succesfully
            if (!_phoXiDevice.CoordinatesSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.CoordinatesSettingsFeature.GetLastErrorMessage());
            }
            Console.WriteLine("Changed Coordinates Settings are the following:");
            PrintCoordinatesSettings(changedCoordinatesSettings);

            //Restore previous values
            _phoXiDevice.CoordinatesSettings = backupCoordinatesSettings;
        }

        public void CalibrationSettingsExample()
        {
            //Retrieving the CalibrationSettings
            var calibrationSettings = _phoXiDevice.CalibrationSettings;
            //Check if the currentCalibrationSettings have been retrieved succesfully
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
            //Check if the currentCalibrationSettings have been retrieved succesfully
            if (!_phoXiDevice.ColorCameraCalibrationSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorCameraCalibrationSettingsFeature.GetLastErrorMessage());
            }
            PrintAdditionalCalibrationSettings(calibrationSettings, "CiolorCamera");
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

        public void PrintCapturingSettings(PhoXiCapturingSettings capturingSettings, PhoXiSize Resolution)
        {
            Console.WriteLine("  CapturingSettings: ");
            Console.WriteLine("    ShutterMultiplier: {0}", capturingSettings.ShutterMultiplier);
            Console.WriteLine("    ScanMultiplier: {0}", capturingSettings.ScanMultiplier);
            Console.WriteLine("    Resolution: {0}x{1}", Resolution.Width, Resolution.Height);
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
            PrintVector("MinCameraSpace(in DataCutting)", processingSettings.ROI3D.CameraSpace.min);
            PrintVector("MaxCameraSpace(in DataCutting)", processingSettings.ROI3D.CameraSpace.max);
            PrintVector("MinPointCloudSpace (in DataCutting)", processingSettings.ROI3D.PointCloudSpace.min);
            PrintVector("MaxPointCloudSpace (in DataCutting)", processingSettings.ROI3D.PointCloudSpace.max);
            Console.WriteLine("    MaxCameraAngle: {0}", processingSettings.NormalAngle.MaxCameraAngle);
            Console.WriteLine("    MaxProjectionAngle: {0}", processingSettings.NormalAngle.MaxProjectorAngle);
            Console.WriteLine("    MinHalfwayAngle: {0}", processingSettings.NormalAngle.MinHalfwayAngle);
            Console.WriteLine("    MaxHalfwayAngle: {0}", processingSettings.NormalAngle.MaxHalfwayAngle);
            Console.WriteLine("    SurfaceSmoothness: {0}",
                Enum.GetName(typeof(PhoXiSurfaceSmoothness.Value), (int)processingSettings.SurfaceSmoothness));
            Console.WriteLine("    NormalsEstimationRadius: {0}", processingSettings.NormalsEstimationRadius);
            Console.WriteLine("    InterreflectionsFiltering: {0}", processingSettings.InterreflectionsFiltering);
            Console.WriteLine("    InterreflectionFilterStrength: {0}", processingSettings.InterreflectionFilterStrength);
            Console.WriteLine("    PatternDecompositionReach: {0}", (string)processingSettings.PatternDecompositionReach);
            Console.WriteLine("    SignalContrastThreshold: {0}", processingSettings.SignalContrastThreshold);
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
            Console.WriteLine("    RecognizeMarkers: {0}", coordinatesSettings.RecognizeMarkers);
            Console.WriteLine("    MarkerScale: {0} x {1}",
                coordinatesSettings.MarkersSettings.MarkerScale.Width,
                coordinatesSettings.MarkersSettings.MarkerScale.Height);
        }

        public void PrintCalibrationSettings(PhoXiCalibrationSettings calibrationSettings, string source)
        {
            Console.WriteLine("\nSource: {0}", source);
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
                ChangeCapturingSettingsExample();
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