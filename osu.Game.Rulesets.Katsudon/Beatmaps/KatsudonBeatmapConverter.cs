using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Katsudon.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace osu.Game.Rulesets.Katsudon.Beatmaps
{
    public class KatsudonBeatmapConverter(IBeatmapConverter baseConverter, IBeatmap beatmap, Ruleset ruleset) : BeatmapConverter<KatsudonHitObject>(beatmap, ruleset)
    {
        /// <summary>
        /// A speed multiplier applied globally to osu!taiko.
        /// </summary>
        /// <remarks>
        /// osu! is generally slower than taiko, so a factor was historically added to increase speed for converts.
        /// This must be used everywhere slider length or beat length is used in taiko.
        ///
        /// Of note, this has never been exposed to the end user, and is considered a hidden internal multiplier.
        /// </remarks>
        public const float VELOCITY_MULTIPLIER = 1.4f;

        /// <summary>
        /// Because swells are easier in taiko than spinners are in osu!,
        /// legacy taiko multiplies a factor when converting the number of required hits.
        /// </summary>
        private const float swell_hit_multiplier = 1.65f;

        /// <summary>
        /// Base osu! slider scoring distance.
        /// </summary>
        private const float osu_base_scoring_distance = 100;

        private readonly IBeatmapConverter baseConverter = baseConverter;

        public override bool CanConvert() {
            return true;
        }

        protected override Beatmap<KatsudonHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            Beatmap<KatsudonHitObject> converted = base.ConvertBeatmap(original, cancellationToken);

            if (original.BeatmapInfo.Ruleset.OnlineID == 0)
            {
                // Post processing step to transform standard slider velocity changes into scroll speed changes
                double lastScrollSpeed = 1;

                foreach (HitObject hitObject in original.HitObjects)
                {
                    if (hitObject is not IHasSliderVelocity hasSliderVelocity) continue;

                    double nextScrollSpeed = hasSliderVelocity.SliderVelocityMultiplier;
                    EffectControlPoint currentEffectPoint = converted.ControlPointInfo.EffectPointAt(hitObject.StartTime);

                    if (!Precision.AlmostEquals(lastScrollSpeed, nextScrollSpeed, acceptableDifference: currentEffectPoint.ScrollSpeedBindable.Precision))
                    {
                        converted.ControlPointInfo.Add(hitObject.StartTime, new EffectControlPoint
                        {
                            KiaiMode = currentEffectPoint.KiaiMode,
                            ScrollSpeed = lastScrollSpeed = nextScrollSpeed,
                        });
                    }
                }
            }

            if (original.BeatmapInfo.Ruleset.OnlineID == 3)
            {
                // Post processing step to transform mania hit objects with the same start time into strong hits
                converted.HitObjects = converted.HitObjects.GroupBy(t => t.StartTime).Select(x =>
                {
                    KatsudonHitObject first = x.First();
                    if (x.Skip(1).Any() && first is KatsudonStrongableHitObject strong)
                        strong.IsStrong = true;
                    return first;
                }).ToList();
            }

            // TODO: stable makes the last tick of a drumroll non-required when the next object is too close.
            // This probably needs to be reimplemented:
            //
            // List<HitObject> hitobjects = hitObjectManager.hitObjects;
            // int ind = hitobjects.IndexOf(this);
            // if (i < hitobjects.Count - 1 && hitobjects[i + 1].HittableStartTime - (EndTime + (int)TickSpacing) <= (int)TickSpacing)
            //     lastTickHittable = false;

            return converted;
        }

        protected override IEnumerable<KatsudonHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var result = (IEnumerable<Taiko.Objects.TaikoHitObject>)
                baseConverter.GetType().GetMethod(nameof(ConvertHitObject),
                        BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(baseConverter, [original, beatmap, cancellationToken])!;
            return result.SelectMany(obj => obj switch
            {
                // spinners are played by both players at once
                Taiko.Objects.Swell swell => [Swell.Convert(swell)],
                Taiko.Objects.SwellTick swellTick => [SwellTick.Convert(swellTick)],
                _ => new KatsudonHitObject[]
                {
                    KatsudonHitObject.Convert(obj, 0),
                    KatsudonHitObject.Convert(obj, 1)
                }
            });
        }

        protected override Beatmap<KatsudonHitObject> CreateBeatmap() => new KatsudonBeatmap();
    }
}
