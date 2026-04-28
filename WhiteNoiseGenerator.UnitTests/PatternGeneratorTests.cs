using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace WhiteNoiseGenerator.UnitTests
{
    [TestFixture]
    public class PatternGeneratorTests
    {
        [Test]
        public void TestRandomizerWithNoSeed_ShouldGenerateSame()
        {
            const Int32 dataLength = 11;

            var randomizerA = new Random();
            var dataA = new List<int>(dataLength);
            var randomizerB = new Random();
            var dataB = new List<int>(dataLength);

            for (int index = 0; index < dataLength; index++)
            {
                var value = randomizerA.Next();
                dataA.Add(value);
            }

            for (int index = 0; index < dataLength; index++)
            {
                var value = randomizerB.Next();
                dataB.Add(value);
            }

#if NET472
            Assert.AreEqual(dataA, dataB);
#elif NET8_0
            Assert.AreNotEqual(dataA, dataB);
#endif
        }

        [Test]
        public void TestGenerateRandomPatternWithNoSeed_ShouldGenerateSame()
        {
            Int32 sourceLength = 5;
            Int32 patternLength = 10;

            var patternData = PatternGenerator.GenerateRandomPattern(sourceLength, patternLength);
            var patternDataOne = patternData.ToArray();

            patternData = PatternGenerator.GenerateRandomPattern(sourceLength, patternLength);
            var patternDataTwo = patternData.ToArray();

#if NET472
            Assert.AreEqual(patternDataOne, patternDataTwo);
#elif NET8_0
            Assert.AreNotEqual(patternDataOne, patternDataTwo);
#endif
        }

        [Test]
        public void TestGenerateRandomPatternWithSameSeed_ShouldGenerateSame()
        {
            Int32 sourceLength = 5;
            Int32 patternLength = 10;
            Int32 randomizerSeed = 1234567890;

            var patternData = PatternGenerator.GenerateRandomPattern(sourceLength, patternLength,
                randomizerSeed: randomizerSeed);
            var patternDataOne = patternData.ToArray();

            patternData = PatternGenerator.GenerateRandomPattern(sourceLength, patternLength,
                randomizerSeed: randomizerSeed);
            var patternDataTwo = patternData.ToArray();

            Assert.AreEqual(patternDataOne, patternDataTwo);
        }

        [Test]
        public void TestGenerateRandomPatternWithDifferentSeeds_ShouldGenerateDifferent()
        {
            Int32 sourceLength = 5;
            Int32 patternLength = 10;
            Int32 randomizerSeedOne = 1;
            Int32 randomizerSeedTwo = 1234567890;

            var patternData = PatternGenerator.GenerateRandomPattern(sourceLength, patternLength,
                randomizerSeed: randomizerSeedOne);
            var patternDataOne = patternData.ToArray();

            patternData = PatternGenerator.GenerateRandomPattern(sourceLength, patternLength,
                randomizerSeed: randomizerSeedTwo);
            var patternDataTwo = patternData.ToArray();

            Assert.AreNotEqual(patternDataOne, patternDataTwo);
        }
    }
}