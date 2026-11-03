using CoreGraphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using OWCE.Views;
using UIKit;

namespace OWCE.iOS
{
    public static class ThickSliderStyle
    {
        public static void Apply(ISliderHandler handler, ThickSlider slider)
        {
            var uiSlider = handler.PlatformView;
            if (uiSlider == null)
            {
                return;
            }

            var thickness = (float)slider.TrackThickness;

            // UISlider has no thickness setting, the height of the track images decides it.
            // Because custom images are used, the track colours are drawn into them.
            uiSlider.SetMinTrackImage(CreateTrackImage((slider.MinimumTrackColor ?? Colors.Black).ToPlatform(), thickness), UIControlState.Normal);
            uiSlider.SetMaxTrackImage(CreateTrackImage((slider.MaximumTrackColor ?? Colors.LightGray).ToPlatform(), thickness), UIControlState.Normal);
        }

        static UIImage CreateTrackImage(UIColor color, float thickness)
        {
            var size = new CGSize(thickness, thickness);
            using var renderer = new UIGraphicsImageRenderer(size);

            var image = renderer.CreateImage(context =>
            {
                color.SetFill();
                UIBezierPath.FromRoundedRect(new CGRect(CGPoint.Empty, size), thickness / 2).Fill();
            });

            // Stretch the middle so only the rounded ends keep their shape.
            return image.CreateResizableImage(new UIEdgeInsets(0, thickness / 2, 0, thickness / 2));
        }
    }
}
