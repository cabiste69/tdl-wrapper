using System.Runtime.InteropServices;

namespace tdlWrapper;

public sealed class Arch
{
    public static string GetArchString(Architecture arch)
    {
        switch (arch)
        {
            case Architecture.X86: return "32bit";
            case Architecture.X64: return "64bit";
            case Architecture.Arm: return "armv5";
            case Architecture.Armv6: return "armv6";
            case Architecture.Arm64: return "arm64";
            // idk man, are you on legv4?
            default:
                throw new Exception("Unknown OS Architecture!");
        }
    }
}