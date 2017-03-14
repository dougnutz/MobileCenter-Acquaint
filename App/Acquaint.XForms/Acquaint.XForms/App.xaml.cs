using FormsToolkit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Acquaint.XForms
{
	public partial class App : Application
	{
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
            try
            {
                WerRegisterCustomMetadata("VSMCAppSecret", "34d5ed40-4ff1-4db4-9ef6-0eefbf97e8ab");
                Analytics.TrackEvent("CrashMetadataSet");
            }
            catch (System.Exception e)
            {
                Analytics.TrackEvent("crashInit:" + e.Message.ToString());
            }
            Analytics.TrackEvent("CrashEnabled:"+Crashes.Enabled.ToString());
            Analytics.TrackEvent("HasCrashedInLastSession:" + Crashes.HasCrashedInLastSession.ToString());

        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, ExactSpelling = true, ThrowOnUnmappableChar = false)]
        private static extern int WerRegisterCustomMetadata([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]string key, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]string value);

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

