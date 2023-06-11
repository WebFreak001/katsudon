using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Katsudon.Scoring
{
	public class TwoPlayerScoring
	{
		public ScoreProcessor Player1 { get; set; } = new KatsudonScoreProcessor();
		public ScoreProcessor Player2 { get; set; } = new KatsudonScoreProcessor();
	}
}