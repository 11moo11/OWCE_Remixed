using CommunityToolkit.Mvvm.Messaging;
using OWCE.Themes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;
using OWCE.Extensions;

namespace OWCE.Views
{
    public class RideModeButton : INotifyPropertyChanged
    {
        protected string _name;
        public string Name
        {
            get { return _name; }
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        protected ushort _value;
        public ushort Value
        {
            get { return _value; }
            set { if (_value != value) { _value = value; OnPropertyChanged(); } }
        }

        protected int _rideMode;
        public int RideMode
        {
            get { return _rideMode; }
            set { if (_rideMode != value) { _rideMode = value; OnPropertyChanged(); } }
        }

        protected bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class RideModeView : ContentView
    {
        public static readonly BindableProperty BoardTypeProperty = BindableProperty.Create(
            "BoardType",
            typeof(OWBoardType),
            typeof(RideModeView));

        public OWBoardType BoardType
        {
            get { return (OWBoardType)GetValue(BoardTypeProperty); }
            set
            {
                SetValue(BoardTypeProperty, value);
            }
        }

        public static readonly BindableProperty SimpleStopEnabledProperty = BindableProperty.Create(
            "SimpleStopEnabled",
            typeof(bool?),
            typeof(RideModeView));

        public bool? SimpleStopEnabled
        {
            get { return (bool?)GetValue(SimpleStopEnabledProperty); }
            set
            {
                SetValue(SimpleStopEnabledProperty, value);
            }
        }

        public RideModeView()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<RideModeView, ThemeChangedMessage>(this, (recipient, message) => recipient.UpdateButtonRadius());

            _customShapingControls = new[]
            {
                new CustomShapingControl(RideTrait.Aggressiveness, AggressivenessSlider, AggressivenessValueLabel, "F1", ""),
                new CustomShapingControl(RideTrait.TurnCompensation, TurnCompensationSlider, TurnCompensationValueLabel, "F1", ""),
                new CustomShapingControl(RideTrait.AngleOffset, AngleOffsetSlider, AngleOffsetValueLabel, "F1", "°"),
            };
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext is OWBoard board)
            {
                _availableRideModes = board.GetAvailableRideModes();
                SetupRideModeButtons();
            }

            HookBoard();
        }

        List<(string, ushort)> _availableRideModes = null;

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName.Equals(BoardTypeProperty.PropertyName))
            {
                if (BindingContext is OWBoard board)
                {
                    _availableRideModes = board.GetAvailableRideModes();
                    SetupRideModeButtons();
                }
            }
        }

        // ---- Choosing the ride mode ----

        sealed class ModeButton
        {
            public ushort Value;
            public Border Border;
            public BoxView Tint;
            public Label Label;
        }

        readonly List<ModeButton> _modeButtons = new List<ModeButton>();

        // One big button for each ride mode the board has, two to a row.
        void SetupRideModeButtons()
        {
            ModeGrid.Children.Clear();
            ModeGrid.RowDefinitions.Clear();
            ModeGrid.ColumnDefinitions.Clear();
            _modeButtons.Clear();

            if (_availableRideModes == null)
            {
                return;
            }

            ModeGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ModeGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            for (var i = 0; i < _availableRideModes.Count; ++i)
            {
                var mode = _availableRideModes[i];
                if (i % 2 == 0)
                {
                    ModeGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                }

                var button = BuildModeButton(mode.Item1, mode.Item2);
                _modeButtons.Add(button);

                // An odd one out at the end gets the whole row.
                var last = i == _availableRideModes.Count - 1 && i % 2 == 0;
                ModeGrid.Add(button.Border, i % 2, i / 2);
                if (last)
                {
                    Grid.SetColumnSpan(button.Border, 2);
                }
            }

            _shownModeValue = null;
            SyncFromBoard();
        }

        ModeButton BuildModeButton(string name, ushort value)
        {
            var button = new ModeButton() { Value = value };

            button.Tint = new BoxView() { InputTransparent = true };

            button.Label = new Label()
            {
                Text = name,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
            };

            var grid = new Grid() { Children = { button.Tint, button.Label } };

            button.Border = new Border()
            {
                HeightRequest = 64,
                StrokeThickness = 0,
                Padding = new Thickness(0),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = ButtonRadius },
                Content = grid,
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (sender, e) => ModeButtonTapped(button);
            button.Border.GestureRecognizers.Add(tap);

            ApplyModeButtonState(button, false);
            return button;
        }

        // The buttons follow the theme's corners (a little less round than the panel they sit in).
        static double ButtonRadius => ThemeManager.CornerRadiusFor(ThemeManager.Current.Corners) * 0.65;

        void UpdateButtonRadius()
        {
            foreach (var button in _modeButtons)
            {
                button.Border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = ButtonRadius };
            }
        }

        static void ApplyModeButtonState(ModeButton button, bool selected)
        {
            button.Tint.Opacity = 1;
            if (selected)
            {
                // The chosen mode is in the accent colour.
                button.Tint.SetDynamicResource(VisualElement.BackgroundColorProperty, "RideModeButtonSelectedColor");
                button.Label.SetDynamicResource(Label.TextColorProperty, "OnRideModeButtonSelectedColor");
            }
            else
            {
                button.Tint.SetDynamicResource(VisualElement.BackgroundColorProperty, "RideModeButtonColor");
                button.Label.SetDynamicResource(Label.TextColorProperty, "OnRideModeButtonColor");
            }
        }

        ushort? _shownModeValue;

        void ShowSelectedMode(ushort? value)
        {
            _shownModeValue = value;
            foreach (var button in _modeButtons)
            {
                ApplyModeButtonState(button, value.HasValue && button.Value == value.Value);
            }
        }

        void ModeButtonTapped(ModeButton button)
        {
            Motion.Tick();
            _ = Motion.SquishAsync(button.Border, 0.94);

            if (_shownModeValue == button.Value)
            {
                return;
            }

            // Highlight straight away, then tell the board.
            ShowSelectedMode(button.Value);
            _ignoreBoardUntil = DateTime.UtcNow.AddSeconds(1.9);
            _board?.ChangeRideMode(button.Value);
            _ = ResyncLaterAsync();
        }

        int _syncVersion = 0;

        // After the board has had time to answer, show what it says (in case the change did not happen).
        async Task ResyncLaterAsync()
        {
            var version = ++_syncVersion;
            await Task.Delay(2000);
            if (version == _syncVersion)
            {
                SyncFromBoard();
            }
        }

        // Shows the board's ride mode.
        void SyncFromBoard()
        {
            if (_board == null)
            {
                return;
            }

            // While a change the user just made is waiting for the board, leave the highlight alone.
            if (DateTime.UtcNow < _ignoreBoardUntil)
            {
                return;
            }

            var known = _availableRideModes != null && _availableRideModes.Any(x => x.Item2 == _board.RideMode);
            ShowSelectedMode(known ? _board.RideMode : (ushort?)null);
        }

        DateTime _ignoreBoardUntil = DateTime.MinValue;

        // ---- Custom shaping ----

        const string CustomShapingWarningAcceptedKey = "custom_shaping_warning_accepted";

        record CustomShapingControl(RideTrait Trait, Slider Slider, Label ValueLabel, string Format, string Suffix);

        readonly CustomShapingControl[] _customShapingControls;
        OWBoard _board;

        void HookBoard()
        {
            if (_board != null)
            {
                _board.PropertyChanged -= Board_PropertyChanged;
            }

            _board = BindingContext as OWBoard;

            if (_board != null)
            {
                _board.PropertyChanged += Board_PropertyChanged;
            }

            RefreshCustomShaping();
            SyncFromBoard();
        }

        void Board_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case nameof(OWBoard.RideMode):
                case nameof(OWBoard.CanCustomShape):
                case nameof(OWBoard.BoardType):
                case nameof(OWBoard.AngleOffset):
                case nameof(OWBoard.TurnCompensation):
                case nameof(OWBoard.Aggressiveness):
                case nameof(OWBoard.HasCustomShapingValues):
                    // Board updates arrive on Bluetooth threads.
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        RefreshCustomShaping();
                        if (e.PropertyName == null || e.PropertyName == nameof(OWBoard.RideMode))
                        {
                            SyncFromBoard();
                        }
                    });
                    break;
            }
        }

        // Values are snapped to 0.1 which is also what the board can sensibly represent.
        static double Snap(double value) => Math.Round(value, 1);

        void RefreshCustomShaping()
        {
            var board = _board;

            var show = board != null && board.CanCustomShape && board.IsInCustomRideMode;
            if (show && CustomShapingPanel.IsVisible == false)
            {
                // Opens up with a little spring.
                CustomShapingPanel.Opacity = 0;
                CustomShapingPanel.TranslationY = -16;
                CustomShapingPanel.IsVisible = true;
                _ = CustomShapingPanel.FadeToAsync(1, 250, Easing.CubicOut);
                _ = CustomShapingPanel.TranslateToAsync(0, 0, 450, Easing.SpringOut);
            }
            else
            {
                CustomShapingPanel.IsVisible = show;
            }

            if (show == false)
            {
                return;
            }

            // Moving the sliders to the board's values must not look like the user changing them.
            _isRefreshingCustomShaping = true;
            try
            {
                foreach (var control in _customShapingControls)
                {
                    var raw = board.GetCustomShapingValue(control.Trait);
                    if (raw.HasValue)
                    {
                        control.Slider.Value = Snap(CustomShaping.RawToDisplay(control.Trait, raw.Value));
                    }
                    else
                    {
                        control.ValueLabel.Text = "-";
                    }
                }
            }
            finally
            {
                _isRefreshingCustomShaping = false;
            }

            CustomShapingSliders.IsEnabled = board.HasCustomShapingValues;

            if (board.HasCustomShapingValues == false)
            {
                CustomShapingStatusLabel.Text = "Waiting for the board to report its current settings...";
            }
            else if (board.BoardType == OWBoardType.Pint)
            {
                CustomShapingStatusLabel.Text = "Pint custom shaping needs a board patched with Rewheel.";
            }
            else
            {
                CustomShapingStatusLabel.Text = string.Empty;
            }

            CustomShapingStatusLabel.IsVisible = string.IsNullOrEmpty(CustomShapingStatusLabel.Text) == false;
        }

        bool _isRefreshingCustomShaping = false;


        void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var control = _customShapingControls.FirstOrDefault(x => x.Slider == sender);
            if (control != null)
            {
                control.ValueLabel.Text = Snap(e.NewValue).ToString(control.Format, System.Globalization.CultureInfo.CurrentCulture) + control.Suffix;
            }
        }

        // Only sent to the board when the user lets go of the slider, never while dragging.
        async void Slider_DragCompleted(object sender, EventArgs e)
        {
            var control = _customShapingControls.FirstOrDefault(x => x.Slider == sender);
            if (control == null || _board == null || _isRefreshingCustomShaping)
            {
                return;
            }

            if (await ConfirmCustomShapingWarningAsync() == false)
            {
                RefreshCustomShaping();
                return;
            }

            var error = await _board.SetCustomShapingValueAsync(control.Trait, Snap(control.Slider.Value));
            if (error != null)
            {
                await App.Current.RootPage.DisplayAlertAsync("Custom shaping", error, "OK");
                RefreshCustomShaping();
            }
        }

        static async Task<bool> ConfirmCustomShapingWarningAsync()
        {
            if (Preferences.Get(CustomShapingWarningAcceptedKey, false))
            {
                return true;
            }

            var accepted = await App.Current.RootPage.DisplayAlertAsync(
                "Custom shaping",
                "This changes how your board rides and is sent straight to the board. Make small changes and check how it feels.",
                "I understand",
                "Cancel");

            if (accepted)
            {
                Preferences.Set(CustomShapingWarningAcceptedKey, true);
            }

            return accepted;
        }
    }
}
