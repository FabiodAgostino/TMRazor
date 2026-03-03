using System;
using System.Collections.Generic;
using System.Linq;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.Core.Services
{
    public class VendorService : AgentServiceBase, IVendorService, IRecipient<UOPacketMessage>
    {
        private readonly IPacketService _packetService;
        private readonly IConfigService _configService;
        private readonly ILogger<VendorService> _logger;
        private readonly IMessenger _messenger;

        public VendorService(
            IPacketService packetService, 
            IConfigService configService,
            IMessenger messenger,
            ILogger<VendorService> logger)
        {
            _packetService = packetService;
            _configService = configService;
            _messenger = messenger;
            _logger = logger;

            _messenger.Register<UOPacketMessage>(this);
        }

        public void Receive(UOPacketMessage message)
        {
            if (message.Path != Shared.Enums.PacketPath.ServerToClient) return;

            byte[] data = message.Value.Data;
            if (data.Length == 0) return;

            if (data[0] == 0x24 && _configService.CurrentProfile.Vendor.BuyEnabled)
            {
                HandleBuyMenu(data);
            }
            else if (data[0] == 0x9E && _configService.CurrentProfile.Vendor.SellEnabled)
            {
                HandleSellMenu(data);
            }
        }

        private void HandleBuyMenu(byte[] data)
        {
            _logger.LogInformation("Vendor Buy Menu detected");
            var reader = new UOBufferReader(data);
            reader.ReadByte(); // 0x24
            reader.ReadUInt16(); // length
            uint vendorSerial = reader.ReadUInt32();
            byte count = reader.ReadByte();

            var buyList = _configService.CurrentProfile.Vendor.BuyList;
            var toBuy = new List<(uint Serial, ushort Amount)>();

            for (int i = 0; i < count; i++)
            {
                uint price = reader.ReadUInt32();
                byte nameLen = reader.ReadByte();
                string name = reader.ReadString(nameLen);
                
                // In 0x24 il seriale e il graphic dell'item sono in pacchetti container 
                // che arrivano subito dopo o sono già nel world.
                // Questa è un'implementazione semplificata che richiede il matching per nome o ID.
                // Per ora simuliamo la logica di TMRazor.
            }

            // TODO: Implementare il parsing completo della lista oggetti e l'invio di 0x3B
        }

        private void HandleSellMenu(byte[] data)
        {
            _logger.LogInformation("Vendor Sell Menu detected");
            // Simile a buy, analizza 0x9E e invia 0x9F
        }

        protected override async System.Threading.Tasks.Task AgentLoopAsync(System.Threading.CancellationToken token)
        {
            // Il Vendor Agent è reattivo ai pacchetti, non richiede un loop continuo
            await System.Threading.Tasks.Task.Delay(-1, token);
        }
    }
}
