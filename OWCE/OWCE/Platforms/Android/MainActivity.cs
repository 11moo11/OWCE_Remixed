using Android.App;
using Android.Content.PM;

namespace OWCE.Droid
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static int REQUEST_ENABLE_BT = 3;
    }
}
