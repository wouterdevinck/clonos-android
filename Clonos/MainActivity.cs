using System;
using Android.App;
using Android.Widget;
using Android.OS;

namespace Clonos {

    [Activity(Label = "Clonos", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity {

        int count = 1;
        TextView text;

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            Button button = FindViewById<Button>(Resource.Id.MyButton);
            text = FindViewById<TextView>(Resource.Id.MyText);

            BeaconManager.Instance.BeaconDiscovered += BeaconCallback;

            button.Click += (object sender, EventArgs e) => {
                if (!BeaconManager.Instance.IsScanning) {
                    BeaconManager.Instance.BeginScanningForDevices();
                } else {
                    BeaconManager.Instance.StopScanningForDevices();
                }
            };

        }

        private void BeaconCallback(object sender, BeaconManager.BeaconDiscoveredEventArgs e) {
            text.Text += string.Format("Beacon - Rssi: {0} Major: {1} Minor: {2} Power: {3}\n", e.Rssi, e.Major, e.Minor, e.TxPower);
        }

    }

}
