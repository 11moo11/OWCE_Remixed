namespace OWCE.Themes
{
    public partial class MaterialTheme : ResourceDictionary
    {
        public const string SeedPreferenceKey = "material_seed";
        public const string DarkPreferenceKey = "material_dark";

        /// <summary>The colour the Material theme's scheme is built from.</summary>
        public static Color Seed
        {
            get => Color.FromArgb(Preferences.Get(SeedPreferenceKey, MaterialPalette.DefaultSeed));
            set => Preferences.Set(SeedPreferenceKey, value.ToRgbaHex());
        }

        public static bool IsDark
        {
            get => Preferences.Get(DarkPreferenceKey, false);
            set => Preferences.Set(DarkPreferenceKey, value);
        }

        public MaterialTheme()
        {
            InitializeComponent();
        }

        /// <summary>The Material theme in the user's chosen colour and light or dark mode.</summary>
        public static MaterialTheme Create()
        {
            var theme = new MaterialTheme();

            // The XAML holds the light baseline purple, so only anything else needs the generated colours.
            var seed = Seed;
            var dark = IsDark;
            if (dark || seed.ToRgbaHex() != Color.FromArgb(MaterialPalette.DefaultSeed).ToRgbaHex())
            {
                var colors = MaterialPalette.Generate(seed, dark);
                foreach (var item in colors)
                {
                    theme[item.Key] = item.Value;
                }

                // Some properties (Border.Stroke, Shape.Fill) take a brush, so those colours also exist as brushes.
                theme["OnBackgroundBrush"] = new SolidColorBrush(colors["OnBackgroundColor"]);
                theme["MutedTextBrush"] = new SolidColorBrush(colors["MutedTextColor"]);
                theme["DividerBrush"] = new SolidColorBrush(colors["DividerColor"]);
                theme["ButtonBorderBrush"] = new SolidColorBrush(colors["ButtonBorderColor"]);
                theme["OnLightsBrush"] = new SolidColorBrush(colors["OnLights"]);
                theme["AccentBrush"] = new SolidColorBrush(colors["AccentColor"]);
            }

            return theme;
        }
    }
}
