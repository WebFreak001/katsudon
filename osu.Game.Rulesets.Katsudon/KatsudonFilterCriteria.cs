using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Filter;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Rulesets.Katsudon
{
    public class KatsudonFilterCriteria : IRulesetFilterCriteria
    {
        public bool Matches(BeatmapInfo beatmapInfo, FilterCriteria criteria)
        {
            if (beatmapInfo.Ruleset.ShortName == Taiko.TaikoRuleset.SHORT_NAME ||
                beatmapInfo.Ruleset.ShortName == KatsudonRuleset.SHORT_NAME)
                return true;

            return criteria.AllowConvertedBeatmaps && beatmapInfo.Ruleset.ShortName == "osu";
        }

        public bool TryParseCustomKeywordCriteria(string key, Operator op, string value)
        {
            return false;
        }

        public bool FilterMayChangeFromMods(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            return false;
        }
    }
}
