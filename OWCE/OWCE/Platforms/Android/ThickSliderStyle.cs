using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Microsoft.Maui.Handlers;
using OWCE.Views;

namespace OWCE.Droid
{
    public static class ThickSliderStyle
    {
        public static void Apply(ISliderHandler handler, ThickSlider slider)
        {
            var seekBar = handler.PlatformView;
            if (seekBar?.ProgressDrawable == null)
            {
                return;
            }

            var thickness = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)slider.TrackThickness, seekBar.Resources.DisplayMetrics);

            // The track is a LayerDrawable (background and progress layers). Make each layer the same height and centred.
            if (seekBar.ProgressDrawable.Mutate() is LayerDrawable layers)
            {
                for (var i = 0; i < layers.NumberOfLayers; ++i)
                {
                    layers.SetLayerHeight(i, thickness);
                    layers.SetLayerGravity(i, GravityFlags.CenterVertical | GravityFlags.FillHorizontal);
                }

                seekBar.Invalidate();
            }
        }
    }
}
