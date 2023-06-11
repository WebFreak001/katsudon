// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Katsudon.Objects;
using osu.Game.Rulesets.Katsudon.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Katsudon.UI
{
    public partial class BarLinePlayfield : ScrollingPlayfield
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            RegisterPool<BarLine, DrawableBarLine>(15);
        }
    }
}
