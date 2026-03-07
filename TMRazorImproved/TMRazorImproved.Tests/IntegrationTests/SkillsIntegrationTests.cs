using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;
using Moq;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Buffers.Binary;

namespace TMRazorImproved.Tests.Integration
{
    public class SkillsIntegrationTests
    {
        [Fact]
        public void SkillsService_InitialState_ShouldBePopulated()
        {
            // ARRANGE
            var messenger = new WeakReferenceMessenger();
            var skillsService = new SkillsService(
                new Mock<IPacketService>().Object,
                messenger,
                NullLogger<SkillsService>.Instance
            );

            // ACT
            var skills = skillsService.Skills;
            var alchemy = skills[0];

            // ASSERT
            Assert.True(skills.Count >= 58, $"Expected standard UO skills, found {skills.Count}");
            Assert.Equal("Alchemy", alchemy.Name);
            Assert.Equal(0.0, alchemy.Value);
        }

        [Fact]
        public void SkillsService_ReceiveUpdate_ShouldUpdateValues()
        {
            // ARRANGE
            var messenger = new WeakReferenceMessenger();
            var skillsService = new SkillsService(
                new Mock<IPacketService>().Object,
                messenger,
                NullLogger<SkillsService>.Instance
            );

            // Costruiamo un pacchetto 0x3A realistico per aggiornamento singola skill (tipo 0xDF)
            // Formato: [1:0x3A][2:len][1:type=0xDF][2:skillId=0][2:val=1000][2:baseVal=955][1:lock=1][2:cap=1200]
            byte[] packet = new byte[13];
            packet[0] = 0x3A;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(1), 13);
            packet[3] = 0xDF; // Single skill with cap
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(4), 0);    // Alchemy ID
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(6), 1000); // 100.0%
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(8), 955);  // 95.5%
            packet[10] = (byte)SkillLock.Down;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(11), 1200); // 120.0% cap

            // ACT
            messenger.Send(new SkillsUpdatedMessage(packet));

            // ASSERT
            var alchemy = skillsService.Skills[0];
            Assert.Equal(100.0, alchemy.Value);
            Assert.Equal(95.5, alchemy.BaseValue);
            Assert.Equal(120.0, alchemy.Cap);
            Assert.Equal(SkillLock.Down, alchemy.Lock);
        }

        [Fact]
        public void SkillsService_Totals_ShouldSumBaseValues()
        {
            // ARRANGE
            var messenger = new WeakReferenceMessenger();
            var skillsService = new SkillsService(
                new Mock<IPacketService>().Object,
                messenger,
                NullLogger<SkillsService>.Instance
            );

            // ACT: Aggiorniamo due skill (usando il pacchetto 0xDF come sopra)
            UpdateSkillDirectly(messenger, 0, 500, 500); // Alchemy 50.0
            UpdateSkillDirectly(messenger, 1, 405, 405); // Anatomy 40.5

            // ASSERT
            Assert.Equal(90.5, skillsService.TotalBase);
        }

        private void UpdateSkillDirectly(IMessenger messenger, ushort id, ushort val, ushort baseVal)
        {
            byte[] packet = new byte[13];
            packet[0] = 0x3A;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(1), 13);
            packet[3] = 0xDF;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(4), id);
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(6), val);
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(8), baseVal);
            packet[10] = (byte)SkillLock.Up;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(11), 1000);
            messenger.Send(new SkillsUpdatedMessage(packet));
        }
    }
}
