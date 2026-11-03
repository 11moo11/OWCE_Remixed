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
            SliderHandler.Mapper.AppendToMapping("OWCEThickTrack", (handler, view) =>
            {
                if (view is ThickSlider thickSlider)
                {
#if ANDROID
                    OWCE.Droid.ThickSliderStyle.Apply(handler, thickSlider);
#elif IOS
                    OWCE.iOS.ThickSliderStyle.Apply(handler, thickSlider);
#endif
                }
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
