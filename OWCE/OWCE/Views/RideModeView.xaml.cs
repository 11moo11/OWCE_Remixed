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

            _customShapingControls = new[]
            {
                new CustomShapingControl(RideTrait.Aggressiveness, AggressivenessSlider, AggressivenessValueLabel, "F1", ""),
                new CustomShapingControl(RideTrait.TurnCompensation, TurnCompensationSlider, TurnCompensationValueLabel, "F1", ""),
                new CustomShapingControl(RideTrait.AngleOffset, AngleOffsetSlider, AngleOffsetValueLabel, "F1", "°"),
            };

            var pan = new PanGestureRecognizer();
            pan.PanUpdated += Header_PanUpdated;
            Header.GestureRecognizers.Add(pan);

            // The arrows nudge now and then to say they can be swiped.
            Loaded += (sender, e) => StartHint();
            Unloaded += (sender, e) => this.AbortAnimation(HintName);
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

        void SetupRideModeButtons()
        {
            BuildDots();
            SyncFromBoard();
        }

        // ---- Choosing the ride mode by swiping ----

        const string HintName = "RideModeHint";

        int _modeIndex = -1;
        int _commitVersion = 0;
        bool _isAnimatingMode = false;
        readonly List<Border> _dots = new List<Border>();

        void BuildDots()
        {
            Dots.Children.Clear();
            _dots.Clear();

            if (_availableRideModes == null)
            {
                return;
            }

            foreach (var mode in _availableRideModes)
            {
                var dot = new Border()
                {
                    HeightRequest = 8,
                    WidthRequest = 8,
                    StrokeThickness = 0,
                    Opacity = 0.4,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle() { CornerRadius = 4 },
                };
                dot.SetDynamicResource(VisualElement.BackgroundColorProperty, "OnRideModePanel");
                _dots.Add(dot);
                Dots.Children.Add(dot);
            }
        }

        void UpdateDots(bool animate)
        {
            for (var i = 0; i < _dots.Count; ++i)
            {
                var selected = i == _modeIndex;
                var width = selected ? 24.0 : 8.0;
                var opacity = selected ? 1.0 : 0.4;

                if (animate)
                {
                    var dot = _dots[i];
                    var from = dot.WidthRequest;
                    dot.Animate("dot", new Animation(v => dot.WidthRequest = v, from, width, Easing.SpringOut), 16, 350);
                    _ = dot.FadeToAsync(opacity, 200);
                }
                else
                {
                    _dots[i].WidthRequest = width;
                    _dots[i].Opacity = opacity;
                }
            }
        }

        // Shows the board's ride mode (unless the user is in the middle of choosing one).
        void SyncFromBoard()
        {
            if (_availableRideModes == null || _board == null || _isAnimatingMode)
            {
                return;
            }

            var index = _availableRideModes.FindIndex(x => x.Item2 == _board.RideMode);
            if (index < 0)
            {
                _modeIndex = -1;
                ModeLabel.Text = _board.RideModeString;
                UpdateDots(false);
                return;
            }

            if (index != _modeIndex)
            {
                _modeIndex = index;
                ModeLabel.Text = _availableRideModes[index].Item1;
                UpdateDots(false);
            }
        }

        void StartHint()
        {
            this.AbortAnimation(HintName);

            // Slow breathing on the arrows.
            var animation = new Animation();
            animation.Add(0, 0.5, new Animation(v => { PreviousArrow.Opacity = v; NextArrow.Opacity = v; }, 1, 0.35, Easing.SinInOut));
            animation.Add(0.5, 1, new Animation(v => { PreviousArrow.Opacity = v; NextArrow.Opacity = v; }, 0.35, 1, Easing.SinInOut));
            animation.Commit(this, HintName, 16, 2200, repeat: () => true);
        }

        void PreviousArrow_Tapped(object sender, TappedEventArgs e) => _ = StepAsync(-1);

        void NextArrow_Tapped(object sender, TappedEventArgs e) => _ = StepAsync(1);

        void Header_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (_availableRideModes == null || _modeIndex < 0 || _isAnimatingMode)
            {
                return;
            }

            var width = Math.Max(Stage.Width, 1);

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                {
                    // The name follows the finger. At the first and last mode it resists.
                    var dx = e.TotalX;
                    var blocked = (dx > 0 && _modeIndex == 0) || (dx < 0 && _modeIndex == _availableRideModes.Count - 1);
                    ModeLabel.TranslationX = dx * (blocked ? 0.2 : 0.7);
                    ModeLabel.Opacity = 1 - Math.Min(Math.Abs(dx) / width, 1) * 0.6;
                    break;
                }

                case GestureStatus.Completed:
                {
                    var dx = e.TotalX;
                    if (Math.Abs(dx) > 50)
                    {
                        _ = StepAsync(dx < 0 ? 1 : -1);
                    }
                    else
                    {
                        _ = SettleLabelAsync();
                    }
                    break;
                }

                case GestureStatus.Canceled:
                    _ = SettleLabelAsync();
                    break;
            }
        }

        async Task SettleLabelAsync()
        {
            await Task.WhenAll(
                ModeLabel.TranslateToAsync(0, 0, 400, Easing.SpringOut),
                ModeLabel.FadeToAsync(1, 200));
        }

        // Moves one ride mode along (direction 1 = next, -1 = previous). The new mode slides in from that side.
        async Task StepAsync(int direction)
        {
            if (_availableRideModes == null || _modeIndex < 0 || _isAnimatingMode)
            {
                return;
            }

            var target = _modeIndex + direction;
            if (target < 0 || target >= _availableRideModes.Count)
            {
                Motion.Tick(HapticFeedbackType.LongPress);
                await SettleLabelAsync();
                return;
            }

            _isAnimatingMode = true;
            try
            {
                Motion.Tick();
                _modeIndex = target;
                UpdateDots(true);

                var distance = Math.Max(Stage.Width, 1) * 0.5;
                await Task.WhenAll(
                    ModeLabel.TranslateToAsync(-direction * distance, 0, 130, Easing.CubicIn),
                    ModeLabel.FadeToAsync(0, 130));

                ModeLabel.Text = _availableRideModes[target].Item1;
                ModeLabel.TranslationX = direction * distance;

                await Task.WhenAll(
                    ModeLabel.TranslateToAsync(0, 0, 450, Easing.SpringOut),
                    ModeLabel.FadeToAsync(1, 200));
            }
            finally
            {
                _isAnimatingMode = false;
            }

            _ = CommitAfterPauseAsync(_availableRideModes[target].Item2);
        }

        // The board is only told once the user has stopped swiping, so swiping past modes does not send each one.
        async Task CommitAfterPauseAsync(ushort rideMode)
        {
            var version = ++_commitVersion;
            await Task.Delay(450);
            if (version != _commitVersion)
            {
                return;
            }

            _board?.ChangeRideMode(rideMode);

            // Then show whatever the board reports, once it has had time to answer.
            await Task.Delay(1500);
            if (version == _commitVersion)
            {
                SyncFromBoard();
            }
        }

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
