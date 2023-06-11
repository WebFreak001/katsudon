// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Katsudon.Objects
{
    /// <summary>
    /// Base type for nested strong hits.
    /// Used by <see cref="KatsudonStrongableHitObject"/>s to represent their strong bonus scoring portions.
    /// </summary>
    public abstract class StrongNestedHitObject : KatsudonHitObject
    {
        public KatsudonHitObject Parent { get; internal set; }

        protected StrongNestedHitObject(KatsudonHitObject parent)
        {
            Parent = parent;
        }

        public override Judgement CreateJudgement() => new TaikoStrongJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
