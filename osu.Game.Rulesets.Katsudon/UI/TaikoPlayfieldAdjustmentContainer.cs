// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Katsudon.Beatmaps;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Katsudon.UI
{
    public partial class KatsudonPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        public const float MAXIMUM_ASPECT = 16f / 9f;
        public const float MINIMUM_ASPECT = 5f / 4f;

        private const float stable_gamefield_height = 480f;

        public readonly IBindable<bool> LockPlayfieldAspectRange = new BindableBool(true);

        [Resolved]
        private OsuGame? osuGame { get; set; }

        public KatsudonPlayfieldAdjustmentContainer()
        {
            RelativeSizeAxes = Axes.X;
            RelativePositionAxes = Axes.Y;
            Height = KatsudonPlayfield.DEFAULT_HEIGHT / 2;

            // Matches stable, see https://github.com/peppy/osu-stable-reference/blob/7519cafd1823f1879c0d9c991ba0e5c7fd3bfa02/osu!/GameModes/Play/Rulesets/Taiko/RulesetTaiko.cs#L514
            Y = 135f / stable_gamefield_height;
        }

        protected override void Update()
        {
            base.Update();

            const float base_relative_height = KatsudonPlayfield.DEFAULT_HEIGHT / 768;

            float relativeHeight = base_relative_height;

            // Players coming from stable expect to be able to change the aspect ratio regardless of the window size.
            // We originally wanted to limit this more, but there was considerable pushback from the community.
            //
            // As a middle-ground, the aspect ratio can still be adjusted in the downwards direction but has a maximum limit.
            // This is still a bit weird, because readability changes with window size, but it is what it is.
            if (LockPlayfieldAspectRange.Value)
            {
                float currentAspect = Parent!.ChildSize.X / Parent!.ChildSize.Y;

                if (currentAspect > MAXIMUM_ASPECT)
                    relativeHeight *= currentAspect / MAXIMUM_ASPECT;
                else if (currentAspect < MINIMUM_ASPECT)
                    relativeHeight *= currentAspect / MINIMUM_ASPECT;
            }

            // Limit the maximum relative height of the playfield to one-third of available area to avoid it masking out on extreme resolutions.
            relativeHeight = Math.Min(relativeHeight, 1f / 3f);

            Scale = new Vector2(Math.Max((Parent!.ChildSize.Y / 768f) * (relativeHeight / base_relative_height), 1f));

            // on mobile platforms where the base aspect ratio is wider, the katsudon playfield
            // needs to be scaled down to remain playable.
            if (RuntimeInfo.IsMobile && osuGame != null)
            {
                const float base_aspect_ratio = 1024f / 768f;
                float gameAspectRatio = osuGame.ScalingContainerTargetDrawSize.X / osuGame.ScalingContainerTargetDrawSize.Y;
                // this magic scale is unexplainable, but required so the playfield doesn't become too zoomed out as the aspect ratio increases.
                const float magic_scale = 1.25f;
                Scale *= magic_scale * new Vector2(base_aspect_ratio / gameAspectRatio);
            }

            Width = 1 / Scale.X;
        }

        public double ComputeTimeRange()
        {
            float currentAspect = Parent!.ChildSize.X / Parent!.ChildSize.Y;

            if (LockPlayfieldAspectRange.Value)
                currentAspect = Math.Clamp(currentAspect, MINIMUM_ASPECT, MAXIMUM_ASPECT);

            // in a game resolution of 1024x768, stable's scrolling system consists of objects being placed 600px (widthScaled - 40) away from their hit location.
            // however, the point at which the object renders at the end of the screen is exactly x=640, but stable makes the object start moving from beyond the screen instead of the boundary point.
            // therefore, in lazer we have to adjust the "in length" so that it's in a 640px->160px fashion before passing it down as a "time range".
            // see stable's "in length": https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManagerTaiko.cs#L168
            const float stable_hit_location = 160f;
            float widthScaled = currentAspect * stable_gamefield_height;
            float inLength = widthScaled - stable_hit_location;

            // also in a game resolution of 1024x768, stable makes hit objects scroll from 760px->160px at a duration of 6000ms, divided by slider velocity (i.e. at a rate of 0.1px/ms)
            // compare: https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManagerTaiko.cs#L218
            // note: the variable "sv", in the linked reference, is equivalent to MultiplierControlPoint.Multiplier * 100, but since time range is agnostic of velocity, we replace "sv" with 100 below.
            float inMsLength = inLength / 100 * 1000;

            // stable multiplies the slider velocity by 1.4x for certain reasons, divide the time range by that factor to achieve similar result.
            // for references on how the factor is applied to the time range, see:
            //  1. https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManagerTaiko.cs#L79 (DifficultySliderMultiplier multiplied by 1.4x)
            //  2. https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManager.cs#L468-L470 (DifficultySliderMultiplier used to calculate SliderScoringPointDistance)
            //  3. https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManager.cs#L248-L250 (SliderScoringPointDistance used to calculate slider velocity, i.e. the "sv" variable from above)
            inMsLength /= KatsudonBeatmapConverter.VELOCITY_MULTIPLIER;

            return inMsLength;
        }
    }
}
