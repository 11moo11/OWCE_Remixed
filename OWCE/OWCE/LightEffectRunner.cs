using System.Diagnostics;

namespace OWCE
{
    /// <summary>
    /// Runs a light effect by writing front/back levels to the board a few times a second.
    /// Bluetooth writes are slow, so updates are limited (and the effects are slow fades, never flashes).
    /// </summary>
    public class LightEffectRunner
    {
        // About 5 updates per second (2 writes each) keeps Bluetooth free for the board's live data.
        const int TickMilliseconds = 200;
        const int StopTimeoutMilliseconds = 2000;

        readonly OWBoard _board;
        CancellationTokenSource _cancellation;
        Task _loop;
        ushort _modeToRestore = LightProtocol.ModeDefault;

        // Written from the UI thread, read by the effect loop.
        volatile float _speed = 5;
        volatile float _brightness = 0.6f;

        public LightEffect Effect { get; private set; } = LightEffect.None;
        public bool IsRunning => _loop != null && _loop.IsCompleted == false;

        /// <summary>Raised (on a background thread) if the effect stops because the board could not be written to.</summary>
        public event Action<string> Faulted;

        /// <summary>1 (slowest) to 10 (fastest). Can be changed while running.</summary>
        public double Speed
        {
            get => _speed;
            set => _speed = (float)Math.Clamp(value, 1, 10);
        }

        /// <summary>0 to 1. Can be changed while running.</summary>
        public double Brightness
        {
            get => _brightness;
            set => _brightness = (float)Math.Clamp(value, 0, 1);
        }

        public LightEffectRunner(OWBoard board)
        {
            _board = board;
        }

        public async Task StartAsync(LightEffect effect)
        {
            if (effect == LightEffect.None)
            {
                await StopAsync(restoreLights: true);
                return;
            }

            if (IsRunning)
            {
                // Switching effects: keep the mode we will restore to, and just replace the loop.
                await StopLoopAsync();
            }
            else
            {
                // Remember what to go back to. If the lights were in manual mode, go back to the default lights.
                _modeToRestore = _board.LightModeValue == LightProtocol.ModeManual ? LightProtocol.ModeDefault : _board.LightModeValue;
            }

            Effect = effect;
            _cancellation = new CancellationTokenSource();
            _loop = Task.Run(() => RunAsync(effect, _cancellation.Token));
        }

        public async Task StopAsync(bool restoreLights)
        {
            var wasRunning = IsRunning;
            await StopLoopAsync();
            Effect = LightEffect.None;

            if (wasRunning && restoreLights)
            {
                try
                {
                    await _board.RestoreLightModeAsync(_modeToRestore);
                }
                catch (Exception err)
                {
                    Debug.WriteLine("LightEffectRunner restore: " + err.Message);
                }
            }
        }

        async Task StopLoopAsync()
        {
            var cancellation = _cancellation;
            var loop = _loop;
            _cancellation = null;
            _loop = null;

            if (cancellation == null || loop == null)
            {
                return;
            }

            cancellation.Cancel();

            // A stuck Bluetooth write must not freeze the app.
            await Task.WhenAny(loop, Task.Delay(StopTimeoutMilliseconds));
            cancellation.Dispose();
        }

        async Task RunAsync(LightEffect effect, CancellationToken token)
        {
            try
            {
                await _board.EnsureManualLightsAsync();

                var clock = Stopwatch.StartNew();
                while (token.IsCancellationRequested == false)
                {
                    var tickStart = clock.ElapsedMilliseconds;

                    var (front, back) = LightEffects.GetFrame(effect, clock.Elapsed.TotalSeconds, LightEffects.PeriodForSpeed(_speed), _brightness);
                    await _board.WriteLightLevelsAsync(front, back);

                    var wait = TickMilliseconds - (int)(clock.ElapsedMilliseconds - tickStart);
                    if (wait > 0)
                    {
                        await Task.Delay(wait, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Stopped on purpose.
            }
            catch (Exception err)
            {
                Debug.WriteLine("LightEffectRunner: " + err.Message);
                if (token.IsCancellationRequested == false)
                {
                    Effect = LightEffect.None;
                    Faulted?.Invoke(err is InvalidOperationException ? err.Message : "Lost contact with the board's lights, so the effect was stopped.");
                }
            }
        }
    }
}
