/*
* Photoneo's API Example - ChangeProfileExample
* Defines the entry point for the console application.
* Demonstrates the extended functionality of PhoXi devices. This Example shows how to change the profiles of the device.
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
            Console.WriteLine("Current profile is the following: " + actualprofile);

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

            Console.WriteLine("Changed profile is the following: " + _phoXiDevice.ActiveProfile);

            _phoXiDevice.ActiveProfile = actualprofile;

            //Check if profile has been changed back successfully
            if (!_phoXiDevice.ActiveProfileFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.ActiveProfileFeature.GetLastErrorMessage());

            Console.WriteLine("Changed profile back is the following: " + _phoXiDevice.ActiveProfile);

            PhoXiProfileContent exportedProfile = _phoXiDevice.ExportProfile;
            //Check if profile has been exported successfully
            if (!_phoXiDevice.ExportProfileFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.ExportProfileFeature.GetLastErrorMessage());

            // Save exported profile to the file
            //System.IO.File.WriteAllBytes("profile.phop", exportedProfile.GetContent());

            Console.WriteLine("Exported profile: " + exportedProfile.Name);

            PhoXiProfileContent importProfile = new PhoXiProfileContent();
            importProfile.Name = "newImported";
            importProfile.SetContent(exportedProfile.GetContent());
            // Load profile from file
            //importProfile.SetContent(System.IO.File.ReadAllBytes("profile.phop"));
            _phoXiDevice.ImportProfile = importProfile;
            //Check if profile has been imported successfully
            if (!_phoXiDevice.ImportProfileFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.ImportProfileFeature.GetLastErrorMessage());

            Console.WriteLine("Imported profile: " + importProfile.Name);

            _phoXiDevice.CreateProfile = "createdProfile";
            //Check if profile has been created successfully
            if (!_phoXiDevice.CreateProfileFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.CreateProfileFeature.GetLastErrorMessage());

            _phoXiDevice.UpdateProfile = "createdProfile";
            //Check if profile has been updated successfully
            if (!_phoXiDevice.UpdateProfileFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.UpdateProfileFeature.GetLastErrorMessage());

            Console.WriteLine("Updated profile: " + "createdProfile");

            _phoXiDevice.StartupProfile = actualprofile;
            //Check if profile has been deleted successfully
            if (!_phoXiDevice.StartupProfileFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.StartupProfileFeature.GetLastErrorMessage());

            Console.WriteLine("Startup profile: " + actualprofile);

            _phoXiDevice.DeleteProfile = "createdProfile";
            //Check if profile has been deleted successfully
            if (!_phoXiDevice.DeleteProfileFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.DeleteProfileFeature.GetLastErrorMessage());

            _phoXiDevice.DeleteProfile = "newImported";
            //Check if profile has been deleted successfully
            if (!_phoXiDevice.DeleteProfileFeature.isLastOperationSuccessful())
                throw new Exception(_phoXiDevice.DeleteProfileFeature.GetLastErrorMessage());
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

        public PhoXiExamples()
        {
            try
            {
                ConnectPhoXiDeviceBySerialExample();
                ChangeProfileExample();
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