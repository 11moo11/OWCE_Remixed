using Microsoft.Maui.Graphics;

namespace OWCE.Themes
{
    /// <summary>
    /// Builds a Material Design 3 style colour scheme from one seed colour, in light or dark. Like Material's own tonal palettes
    /// it makes a ladder of tones for a few related hues (the seed's hue, a muted version, a shifted one, and greys tinted
    /// with the seed) and picks the tones the theme needs. Text colours are checked against what they sit on and
    /// adjusted until they are readable (4.5 : 1).
    /// This is an approximation using HSL, not the exact Material colour science.
    /// </summary>
    public static class MaterialPalette
    {
        public const string DefaultSeed = "#6750A4";

        /// <summary>Ready made seed colours offered in the settings.</summary>
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
        };

        /// <summary>
        /// The theme's colour keys with their new values, built from three seed colours. As in Material 3, each seed becomes a
        /// ladder of tones and the scheme picks tones from them, so the result is balanced whatever colours are given:
        /// <list type="bullet">
        /// <item>primary: the highlight colour (selected items, sliders, headings) and the speed and battery panels, and it
        /// tints the greys of the page;</item>
        /// <item>secondary: supporting surfaces: cards, buttons, the lights panel;</item>
        /// <item>accent: the power and temperature panels.</item>
        /// </list>
        /// </summary>
        public static Dictionary<string, Color> Generate(Color primary, Color secondary, Color accent, bool dark)
        {
            var hue = primary.GetHue();
            var chroma = Math.Clamp(primary.GetSaturation(), 0.3f, 0.9f);
            var primarySat = Math.Clamp(primary.GetSaturation(), 0.35f, 1f);
            var secondaryHue = secondary.GetHue();
            var secondaryChroma = Math.Clamp(secondary.GetSaturation(), 0.08f, 0.8f);
            var secondarySat = Math.Clamp(secondary.GetSaturation(), 0.08f, 1f);
            var accentHue = accent.GetHue();
            var accentChroma = Math.Clamp(accent.GetSaturation() * 0.8f, 0.2f, 0.85f);
            var accentSat = Math.Clamp(accent.GetSaturation(), 0.35f, 1f);

            Color P(double tone) => Tone(hue, chroma, tone);
            Color S(double tone) => Tone(secondaryHue, secondaryChroma, tone);
            Color T(double tone) => Tone(accentHue, accentChroma, tone);
            // Panels are vivid: the colour at its full strength and a chosen lightness. (Material's own tones are pale or dark and
            // washed out, which turns a strong colour into grey; panels should look like the colour that was picked.)
            Color Vp(double l) => Color.FromHsla(hue, primarySat, (float)l);
            Color Vs(double l) => Color.FromHsla(secondaryHue, secondarySat, (float)l);
            Color Vt(double l) => Color.FromHsla(accentHue, accentSat, (float)l);
            Color N(double tone) => Tone(hue, 0.07f, tone);
            Color V(double tone) => Tone(hue, 0.12f, tone);

            var c = new Dictionary<string, Color>();

            if (dark == false)
            {
                c["BackgroundGradientStart"] = N(98);
                c["BackgroundGradientEnd"] = N(94);
                c["OnBackgroundColor"] = N(10);
                c["SurfaceColor"] = N(99);
                c["OnSurfaceColor"] = N(10);
                c["MutedTextColor"] = V(30);
                c["DividerColor"] = V(80);
                c["CardColor"] = Vs(.80);
                c["ButtonColor"] = Vs(.88);
                c["ButtonBorderColor"] = V(50);
                c["SelectedButtonColor"] = Vp(.42);
                c["SliderActiveColor"] = Vp(.42);
                c["SliderInactiveColor"] = V(90);
                c["SpeedPanel"] = Vp(.62);
                c["BatteryPanel"] = Vs(.72);
                c["BatteryHeader"] = Vp(.38);
                c["BatteryBar"] = Vp(.50);
                c["PowerPanel"] = Vt(.45);
                c["PowerBar"] = Vt(.35);
                c["TempPanel"] = Vt(.70);
                c["TempTrack"] = Vt(.64);
                c["TempBar"] = Vt(.58);
                c["RideModePanel"] = N(20);
                c["LightsPanel"] = Vs(.50);
                c["MenuButtonColor"] = Vp(.42);
                c["CardSurfaceColor"] = N(95);
                c["AccentColor"] = Vp(.42);
            }
            else
            {
                c["BackgroundGradientStart"] = N(6);
                c["BackgroundGradientEnd"] = N(10);
                c["OnBackgroundColor"] = N(90);
                c["SurfaceColor"] = N(10);
                c["OnSurfaceColor"] = N(90);
                c["MutedTextColor"] = V(80);
                c["DividerColor"] = V(30);
                c["CardColor"] = Vs(.28);
                c["ButtonColor"] = Vs(.30);
                c["ButtonBorderColor"] = V(60);
                c["SelectedButtonColor"] = Vp(.62);
                c["SliderActiveColor"] = Vp(.62);
                c["SliderInactiveColor"] = V(30);
                c["SpeedPanel"] = Vp(.42);
                c["BatteryPanel"] = Vs(.34);
                c["BatteryHeader"] = Vp(.30);
                c["BatteryBar"] = Vp(.42);
                c["PowerPanel"] = Vt(.40);
                c["PowerBar"] = Vt(.30);
                c["TempPanel"] = Vt(.30);
                c["TempTrack"] = Vt(.38);
                c["TempBar"] = Vt(.48);
                c["RideModePanel"] = N(20);
                c["LightsPanel"] = Vs(.38);
                c["MenuButtonColor"] = Vp(.62);
                c["CardSurfaceColor"] = N(17);
                c["AccentColor"] = Vp(.62);
            }

            // Text on top of the colours above: the tone Material would use if it is readable, otherwise black or white.
            var light = N(98);
            var darkText = N(10);
            Color On(Color preferred, params Color[] backgrounds) => Readable(preferred, light, darkText, backgrounds);

            c["OnCardColor"] = On(dark ? S(90) : S(10), c["CardColor"]);
            c["OnButtonColor"] = On(dark ? S(90) : S(10), c["ButtonColor"]);
            c["OnSelectedButtonColor"] = On(dark ? P(20) : N(100), c["SelectedButtonColor"]);
            c["OnSpeedPanel"] = On(dark ? P(90) : P(10), c["SpeedPanel"]);
            c["OnBattery"] = On(dark ? P(95) : N(100), c["BatteryHeader"], c["BatteryBar"]);
            c["OnPower"] = On(dark ? T(90) : N(100), c["PowerPanel"], c["PowerBar"]);
            c["OnTemp"] = On(dark ? T(90) : T(10), c["TempPanel"], c["TempTrack"], c["TempBar"]);
            c["OnRideModePanel"] = On(N(95), c["RideModePanel"]);
            c["OnLights"] = On(dark ? S(90) : N(100), c["LightsPanel"]);
            c["OnMenuButtonColor"] = On(dark ? P(20) : N(100), c["MenuButtonColor"]);
            c["OnAccentColor"] = On(dark ? P(20) : N(100), c["AccentColor"]);

            // A colour that is very light or dark next to its panel-mates can leave no text colour that suits them all,
            // so panels are nudged until their text is readable.
            void Fit(string textKey, params string[] backgroundKeys)
            {
                foreach (var key in backgroundKeys)
                {
                    c[key] = FitBackground(c[key], c[textKey]);
                }
            }

            Fit("OnCardColor", "CardColor");
            Fit("OnButtonColor", "ButtonColor");
            Fit("OnSpeedPanel", "SpeedPanel");
            Fit("OnBattery", "BatteryHeader", "BatteryBar");
            Fit("OnPower", "PowerPanel", "PowerBar");
            Fit("OnTemp", "TempPanel", "TempTrack", "TempBar");
            Fit("OnLights", "LightsPanel");
            Fit("OnRideModePanel", "RideModePanel");

            // Muted text and the accent are used on the page, so they must be readable on every background they meet.
            c["MutedTextColor"] = Ensure(c["MutedTextColor"], dark, c["SurfaceColor"], c["CardSurfaceColor"]);
            c["AccentColor"] = Ensure(c["AccentColor"], dark, c["BackgroundGradientStart"], c["BackgroundGradientEnd"], c["CardSurfaceColor"], c["SurfaceColor"]);
            c["OnAccentColor"] = On(c["OnAccentColor"], c["AccentColor"]);
            c["SelectedButtonColor"] = c["AccentColor"];
            c["SliderActiveColor"] = c["AccentColor"];
            c["MenuButtonColor"] = c["AccentColor"];
            c["OnSelectedButtonColor"] = c["OnAccentColor"];
            c["OnMenuButtonColor"] = c["OnAccentColor"];

            return c;
        }

        static Color Tone(float hue, float saturation, double tone)
        {
            var lightness = (float)(Math.Clamp(tone, 0, 100) / 100);

            // Very light and very dark tones wash out, as in real Material palettes.
            var edge = Math.Abs(2 * lightness - 1);
            var s = saturation * (1 - 0.55f * edge * edge);
            return Color.FromHsla(hue, s, lightness);
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

        // The preferred colour if it is readable on every background, otherwise whichever of light or dark text reads best.
        static Color Readable(Color preferred, Color light, Color dark, Color[] backgrounds)
        {
            if (WorstContrast(preferred, backgrounds) >= 4.5)
            {
                return preferred;
            }

            return WorstContrast(light, backgrounds) >= WorstContrast(dark, backgrounds) ? light : dark;
        }

        // Moves a background away from its text colour (darker for light text, lighter for dark text) until the text is readable.
        static Color FitBackground(Color background, Color text)
        {
            var step = Luminance(text) > Luminance(background) ? -0.015f : 0.015f;
            var hue = background.GetHue();
            var saturation = background.GetSaturation();
            var lightness = background.GetLuminosity();

            for (var i = 0; i < 60 && Contrast(text, background) < 4.5; ++i)
            {
                lightness = Math.Clamp(lightness + step, 0f, 1f);
                background = Color.FromHsla(hue, saturation, lightness);
            }

            return background;
        }

        // Moves a colour towards lighter (on a dark theme) or darker (on a light theme) until it is readable on the backgrounds.
        static Color Ensure(Color color, bool dark, params Color[] backgrounds)
        {
            var hue = color.GetHue();
            var saturation = color.GetSaturation();
            var lightness = color.GetLuminosity();

            for (var i = 0; i < 60 && WorstContrast(color, backgrounds) < 4.5; ++i)
            {
                lightness = Math.Clamp(lightness + (dark ? 0.015f : -0.015f), 0f, 1f);
                color = Color.FromHsla(hue, saturation, lightness);
            }

            return color;
        }
    }
}
