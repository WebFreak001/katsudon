// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Katsudon.Objects;

namespace osu.Game.Rulesets.Katsudon.Scoring
{
    /// <summary>
    /// A <see cref="HealthProcessor"/> for the katsudon ruleset.
    /// Katsudon fails if the player has not half-filled their health by the end of the map.
    /// </summary>
    public partial class KatsudonHealthProcessor : AccumulatingHealthProcessor
    {
        /// <summary>
        /// A value used for calculating <see cref="hpMultiplier"/>.
        /// </summary>
        private const double object_count_factor = 3;

        /// <summary>
        /// HP multiplier for a successful <see cref="HitResult"/>.
        /// </summary>
        private double hpMultiplier;

        /// <summary>
        /// HP multiplier for a <see cref="HitResult"/> that does not satisfy <see cref="HitResultExtensions.IsHit"/>.
        /// </summary>
        private double hpMissMultiplier;

        public KatsudonHealthProcessor()
            : base(0.5)
        {
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            base.ApplyBeatmap(beatmap);

            hpMultiplier = 1 / (object_count_factor * Math.Max(1, beatmap.HitObjects.OfType<Hit>().Count() / 2) * IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0.5, 0.75, 0.98));
            hpMissMultiplier = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0.0018, 0.0075, 0.0120);
        }

        protected override double GetHealthIncreaseFor(JudgementResult result)
            => (result.HitObject is KatsudonHitObject kobj && !kobj.IsPlayer1) ? 0.0 : base.GetHealthIncreaseFor(result) * (result.IsHit ? hpMultiplier : hpMissMultiplier);
    }
}
