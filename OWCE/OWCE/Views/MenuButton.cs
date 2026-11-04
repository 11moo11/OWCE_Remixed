using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace OWCE.Views
{
    /// <summary>
    /// A big rounded button for the side menu: an icon and a label on a tinted background. Made to be easy to hit while riding,
    /// so it is tall (64) and the whole thing is the touch target. A "danger" button is solid red, for things like Disconnect.
    /// </summary>
    public class MenuButton : Border
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuButton), string.Empty,
            propertyChanged: (b, o, n) => ((MenuButton)b)._label.Text = (string)n);

        /// <summary>The name of an image in Resources/Images. The images are black, and are tinted to match the button.</summary>
        public static readonly BindableProperty IconSourceProperty = BindableProperty.Create(nameof(IconSource), typeof(string), typeof(MenuButton), null,
            propertyChanged: (b, o, n) => ((MenuButton)b).UpdateIcon());

        /// <summary>A vector icon (see Icons) instead of an image.</summary>
        public static readonly BindableProperty IconPathDataProperty = BindableProperty.Create(nameof(IconPathData), typeof(string), typeof(MenuButton), null,
            propertyChanged: (b, o, n) => ((MenuButton)b).UpdateIcon());

        public static readonly BindableProperty IsDangerProperty = BindableProperty.Create(nameof(IsDanger), typeof(bool), typeof(MenuButton), false,
            propertyChanged: (b, o, n) => ((MenuButton)b).UpdateStyle());

        /// <summary>Makes the icon breathe (fade and swell). Used for something that is going on, like recording.</summary>
        public static readonly BindableProperty IsPulsingProperty = BindableProperty.Create(nameof(IsPulsing), typeof(bool), typeof(MenuButton), false,
            propertyChanged: (b, o, n) => ((MenuButton)b).UpdatePulse());

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(MenuButton));

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(MenuButton));

        readonly BoxView _background;
        readonly Label _label;
        readonly ContentView _iconHolder;

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }

        public string IconSource { get => (string)GetValue(IconSourceProperty); set => SetValue(IconSourceProperty, value); }

        public string IconPathData { get => (string)GetValue(IconPathDataProperty); set => SetValue(IconPathDataProperty, value); }

        public bool IsDanger { get => (bool)GetValue(IsDangerProperty); set => SetValue(IsDangerProperty, value); }

        public bool IsPulsing { get => (bool)GetValue(IsPulsingProperty); set => SetValue(IsPulsingProperty, value); }

        public ICommand Command { get => (ICommand)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

        public object CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

        /// <summary>Raised when the button is tapped (as well as running the command).</summary>
        public event EventHandler Tapped;

        public MenuButton()
        {
            HeightRequest = 64;
            StrokeThickness = 0;
            Padding = new Thickness(0);
            BackgroundColor = Colors.Transparent;
            StrokeShape = new RoundRectangle() { CornerRadius = 20 };

            _background = new BoxView();

            _label = new Label()
            {
                FontSize = 18,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
            };

            _iconHolder = new ContentView()
            {
                WidthRequest = 60,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(8, 0, 0, 0),
            };

            var grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                },
            };
            grid.Add(_background, 0, 0);
            Grid.SetColumnSpan(_background, 2);
            grid.Add(_iconHolder, 0, 0);
            grid.Add(_label, 1, 0);
            Content = grid;

            var tap = new TapGestureRecognizer();
            tap.Tapped += (sender, e) =>
            {
                Motion.Tick();

                // The button squashes and springs back, and its icon gives a little jump.
                _ = Motion.SquishAsync(this, 0.95);
                _ = JumpIconAsync();

                Tapped?.Invoke(this, EventArgs.Empty);

                var command = Command;
                if (command != null && command.CanExecute(CommandParameter))
                {
                    command.Execute(CommandParameter);
                }
            };
            GestureRecognizers.Add(tap);

            UpdateStyle();

            Loaded += (sender, e) => UpdatePulse();
            Unloaded += (sender, e) => this.AbortAnimation(PulseName);
        }

        const string PulseName = "MenuButtonPulse";

        void UpdatePulse()
        {
            this.AbortAnimation(PulseName);
            _iconHolder.Opacity = 1;
            _iconHolder.Scale = 1;

            if (IsPulsing == false || IsLoaded == false)
            {
                return;
            }

            var animation = new Animation();
            animation.Add(0, 0.5, new Animation(v => { _iconHolder.Opacity = v; _iconHolder.Scale = 1 + (1 - v) * 0.35; }, 1, 0.35, Easing.SinInOut));
            animation.Add(0.5, 1, new Animation(v => { _iconHolder.Opacity = v; _iconHolder.Scale = 1 + (1 - v) * 0.35; }, 0.35, 1, Easing.SinInOut));
            animation.Commit(this, PulseName, 16, 1300, repeat: () => true);
        }

        async Task JumpIconAsync()
        {
            if (IsPulsing)
            {
                return;
            }

            try
            {
                _iconHolder.CancelAnimations();
                await _iconHolder.TranslateToAsync(6, 0, 90, Easing.CubicOut);
                await _iconHolder.TranslateToAsync(0, 0, 400, Easing.SpringOut);
            }
            catch (Exception)
            {
            }
        }

        void UpdateStyle()
        {
            if (IsDanger)
            {
                // Solid red with white text.
                _background.Opacity = 1;
                _background.SetDynamicResource(VisualElement.BackgroundColorProperty, "DangerColor");
                _label.SetDynamicResource(Label.TextColorProperty, "OnDangerColor");
                _label.FontAttributes = FontAttributes.Bold;
            }
            else
            {
                // The accent colour at a low strength, so it is a tint of the theme.
                _background.Opacity = 0.14;
                _background.SetDynamicResource(VisualElement.BackgroundColorProperty, "AccentColor");
                _label.SetDynamicResource(Label.TextColorProperty, "OnSurfaceColor");
                _label.FontAttributes = FontAttributes.None;
            }

            UpdateIcon();
        }

        void UpdateIcon()
        {
            var colorKey = IsDanger ? "OnDangerColor" : "AccentColor";
            var brushKey = IsDanger ? "OnDangerBrush" : "AccentBrush";

            if (string.IsNullOrEmpty(IconPathData) == false)
            {
                _iconHolder.Content = Icons.Create(IconPathData, brushKey, 28);
            }
            else if (string.IsNullOrEmpty(IconSource) == false)
            {
                var image = new Image()
                {
                    Source = IconSource,
                    WidthRequest = 28,
                    HeightRequest = 28,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                };
                image.Behaviors.Add(new ThemedIconTint() { ThemeColorKey = colorKey });
                _iconHolder.Content = image;
            }
            else
            {
                _iconHolder.Content = null;
            }
        }
    }
}
