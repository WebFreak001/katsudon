// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Katsudon.Objects;

namespace osu.Game.Rulesets.Katsudon.UI
{
    /// <summary>
    /// A component that is displayed at the hit position in the Katsudon playfield.
    /// </summary>
    internal partial class KatsudonHitTarget : Container
    {
        /// <summary>
        /// Thickness of all drawn line pieces.
        /// </summary>
        private const float border_thickness = 2.5f;

        public KatsudonHitTarget()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new CircularContainer
                {
                    Name = "Upper Strong Hit Ring",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(KatsudonStrongableHitObject.DEFAULT_STRONG_SIZE),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = border_thickness,
                    Alpha = 0.1f,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                new CircularContainer
                {
                    Name = "Upper Normal Hit Ring",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(KatsudonHitObject.DEFAULT_SIZE),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = border_thickness,
                    Alpha = 0.5f,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                new CircularContainer
                {
                    Name = "Lower Strong Hit Ring",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(KatsudonStrongableHitObject.DEFAULT_STRONG_SIZE),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = border_thickness,
                    Alpha = 0.1f,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                new CircularContainer
                {
                    Name = "Lower Normal Hit Ring",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(KatsudonHitObject.DEFAULT_SIZE),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = border_thickness,
                    Alpha = 0.5f,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                new Box
                {
                    Name = "Bar center",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(border_thickness, (1 - KatsudonStrongableHitObject.DEFAULT_STRONG_SIZE) / 2f),
                    Alpha = 0.1f
                },
            };
        }
    }
}
