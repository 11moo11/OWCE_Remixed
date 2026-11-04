using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace OWCE.Views
{
    /// <summary>
    /// One panel on the board screen: the panel scaled to its size, and the controls shown over it while the layout is being edited.
    /// </summary>
    public class PanelHost : Grid
    {
        // Enough room for the edit controls even when the panel is small.
        const double EditingMinimumHeight = 80;

        readonly PanelScaleBox _scaleBox = new PanelScaleBox();
        readonly Grid _editOverlay;
        readonly Border _dropHighlight;
        readonly Label _upLabel;
        readonly Label _downLabel;
        readonly Label _widthLabel;
        readonly Label _sizeLabel;

        public string Id { get; }
        public string Title { get; }

        public event Action<PanelHost> MoveUpRequested;
        public event Action<PanelHost> MoveDownRequested;
        public event Action<PanelHost> ToggleWidthRequested;
        public event Action<PanelHost> CycleSizeRequested;

        public PanelHost(string id, string title, View panel)
        {
            Id = id;
            Title = title;

            _scaleBox.Panel = panel;
            Children.Add(_scaleBox);

            _upLabel = MakeChipLabel("↑");
            _downLabel = MakeChipLabel("↓");
            _widthLabel = MakeChipLabel("FULL");
            _sizeLabel = MakeChipLabel("M");

            var chips = new FlexLayout()
            {
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustify.Center,
                AlignItems = FlexAlignItems.Center,
            };
            chips.Children.Add(MakeChip(_upLabel, () => MoveUpRequested?.Invoke(this)));
            chips.Children.Add(MakeChip(_downLabel, () => MoveDownRequested?.Invoke(this)));
            chips.Children.Add(MakeChip(_widthLabel, () => ToggleWidthRequested?.Invoke(this)));
            chips.Children.Add(MakeChip(_sizeLabel, () => CycleSizeRequested?.Invoke(this)));

            var titleLabel = new Label()
            {
                Text = title.ToUpperInvariant(),
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                CharacterSpacing = 0.5,
                HorizontalOptions = LayoutOptions.Center,
            };

            // Dims the panel while editing. It is only visual: touches go through it (so a long press reaches this host,
            // which is what starts a drag) except for the buttons on it, which stay tappable (CascadeInputTransparent = false).
            // The panel itself is switched off while editing (see IsEditing) so taps do not toggle its expanders and so on.
            _editOverlay = new Grid()
            {
                IsVisible = false,
                InputTransparent = true,
                CascadeInputTransparent = false,
                BackgroundColor = Color.FromArgb("#B3000000"),
            };
            _editOverlay.Children.Add(new VerticalStackLayout()
            {
                VerticalOptions = LayoutOptions.Center,
                Spacing = 2,
                Children = { titleLabel, chips },
            });
            Children.Add(_editOverlay);

            _dropHighlight = new Border()
            {
                IsVisible = false,
                InputTransparent = true,
                StrokeThickness = 4,
                BackgroundColor = Colors.Transparent,
                StrokeShape = new RoundRectangle() { CornerRadius = 8 },
            };
            _dropHighlight.SetDynamicResource(Border.StrokeProperty, "OnBackgroundBrush");
            Children.Add(_dropHighlight);
        }

        public bool IsEditing
        {
            get => _editOverlay.IsVisible;
            set
            {
                _editOverlay.IsVisible = value;
                _scaleBox.Panel.InputTransparent = value;
                MinimumHeightRequest = value ? EditingMinimumHeight : -1;

                if (value == false)
                {
                    IsDropTarget = false;
                }
            }
        }

        /// <summary>Shown while another panel is being dragged over this one.</summary>
        public bool IsDropTarget
        {
            get => _dropHighlight.IsVisible;
            set => _dropHighlight.IsVisible = value;
        }

        /// <summary>Shows the panel's width and size, and which ways it can still move.</summary>
        public void Apply(PanelLayoutItem item, bool canMoveUp, bool canMoveDown)
        {
            _scaleBox.PanelScale = PanelLayout.ScaleFor(item.Size);
            _widthLabel.Text = item.Half ? "HALF" : "FULL";
            _sizeLabel.Text = PanelLayout.SizeName(item.Size);
            _upLabel.Opacity = canMoveUp ? 1.0 : 0.35;
            _downLabel.Opacity = canMoveDown ? 1.0 : 0.35;
        }

        static Label MakeChipLabel(string text)
        {
            var label = new Label()
            {
                Text = text,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            };
            label.SetDynamicResource(Label.TextColorProperty, "OnButtonColor");
            return label;
        }

        static Border MakeChip(Label label, Action onTap)
        {
            var chip = new Border()
            {
                Content = label,
                Padding = new Thickness(10, 0),
                MinimumWidthRequest = 36,
                HeightRequest = 32,
                Margin = new Thickness(3),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle() { CornerRadius = 8 },
            };
            chip.SetDynamicResource(VisualElement.BackgroundColorProperty, "ButtonColor");
            chip.SetDynamicResource(Border.StrokeProperty, "ButtonBorderBrush");
            chip.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(onTap) });
            return chip;
        }
    }
}
