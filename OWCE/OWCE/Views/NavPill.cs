using Microsoft.Maui.Controls.Shapes;

namespace OWCE.Views
{
    public enum NavPopStyle
    {
        /// <summary>Squashes small, then springs up.</summary>
        Bounce,
        /// <summary>A quarter turn, like a cog clicking round.</summary>
        Spin,
        /// <summary>A full turn backwards.</summary>
        SpinBack,
        /// <summary>A quick wobble.</summary>
        Wiggle,
    }

    /// <summary>
    /// A bottom navigation bar shaped like a pill. It holds a few big items (an icon above a label). One tinted pill glides under
    /// the selected item, stretching as it goes. Tap an item, or put a finger on the bar and slide: the pill follows the finger,
    /// and the whole screen above slides with it (see <see cref="Attach"/>).
    /// </summary>
    public class NavPill : Border
    {
        sealed class Item
        {
            public Grid Wrapper;
            public Microsoft.Maui.Controls.Shapes.Path Icon;
            public Label Label;
            public NavPopStyle Pop;
            public bool Selected;
        }

        const double BarPadding = 6;

        readonly Grid _root;
        readonly Grid _items;
        readonly Border _indicator;
        readonly List<Item> _list = new List<Item>();

        TabPager _pager;
        double _cell;
        double _position;

        // Dragging.
        double _dragStart;
        double _lastX;
        DateTime _lastTime;
        double _velocity;
        int _lastTick = -1;

        public NavPill()
        {
            HeightRequest = 76;
            StrokeThickness = 1.5;
            Padding = new Thickness(BarPadding);
            StrokeShape = new RoundRectangle() { CornerRadius = 38 };
            this.SetDynamicResource(BackgroundColorProperty, "SurfaceColor");
            this.SetDynamicResource(StrokeProperty, "DividerBrush");

            var tint = new BoxView() { InputTransparent = true };
            tint.SetDynamicResource(VisualElement.BackgroundColorProperty, "AccentColor");
            tint.Opacity = 0.18;

            _indicator = new Border()
            {
                StrokeThickness = 1.5,
                Padding = new Thickness(0),
                StrokeShape = new RoundRectangle() { CornerRadius = 32 },
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Start,
                InputTransparent = true,
                Content = tint,
            };
            _indicator.SetDynamicResource(Border.StrokeProperty, "AccentBrush");

            _items = new Grid() { ColumnSpacing = 0 };
            _root = new Grid() { BackgroundColor = Colors.Transparent };
            _root.Children.Add(_indicator);
            _root.Children.Add(_items);

            // One clear layer over the whole bar takes every touch, so a slide can start anywhere (including on the selected
            // tab) and a tap is turned into the tab under the finger.
            _items.InputTransparent = true;
            _indicator.VerticalOptions = LayoutOptions.Center;
            _indicator.HeightRequest = 56;
            var touch = new BoxView() { Color = Colors.Transparent };
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (sender, e) =>
            {
                var point = e.GetPosition(touch);
                if (point == null || _cell <= 0 || _pager == null)
                {
                    return;
                }

                var index = Math.Clamp((int)(point.Value.X / _cell), 0, _list.Count - 1);
                Motion.Tick();
                await _pager.SelectAsync(index);
            };
            touch.GestureRecognizers.Add(tap);
            var pan = new PanGestureRecognizer();
            pan.PanUpdated += Pan_PanUpdated;
            touch.GestureRecognizers.Add(pan);
            _root.Children.Add(touch);
            Content = _root;

            _root.SizeChanged += (sender, e) => UpdateCell();

        }

        /// <summary>
        /// Links the bar to a pager: tapping or sliding on the bar moves the pager, and the bar follows the pager
        /// wherever it goes (so the pill is always in step with the screen).
        /// </summary>
        public void Attach(TabPager pager)
        {
            _pager = pager;
            pager.PositionChanged += SetPosition;
            pager.TabSelected += (index, fromDrag) => PopItem(index);
            SetPosition(pager.Position);
        }

        /// <param name="iconPath">A vector icon, see Icons.</param>
        public void AddItem(string text, string iconPath, NavPopStyle pop = NavPopStyle.Bounce)
        {
            var index = _list.Count;

            var item = new Item() { Pop = pop };
            item.Icon = Icons.Create(iconPath, "MutedTextBrush", 26);
            item.Label = new Label()
            {
                Text = text,
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
            };

            var stack = new VerticalStackLayout()
            {
                Spacing = 3,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                InputTransparent = true,
            };
            stack.Children.Add(item.Icon);
            stack.Children.Add(item.Label);

            item.Wrapper = new Grid() { BackgroundColor = Colors.Transparent };
            item.Wrapper.Children.Add(stack);

            _list.Add(item);
            _items.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _items.Add(item.Wrapper, index, 0);

            ApplyColours(item, false);
            UpdateCell();
        }

        void UpdateCell()
        {
            if (_list.Count == 0 || _root.Width <= 0)
            {
                return;
            }

            _cell = _root.Width / _list.Count;
            _indicator.WidthRequest = _cell - 4;
            SetPosition(_position);
        }

        /// <summary>Puts the pill at a fractional item number. Called by the pager as it moves.</summary>
        public void SetPosition(double position)
        {
            // The screen may spring past its end; the pill stays inside the bar.
            position = Math.Clamp(position, 0, Math.Max(_list.Count - 1, 0));
            _position = position;
            if (_cell <= 0)
            {
                return;
            }

            // The pill stretches when it is between two items, and is its normal size on an item.
            // Only transforms change as it moves (a width change would lay the bar out again on every frame).
            var frac = position - Math.Floor(position);
            var stretch = Math.Sin(Math.PI * frac);
            _indicator.ScaleX = 1 + 0.28 * stretch;
            _indicator.TranslationX = position * _cell + 2;

            var nearest = Math.Clamp((int)Math.Round(position), 0, _list.Count - 1);
            for (var i = 0; i < _list.Count; ++i)
            {
                var item = _list[i];
                var closeness = Math.Clamp(1 - Math.Abs(i - position), 0, 1);

                // The item under the pill grows and lifts a little.
                item.Wrapper.Scale = 1 + 0.04 * closeness;

                var selected = i == nearest;
                if (selected != item.Selected)
                {
                    ApplyColours(item, selected);
                }
            }
        }

        static void ApplyColours(Item item, bool selected)
        {
            item.Selected = selected;
            if (selected)
            {
                item.Icon.SetDynamicResource(Microsoft.Maui.Controls.Shapes.Shape.FillProperty, "AccentBrush");
                item.Label.SetDynamicResource(Label.TextColorProperty, "OnSurfaceColor");
                item.Label.FontAttributes = FontAttributes.Bold;
            }
            else
            {
                item.Icon.SetDynamicResource(Microsoft.Maui.Controls.Shapes.Shape.FillProperty, "MutedTextBrush");
                item.Label.SetDynamicResource(Label.TextColorProperty, "MutedTextColor");
                item.Label.FontAttributes = FontAttributes.None;
            }
        }

        // A little flourish on the icon of the tab that was just chosen.
        async void PopItem(int index)
        {
            if (index < 0 || index >= _list.Count)
            {
                return;
            }

            var item = _list[index];
            var icon = item.Icon;
            try
            {
                icon.CancelAnimations();
                icon.Rotation = 0;
                icon.Scale = 1;

                switch (item.Pop)
                {
                    case NavPopStyle.Spin:
                        await icon.RotateToAsync(90, 650, Easing.SpringOut);
                        icon.Rotation = 0;
                        break;
                    case NavPopStyle.SpinBack:
                        await icon.RotateToAsync(-360, 700, Easing.CubicOut);
                        icon.Rotation = 0;
                        break;
                    case NavPopStyle.Wiggle:
                        icon.Rotation = -22;
                        await icon.RotateToAsync(0, 700, Easing.SpringOut);
                        break;
                    default:
                        icon.Scale = 0.55;
                        await icon.ScaleToAsync(1, 550, Easing.SpringOut);
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        void Pan_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (_pager == null || _cell <= 0)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _pager.AbortAnimation("TabPager");
                    _dragStart = _pager.Position;
                    _lastX = 0;
                    _lastTime = DateTime.UtcNow;
                    _velocity = 0;
                    _lastTick = (int)Math.Round(_dragStart);
                    _ = _indicator.ScaleToAsync(1.02, 120, Easing.CubicOut);
                    break;

                case GestureStatus.Running:
                {
                    var now = DateTime.UtcNow;
                    var elapsed = (now - _lastTime).TotalMilliseconds;
                    if (elapsed > 0)
                    {
                        // Positions per millisecond, smoothed.
                        var instant = (e.TotalX - _lastX) / _cell / elapsed;
                        _velocity = _velocity * 0.6 + instant * 0.4;
                    }
                    _lastX = e.TotalX;
                    _lastTime = now;

                    var position = _dragStart + e.TotalX / _cell;
                    var last = _list.Count - 1;

                    // Pulling past either end gets stiffer, like a rubber band.
                    if (position < 0)
                    {
                        position = position * 0.3;
                    }
                    else if (position > last)
                    {
                        position = last + (position - last) * 0.3;
                    }

                    _pager.SetPosition(position);

                    var nearest = Math.Clamp((int)Math.Round(position), 0, last);
                    if (nearest != _lastTick)
                    {
                        _lastTick = nearest;
                        Motion.Tick();
                    }
                    break;
                }

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                {
                    _ = _indicator.ScaleToAsync(1, 300, Easing.SpringOut);

                    // A quick flick carries on to the next item.
                    var flick = Math.Abs(_velocity) > 0.0008 ? Math.Sign(_velocity) * 0.6 : Math.Sign(_pager.Position - _dragStart) * 0.25;
                    var target = Math.Clamp((int)Math.Round(_pager.Position + flick), 0, _list.Count - 1);
                    _ = _pager.SelectAsync(target, fromDrag: true);
                    break;
                }
            }
        }

        /// <summary>The bar springs up from below the screen.</summary>
        public async void PlayEntrance()
        {
            try
            {
                this.CancelAnimations();
                TranslationY = 140;
                Opacity = 0;
                await Task.WhenAll(
                    this.TranslateToAsync(0, 0, 650, Easing.SpringOut),
                    this.FadeToAsync(1, 250, Easing.CubicOut));
            }
            catch (Exception)
            {
            }
        }
    }
}
