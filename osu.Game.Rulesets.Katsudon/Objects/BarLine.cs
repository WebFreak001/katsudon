// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Katsudon.Objects
{
    public class BarLine : KatsudonHitObject, IBarLine
    {
        private HitObjectProperty<bool> major;

        public Bindable<bool> MajorBindable => major.Bindable;

        public bool Major
        {
            get => major.Value;
            set => major.Value = value;
        }

        public override Judgement CreateJudgement() => new IgnoreJudgement();

        public static KatsudonHitObject Convert(Taiko.Objects.BarLine barline, int playerId)
        {
            return new BarLine
            {
                HitWindows = barline.HitWindows,
                LegacyBpmMultiplier = barline.LegacyBpmMultiplier,
                Samples = barline.Samples,
                StartTime = barline.StartTime,
                Major = barline.Major,
                PlayerId = playerId
            };
        }
    }
}
