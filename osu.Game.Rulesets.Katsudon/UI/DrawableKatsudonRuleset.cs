﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Katsudon.Objects;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Katsudon.UI
{
    public partial class DrawableKatsudonRuleset : DrawableScrollingRuleset<KatsudonHitObject>
    {
        public new BindableDouble TimeRange => base.TimeRange;

        public readonly BindableBool LockPlayfieldAspectRange = new BindableBool(true);

        public new KatsudonInputManager KeyBindingInputManager => (KatsudonInputManager)base.KeyBindingInputManager;

        protected override ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Overlapping;

        protected override bool UserScrollSpeedAdjustment => false;

        public override bool AllowGameplayOverlays => false;

        private SkinnableDrawable scroller;

        public DrawableKatsudonRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Left;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            new BarLineGenerator<BarLine>(Beatmap).BarLines.ForEach(bar => Playfield.Add(bar));

            FrameStableComponents.Add(scroller = new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.Scroller), _ => Empty())
            {
                RelativeSizeAxes = Axes.X,
                Depth = float.MaxValue
            });

            // KeyBindingInputManager.Add(new DrumTouchInputArea());
        }

        protected override void Update()
        {
            base.Update();

            // Katsudon scrolls at a constant 100px per 1000ms. More notes become visible as the playfield is lengthened.
            const float scroll_rate = 10;

            // Since the time range will depend on a positional value, it is referenced to the x480 pixel space.
            // Width is used because it defines how many notes fit on the playfield.
            // We clamp the ratio to the maximum aspect ratio to keep scroll speed consistent on widths lower than the default.
            float ratio = Math.Max(DrawSize.X / 768f, TaikoPlayfieldAdjustmentContainer.MAXIMUM_ASPECT);

            TimeRange.Value = (Playfield.HitObjectContainer.DrawWidth / ratio) * scroll_rate;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var playfieldScreen = Playfield.ScreenSpaceDrawQuad;
            scroller.Height = ToLocalSpace(playfieldScreen.TopLeft + new Vector2(0, playfieldScreen.Height / 20)).Y;
        }

        public MultiplierControlPoint ControlPointAt(double time)
        {
            int result = ControlPoints.BinarySearch(new MultiplierControlPoint(time));
            if (result < 0)
                result = Math.Clamp(~result - 1, 0, ControlPoints.Count);
            return ControlPoints[result];
        }

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new TaikoPlayfieldAdjustmentContainer
        {
            LockPlayfieldAspectRange = { BindTarget = LockPlayfieldAspectRange }
        };

        protected override PassThroughInputManager CreateInputManager() => new KatsudonInputManager(Ruleset.RulesetInfo);

        protected override Playfield CreatePlayfield() => new KatsudonPlayfield();

        public override DrawableHitObject<KatsudonHitObject> CreateDrawableRepresentation(KatsudonHitObject h) => null;

        // protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new KatsudonFramedReplayInputHandler(replay);

        // protected override ReplayRecorder CreateReplayRecorder(Score score) => new KatsudonReplayRecorder(score);
    }
}
