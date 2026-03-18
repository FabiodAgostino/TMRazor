using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class GumpListViewModel : ObservableObject
    {
        private readonly IWorldService _worldService;
        private readonly IPacketService _packetService;

        [ObservableProperty]
        private ObservableCollection<UOGump> _openGumps = new();

        public GumpListViewModel(IWorldService worldService, IPacketService packetService, IMessenger messenger)
        {
            _worldService = worldService;
            _packetService = packetService;

            RefreshGumps();

            // Sottoscrizione ai messaggi di aggiornamento mondo
            messenger.Register<GumpListViewModel, GumpMessage>(this, (r, m) => {
                r.RefreshGumps();
            });
            messenger.Register<GumpListViewModel, GumpClosedMessage>(this, (r, m) => {
                r.RefreshGumps();
            });
        }

        [RelayCommand]
        private void RefreshGumps()
        {
            var gumps = _worldService.OpenGumps.Values.ToList();
            OpenGumps = new ObservableCollection<UOGump>(gumps);
        }

        [RelayCommand]
        private void CloseGump(UOGump gump)
        {
            if (gump == null) return;
            
            // Invia pacchetto di chiusura al server (0xBF sub 0x04 o risposta gump vuota)
            // Per semplicità usiamo l'interfaccia PacketService se ha metodi specifici o mandiamo un pacchetto generico
            // In Razor si usa spesso rispondere con button 0 per chiudere.
            
            _worldService.RemoveGump(gump.GumpId);
            RefreshGumps();
        }

        [RelayCommand]
        private void InspectGump(UOGump gump)
        {
            if (gump == null) return;
            WeakReferenceMessenger.Default.Send(new NavigateToInspectorMessage(gump));
        }
    }
}
