using Foundation;
using UIKit;
using WatchConnectivity;

namespace OWCE.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Apple Watch session manager
            WCSessionManager.SharedManager.StartSession();

            return base.FinishedLaunching(application, launchOptions);
        }
    }
}
