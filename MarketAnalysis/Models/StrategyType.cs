using System.ComponentModel;

namespace MarketAnalysis.Models
{
    public enum StrategyType
    {
        [Description("Specific Days")]
        StaticDates = 1,

        [Description("Entropy")]
        Entropy,

        [Description("Pattern Recognition")]
        PatternRecognition,

        [Description("Relative Strength")]
        RelativeStrength,

        [Description("Delta")]
        Delta,

        [Description("Gradient")]
        Gradient,

        [Description("Linear Regression")]
        LinearRegression,

        [Description("Volume")]
        Volume,

        [Description("Weighted")]
        Weighted,

        [Description("Clustering")]
        Cluster,

        [Description("Moving Average")]
        MovingAverage,
        
        [Description("Holiday Effect")]
        HolidayEffect,
        
        [Description("Optimal Stopping")]
        OptimalStopping,

        [Description("Probability")]
        Probability,
        
        [Description("Spread")]
        Spread,
    }
}
