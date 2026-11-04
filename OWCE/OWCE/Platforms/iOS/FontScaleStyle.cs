using OWCE.Themes;
using ObjCRuntime;
using UIKit;

namespace OWCE.iOS
{
    /// <summary>
    /// Draws text in the theme builder's fonts at a size that suits them (see FontCatalog.FontChoice.Scale), because
    /// fonts of the same size are not the same visual size.
    /// </summary>
    public static class FontScaleStyle
    {
        // Runs after MAUI has set the font and size, so it only has to change the size.
        public static void Apply(UIFont current, ITextStyle view, Action<UIFont> set)
        {
            var font = view.Font;
            var scale = FontCatalog.ScaleFor(font.Family);
            if (current == null || scale == 1.0 || font.Size <= 0)
            {
                return;
            }

            set(current.WithSize((nfloat)(font.Size * scale)));
        }
    }
}
