namespace OWCE.Themes
{
    /// <summary>How round the panels' corners are.</summary>
    public enum CornerStyle
    {
        VeryRound,
        Round,
        Sharp,
    }

    /// <summary>The effect around the panels.</summary>
    public enum DepthStyle
    {
        None,
        /// <summary>A soft dark shadow underneath.</summary>
        DropShadow,
        /// <summary>A thin outline in the accent colour with a glow of the same colour.</summary>
        EdgeLight,
    }

    /// <summary>
    /// A theme described by a few simple settings: which colour palette (a preset, see <see cref="ThemeManager.Themes"/>) it
    /// starts from, the panel corners, the panel depth effect and the font. The presets are specs too.
    /// Styles the user saves are specs with their own Id and name.
    /// </summary>
    public sealed class ThemeSpec
    {
        /// <summary>A preset's id (eg "codec"), or a unique id for a style the user saved.</summary>
        public string Id { get; set; } = ThemeManager.DefaultThemeId;

        public string Name { get; set; } = "Default";

        /// <summary>The preset whose colours this uses.</summary>
        public string BaseId { get; set; } = ThemeManager.DefaultThemeId;

        public CornerStyle Corners { get; set; } = CornerStyle.VeryRound;

        public DepthStyle Depth { get; set; } = DepthStyle.DropShadow;

        /// <summary>The font family (see <see cref="FontCatalog"/>). Empty is the system font.</summary>
        public string Font { get; set; } = string.Empty;

        /// <summary>The colour Material builds its scheme from. Only used when BaseId is "material".</summary>
        public string Seed { get; set; } = MaterialPalette.DefaultSeed;

        /// <summary>Material's dark mode. Only used when BaseId is "material".</summary>
        public bool Dark { get; set; }

        public ThemeSpec Clone() => (ThemeSpec)MemberwiseClone();

        public Dictionary<string, string> ToMap() => new Dictionary<string, string>()
        {
            ["id"] = Id,
            ["name"] = Name,
            ["base"] = BaseId,
            ["corners"] = Corners.ToString(),
            ["depth"] = Depth.ToString(),
            ["font"] = Font,
            ["seed"] = Seed,
            ["dark"] = Dark ? "1" : "0",
        };

        public static ThemeSpec FromMap(IDictionary<string, string> map)
        {
            string Get(string key, string fallback) => map.TryGetValue(key, out var value) && value != null ? value : fallback;

            var spec = new ThemeSpec()
            {
                Id = Get("id", ThemeManager.DefaultThemeId),
                Name = Get("name", "Style"),
                BaseId = Get("base", ThemeManager.DefaultThemeId),
                Font = Get("font", string.Empty),
                Seed = Get("seed", MaterialPalette.DefaultSeed),
                Dark = Get("dark", "0") == "1",
            };

            spec.Corners = Enum.TryParse<CornerStyle>(Get("corners", ""), out var corners) ? corners : CornerStyle.VeryRound;
            spec.Depth = Enum.TryParse<DepthStyle>(Get("depth", ""), out var depth) ? depth : DepthStyle.DropShadow;
            return spec;
        }
    }

    /// <summary>The fonts the theme builder offers: free Google Fonts that are bundled with the app (Resources/Fonts).</summary>
    public static class FontCatalog
    {
        public sealed record FontChoice(string Name, string Family);

        public static IReadOnlyList<FontChoice> Fonts { get; } = new List<FontChoice>
        {
            new FontChoice("System", ""),
            new FontChoice("Poppins", "Poppins"),
            new FontChoice("Space Grotesk", "SpaceGrotesk"),
            new FontChoice("Oswald", "Oswald"),
            new FontChoice("Bebas Neue", "BebasNeue"),
            new FontChoice("Rajdhani", "Rajdhani"),
            new FontChoice("Exo 2", "Exo2"),
            new FontChoice("Chakra Petch", "ChakraPetch"),
            new FontChoice("Orbitron", "Orbitron"),
            new FontChoice("Michroma", "Michroma"),
            new FontChoice("Audiowide", "Audiowide"),
            new FontChoice("Russo One", "RussoOne"),
            new FontChoice("Righteous", "Righteous"),
            new FontChoice("Share Tech Mono", "ShareTechMono"),
            new FontChoice("Press Start 2P", "PressStart2P"),
        };
    }
}
