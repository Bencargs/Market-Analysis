using ShellProgressBar;
using System;

namespace MarketAnalysis.Providers
{
    public class ProgressBarProvider
    {
        public static ProgressBar Create(int maxCount, string title)
        {
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true,
                BackgroundColor = ConsoleColor.DarkGreen
            };
            return new ProgressBar(maxCount, title, options);
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
