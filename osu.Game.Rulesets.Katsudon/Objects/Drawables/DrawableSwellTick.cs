// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Katsudon.Objects.Drawables
{
    public partial class DrawableSwellTick : DrawableKatsudonHitObject<SwellTick>
    {
        public override bool DisplayResult => false;

        public DrawableSwellTick()
            : this(null)
        {
        }

        public DrawableSwellTick([CanBeNull] SwellTick hitObject)
            : base(hitObject)
        {
        }

        public void TriggerResult(bool hit)
        {
            HitObject.StartTime = Time.Current;

            if (hit)
                ApplyMaxResult();
            else
                ApplyMinResult();
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
        }

        public override bool OnPressed(KeyBindingPressEvent<KatsudonAction> e) => false;

        // protected override SkinnableDrawable CreateMainPiece() => null;
        protected override SkinnableDrawable CreateMainPiece() => new SkinnableDrawable(new KatsudonSkinComponentLookup(HitObject.PlayerId, KatsudonSkinComponents.DrumRollTick),
            _ => new TickPiece());
    }
}
