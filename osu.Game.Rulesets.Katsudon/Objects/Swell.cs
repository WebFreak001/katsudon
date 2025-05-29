// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Katsudon.Objects
{
    public class Swell : KatsudonHitObject, IHasDuration
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        /// <summary>
        /// The number of hits required to complete the swell successfully.
        /// </summary>
        public int RequiredHits = 10;

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            for (int i = 0; i < RequiredHits; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AddNested(new SwellTick
                {
                    StartTime = StartTime,
                    Samples = Samples
                });
            }
        }

        public override Judgement CreateJudgement() => new TaikoSwellJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public static KatsudonHitObject Convert(Taiko.Objects.Swell swell)
        {
            return new Swell
            {
                HitWindows = swell.HitWindows,
                Samples = swell.Samples,
                StartTime = swell.StartTime,
                Duration = swell.Duration,
                RequiredHits = (int)Math.Round(swell.RequiredHits * 2.5),
                PlayerId = -1
            };
        }
    }
}
