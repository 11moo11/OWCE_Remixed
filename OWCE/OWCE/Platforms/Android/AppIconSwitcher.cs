using Android.Content;
using Android.Content.PM;

namespace OWCE.Droid
{
    /// <summary>
    /// Changes the launcher icon. Android has no way to swap an app icon directly, so the manifest has one launcher entry (an
    /// activity-alias) per icon colour, and this turns one on and the others off.
    /// </summary>
    public static class AppIconSwitcher
    {
        const string Prefix = "com.owcommunity.owce.Icon_";

        public static void Set(string name)
        {
            var context = Android.App.Application.Context;
            var packageManager = context.PackageManager;
            var package = context.PackageName;

            ComponentName Alias(string variant) => new ComponentName(package, Prefix + variant);

            var wanted = Alias(name);
            if (packageManager.GetComponentEnabledSetting(wanted) == ComponentEnabledState.Enabled)
            {
                // Already the icon in use (the default setting means "as in the manifest", which is only classic).
                return;
            }

            // Turn the new one on first so there is always a launcher entry, then the others off.
            packageManager.SetComponentEnabledSetting(wanted, ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
            foreach (var variant in OWCE.AppIconCatalog.Names)
            {
                if (variant != name)
                {
                    packageManager.SetComponentEnabledSetting(Alias(variant), ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
                }
            }
        }
    }
}
