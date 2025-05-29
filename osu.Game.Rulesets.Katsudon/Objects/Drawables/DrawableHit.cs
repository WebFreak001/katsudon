// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Katsudon.Objects.Drawables
{
    public partial class DrawableHit : DrawableKatsudonStrongableHitObject<Hit, Hit.StrongNestedHit>
    {
        /// <summary>
        /// The action that caused this <see cref="DrawableHit"/> to be hit.
        /// </summary>
        public KatsudonAction? HitAction
        {
            get;
            private set;
        }

        private bool validActionPressed;

        private double?[] lastPressHandleTime = new double?[2];

        private readonly Bindable<Taiko.Objects.HitType> type = new();

        public DrawableHit()
            : this(null)
        {
        }

        public DrawableHit([CanBeNull] Hit hit)
            : base(hit)
        {
            FillMode = FillMode.Fit;
        }

        protected override void OnApply()
        {
            type.BindTo(HitObject.TypeBindable);
            // this doesn't need to be run inline as RecreatePieces is called by the base call below.
            type.BindValueChanged(_ => Scheduler.AddOnce(RecreatePieces));

            base.OnApply();
        }

        protected override void RecreatePieces()
        {
            base.RecreatePieces();
            Size = new Vector2(HitObject.IsStrong ? KatsudonStrongableHitObject.DEFAULT_STRONG_SIZE : KatsudonHitObject.DEFAULT_SIZE);
        }

        protected override void OnFree()
        {
            base.OnFree();

            type.UnbindFrom(HitObject.TypeBindable);
            type.UnbindEvents();

            UnproxyContent();

            HitAction = null;
            validActionPressed = false;
            lastPressHandleTime[0] = null;
            lastPressHandleTime[1] = null;
        }

        private bool isSuitableAction(KatsudonAction action)
        {
            if (HitObject.IsPlayer1)
            {
                if (HitObject.Type == Taiko.Objects.HitType.Centre)
                {
                    if (action is KatsudonAction.P1_LeftCentre or KatsudonAction.P1_RightCentre)
                        return true;
                }
                else
                {
                    if (action is KatsudonAction.P1_LeftRim or KatsudonAction.P1_RightRim)
                        return true;
                }
            }
            if (HitObject.IsPlayer2)
            {
                if (HitObject.Type == Taiko.Objects.HitType.Centre)
                {
                    if (action is KatsudonAction.P2_LeftCentre or KatsudonAction.P2_RightCentre)
                        return true;
                }
                else
                {
                    if (action is KatsudonAction.P2_LeftRim or KatsudonAction.P2_RightRim)
                        return true;
                }
            }
            return false;
        }

        protected override SkinnableDrawable CreateMainPiece() => HitObject.Type == Taiko.Objects.HitType.Centre
            ? new SkinnableDrawable(new KatsudonSkinComponentLookup(HitObject.PlayerId, KatsudonSkinComponents.CentreHit), _ => new CentreHitCirclePiece(), confineMode: ConfineMode.ScaleToFit)
            : new SkinnableDrawable(new KatsudonSkinComponentLookup(HitObject.PlayerId, KatsudonSkinComponents.RimHit), _ => new RimHitCirclePiece(), confineMode: ConfineMode.ScaleToFit);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyMinResult();
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            if (!validActionPressed)
                ApplyMinResult();
            else
                ApplyResult(result);
        }

        public override bool OnPressed(KeyBindingPressEvent<KatsudonAction> e)
        {
            int playerIdx = e.Action.GetPlayerNo();
            if (lastPressHandleTime[playerIdx] == Time.Current)
                return true;
            if (HitObject.PlayerId != -1 && e.Action.GetPlayerNo() != HitObject.PlayerId)
                return false;
            if (Judged)
                return false;

            validActionPressed = isSuitableAction(e.Action);

            // Only count this as handled if the new judgement is a hit
            bool result = UpdateResult(true);
            if (IsHit)
                HitAction = e.Action;

            // Regardless of whether we've hit or not, any secondary key presses in the same frame should be discarded
            // E.g. hitting a non-strong centre as a strong should not fall through and perform a hit on the next note
            lastPressHandleTime[playerIdx] = Time.Current;
            return result;
        }

        public override void OnReleased(KeyBindingReleaseEvent<KatsudonAction> e)
        {
            if (e.Action == HitAction)
                HitAction = null;
            base.OnReleased(e);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            Debug.Assert(HitObject.HitWindows != null);

            switch (state)
            {
                case ArmedState.Idle:
                    validActionPressed = false;

                    UnproxyContent();
                    break;

                case ArmedState.Miss:
                    this.FadeOut(100);
                    break;

                case ArmedState.Hit:
                    // If we're far enough away from the left stage, we should bring ourselves in front of it
                    ProxyContent();

                    const float gravity_time = 300;
                    const float gravity_travel_height = 200;

                    float y_multiplier = HitObject.IsAnyPlayer ? 0.0f : HitObject.IsPlayer1 ? 1.0f : -1.0f;

                    if (SnapJudgementLocation)
                        MainPiece.MoveToX(-X);

                    this.ScaleTo(0.8f, gravity_time * 2, Easing.OutQuad);

                    this.MoveToY(-gravity_travel_height * y_multiplier, gravity_time, Easing.Out)
                        .Then()
                        .MoveToY(gravity_travel_height * 2 * y_multiplier, gravity_time * 2, Easing.In);

                    this.FadeOut(800);
                    break;
            }
        }

        protected override DrawableStrongNestedHit CreateStrongNestedHit(Hit.StrongNestedHit hitObject) => new StrongNestedHit(hitObject);

        public partial class StrongNestedHit : DrawableStrongNestedHit
        {
            public new DrawableHit ParentHitObject => (DrawableHit)base.ParentHitObject;

            /// <summary>
            /// The lenience for the second key press.
            /// This does not adjust by map difficulty in ScoreV2 yet.
            /// </summary>
            public const double SECOND_HIT_WINDOW = 30;

            public StrongNestedHit()
                : this(null)
            {
            }

            public StrongNestedHit([CanBeNull] Hit.StrongNestedHit nestedHit)
                : base(nestedHit)
            {
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (!ParentHitObject.Result.HasResult)
                {
                    base.CheckForResult(userTriggered, timeOffset);
                    return;
                }

                if (!ParentHitObject.Result.IsHit)
                {
                    ApplyMinResult();
                    return;
                }

                if (!userTriggered)
                {
                    if (timeOffset - ParentHitObject.Result.TimeOffset > SECOND_HIT_WINDOW)
                        ApplyMinResult();
                    return;
                }

                if (Math.Abs(timeOffset - ParentHitObject.Result.TimeOffset) <= SECOND_HIT_WINDOW)
                    ApplyMaxResult();
            }

            public override bool OnPressed(KeyBindingPressEvent<KatsudonAction> e)
            {
                if (HitObject.PlayerId != -1 && e.Action.GetPlayerNo() != HitObject.PlayerId)
                    return false;

                // Don't process actions until the main hitobject is hit
                if (!ParentHitObject.IsHit)
                    return false;

                // Don't process actions if the pressed button was released
                if (ParentHitObject.HitAction == null)
                    return false;

                // Don't handle invalid hit action presses
                if (!ParentHitObject.isSuitableAction(e.Action))
                    return false;

                return UpdateResult(true);
            }
        }
    }
}
