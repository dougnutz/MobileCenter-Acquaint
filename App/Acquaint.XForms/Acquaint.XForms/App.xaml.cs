using FormsToolkit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using System;
using System.Runtime.InteropServices;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Acquaint.XForms
{
    public partial class App : Application
	{
        private const string WatsonKey = "VSMCAppSecret";

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr LoadPackagedLibrary(string libname);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        private delegate int RegisterCustomMetadataDelegate([MarshalAs(UnmanagedType.LPWStr)]string key, [MarshalAs(UnmanagedType.LPWStr)]string value);

        public App()
		{
            InitializeComponent();

			SubscribeToDisplayAlertMessages();

			// The navigation logic startup needs to diverge per platform in order to meet the UX design requirements
			if (Xamarin.Forms.Device.OS == TargetPlatform.Android)
			{
				// if this is an Android device, set the MainPage to a new SplashPage
				MainPage = new SplashPage();
			}
			else
			{
				// create a new NavigationPage, with a new AcquaintanceListPage set as the Root
				var navPage =
					new NavigationPage(
						new AcquaintanceListPage()
						{
							BindingContext = new AcquaintanceListViewModel(),
							Title = "Acquaintances"
						})
					{
						BarBackgroundColor = Color.FromHex("547799")
					};

			    navPage.BarTextColor = Color.White;

                // set the MainPage of the app to the navPage
                MainPage = navPage;
			}
            MobileCenter.LogLevel = LogLevel.Verbose;
            MobileCenter.Start("android=34d5ed40-4ff1-4db4-9ef6-0eefbf97e8ab;uwp=34d5ed40-4ff1-4db4-9ef6-0eefbf97e8ab;ios=7056d0a8-3a01-49e4-8fca-f5eff47839df", typeof(Analytics),typeof(Crashes));


            var handle = LoadPackagedLibrary("kernel32.dll");
            if (handle == IntPtr.Zero)
            {
                Analytics.TrackEvent("Pointer is zero");
                // throw new MobileCenterException(ErrorMessage);
            }
            Analytics.TrackEvent("handlefound");
            try
            {
                var address = GetProcAddress(handle, "WerRegisterCustomMetadata");
                Analytics.TrackEvent("GetProc");
                if (address == IntPtr.Zero)
                {
                    Analytics.TrackEvent("GetProcIszero");
                    // throw new MobileCenterException(ErrorMessage);
                }
                var registrationMethod = Marshal.GetDelegateForFunctionPointer(address, typeof(RegisterCustomMetadataDelegate));
                if (registrationMethod == null)
                {
                    Analytics.TrackEvent("MethodIsNull");
                    // throw new MobileCenterException(ErrorMessage);
                }
                registrationMethod.DynamicInvoke(WatsonKey, "34d5ed40-4ff1-4db4-9ef6-0eefbf97e8ab");
                Analytics.TrackEvent("CrashMetadataSet");
            }
            catch(Exception e)
            {
                Analytics.TrackEvent("crashInit:" + e.Message.ToString());
            }
            finally
            {
                FreeLibrary(handle);
            }

            //try
            //{
            //  //  WerRegisterCustomMetadata("VSMCAppSecret", "34d5ed40-4ff1-4db4-9ef6-0eefbf97e8ab");

            //    Analytics.TrackEvent("CrashMetadataSet");
            //}
            //catch (System.Exception e)
            //{
            //    Analytics.TrackEvent("crashInit:" + e.Message.ToString());
            //}
            Analytics.TrackEvent("CrashEnabled:"+Crashes.Enabled.ToString());

        }

        //[System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, ExactSpelling = true)]
        //private static extern int WerRegisterCustomMetadata([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]string key, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]string value);

        /// <summary>
        /// Subscribes to messages for displaying alerts.
        /// </summary>
        static void SubscribeToDisplayAlertMessages()
		{
			MessagingService.Current.Subscribe<MessagingServiceAlert>(MessageKeys.DisplayAlert, async (service, info) => {
				var task = Current?.MainPage?.DisplayAlert(info.Title, info.Message, info.Cancel);
				if (task != null)
				{
					await task;
					info?.OnCompleted?.Invoke();
				}
			});

			MessagingService.Current.Subscribe<MessagingServiceQuestion>(MessageKeys.DisplayQuestion, async (service, info) => {
				var task = Current?.MainPage?.DisplayAlert(info.Title, info.Question, info.Positive, info.Negative);
				if (task != null)
				{
					var result = await task;
					info?.OnCompleted?.Invoke(result);
				}
			});
		}
	}
}

