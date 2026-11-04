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

        /// <summary>Light or dark. (A preset has its own: most are dark.)</summary>
        public bool Dark { get; set; }

        /// <summary>The highlight colour (hex): selected items, sliders, headings, the speed and battery panels.</summary>
        public string Primary { get; set; } = MaterialPalette.DefaultSeed;

        /// <summary>The supporting colour (hex): cards, buttons, the lights panel.</summary>
        public string Secondary { get; set; } = "#625B71";

        /// <summary>The extra colour (hex): the power and temperature panels.</summary>
        public string Accent { get; set; } = "#7D5260";

        public ThemeSpec Clone() => (ThemeSpec)MemberwiseClone();

        public Dictionary<string, string> ToMap() => new Dictionary<string, string>()
        {
            ["id"] = Id,
            ["name"] = Name,
            ["base"] = BaseId,
            ["corners"] = Corners.ToString(),
            ["depth"] = Depth.ToString(),
            ["font"] = Font,
            ["primary"] = Primary,
            ["secondary"] = Secondary,
            ["accent"] = Accent,
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
                Primary = Get("primary", Get("seed", MaterialPalette.DefaultSeed)),
                Secondary = Get("secondary", "#625B71"),
                Accent = Get("accent", "#7D5260"),
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
        /// <param name="Scale">
        /// Fonts of the same point size are not the same visual size (a wide font like Michroma looks much bigger than the
        /// system font). Text in this font is drawn at its size times this number so it looks about the same size as the rest.
        /// </param>
        public sealed record FontChoice(string Name, string Family, double Scale = 1.0);

        public static IReadOnlyList<FontChoice> Fonts { get; } = new List<FontChoice>
        {
            new FontChoice("System", ""),
            new FontChoice("Poppins", "Poppins", 0.95),
            new FontChoice("Space Grotesk", "SpaceGrotesk", 0.97),
            new FontChoice("Oswald", "Oswald", 0.95),
            new FontChoice("Bebas Neue", "BebasNeue", 1.1),
            new FontChoice("Rajdhani", "Rajdhani", 1.08),
            new FontChoice("Exo 2", "Exo2", 0.97),
            new FontChoice("Chakra Petch", "ChakraPetch", 0.97),
            new FontChoice("Orbitron", "Orbitron", 0.86),
            new FontChoice("Michroma", "Michroma", 0.74),
            new FontChoice("Audiowide", "Audiowide", 0.88),
            new FontChoice("Russo One", "RussoOne", 0.92),
            new FontChoice("Righteous", "Righteous", 0.93),
            new FontChoice("Share Tech Mono", "ShareTechMono"),
            new FontChoice("Press Start 2P", "PressStart2P", 0.6),
        };

        /// <summary>How much to scale text in a font family (1 for the system font or an unknown one).</summary>
        public static double ScaleFor(string family)
        {
            if (string.IsNullOrEmpty(family))
            {
                return 1.0;
            }

            foreach (var font in Fonts)
            {
                if (font.Family == family)
                {
                    return font.Scale;
                }
            }

            return 1.0;
        }
    }
}
