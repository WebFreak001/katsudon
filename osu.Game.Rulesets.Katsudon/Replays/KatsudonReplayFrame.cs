// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Katsudon.Replays
{
    public class KatsudonReplayFrame : ReplayFrame
    {
        public List<KatsudonAction> Actions = new List<KatsudonAction>();

        public KatsudonReplayFrame(KatsudonAction? button = null)
        {
            if (button.HasValue)
                Actions.Add(button.Value);
        }
    }
}
