using ShellProgressBar;
using System;
using System.Diagnostics;
using System.IO;

namespace MarketAnalysis.Providers
{
    public class ProgressBarProvider
    {
        [DebuggerStepThrough]
        public static ProgressBar Create(int maxCount, string title)
        {
            // Bugfix where ProgressBar throws an exception when run in a test setting with no console window
            if (Console.IsInputRedirected)
                return null;

            try
            {
                var options = new ProgressBarOptions
                {
                    ProgressCharacter = '─',
                    ProgressBarOnBottom = true,
                    BackgroundColor = ConsoleColor.DarkGreen
                };
                return new ProgressBar(maxCount, title, options);
            }
            catch (Exception)
            {
                // Though the bug persist when debugging a test
                return null;
            }
        }

        public static ChildProgressBar Create(ProgressBar parent, int maxCount, string title)
        {
            var options = new ProgressBarOptions
            {
                ProgressBarOnBottom = true,
                CollapseWhenFinished = false,
                DisplayTimeInRealTime = false,
                BackgroundColor = ConsoleColor.DarkGreen
            };
            return parent?.Spawn(maxCount, title, options);
        }

        public ChildProgressBar Create(ChildProgressBar child, int maxCount, string title)
        {
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = false,
                CollapseWhenFinished = true,
                DisplayTimeInRealTime = false,
                BackgroundColor = ConsoleColor.DarkGreen
            };
            return child?.Spawn(maxCount, title, options);
        }
    }
}
