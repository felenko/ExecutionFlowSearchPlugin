// Guids.cs
// MUST match guids.h
using System;

namespace Company.VSPackage2
{
    static class GuidList
    {
        public const string guidVSPackage2PkgString = "de1490ee-6fa5-4f72-bcfa-e97d7f4ee032";
        public const string guidVSPackage2CmdSetString = "7e2b51c6-c78d-4f80-beef-7fb6046396c5";
        public const string guidToolWindowPersistanceString = "e4629de2-bb02-488b-9f02-a90a8402bea5";

        public static readonly Guid guidVSPackage2CmdSet = new Guid(guidVSPackage2CmdSetString);
    };
}