using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// Exposes ClassicUO-integration functions to scripts as the <c>CUO</c> global variable.
    /// Mirrors <c>RazorEnhanced.CUO</c> from the legacy codebase.
    ///
    /// NOTE: Methods that relied on in-process reflection of the ClassicUO assembly are unavailable
    /// in TMRazorImproved (which runs out-of-process).  Where possible, equivalent behaviour is
    /// achieved via <see cref="IPacketService"/> or <see cref="IClientInteropService"/>.
    /// Unavailable methods log a warning and are no-ops so that existing scripts do not crash.
    /// </summary>
    public class CuoApi
    {
        private readonly IPacketService _packet;
        private readonly IClientInteropService _interop;
        private readonly IWorldService _world;
        private readonly ScriptCancellationController _cancel;
        private readonly ILogger<CuoApi>? _logger;

        // Follow-mobile state (tracked in-process, movement handled via IPathFindingService if available)
        private static uint _followSerial = 0;

        public CuoApi(
            IPacketService packet,
            IClientInteropService interop,
            IWorldService world,
            ScriptCancellationController cancel,
            ILogger<CuoApi>? logger = null)
        {
            _packet = packet;
            _interop = interop;
            _world = world;
            _cancel = cancel;
            _logger = logger;
        }

        // ------------------------------------------------------------------
        // Map / World Map
        // ------------------------------------------------------------------

        /// <summary>Reloads world-map markers. Requires in-process CUO access — stub in TMRazorImproved.</summary>
        public virtual void LoadMarkers()
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.LoadMarkers() requires in-process ClassicUO access — not available in TMRazorImproved.");
        }

        /// <summary>Pans the world-map to the specified coordinates. Requires in-process CUO access — stub.</summary>
        public virtual void GoToMarker(int x, int y)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.GoToMarker({X},{Y}) requires in-process ClassicUO access — not available in TMRazorImproved.", x, y);
        }

        /// <summary>Toggles the free-view mode on the world map. Stub in TMRazorImproved.</summary>
        public virtual void FreeView(bool free)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.FreeView({Free}) requires in-process ClassicUO access — not available in TMRazorImproved.", free);
        }

        /// <summary>Closes the world-map gump. Stub in TMRazorImproved.</summary>
        public virtual bool CloseTMap()
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.CloseTMap() requires in-process ClassicUO access — not available in TMRazorImproved.");
            return false;
        }

        // ------------------------------------------------------------------
        // Profile / Settings
        // ------------------------------------------------------------------

        /// <summary>Sets a ClassicUO profile boolean property. Stub in TMRazorImproved.</summary>
        public virtual void ProfilePropertySet(string propertyName, bool enable)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.ProfilePropertySet({Name}) requires in-process ClassicUO access — not available in TMRazorImproved.", propertyName);
        }

        /// <summary>Sets a ClassicUO profile integer property. Stub in TMRazorImproved.</summary>
        public virtual void ProfilePropertySet(string propertyName, int value)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.ProfilePropertySet({Name}) requires in-process ClassicUO access — not available in TMRazorImproved.", propertyName);
        }

        /// <summary>Sets a ClassicUO profile string property. Stub in TMRazorImproved.</summary>
        public virtual void ProfilePropertySet(string propertyName, string value)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.ProfilePropertySet({Name}) requires in-process ClassicUO access — not available in TMRazorImproved.", propertyName);
        }

        /// <summary>Reads a ClassicUO setting by name. Returns empty string — stub in TMRazorImproved.</summary>
        public virtual string GetSetting(string settingName)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.GetSetting({Name}) requires in-process ClassicUO access — not available in TMRazorImproved.", settingName);
            return string.Empty;
        }

        // ------------------------------------------------------------------
        // Container / Gump positioning
        // ------------------------------------------------------------------

        /// <summary>
        /// Sets the next-open position for the container with the given serial and then double-clicks it.
        /// Uses <see cref="IClientInteropService.NextContPosition"/> + item-use packet.
        /// </summary>
        public virtual void OpenContainerAt(uint serial, int x, int y)
        {
            _cancel.ThrowIfCancelled();
            _interop.NextContPosition(x, y);
            // Double-click the container (C2S 0x06)
            _packet.SendToServer(new byte[] { 0x06,
                (byte)(serial >> 24), (byte)(serial >> 16), (byte)(serial >> 8), (byte)serial });
        }

        /// <summary>int-serial overload.</summary>
        public virtual void OpenContainerAt(int serial, int x, int y) => OpenContainerAt((uint)serial, x, y);

        /// <summary>
        /// Saves the desired open-position for a gump. Requires in-process CUO UIManager — stub.
        /// </summary>
        public virtual void SetGumpOpenLocation(uint gumpSerial, int x, int y)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.SetGumpOpenLocation() requires in-process ClassicUO access — not available in TMRazorImproved.");
        }

        /// <summary>int-serial overload.</summary>
        public virtual void SetGumpOpenLocation(int gumpSerial, int x, int y) => SetGumpOpenLocation((uint)gumpSerial, x, y);

        /// <summary>Moves an open gump to new screen coordinates. Requires in-process CUO access — stub.</summary>
        public virtual void MoveGump(uint serial, int x, int y)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.MoveGump() requires in-process ClassicUO access — not available in TMRazorImproved.");
        }

        /// <summary>int-serial overload.</summary>
        public virtual void MoveGump(int serial, int x, int y) => MoveGump((uint)serial, x, y);

        /// <summary>
        /// Sends a S2C packet 0xBF/0x0004 to the client instructing it to close the specified gump.
        /// </summary>
        public virtual void CloseGump(uint gumpId)
        {
            _cancel.ThrowIfCancelled();
            // S2C packet 0xBF sub 0x0004 (CloseGump): [0xBF][len:2=0x0D][0x00,0x04][gumpId:4][button:4=0]
            byte[] data = new byte[13];
            data[0] = 0xBF;
            data[1] = 0x00; data[2] = 0x0D;      // length
            data[3] = 0x00; data[4] = 0x04;      // sub-command: CloseGump
            data[5] = (byte)(gumpId >> 24); data[6] = (byte)(gumpId >> 16);
            data[7] = (byte)(gumpId >> 8);  data[8] = (byte)gumpId;
            data[9] = 0x00; data[10] = 0x00; data[11] = 0x00; data[12] = 0x00; // button
            _packet.SendToClient(data);
        }

        /// <summary>int-serial overload.</summary>
        public virtual void CloseGump(int gumpId) => CloseGump((uint)gumpId);

        // ------------------------------------------------------------------
        // Status bars / Health bars
        // ------------------------------------------------------------------

        /// <summary>Closes the player's own status bar. Stub in TMRazorImproved.</summary>
        public virtual void CloseMyStatusBar()
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.CloseMyStatusBar() requires in-process ClassicUO access — not available in TMRazorImproved.");
        }

        /// <summary>Opens the player's own status bar at the given position. Stub in TMRazorImproved.</summary>
        public virtual void OpenMyStatusBar(int x = 0, int y = 0)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.OpenMyStatusBar() requires in-process ClassicUO access — not available in TMRazorImproved.");
        }

        /// <summary>Opens a health bar for the specified mobile. Stub in TMRazorImproved.</summary>
        public virtual void OpenMobileHealthBar(uint mobileSerial, int x = 0, int y = 0, bool custom = false)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.OpenMobileHealthBar() requires in-process ClassicUO access — not available in TMRazorImproved.");
        }

        /// <summary>int-serial overload.</summary>
        public virtual void OpenMobileHealthBar(int mobileSerial, int x = 0, int y = 0, bool custom = false)
            => OpenMobileHealthBar((uint)mobileSerial, x, y, custom);

        /// <summary>Closes the health bar for the specified mobile. Stub in TMRazorImproved.</summary>
        public virtual void CloseMobileHealthBar(uint mobileSerial)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.CloseMobileHealthBar() requires in-process ClassicUO access — not available in TMRazorImproved.");
        }

        /// <summary>int-serial overload.</summary>
        public virtual void CloseMobileHealthBar(int mobileSerial) => CloseMobileHealthBar((uint)mobileSerial);

        // ------------------------------------------------------------------
        // Macros
        // ------------------------------------------------------------------

        /// <summary>Runs a ClassicUO macro by name. Stub in TMRazorImproved.</summary>
        public virtual void PlayMacro(string macroName)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogWarning("CUO.PlayMacro({Name}) requires in-process ClassicUO access — not available in TMRazorImproved.", macroName);
        }

        // ------------------------------------------------------------------
        // Follow mobile
        // ------------------------------------------------------------------

        /// <summary>
        /// Activates client-side following of a mobile.
        /// Stores the serial; actual movement must be driven externally via pathfinding.
        /// </summary>
        public virtual void FollowMobile(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _followSerial = serial;
            _logger?.LogInformation("CUO.FollowMobile({Serial:X}) — follow state set. Movement must be driven by script/pathfinding.", serial);
        }

        /// <summary>int-serial overload.</summary>
        public virtual void FollowMobile(int serial) => FollowMobile((uint)serial);

        /// <summary>Deactivates mobile following.</summary>
        public virtual void FollowOff()
        {
            _cancel.ThrowIfCancelled();
            _followSerial = 0;
        }

        /// <summary>Returns the serial of the mobile currently being followed, or 0 if none.</summary>
        public virtual uint Following()
        {
            _cancel.ThrowIfCancelled();
            return _followSerial;
        }
    }
}
