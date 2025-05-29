// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Katsudon.Skinning.Argon
{
    public class KatsudonArgonSkinTransformer : SkinTransformer
    {
        public KatsudonArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case SkinComponentLookup<HitResult> resultComponent:
                    // This should eventually be moved to a skin setting, when supported.
                    if (Skin is ArgonProSkin && resultComponent.Component >= HitResult.Great)
                        return Drawable.Empty();

                    return new ArgonJudgementPiece(resultComponent.Component);

                case KatsudonSkinComponentLookup katsudonComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (katsudonComponent.Component)
                    {
                        case KatsudonSkinComponents.CentreHit:
                            return new ArgonCentreCirclePiece();

                        case KatsudonSkinComponents.RimHit:
                            return new ArgonRimCirclePiece();

                        case KatsudonSkinComponents.PlayfieldBackgroundLeft:
                            return new ArgonPlayfieldBackgroundLeft();

                        case KatsudonSkinComponents.PlayfieldBackgroundRight:
                            return new ArgonPlayfieldBackgroundRight();

                        case KatsudonSkinComponents.InputDrum:
                            return new ArgonInputDrum();

                        case KatsudonSkinComponents.HitTarget:
                            return new ArgonHitTarget();

                        case KatsudonSkinComponents.BarLine:
                            return new ArgonBarLine();

                        case KatsudonSkinComponents.DrumRollBody:
                            return new ArgonElongatedCirclePiece();

                        case KatsudonSkinComponents.DrumRollTick:
                            return new ArgonTickPiece();

                        case KatsudonSkinComponents.KatsudonExplosionKiai:
                            // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                            return Drawable.Empty().With(d => d.Expire());

                        case KatsudonSkinComponents.DrumSamplePlayer:
                            return new ArgonDrumSamplePlayer();

                        case KatsudonSkinComponents.KatsudonExplosionGreat:
                        case KatsudonSkinComponents.KatsudonExplosionMiss:
                        case KatsudonSkinComponents.KatsudonExplosionOk:
                            return new ArgonHitExplosion(katsudonComponent.Component);

                        case KatsudonSkinComponents.Swell:
                            return new ArgonSwell();
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
