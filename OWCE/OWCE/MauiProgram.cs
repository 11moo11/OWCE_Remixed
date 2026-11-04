using CommunityToolkit.Maui;
using Mopups.Hosting;
using OWCE.DependencyInterfaces;
using Microsoft.Maui.Handlers;
using OWCE.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace OWCE
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()
                .ConfigureMopups();

            // Thicker track for ThickSlider (MAUI has no setting for this).
            // Also re-applied when the track colours change (a theme change), because on iOS the colours are drawn into the track.
            Action<ISliderHandler, ISlider> applyThickTrack = (handler, view) =>
            {
                if (view is ThickSlider thickSlider)
                {
#if ANDROID
                    OWCE.Droid.ThickSliderStyle.Apply(handler, thickSlider);
#elif IOS
                    OWCE.iOS.ThickSliderStyle.Apply(handler, thickSlider);
#endif
                }
            };
            SliderHandler.Mapper.AppendToMapping("OWCEThickTrack", applyThickTrack);
            SliderHandler.Mapper.AppendToMapping(nameof(ISlider.MinimumTrackColor), applyThickTrack);
            SliderHandler.Mapper.AppendToMapping(nameof(ISlider.MaximumTrackColor), applyThickTrack);

            // The theme's font (the AppFontFamily resource) is attached to every label and button when its handler is created, so it
            // follows theme changes wherever the control lives (setting it only through a style missed the dashboard panels).
            LabelHandler.Mapper.AppendToMapping("OWCEAppFont", (handler, view) =>
            {
                if (view is Label label && label.StyleId != "font-sample")
                {
                    label.SetDynamicResource(Label.FontFamilyProperty, "AppFontFamily");
                }
            });
            ButtonHandler.Mapper.AppendToMapping("OWCEAppFont", (handler, view) =>
            {
                if (view is Button button)
                {
                    button.SetDynamicResource(Button.FontFamilyProperty, "AppFontFamily");
                }
            });

            // Text in the theme builder's fonts is scaled so that every font looks about the same size.
            LabelHandler.Mapper.AppendToMapping(nameof(ITextStyle.Font), (handler, view) =>
            {
#if ANDROID
                OWCE.Droid.FontScaleStyle.Apply(handler.PlatformView, view);
#elif IOS
                OWCE.iOS.FontScaleStyle.Apply(handler.PlatformView.Font, view, font => handler.PlatformView.Font = font);
#endif
            });
            ButtonHandler.Mapper.AppendToMapping(nameof(ITextStyle.Font), (handler, view) =>
            {
#if ANDROID
                OWCE.Droid.FontScaleStyle.Apply(handler.PlatformView, (ITextStyle)view);
#elif IOS
                OWCE.iOS.FontScaleStyle.Apply(handler.PlatformView.TitleLabel?.Font, (ITextStyle)view, font => handler.PlatformView.TitleLabel.Font = font);
#endif
            });

            // Platform specific implementations of the interfaces in OWCE.DependencyInterfaces.
#if ANDROID
            builder.Services.AddSingleton<IOWBLE, OWCE.Droid.DependencyImplementations.OWBLE>();
            builder.Services.AddSingleton<IPermissionPrompt, OWCE.Droid.DependencyImplementations.PermissionPrompt>();
            builder.Services.AddSingleton<IUserAgent, OWCE.Droid.DependencyImplementations.UserAgent>();
            builder.Services.AddSingleton<IWatch, OWCE.Droid.DependencyImplementations.Watch>();
#elif IOS
            builder.Services.AddSingleton<IOWBLE, OWCE.iOS.DependencyImplementations.OWBLE>();
            builder.Services.AddSingleton<IPermissionPrompt, OWCE.iOS.DependencyImplementations.PermissionPrompt>();
            builder.Services.AddSingleton<IUserAgent, OWCE.iOS.DependencyImplementations.UserAgent>();
            builder.Services.AddSingleton<IWatch, OWCE.iOS.DependencyImplementations.Watch>();
#endif

            return builder.Build();
        }
    }
}
