// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Katsudon.Skinning.Legacy
{
    public class KatsudonLegacySkinTransformer : LegacySkinTransformer
    {
        public override bool IsProvidingLegacyResources => base.IsProvidingLegacyResources || hasHitCircle || hasBarLeft;

        private readonly Lazy<bool> hasExplosion;

        private bool hasHitCircle => GetTexture("taikohitcircle") != null;
        private bool hasBarLeft => GetTexture("taiko-bar-left") != null;

        public KatsudonLegacySkinTransformer(ISkin skin)
            : base(skin)
        {
            hasExplosion = new Lazy<bool>(() => GetTexture(getHitName(KatsudonSkinComponents.KatsudonExplosionGreat)) != null);
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            if (lookup is SkinComponentLookup<HitResult>)
            {
                // if a taiko skin is providing explosion sprites, hide the judgements completely
                if (hasExplosion.Value)
                    return Drawable.Empty().With(d => d.Expire());
            }

            if (lookup is KatsudonSkinComponentLookup taikoComponent)
            {
                switch (taikoComponent.Component)
                {
                    case KatsudonSkinComponents.DrumRollBody:
                        if (GetTexture("taiko-roll-middle") != null)
                            return new LegacyDrumRoll();

                        return null;

                    case KatsudonSkinComponents.InputDrum:
                        if (hasBarLeft)
                            return new LegacyInputDrum();

                        return null;

                    case KatsudonSkinComponents.DrumSamplePlayer:
                        return null;

                    case KatsudonSkinComponents.CentreHit:
                    case KatsudonSkinComponents.RimHit:
                        if (hasHitCircle)
                            return new LegacyHit(taikoComponent.Component);

                        return null;

                    case KatsudonSkinComponents.DrumRollTick:
                        return this.GetAnimation("sliderscorepoint", false, false);

                    case KatsudonSkinComponents.Swell:
                        if (GetTexture("spinner-circle") != null)
                            return new LegacySwell();

                        return null;

                    case KatsudonSkinComponents.HitTarget:
                        if (GetTexture("taikobigcircle") != null)
                            return new KatsudonLegacyHitTarget();

                        return null;

                    case KatsudonSkinComponents.PlayfieldBackgroundRight:
                        if (GetTexture("taiko-bar-right") != null)
                            return new KatsudonLegacyPlayfieldBackgroundRight();

                        return null;

                    case KatsudonSkinComponents.PlayfieldBackgroundLeft:
                        // This is displayed inside LegacyInputDrum. It is required to be there for layout purposes (can be seen on legacy skins).
                        if (GetTexture("taiko-bar-right") != null)
                            return Drawable.Empty();

                        return null;

                    case KatsudonSkinComponents.BarLine:
                        if (GetTexture("taiko-barline") != null)
                            return new LegacyBarLine();

                        return null;

                    case KatsudonSkinComponents.KatsudonExplosionMiss:
                        var missSprite = this.GetAnimation(getHitName(taikoComponent.Component), true, false);
                        if (missSprite != null)
                            return new LegacyHitExplosion(missSprite);

                        return null;

                    case KatsudonSkinComponents.KatsudonExplosionOk:
                    case KatsudonSkinComponents.KatsudonExplosionGreat:
                        string hitName = getHitName(taikoComponent.Component);
                        var hitSprite = this.GetAnimation(hitName, true, false);

                        if (hitSprite != null)
                        {
                            var strongHitSprite = this.GetAnimation($"{hitName}k", true, false);

                            return new LegacyHitExplosion(hitSprite, strongHitSprite);
                        }

                        return null;

                    case KatsudonSkinComponents.KatsudonExplosionKiai:
                        // suppress the default kiai explosion if the skin brings its own sprites.
                        // the drawable needs to expire as soon as possible to avoid accumulating empty drawables on the playfield.
                        if (hasExplosion.Value)
                            return Drawable.Empty().With(d => d.Expire());

                        return null;

                    case KatsudonSkinComponents.Scroller:
                        if (GetTexture("taiko-slider") != null)
                            return new LegacyKatsudonScroller();

                        return null;

                    case KatsudonSkinComponents.Mascot:
                        return new DrawableTaikoMascot();

                    case KatsudonSkinComponents.KiaiGlow:
                        if (GetTexture("taiko-glow") != null)
                            return new LegacyKiaiGlow();

                        return null;

                    default:
                        throw new UnsupportedSkinComponentException(lookup);
                }
            }

            return base.GetDrawableComponent(lookup);
        }

        private string getHitName(KatsudonSkinComponents component)
        {
            switch (component)
            {
                case KatsudonSkinComponents.KatsudonExplosionMiss:
                    return "taiko-hit0";

                case KatsudonSkinComponents.KatsudonExplosionOk:
                    return "taiko-hit100";

                case KatsudonSkinComponents.KatsudonExplosionGreat:
                    return "taiko-hit300";
            }

            throw new ArgumentOutOfRangeException(nameof(component), $"Invalid component type: {component}");
        }

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            if (sampleInfo is HitSampleInfo hitSampleInfo)
                return base.GetSample(new LegacyKatsudonSampleInfo(hitSampleInfo));

            return base.GetSample(sampleInfo);
        }

        private class LegacyKatsudonSampleInfo : HitSampleInfo
        {
            public LegacyKatsudonSampleInfo(HitSampleInfo sampleInfo)
                : base(sampleInfo.Name, sampleInfo.Bank, sampleInfo.Suffix, sampleInfo.Volume)

            {
            }

            public override IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (string name in base.LookupNames)
                        yield return name.Insert(name.LastIndexOf('/') + 1, "taiko-");
                }
            }
        }
    }
}
