// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Katsudon.Beatmaps;
using osuTK;

namespace osu.Game.Rulesets.Katsudon.Objects
{
    public class DrumRoll : KatsudonStrongableHitObject, IHasPath
    {
        /// <summary>
        /// Drum roll distance that results in a duration of 1 speed-adjusted beat length.
        /// </summary>
        private const float base_distance = 100;

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        /// <summary>
        /// Velocity of this <see cref="DrumRoll"/>.
        /// </summary>
        public double Velocity { get; private set; }

        /// <summary>
        /// Numer of ticks per beat length.
        /// </summary>
        public int TickRate = 1;

        /// <summary>
        /// The length (in milliseconds) between ticks of this drumroll.
        /// <para>Half of this value is the hit window of the ticks.</para>
        /// </summary>
        private double tickSpacing = 100;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            EffectControlPoint effectPoint = controlPointInfo.EffectPointAt(StartTime);

            double scoringDistance = base_distance * (difficulty.SliderMultiplier * KatsudonBeatmapConverter.VELOCITY_MULTIPLIER) * effectPoint.ScrollSpeed;
            Velocity = scoringDistance / timingPoint.BeatLength;

            TickRate = difficulty.SliderTickRate == 3 ? 3 : 4;

            tickSpacing = timingPoint.BeatLength / TickRate;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            createTicks(cancellationToken);

            base.CreateNestedHitObjects(cancellationToken);
        }

        private void createTicks(CancellationToken cancellationToken)
        {
            if (tickSpacing == 0)
                return;

            bool first = true;

            for (double t = StartTime; t < EndTime + tickSpacing / 2; t += tickSpacing)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AddNested(new DrumRollTick(this)
                {
                    FirstTick = first,
                    TickSpacing = tickSpacing,
                    StartTime = t,
                    IsStrong = IsStrong,
                    Samples = Samples,
                    PlayerId = PlayerId
                });

                first = false;
            }
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit(this)
        {
            StartTime = startTime,
            Samples = Samples
        };

        public static KatsudonHitObject Convert(Taiko.Objects.DrumRoll drumRoll, int playerId)
        {
            var ret = new DrumRoll
            {
                HitWindows = drumRoll.HitWindows,
                Samples = drumRoll.Samples,
                StartTime = drumRoll.StartTime,
                Duration = drumRoll.Duration,
                Velocity = drumRoll.Velocity,
                TickRate = drumRoll.TickRate,
                IsStrong = drumRoll.IsStrong,
                PlayerId = playerId
            };

            foreach (var child in drumRoll.NestedHitObjects)
                if (child is osu.Game.Rulesets.Taiko.Objects.TaikoHitObject taikoChild)
                {
                    var convChild = KatsudonHitObject.Convert(taikoChild, playerId);
                    ret.AddNested(convChild);
                }
            return ret;
        }

        public static Taiko.Objects.TaikoHitObject Unconvert(DrumRoll drumRoll)
        {
            var ret = new Taiko.Objects.DrumRoll
            {
                HitWindows = drumRoll.HitWindows,
                Samples = drumRoll.Samples,
                StartTime = drumRoll.StartTime,
                Duration = drumRoll.Duration,
                TickRate = drumRoll.TickRate,
                IsStrong = drumRoll.IsStrong,
            };

            return ret;
        }

        public class StrongNestedHit : StrongNestedHitObject
        {
            // The strong hit of the drum roll doesn't actually provide any score.
            public override Judgement CreateJudgement() => new IgnoreJudgement();

            public StrongNestedHit(KatsudonHitObject parent)
                : base(parent)
            {
            }
        }

        #region LegacyBeatmapEncoder

        double IHasDistance.Distance => Duration * Velocity;

        SliderPath IHasPath.Path
            => new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(1) }, ((IHasDistance)this).Distance / KatsudonBeatmapConverter.VELOCITY_MULTIPLIER);

        #endregion
    }
}
