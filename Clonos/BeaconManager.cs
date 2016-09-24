using System;
using Android.App;
using Android.Runtime;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Java.Util;
using System.Linq;

namespace Clonos {

    public class BeaconManager : ScanCallback {

        public event EventHandler<BeaconDiscoveredEventArgs> BeaconDiscovered = delegate { };

        private BluetoothAdapter _adapter;

        // iBeacon UUID
        private readonly UUID BeaconUUID = UUID.FromString("E2C56DB5-DFFB-48D2-B060-D0F5A71096E0");

        public bool IsScanning { get; private set; }

        #region Singleton

        public static BeaconManager Instance {
            get { return _instance; }
        }

        private static BeaconManager _instance;

        static BeaconManager() {
            _instance = new BeaconManager();
        }

        protected BeaconManager() {
            var appContext = Application.Context;
            var manager = (BluetoothManager)appContext.GetSystemService("bluetooth");
            _adapter = manager.Adapter;
        }

        #endregion

        public void BeginScanningForDevices() {
            IsScanning = true;
            var settingsBuilder = new ScanSettings.Builder();
            settingsBuilder.SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency);
            settingsBuilder.SetReportDelay(0);
            var settings = settingsBuilder.Build();
            _adapter.BluetoothLeScanner.StartScan(null, settings, this);
        }

        public void StopScanningForDevices() {
            IsScanning = false;
            _adapter.BluetoothLeScanner.StopScan(this);
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result) {
            base.OnScanResult(callbackType, result);

            // Get the date - flip to big endian
            var data = result.ScanRecord.GetManufacturerSpecificData(76).Reverse().ToArray();

            // Get UUID
            var lower = BitConverter.ToInt64(data, 13);
            var upper = BitConverter.ToInt64(data, 5);
            var uuid = new UUID(lower, upper);

            // Filter iBeacon UUID
            if (!uuid.Equals(BeaconUUID)) return;

            // Get major & minor
            var major = BitConverter.ToInt16(data, 3);
            var minor = BitConverter.ToInt16(data, 1);

            // Get calibrated power level (2's complement)
            var power = data[0] - 256;

            // Fire event
            BeaconDiscovered(this, new BeaconDiscoveredEventArgs {
                Rssi = result.Rssi,
                Major = major,
                Minor = minor,
                TxPower = power
            });

        }

        public class BeaconDiscoveredEventArgs : EventArgs {

            public int Rssi;
            public int Major;
            public int Minor;
            public int TxPower;

            public BeaconDiscoveredEventArgs() : base() { }

        }

    }

}