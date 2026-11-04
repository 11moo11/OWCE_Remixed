using AsyncAwaitBestPractices;
using System.ComponentModel;
using System.Globalization;

namespace OWCE.Views
{
    public partial class LightsView : ContentView
    {
        bool _isBusy = false;
        bool _isRefreshing = false;
        OWBoard _board;
        LightEffectRunner _runner;

        Slider[] _mixerSliders;
        Label[] _mixerLabels;
        (Button Button, LightEffect Effect)[] _effectButtons;

        public LightsView()
        {
            InitializeComponent();

            _mixerSliders = new Slider[] { FrontWhiteSlider, FrontRedSlider, BackWhiteSlider, BackRedSlider };
            _mixerLabels = new Label[] { FrontWhiteValueLabel, FrontRedValueLabel, BackWhiteValueLabel, BackRedValueLabel };
            _effectButtons = new[]
            {
                (BreatheButton, LightEffect.Breathe),
                (FadeButton, LightEffect.Fade),
                (SwapButton, LightEffect.Swap),
            };

            UpdateLabels();
            UpdateEffectButtons();

            // Stop any running effect when this view leaves the screen (eg the board is disconnected).
            Unloaded += (sender, e) =>
            {
                _runner?.StopAsync(restoreLights: false).SafeFireAndForget();
            };
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (_board != null)
            {
                _board.PropertyChanged -= Board_PropertyChanged;
                _runner?.StopAsync(restoreLights: false).SafeFireAndForget();
            }

            _board = BindingContext as OWBoard;
            _runner = null;

            if (_board != null)
            {
                _board.PropertyChanged += Board_PropertyChanged;

                _runner = new LightEffectRunner(_board);
                _runner.Speed = SpeedSlider.Value;
                _runner.Brightness = BrightnessSlider.Value / 100.0;
                _runner.Faulted += Runner_Faulted;
            }

            RefreshMixerFromBoard();
            UpdateEffectButtons();
        }

        void Board_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OWBoard.FrontLightMode) || e.PropertyName == nameof(OWBoard.RearLightMode))
            {
                MainThread.BeginInvokeOnMainThread(RefreshMixerFromBoard);
            }
        }

        // Header: tap to turn the lights on or off.
        async void OnTapped(object sender, TappedEventArgs e)
        {
            // Ignore taps while a change is still being sent to the board.
            if (_isBusy || _board == null)
            {
                return;
            }

            _isBusy = true;
            Opacity = 0.6;

            try
            {
                // An effect would keep overwriting the lights, so end it first.
                await StopEffectAsync(restoreLights: false);

                var error = await _board.SetLightsAsync(!_board.LightMode);
                if (error != null)
                {
                    await App.Current.RootPage.DisplayAlertAsync("Lights", error, "OK");
                }
            }
            finally
            {
                Opacity = 1.0;
                _isBusy = false;
            }
        }

        void OnArrowTapped(object sender, TappedEventArgs e)
        {
            DetailsPanel.IsVisible = !DetailsPanel.IsVisible;

            if (DetailsPanel.IsVisible)
            {
                ExpanderArrow.RotateToAsync(180, 200, Easing.CubicIn);
            }
            else
            {
                ExpanderArrow.RotateToAsync(0, 200, Easing.CubicOut);
            }
        }

        // ---- Mixer ----

        void RefreshMixerFromBoard()
        {
            // While an effect runs the board values are the effect's, not something to show on the mixer.
            if (_board == null || (_runner?.IsRunning ?? false))
            {
                return;
            }

            _isRefreshing = true;
            try
            {
                var front = _board.FrontLightLevels;
                var back = _board.RearLightLevels;

                FrontWhiteSlider.Value = Math.Round(front.WhitePercent);
                FrontRedSlider.Value = Math.Round(front.RedPercent);
                BackWhiteSlider.Value = Math.Round(back.WhitePercent);
                BackRedSlider.Value = Math.Round(back.RedPercent);
            }
            finally
            {
                _isRefreshing = false;
            }

            UpdateLabels();
        }

        void UpdateLabels()
        {
            // Sliders can raise ValueChanged while the XAML is still being set up.
            if (_mixerSliders == null)
            {
                return;
            }

            for (var i = 0; i < _mixerSliders.Length; ++i)
            {
                _mixerLabels[i].Text = Math.Round(_mixerSliders[i].Value).ToString(CultureInfo.CurrentCulture) + "%";
            }

            SpeedValueLabel.Text = Math.Round(SpeedSlider.Value).ToString(CultureInfo.CurrentCulture);
            BrightnessValueLabel.Text = Math.Round(BrightnessSlider.Value).ToString(CultureInfo.CurrentCulture) + "%";
        }

        void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            UpdateLabels();

            // Effect settings take effect straight away while an effect is running (no Bluetooth write needed here).
            if (_runner != null)
            {
                if (sender == SpeedSlider)
                {
                    _runner.Speed = e.NewValue;
                }
                else if (sender == BrightnessSlider)
                {
                    _runner.Brightness = e.NewValue / 100.0;
                }
            }
        }

        // Sent to the board when the user lets go of a slider, never while dragging.
        async void MixerSlider_DragCompleted(object sender, EventArgs e)
        {
            if (_isRefreshing || _board == null)
            {
                return;
            }

            // Changing a colour by hand ends any effect.
            await StopEffectAsync(restoreLights: false);

            var front = LightLevels.FromPercent(FrontWhiteSlider.Value, FrontRedSlider.Value);
            var back = LightLevels.FromPercent(BackWhiteSlider.Value, BackRedSlider.Value);

            var error = await _board.SetCustomLightsAsync(front, back);
            if (error != null)
            {
                await App.Current.RootPage.DisplayAlertAsync("Lights", error, "OK");
            }
        }

        // ---- Effects ----

        async Task StopEffectAsync(bool restoreLights)
        {
            if (_runner != null && _runner.IsRunning)
            {
                await _runner.StopAsync(restoreLights);
            }

            UpdateEffectButtons();
        }

        async void EffectButton_Clicked(object sender, EventArgs e)
        {
            if (_runner == null || sender is not Button button)
            {
                return;
            }

            var effect = _effectButtons.First(x => x.Button == button).Effect;

            if (_runner.IsRunning && _runner.Effect == effect)
            {
                // Tapping the running effect again stops it and puts the lights back how they were.
                await StopEffectAsync(restoreLights: true);
                return;
            }

            await _runner.StartAsync(effect);
            UpdateEffectButtons();
        }

        void Runner_Faulted(string message)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                UpdateEffectButtons();

                // If the view is gone (the board was disconnected) there is nobody to tell.
                if (IsLoaded)
                {
                    await App.Current.RootPage.DisplayAlertAsync("Lights", message, "OK");
                }
            });
        }

        void UpdateEffectButtons()
        {
            var selectedStyle = (Style)Application.Current.Resources["SelectedRoundButtonStyle"];
            var normalStyle = (Style)Application.Current.Resources["RoundButtonStyle"];

            foreach (var (button, effect) in _effectButtons)
            {
                var isRunning = _runner != null && _runner.IsRunning && _runner.Effect == effect;
                button.Style = isRunning ? selectedStyle : normalStyle;
                button.Margin = new Thickness(0, 0, 10, 0);
            }
        }
    }
}
