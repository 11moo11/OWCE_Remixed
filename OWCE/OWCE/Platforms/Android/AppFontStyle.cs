using Android.Util;
using Android.Widget;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Handlers;
using OWCE.Themes;

namespace OWCE.Droid
{
    /// <summary>
    /// Puts the theme's font on text directly, whatever the control's own FontFamily says, and scales the size to suit the font
    /// (see FontCatalog.FontChoice.Scale). A control with its own font (eg the font samples in the settings) keeps it.
    /// Going through the controls' FontFamily property alone missed some panels (Edge light ones), so this is done on the native
    /// text view when the control is created, whenever its font changes, and for every live text view when the theme changes.
    /// </summary>
    public static class AppFontStyle
    {
        sealed class Tracked
        {
            public WeakReference<TextView> TextView;
            public WeakReference<ITextStyle> Style;
            public WeakReference<IFontManager> Fonts;
        }

        static readonly List<Tracked> _live = new List<Tracked>();
        static readonly object _recipient = new object();

        static AppFontStyle()
        {
            // When the theme (and so possibly the font) changes, every text view is updated.
            WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(_recipient, (r, m) => MainThread.BeginInvokeOnMainThread(Refresh));
        }

        public static void Apply(TextView textView, ITextStyle style, IFontManager fonts, bool isSample = false)
        {
            if (textView == null || style == null)
            {
                return;
            }

            if (isSample == false)
            {
                lock (_live)
                {
                    if (_live.Any(x => x.TextView.TryGetTarget(out var existing) && ReferenceEquals(existing, textView)) == false)
                    {
                        _live.Add(new Tracked()
                        {
                            TextView = new WeakReference<TextView>(textView),
                            Style = new WeakReference<ITextStyle>(style),
                            Fonts = new WeakReference<IFontManager>(fonts),
                        });
                    }
                }
            }

            Set(textView, style, fonts, isSample);
        }

        static void Set(TextView textView, ITextStyle style, IFontManager fonts, bool isSample)
        {
            var font = style.Font;
            var own = font.Family;

            // The control's own font wins (the font samples); everything else gets the theme's.
            var family = isSample ? own : ThemeManager.Current.Font;
            if (string.IsNullOrEmpty(family) && isSample == false)
            {
                family = own;
            }

            if (fonts != null && (string.IsNullOrEmpty(family) == false || isSample || ThemeManager.Current.Font != string.Empty))
            {
                // (An empty family gives the system font, in the right weight.)
                var typeface = fonts.GetTypeface(Microsoft.Maui.Font.OfSize(family, font.Size > 0 ? font.Size : 14, font.Weight, font.Slant, font.AutoScalingEnabled));
                if (typeface != null)
                {
                    textView.Typeface = typeface;
                }
            }

            var scale = FontCatalog.ScaleFor(family);
            if (font.Size > 0 && (scale != 1.0 || string.IsNullOrEmpty(family)))
            {
                textView.SetTextSize(font.AutoScalingEnabled ? ComplexUnitType.Sp : ComplexUnitType.Dip, (float)(font.Size * scale));
            }
        }

        static void Refresh()
        {
            lock (_live)
            {
                _live.RemoveAll(x => x.TextView.TryGetTarget(out _) == false);

                foreach (var item in _live)
                {
                    if (item.TextView.TryGetTarget(out var textView) && item.Style.TryGetTarget(out var style))
                    {
                        item.Fonts.TryGetTarget(out var fonts);
                        try
                        {
                            Set(textView, style, fonts, false);
                        }
                        catch (Exception)
                        {
                            // A text view that is being torn down is skipped.
                        }
                    }
                }
            }
        }
    }
}
