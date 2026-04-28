using System;
using System.Collections.Generic;
using System.IO;

namespace WhiteNoiseGenerator
{
    public class EnumerableByteStream : Stream, IDisposable
    {
        private readonly IEnumerable<byte> _enumerableData;
        private readonly IEnumerator<byte> _enumerator;
        private readonly Int64 _dataLength;

        private Int64 _position;

        public EnumerableByteStream(IEnumerable<byte> enumerable, Int64 dataLength)
        {
            this._enumerableData = enumerable;
            this._dataLength = dataLength;
            this._enumerator = _enumerableData.GetEnumerator();

            this._position = 0;
        }

        public override Int32 Read(byte[] buffer, Int32 offset, Int32 count)
        {
            var remaining = _dataLength - _position;
            if (remaining <= 0)
            {
                return 0;
            }

            Int64 toCopy = Math.Min(count, remaining);
            Int32 actualCopied = 0;
            for (Int32 index = 0; index < toCopy && _enumerator.MoveNext(); index++)
            {
                var value = _enumerator.Current;
                var destinationIndex = offset + index;
                buffer[destinationIndex] = value;

                actualCopied++;
            }

            _position += actualCopied;

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
                    newPosition = _dataLength + offset;
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
            for (Int64 index = _position; index < newPosition; index++)
            {
                _enumerator.MoveNext();
            }

            _position = newPosition;
        }
    }
}