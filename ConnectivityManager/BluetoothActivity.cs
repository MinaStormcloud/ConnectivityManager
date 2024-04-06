using System;
using Android.Bluetooth;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Java.Util;

namespace ConnectivityManager
{
    [Activity(Label = "Bluetooth", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.NoActionBar")]
    public class BluetoothActivity : Activity
    {
        public static ListView BluetoothListView;
        public static ArrayAdapter bluetoothArrayAdapter;        
        public static BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        public static BluetoothServerSocket bluetoothServerSocket;
        private static int DISCOVERABLE_BT_REQUEST_CODE = 0;
        private static int ENABLE_BT_REQUEST_CODE = 1;
        private static int DISCOVERABLE_DURATION = 600;
        public const string EXTRA_DEVICE_ADDRESS = "device_address";
        private static UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
        private BluetoothReceiver bluetoothReceiver;        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Bluetooth);
            bluetoothAdapter.Disable(); // Turn off Bluetooth when the BluetoothActivity is called                     
            
            ToggleButton btnToggleBluetooth = FindViewById<ToggleButton>(Resource.Id.btnToggleBluetooth);
            BluetoothListView = FindViewById<ListView>(Resource.Id.BluetoothListView);
            bluetoothArrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);
            BluetoothListView.Adapter = bluetoothArrayAdapter;
            BluetoothListView.ItemClick += SelectedDeviceClickListener;

            IntentFilter filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);

            bluetoothReceiver = new BluetoothReceiver();
            this.RegisterReceiver(bluetoothReceiver, filter);

            btnToggleBluetooth.Click += delegate
            {
                if (bluetoothAdapter == null)
                {
                    Toast.MakeText(ApplicationContext, "Your device does not support Bluetooth", ToastLength.Short).Show();
                    btnToggleBluetooth.Checked.Equals(false);
                }
                else
                {
                    if (btnToggleBluetooth.Checked.Equals(true))
                    {
                        if (!bluetoothAdapter.IsEnabled)
                        {
                            Intent enableBluetoothIntent = new Intent(BluetoothAdapter.ActionRequestDiscoverable);
                            enableBluetoothIntent.PutExtra(BluetoothAdapter.ExtraDiscoverableDuration, DISCOVERABLE_DURATION);
                            StartActivityForResult(enableBluetoothIntent, ENABLE_BT_REQUEST_CODE);
                        }
                        else
                        {
                            Toast.MakeText(ApplicationContext, "Discovering devices...", ToastLength.Short).Show();
                            DoDiscovery();
                            MakeDiscoverable();
                        }
                    }
                    else
                    {
                        bluetoothAdapter.Disable();
                        bluetoothArrayAdapter.Clear();                        
                    }
                }
            };
        }

        private void DoDiscovery()
        {
            if (bluetoothAdapter.IsDiscovering)
            {
                bluetoothAdapter.CancelDiscovery();
            }

            bluetoothAdapter.StartDiscovery();
            Toast.MakeText(ApplicationContext, "Searching for Bluetooth devices", ToastLength.Short).Show();
        }

        public void SelectedDeviceClickListener(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (bluetoothAdapter.IsDiscovering)
            {
                bluetoothAdapter.CancelDiscovery();
                Toast.MakeText(this, e.Position.ToString(), ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(this, e.Position.ToString(), ToastLength.Short).Show();
            }
            
            int position = e.Position;
            string itemValue = (string)BluetoothListView.GetItemAtPosition(position);
            string MAC = itemValue.Substring(itemValue.Length - 17);
            BluetoothDevice bluetoothDevice = bluetoothAdapter.GetRemoteDevice(MAC);

            System.Collections.Generic.ICollection<BluetoothDevice> devices = bluetoothAdapter.BondedDevices;

            if (devices != null && devices.Count > 0)
            {
                foreach (BluetoothDevice bd in devices)
                {
                    if (bd.Address == bluetoothDevice.Address) 
                    {
                        Toast.MakeText(ApplicationContext, "These devices are already bonded", ToastLength.Short).Show();

                        BluetoothManager blue = (BluetoothManager)GetSystemService(Context.BluetoothService);
                        Intent blueIntent = new Intent(Android.Provider.Settings.ActionBluetoothSettings);
                        StartActivityForResult(blueIntent, 0); // Displays the built-in Bluetooth menu
                    }
                }
            }

            ConnectingThread t = new ConnectingThread(bluetoothDevice); // Initiate a connection request in a separate thread
            t.Start();
        }

        protected void MakeDiscoverable()
        {
            Intent discoverableIntent = new Intent(BluetoothAdapter.ActionRequestDiscoverable);
            discoverableIntent.PutExtra(BluetoothAdapter.ExtraDiscoverableDuration, DISCOVERABLE_DURATION);
            StartActivityForResult(discoverableIntent, DISCOVERABLE_BT_REQUEST_CODE);
        }

        protected override void OnResume()
        {
            base.OnResume();
            bluetoothReceiver = new BluetoothReceiver();
            this.RegisterReceiver(bluetoothReceiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryStarted));
            bluetoothAdapter.StartDiscovery();
        }

        protected override void OnPause()
        {
            base.OnPause();
            bluetoothAdapter.CancelDiscovery();
            this.UnregisterReceiver(bluetoothReceiver);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (bluetoothAdapter.IsDiscovering)
            {
                bluetoothAdapter.CancelDiscovery();
            }

            bluetoothAdapter.Disable();

            if (bluetoothReceiver != null)
            {
                try
                {
                    this.UnregisterReceiver(bluetoothReceiver);
                }
                catch (Exception ex)
                {
                    ex.StackTrace.ToString();
                }
            }
        }

        class BluetoothReceiver : BroadcastReceiver
        {            
            public override void OnReceive(Context context, Intent intent)
            {                
                string action = intent.Action;

                if (BluetoothDevice.ActionFound.Equals(action))
                {                                      
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);  
                    bluetoothArrayAdapter.Add(device.Name + "\n" + device.Address);
                }
                else if (BluetoothAdapter.ActionDiscoveryFinished.Equals(action) && bluetoothAdapter.IsEnabled)
                {
                    Toast.MakeText(context, "Discovery has finished!", ToastLength.Short).Show();
                }
            }
        }

        class ListeningThread : Java.Lang.Thread
        {
            public ListeningThread()
            {
                BluetoothServerSocket temp = null;
                try
                {
                    temp = bluetoothAdapter.ListenUsingInsecureRfcommWithServiceRecord("BluetoothConnection", uuid); 
                }
                catch (Java.IO.IOException e)
                {
                    e.StackTrace.ToString();
                }

                bluetoothServerSocket = temp;
            }

            public override void Run()
            {
                BluetoothSocket bluetoothSocket = null;
                
                while (true)
                {
                    try
                    {
                        bluetoothSocket = bluetoothServerSocket.Accept();
                    }
                    catch (Java.IO.IOException e)
                    {
                        e.StackTrace.ToString();
                        break;
                    }
                    
                    if (bluetoothSocket != null)
                    {
                        try
                        {   
                            bluetoothServerSocket.Close();
                        }
                        catch (Java.IO.IOException e)
                        {
                            e.StackTrace.ToString();
                        }
                        break;
                    }
                }
            }

            public void Cancel()
            {
                try
                {
                    bluetoothServerSocket.Close();
                }
                catch (Java.IO.IOException e)
                {
                    e.StackTrace.ToString();
                }
            }
        }

        class ConnectingThread : Java.Lang.Thread
        {
            private BluetoothSocket bluetoothSocket;            

            public ConnectingThread(BluetoothDevice device)
            {
                try
                {
                    bluetoothSocket = (BluetoothSocket)device.Class.GetMethod("createRfcommSocket", new Java.Lang.Class[] { Java.Lang.Integer.Type }).Invoke(device, 1);
                }
                catch (Java.IO.IOException e)
                {
                    e.StackTrace.ToString();
                }
            }

            public override void Run()
            {
                bluetoothAdapter.CancelDiscovery(); 

                try
                { 
                    bluetoothSocket.Connect(); 
                }
                catch (Java.IO.IOException connectException)
                {
                    connectException.StackTrace.ToString();
                    try
                    {
                        bluetoothSocket.Close();
                    }
                    catch (Java.IO.IOException CloseException)
                    {
                        CloseException.StackTrace.ToString();
                    }
                }                
            }

            public void Cancel()
            {
                try
                {
                    bluetoothSocket.Close();
                }
                catch (Java.IO.IOException e)
                {
                    e.StackTrace.ToString();
                }
            }
        }

        class ConnectedThread : Java.Lang.Thread
        {
            private BluetoothSocket bSocket;
            private System.IO.Stream InStream;
            private System.IO.Stream OutStream;

            public ConnectedThread(BluetoothSocket socket)
            {
                bSocket = socket;
                System.IO.Stream In = null;
                System.IO.Stream Out = null;

                try  
                {
                    In = socket.InputStream;
                    Out = socket.OutputStream;
                }
                catch (Java.IO.IOException e)
                {
                    e.StackTrace.ToString();
                }

                InStream = In;
                OutStream = Out;
            }

            public override void Run()
            {
                byte[] buffer = new byte[1024];
                int bytes;

                while (true)  
                {
                    try
                    {                         
                        bytes = InStream.Read(buffer, 0, buffer.Length);
                    }
                    catch (Java.IO.IOException e)
                    {
                        e.StackTrace.ToString();
                        break;
                    }
                }
            }
            
 			public void Write(byte[] buffer)
            {
                try
                {
                    OutStream.Write(buffer, 0, buffer.Length);

                }
                catch (Java.IO.IOException e)
                {
                    e.StackTrace.ToString();
                }
            }

            public void Cancel()
            {
                try
                {
                    bSocket.Close();
                }
                catch (Java.IO.IOException e)
                {
                    e.StackTrace.ToString();
                }
            }
        }
    }
}