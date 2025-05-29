// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
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
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
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

        protected new KatsudonPlayfieldAdjustmentContainer PlayfieldAdjustmentContainer => (KatsudonPlayfieldAdjustmentContainer)base.PlayfieldAdjustmentContainer;

        protected override bool UserScrollSpeedAdjustment => false;

        private SkinnableDrawable? scroller;

        public DrawableKatsudonRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Left;
            VisualisationMethod = ScrollVisualisationMethod.Overlapping;
        }

        [BackgroundDependencyLoader(true)]
        private void load(GameplayState? gameplayState)
        {
            new BarLineGenerator<BarLine>(Beatmap).BarLines.ForEach(bar => Playfield.Add(bar));

            var spriteElements = gameplayState?.Storyboard.Layers.Where(l => l.Name != @"Overlay")
                                              .SelectMany(l => l.Elements)
                                              .OfType<StoryboardSprite>()
                                              .DistinctBy(e => e.Path) ?? Enumerable.Empty<StoryboardSprite>();

            if (spriteElements.Count() < 10)
            {
                FrameStableComponents.Add(scroller = new SkinnableDrawable(new KatsudonSkinComponentLookup(-1, KatsudonSkinComponents.Scroller), _ => Empty())
                {
                    RelativeSizeAxes = Axes.X,
                    Depth = float.MaxValue,
                });
            }
        }

        protected override void Update()
        {
            base.Update();

            TimeRange.Value = ComputeTimeRange();
        }

        protected virtual double ComputeTimeRange()
        {
            // Using the constant algorithm results in a sluggish scroll speed that's equal to 60 BPM.
            // We need to adjust it to the expected default scroll speed (BPM * base SV multiplier).
            double multiplier = VisualisationMethod == ScrollVisualisationMethod.Constant
                ? (Beatmap.BeatmapInfo.BPM * Beatmap.Difficulty.SliderMultiplier) / 60
                : 1;
            return PlayfieldAdjustmentContainer.ComputeTimeRange() / multiplier;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var playfieldScreen = Playfield.ScreenSpaceDrawQuad;

            if (scroller != null)
                scroller.Height = ToLocalSpace(playfieldScreen.TopLeft + new Vector2(0, playfieldScreen.Height / 20)).Y;
        }

        public MultiplierControlPoint ControlPointAt(double time)
        {
            int result = ControlPoints.BinarySearch(new MultiplierControlPoint(time));
            if (result < 0)
                result = Math.Clamp(~result - 1, 0, ControlPoints.Count);
            return ControlPoints[result];
        }

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new KatsudonPlayfieldAdjustmentContainer
        {
            LockPlayfieldAspectRange = { BindTarget = LockPlayfieldAspectRange }
        };

        protected override PassThroughInputManager CreateInputManager() => new KatsudonInputManager(Ruleset.RulesetInfo);

        protected override Playfield CreatePlayfield() => new KatsudonPlayfield();

        public override DrawableHitObject<KatsudonHitObject> CreateDrawableRepresentation(KatsudonHitObject h) => null;

        // protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new KatsudonFramedReplayInputHandler(replay);

        // protected override ReplayRecorder CreateReplayRecorder(Score score) => new KatsudonReplayRecorder(score);

        protected override ResumeOverlay CreateResumeOverlay() => new DelayedResumeOverlay();
    }
}
