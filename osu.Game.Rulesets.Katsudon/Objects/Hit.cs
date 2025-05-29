// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Katsudon.Objects
{
    public class Hit : KatsudonStrongableHitObject, IHasDisplayColour
    {
        private HitObjectProperty<Taiko.Objects.HitType> type;

        public Bindable<Taiko.Objects.HitType> TypeBindable => type.Bindable;

        /// <summary>
        /// The <see cref="HitType"/> that actuates this <see cref="Hit"/>.
        /// </summary>
        public Taiko.Objects.HitType Type
        {
            get => type.Value;
            set => type.Value = value;
        }

        public Bindable<Color4> DisplayColour { get; } = new Bindable<Color4>(COLOUR_CENTRE);

        public static readonly Color4 COLOUR_CENTRE = Color4Extensions.FromHex(@"bb1177");
        public static readonly Color4 COLOUR_RIM = Color4Extensions.FromHex(@"2299bb");

        public Hit()
        {
            TypeBindable.BindValueChanged(_ =>
            {
                updateSamplesFromType();
                DisplayColour.Value = Type == Taiko.Objects.HitType.Centre ? COLOUR_CENTRE : COLOUR_RIM;
            });

            SamplesBindable.BindCollectionChanged((_, _) => updateTypeFromSamples());
        }

        private void updateTypeFromSamples()
        {
            Type = getRimSamples().Any() ? Taiko.Objects.HitType.Rim : Taiko.Objects.HitType.Centre;
        }

        /// <summary>
        /// Returns an array of any samples which would cause this object to be a "rim" type hit.
        /// </summary>
        private HitSampleInfo[] getRimSamples() => Samples.Where(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE).ToArray();

        private void updateSamplesFromType()
        {
            var rimSamples = getRimSamples();

            bool isRimType = Type == Taiko.Objects.HitType.Rim;

            if (isRimType != rimSamples.Any())
            {
                if (isRimType)
                    Samples.Add(CreateHitSampleInfo(HitSampleInfo.HIT_CLAP));
                else
                {
                    foreach (var sample in rimSamples)
                        Samples.Remove(sample);
                }
            }
        }

        protected override StrongNestedHitObject CreateStrongNestedHit(double startTime) => new StrongNestedHit(this)
        {
            StartTime = startTime,
            Samples = Samples
        };

        public static KatsudonHitObject Convert(Taiko.Objects.Hit hit, int playerId)
        {
            return new Hit
            {
                HitWindows = hit.HitWindows,
                Samples = hit.Samples,
                StartTime = hit.StartTime,
                IsStrong = hit.IsStrong,
                Type = hit.Type,
                PlayerId = playerId
            };
        }

        public static Taiko.Objects.TaikoHitObject Unconvert(Hit hit)
        {
            return new Taiko.Objects.Hit
            {
                HitWindows = hit.HitWindows,
                Samples = hit.Samples,
                StartTime = hit.StartTime,
                IsStrong = hit.IsStrong,
                Type = hit.Type
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
