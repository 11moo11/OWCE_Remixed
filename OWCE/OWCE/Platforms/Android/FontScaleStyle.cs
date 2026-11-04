using Android.Util;
using Android.Widget;
using Microsoft.Maui.Handlers;
using OWCE.Themes;

namespace OWCE.Droid
{
    /// <summary>
    /// Draws text in the theme builder's fonts at a size that suits them (see FontCatalog.FontChoice.Scale), because
    /// fonts of the same size are not the same visual size.
    /// </summary>
    public static class FontScaleStyle
    {
        // Runs after MAUI has set the font and size, so it only has to change the size.
        public static void Apply(TextView textView, ITextStyle view)
        {
            var font = view.Font;
            var scale = FontCatalog.ScaleFor(font.Family);
            if (textView == null || scale == 1.0 || font.Size <= 0)
            {
                return;
            }

            textView.SetTextSize(font.AutoScalingEnabled ? ComplexUnitType.Sp : ComplexUnitType.Dip, (float)(font.Size * scale));
        }
    }
}
