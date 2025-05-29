// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Katsudon.Objects.Drawables
{
    /// <summary>
    /// Used as a nested hitobject to provide <see cref="TaikoStrongJudgement"/>s for <see cref="DrawableKatsudonStrongableHitObject{TObject,TStrongNestedObject}"/>s.
    /// </summary>
    public abstract partial class DrawableStrongNestedHit : DrawableKatsudonHitObject
    {
        public new DrawableKatsudonHitObject? ParentHitObject => base.ParentHitObject as DrawableKatsudonHitObject;

        protected DrawableStrongNestedHit(StrongNestedHitObject? nestedHit)
            : base(nestedHit)
        {
        }

        public override void OnKilled()
        {
            base.OnKilled();

            // usually, the strong nested hit isn't judged itself, it is judged by its parent object.
            // however, in rare cases (see: drum rolls, hits with hidden active),
            // it can happen that the hit window of the nested strong hit extends past the lifetime of the parent object.
            // this is a safety to prevent such cases from causing the nested hit to never be judged and as such prevent gameplay from completing.
            if (!Judged && Time.Current > ParentHitObject?.HitObject.GetEndTime())
                ApplyMinResult();
        }
    }
}
