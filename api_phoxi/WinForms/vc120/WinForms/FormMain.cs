using pho.api.csharp;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsNoCMake
{
    public partial class FormMain : Form
    {
        private PhoXi _phoXiDevice;
        private PhoXiFactory _factory;

        public delegate void PhoXiControlIsRunningEvent();
        public event PhoXiControlIsRunningEvent OnPhoXiControlIsRunning;

        public FormMain()
        {
            InitializeComponent();
            Shown += FormMain_Shown;
        }

        public async void FormMain_Shown(object sender, EventArgs e)
        {

            // PhoXiFactory is used to create PhoXiDevice instances.
            // Furthermore it is used to get list of available devices
            // and determine whether PhoXi Control is running, what is
            // its version etc.
            // Important: You should only have one instance of PhoXiFactory.
            _factory = new PhoXiFactory();

            // Handles event fired when PhoXi Control is running
            OnPhoXiControlIsRunning += HandleOnPhoXiControlIsRunningEvent;
            // Wait until PhoXi Control is running
            await WaitForPhoXiControl();
            await UpdateButtons();
        }

        public async Task GetAvailableDevicesExample()
        {
            lvDevices.Items.Clear();

            foreach (var device in await GetDeviceListAsync())
            {
                var deviceStatus = device.Status.Attached
                    ? "Attached to PXC."
                    : "Not attached to PXC. ";
                deviceStatus += device.Status.Ready
                    ? "Ready"
                    : "Occupied";

                var deviceItem =
                    new ListViewItem(new[]
                    {
                        device.HWIdentification,
                        deviceStatus
                    })
                    {
                        Name = device.HWIdentification
                    };
                lvDevices.Items.Add(deviceItem);
            }
        }

        public async Task<bool> ConnectPhoXiDeviceBySerialExample(string hardwareIdentification)
        {
            if (string.IsNullOrEmpty(hardwareIdentification))
            {
                LogLine(string.Empty);
                LogLine("Incorrect Hardware Identification Number.");
                LogLine("Refresh the device list and choose another device.");
            }

            if (await CanConnectAsync(hardwareIdentification))
            {
                PhoXiTimeout timeout = PhoXiTimeout.Value.ZeroTimeout;
                _phoXiDevice = await CreateAndConnectAsync(hardwareIdentification, timeout);

                if (_phoXiDevice != null)
                {
                    LogLine("Connection to the device {0} was Successful!", hardwareIdentification);
                    return true;
                }

                LogLine("Connection to the device {0} was Unsuccessful!", hardwareIdentification);
                return false;
            }

            LogLine("Can't connect to device {0}!", hardwareIdentification);
            return false;
        }

        public async Task SoftwareTriggerExample()
        {
            // Check if the device is connected
            if (!await IsPhoXiDeviceConnectedAsync())
            {
                LogLine("Device is not created or not connected!");
                return;
            }

            // If it is not in Software trigger mode, we need to switch the modes
            if (_phoXiDevice.TriggerMode != PhoXiTriggerMode.Value.Software)
            {
                LogLine("Device is not in Software trigger mode");
                if (await IsAcquiringAsync())
                {
                    LogLine("Stopping acquisition");
                    // If the device is in Acquisition mode, we need to stop the acquisition
                    if (!await StopAcquisitionAsync())
                    {
                        LogLine("Error in StopAcquistion");
                    }
                }
                LogLine("Switching to Software trigger mode ");
                // Switching the mode is as easy as assigning of a value
                // It will call the appropriate calls in the background
                _phoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Software;
                // Just check if did everything run smoothly
                if (_phoXiDevice.TriggerModeFeature.isLastOperationSuccessful())
                {
                    throw new Exception(_phoXiDevice.TriggerModeFeature.GetLastErrorMessage());
                }
            }
            // Start the device acquisition, if necessary
            if (!await IsAcquiringAsync())
            {
                if (!await StartAcquisitionAsync())
                {
                    throw new Exception("Error in StartAcquisition");
                }
            }
            // We can clear the current Acquisition buffer
            // - This will not clear Frames that arrives to the PC after the Clear command is performed
            var clearedFrames = await ClearFramesAsync();
            LogLine("{0} frames were cleared from the cyclic buffer", clearedFrames);

            // While we checked the state of the StartAcquisition call
            // This check is not necessary, but it is a good practice
            if (!await IsAcquiringAsync())
            {
                LogLine("Device is not acquiring");
                return;
            }

            LogLine("Triggering a frame");
            // If false is passed here, the device will reject the frame if it is not ready to be triggered
            // If true us supplied, it will wait for the trigger
            var frameId = await TriggerFrameAsync();
            if (frameId < 0)
            {
                //If negative number is returned trigger was unsuccessful
                LogLine("Trigger was unsuccessful! code={0}", frameId);
            }

            LogLine("Frame was triggered, Frame Id: {0}", frameId);
            LogLine("Waiting for frame");
            // Wait for a frame with specific FrameID.
            // There is a possibility, that frame triggered before the trigger
            // will arrive after the trigger call, and will be retrieved before requested frame
            // Because of this, the TriggerFrame call returns the requested frame ID,
            // so it can than be retrieved from the Frame structure.
            // This call is doing that internally in background.
            // You can specify Timeout here
            // - default is the Timeout stored in Timeout Feature. Infinity by default.
            var myFrame = await GetFrameAsync();
            if (myFrame != null)
            {
                PrintFrameInfo(myFrame);
                PrintFrameData(myFrame);
            }
            else
            {
                LogLine("Failed to retrieve the MyFrame!");
            }
        }

        public async Task CorrectDisconnectExample()
        {
            // To disconnect from device and logout, call Disconnect(true)
            // To disconnect from device without logging out,
            // call Disconnect(false) or just Disconnect()
            await StopAcquisitionAsync();
            LogLine(await DisconnectAsync(true)
                ? "Disconnect from device was successful"
                : "Disconnect from device was unsuccessful!");
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (lvDevices.SelectedItems.Count <= 0)
            {
                LogLine("You did not select a device to connect to.");
                return;
            }

            var selectedItem = lvDevices.SelectedItems[0].Text;
            Cursor.Current = Cursors.WaitCursor;
            await ConnectPhoXiDeviceBySerialExample(selectedItem);
            Cursor.Current = Cursors.Default;
            await GetDeviceListAsync();
            await UpdateButtons();
            await GetAvailableDevicesExample();
        }

        private async void btnTriggerScan_Click(object sender, EventArgs e)
        {
            await SoftwareTriggerExample();
        }

        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            await CorrectDisconnectExample();
            await UpdateButtons();
            await GetAvailableDevicesExample();
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            rtbOutput.Clear();
            LogLine(@"PhoXi Control version: {0}", _factory.GetPhoXiControlVersion());
            LogLine(@"PhoXi API version: {0}", _factory.GetAPIVersion());
            await GetAvailableDevicesExample();
        }

        private async void lvDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            await UpdateButtons();
        }

        private async void lvDevices_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            await UpdateButtons();
            btnConnect.PerformClick();
        }

        private async Task UpdateButtons()
        {
            if (_phoXiDevice == null)
            {
                btnDisconnect.Enabled = false;
                return;
            }

            var isAnyDeviceConnected =
                await IsPhoXiDeviceConnectedAsync();
            btnConnect.Enabled = !isAnyDeviceConnected;
            btnDisconnect.Enabled = isAnyDeviceConnected;
            btnTriggerScan.Enabled = isAnyDeviceConnected;
        }

        private async Task WaitForPhoXiControl()
        {
            while (true)
            {
                var isRunning = Task.Run(() => _factory.isPhoXiControlRunning());
                await isRunning;
                if (isRunning.Result)
                {
                    break;
                }

                LogLine("PhoXi Control is not running!");

                await Task.Delay(500);
            }
            OnPhoXiControlIsRunning?.Invoke();
        }

        private void HandleOnPhoXiControlIsRunningEvent()
        {
            var availableDevicesExample = GetAvailableDevicesExample();
        }

        private async Task<PhoXiDeviceInformation[]> GetDeviceListAsync()
        {
            var deviceList = await Task.Run(() => _factory.GetDeviceList());
            while (true)
            {
                if (deviceList.Length == 0 ||
                    string.IsNullOrEmpty(deviceList[0].HWIdentification))
                {
                    await Task.Delay(200);
                    deviceList = await Task.Run(() => _factory.GetDeviceList());
                }
                else
                {
                    break;
                }
            }
            return deviceList;
        }

        private async Task<bool> CanConnectAsync(string hardwareIdentification)
        {
            return await Task.Run(() =>
                _factory.CanConnect(hardwareIdentification));
        }

        private async Task<PhoXi> CreateAndConnectAsync(
            string hardwareIdentification, PhoXiTimeout timeout)
        {
            return await Task.Run(() =>
                _factory.CreateAndConnect(hardwareIdentification, timeout));
        }

        private async Task<bool> IsPhoXiDeviceConnectedAsync()
        {
            return _phoXiDevice != null &&
                   await Task.Run(() => _phoXiDevice.isConnected());
        }

        private async Task<bool> IsAcquiringAsync()
        {
            return await Task.Run(() => _phoXiDevice.isAcquiring());
        }

        private async Task<bool> StartAcquisitionAsync()
        {
            return await Task.Run(() => _phoXiDevice.StartAcquisition());
        }

        private async Task<bool> StopAcquisitionAsync()
        {
            return await Task.Run(() => _phoXiDevice.StopAcquisition());
        }

        private async Task<int> ClearFramesAsync()
        {
            return await Task.Run(() => _phoXiDevice.ClearBuffer());
        }

        private async Task<int> TriggerFrameAsync(
            bool waitForAccept = true, bool waitForGrabbingEnd = false)
        {
            return await Task.Run(() =>
                _phoXiDevice.TriggerFrame(waitForAccept, waitForGrabbingEnd));
        }

        private async Task<Frame> GetFrameAsync(PhoXiTimeout timeout)
        {
            return await Task.Run(() => _phoXiDevice.GetFrame(timeout));
        }

        private async Task<Frame> GetFrameAsync()
        {
            return await GetFrameAsync(PhoXiTimeout.Value.LastStored);
        }

        private async Task<bool> DisconnectAsync(bool shouldAlsoLogout = false)
        {
            return await Task.Run(() =>
                _phoXiDevice.Disconnect(shouldAlsoLogout));
        }

        private void PrintFrameInfo(Frame frame)
        {
            var frameInfo = frame.Info;
            LogLine("  Frame params: ");
            LogLine("    Frame Index: {0}", frameInfo.FrameIndex);
            LogLine("    Frame Timestamp: {0} s", frameInfo.FrameTimestamp);
            LogLine("    Frame Acquisition duration: {0} ms", frameInfo.FrameDuration);
            LogLine("    Frame Computation duration: {0} ms", frameInfo.FrameComputationDuration);
            LogLine("    Frame Transfer duration: {0} ms", frameInfo.FrameTransferDuration);
            LogLine("    Sensor Position: [{0}; {1}; {2}]",
                (double)frameInfo.SensorPosition.x,
                (double)frameInfo.SensorPosition.y,
                (double)frameInfo.SensorPosition.z);
            LogLine("    Total scan count: {0}", frameInfo.TotalScanCount);
        }

        private void PrintFrameData(Frame frame)
        {
            if (frame.Empty())
            {
                LogLine("Frame is empty");
                return;
            }

            LogLine("  Frame data: ");
            if (!frame.PointCloud.Empty())
            {
                LogLine("    PointCloud:    ({0} x {1}) Type: {2}",
                    frame.PointCloud.Size.Width,
                    frame.PointCloud.Size.Height,
                    PointCloud32f.GetElementName());
            }

            if (!frame.NormalMap.Empty())
            {
                LogLine("    NormalMap:     ({0} x {1}) Type: {2}",
                    frame.NormalMap.Size.Width,
                    frame.NormalMap.Size.Height,
                    NormalMap32f.GetElementName());
            }

            if (!frame.DepthMap.Empty())
            {
                LogLine("    DepthMap:     ({0} x {1}) Type: {2}",
                    frame.DepthMap.Size.Width,
                    frame.DepthMap.Size.Height,
                    DepthMap32f.GetElementName());
            }

            if (!frame.ConfidenceMap.Empty())
            {
                LogLine("    ConfidenceMap:     ({0} x {1}) Type: {2}",
                    frame.ConfidenceMap.Size.Width,
                    frame.ConfidenceMap.Size.Height,
                    ConfidenceMap32f.GetElementName());
            }

            if (!frame.Texture.Empty())
            {
                LogLine("    Texture:     ({0} x {1}) Type: {2}",
                    frame.Texture.Size.Width,
                    frame.Texture.Size.Height,
                    Texture32f.GetElementName());
            }
        }

        private void Log(string text)
        {
            var args = new object[] { };
            if (InvokeRequired)
            {
                var log = (MethodInvoker)delegate
                {
                    Log(text, args);
                };
                Invoke(log);
            }
            else
            {
                Log(text, args);
            }
        }

        private void Log(string format, params object[] args)
        {
            if (InvokeRequired)
            {
                var log = (MethodInvoker)delegate
                {
                    rtbOutput.AppendText(string.Format(format, args));
                    rtbOutput.SelectionStart = rtbOutput.Text.Length;
                    rtbOutput.ScrollToCaret();
                };
                Invoke(log);
            }
            else
            {
                rtbOutput.AppendText(string.Format(format, args));
                rtbOutput.SelectionStart = rtbOutput.Text.Length;
                rtbOutput.ScrollToCaret();
            }
        }

        private void LogLine(string text)
        {
            Log(Environment.NewLine + text);
        }

        private void LogLine(string format, params object[] args)
        {
            Log(Environment.NewLine + format, args);
        }
    }
}
