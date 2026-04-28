using System;

namespace WhiteNoiseGenerator
{
    internal static class PatternValueConverter
    {
        private const Int32 ShiftCount = 16;

        public static (UInt16 valueA, UInt16 valueB) Convert(UInt32 patternValue)
        {
            const UInt32 mask = 0xFFFF;

            var patternValueA = patternValue >> ShiftCount;
            var patternValueB = patternValue & mask;

            return ((UInt16)patternValueA, (UInt16)patternValueB);
        }

        public static UInt32 Convert(UInt16 valueA, UInt16 valueB)
        {
            UInt32 patternValue = Convert((UInt32)valueA, (UInt32)valueB);
            return patternValue;
        }

        public static UInt32 Convert(UInt32 valueA, UInt32 valueB)
        {
            UInt32 patternValue = valueA << ShiftCount;
            patternValue |= valueB;

            return patternValue;
        }
    }
}