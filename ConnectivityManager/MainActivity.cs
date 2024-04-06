using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Nfc;

namespace ConnectivityManager
{
    [Activity(Label = "Connectivity Manager", MainLauncher = true, Icon = "@drawable/ic_launcher_round", 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main); 

            Button btnBluetooth = FindViewById<Button>(Resource.Id.btnBluetooth);
            Button btnWiFi = FindViewById<Button>(Resource.Id.btnWiFi);
            Button btnNFC = FindViewById<Button>(Resource.Id.btnNFC);
            Button btnMobileData = FindViewById<Button>(Resource.Id.btnMobileData);
            Button btnGetSignalStrength = FindViewById<Button>(Resource.Id.btnGetSignalStrength);
            Button btnDial = FindViewById<Button>(Resource.Id.btnDial);

            btnBluetooth.Click += delegate {
                var Bluetooth = new Intent(this, typeof(BluetoothActivity));                
                StartActivity(Bluetooth);                
            };

            btnWiFi.Click += delegate {
                var WiFi = new Intent(this, typeof(WiFiActivity));
                StartActivity(WiFi);                
            };

            btnNFC.Click += delegate {
                var NFC = new Intent(this, typeof(NFC_Activity));
                StartActivity(NFC);              
            };

            btnMobileData.Click += delegate {
                Intent intent = new Intent();
                intent.SetClassName("com.android.settings", "com.android.settings.Settings$DataUsageSummaryActivity");
                StartActivityForResult(intent, 1);
            };

            btnGetSignalStrength.Click += delegate {
                var MN = new Intent(this, typeof(MobileNetwork_Activity));
                StartActivity(MN);
            };

            btnDial.Click += delegate {
                Intent intent = new Intent(Intent.ActionDial);
                StartActivityForResult(intent, 1);
            };
        }
    }
}

