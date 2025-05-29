// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Taiko;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Katsudon.Skinning.Legacy
{
    public partial class LegacyHit : LegacyCirclePiece
    {
        private readonly KatsudonSkinComponents component;

        public LegacyHit(KatsudonSkinComponents component)
        {
            this.component = component;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AccentColour = LegacyColourCompatibility.DisallowZeroAlpha(
                component == KatsudonSkinComponents.CentreHit
                    ? new Color4(235, 69, 44, 255)
                    : new Color4(67, 142, 172, 255));
        }
    }
}
