namespace OWCE.Views
{
    /// <summary>
    /// A Slider with a thicker track. MAUI has no track thickness setting, so the platform tweaks live in
    /// Platforms/Android/ThickSliderStyle.cs and Platforms/iOS/ThickSliderStyle.cs and are hooked up in MauiProgram.
    /// </summary>
    public class ThickSlider : Slider
    {
        public const double DefaultTrackThickness = 8;

        public static readonly BindableProperty TrackThicknessProperty = BindableProperty.Create(
            nameof(TrackThickness),
            typeof(double),
            typeof(ThickSlider),
            DefaultTrackThickness);

        /// <summary>Track thickness in device independent units. Applied when the control is created.</summary>
        public double TrackThickness
        {
            get { return (double)GetValue(TrackThicknessProperty); }
            set { SetValue(TrackThicknessProperty, value); }
        }
    }
}
