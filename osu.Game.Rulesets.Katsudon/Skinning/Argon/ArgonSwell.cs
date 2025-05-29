// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Katsudon.Skinning.Argon
{
    public partial class ArgonSwell : Default.DefaultSwell
    {
        protected override Drawable CreateCentreCircle()
        {
            return new ArgonSwellCirclePiece
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }
    }
}
