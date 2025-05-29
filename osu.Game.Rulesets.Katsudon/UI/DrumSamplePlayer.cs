// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.Katsudon.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Katsudon.UI
{
    internal partial class DrumSamplePlayer : CompositeDrawable, IKeyBindingHandler<KatsudonAction>
    {
        private DrumSampleTriggerSource leftCentreTrigger = null!;
        private DrumSampleTriggerSource rightCentreTrigger = null!;
        private DrumSampleTriggerSource leftRimTrigger = null!;
        private DrumSampleTriggerSource rightRimTrigger = null!;
        private DrumSampleTriggerSource strongCentreTrigger = null!;
        private DrumSampleTriggerSource strongRimTrigger = null!;

        private double[] lastHitTime = new double[2];
        private KatsudonAction?[] lastAction = new KatsudonAction?[2];

        [BackgroundDependencyLoader]
        private void load(Playfield playfield)
        {
            var hitObjectContainer = playfield.HitObjectContainer;
            InternalChildren = new Drawable[]
            {
                leftCentreTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightCentreTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Right),
                leftRimTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightRimTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Right),
                strongCentreTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Centre),
                strongRimTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Centre)
            };
        }

        protected virtual DrumSampleTriggerSource CreateTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance)
            => new DrumSampleTriggerSource(hitObjectContainer);

        public bool OnPressed(KeyBindingPressEvent<KatsudonAction> e)
        {
            if ((Clock as IGameplayClock)?.IsRewinding == true)
                return false;

            Taiko.Objects.HitType hitType;

            DrumSampleTriggerSource triggerSource;

            int no = e.Action.GetPlayerNo();

            bool strong = checkStrongValidity(e.Action, lastAction[no], Time.Current - lastHitTime[no]);

            switch (e.Action)
            {
                case KatsudonAction.P1_LeftCentre:
                case KatsudonAction.P2_LeftCentre:
                    hitType = Taiko.Objects.HitType.Centre;
                    triggerSource = strong ? strongCentreTrigger : leftCentreTrigger;
                    break;

                case KatsudonAction.P1_RightCentre:
                case KatsudonAction.P2_RightCentre:
                    hitType = Taiko.Objects.HitType.Centre;
                    triggerSource = strong ? strongCentreTrigger : rightCentreTrigger;
                    break;

                case KatsudonAction.P1_LeftRim:
                case KatsudonAction.P2_LeftRim:
                    hitType = Taiko.Objects.HitType.Rim;
                    triggerSource = strong ? strongRimTrigger : leftRimTrigger;
                    break;

                case KatsudonAction.P1_RightRim:
                case KatsudonAction.P2_RightRim:
                    hitType = Taiko.Objects.HitType.Rim;
                    triggerSource = strong ? strongRimTrigger : rightRimTrigger;
                    break;

                default:
                    return false;
            }

            if (strong)
            {
                switch (hitType)
                {
                    case Taiko.Objects.HitType.Centre:
                        flushCenterTriggerSources();
                        break;

                    case Taiko.Objects.HitType.Rim:
                        flushRimTriggerSources();
                        break;
                }
            }

            Play(triggerSource, hitType, strong);

            lastHitTime[no] = Time.Current;
            lastAction[no] = e.Action;

            return false;
        }

        protected virtual void Play(DrumSampleTriggerSource triggerSource, Taiko.Objects.HitType hitType, bool strong) =>
            triggerSource.Play(hitType, strong);

        private bool checkStrongValidity(KatsudonAction newAction, KatsudonAction? lastAction, double timeBetweenActions)
        {
            if (lastAction == null)
                return false;

            if (timeBetweenActions < 0 || timeBetweenActions > DrawableHit.StrongNestedHit.SECOND_HIT_WINDOW)
                return false;

            switch (newAction)
            {
                case KatsudonAction.P1_LeftCentre:
                    return lastAction == KatsudonAction.P1_RightCentre;
                case KatsudonAction.P2_LeftCentre:
                    return lastAction == KatsudonAction.P2_RightCentre;

                case KatsudonAction.P1_RightCentre:
                    return lastAction == KatsudonAction.P1_LeftCentre;
                case KatsudonAction.P2_RightCentre:
                    return lastAction == KatsudonAction.P2_LeftCentre;

                case KatsudonAction.P1_LeftRim:
                    return lastAction == KatsudonAction.P1_RightRim;
                case KatsudonAction.P2_LeftRim:
                    return lastAction == KatsudonAction.P2_RightRim;

                case KatsudonAction.P1_RightRim:
                    return lastAction == KatsudonAction.P1_LeftRim;
                case KatsudonAction.P2_RightRim:
                    return lastAction == KatsudonAction.P2_LeftRim;

                default:
                    return false;
            }
        }

        private void flushCenterTriggerSources()
        {
            leftCentreTrigger.StopAllPlayback();
            rightCentreTrigger.StopAllPlayback();
            strongCentreTrigger.StopAllPlayback();
        }

        private void flushRimTriggerSources()
        {
            leftRimTrigger.StopAllPlayback();
            rightRimTrigger.StopAllPlayback();
            strongRimTrigger.StopAllPlayback();
        }

        public void OnReleased(KeyBindingReleaseEvent<KatsudonAction> e)
        {
        }
    }
}
