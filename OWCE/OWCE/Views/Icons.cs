using Microsoft.Maui.Controls.Shapes;

namespace OWCE.Views
{
    /// <summary>
    /// A few vector icons (Material Design icons, 24 x 24), drawn as shapes so they stay sharp at any size and take their
    /// colour from the theme.
    /// </summary>
    public static class Icons
    {
        public const string ArrowBack = "M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z";
        public const string ChevronRight = "M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z";

        /// <param name="brushKey">The name of a brush in the theme, eg OnBackgroundBrush.</param>
        public static Microsoft.Maui.Controls.Shapes.Path Create(string pathData, string brushKey, double size = 24)
        {
            var path = new Microsoft.Maui.Controls.Shapes.Path()
            {
                Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(pathData),
                Aspect = Stretch.Uniform,
                WidthRequest = size,
                HeightRequest = size,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
            };
            path.SetDynamicResource(Shape.FillProperty, brushKey);
            return path;
        }
    }
}
