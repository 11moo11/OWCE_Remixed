using System;

namespace OWCE
{
    /// <summary>
    /// Conversions and wire format for Custom Shaping (ride traits).
    ///
    /// Wire format (characteristic E659F31E): 2 bytes, [value, trait].
    /// Shaping values are signed bytes (two's complement). The "raw" values below are those signed values.
    ///
    ///   Aggressiveness   raw -80..127   shown as 0..10
    ///   TurnCompensation raw -100..100  shown as 0..10
    ///   AngleOffset      shown in degrees as (raw * 0.05), limited to -3.0..1.5 degrees (positive is nose up)
    ///
    /// Protocol details come from community work on OWCE (upstream PR #85 "Simple Stop + Custom Shaping")
    /// and have not been independently verified against official app traffic. See Documentation/CustomShaping.md.
    /// </summary>
    public static class CustomShaping
    {
        public const double UiMin = 0;
        public const double UiMax = 10;

        public const double AngleOffsetMinDegrees = -3.0;
        public const double AngleOffsetMaxDegrees = 1.5;
        const double AngleOffsetDegreesPerStep = 0.05;

        const int AggressivenessRawMin = -80;
        const int AggressivenessRawMax = 127;
        const int TurnCompensationRawMin = -100;
        const int TurnCompensationRawMax = 100;

        static readonly int AngleOffsetRawMin = (int)Math.Round(AngleOffsetMinDegrees / AngleOffsetDegreesPerStep); // -60
        static readonly int AngleOffsetRawMax = (int)Math.Round(AngleOffsetMaxDegrees / AngleOffsetDegreesPerStep); // 30

        public static (int Min, int Max) RawRange(RideTrait trait) => trait switch
        {
            RideTrait.Aggressiveness => (AggressivenessRawMin, AggressivenessRawMax),
            RideTrait.TurnCompensation => (TurnCompensationRawMin, TurnCompensationRawMax),
            RideTrait.AngleOffset => (AngleOffsetRawMin, AngleOffsetRawMax),
            _ => throw new ArgumentOutOfRangeException(nameof(trait), trait, "Not a custom shaping trait."),
        };

        public static bool IsShapingTrait(RideTrait trait)
        {
            return trait == RideTrait.AngleOffset || trait == RideTrait.TurnCompensation || trait == RideTrait.Aggressiveness;
        }

        /// <summary>Converts what the slider shows (0..10, or degrees for the angle offset) to the raw board value.</summary>
        public static int DisplayToRaw(RideTrait trait, double display)
        {
            switch (trait)
            {
                case RideTrait.Aggressiveness:
                    return Clamp(trait, (int)Math.Round(Rescale(Math.Clamp(display, UiMin, UiMax), UiMin, UiMax, AggressivenessRawMin, AggressivenessRawMax)));
                case RideTrait.TurnCompensation:
                    return Clamp(trait, (int)Math.Round(Rescale(Math.Clamp(display, UiMin, UiMax), UiMin, UiMax, TurnCompensationRawMin, TurnCompensationRawMax)));
                case RideTrait.AngleOffset:
                    return Clamp(trait, (int)Math.Round(Math.Clamp(display, AngleOffsetMinDegrees, AngleOffsetMaxDegrees) / AngleOffsetDegreesPerStep));
                default:
                    throw new ArgumentOutOfRangeException(nameof(trait), trait, "Not a custom shaping trait.");
            }
        }

        /// <summary>Converts a raw board value to what the slider shows.</summary>
        public static double RawToDisplay(RideTrait trait, int raw)
        {
            switch (trait)
            {
                case RideTrait.Aggressiveness:
                    return Math.Clamp(Rescale(raw, AggressivenessRawMin, AggressivenessRawMax, UiMin, UiMax), UiMin, UiMax);
                case RideTrait.TurnCompensation:
                    return Math.Clamp(Rescale(raw, TurnCompensationRawMin, TurnCompensationRawMax, UiMin, UiMax), UiMin, UiMax);
                case RideTrait.AngleOffset:
                    return Math.Clamp(raw * AngleOffsetDegreesPerStep, AngleOffsetMinDegrees, AngleOffsetMaxDegrees);
                default:
                    throw new ArgumentOutOfRangeException(nameof(trait), trait, "Not a custom shaping trait.");
            }
        }

        /// <summary>Limits a raw value to the range we are willing to send to the board.</summary>
        public static int Clamp(RideTrait trait, int raw)
        {
            var (min, max) = RawRange(trait);
            return Math.Clamp(raw, min, max);
        }

        /// <summary>Builds the 2 byte frame to write: [value, trait].</summary>
        public static byte[] BuildFrame(RideTrait trait, int raw)
        {
            raw = Clamp(trait, raw);
            return new byte[] { unchecked((byte)(sbyte)raw), (byte)trait };
        }

        /// <summary>Parses a 2 byte frame reported by the board: [value, trait].</summary>
        public static bool TryParseFrame(byte[] data, out RideTrait trait, out int raw)
        {
            trait = default;
            raw = 0;

            if (data == null || data.Length != 2 || Enum.IsDefined(typeof(RideTrait), data[1]) == false)
            {
                return false;
            }

            trait = (RideTrait)data[1];
            raw = unchecked((sbyte)data[0]);
            return true;
        }

        static double Rescale(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            return toMin + ((toMax - toMin) / (fromMax - fromMin)) * (value - fromMin);
        }
    }
}
