using System;

namespace MarketAnalysis
{
    public static class UnitTestDetector
    {
        public static bool IsRunningFromNUnit { get; }

        static UnitTestDetector()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Can't do something like this as it will load the nUnit assembly
                // if (assembly == typeof(NUnit.Framework.Assert))
                if (assembly.FullName == null ||
                    !assembly.FullName.ToLowerInvariant().StartsWith("nunit.framework"))
                    continue;

                IsRunningFromNUnit = true;
                break;
            }
        }
    }
}
