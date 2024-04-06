using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Net.Wifi;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Android.Net;

namespace ConnectivityManager
{
    [Activity(Label = "Wi-Fi", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
        Theme = "@android:style/Theme.Holo.NoActionBar")]
    public class WiFiActivity : Activity
    {
        private WifiReceiver wifiReceiver;
        public static WifiManager wifi;
        private WifiConfiguration config;
        public static ListView WiFiListView;
        public static ArrayAdapter arrayAdapter;
        public static string key;
        public static string ssid;
        private EditText pass;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.WiFi);

            ToggleButton btnToggleWiFi = FindViewById<ToggleButton>(Resource.Id.btnToggleWiFi);
            WiFiListView = FindViewById<ListView>(Resource.Id.WiFiListView);

            WifiManager wifi = (WifiManager)GetSystemService(WifiService);  
            wifi.SetWifiEnabled(false);   
            
            config = new WifiConfiguration();

            wifi = (WifiManager)GetSystemService(Context.WifiService);            
            IntentFilter filter = new IntentFilter();
           
            filter.AddAction(WifiManager.ScanResultsAvailableAction);
            filter.AddAction(WifiManager.SupplicantConnectionChangeAction);
            filter.AddAction(WifiManager.WifiStateChangedAction);
            filter.AddAction(WifiManager.NetworkStateChangedAction);

            wifiReceiver = new WifiReceiver();               
            this.RegisterReceiver(wifiReceiver, filter);

            arrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);
            arrayAdapter.Clear();
            WiFiListView.Adapter = arrayAdapter;
            WiFiListView.ItemClick += AvailableWiFiNetworks_ItemClick;
            
            btnToggleWiFi.Click += delegate {

                if (wifi.IsWifiEnabled == true)
                {
                    wifi.SetWifiEnabled(false);
                    arrayAdapter.Clear();                    
                }
                else
                {
                    wifi.SetWifiEnabled(true);                    
                    wifi.StartScan();                    
                }
            };
        }

        public void AvailableWiFiNetworks_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {               
            string selectedItem = WiFiListView.GetItemAtPosition(e.Position).ToString();
            string MAC = selectedItem.Substring(selectedItem.Length - 17);            
            ShowPwdDialogue(selectedItem);            
        }

        public void ShowPwdDialogue(string wifiSSID)
        {
            //ConnectWiFi(ssid, key);
            Dialog dialog = new Dialog(this);
            dialog.SetContentView(Resource.Layout.PwdDialogue);
            TextView textSSID = dialog.FindViewById<TextView>(Resource.Id.textSSID1);

            Button dialogButton = dialog.FindViewById<Button>(Resource.Id.btnOK);
            Button btnCancel = dialog.FindViewById<Button>(Resource.Id.btnCancel);
            pass = dialog.FindViewById<EditText>(Resource.Id.textPassword);

            textSSID.Text = wifiSSID;
            string checkPassword = pass.Text.ToString();

            dialogButton.Click += delegate {
                ConnectWiFi(wifiSSID, checkPassword);

                if (!pass.Equals(config.PreSharedKey))
                {
                    Toast.MakeText(ApplicationContext, "The password is incorrect", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(ApplicationContext, "Connecting...", ToastLength.Short).Show();
                }
                dialog.Dismiss();
            };

            btnCancel.Click += delegate {
                dialog.Cancel();
                Toast.MakeText(ApplicationContext, "The connection request was canceled", ToastLength.Short).Show();
            };

            dialog.Show();
        }        

        public bool ConnectWiFi(string ssid, string key)
        {
            try
            {
                WifiConfiguration wifiConfig = new WifiConfiguration();
                wifiConfig.Ssid = String.Format("\"{0}\"", ssid);
                wifiConfig.PreSharedKey = String.Format("\"{0}\"", key);
                WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
                wifiManager.SetWifiEnabled(true);
                
                int netId;
                var network = wifiManager.ConfiguredNetworks.FirstOrDefault(cn => cn.Ssid == ssid);
                if (network != null)
                {
                    netId = network.NetworkId;
                }
                else
                {
                    netId = wifi.AddNetwork(wifiConfig);
                    wifiManager.SaveConfiguration();
                }

                wifi.UpdateNetwork(wifiConfig); 
                IList<WifiConfiguration> myWifi = wifiManager.ConfiguredNetworks;
                wifiManager.Disconnect();
                wifiManager.EnableNetwork(netId, true);
                wifiManager.Reconnect();
                Toast.MakeText(ApplicationContext, "Reconnecting...", ToastLength.Short).Show();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        protected override void OnResume()
        {
            base.OnResume();
            wifi = (WifiManager)GetSystemService(Context.WifiService);             
            IntentFilter filter = new IntentFilter();
            filter.AddAction(WifiManager.ScanResultsAvailableAction);
            filter.AddAction(WifiManager.SupplicantConnectionChangeAction);
            filter.AddAction(WifiManager.WifiStateChangedAction);
            filter.AddAction(WifiManager.NetworkStateChangedAction);

            wifiReceiver = new WifiReceiver();               
            this.RegisterReceiver(wifiReceiver, filter); 
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.UnregisterReceiver(wifiReceiver);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            wifi.SetWifiEnabled(false);

            if (wifiReceiver != null)
            {
                try
                {
                    this.UnregisterReceiver(wifiReceiver);
                }
                catch (Exception ex)
                {
                    ex.StackTrace.ToString();
                }
            }
        }

        class WifiReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                arrayAdapter.Clear(); 
                string action = intent.Action;
                if (WifiManager.ScanResultsAvailableAction.Equals(action))
                {
                    IList<ScanResult> scanwifinetworks = wifi.ScanResults;
                    foreach (ScanResult wifinetwork in scanwifinetworks)
                    {
                        arrayAdapter.Add(wifinetwork.Ssid + "\n" + wifinetwork.Bssid);                        
                    }                   
                }                
            }
        }
    }
}