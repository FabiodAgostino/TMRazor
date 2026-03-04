using System;

namespace TMRazorImproved.Shared.Interfaces
{
    public enum UOPatchType
    {
        FPS = 1,
        Stamina,
        AlwaysLight,
        PaperdollSlots,
        SplashScreen,
        Resolution,
        OptionsNotification,
        MultiUO,
        NoCrypt,
        GlobalSound,
        ViewRange,
        Count
    }

    public interface IUOModService
    {
        void InjectUoMod(int pid);
        void EnablePatch(UOPatchType patch, bool enable);
    }
}
