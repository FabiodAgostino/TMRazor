using TMRazorImproved.Shared.Models;
using Xunit;

namespace TMRazorImproved.Tests.MockTests.Networking
{
    public class UOBufferReaderTests
    {
        [Fact]
        public void ReadInt32_ShouldReturnCorrectValue()
        {
            // Arange: Pacchetto con 0x00000001 in Big Endian
            byte[] data = { 0x00, 0x00, 0x00, 0x01 };
            var reader = new UOBufferReader(data);

            // Act
            int value = reader.ReadInt32();

            // Assert
            Assert.Equal(1, value);
            Assert.True(reader.AtEnd);
        }

        [Fact]
        public void ReadString_ShouldReturnCorrectString()
        {
            // Arrange: "UO" in ASCII (0x55, 0x4F) + null terminator
            byte[] data = { 0x55, 0x4F, 0x00 };
            var reader = new UOBufferReader(data);

            // Act
            string value = reader.ReadString(2);

            // Assert
            Assert.Equal("UO", value);
        }

        [Fact]
        public void ReadUnicodeString_ShouldReturnCorrectString()
        {
            // Arrange: "UO" in UTF-16 Big Endian (0x0055, 0x004F)
            byte[] data = { 0x00, 0x55, 0x00, 0x4F };
            var reader = new UOBufferReader(data);

            // Act
            string value = reader.ReadUnicodeString(2);

            // Assert
            Assert.Equal("UO", value);
        }

        [Fact]
        public void AtEnd_ShouldBeTrue_WhenBufferIsConsumed()
        {
            // Arrange
            byte[] data = { 0x01, 0x02 };
            var reader = new UOBufferReader(data);

            // Act & Assert
            reader.ReadByte();
            Assert.False(reader.AtEnd);
            reader.ReadByte();
            Assert.True(reader.AtEnd);
        }
    }
}
