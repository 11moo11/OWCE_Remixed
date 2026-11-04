using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace OWCE.Views
{
    /// <summary>
    /// One row of a settings style list: a title, an optional line of detail, and something on the right (a switch, a value, an arrow).
    /// Put several in a <see cref="SettingsGroup"/>. The things that go on the right are the content, eg
    /// <c>&lt;views:SettingsItem Title="Units"&gt;&lt;Switch /&gt;&lt;/views:SettingsItem&gt;</c>.
    /// </summary>
    [ContentProperty(nameof(Trailing))]
    public class SettingsItem : Border
    {
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(SettingsItem), string.Empty,
            propertyChanged: (b, o, n) => ((SettingsItem)b)._titleLabel.Text = (string)n);

        public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(SettingsItem), string.Empty,
            propertyChanged: (b, o, n) => ((SettingsItem)b).UpdateSubtitle());

        public static readonly BindableProperty TrailingProperty = BindableProperty.Create(nameof(Trailing), typeof(View), typeof(SettingsItem), null,
            propertyChanged: (b, o, n) => ((SettingsItem)b).UpdateTrailing());

        public static readonly BindableProperty ShowChevronProperty = BindableProperty.Create(nameof(ShowChevron), typeof(bool), typeof(SettingsItem), false,
            propertyChanged: (b, o, n) => ((SettingsItem)b).UpdateTrailing());

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SettingsItem));

        // Outer corners of a group are round, the corners between items are only slightly round.
        public const double OuterRadius = 24;
        public const double InnerRadius = 6;

        readonly Label _titleLabel;
        readonly Label _subtitleLabel;
        readonly ContentView _trailingHolder;

        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

        public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }

        /// <summary>What is shown on the right, eg a Switch or a Label.</summary>
        public View Trailing { get => (View)GetValue(TrailingProperty); set => SetValue(TrailingProperty, value); }

        /// <summary>Shows an arrow on the right (when there is nothing else there) to say that this opens something.</summary>
        public bool ShowChevron { get => (bool)GetValue(ShowChevronProperty); set => SetValue(ShowChevronProperty, value); }

        /// <summary>Run when the row is tapped.</summary>
        public ICommand Command { get => (ICommand)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

        public SettingsItem()
        {
            StrokeThickness = 0;
            Padding = new Thickness(20, 14);
            MinimumHeightRequest = 64;
            SetCorners(OuterRadius, OuterRadius);
            this.SetDynamicResource(BackgroundColorProperty, "CardSurfaceColor");

            _titleLabel = new Label() { FontSize = 16, LineBreakMode = LineBreakMode.WordWrap };
            _titleLabel.SetDynamicResource(Label.TextColorProperty, "OnSurfaceColor");

            _subtitleLabel = new Label() { FontSize = 14, IsVisible = false, LineBreakMode = LineBreakMode.WordWrap };
            _subtitleLabel.SetDynamicResource(Label.TextColorProperty, "MutedTextColor");

            _trailingHolder = new ContentView() { VerticalOptions = LayoutOptions.Center, Margin = new Thickness(16, 0, 0, 0) };

            var grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                VerticalOptions = LayoutOptions.Center,
            };
            grid.Add(new VerticalStackLayout() { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { _titleLabel, _subtitleLabel } }, 0, 0);
            grid.Add(_trailingHolder, 1, 0);
            Content = grid;

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (sender, e) =>
            {
                var command = Command;
                if (command != null && command.CanExecute(null))
                {
                    // A quick dip so the row visibly reacts to the tap.
                    Opacity = 0.7;
                    command.Execute(null);
                    await this.FadeToAsync(1.0, 150);
                }
            };
            GestureRecognizers.Add(tap);
        }

        /// <summary>Sets how round the top and bottom corners are. SettingsGroup does this for you.</summary>
        public void SetCorners(double top, double bottom)
        {
            StrokeShape = new RoundRectangle() { CornerRadius = new CornerRadius(top, top, bottom, bottom) };
        }

        void UpdateSubtitle()
        {
            _subtitleLabel.Text = Subtitle;
            _subtitleLabel.IsVisible = string.IsNullOrEmpty(Subtitle) == false;
        }

        void UpdateTrailing()
        {
            if (Trailing != null)
            {
                _trailingHolder.Content = Trailing;
            }
            else if (ShowChevron)
            {
                _trailingHolder.Content = Icons.Create(Icons.ChevronRight, "MutedTextBrush", 24);
            }
            else
            {
                _trailingHolder.Content = null;
            }
        }
    }
}
