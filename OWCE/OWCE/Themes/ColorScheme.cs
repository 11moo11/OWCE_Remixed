using Microsoft.Maui.Graphics;

namespace OWCE.Themes
{
    /// <summary>
    /// Builds every colour the app uses from five colours, in the way Realtime Colors (realtimecolors.com) describes a palette:
    /// neutral <b>text</b> and <b>background</b> colours, a <b>primary</b> colour, a less highlighted <b>secondary</b> colour,
    /// and an <b>accent</b> colour "used for highlights, checkmarks, boxes, links, etc.".
    /// <list type="bullet">
    /// <item>Background: the page, and (a touch lighter or darker) the cards and bars on it.</item>
    /// <item>Text: text and icons on the page. Greys for dividers and quiet text are mixes of text and background.</item>
    /// <item>Primary: the main colour: the speed and battery panels, the board cards, selected buttons.</item>
    /// <item>Secondary: supporting colour: the lights and ride mode panels and soft buttons.</item>
    /// <item>Accent: highlights: selected tab, sliders, switches, headings, links, the edge light, and the power and temperature panels.</item>
    /// </list>
    /// The colours are used as they are picked (panels are exactly the picked colours, with darker and lighter shades of them for bars
    /// and tracks). Only text is adjusted: the text on a panel is black or white, whichever reads best, and the page text, quiet text and
    /// accent are nudged lighter or darker where needed so they stay readable (4.5 : 1) on the page.
    /// </summary>
    public static class ColorScheme
    {
        public const string DefaultSeed = "#6750A4";

        /// <summary>Ready made colours offered in the settings.</summary>
        public static readonly string[] SeedChoices =
        {
            "#6750A4", // purple (the Material baseline)
            "#3F5AA6", // indigo
            "#1E88E5", // blue
            "#00897B", // teal
            "#2E7D32", // green
            "#9E9D24", // lime
            "#F9A825", // amber
            "#EF6C00", // orange
            "#D32F2F", // red
            "#C2185B", // pink
            "#795548", // brown
            "#546E7A", // slate
            "#000000", // black
            "#808080", // grey
            "#FFFFFF", // white
        };

        /// <summary>
        /// The usual page colours for light or dark: a very dark or very light background and the opposite for text, both lightly
        /// tinted with the primary colour (so a dark theme from a green is dark green, not grey).
        /// </summary>
        public static (Color Background, Color Text) DefaultNeutrals(Color primary, bool dark)
        {
            var hue = primary.GetHue();
            var sat = Math.Clamp(primary.GetSaturation(), 0.1f, 1f);
            var tint = dark ? Math.Clamp(sat * 0.5f, 0.08f, 0.5f) : Math.Clamp(sat * 0.35f, 0.05f, 0.4f);

            return dark
                ? (Color.FromHsla(hue, tint, 0.07f), Color.FromHsla(hue, Math.Min(tint, 0.3f), 0.93f))
                : (Color.FromHsla(hue, tint, 0.97f), Color.FromHsla(hue, Math.Min(tint, 0.4f), 0.08f));
        }

        public static bool IsDark(Color background, Color text) => Luminance(background) < Luminance(text);

        /// <summary>The theme's colour keys with their values.</summary>
        public static Dictionary<string, Color> Generate(Color background, Color text, Color primary, Color secondary, Color accent)
        {
            var dark = IsDark(background, text);
            var black = Colors.Black;
            var white = Colors.White;

            var c = new Dictionary<string, Color>();

            // The page. Each surface is the background nudged towards the text colour.
            c["BackgroundGradientStart"] = background;
            c["BackgroundGradientEnd"] = Mix(background, primary, 0.07);
            c["SurfaceColor"] = Mix(background, text, 0.04);
            c["CardSurfaceColor"] = Mix(background, text, 0.09);
            c["DividerColor"] = Mix(background, text, 0.2);
            c["ButtonBorderColor"] = Mix(background, text, 0.4);
            c["SliderInactiveColor"] = Mix(background, text, 0.22);
            var page = new[] { c["BackgroundGradientStart"], c["BackgroundGradientEnd"], c["SurfaceColor"], c["CardSurfaceColor"] };

            c["OnBackgroundColor"] = Ensure(text, dark, page);
            c["OnSurfaceColor"] = c["OnBackgroundColor"];
            c["MutedTextColor"] = Ensure(Mix(text, background, 0.35), dark, page);

            // The accent is used as text on the page (headings, links), so it is kept readable there.
            c["AccentColor"] = Ensure(accent, dark, page);
            c["SliderActiveColor"] = c["AccentColor"];
            c["OnAccentColor"] = BestText(black, white, c["AccentColor"]);

            // Primary: the main panels.
            c["SpeedPanel"] = primary;
            c["BatteryHeader"] = Shade(primary, -0.10);
            c["BatteryBar"] = Shade(primary, 0.06);
            c["CardColor"] = primary;
            c["SelectedButtonColor"] = primary;
            c["MenuButtonColor"] = primary;

            // Secondary: supporting panels.
            c["BatteryPanel"] = secondary;
            c["LightsPanel"] = secondary;
            c["ButtonColor"] = Mix(secondary, background, 0.45);
            c["RideModePanel"] = Mix(secondary, Colors.Black, 0.6);

            // Accent: highlight panels.
            c["PowerPanel"] = accent;
            c["PowerBar"] = Shade(accent, -0.12);
            c["TempPanel"] = Shade(accent, 0.10);
            c["TempTrack"] = Shade(accent, 0.04);
            c["TempBar"] = Shade(accent, -0.04);

            // Ride mode buttons: quiet ones on the (dark) ride mode panel, and the chosen one in the accent colour.
            c["RideModeButtonColor"] = Mix(c["RideModePanel"], Colors.White, 0.12);
            c["RideModeButtonSelectedColor"] = accent;

            LimitBrightness(c);

            // Text on the panels: black or white, whichever reads best on all the colours it sits on.
            c["OnSpeedPanel"] = BestText(black, white, c["SpeedPanel"]);
            c["OnBattery"] = BestText(black, white, c["BatteryHeader"], c["BatteryBar"]);
            c["OnCardColor"] = BestText(black, white, c["CardColor"]);
            c["OnSelectedButtonColor"] = BestText(black, white, c["SelectedButtonColor"]);
            c["OnMenuButtonColor"] = BestText(black, white, c["MenuButtonColor"]);
            c["OnLights"] = BestText(black, white, c["LightsPanel"]);
            c["OnButtonColor"] = BestText(black, white, c["ButtonColor"]);
            c["OnRideModePanel"] = BestText(black, white, c["RideModePanel"]);
            c["OnPower"] = BestText(black, white, c["PowerPanel"], c["PowerBar"]);
            c["OnTemp"] = BestText(black, white, c["TempPanel"], c["TempTrack"], c["TempBar"]);

            // Very light and very dark neighbours can leave no text colour that suits a panel's shades, so nudge them.
            Fit(c, "OnBattery", "BatteryHeader", "BatteryBar");
            Fit(c, "OnPower", "PowerPanel", "PowerBar");
            Fit(c, "OnTemp", "TempPanel", "TempTrack", "TempBar");

            // Fitting the text can lighten a panel, so the brightness order is enforced once more, and the text checked again.
            LimitBrightness(c);
            c["OnBattery"] = BestText(black, white, c["BatteryHeader"], c["BatteryBar"]);
            c["OnPower"] = BestText(black, white, c["PowerPanel"], c["PowerBar"]);
            c["OnTemp"] = BestText(black, white, c["TempPanel"], c["TempTrack"], c["TempBar"]);
            c["OnLights"] = BestText(black, white, c["LightsPanel"]);
            c["OnRideModeButtonSelectedColor"] = BestText(black, white, c["RideModeButtonSelectedColor"]);
            c["OnRideModeButtonColor"] = BestText(black, white, c["RideModeButtonColor"]);

            return c;
        }

        // The dashboard has an order of brightness: the speed panel is the brightest, the battery panel is below it, and the current
        // (power) panel is never brighter than the battery panel. Everything else stays below the speed panel. Panels that are too
        // bright are made darker. (If the speed panel is nearly black there is nothing darker to go to, so nothing is changed.)
        static void LimitBrightness(Dictionary<string, Color> c)
        {
            var speed = Luminance(c["SpeedPanel"]);
            if (speed < 0.02)
            {
                return;
            }

            void Cap(string key, double maxLuminance) => c[key] = Dim(c[key], maxLuminance);

            Cap("BatteryHeader", speed * 0.8);
            Cap("BatteryBar", speed * 0.9);
            Cap("BatteryPanel", speed * 0.8);
            Cap("LightsPanel", speed * 0.8);
            Cap("TempPanel", speed * 0.85);
            Cap("TempTrack", speed * 0.85);
            Cap("TempBar", speed * 0.85);
            Cap("RideModeButtonSelectedColor", speed * 0.75);

            // Current is never brighter than battery.
            var battery = Math.Min(Luminance(c["BatteryHeader"]), Luminance(c["BatteryBar"]));
            Cap("PowerPanel", Math.Min(battery, speed * 0.8));
            Cap("PowerBar", Luminance(c["PowerPanel"]));
        }

        // Makes a colour darker (same hue and saturation) until its luminance is at most the given amount.
        static Color Dim(Color color, double maxLuminance)
        {
            var hue = color.GetHue();
            var saturation = color.GetSaturation();
            var lightness = color.GetLuminosity();

            for (var i = 0; i < 100 && Luminance(color) > maxLuminance; ++i)
            {
                lightness = Math.Max(0f, lightness - 0.01f);
                color = Color.FromHsla(hue, saturation, lightness);
            }

            return color;
        }

        static void Fit(Dictionary<string, Color> c, string textKey, params string[] backgroundKeys)
        {
            foreach (var key in backgroundKeys)
            {
                c[key] = FitBackground(c[key], c[textKey]);
            }
        }

        /// <summary>A colour made lighter (positive) or darker (negative) by a change in HSL lightness.</summary>
        public static Color Shade(Color color, double change)
        {
            return Color.FromHsla(color.GetHue(), color.GetSaturation(), (float)Math.Clamp(color.GetLuminosity() + change, 0, 1));
        }

        public static Color Mix(Color a, Color b, double amountOfB)
        {
            var t = (float)Math.Clamp(amountOfB, 0, 1);
            return new Color(a.Red + (b.Red - a.Red) * t, a.Green + (b.Green - a.Green) * t, a.Blue + (b.Blue - a.Blue) * t);
        }

        public static double Luminance(Color color)
        {
            static double Channel(float v) => v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
            return 0.2126 * Channel(color.Red) + 0.7152 * Channel(color.Green) + 0.0722 * Channel(color.Blue);
        }

        public static double Contrast(Color a, Color b)
        {
            var la = Luminance(a);
            var lb = Luminance(b);
            return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
        }

        static double WorstContrast(Color foreground, Color[] backgrounds) => backgrounds.Min(x => Contrast(foreground, x));

        // Black or white, whichever is readable on every background (or reads best).
        static Color BestText(Color dark, Color light, params Color[] backgrounds)
        {
            return WorstContrast(light, backgrounds) >= WorstContrast(dark, backgrounds) ? light : dark;
        }

        // Moves a background away from its text colour (darker for light text, lighter for dark text) until the text is readable.
        static Color FitBackground(Color background, Color text)
        {
            var step = Luminance(text) > Luminance(background) ? -0.015f : 0.015f;
            var hue = background.GetHue();
            var saturation = background.GetSaturation();
            var lightness = background.GetLuminosity();

            for (var i = 0; i < 70 && Contrast(text, background) < 4.5; ++i)
            {
                lightness = Math.Clamp(lightness + step, 0f, 1f);
                background = Color.FromHsla(hue, saturation, lightness);
            }

            return background;
        }

        // Moves a colour towards lighter (on a dark page) or darker (on a light page) until it is readable on the backgrounds.
        static Color Ensure(Color color, bool dark, params Color[] backgrounds)
        {
            var hue = color.GetHue();
            var saturation = color.GetSaturation();
            var lightness = color.GetLuminosity();

            for (var i = 0; i < 80 && WorstContrast(color, backgrounds) < 4.5; ++i)
            {
                lightness = Math.Clamp(lightness + (dark ? 0.015f : -0.015f), 0f, 1f);
                color = Color.FromHsla(hue, saturation, lightness);
            }

            // Still not readable (a background in the middle of the range): fall back to plain black or white.
            if (WorstContrast(color, backgrounds) < 4.5)
            {
                var black = Colors.Black;
                var white = Colors.White;
                return WorstContrast(white, backgrounds) >= WorstContrast(black, backgrounds) ? white : black;
            }

            return color;
        }
    }
}
