// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Katsudon.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Katsudon.Objects.Drawables
{
    public abstract partial class DrawableKatsudonHitObject : DrawableHitObject<KatsudonHitObject>, IKeyBindingHandler<KatsudonAction>
    {
        protected readonly Container Content;
        private readonly Container proxiedContent;

        private readonly Container nonProxiedContent;

        /// <summary>
        /// Whether the location of the hit should be snapped to the hit target before animating.
        /// </summary>
        /// <remarks>
        /// This is how osu-stable worked, but notably is not how TnT works.
        /// Not snapping results in less visual feedback on hit accuracy.
        /// </remarks>
        public bool SnapJudgementLocation { get; set; }

        [Resolved]
        private TwoPlayerScoring twoPlayerScoring { get; set; } = null!;

        protected DrawableKatsudonHitObject([CanBeNull] KatsudonHitObject hitObject)
            : base(hitObject)
        {
            AddRangeInternal(new[]
            {
                nonProxiedContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = Content = new Container { RelativeSizeAxes = Axes.Both }
                },
                proxiedContent = new ProxiedContentContainer { RelativeSizeAxes = Axes.Both }
            });

            OnNewResult += ForwardTwoPlayerResult;
            OnRevertResult += RevertTwoPlayerResult;
        }

        /// <summary>
        /// <see cref="proxiedContent"/> is proxied into an upper layer. We don't want to get masked away otherwise <see cref="proxiedContent"/> would too.
        /// </summary>
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        private bool isProxied;

        /// <summary>
        /// Moves <see cref="Content"/> to a layer proxied above the playfield.
        /// Does nothing if content is already proxied.
        /// </summary>
        protected void ProxyContent()
        {
            if (isProxied) return;

            isProxied = true;

            nonProxiedContent.Remove(Content, false);
            proxiedContent.Add(Content);
        }

        /// <summary>
        /// Moves <see cref="Content"/> to the normal hitobject layer.
        /// Does nothing is content is not currently proxied.
        /// </summary>
        protected void UnproxyContent()
        {
            if (!isProxied) return;

            isProxied = false;

            proxiedContent.Remove(Content, false);
            nonProxiedContent.Add(Content);
        }

        /// <summary>
        /// Creates a proxy for the content of this <see cref="DrawableKatsudonHitObject"/>.
        /// </summary>
        public Drawable CreateProxiedContent() => proxiedContent.CreateProxy();

        public abstract bool OnPressed(KeyBindingPressEvent<KatsudonAction> e);

        public virtual void OnReleased(KeyBindingReleaseEvent<KatsudonAction> e)
        {
        }

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set
            {
                base.LifetimeStart = value;
                proxiedContent.LifetimeStart = value;
            }
        }

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set
            {
                base.LifetimeEnd = value;
                proxiedContent.LifetimeEnd = value;
            }
        }

        protected void ForwardTwoPlayerResult(DrawableHitObject sender, JudgementResult result)
        {
            if (sender.HitObject is KatsudonHitObject obj)
            {
                if (obj.IsPlayer1)
                    twoPlayerScoring.Player1.ApplyResult(result);
                if (obj.IsPlayer2)
                    twoPlayerScoring.Player2.ApplyResult(result);
            }
        }

        protected void RevertTwoPlayerResult(DrawableHitObject sender, JudgementResult result)
        {
            if (sender.HitObject is KatsudonHitObject obj)
            {
                if (HitObject.IsPlayer1)
                    twoPlayerScoring.Player1.RevertResult(result);
                if (HitObject.IsPlayer2)
                    twoPlayerScoring.Player2.RevertResult(result);
            }
        }

        private partial class ProxiedContentContainer : Container
        {
            public override bool RemoveWhenNotAlive => false;
        }

        // Most osu!taiko hitsounds are managed by the drum (see DrumSampleTriggerSource).
        public override IEnumerable<HitSampleInfo> GetSamples() => Enumerable.Empty<HitSampleInfo>();
    }

    public abstract partial class DrawableKatsudonHitObject<TObject> : DrawableKatsudonHitObject
        where TObject : KatsudonHitObject
    {
        public override Vector2 OriginPosition => new Vector2(DrawHeight / 2);

        private const float LaneHeight = UI.KatsudonPlayfield.DEFAULT_HEIGHT / 2;

        public new TObject HitObject => (TObject)base.HitObject;

        protected SkinnableDrawable MainPiece;

        protected DrawableKatsudonHitObject([CanBeNull] TObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Custom;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void OnApply()
        {
            base.OnApply();

            // TODO: THIS CANNOT BE HERE, it makes pooling pointless (see https://github.com/ppy/osu/issues/21072).
            RecreatePieces();

            if (MainPiece != null && HitObject.PlayerId != -1)
                MainPiece.Y = HitObject.PlayerId * LaneHeight - LaneHeight * 0.5f;
        }

        protected virtual void RecreatePieces()
        {
            if (MainPiece != null)
                Content.Remove(MainPiece, true);

            MainPiece = CreateMainPiece();

            if (MainPiece != null)
                Content.Add(MainPiece);
        }

        [CanBeNull]
        protected abstract SkinnableDrawable CreateMainPiece();
    }
}
