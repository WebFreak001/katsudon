﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    }
}
