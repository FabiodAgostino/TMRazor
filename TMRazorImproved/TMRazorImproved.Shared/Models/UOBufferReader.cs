using System;
using System.Buffers.Binary;
using System.Text;

namespace TMRazorImproved.Shared.Models
{
    /// <summary>
    /// Utility per la lettura dei dati dai pacchetti UO in modo performante (Big-Endian).
    /// </summary>
    public ref struct UOBufferReader
    {
        private readonly ReadOnlySpan<byte> _buffer;
        private int _position;

        public UOBufferReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
        }

        public int Position
        {
            get => _position;
            set => _position = value;
        }

        public byte ReadByte() => _buffer[_position++];

        public sbyte ReadSByte() => (sbyte)_buffer[_position++];

        public ushort ReadUInt16()
        {
            ushort value = BinaryPrimitives.ReadUInt16BigEndian(_buffer.Slice(_position));
            _position += 2;
            return value;
        }

        public uint ReadUInt32()
        {
            uint value = BinaryPrimitives.ReadUInt32BigEndian(_buffer.Slice(_position));
            _position += 4;
            return value;
        }

        public short ReadInt16()
        {
            short value = BinaryPrimitives.ReadInt16BigEndian(_buffer.Slice(_position));
            _position += 2;
            return value;
        }

        public int ReadInt32()
        {
            int value = BinaryPrimitives.ReadInt32BigEndian(_buffer.Slice(_position));
            _position += 4;
            return value;
        }

        public void Skip(int count)
        {
            _position += count;
        }

        public string ReadString(int fixedLength)
        {
            if (_position + fixedLength > _buffer.Length)
                fixedLength = _buffer.Length - _position;

            ReadOnlySpan<byte> slice = _buffer.Slice(_position, fixedLength);
            int zeroIndex = slice.IndexOf((byte)0);
            if (zeroIndex >= 0) slice = slice.Slice(0, zeroIndex);

            _position += fixedLength;
            return Encoding.ASCII.GetString(slice);
        }

        public string ReadString()
        {
            ReadOnlySpan<byte> slice = _buffer.Slice(_position);
            int zeroIndex = slice.IndexOf((byte)0);
            int len = zeroIndex >= 0 ? zeroIndex : slice.Length;
            string result = Encoding.ASCII.GetString(slice.Slice(0, len));
            _position += len + (zeroIndex >= 0 ? 1 : 0);
            return result;
        }

        public string ReadUnicodeString(int fixedLengthChars)
        {
            int byteLength = fixedLengthChars * 2;
            if (_position + byteLength > _buffer.Length)
                byteLength = _buffer.Length - _position;

            ReadOnlySpan<byte> slice = _buffer.Slice(_position, byteLength);
            // In UO, unicode is Big-Endian UTF-16
            
            _position += byteLength;
            return Encoding.BigEndianUnicode.GetString(slice).TrimEnd('\0');
        }

        public string ReadUnicodeString()
        {
            ReadOnlySpan<byte> slice = _buffer.Slice(_position);
            int zeroIndex = -1;
            for (int i = 0; i < slice.Length - 1; i += 2)
            {
                if (slice[i] == 0 && slice[i + 1] == 0)
                {
                    zeroIndex = i;
                    break;
                }
            }

            int len = zeroIndex >= 0 ? zeroIndex : (slice.Length & ~1);
            string result = Encoding.BigEndianUnicode.GetString(slice.Slice(0, len));
            _position += len + (zeroIndex >= 0 ? 2 : 0);
            return result;
        }

        public bool AtEnd => _position >= _buffer.Length;
        public int Remaining => _buffer.Length - _position;
    }
}
