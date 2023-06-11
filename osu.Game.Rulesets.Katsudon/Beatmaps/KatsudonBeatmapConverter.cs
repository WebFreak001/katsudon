using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Katsudon.Objects;
using osu.Framework.Logging;
using System.Reflection;

namespace osu.Game.Rulesets.Katsudon.Beatmaps
{
    public class KatsudonBeatmapConverter : BeatmapConverter<KatsudonHitObject>
    {
        private readonly IBeatmapConverter baseConverter;

        public KatsudonBeatmapConverter(IBeatmapConverter baseConverter, IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            this.baseConverter = baseConverter;
        }

        public override bool CanConvert() => Beatmap.BeatmapInfo.Ruleset.ShortName == Taiko.TaikoRuleset.SHORT_NAME
            || baseConverter.CanConvert();

        protected override IEnumerable<KatsudonHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var result = (IEnumerable<osu.Game.Rulesets.Taiko.Objects.TaikoHitObject>)
                baseConverter.GetType().GetMethod(nameof(ConvertHitObject),
                        System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(baseConverter,
                        new object[] { original, beatmap, cancellationToken }
                    )!;
            return result.SelectMany(obj => obj switch
            {
                // spinners are played by both players at once
                osu.Game.Rulesets.Taiko.Objects.Swell swell
                    => new KatsudonHitObject[] { Swell.Convert(swell) },
                osu.Game.Rulesets.Taiko.Objects.SwellTick swellTick
                    => new KatsudonHitObject[] { SwellTick.Convert(swellTick) },
                _ => new KatsudonHitObject[]
                {
                    KatsudonHitObject.Convert(obj, 0),
                    KatsudonHitObject.Convert(obj, 1)
                }
            });
        }
    }
}
