namespace RetailPulse.Models.ViewModels
{
    /// <summary>
    /// A single automatically-generated business insight sentence, plus a
    /// direction used purely for styling (up/down/neutral icon and colour).
    
    public class InsightViewModel
    {
        public string Text { get; set; } = string.Empty;

        public InsightDirection Direction { get; set; } = InsightDirection.Neutral;
    }

    public enum InsightDirection
    {
        Neutral,
        Positive,
        Negative
    }
}
