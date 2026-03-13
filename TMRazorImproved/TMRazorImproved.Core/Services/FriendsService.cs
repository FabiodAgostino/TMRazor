using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Messages;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Core.Services
{
    public class FriendsService : IFriendsService
    {
        private readonly IConfigService _config;
        private readonly IWorldService _worldService;
        private readonly IMessenger _messenger;

        public FriendsConfig ActiveList => _config.CurrentProfile.FriendsLists.FirstOrDefault(l => l.Name == _config.CurrentProfile.ActiveFriendsList) 
                                         ?? _config.CurrentProfile.FriendsLists[0];

        public FriendsService(IConfigService config, IWorldService worldService, IMessenger messenger)
        {
            _config = config;
            _worldService = worldService;
            _messenger = messenger;
        }

        public bool IsFriend(uint serial)
        {
            var config = ActiveList;

            // 1. Check direct players list
            if (config.Players.Any(p => p.Serial == serial && p.Enabled))
                return true;

            // 2. Check party if enabled
            if (config.IncludeParty)
            {
                // TODO: Need IPartyService or check world for party members
                // For now placeholder logic: if we have a way to know if serial is in party
            }

            var mobile = _worldService.FindMobile(serial);
            if (mobile == null) return false;

            // 3. Check Guilds and Factions via OPL
            if (mobile.OPL != null && mobile.OPL.Properties.Count > 0)
            {
                string firstProp = mobile.OPL.Properties[0].Arguments;

                // Factions
                if (config.SLFriend && firstProp.Contains("[SL]")) return true;
                if (config.TBFriend && firstProp.Contains("[TB]")) return true;
                if (config.COMFriend && firstProp.Contains("[CoM]")) return true;
                if (config.MINFriend && firstProp.Contains("[MiN]")) return true;

                // Guilds
                foreach (var guild in config.Guilds)
                {
                    if (guild.Enabled && firstProp.Contains($"[{guild.Name}]"))
                        return true;
                }
            }

            return false;
        }

        public void AddFriend(uint serial, string name)
        {
            var config = ActiveList;
            if (!config.Players.Any(p => p.Serial == serial))
            {
                config.Players.Add(new FriendPlayer { Serial = serial, Name = name, Enabled = true });
                _config.Save();
            }
        }

        public void RemoveFriend(uint serial)
        {
            var config = ActiveList;
            var friend = config.Players.FirstOrDefault(p => p.Serial == serial);
            if (friend != null)
            {
                config.Players.Remove(friend);
                _config.Save();
            }
        }

        public void AddGuild(string name)
        {
            var config = ActiveList;
            if (!config.Guilds.Any(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                config.Guilds.Add(new FriendGuild { Name = name, Enabled = true });
                _config.Save();
            }
        }

        public void RemoveGuild(string name)
        {
            var config = ActiveList;
            var guild = config.Guilds.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (guild != null)
            {
                config.Guilds.Remove(guild);
                _config.Save();
            }
        }

        public void CreateList(string name)
        {
            if (!_config.CurrentProfile.FriendsLists.Any(l => l.Name == name))
            {
                _config.CurrentProfile.FriendsLists.Add(new FriendsConfig { Name = name });
                _config.Save();
            }
        }

        public void DeleteList(string name)
        {
            if (_config.CurrentProfile.FriendsLists.Count > 1)
            {
                var list = _config.CurrentProfile.FriendsLists.FirstOrDefault(l => l.Name == name);
                if (list != null)
                {
                    _config.CurrentProfile.FriendsLists.Remove(list);
                    if (_config.CurrentProfile.ActiveFriendsList == name)
                        _config.CurrentProfile.ActiveFriendsList = _config.CurrentProfile.FriendsLists[0].Name;
                    _config.Save();
                }
            }
        }

        public void SwitchList(string name)
        {
            if (_config.CurrentProfile.FriendsLists.Any(l => l.Name == name))
            {
                _config.CurrentProfile.ActiveFriendsList = name;
                _config.Save();
            }
        }
    }
}
