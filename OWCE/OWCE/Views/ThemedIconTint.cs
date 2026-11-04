using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.Messaging;
using OWCE.Themes;

namespace OWCE.Views
{
    /// <summary>
    /// Tints an icon with one of the theme's colours and follows theme changes.
    /// (A DynamicResource set directly on IconTintColorBehavior does not update, because behaviours are not part of the
    /// element tree.) Most icons in the app are black, so without this they disappear on dark themes.
    /// Usage: &lt;views:ThemedIconTint ThemeColorKey="OnSurfaceColor" /&gt;
    /// </summary>
    public class ThemedIconTint : IconTintColorBehavior
    {
        public static readonly BindableProperty ThemeColorKeyProperty = BindableProperty.Create(
            nameof(ThemeColorKey),
            typeof(string),
            typeof(ThemedIconTint),
            "OnSurfaceColor",
            propertyChanged: (bindable, oldValue, newValue) => ((ThemedIconTint)bindable).ApplyTheme());

        /// <summary>The name of a colour in the theme, eg OnSurfaceColor.</summary>
        public string ThemeColorKey
        {
            get => (string)GetValue(ThemeColorKeyProperty);
            set => SetValue(ThemeColorKeyProperty, value);
        }

        public ThemedIconTint()
        {
            ApplyTheme();

            // The handler must not capture "this", or the messenger would keep the behaviour alive forever.
            WeakReferenceMessenger.Default.Register<ThemedIconTint, ThemeChangedMessage>(this, (recipient, message) =>
            {
                MainThread.BeginInvokeOnMainThread(recipient.ApplyTheme);
            });
        }

        void ApplyTheme()
        {
            if (Application.Current == null)
            {
                return;
            }

            TintColor = ThemeManager.GetColor(ThemeColorKey ?? "OnSurfaceColor", Colors.Black);
        }
    }
}
