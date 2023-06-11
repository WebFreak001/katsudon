// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Katsudon.Objects;

namespace osu.Game.Rulesets.Katsudon.Scoring
{
    public partial class KatsudonScoreProcessor : ScoreProcessor
    {
        private const double combo_base = 4;

        public KatsudonScoreProcessor()
            : base(new KatsudonRuleset())
        {
        }

        protected override double ComputeTotalScore(double comboProgress, double accuracyProgress, double bonusPortion)
        {
            return GetScoreProcessorStatistics().BaseScore;
        }
    }
}
