using CommunityToolkit.Mvvm.Messaging;

namespace OWCE.Themes
{
    /// <param name="Primary">The preset's three seed colours (hex), which are what the colour pickers start from.</param>
    public sealed record ThemeInfo(string Id, string Name, string Description, Func<ResourceDictionary> Create, CornerStyle Corners, DepthStyle Depth,
        bool Dark, string Primary, string Secondary, string Accent);

    /// <summary>Sent after a different theme has been applied.</summary>
    public sealed class ThemeChangedMessage
    {
    }

    /// <summary>
    /// Themes are ResourceDictionaries (in Themes/) that all define the same keys: colours such as BackgroundGradientStart,
    /// SpeedPanel and OnSpeedPanel, plus the panel and button styles. The rest of the app uses those keys with
    /// DynamicResource, so applying a theme restyles everything that is already on screen.
    /// To add a theme: copy an existing one, change its values, and add it to <see cref="Themes"/>.
    /// </summary>
    public static class ThemeManager
    {
        public const string DefaultThemeId = "default";

        /// <summary>The built in presets. Each is a colour palette plus default corners and depth.</summary>
        public static IReadOnlyList<ThemeInfo> Themes { get; } = new List<ThemeInfo>
        {
            new ThemeInfo(DefaultThemeId, "Default", "The original OWCE look.", () => new DefaultTheme(), CornerStyle.VeryRound, DepthStyle.DropShadow, false, "#1F4FD8", "#FF8C00", "#CC00CC"),
            new ThemeInfo("material", "Material", "Material Design 3: tonal surfaces and large rounded corners. Pick any colour, and dark mode.", () => new MaterialTheme(), CornerStyle.VeryRound, DepthStyle.DropShadow, false, "#6750A4", "#625B71", "#7D5260"),
            new ThemeInfo("devil-trigger", "Devil Trigger", "Inspired by Devil May Cry: black, crimson and bone with hard edges.", () => new DevilTriggerTheme(), CornerStyle.Sharp, DepthStyle.EdgeLight, true, "#FF5C70", "#D4A62A", "#B30D26"),
            new ThemeInfo("codec", "Codec", "Inspired by Metal Gear Solid: green phosphor on black.", () => new CodecTheme(), CornerStyle.Sharp, DepthStyle.EdgeLight, true, "#4DFF88", "#0E8F44", "#00B34A"),
            new ThemeInfo("underground", "Underground", "Inspired by Need for Speed Underground: night city neon.", () => new UndergroundTheme(), CornerStyle.Sharp, DepthStyle.EdgeLight, true, "#00E5FF", "#FF2E88", "#FFB400"),
            new ThemeInfo("nanotech", "Nanotech", "Inspired by Ratchet & Clank: deep blue with bright orange.", () => new NanotechTheme(), CornerStyle.VeryRound, DepthStyle.DropShadow, true, "#FFB347", "#00C2FF", "#FF6B00"),
            new ThemeInfo("master-thief", "Master Thief", "Inspired by Sly Cooper: rooftop-at-night indigo with gold.", () => new MasterThiefTheme(), CornerStyle.Round, DepthStyle.EdgeLight, true, "#F2B705", "#5B49C8", "#2EC4B6"),
            new ThemeInfo("keyblade", "Keyblade", "Inspired by Kingdom Hearts II: command-menu navy and sky blue with gold.", () => new KeybladeTheme(), CornerStyle.Round, DepthStyle.EdgeLight, true, "#FFD65C", "#1B4FA8", "#7FB2FF"),
            new ThemeInfo("city-escape", "City Escape", "Inspired by Sonic Adventure 2: black and yellow with hero blue and rival red.", () => new CityEscapeTheme(), CornerStyle.Sharp, DepthStyle.EdgeLight, true, "#FFD500", "#0A5BD8", "#C4161F"),
        };

        const string CurrentKey = "theme_current";
        const string UserStylesKey = "theme_user_styles";

        static ResourceDictionary _current;
        static List<ThemeSpec> _userStyles;

        /// <summary>The theme in use, including any changes made to it that have not been saved as a style.</summary>
        public static ThemeSpec Current { get; private set; } = PresetSpec(DefaultThemeId);

        public static string CurrentId => Current.Id;

        public static ThemeInfo Find(string id) => Themes.FirstOrDefault(x => x.Id == id) ?? Themes[0];

        /// <summary>A preset as a spec: its palette with its default corners and depth, and the system font.</summary>
        public static ThemeSpec PresetSpec(string id)
        {
            var info = Find(id);
            return new ThemeSpec() { Id = info.Id, Name = info.Name, BaseId = info.Id, Corners = info.Corners, Depth = info.Depth, Dark = info.Dark, Primary = info.Primary, Secondary = info.Secondary, Accent = info.Accent };
        }

        /// <summary>The styles the user has saved.</summary>
        public static IReadOnlyList<ThemeSpec> UserStyles => LoadUserStyles();

        /// <summary>The presets followed by the user's saved styles.</summary>
        public static IEnumerable<ThemeSpec> AllSpecs => Themes.Select(x => PresetSpec(x.Id)).Concat(LoadUserStyles());

        public static bool IsUserStyle(string id) => LoadUserStyles().Any(x => x.Id == id);

        /// <summary>The spec for a preset or saved style, or null.</summary>
        public static ThemeSpec FindSpec(string id) => AllSpecs.FirstOrDefault(x => x.Id == id);

        static List<ThemeSpec> LoadUserStyles()
        {
            if (_userStyles != null)
            {
                return _userStyles;
            }

            _userStyles = new List<ThemeSpec>();
            try
            {
                var json = Preferences.Get(UserStylesKey, string.Empty);
                if (string.IsNullOrEmpty(json) == false)
                {
                    var maps = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json);
                    _userStyles.AddRange(maps.Select(ThemeSpec.FromMap));
                }
            }
            catch (Exception)
            {
                // Unreadable saved styles are ignored.
            }

            return _userStyles;
        }

        static void SaveUserStyles()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(LoadUserStyles().Select(x => x.ToMap()).ToList());
            Preferences.Set(UserStylesKey, json);
        }

        /// <summary>Saves the theme in use as a new style with this name, and makes it the current one.</summary>
        public static ThemeSpec SaveAsNewStyle(string name)
        {
            var style = Current.Clone();
            style.Id = "user-" + Guid.NewGuid().ToString("N");
            style.Name = name;
            LoadUserStyles().Add(style);
            SaveUserStyles();

            Apply(style);
            return style;
        }

        /// <summary>Saves the changes made to the current saved style.</summary>
        public static void UpdateCurrentStyle()
        {
            var styles = LoadUserStyles();
            var index = styles.FindIndex(x => x.Id == Current.Id);
            if (index >= 0)
            {
                styles[index] = Current.Clone();
                SaveUserStyles();
            }
        }

        public static void DeleteStyle(string id)
        {
            LoadUserStyles().RemoveAll(x => x.Id == id);
            SaveUserStyles();

            if (Current.Id == id)
            {
                Apply(PresetSpec(DefaultThemeId));
            }
        }

        /// <summary>Applies the theme from the last run (called at startup).</summary>
        public static void ApplySaved()
        {
            ThemeSpec spec = null;
            try
            {
                var json = Preferences.Get(CurrentKey, string.Empty);
                if (string.IsNullOrEmpty(json) == false)
                {
                    spec = ThemeSpec.FromMap(System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json));
                }
            }
            catch (Exception)
            {
            }

            // Before the builder existed only a preset id was saved.
            spec ??= PresetSpec(Preferences.Get("theme", DefaultThemeId));
            Apply(spec);
        }

        /// <summary>Applies a preset (or saved style) by id, with its own settings.</summary>
        public static void Apply(string id) => Apply((FindSpec(id) ?? PresetSpec(DefaultThemeId)).Clone());

        /// <summary>Applies a theme to the running app and remembers it. Must be called on the UI thread.</summary>
        public static void Apply(ThemeSpec spec)
        {
            var merged = Application.Current.Resources.MergedDictionaries;

            // Add the new theme before removing the old one: the last dictionary added wins, so everything switches in one go
            // without a moment where the theme's keys are missing.
            var next = CreateDictionary(spec);
            merged.Add(next);

            if (_current != null)
            {
                merged.Remove(_current);
            }

            _current = next;
            Current = spec.Clone();

            try
            {
                Preferences.Set(CurrentKey, System.Text.Json.JsonSerializer.Serialize(Current.ToMap()));
            }
            catch (Exception)
            {
            }

            WeakReferenceMessenger.Default.Send(new ThemeChangedMessage());
        }

        /// <summary>Builds the resources for a spec: the preset's palette, then the font, corners and depth on top.</summary>
        public static ResourceDictionary CreateDictionary(ThemeSpec spec)
        {
            var preset = Find(spec.BaseId);
            var dictionary = preset.Create();

            // A preset's own hand made colours are kept as they are, until the mode or a colour is changed.
            if (HasCustomColors(spec, preset))
            {
                var colors = MaterialPalette.Generate(Color.FromArgb(spec.Primary), Color.FromArgb(spec.Secondary), Color.FromArgb(spec.Accent), spec.Dark);
                foreach (var item in colors)
                {
                    dictionary[item.Key] = item.Value;
                }

                // Some properties (Border.Stroke, Shape.Fill) take a brush, so those colours also exist as brushes.
                dictionary["OnBackgroundBrush"] = new SolidColorBrush(colors["OnBackgroundColor"]);
                dictionary["MutedTextBrush"] = new SolidColorBrush(colors["MutedTextColor"]);
                dictionary["DividerBrush"] = new SolidColorBrush(colors["DividerColor"]);
                dictionary["ButtonBorderBrush"] = new SolidColorBrush(colors["ButtonBorderColor"]);
                dictionary["OnLightsBrush"] = new SolidColorBrush(colors["OnLights"]);
                dictionary["AccentBrush"] = new SolidColorBrush(colors["AccentColor"]);
            }

            dictionary["AppFontFamily"] = spec.Font ?? string.Empty;

            // A preset's own panel style is kept as it is, unless the corners or depth were changed.
            if (spec.Corners != preset.Corners || spec.Depth != preset.Depth)
            {
                dictionary["BoardDetailBlockBaseStyle"] = BuildPanelStyle(spec, dictionary);
            }

            return dictionary;
        }

        static bool SameColour(string a, string b) => Color.FromArgb(a).ToRgbaHex() == Color.FromArgb(b).ToRgbaHex();

        /// <summary>True if the spec's mode or colours differ from the preset it started from.</summary>
        public static bool HasCustomColors(ThemeSpec spec, ThemeInfo preset = null)
        {
            preset ??= Find(spec.BaseId);
            return spec.Dark != preset.Dark
                || SameColour(spec.Primary, preset.Primary) == false
                || SameColour(spec.Secondary, preset.Secondary) == false
                || SameColour(spec.Accent, preset.Accent) == false;
        }

        public static double CornerRadiusFor(CornerStyle corners) => corners switch
        {
            CornerStyle.VeryRound => 28,
            CornerStyle.Round => 14,
            _ => 2,
        };

        static Style BuildPanelStyle(ThemeSpec spec, ResourceDictionary colors)
        {
            var accent = colors.TryGetValue("AccentColor", out var value) && value is Color color ? color : Colors.Gray;

            var style = new Style(typeof(Microsoft.Maui.Controls.Border));
            style.Setters.Add(new Setter() { Property = View.MarginProperty, Value = new Thickness(0) });
            style.Setters.Add(new Setter() { Property = Microsoft.Maui.Controls.Border.PaddingProperty, Value = new Thickness(0) });
            style.Setters.Add(new Setter()
            {
                Property = Microsoft.Maui.Controls.Border.StrokeShapeProperty,
                Value = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = new CornerRadius(CornerRadiusFor(spec.Corners)) },
            });

            switch (spec.Depth)
            {
                case DepthStyle.DropShadow:
                    style.Setters.Add(new Setter() { Property = Microsoft.Maui.Controls.Border.StrokeThicknessProperty, Value = 0.0 });
                    style.Setters.Add(new Setter()
                    {
                        Property = VisualElement.ShadowProperty,
                        Value = new Shadow() { Brush = new SolidColorBrush(Colors.Black), Opacity = 0.28f, Offset = new Point(0, 4), Radius = 10 },
                    });
                    break;

                case DepthStyle.EdgeLight:
                    style.Setters.Add(new Setter() { Property = Microsoft.Maui.Controls.Border.StrokeProperty, Value = new SolidColorBrush(accent) });
                    style.Setters.Add(new Setter() { Property = Microsoft.Maui.Controls.Border.StrokeThicknessProperty, Value = 2.0 });
                    style.Setters.Add(new Setter()
                    {
                        Property = VisualElement.ShadowProperty,
                        Value = new Shadow() { Brush = new SolidColorBrush(accent), Opacity = 0.55f, Offset = new Point(0, 0), Radius = 14 },
                    });
                    break;

                default:
                    style.Setters.Add(new Setter() { Property = Microsoft.Maui.Controls.Border.StrokeThicknessProperty, Value = 0.0 });
                    break;
            }

            return style;
        }

        /// <summary>Looks up a theme colour. (ResourceDictionary's indexer does not search merged dictionaries.)</summary>
        public static Color GetColor(string key, Color fallback)
        {
            if (Application.Current.Resources.TryGetValue(key, out var value) && value is Color color)
            {
                return color;
            }

            return fallback;
        }

        public static Style GetStyle(string key)
        {
            return Application.Current.Resources.TryGetValue(key, out var value) ? value as Style : null;
        }
    }
}
