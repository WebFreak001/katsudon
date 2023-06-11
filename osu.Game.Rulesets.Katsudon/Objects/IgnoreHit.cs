// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
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
                LegacyBpmMultiplier = ignoreHit.LegacyBpmMultiplier,
                Samples = ignoreHit.Samples,
                StartTime = ignoreHit.StartTime,
                IsStrong = ignoreHit.IsStrong,
                PlayerId = playerId
            };
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
