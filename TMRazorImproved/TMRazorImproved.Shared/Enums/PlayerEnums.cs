namespace TMRazorImproved.Shared.Enums
{
    public enum Direction : byte
    {
        North = 0x00,
        Right = 0x01,
        East = 0x02,
        Down = 0x03,
        South = 0x04,
        Left = 0x05,
        West = 0x06,
        Up = 0x07,
        Running = 0x80
    }

    public enum LockType : byte
    {
        Up = 0,
        Down = 1,
        Locked = 2
    }
}
