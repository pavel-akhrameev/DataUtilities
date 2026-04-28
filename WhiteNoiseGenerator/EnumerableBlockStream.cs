using System;
using System.Collections.Generic;
using System.IO;

namespace WhiteNoiseGenerator
{
    public class EnumerableBlockStream : Stream, IDisposable
    {
        public const Int32 BlockSize = 1048576; // 1 MiB 

        private readonly IEnumerable<byte[]> _enumerableBlocks;
        private readonly IEnumerator<byte[]> _enumerator;
        private readonly Int64 _dataLength;
        private readonly Int64 _enumerableLength;

        private Int64 _position;
        private Int32 _currentBlockIndex;
        private byte[] _currentBlock;

        public EnumerableBlockStream(IEnumerable<byte[]> enumerableBlocks, Int64 dataLength)
        {
            this._enumerableBlocks = enumerableBlocks;
            this._dataLength = dataLength;
            this._enumerator = _enumerableBlocks.GetEnumerator();

            this._enumerableLength = dataLength / BlockSize;
            if (dataLength % BlockSize != 0)
            {
                this._enumerableLength++;
            }

            this._position = 0;

            TakeFirstBlock();
        }

        public override Int32 Read(byte[] buffer, Int32 offset, Int32 count)
        {
            Int64 remaining = _dataLength - _position;
            if (remaining <= 0)
            {
                return 0;
            }

            Int64 toCopy = Math.Min(count, remaining);
            Int32 actualCopied = 0;
            while (toCopy > 0)
            {
                var remainingInCurrentBlock = RemainingDataLengthInCurrentBlock();
                if (remainingInCurrentBlock == 0)
                {
                    TakeNextBlock();
                }

                var actualCopiedLength = CurrentBlockToBuffer(buffer, offset, toCopy);

                _position += actualCopiedLength;
                actualCopied += actualCopiedLength;
                offset += actualCopiedLength;
                toCopy -= actualCopiedLength;
            }

            return actualCopied;
        }

        #region Overrides of Stream

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override Int64 Length => _dataLength;

        public override Int64 Position
        {
            get => _position;

            set
            {
                if (ValidateCanSeek(value))
                {
                    SeekInternal((Int32)value);
                }
            }
        }

        public override Int64 Seek(Int64 offset, SeekOrigin origin)
        {
            Int64 newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = _position + offset;
                    break;
                case SeekOrigin.End:
                    newPosition = _enumerableLength + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(offset), origin, null);
            }

            if (ValidateCanSeek(newPosition))
            {
                SeekInternal((Int32)newPosition);
            }

            return _position;
        }

        public override void Flush()
        {
            // Nothing to do here.
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        #endregion

        public new void Dispose()
        {
            base.Dispose();

            if (_enumerator != null)
            {
                _enumerator.Dispose();
            }
        }

        private Int32 CurrentBlockToBuffer(byte[] buffer, Int32 offset, Int64 count)
        {
            var remainingInBlock = RemainingDataLengthInCurrentBlock();
            if (remainingInBlock <= 0)
            {
                return 0;
            }

            Int32 toCopy = (Int32)Math.Min(count, remainingInBlock);
            Int32 indexInBlock = BlockSize - remainingInBlock;
            Array.Copy(_currentBlock, indexInBlock, buffer, offset, toCopy);

            return toCopy;
        }

        private bool ValidateCanSeek(long newPosition)
        {
            if (newPosition < _position)
            {
                throw new NotSupportedException("Seeking behind is not supported.");
            }

            if (newPosition >= _dataLength)
            {
                throw new ArgumentOutOfRangeException(nameof(newPosition), "New position is out of range.");
            }

            return true;
        }

        private void SeekInternal(Int64 newPosition)
        {
            Int32 actualBlockIndex = (Int32)(newPosition / BlockSize);

            for (; _currentBlockIndex < actualBlockIndex; _currentBlockIndex++)
            {
                _enumerator.MoveNext();
            }
            
            _currentBlock = _enumerator.Current;
            _position = newPosition;
        }

        private void TakeFirstBlock()
        {
            _enumerator.MoveNext();
            _currentBlock = _enumerator.Current;

            _currentBlockIndex = 0;
        }

        private void TakeNextBlock()
        {
            _enumerator.MoveNext();
            _currentBlock = _enumerator.Current;

            _currentBlockIndex++;
        }

        private Int32 RemainingDataLengthInCurrentBlock()
        {
            var remainingBytesInBlock = (Int32)((_currentBlockIndex + 1) * BlockSize - _position);
            return remainingBytesInBlock;
        }
    }
}