/*
* Photoneo's API Example - ChangeColorSettingsExample.cpp
* Defines the entry point for the console application.
* Demonstrates the extended functionality of PhoXi devices. This Example shows how to change the settings of the device.
* Points out the correct way to disconnect the device from PhoXiControl.
*/

using pho.api.csharp;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public void ChangeColorSettingsExample()
        {
            Console.WriteLine("Change Color Settings Example");
            //check if device support color camera
            if (!_phoXiDevice.Info().CheckFeature("Color"))
            {
                Console.WriteLine("Device does not support color features!");
                return;
            }
            //this settings are backup to restore values before this example
            var BackupCurrentColorSettings = _phoXiDevice.ColorSettings;

            //Retrieving the current ColorSettings
            var ColorSettings = _phoXiDevice.ColorSettings;
            //Check if the Current Color Settings have been retrieved succesfully
            if (!_phoXiDevice.ColorSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorSettingsFeature.GetLastErrorMessage());
            }
            Console.WriteLine("Settings before set up:");
            PrintColorSettings(ColorSettings);

            //Get all SupportedColorCapturingModes
            var CapturingModes = _phoXiDevice.SupportedColorCapturingModes;
            //Check if the SupportedColorCapturingModes have been retrieved succesfully
            if (!_phoXiDevice.SupportedColorCapturingModesFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.SupportedColorCapturingModesFeature.GetLastErrorMessage());
            }
            //Pick a capturing mode
            ColorSettings.CapturingMode = CapturingModes[0];
            _phoXiDevice.ColorSettings = ColorSettings;
            //Check if the Color Settings have been changed succesfully
            if (!_phoXiDevice.ColorSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorSettingsFeature.GetLastErrorMessage());
            }

            //Get Supported Iso values
            var SupportedIsoValues = _phoXiDevice.SupportedColorIso;
            //Check if the SupportedColorIso have been retrieved succesfully
            if (!_phoXiDevice.SupportedColorIsoFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.SupportedColorIsoFeature.GetLastErrorMessage());
            }
            //Pick a Iso value
            ColorSettings.Iso = SupportedIsoValues[4];
            _phoXiDevice.ColorSettings = ColorSettings;
            //Check if the Color Settings have been changed succesfully
            if (!_phoXiDevice.ColorSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorSettingsFeature.GetLastErrorMessage());
            }

            //Get Supported Exposure values
            var SupportedExposureValues = _phoXiDevice.SupportedColorExposure;
            //Check if the SupportedColorExposure have been retrieved succesfully
            if (!_phoXiDevice.SupportedColorExposureFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.SupportedColorExposureFeature.GetLastErrorMessage());
            }
            //Pick a Exposure value
            ColorSettings.Exposure = SupportedExposureValues[2];
            _phoXiDevice.ColorSettings = ColorSettings;
            //Check if the Color Settings have been changed succesfully
            if (!_phoXiDevice.ColorSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorSettingsFeature.GetLastErrorMessage());
            }

            //Get Supported White Presets
            var SupportedColorWhiteBalancePresets = _phoXiDevice.SupportedColorWhiteBalancePresets;
            //Check if the SupportedColorExposure have been retrieved succesfully
            if (!_phoXiDevice.SupportedColorWhiteBalancePresetsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.SupportedColorWhiteBalancePresetsFeature.GetLastErrorMessage());
            }
            //Pick a capturing mode
            PhoXiWhiteBalance whiteBalance = ColorSettings.WhiteBalance;
            whiteBalance.Preset = SupportedColorWhiteBalancePresets[1];
            ColorSettings.WhiteBalance = whiteBalance;
            _phoXiDevice.ColorSettings = ColorSettings;
            //Check if the Color Settings have been changed succesfully
            if (!_phoXiDevice.ColorSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorSettingsFeature.GetLastErrorMessage());
            }

            Console.WriteLine("Settings after set up:");
            PrintColorSettings(_phoXiDevice.ColorSettings);

            //Restore previous values, if you want to return to previous state
            _phoXiDevice.ColorSettings = BackupCurrentColorSettings;
        }
        public void ComputeCustomWhiteBalanceExample()
        {
            Console.WriteLine("Compute Custom White Balance Example");

            //Check if device support color camera
            if (!_phoXiDevice.Info().CheckFeature("Color"))
            {
                Console.WriteLine("Device does not support color features!");
                return;
            }

            //Start camera acquisition if not acquiring
            if (!_phoXiDevice.isAcquiring() && !_phoXiDevice.StartAcquisition())
            {
                throw new Exception("Failed to start acquisition");
            }

            //Set a software trigger
            _phoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Software;
            if (!_phoXiDevice.TriggerModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.TriggerModeFeature.GetLastErrorMessage());
            }

            //Enable ComputeCustomWhiteBalance setting
            _phoXiDevice.ColorSettings.WhiteBalance.ComputeCustomWhiteBalance = true;
            if (!_phoXiDevice.ColorSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorSettingsFeature.GetLastErrorMessage());
            }

            int frameId = _phoXiDevice.TriggerFrame(true, true);
            if (frameId < 0)
            {
                throw new Exception("Failed to trigger a frame");
            }

            Frame frame = _phoXiDevice.GetSpecificFrame(frameId);
            if (frame == null)
            {
                throw new Exception("Failed to acquire a frame");
            }

            //The computed white balance factors are NOT updated in ColorSettings white balance structure
            //but are present in acquired frame info structure
            Console.WriteLine("Computed white balance factors:");
            Console.WriteLine("R: {0}", frame.Info.BalanceRGB.x);
            Console.WriteLine("G: {0}", frame.Info.BalanceRGB.y);
            Console.WriteLine("B: {0}", frame.Info.BalanceRGB.z);

            //If white balance settings are acceprable, they can be made persistent by setting them 
            //into ColorSetting structure and disabling ComputeCustomWhiteBalance setting
            _phoXiDevice.ColorSettings.WhiteBalance.BalanceRGB = frame.Info.BalanceRGB;
            if (!_phoXiDevice.ColorSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorSettingsFeature.GetLastErrorMessage());
            }

            _phoXiDevice.ColorSettings.WhiteBalance.ComputeCustomWhiteBalance = false;
            if (!_phoXiDevice.ColorSettingsFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.ColorSettingsFeature.GetLastErrorMessage());
            }

            Console.WriteLine("Settings after ComputeCustomWhiteBalance:");
            PrintColorSettings(_phoXiDevice.ColorSettings);
        }

        public void SaveTextureToBitmap()
        {
            Console.WriteLine("Save RGB Color Texture Example");

            //Check if device support color camera
            if (!_phoXiDevice.Info().CheckFeature("Color"))
            {
                Console.WriteLine("Device does not support color features!");
                return;
            }

            //Start camera acquisition if not acquiring
            if (!_phoXiDevice.isAcquiring() && !_phoXiDevice.StartAcquisition())
            {
                throw new Exception("Failed to start acquisition");
            }

            //Set a software trigger
            _phoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Software;
            if (!_phoXiDevice.TriggerModeFeature.isLastOperationSuccessful())
            {
                throw new Exception(_phoXiDevice.TriggerModeFeature.GetLastErrorMessage());
            }

            _phoXiDevice.OutputSettings.SendColorCameraImage = true;
            if (_phoXiDevice.MotionCam.OperationMode == PhoXiOperationMode.Value.Camera)
            {
                _phoXiDevice.MotionCamCameraMode.TextureSource = PhoXiTextureSource.Value.Color;
            }
            else if (_phoXiDevice.MotionCam.OperationMode == PhoXiOperationMode.Value.Scanner)
            {
                _phoXiDevice.MotionCamScannerMode.TextureSource = PhoXiTextureSource.Value.Color;
            }
            else
            {
                Console.WriteLine("Device is not in Camera or Scanner mode!");
                return;
            }

            int frameId = _phoXiDevice.TriggerFrame(true, true);
            if (frameId < 0)
            {
                throw new Exception("Failed to trigger a frame");
            }

            Frame frame = _phoXiDevice.GetSpecificFrame(frameId);
            if (frame == null)
            {
                throw new Exception("Failed to acquire a frame");
            }

            SaveTexture(frame.TextureRGB, $@"TextureRGB.png");
            SaveTexture(frame.ColorCameraImage, $@"ColorCameraImage.png");
        }

        static ushort normalization(ushort v, ushort chMin, ushort chMax)
        {
            float min = (float)chMin;
            float max = (float)chMax;
            const float range = 65535.0F;
            v = (ushort)(((float)v - min) / (max - min) * range);
            return v;
        }

        static void SaveTexture(TextureRGB16 rgb, string BitmapName)
        {
            var pixels = rgb.GetDataCopy();
            PhoXiSize size = rgb.Size;
            int w = size.Width;
            int h = size.Height;
            System.Drawing.Bitmap pic = new System.Drawing.Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format48bppRgb);//Format16bppRgb555
            Rectangle rect = new Rectangle(0, 0, w, h);
            // Lock bits for direct access
            System.Drawing.Imaging.BitmapData bitmapData = pic.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, pic.PixelFormat);
            unsafe
            {
                ushort* ptr = (ushort*)bitmapData.Scan0.ToPointer();
                ushort chMin = 65535;
                ushort chMax = 0;
                // calc min, max from all RGB channels. Values will be used for color stretching
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int arrayIndex = y * (w * 3) + x * 3;
                        for (int ch = 0; ch < 3; ch++)
                        {
                            chMin = Math.Min(chMin, pixels[arrayIndex + ch]);
                            chMax = Math.Max(chMax, pixels[arrayIndex + ch]);
                        }
                    }
                }
                // copy normalized texture RGB data to the BMP buffer (ptr)
                // BMP buffer have BGR organisation
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int arrayIndex = y * (w * 3) + x * 3;
                        ptr[arrayIndex] = normalization(pixels[arrayIndex + 2], chMin, chMax);
                        ptr[arrayIndex + 1] = normalization(pixels[arrayIndex + 1], chMin, chMax);
                        ptr[arrayIndex + 2] = normalization(pixels[arrayIndex], chMin, chMax);
                    }
                }
            }
            // Unlock the bits.
            pic.UnlockBits(bitmapData);
            pic.Save(BitmapName);
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

        public void PrintColorSettings(PhoXiColorSettings ColorSettings)
        {
            Console.WriteLine("  ColorSettings: ");
            Console.WriteLine("    Iso:                {0}", ColorSettings.Iso);
            Console.WriteLine("    Exposure:           {0}", ColorSettings.Exposure);
            Console.WriteLine("    Gamma:              {0}", ColorSettings.Gamma);
            Console.WriteLine("    Resolution: Width:  {0}", ColorSettings.CapturingMode.Resolution.Width);
            Console.WriteLine("    Resolution: Height: {0}", ColorSettings.CapturingMode.Resolution.Height);
            Console.WriteLine("    WhiteBalancePreset: {0}", ColorSettings.WhiteBalance.Preset);
            Console.WriteLine("    WhiteBalance:    R: {0}", (double)ColorSettings.WhiteBalance.BalanceRGB.x);
            Console.WriteLine("    WhiteBalance:    G: {0}", (double)ColorSettings.WhiteBalance.BalanceRGB.y);
            Console.WriteLine("    WhiteBalance:    B: {0}", (double)ColorSettings.WhiteBalance.BalanceRGB.z);
        }
        public PhoXiExamples()
        {
            try
            {
                ConnectPhoXiDeviceBySerialExample();
                ChangeColorSettingsExample();
                ComputeCustomWhiteBalanceExample();
                SaveTextureToBitmap();
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