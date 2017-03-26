using Plugin.FelicaReader.Abstractions;
using Plugin.FelicaReader;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Sample.FelicaReader.ViewModels
{
    public class MainPageViewModel : BindableBase, INavigationAware
    {
        private IFelicaReader felicaReader;

        private string idmString;
        public string IDmString
        {
            get { return idmString; }
            set { SetProperty(ref idmString, value); }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }

        private IDisposable subscription;
        
        public DelegateCommand ReadButtonClickedCommand { get; private set; }

        public DelegateCommand ClearButtonClickedCommand { get; private set; }


        public MainPageViewModel()
        {
            this.felicaReader = CrossFelicaReader.Current;
            this.ReadButtonClickedCommand = new DelegateCommand(() => ButtonClicked());
            this.ClearButtonClickedCommand = new DelegateCommand(() => ClearButtonClicked());
        }
                

        private void ButtonClicked()
        {
            this.felicaReader.FindCard();
        }

        private void ClearButtonClicked()
        {
            this.Message = String.Empty;
            this.IDmString = String.Empty;
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
            this.subscription.Dispose();
        }
        
        public void OnNavigatingTo(NavigationParameters parameters)
        {
            
        }

        public  void OnNavigatedTo(NavigationParameters parameters)
        {
            this.subscription = this.felicaReader.WhenCardFound().Subscribe(async x =>
            {
                try
                {
                    var byteIdm = await x.GetIdm();
                    this.IDmString = BitConverter.ToString(byteIdm);
                    System.Diagnostics.Debug.WriteLine("Idm: {0}", this.IDmString, 0x00);

                    var result = await x.ReadWithoutEncryption(byteIdm, 0x008b, 1, new byte[] { 0x80, 0x00 });
                    string resStr = BitConverter.ToString(result.PacketData);
                    System.Diagnostics.Debug.WriteLine("Res: {0}(len={1})", resStr, result.PacketData.Length);

                    this.Message = String.Format("Res: {0}", resStr, 0x00);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
                finally
                {
                    x.Dispose();
                }
            },
            onError: x =>
            {
                System.Diagnostics.Debug.WriteLine(x.Message);
            });
        }
    }
}
