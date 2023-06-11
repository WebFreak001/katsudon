using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Rulesets.Katsudon.Objects.Drawables;
using osu.Game.Rulesets.Katsudon.Objects;
using osu.Game.Skinning;
using osuTK;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.Katsudon.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Rulesets.Katsudon.UI
{
    public partial class KatsudonPlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// Default height of a <see cref="KatsudonPlayfield"/> when inside a <see cref="DrawableKatsudonRuleset"/>.
        /// </summary>
        public const float DEFAULT_HEIGHT = 400;

        /// <summary>
        /// Whether the hit target should be nudged further towards the left area, matching the stable "classic" position.
        /// </summary>
        public Bindable<bool> ClassicHitTargetPosition = new BindableBool();

        // private Container<HitExplosion> hitExplosionContainer;
        private Container<KiaiHitExplosion> kiaiExplosionContainer;
        private JudgementContainer<DrawableTaikoJudgement> judgementContainer;
        private ScrollingHitObjectContainer drumRollHitContainer;
        internal Drawable HitTarget;
        private SkinnableDrawable mascot;

        private readonly IDictionary<HitResult, DrawablePool<DrawableTaikoJudgement>> judgementPools = new Dictionary<HitResult, DrawablePool<DrawableTaikoJudgement>>();
        // private readonly IDictionary<HitResult, HitExplosionPool> explosionPools = new Dictionary<HitResult, HitExplosionPool>();

        private ProxyContainer topLevelHitContainer;
        private InputDrum inputDrum;
        private Container rightArea;

        /// <remarks>
        /// <see cref="Playfield.AddNested"/> is purposefully not called on this to prevent i.e. being able to interact
        /// with bar lines in the editor.
        /// </remarks>
        private BarLinePlayfield barLinePlayfield;

        private Container barLineContent;
        private Container hitObjectContent;
        private Container overlayContent;

        private TwoPlayerScoring twoPlayerScoring = new();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(twoPlayerScoring);
            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            inputDrum = new InputDrum
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
            };

            InternalChildren = new[]
            {
                new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.PlayfieldBackgroundRight), _ => new PlayfieldBackgroundRight()),
                new Container
                {
                    Name = "Left overlay",
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    BorderColour = colours.Gray0,
                    Children = new[]
                    {
                        new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.PlayfieldBackgroundLeft), _ => new PlayfieldBackgroundLeft()),
                        inputDrum.CreateProxy(),
                    }
                },
                // TODO: positioning is hacky
                new PlayerScoring(twoPlayerScoring.Player1)
                {
                    Name = "Player 1 scoring",
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.TopCentre,
                    Margin = new MarginPadding { Bottom = 100 },
                    Children = new Drawable[]
                    {
                        new DefaultComboCounter
                        {
                            Name = "P1 Combo",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            AutoSizeAxes = Axes.X,
                            Margin = new MarginPadding { Left = 330, Top = 15 },
                        },
                        new DefaultScoreCounter
                        {
                            Name = "P1 Score",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            AutoSizeAxes = Axes.X,
                        },
                    }
                },
                new PlayerScoring(twoPlayerScoring.Player2)
                {
                    Name = "Player 2 scoring",
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.BottomCentre,
                    Margin = new MarginPadding { Top = 100 },
                    Children = new Drawable[]
                    {
                        new DefaultComboCounter
                        {
                            Name = "P2 Combo",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            AutoSizeAxes = Axes.X,
                            Margin = new MarginPadding { Left = 330, Top = 15 },
                        },
                        new DefaultScoreCounter
                        {
                            Name = "P2 Score",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            AutoSizeAxes = Axes.X,
                        },
                    },
                },
                mascot = new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.Mascot), _ => Empty())
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.TopLeft,
                    RelativePositionAxes = Axes.Y,
                    RelativeSizeAxes = Axes.None,
                    Y = 0.2f
                },
                rightArea = new Container
                {
                    Name = "Right area",
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Elements before hit objects",
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Children = new[]
                            {
                                new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.KiaiGlow), _ => Empty())
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                // hitExplosionContainer = new Container<HitExplosion>
                                // {
                                //     RelativeSizeAxes = Axes.Both,
                                // },
                                HitTarget = new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.HitTarget), _ => new KatsudonHitTarget())
                                {
                                    RelativeSizeAxes = Axes.Both
                                }
                            }
                        },
                        barLineContent = new Container
                        {
                            Name = "Bar line content",
                            RelativeSizeAxes = Axes.Both,
                            Child = barLinePlayfield = new BarLinePlayfield(),
                        },
                        hitObjectContent = new Container
                        {
                            Name = "Masked hit objects content",
                            RelativeSizeAxes = Axes.Both,
                            Masking = false,
                            // Masking = true, // TODO: why was this in the original code?
                            Child = HitObjectContainer,
                        },
                        overlayContent = new Container
                        {
                            Name = "Elements after hit objects",
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                drumRollHitContainer = new DrumRollHitContainer(),
                                kiaiExplosionContainer = new Container<KiaiHitExplosion>
                                {
                                    Name = "Kiai hit explosions",
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                                judgementContainer = new JudgementContainer<DrawableTaikoJudgement>
                                {
                                    Name = "Judgements",
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                            }
                        },
                    }
                },
                topLevelHitContainer = new ProxyContainer
                {
                    Name = "Top level hit objects",
                    RelativeSizeAxes = Axes.Both,
                },
                drumRollHitContainer.CreateProxy(),
                new DrumSamplePlayer(HitObjectContainer),
                // this is added at the end of the hierarchy to receive input before Katsudon objects.
                // but is proxied below everything to not cover visual effects such as hit explosions.
                inputDrum,
            };

            RegisterPool<Hit, DrawableHit>(50);
            RegisterPool<Hit.StrongNestedHit, DrawableHit.StrongNestedHit>(50);

            RegisterPool<DrumRoll, DrawableDrumRoll>(5);
            RegisterPool<DrumRoll.StrongNestedHit, DrawableDrumRoll.StrongNestedHit>(5);

            RegisterPool<DrumRollTick, DrawableDrumRollTick>(100);
            RegisterPool<DrumRollTick.StrongNestedHit, DrawableDrumRollTick.StrongNestedHit>(100);

            RegisterPool<Swell, DrawableSwell>(5);
            RegisterPool<SwellTick, DrawableSwellTick>(100);

            var hitWindows = new KatsudonHitWindows();

            foreach (var result in Enum.GetValues<HitResult>().Where(r => hitWindows.IsHitResultAllowed(r)))
            {
                judgementPools.Add(result, new DrawablePool<DrawableTaikoJudgement>(15));
                // explosionPools.Add(result, new HitExplosionPool(result));
            }

            AddRangeInternal(judgementPools.Values);
            // AddRangeInternal(explosionPools.Values);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            NewResult += OnNewResult;
        }

        protected override void OnNewDrawableHitObject(DrawableHitObject drawableHitObject)
        {
            base.OnNewDrawableHitObject(drawableHitObject);

            var KatsudonObject = (DrawableKatsudonHitObject)drawableHitObject;
            topLevelHitContainer.Add(KatsudonObject.CreateProxiedContent());
        }

        protected override void Update()
        {
            base.Update();

            // Padding is required to be updated for elements which are based on "absolute" X sized elements.
            // This is basically allowing for correct alignment as relative pieces move around them.
            rightArea.Padding = new MarginPadding { Left = inputDrum.Width };
            barLineContent.Padding = new MarginPadding { Left = HitTarget.DrawWidth / 2 };
            hitObjectContent.Padding = new MarginPadding { Left = HitTarget.DrawWidth / 2 };
            overlayContent.Padding = new MarginPadding { Left = HitTarget.DrawWidth / 2 };

            mascot.Scale = new Vector2(DrawHeight / DEFAULT_HEIGHT);
        }

        #region Pooling support

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case BarLine barLine:
                    barLinePlayfield.Add(barLine);
                    break;

                case KatsudonHitObject KatsudonHitObject:
                    base.Add(KatsudonHitObject);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(HitObject)} type: {h.GetType()}");
            }
        }

        public override bool Remove(HitObject h)
        {
            switch (h)
            {
                case BarLine barLine:
                    return barLinePlayfield.Remove(barLine);

                case KatsudonHitObject KatsudonHitObject:
                    return base.Remove(KatsudonHitObject);

                default:
                    throw new ArgumentException($"Unsupported {nameof(HitObject)} type: {h.GetType()}");
            }
        }

        #endregion

        #region Non-pooling support

        public override void Add(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableBarLine barLine:
                    barLinePlayfield.Add(barLine);
                    break;

                case DrawableKatsudonHitObject:
                    base.Add(h);
                    break;

                default:
                    throw new ArgumentException($"Unsupported {nameof(DrawableHitObject)} type: {h.GetType()}");
            }
        }

        public override bool Remove(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableBarLine barLine:
                    return barLinePlayfield.Remove(barLine);

                case DrawableKatsudonHitObject:
                    return base.Remove(h);

                default:
                    throw new ArgumentException($"Unsupported {nameof(DrawableHitObject)} type: {h.GetType()}");
            }
        }

        #endregion

        internal void OnNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!DisplayJudgements.Value)
                return;
            if (!judgedObject.DisplayResult)
                return;

            switch (result.Judgement)
            {
                case TaikoStrongJudgement:
                    // if (result.IsHit)
                    //     hitExplosionContainer.Children.FirstOrDefault(e => e.JudgedObject == ((DrawableStrongNestedHit)judgedObject).ParentHitObject)?.VisualiseSecondHit(result);
                    break;

                case TaikoDrumRollTickJudgement:
                    if (!result.IsHit)
                        break;

                    var drawableTick = (DrawableDrumRollTick)judgedObject;

                    addDrumRollHit(drawableTick);
                    break;

                default:
                    if (!result.Type.IsScorable())
                        break;

                    judgementContainer.Add(judgementPools[result.Type].Get(j => j.Apply(result, judgedObject)));

                    var type = (judgedObject.HitObject as Hit)?.Type ?? Taiko.Objects.HitType.Centre;
                    addExplosion(judgedObject, result.Type, type);
                    break;
            }
        }

        private void addDrumRollHit(DrawableDrumRollTick drawableTick) =>
            drumRollHitContainer.Add(new DrawableFlyingHit(drawableTick));

        private void addExplosion(DrawableHitObject drawableObject, HitResult result, Taiko.Objects.HitType type)
        {
            // hitExplosionContainer.Add(explosionPools[result]
            //     .Get(explosion => explosion.Apply(drawableObject)));
            if (drawableObject.HitObject.Kiai)
                kiaiExplosionContainer.Add(new KiaiHitExplosion(drawableObject, type));
        }

        private partial class ProxyContainer : LifetimeManagementContainer
        {
            public void Add(Drawable proxy) => AddInternal(proxy);

            public override bool UpdateSubTreeMasking(Drawable source, RectangleF maskingBounds)
            {
                // DrawableHitObject disables masking.
                // Hitobject content is proxied and unproxied based on hit status and the IsMaskedAway value could get stuck because of this.
                return false;
            }
        }
    }
}
