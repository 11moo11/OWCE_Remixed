using Android.App;
using Android.Content.PM;

namespace OWCE.Droid
{
    // One activity per icon colour, each with its own splash screen theme (a splash screen can't change colour at run time).
    // They are all the same app: the launcher entries in AndroidManifest.xml point at one each. MainActivity is the original yellow.

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_purple",
        Theme = "@style/Splash_purple",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_purple : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_indigo",
        Theme = "@style/Splash_indigo",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_indigo : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_blue",
        Theme = "@style/Splash_blue",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_blue : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_teal",
        Theme = "@style/Splash_teal",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_teal : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_green",
        Theme = "@style/Splash_green",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_green : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_lime",
        Theme = "@style/Splash_lime",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_lime : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_amber",
        Theme = "@style/Splash_amber",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_amber : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_orange",
        Theme = "@style/Splash_orange",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_orange : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_red",
        Theme = "@style/Splash_red",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_red : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_pink",
        Theme = "@style/Splash_pink",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_pink : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_brown",
        Theme = "@style/Splash_brown",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_brown : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_slate",
        Theme = "@style/Splash_slate",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_slate : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_black",
        Theme = "@style/Splash_black",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_black : MainActivity
    {
    }

    [Activity(
        Name = "com.owcommunity.owce.MainActivity_white",
        Theme = "@style/Splash_white",
        MainLauncher = false,
        Exported = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity_white : MainActivity
    {
    }
}
