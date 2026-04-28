using System;
using System.IO;
using System.Linq;

using WhiteNoiseGenerator;

namespace WhiteNoiseGenerator.Console
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string dataLengthString = args[0];
            var dataLength = Int64.Parse(dataLengthString);
            string filePath = args[1];

            GenerateWhiteNoise(filePath, dataLength);

            System.Console.WriteLine("Done");
        }

        private static void GenerateWhiteNoise(string filePath, long dataLength)
        {
            WhiteNoiseGenerator.CalculateSeedAndPatternLength(dataLength, out Int64 seedLength, out Int32 patternLength);
            var seedDataEnumerable = WhiteNoiseGenerator.GenerateSeedData(seedLength);
            var seedData = seedDataEnumerable.ToArray();

            var patternData = WhiteNoiseGenerator.GenerateLinearPattern(seedLength, patternLength);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                WhiteNoiseGenerator.GenerateWhiteNoise(fileStream, dataLength, seedData, patternData);
            }
        }
    }
}