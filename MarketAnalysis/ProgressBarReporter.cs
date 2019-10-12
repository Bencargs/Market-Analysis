using ShellProgressBar;

namespace MarketAnalysis
{
    public static class ProgressBarReporter
    {
        private static ProgressBar parentProgressBar;

        public static ProgressBar StartProgressBar(int maxCount, string title)
        {
            parentProgressBar?.Dispose();
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true,
                BackgroundColor = System.ConsoleColor.DarkGreen
            };
            parentProgressBar = new ProgressBar(maxCount, title, options);
            return parentProgressBar;
        }

        public static ChildProgressBar SpawnChild(int maxCount, string title)
        {
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = false,
                CollapseWhenFinished = true,
                DisplayTimeInRealTime = false,
                BackgroundColor = System.ConsoleColor.DarkGreen
            };
            return parentProgressBar?.Spawn(maxCount, title, options);
        }
    }
}
