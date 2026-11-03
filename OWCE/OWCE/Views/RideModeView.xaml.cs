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

        ObservableCollection<RideModeButton> _rideModesList = new ObservableCollection<RideModeButton>();

        public RideModeView()
        {
            InitializeComponent();

            _customShapingControls = new[]
            {
                new CustomShapingControl(RideTrait.Aggressiveness, AggressivenessSlider, AggressivenessValueLabel, "F1", ""),
                new CustomShapingControl(RideTrait.TurnCompensation, TurnCompensationSlider, TurnCompensationValueLabel, "F1", ""),
                new CustomShapingControl(RideTrait.AngleOffset, AngleOffsetSlider, AngleOffsetValueLabel, "F1", "°"),
            };
        }

        void ExpanderView_PropertyChanged(System.Object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Expander.IsExpandedProperty.PropertyName.Equals(e.PropertyName))
            {
                if (ExpanderView.IsExpanded)
                {
                    ExpanderArrow.RotateToAsync(180, 200, Easing.CubicIn);
                }
                else
                {
                    ExpanderArrow.RotateToAsync(0, 200, Easing.CubicOut);
                }
            }
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
            _rideModesList.Clear();

            foreach (var rideMode in _availableRideModes)
            {
                _rideModesList.Add(new RideModeButton()
                {
                    Name = rideMode.Item1,
                    Value = rideMode.Item2,
                    IsSelected = false,
                });
            }

            //RideModesCollectionView.ItemsSource = null;
            //RideModesCollectionView.ItemsSource = _rideModesList;
            BindableLayout.SetItemsSource(RideModesStackLayout, null);
            BindableLayout.SetItemsSource(RideModesStackLayout, _rideModesList);
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
                    MainThread.BeginInvokeOnMainThread(RefreshCustomShaping);
                    break;
            }
        }

        // Values are snapped to 0.1 which is also what the board can sensibly represent.
        static double Snap(double value) => Math.Round(value, 1);

        void RefreshCustomShaping()
        {
            var board = _board;
            var show = board != null && board.CanCustomShape && board.IsInCustomRideMode;
            CustomShapingPanel.IsVisible = show;

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

        async void ReloadCustomShaping_Clicked(object sender, EventArgs e)
        {
            if (_board == null)
            {
                return;
            }

            try
            {
                await _board.RequestCustomShapingValuesAsync();
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine("ReloadCustomShaping_Clicked: " + err.Message);
                await App.Current.RootPage.DisplayAlertAsync("Custom shaping", "Could not ask the board for its settings.", "OK");
            }
        }

        void RideModeButton_Clicked(System.Object sender, System.EventArgs e)
        {
            foreach (var button in _rideModesList)
            {
                button.IsSelected = false;
            }

            if (sender is Button senderButton)
            {
                var selectedRideMode = _rideModesList.FirstOrDefault(x => x.Name == senderButton.Text);
                if (selectedRideMode != null)
                {
                    selectedRideMode.IsSelected = true;

                    if (BindingContext is OWBoard board)
                    {
                        board.ChangeRideMode(selectedRideMode.Value);
                    }
                }


               //VisualStateManager.GoToState(button, selectedRideMode.IsSelected ? "SelectedState" : "NormalState");
            }
        }
    }
}
