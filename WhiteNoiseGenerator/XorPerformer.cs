using System;
using System.Numerics;

namespace WhiteNoiseGenerator
{
    internal static class XorPerformer
    {
        /// <returns>Array with XOR data of input arrays.</returns>
        public static byte[] CombineDataBlock(byte[] blockComponentA, byte[] blockComponentB)
        {
            if (blockComponentA.Length != blockComponentB.Length)
            {
                throw new ArgumentException("Arrays must be of the same length");
            }

            var blockLength = blockComponentA.Length;
            var result = new byte[blockLength];

            int vectorSize = Vector<byte>.Count;

            int byteIndex = 0;
            for (; byteIndex <= blockLength - vectorSize; byteIndex += vectorSize)
            {
                var vectorA = new Vector<byte>(blockComponentA, byteIndex);
                var vectorB = new Vector<byte>(blockComponentB, byteIndex);
                (vectorA ^ vectorB).CopyTo(result, byteIndex);
            }

            // Processing remaining data byte by byte.
            for (; byteIndex < blockLength; byteIndex++)
            {
                result[byteIndex] = (byte)(blockComponentA[byteIndex] ^ blockComponentB[byteIndex]);
            }

            return result;
        }
    }
}