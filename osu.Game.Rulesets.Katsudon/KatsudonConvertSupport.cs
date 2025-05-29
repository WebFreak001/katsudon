using osu.Game.Rulesets.Filter;

namespace osu.Game.Rulesets.Katsudon
{
    public class KatsudonConvertSupport : IRulesetConvertSupport
    {
        public bool CanBePlayed(RulesetInfo ruleset, bool conversionEnabled)
        {
            return ruleset.ShortName == "katsudon" || ruleset.ShortName == "taiko" ||
                   (conversionEnabled && ruleset.ShortName == "osu");
        }
    }
}
