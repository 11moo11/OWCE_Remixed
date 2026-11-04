using Android.App;
using Android.Content.PM;

namespace OWCE.Droid
{
    // The launcher entries are the activity-aliases in AndroidManifest.xml (one per icon colour), which point at this activity by name,
    // so it has a fixed name and is not a launcher itself.
    [Activity(
        Name = "com.owcommunity.owce.MainActivity",
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static int REQUEST_ENABLE_BT = 3;
    }
}
