namespace OWCE.Themes
{
    public partial class MaterialTheme : ResourceDictionary
    {
        public MaterialTheme()
        {
            InitializeComponent();
        }

        /// <summary>The Material theme with its scheme built from a seed colour, in light or dark.</summary>
        public static MaterialTheme Create(string seedHex, bool dark)
        {
            var theme = new MaterialTheme();

            // The XAML holds the light baseline purple, so only anything else needs the generated colours.
            var seed = Color.FromArgb(seedHex);
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
