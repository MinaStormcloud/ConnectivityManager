using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Nfc;
using Android.Nfc.Tech;
using System;
using System.Text;
using static Android.Nfc.NfcAdapter;

namespace ConnectivityManager
{
    [Activity(Label = "NFC", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@android:style/Theme.Holo.NoActionBar")]
    [IntentFilter(new[] { ActionTechDiscovered })]
    public class NFC_Activity : Activity, ICreateNdefMessageCallback, IOnNdefPushCompleteCallback
    {
        public NFC_Activity()
        {
            nfcHandler = new NFC_Handler(HandlerHandleMessage);
        }
        private NfcAdapter nfcAdapter;
        
        EditText editText;
        string newLine = System.Environment.NewLine;
        private readonly Handler nfcHandler;
        private const int MESSAGE_SENT = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.NFC);

            editText = FindViewById<EditText>(Resource.Id.editText);
            nfcAdapter = NfcAdapter.GetDefaultAdapter(this);

            Button btnSettings = FindViewById<Button>(Resource.Id.btnSettings);            

            btnSettings.Click += delegate {
                
                NfcManager nfc = (NfcManager)GetSystemService(Context.NfcService);
                Intent nfcIntent = new Intent(Android.Provider.Settings.ActionNfcSettings);
                StartActivityForResult(nfcIntent, 0);
            };

            if (!nfcAdapter.IsEnabled)
            {
                Toast.MakeText(ApplicationContext, "NFC is disabled.", ToastLength.Long).Show();
                return;
            }

            if (nfcAdapter == null)
            {
                Toast.MakeText(ApplicationContext, "NFC is not available.", ToastLength.Long).Show(); // Device does not support NFC
            }
            else
            {
                nfcAdapter.SetNdefPushMessageCallback(this, this); // Register callback to set NDEF message                
                nfcAdapter.SetOnNdefPushCompleteCallback(this, this); // Register callback to listen for message-sent success
            }            
        }           

        

        public NdefMessage CreateNdefMessage(NfcEvent evt)
        {
            DateTime time = DateTime.Now;
            var text = ("Beam me up!\n\n" + "Beam : " +
                time.ToString("HH:mm:ss"));
            NdefMessage msg = new NdefMessage(
                new NdefRecord[]{ CreateMimeRecord (
            editText.Text.ToString(),
            Encoding.UTF8.GetBytes (text)) });

            return msg;
        }        

        public NdefRecord CreateMimeRecord(String mimeType, byte[] payload)
        {
            byte[] mimeBytes = Encoding.UTF8.GetBytes(mimeType);
            NdefRecord mimeRecord = new NdefRecord(NdefRecord.TnfMimeMedia, mimeBytes, new byte[0], payload);
            return mimeRecord;
        }
        public void OnNdefPushComplete(NfcEvent arg0)
        {
            try
            {   // A handler is needed to send messages to the activity when this callback occurs, because it happens from a binder thread 
                nfcHandler.ObtainMessage(MESSAGE_SENT).SendToTarget(); // used to be mHandler
            }
            catch (Exception ex)
            {
                ex.StackTrace.ToString();
            }
        }

        public void ReceiveMessage()
        {
            var intent = new Intent(this, this.Class);
            IParcelable[] rawMsgs = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            NdefMessage msg = (NdefMessage)rawMsgs[0];
            Toast.MakeText(ApplicationContext, "Message from the dark side: " + msg, ToastLength.Short).Show();
        }

        protected override void OnResume()
        {
            base.OnResume();           
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);            
        }

        private void ProcessIntent(Intent intent)
        {
            IParcelable[] rawMsgs = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            NdefMessage msg = (NdefMessage)rawMsgs[0];// only one message sent during the beam
            // record 0 contains the MIME type, record 1 is the AAR, if present
            editText.Text = Encoding.UTF8.GetString(msg.GetRecords()[0].GetPayload()); // For the Send button            
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (nfcAdapter != null)
                nfcAdapter.DisableForegroundDispatch(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (nfcAdapter != null)
            {
                nfcAdapter.Dispose();
                nfcAdapter = null;
            }
        }

        protected void HandlerHandleMessage(Message msg)
        {
            switch (msg.What)
            {
                case MESSAGE_SENT:
                    DateTime time = DateTime.Now;
                    var text = (" Beam Time: " + time.ToString("HH:mm:ss"));
                    Toast.MakeText(ApplicationContext, "Message sent: " + editText.Text.ToString() + text, ToastLength.Short).Show();           
            break;
            }
        }
    }

    public class NFC_Handler : Handler
    {
        public NFC_Handler(Action<Message> handler)
        {
            this.handle_message = handler;
        }

        Action<Message> handle_message;
        public override void HandleMessage(Message msg)
        {
            handle_message(msg);
        }
    }
}