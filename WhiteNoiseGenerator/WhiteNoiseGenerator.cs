using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("WhiteNoiseGenerator.UnitTests")]

namespace WhiteNoiseGenerator
{
    public class WhiteNoiseGenerator
    {
        public const Int32 BlockSize = 1048576; // 1 MiB 

        public static void CalculateSeedAndPatternLength(Int64 dataLength, out Int64 seedLength,
            out Int32 patternLength)
        {
            if (dataLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dataLength));
            }
            else if (dataLength == 0)
            {
                seedLength = 0;
                patternLength = 0;

                return;
            }

            patternLength = (Int32)(dataLength / BlockSize);
            if (dataLength % BlockSize > 0)
            {
                patternLength++;
            }

            Double seedLengthRaw = (1 + Math.Sqrt(1 + 8 * patternLength)) / 2;
            seedLength = (Int64)Math.Ceiling(seedLengthRaw) * BlockSize;
        }

        public static IEnumerable<byte[]> GenerateWhiteNoise(Int64 dataLength, byte[][] seedData,
            IEnumerable<UInt32> patternData)
        {
            Int32 blocksCount = (Int32)(dataLength / BlockSize);
            if (dataLength % BlockSize > 0)
            {
                blocksCount++;
            }

            using (var patternEnumerator = patternData.GetEnumerator())
            {
                for (Int32 blockIndex = 0; blockIndex < blocksCount && patternEnumerator.MoveNext(); blockIndex++)
                {
                    (UInt16 blockComponentAIndex, UInt16 blockComponentBIndex) patternValue =
                        PatternValueConverter.Convert(patternEnumerator.Current);

                    byte[] blockComponentA = seedData[patternValue.blockComponentAIndex];
                    byte[] blockComponentB = seedData[patternValue.blockComponentBIndex];

                    byte[] blockData = XorPerformer.CombineDataBlock(blockComponentA, blockComponentB);
                    yield return blockData;
                }
            }

            yield break;
        }

        public static void GenerateWhiteNoise(Stream outputStream, Int64 dataLength, byte[][] seedData,
            IEnumerable<UInt32> patternData)
        {
            var blocksCount = (Int32)(dataLength / BlockSize);

            IEnumerable<byte[]> whiteNoiseData = GenerateWhiteNoise(dataLength, seedData, patternData);
            using (var blockEnumerator = whiteNoiseData.GetEnumerator())
            {
                for (Int32 blockIndex = 0; blockIndex < blocksCount; blockIndex++)
                {
                    blockEnumerator.MoveNext();
                    var dataBlock = blockEnumerator.Current;

                    outputStream.Write(dataBlock, 0, dataBlock.Length);
                }

                var remainingBytesCount = (Int32)(dataLength - blocksCount * BlockSize);
                if (remainingBytesCount > 0)
                {
                    blockEnumerator.MoveNext();
                    var lastDataBlock = blockEnumerator.Current;

                    outputStream.Write(lastDataBlock, 0, remainingBytesCount);
                }
            }
        }

        public static EnumerableBlockStream GenerateWhiteNoiseStream(Int64 dataLength, byte[][] seedData,
            IEnumerable<UInt32> patternData)
        {
            var whiteNoiseData = GenerateWhiteNoise(dataLength, seedData, patternData);
            var whiteNoiseStream = new EnumerableBlockStream(whiteNoiseData, dataLength);
            return whiteNoiseStream;
        }

        public static IEnumerable<byte[]> GenerateSeedData(Int64 seedLength)
        {
            var seedData = SeedGenerator.GenerateSeed(seedLength, BlockSize);
            return seedData;
        }

        public static void GenerateSeedData(Stream stream, Int32 seedLength)
        {
            ValidateStreamWritable(stream);

            var seedData = SeedGenerator.GenerateSeed(seedLength);
            foreach (byte seedByte in seedData)
            {
                stream.WriteByte(seedByte);
            }
        }

        public static Stream GenerateSeedDataStream(Int32 seedLength)
        {
            var seedDataStream = SeedGenerator.GenerateSeedDataStream(seedLength);
            return seedDataStream;
        }

        public static IEnumerable<UInt32> GenerateLinearPattern(Int64 seedLength, Int32 patternLength)
        {
            var seedBlockCount = (Int32)(seedLength / BlockSize);
            if (seedLength % BlockSize != 0)
            {
                seedBlockCount++;
            }

            var patternData = PatternGenerator.GenerateLinearPattern(seedBlockCount, patternLength);
            return patternData;
        }

        public static void GenerateLinearPattern(Stream stream, Int32 seedLength, Int32 patternLength)
        {
            ValidateStreamWritable(stream);

            var seedBlockCount = seedLength / BlockSize;
            if (seedLength % BlockSize != 0)
            {
                seedBlockCount++;
            }

            var patternData = PatternGenerator.GenerateLinearPattern(seedBlockCount, patternLength);
            using (var streamWriter = new StreamWriter(stream))
            {
                foreach (UInt64 patternValue in patternData)
                {
                    streamWriter.Write(patternValue);
                }
            }
        }

        public static IEnumerable<UInt32> GenerateRandomPattern(Int64 seedLength, Int32 patternLength,
            Int32? randomizerSeed = null)
        {
            var seedBlockCount = (Int32)(seedLength / BlockSize);
            if (seedLength % BlockSize != 0)
            {
                seedBlockCount++;
            }

            var patternData = PatternGenerator.GenerateRandomPattern(seedBlockCount, patternLength);
            return patternData;
        }

        public static void GenerateRandomPattern(Stream stream, Int32 seedLength, Int32 patternLength,
            Int32? randomizerSeed = null)
        {
            ValidateStreamWritable(stream);

            var seedBlockCount = seedLength / BlockSize;
            if (seedLength % BlockSize != 0)
            {
                seedBlockCount++;
            }

            Int32 randomizerSeedValue;
            if (randomizerSeed.HasValue)
            {
                randomizerSeedValue = randomizerSeed.Value;
            }
            else
            {
                var random = new Random(DateTime.Now.Millisecond);
                randomizerSeedValue = random.Next(Int32.MinValue, Int32.MaxValue);
            }

            var patternData = PatternGenerator.GenerateRandomPattern(seedBlockCount, patternLength,
                randomizerSeed: randomizerSeedValue);
            using (var streamWriter = new StreamWriter(stream))
            {
                foreach (UInt64 patternValue in patternData)
                {
                    streamWriter.Write(patternValue);
                }
            }
        }

        private static bool ValidateStreamWritable(Stream stream)
        {
            bool isWritable = stream.CanWrite;
            if (!isWritable)
            {
                throw new ArgumentException("Stream must be writable", nameof(stream));
            }

            return isWritable;
        }
    }
}