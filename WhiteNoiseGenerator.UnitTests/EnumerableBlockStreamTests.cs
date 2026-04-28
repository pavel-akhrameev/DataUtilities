using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace WhiteNoiseGenerator.UnitTests
{
    [TestFixture]
    public class EnumerableBlockStreamTests
    {
        private const int BlockSize = EnumerableBlockStream.BlockSize;

        [Test]
        public void ReadEntireStream_ReturnsAllDataInOrder()
        {
            const int blockCount = 3;
            long dataLength = (long)BlockSize * blockCount;
            var dataBlocks = GenerateBlocks(blockCount, BlockSize);

            var stream = new EnumerableBlockStream(dataBlocks, dataLength);

            var buffer = new byte[dataLength];
            int bytesRead;
            int allBytesRead = 0;

            do
            {
                bytesRead = stream.Read(buffer, allBytesRead, buffer.Length - allBytesRead);
                allBytesRead += bytesRead;
            }
            while (bytesRead > 0);

            Assert.AreEqual(dataLength, allBytesRead);

            for (int byteIndex = 0; byteIndex < dataLength; byteIndex++)
            {
                var expectedValue = GetValue(byteIndex);
                Assert.AreEqual(expectedValue, buffer[byteIndex]);
            }
        }

        [Test]
        public void Position_SetAndSeekForward_WorksCorrectly()
        {
            int blockCount = 2;
            long dataLength = (long)BlockSize * blockCount;

            var dataBlocks = GenerateBlocks(blockCount, BlockSize);

            var stream = new EnumerableBlockStream(dataBlocks, dataLength);

            stream.Position = BlockSize + 10; // Seek forward into second block
            Assert.AreEqual(BlockSize + 10, stream.Position);

            var buffer = new byte[5];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(5, bytesRead);

            for (int byteIndex = 0; byteIndex < bytesRead; byteIndex++)
            {
                var expectedValue = GetValue(byteIndex + 10);
                Assert.AreEqual(expectedValue, buffer[byteIndex]);
            }
        }

        [Test]
        public void Seek_Backwards_ThrowsNotSupportedException()
        {
            int blockCount = 1;
            var dataBlocks = GenerateBlocks(blockCount, BlockSize);

            var stream = new EnumerableBlockStream(dataBlocks, BlockSize);
            stream.Position = 10;

            Assert.Throws<NotSupportedException>(() => stream.Position = 3);
            Assert.Throws<NotSupportedException>(() => stream.Seek(-5, SeekOrigin.Current));
        }

        [Test]
        public void SeekBeyondLength_ThrowsArgumentOutOfRangeException()
        {
            int blockCount = 1;
            var dataBlocks = GenerateBlocks(blockCount, BlockSize);

            var stream = new EnumerableBlockStream(dataBlocks, BlockSize);
            stream.Position = stream.Length - 1;

            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Position = stream.Length);
        }

        [Test]
        public void CanReadCanSeekCanWriteProperties()
        {
            int blockCount = 3;
            int miniBlockSize = 10;
            long dataLength = (long)BlockSize * blockCount;
            var dataBlocks = GenerateBlocks(blockCount, miniBlockSize);

            var stream = new EnumerableBlockStream(dataBlocks, dataLength);

            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanSeek);
            Assert.IsFalse(stream.CanWrite);
        }

        [Test]
        public void LengthProperty_ReturnsDataLength()
        {
            int blockCount = 1;
            long dataLength = 12345;
            var dataBlocks = GenerateBlocks(blockCount, BlockSize);

            var stream = new EnumerableBlockStream(dataBlocks, dataLength);

            Assert.AreEqual(dataLength, stream.Length);
        }

        [Test]
        public void ReadByte_ReturnsAllBytesInOrder()
        {
            int blockCount = 7;
            long dataLength = (long)BlockSize * blockCount;
            var dataBlocks = GenerateBlocks(blockCount, BlockSize);

            using (var stream = new EnumerableBlockStream(dataBlocks, dataLength))
            {
                for (long byteIndex = 0; byteIndex < dataLength; byteIndex++)
                {
                    var expectedValue = GetValue(byteIndex);
                    int actualValue = stream.ReadByte();

                    Assert.AreEqual(expectedValue, actualValue);
                }

                // After the end of the stream, ReadByte should return -1.
                int expectedEndOfStreamValue = -1;
                int actualEndOfStreamValue = stream.ReadByte();
                Assert.AreEqual(expectedEndOfStreamValue, actualEndOfStreamValue);
            }
        }

        [Test]
        public void ReadByte_PartialReadAndSeekForward()
        {
            int blockCount = 2;
            long dataLength = (long)BlockSize * blockCount;
            var dataBlocks = GenerateBlocks(blockCount, BlockSize);

            using (var stream = new EnumerableBlockStream(dataBlocks, dataLength))
            {
                for (int byteIndex = 0; byteIndex < 10; byteIndex++)
                {
                    var expectedValue = GetValue(byteIndex);
                    int actualValue = stream.ReadByte();
                    Assert.AreEqual(expectedValue, actualValue);
                }

                // Seek forward to a middle of the second data block.
                stream.Position = BlockSize + 5;

                for (int byteIndex = 0; byteIndex < 10; byteIndex++)
                {
                    var expectedValue = GetValue(BlockSize + 5 + byteIndex);
                    int actualValue = stream.ReadByte();
                    Assert.AreEqual(expectedValue, actualValue);
                }
            }
        }

        private static IEnumerable<byte[]> GenerateBlocks(int blockCount, int blockSize)
        {
            for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                var block = new byte[blockSize];
                for (int byteIndex = 0; byteIndex < block.Length; byteIndex++)
                {
                    var valueIndex = blockIndex * blockSize + byteIndex;
                    block[byteIndex] = GetValue(valueIndex);
                }

                yield return block;
            }
        }

        private static byte GetValue(Int64 valueIndex)
        {
            var value = (byte)(valueIndex % 256);
            return value;
        }
    }
}