using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Prism.Unity;
using Microsoft.Practices.Unity;
using Plugin.FelicaReader;
using Plugin.FelicaReader.Abstractions;
using Android.Nfc;
using Android.Content;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Nfc.Tech;

namespace Sample.FelicaReader.Droid
{
    [Activity(Label = "Sample.FelicaReader", Icon = "@drawable/icon", MainLauncher = true,
        LaunchMode = Android.Content.PM.LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { NfcAdapter.ActionTechDiscovered })]
    [MetaData(NfcAdapter.ActionTechDiscovered, Resource = "@xml/nfc_filter")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private IFelicaReader felicaReader;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.tabs;
            ToolbarResource = Resource.Layout.toolbar;

            base.OnCreate(bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);

            CrossFelicaReader.Init(this, GetType());
            this.felicaReader = CrossFelicaReader.Current;
            LoadApplication(new App(new AndroidInitializer()));

            this.ProcessActionTechDiscoveredIntent(this.Intent);
        }

        private void ProcessActionTechDiscoveredIntent(Intent intent)
        {
            string action = intent.Action;
            if (action != NfcAdapter.ActionTechDiscovered)
            {
                return;
            }

            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Android.Nfc.Tag;
            if (tag != null)
            {
                var subject = this.felicaReader.WhenCardFound() as Subject<IFelicaCardMedia>;
                NfcF nfc = NfcF.Get(tag);
                nfc.Connect();
                subject.OnNext(new FelicaCardMediaImplementation(nfc));
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            ProcessActionTechDiscoveredIntent(intent);
            return;
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.felicaReader.DisableForeground();
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.felicaReader.EnableForeground();
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IUnityContainer container)
        {

        }
    }
}

