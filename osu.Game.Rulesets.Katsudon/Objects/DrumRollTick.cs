// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Katsudon.Objects
{
    public class DrumRollTick : KatsudonStrongableHitObject
    {
        /// <summary>
        /// Whether this is the first (initial) tick of the slider.
        /// </summary>
        public bool FirstTick;

        /// <summary>
        /// The length (in milliseconds) between this tick and the next.
        /// <para>Half of this value is the hit window of the tick.</para>
        /// </summary>
        public double TickSpacing;

        /// <summary>
        /// The time allowed to hit this tick.
        /// </summary>
        public double HitWindow => TickSpacing / 2;

        public override Judgement CreateJudgement() => new TaikoDrumRollTickJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public override double MaximumJudgementOffset => HitWindow;

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit(this)
        {
            StartTime = startTime,
            Samples = Samples
        };

        public static KatsudonHitObject Convert(Taiko.Objects.DrumRollTick drumRollTick, int playerId)
        {
            return new DrumRollTick
            {
                HitWindows = drumRollTick.HitWindows,
                LegacyBpmMultiplier = drumRollTick.LegacyBpmMultiplier,
                Samples = drumRollTick.Samples,
                StartTime = drumRollTick.StartTime,
                IsStrong = drumRollTick.IsStrong,
                FirstTick = drumRollTick.FirstTick,
                TickSpacing = drumRollTick.TickSpacing,
                PlayerId = playerId
            };
        }

        public class StrongNestedHit : StrongNestedHitObject
        {
            public StrongNestedHit(KatsudonHitObject parent)
                : base(parent)
            {
            }
        }
    }
}
