using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("WhiteNoiseGenerator.UnitTests")]

namespace WhiteNoiseGenerator
{
    internal static class PatternGenerator
    {
        public static IEnumerable<UInt32> GenerateLinearPattern(Int32 sourceBlockCount, Int32 patternLength)
        {
            // TODO: Checks

            var maxPossiblePatternLength = sourceBlockCount * (sourceBlockCount - 1) / 2;
            if (patternLength > maxPossiblePatternLength)
            {
                throw new ArgumentOutOfRangeException(nameof(patternLength),
                    "Target length exceeds {maxPossiblePatternLength} the maximum possible pattern length for provided source length.");
            }

            for (UInt32 indexI = 1; indexI < sourceBlockCount; indexI++)
            {
                var indexJBound = indexI;
                for (UInt32 indexJ = 0; indexJ < indexJBound; indexJ++)
                {
                    UInt32 patternValue = PatternValueConverter.Convert(indexI, indexJ);
                    yield return patternValue;
                }
            }

            yield break;
        }

        /// <returns>Randomized pattern sequence.</returns>
        /// <remarks>A kind of reservoir sampling.</remarks>
        public static IEnumerable<UInt32> GenerateRandomPattern(Int32 sourceBlockCount, Int32 patternLength,
            Int32 bufferSize = UInt16.MaxValue, Int32? randomizerSeed = null)
        {
            // TODO: Checks

            if (bufferSize > patternLength)
            {
                bufferSize = patternLength;
            }

            var buffer = new List<UInt32>(bufferSize);
            var random = randomizerSeed.HasValue
                ? new Random(randomizerSeed.Value)
                : new Random();

            var linearPattern = GenerateLinearPattern(sourceBlockCount, patternLength);
            using (IEnumerator<UInt32> enumerator = linearPattern.GetEnumerator())
            {
                while (buffer.Count < bufferSize && enumerator.MoveNext())
                {
                    buffer.Add(enumerator.Current);
                }

                while (enumerator.MoveNext())
                {
                    Int32 index = random.Next(buffer.Count);
                    yield return buffer[index];

                    buffer[index] = enumerator.Current;
                }
            }

            while (buffer.Count > 0)
            {
                Int32 index = random.Next(buffer.Count);
                yield return buffer[index];

                Int32 lastIndexInBuffer = buffer.Count - 1;
                buffer[index] = buffer[lastIndexInBuffer];
                buffer.RemoveAt(lastIndexInBuffer);
            }

            yield break;
        }
    }
}