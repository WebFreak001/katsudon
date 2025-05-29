// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Katsudon.Objects
{
    public class SwellTick : KatsudonHitObject
    {
        public static KatsudonHitObject Convert(Taiko.Objects.SwellTick swellTick)
        {
            return new SwellTick
            {
                HitWindows = swellTick.HitWindows,
                Samples = swellTick.Samples,
                StartTime = swellTick.StartTime,
                PlayerId = -1
            };
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
