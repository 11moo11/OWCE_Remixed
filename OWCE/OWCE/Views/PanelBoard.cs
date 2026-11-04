using CommunityToolkit.Mvvm.Messaging;

namespace OWCE.Views
{
    public sealed record PanelDefinition(string Id, string Title, View Panel);

    /// <summary>
    /// Arranges the panels on the board screen in a two column grid. Each panel can be half or full width, small, medium or
    /// large, and the order can be changed. Long press a panel and drag it onto another to move it, or use Edit panels in the menu.
    /// The layout is saved and comes back next time.
    /// </summary>
    public class PanelBoard : ContentView
    {
        public const string PreferenceKey = "panel_layout";

        const double Spacing = 16;
        // The space between panels is a little tighter than the space at the edge of the screen.
        const double Gap = 12;
        const double AutoScrollEdge = 90;
        const double AutoScrollStep = 60;

        readonly VerticalStackLayout _root = new VerticalStackLayout() { Spacing = Gap };
        readonly Grid _grid = new Grid() { ColumnSpacing = Gap, RowSpacing = Gap, Padding = new Thickness(Spacing, 0) };
        readonly Border _editBar;
        readonly Dictionary<string, PanelHost> _hosts = new Dictionary<string, PanelHost>();

        PanelLayout _layout;
        string _draggingId;
        DateTime _lastAutoScroll = DateTime.MinValue;

        public bool IsEditing { get; private set; }

        public PanelBoard()
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            _editBar = BuildEditBar();
            _editBar.IsVisible = false;

            _root.Children.Add(_editBar);
            _root.Children.Add(_grid);
            Content = _root;
        }

        /// <summary>Gives the board its panels. Call once, with the views already set up and bound.</summary>
        public void Configure(IEnumerable<PanelDefinition> panels)
        {
            var list = panels.ToList();
            _layout = PanelLayout.FromJson(Preferences.Get(PreferenceKey, string.Empty), list.Select(x => x.Id));

            foreach (var definition in list)
            {
                var host = new PanelHost(definition.Id, definition.Title, definition.Panel);
                host.MoveUpRequested += h => { _layout.MoveBy(h.Id, -1); Changed(); };
                host.MoveDownRequested += h => { _layout.MoveBy(h.Id, 1); Changed(); };
                host.ToggleWidthRequested += h => { _layout.SetHalf(h.Id, _layout.Find(h.Id).Half == false); Changed(); };
                host.CycleSizeRequested += h => { _layout.SetSize(h.Id, (_layout.Find(h.Id).Size + 1) % 3); Changed(); };

                AddDragAndDrop(host);
                _hosts.Add(definition.Id, host);
            }

            Rebuild();
        }

        public void StartEditing() => SetEditing(true);

        public void StopEditing() => SetEditing(false);

        void SetEditing(bool editing)
        {
            IsEditing = editing;
            _editBar.IsVisible = editing;

            foreach (var host in _hosts.Values)
            {
                host.IsEditing = editing;
            }
        }

        void Changed()
        {
            Preferences.Set(PreferenceKey, _layout.ToJson());
            Rebuild();
        }

        void ResetLayout()
        {
            _layout = PanelLayout.FromJson(null, PanelLayout.DefaultOrder.Concat(_hosts.Keys).Distinct().Where(_hosts.ContainsKey));
            Preferences.Remove(PreferenceKey);
            Rebuild();
        }

        void Rebuild()
        {
            _grid.Children.Clear();
            _grid.RowDefinitions.Clear();

            var cells = _layout.Pack();
            var rowCount = cells.Count == 0 ? 0 : cells.Max(x => x.Row) + 1;
            for (var i = 0; i < rowCount; ++i)
            {
                _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            foreach (var cell in cells)
            {
                var host = _hosts[cell.Id];
                var index = _layout.IndexOf(cell.Id);
                host.Apply(_layout.Find(cell.Id), canMoveUp: index > 0, canMoveDown: index < _layout.Items.Count - 1);
                host.IsEditing = IsEditing;

                Grid.SetRow(host, cell.Row);
                Grid.SetColumn(host, cell.Column);
                Grid.SetColumnSpan(host, cell.ColumnSpan);
                _grid.Children.Add(host);
            }
        }

        // ---- Edit bar ----

        Border BuildEditBar()
        {
            var label = new Label()
            {
                Text = "Editing panels. Hold and drag a panel to move it.",
                FontFamily = "SairaExtraCondensed-SemiBold",
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center,
            };
            label.SetDynamicResource(Label.TextColorProperty, "OnSurfaceColor");

            var reset = MakeBarButton("Reset", ResetLayout);
            var done = MakeBarButton("Done", StopEditing);

            var grid = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 6,
                Padding = new Thickness(12, 8),
            };
            grid.Add(label, 0, 0);
            grid.Add(reset, 1, 0);
            grid.Add(done, 2, 0);

            var bar = new Border()
            {
                Content = grid,
                Margin = new Thickness(Spacing, 0),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 8 },
            };
            bar.SetDynamicResource(VisualElement.BackgroundColorProperty, "SurfaceColor");
            bar.SetDynamicResource(Border.StrokeProperty, "DividerBrush");
            return bar;
        }

        static Border MakeBarButton(string text, Action onTap)
        {
            var label = new Label()
            {
                Text = text,
                FontFamily = "SairaExtraCondensed-Bold",
                FontSize = 16,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
            };
            label.SetDynamicResource(Label.TextColorProperty, "OnButtonColor");

            var button = new Border()
            {
                Content = label,
                Padding = new Thickness(14, 0),
                HeightRequest = 36,
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 8 },
            };
            button.SetDynamicResource(VisualElement.BackgroundColorProperty, "ButtonColor");
            button.SetDynamicResource(Border.StrokeProperty, "ButtonBorderBrush");
            button.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(onTap) });
            return button;
        }

        // ---- Drag and drop ----

        void AddDragAndDrop(PanelHost host)
        {
            var drag = new DragGestureRecognizer() { CanDrag = true };
            drag.DragStarting += (sender, e) =>
            {
                _draggingId = host.Id;
                e.Data.Text = host.Id;

                // Moving a panel is editing, so show the edit controls straight away.
                SetEditing(true);

                try
                {
                    HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
                }
                catch (Exception)
                {
                    // Not every device can do this.
                }
            };
            drag.DropCompleted += (sender, e) =>
            {
                _draggingId = null;
                ClearDropTargets();
            };
            host.GestureRecognizers.Add(drag);

            var drop = new DropGestureRecognizer() { AllowDrop = true };
            drop.DragOver += (sender, e) =>
            {
                if (_draggingId == null || _draggingId == host.Id)
                {
                    e.AcceptedOperation = DataPackageOperation.None;
                    return;
                }

                e.AcceptedOperation = DataPackageOperation.Copy;
                host.IsDropTarget = true;
                AutoScroll(e);
            };
            drop.DragLeave += (sender, e) => host.IsDropTarget = false;
            drop.Drop += (sender, e) =>
            {
                var draggedId = _draggingId;
                _draggingId = null;
                ClearDropTargets();

                if (draggedId != null && draggedId != host.Id)
                {
                    // The dragged panel takes the place of the one it was dropped on.
                    _layout.MoveTo(draggedId, _layout.IndexOf(host.Id));
                    Changed();
                }
            };
            host.GestureRecognizers.Add(drop);
        }

        void ClearDropTargets()
        {
            foreach (var host in _hosts.Values)
            {
                host.IsDropTarget = false;
            }
        }

        // The list can be longer than the screen, so scroll it when a panel is dragged near the top or bottom edge.
        void AutoScroll(DragEventArgs e)
        {
            if ((DateTime.UtcNow - _lastAutoScroll).TotalMilliseconds < 40)
            {
                return;
            }

            var scrollView = FindScrollView();
            var position = scrollView == null ? null : e.GetPosition(scrollView);
            if (scrollView == null || position == null)
            {
                return;
            }

            if (position.Value.Y < AutoScrollEdge)
            {
                _lastAutoScroll = DateTime.UtcNow;
                scrollView.ScrollToAsync(0, Math.Max(0, scrollView.ScrollY - AutoScrollStep), false);
            }
            else if (position.Value.Y > scrollView.Height - AutoScrollEdge)
            {
                _lastAutoScroll = DateTime.UtcNow;
                scrollView.ScrollToAsync(0, scrollView.ScrollY + AutoScrollStep, false);
            }
        }

        ScrollView FindScrollView()
        {
            Element element = this;
            while (element != null)
            {
                if (element is ScrollView scrollView)
                {
                    return scrollView;
                }

                element = element.Parent;
            }

            return null;
        }
    }
}
