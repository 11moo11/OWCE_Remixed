using CommunityToolkit.Mvvm.Messaging;

namespace OWCE.Themes
{
    public sealed record ThemeInfo(string Id, string Name, string Description, Func<ResourceDictionary> Create);

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
        const string PreferenceKey = "theme";

        public static IReadOnlyList<ThemeInfo> Themes { get; } = new List<ThemeInfo>
        {
            new ThemeInfo(DefaultThemeId, "Default", "The original OWCE look.", () => new DefaultTheme()),
            new ThemeInfo("material", "Material", "Material Design 3: tonal purple surfaces and large rounded corners.", () => new MaterialTheme()),
            new ThemeInfo("devil-trigger", "Devil Trigger", "Inspired by Devil May Cry: black, crimson and bone with hard edges.", () => new DevilTriggerTheme()),
            new ThemeInfo("codec", "Codec", "Inspired by Metal Gear Solid: green phosphor on black.", () => new CodecTheme()),
            new ThemeInfo("underground", "Underground", "Inspired by Need for Speed Underground: night city neon.", () => new UndergroundTheme()),
            new ThemeInfo("nanotech", "Nanotech", "Inspired by Ratchet & Clank: deep blue with bright orange.", () => new NanotechTheme()),
            new ThemeInfo("master-thief", "Master Thief", "Inspired by Sly Cooper: rooftop-at-night indigo with gold.", () => new MasterThiefTheme()),
            new ThemeInfo("keyblade", "Keyblade", "Inspired by Kingdom Hearts II: command-menu navy and sky blue with gold.", () => new KeybladeTheme()),
            new ThemeInfo("city-escape", "City Escape", "Inspired by Sonic Adventure 2: black and yellow with hero blue and rival red.", () => new CityEscapeTheme()),
        };

        static ResourceDictionary _current;

        public static string CurrentId { get; private set; } = DefaultThemeId;

        /// <summary>The theme saved in preferences.</summary>
        public static string SavedId => Preferences.Get(PreferenceKey, DefaultThemeId);

        public static void Save(string id) => Preferences.Set(PreferenceKey, Find(id).Id);

        public static ThemeInfo Find(string id) => Themes.FirstOrDefault(x => x.Id == id) ?? Themes[0];

        /// <summary>Applies a theme to the running app. Must be called on the UI thread.</summary>
        public static void Apply(string id)
        {
            var theme = Find(id);
            var merged = Application.Current.Resources.MergedDictionaries;

            // Add the new theme before removing the old one: the last dictionary added wins, so everything switches in one go
            // without a moment where the theme's keys are missing.
            var next = theme.Create();
            merged.Add(next);

            if (_current != null)
            {
                merged.Remove(_current);
            }

            _current = next;
            CurrentId = theme.Id;

            WeakReferenceMessenger.Default.Send(new ThemeChangedMessage());
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
