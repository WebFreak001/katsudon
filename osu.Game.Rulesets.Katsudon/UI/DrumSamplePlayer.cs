// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Katsudon.UI
{
    public partial class DrumSamplePlayer : CompositeDrawable, IKeyBindingHandler<KatsudonAction>
    {
        private readonly DrumSampleTriggerSource leftRimSampleTriggerSource;
        private readonly DrumSampleTriggerSource leftCentreSampleTriggerSource;
        private readonly DrumSampleTriggerSource rightCentreSampleTriggerSource;
        private readonly DrumSampleTriggerSource rightRimSampleTriggerSource;

        public DrumSamplePlayer(HitObjectContainer hitObjectContainer)
        {
            InternalChildren = new Drawable[]
            {
                leftRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
                leftCentreSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
                rightCentreSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
                rightRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
            };
        }

        public bool OnPressed(KeyBindingPressEvent<KatsudonAction> e)
        {
            switch (e.Action)
            {
                case KatsudonAction.P1_LeftRim:
                case KatsudonAction.P2_LeftRim:
                    leftRimSampleTriggerSource.Play(Taiko.Objects.HitType.Rim);
                    break;

                case KatsudonAction.P1_LeftCentre:
                case KatsudonAction.P2_LeftCentre:
                    leftCentreSampleTriggerSource.Play(Taiko.Objects.HitType.Centre);
                    break;

                case KatsudonAction.P1_RightCentre:
                case KatsudonAction.P2_RightCentre:
                    rightCentreSampleTriggerSource.Play(Taiko.Objects.HitType.Centre);
                    break;

                case KatsudonAction.P1_RightRim:
                case KatsudonAction.P2_RightRim:
                    rightRimSampleTriggerSource.Play(Taiko.Objects.HitType.Rim);
                    break;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<KatsudonAction> e)
        {
        }
    }
}
