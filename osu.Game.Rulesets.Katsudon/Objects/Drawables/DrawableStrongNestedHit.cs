// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Katsudon.Objects.Drawables
{
    /// <summary>
    /// Used as a nested hitobject to provide <see cref="TaikoStrongJudgement"/>s for <see cref="DrawableKatsudonStrongableHitObject{TObject,TStrongNestedObject}"/>s.
    /// </summary>
    public abstract partial class DrawableStrongNestedHit : DrawableKatsudonHitObject
    {
        public new DrawableKatsudonHitObject ParentHitObject => (DrawableKatsudonHitObject)base.ParentHitObject;

        protected DrawableStrongNestedHit([CanBeNull] StrongNestedHitObject nestedHit)
            : base(nestedHit)
        {
        }
    }
}
