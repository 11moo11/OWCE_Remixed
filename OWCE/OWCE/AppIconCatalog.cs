using OWCE.Themes;

namespace OWCE
{
    /// <summary>
    /// Picks the launcher icon that goes with the theme: the icon colour closest to the theme's primary colour. (There is one icon
    /// per colour below; a launcher icon can't be drawn at run time, so a custom colour gets the nearest of these.)
    /// </summary>
    public static class AppIconCatalog
    {
        static readonly (string Name, string Hex)[] Variants =
        {
            ("classic", "#F5F500"),
            ("purple", "#6750A4"),
            ("indigo", "#3F5AA6"),
            ("blue", "#1E88E5"),
            ("teal", "#00897B"),
            ("green", "#2E7D32"),
            ("lime", "#9E9D24"),
            ("amber", "#F9A825"),
            ("orange", "#EF6C00"),
            ("red", "#D32F2F"),
            ("pink", "#C2185B"),
            ("brown", "#795548"),
            ("slate", "#546E7A"),
            ("black", "#101010"),
            ("white", "#F5F5F5")
        };

        public static IEnumerable<string> Names => Variants.Select(x => x.Name);

        /// <summary>The icon that suits a theme.</summary>
        public static string Choose(ThemeSpec spec)
        {
            var target = Color.FromArgb(spec.Primary);
            var best = Variants[0].Name;
            var bestDistance = double.MaxValue;

            foreach (var (name, hex) in Variants)
            {
                var c = Color.FromArgb(hex);

                // Distance in RGB, weighted for how the eye sees green, red and blue.
                var dr = target.Red - c.Red;
                var dg = target.Green - c.Green;
                var db = target.Blue - c.Blue;
                var distance = 0.3 * dr * dr + 0.59 * dg * dg + 0.11 * db * db;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = name;
                }
            }

            return best;
        }

        /// <summary>
        /// Switches the launcher icon to suit the theme in use. This is done when the app goes to the background, because some
        /// launchers close the app when its icon changes.
        /// </summary>
        public static void ApplyForCurrentTheme()
        {
            try
            {
#if ANDROID
                OWCE.Droid.AppIconSwitcher.Set(Choose(ThemeManager.Current));
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not change the app icon: {ex.Message}");
            }
        }
    }
}
