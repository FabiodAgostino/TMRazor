using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TMRazorImproved.Shared.Messages
{
    /// <summary>Inviato quando il server richiede un target (0x6C S2C).</summary>
    public class TargetCursorMessage : ValueChangedMessage<(uint CursorId, byte TargetType, byte CursorType)>
    {
        /// <param name="cursorId">ID del cursore (da includere nella risposta).</param>
        /// <param name="targetType">0 = oggetto/mobile, 1 = tile terreno.</param>
        /// <param name="cursorType">0 = neutro, 1 = offensivo, 2 = benefico.</param>
        public TargetCursorMessage(uint cursorId, byte targetType, byte cursorType)
            : base((cursorId, targetType, cursorType)) { }
    }

    /// <summary>Inviato quando un mobile si muove (0x77).</summary>
    public class MobileMovingMessage : ValueChangedMessage<(uint Serial, ushort X, ushort Y, sbyte Z, byte Direction)>
    {
        public MobileMovingMessage(uint serial, ushort x, ushort y, sbyte z, byte direction)
            : base((serial, x, y, z, direction)) { }
    }

    /// <summary>Inviato quando viene riprodotto un effetto grafico / animazione (0xC0).</summary>
    public class GraphicalEffectMessage : ValueChangedMessage<(byte Type, uint Source, uint Target, ushort ItemId, ushort SrcX, ushort SrcY, sbyte SrcZ, ushort TgtX, ushort TgtY, sbyte TgtZ)>
    {
        public GraphicalEffectMessage(byte type, uint source, uint target, ushort itemId,
            ushort srcX, ushort srcY, sbyte srcZ, ushort tgtX, ushort tgtY, sbyte tgtZ)
            : base((type, source, target, itemId, srcX, srcY, srcZ, tgtX, tgtY, tgtZ)) { }
    }

    /// <summary>Inviato quando un buff/debuff viene aggiunto o rimosso (0xDF).</summary>
    public class BuffDebuffMessage : ValueChangedMessage<(uint Serial, ushort BuffType, bool Added)>
    {
        public BuffDebuffMessage(uint serial, ushort buffType, bool added)
            : base((serial, buffType, added)) { }
    }

    /// <summary>Inviato quando la traccia musicale cambia (0x6D).</summary>
    public class MusicMessage : ValueChangedMessage<ushort>
    {
        public MusicMessage(ushort musicId) : base(musicId) { }
    }
}
