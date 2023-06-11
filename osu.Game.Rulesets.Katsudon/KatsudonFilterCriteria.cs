using osu.Game.Beatmaps;
using osu.Game.Rulesets.Filter;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Rulesets.Katsudon
{
    public class KatsudonFilterCriteria : IRulesetFilterCriteria
    {
        public bool Matches(BeatmapInfo beatmapInfo)
        {
            return beatmapInfo.Ruleset.ShortName == Taiko.TaikoRuleset.SHORT_NAME
                || beatmapInfo.Ruleset.ShortName == KatsudonRuleset.SHORT_NAME;
        }

        public bool TryParseCustomKeywordCriteria(string key, Operator op, string value)
        {
            return false;
        }
    }
}