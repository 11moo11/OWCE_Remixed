using System;

namespace OWCE
{
    /// <summary>White and red brightness for one end of the board. Each is 0..<see cref="Max"/>.</summary>
    public readonly record struct LightLevels(int White, int Red)
    {
        /// <summary>Brightest level the board accepts (0x75).</summary>
        public const int Max = 117;

        public LightLevels Clamped() => new LightLevels(Math.Clamp(White, 0, Max), Math.Clamp(Red, 0, Max));

        public static LightLevels FromPercent(double whitePercent, double redPercent) =>
            new LightLevels((int)Math.Round(whitePercent / 100.0 * Max), (int)Math.Round(redPercent / 100.0 * Max)).Clamped();

        public double WhitePercent => Math.Clamp(White, 0, Max) * 100.0 / Max;
        public double RedPercent => Math.Clamp(Red, 0, Max) * 100.0 / Max;
    }

    /// <summary>
    /// Light characteristics (LightsFront / LightsBack) are 2 bytes on the wire: [white, red].
    /// The BLE layer reverses bytes for us on read and write, so in the app the value is a ushort of (white &lt;&lt; 8) | red.
    /// Light modes (LightMode characteristic): 0 off, 1 default lights, 2 manual (front/back levels are used).
    /// Source: community documentation (pOnewheel), not verified against every board.
    /// </summary>
    public static class LightProtocol
    {
        public const ushort ModeOff = 0;
        public const ushort ModeDefault = 1;
        public const ushort ModeManual = 2;

        public static ushort ToValue(LightLevels levels)
        {
            levels = levels.Clamped();
            return (ushort)((levels.White << 8) | levels.Red);
        }

        public static LightLevels FromValue(ushort value) => new LightLevels(value >> 8, value & 0xFF);

        /// <summary>Bytes to hand to the BLE layer (it reverses them into [white, red] on the wire).</summary>
        public static byte[] Encode(LightLevels levels) => BitConverter.GetBytes(ToValue(levels));
    }

    public enum LightEffect
    {
        None,
        /// <summary>Normal colours (white front, red back) fade in and out.</summary>
        Breathe,
        /// <summary>Both ends fade white to red and back, together.</summary>
        Fade,
        /// <summary>Front and back fade between white and red in opposite phase.</summary>
        Swap,
    }

    /// <summary>Works out light levels over time for the effects. Pure maths, no Bluetooth.</summary>
    public static class LightEffects
    {
        // Keeps effects slow enough to never strobe. Updates are also limited by how fast Bluetooth writes go.
        public const double MinPeriodSeconds = 1.5;
        public const double MaxPeriodSeconds = 10;

        /// <summary>Speed 1 (slowest) to 10 (fastest) to seconds per cycle.</summary>
        public static double PeriodForSpeed(double speed)
        {
            speed = Math.Clamp(speed, 1, 10);
            return MaxPeriodSeconds - (speed - 1) / 9.0 * (MaxPeriodSeconds - MinPeriodSeconds);
        }

        /// <param name="seconds">Time since the effect started.</param>
        /// <param name="periodSeconds">Length of one cycle.</param>
        /// <param name="brightness">0..1 scale applied to the maximum level.</param>
        public static (LightLevels Front, LightLevels Back) GetFrame(LightEffect effect, double seconds, double periodSeconds, double brightness)
        {
            brightness = Math.Clamp(brightness, 0, 1);
            periodSeconds = Math.Max(periodSeconds, MinPeriodSeconds);

            var phase = (seconds / periodSeconds) % 1.0;
            var wave = 0.5 - 0.5 * Math.Cos(2 * Math.PI * phase); // 0 -> 1 -> 0 over one cycle

            int Level(double amount) => (int)Math.Round(Math.Clamp(amount, 0, 1) * brightness * LightLevels.Max);

            switch (effect)
            {
                case LightEffect.Breathe:
                    return (new LightLevels(Level(wave), 0), new LightLevels(0, Level(wave)));
                case LightEffect.Fade:
                    return (new LightLevels(Level(1 - wave), Level(wave)), new LightLevels(Level(1 - wave), Level(wave)));
                case LightEffect.Swap:
                    return (new LightLevels(Level(1 - wave), Level(wave)), new LightLevels(Level(wave), Level(1 - wave)));
                default:
                    return (new LightLevels(0, 0), new LightLevels(0, 0));
            }
        }
    }
}
