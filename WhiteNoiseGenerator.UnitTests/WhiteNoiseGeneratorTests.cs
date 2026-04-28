using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

using WhiteNoiseGenerator;

namespace WhiteNoiseGenerator.UnitTests
{
    [TestFixture]
    public class WhiteNoiseGeneratorTests
    {
        [Test]
        [TestCase(0, 0, 0)]
        [TestCase(1, 2 * WhiteNoiseGenerator.BlockSize, 1)]
        [TestCase(WhiteNoiseGenerator.BlockSize, 2 * WhiteNoiseGenerator.BlockSize, 1)]
        [TestCase(1048577, 3 * WhiteNoiseGenerator.BlockSize, 2)]
        [TestCase(9437185, 5 * WhiteNoiseGenerator.BlockSize, 10)]
        [TestCase(45L * WhiteNoiseGenerator.BlockSize, 10 * WhiteNoiseGenerator.BlockSize, 45)]
        [TestCase(5120L * WhiteNoiseGenerator.BlockSize, 102 * WhiteNoiseGenerator.BlockSize, 5120)]
        [TestCase(134209536L * WhiteNoiseGenerator.BlockSize, 16384L * WhiteNoiseGenerator.BlockSize, 134209536)] // 131 064 GiB
        public void TestCalculateSeedAndPatternLength(Int64 dataLength, Int64 expectedSeedLength, Int32 expectedPatternLength)
        {
            WhiteNoiseGenerator.CalculateSeedAndPatternLength(dataLength, out Int64 actualSeedLength, out Int32 actualPatternLength);

            Assert.AreEqual(expectedSeedLength, actualSeedLength);
            Assert.AreEqual(expectedPatternLength, actualPatternLength);
        }

        [Test]
        [Explicit("Takes time to complete")]
        public void TestWhiteNoiseStreamGeneration()
        {
            var startTime = DateTime.Now;
            
            const Int64 dataLength = Int32.MaxValue;
            const string tempFileName = "temp_test_file.bin";

            WhiteNoiseGenerator.CalculateSeedAndPatternLength(dataLength, out Int64 seedLength, out Int32 patternLength);
            var seedDataEnumerable = WhiteNoiseGenerator.GenerateSeedData(seedLength);
            var seedData = seedDataEnumerable.ToArray();

            var patternDataEnumerable = WhiteNoiseGenerator.GenerateRandomPattern(seedLength, patternLength);
            var patternData = patternDataEnumerable.ToArray();

            using (var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                WhiteNoiseGenerator.GenerateWhiteNoise(fileStream, dataLength, seedData, patternData);
            }

            // Act
            var actualStream = WhiteNoiseGenerator.GenerateWhiteNoiseStream(dataLength, seedData, patternData);

            const Int32 blockLength = UInt16.MaxValue;
            var expectedBlock = new byte[blockLength];
            var actualBlock = new byte[blockLength];

            using (var fileStream = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                for (Int64 index = 0; index < dataLength; index += blockLength)
                {
                    fileStream.Read(expectedBlock, 0, blockLength);
                    actualStream.Read(actualBlock, 0, blockLength);

                    Assert.AreEqual(expectedBlock, actualBlock);
                }
            }

            var elapsedTime = DateTime.Now.Subtract(startTime);
            Console.WriteLine("Regeneration and validation 2 GiB data finished. Elapsed time: {0}", elapsedTime);
            
            File.Delete(tempFileName);
        }
    }
}