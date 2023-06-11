// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Katsudon
{
    public partial class KatsudonInputManager : RulesetInputManager<KatsudonAction>
    {
        public KatsudonInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum KatsudonAction
    {
        [Description("Player 1: Left (rim)")]
        P1_LeftRim,

        [Description("Player 1: Left (centre)")]
        P1_LeftCentre,

        [Description("Player 1: Right (centre)")]
        P1_RightCentre,

        [Description("Player 1: Right (rim)")]
        P1_RightRim,

        [Description("Player 2: Left (rim)")]
        P2_LeftRim,

        [Description("Player 2: Left (centre)")]
        P2_LeftCentre,

        [Description("Player 2: Right (centre)")]
        P2_RightCentre,

        [Description("Player 2: Right (rim)")]
        P2_RightRim,
    }

    public static class KatsudonActionExtensions
    {
        public static int GetPlayerNo(this KatsudonAction action)
        {
            return (int)action / 4;
        }

        public static KatsudonAction WithPlayerNo(this KatsudonAction action, int playerNo)
        {
            return (KatsudonAction)(((int)action % 4) + playerNo * 4);
        }
    }
}
