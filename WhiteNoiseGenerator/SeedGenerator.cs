using System;
using System.Collections.Generic;

namespace WhiteNoiseGenerator
{
    internal static class SeedGenerator
    {
        public static IEnumerable<Byte> GenerateSeed(Int64 seedLength, Int32? randomizerSeed = null)
        {
            var random = randomizerSeed.HasValue
                ? new Random(randomizerSeed.Value)
                : new Random();

            for (Int32 index = 0; index < seedLength; index++)
            {
                var value = random.Next(Byte.MinValue, Byte.MaxValue);
                yield return (Byte)value;
            }

            yield break;
        }

        public static IEnumerable<byte[]> GenerateSeed(Int64 seedLength, Int32 blockSize, Int32? randomizerSeed = null)
        {
            var seedBlockCount = seedLength / blockSize;
            if (seedLength % blockSize != 0)
            {
                seedBlockCount++;
            }

            var seedData = GenerateSeed(seedLength, randomizerSeed);
            using (var seedEnumerator = seedData.GetEnumerator())
            {
                for (int blockIndex = 0; blockIndex < seedBlockCount; blockIndex++)
                {
                    var dataBlock = new byte[blockSize];
                    for (int byteIndex = 0; byteIndex < blockSize && seedEnumerator.MoveNext(); byteIndex++)
                    {
                        var byteValue = seedEnumerator.Current;
                        dataBlock[byteIndex] = byteValue;
                    }

                    yield return dataBlock;
                }
            }

            yield break;
        }

        public static EnumerableByteStream GenerateSeedDataStream(Int32 seedLength)
        {
            var seedData = GenerateSeed(seedLength);
            var seedDataStream = new EnumerableByteStream(seedData, seedLength);
            return seedDataStream;
        }
    }
}