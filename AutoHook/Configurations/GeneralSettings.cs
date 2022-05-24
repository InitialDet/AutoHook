using System;

namespace AutoHook.Configurations
{
    public class HookSettings
    {

        public bool Enabled = true;

        public string BaitName = "Default";

        public bool HookWeak = true;
        public bool HookStrong = true;
        public bool HookLendary = true;

        public double MaxTimeDelay = 0;

        public HookSettings(string bait)
        {
            BaitName = bait;
        }

        public override bool Equals(object? obj)
        {
            return obj is HookSettings settings &&
                   BaitName == settings.BaitName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BaitName + "a");
        }
    }
}
