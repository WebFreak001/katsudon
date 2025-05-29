// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Katsudon.Objects
{
    public class IgnoreHit : Hit
    {
        public static KatsudonHitObject Convert(Taiko.Objects.IgnoreHit ignoreHit, int playerId)
        {
            return new IgnoreHit
            {
                HitWindows = ignoreHit.HitWindows,
                Samples = ignoreHit.Samples,
                StartTime = ignoreHit.StartTime,
                IsStrong = ignoreHit.IsStrong,
                PlayerId = playerId
            };
        }

        public static Taiko.Objects.IgnoreHit Unconvert(IgnoreHit ignoreHit, int playerId)
        {
            return new Taiko.Objects.IgnoreHit
            {
                HitWindows = ignoreHit.HitWindows,
                Samples = ignoreHit.Samples,
                StartTime = ignoreHit.StartTime,
                IsStrong = ignoreHit.IsStrong
            };
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
