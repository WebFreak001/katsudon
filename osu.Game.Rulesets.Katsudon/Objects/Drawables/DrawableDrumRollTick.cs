// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Katsudon.Skinning.Default;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Katsudon.Objects.Drawables
{
    public partial class DrawableDrumRollTick : DrawableKatsudonStrongableHitObject<DrumRollTick, DrumRollTick.StrongNestedHit>
    {
        public BindableBool IsFirstTick = new BindableBool();

        /// <summary>
        /// The hit type corresponding to the <see cref="KatsudonAction"/> that the user pressed to hit this <see cref="DrawableDrumRollTick"/>.
        /// </summary>
        public Taiko.Objects.HitType JudgementType;

        public DrawableDrumRollTick()
            : this(null)
        {
        }

        public DrawableDrumRollTick([CanBeNull] DrumRollTick tick)
            : base(tick)
        {
            FillMode = FillMode.Fit;
        }

        protected override SkinnableDrawable CreateMainPiece() => new SkinnableDrawable(new KatsudonSkinComponentLookup(HitObject.PlayerId, KatsudonSkinComponents.DrumRollTick), _ => new TickPiece());

        protected override void OnApply()
        {
            base.OnApply();

            IsFirstTick.Value = HitObject.FirstTick;
        }

        protected override void RecreatePieces()
        {
            base.RecreatePieces();
            Size = new Vector2(HitObject.IsStrong ? KatsudonStrongableHitObject.DEFAULT_STRONG_SIZE : KatsudonHitObject.DEFAULT_SIZE);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (timeOffset > HitObject.HitWindow)
                    ApplyMinResult();
                return;
            }

            if (Math.Abs(timeOffset) > HitObject.HitWindow)
                return;

            ApplyMaxResult();
        }

        public override void OnKilled()
        {
            base.OnKilled();

            if (Time.Current > HitObject.GetEndTime() && !Judged)
                ApplyMinResult();
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    this.ScaleTo(1.4f, 200, Easing.OutQuint);
                    this.FadeOut(200, Easing.OutQuint);
                    break;
            }
        }

        public override bool OnPressed(KeyBindingPressEvent<KatsudonAction> e)
        {
            if (HitObject.PlayerId != -1 && e.Action.GetPlayerNo() != HitObject.PlayerId)
                return false;

            JudgementType =
                   e.Action == KatsudonAction.P1_LeftRim || e.Action == KatsudonAction.P1_RightRim
                || e.Action == KatsudonAction.P2_LeftRim || e.Action == KatsudonAction.P2_RightRim
                 ? Taiko.Objects.HitType.Rim
                 : Taiko.Objects.HitType.Centre;
            return UpdateResult(true);
        }

        protected override DrawableStrongNestedHit CreateStrongNestedHit(DrumRollTick.StrongNestedHit hitObject) => new StrongNestedHit(hitObject);

        public partial class StrongNestedHit : DrawableStrongNestedHit
        {
            public new DrawableDrumRollTick ParentHitObject => (DrawableDrumRollTick)base.ParentHitObject;

            public StrongNestedHit()
                : this(null)
            {
            }

            public StrongNestedHit([CanBeNull] DrumRollTick.StrongNestedHit nestedHit)
                : base(nestedHit)
            {
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (!ParentHitObject.Judged)
                    return;

                ApplyResult(static (r, hitObject) =>
                {
                    var nestedHit = (StrongNestedHit)hitObject;
                    r.Type = nestedHit.ParentHitObject!.IsHit ? r.Judgement.MaxResult : r.Judgement.MinResult;
                });
            }

            public override bool OnPressed(KeyBindingPressEvent<KatsudonAction> e) => false;
        }
    }
}
