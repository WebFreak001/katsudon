// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Katsudon.Scoring;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Katsudon.Objects
{
    public abstract class KatsudonHitObject : HitObject
    {
        public int PlayerId { get; set; } = -1;

        public bool IsAnyPlayer => PlayerId == -1;
        public bool IsPlayer1 => IsAnyPlayer || PlayerId == 0;
        public bool IsPlayer2 => IsAnyPlayer || PlayerId == 1;

        /// <summary>
        /// Default size of a drawable taiko hit object.
        /// </summary>
        public const float DEFAULT_SIZE = 0.475f;

        public override Judgement CreateJudgement() => new TaikoJudgement();

        protected override HitWindows CreateHitWindows() => new KatsudonHitWindows();

        public static KatsudonHitObject Convert(Taiko.Objects.TaikoHitObject obj, int playerNo)
        {
            var ret = obj switch
            {
                osu.Game.Rulesets.Taiko.Objects.DrumRoll drumroll => DrumRoll.Convert(drumroll, playerNo),
                osu.Game.Rulesets.Taiko.Objects.IgnoreHit ignoreHit => IgnoreHit.Convert(ignoreHit, playerNo),
                osu.Game.Rulesets.Taiko.Objects.Hit hit => Hit.Convert(hit, playerNo),
                _ => throw new NotImplementedException("conversion from type " + obj.GetType() + " not implemented")
            };
            if (ret is StrongNestedHitObject nested)
            {
                var parent = ((Taiko.Objects.StrongNestedHitObject)obj).Parent;
                if (parent != null)
                    nested.Parent = Convert(parent, playerNo);
            }
            return ret;
        }

        public static Taiko.Objects.TaikoHitObject Unconvert(KatsudonHitObject obj)
        {
            var ret = obj switch
            {
                DrumRoll drumroll => DrumRoll.Unconvert(drumroll),
                IgnoreHit ignoreHit => IgnoreHit.Unconvert(ignoreHit),
                Hit hit => Hit.Unconvert(hit),
                _ => throw new NotImplementedException("conversion from type " + obj.GetType() + " not implemented")
            };
            return ret;
        }
    }
}
