using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        public const float INPUT_DRUM_WIDTH = 180f;

        public Container UnderlayElements { get; private set; } = null!;

        // private Container<HitExplosion> hitExplosionContainer;
        private Container<KiaiHitExplosion>[] kiaiExplosionContainers = new Container<KiaiHitExplosion>[3];
        private JudgementContainer<DrawableTaikoJudgement>[] judgementContainers = new JudgementContainer<DrawableTaikoJudgement>[3];
        private ScrollingHitObjectContainer drumRollHitContainer = null!;
        internal Drawable HitTarget = null!;

        private JudgementPooler<DrawableTaikoJudgement> judgementPooler = null!;
        // private readonly IDictionary<HitResult, HitExplosionPool> explosionPools = new Dictionary<HitResult, HitExplosionPool>();

        private ProxyContainer topLevelHitContainer = null!;
        private InputDrum inputDrum = null!;

        /// <remarks>
        /// <see cref="Playfield.AddNested"/> is purposefully not called on this to prevent i.e. being able to interact
        /// with bar lines in the editor.
        /// </remarks>
        private BarLinePlayfield barLinePlayfield = null!;

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
            const float hit_target_width = 200;
            const float hit_target_offset = -24f;

            inputDrum = new InputDrum
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                Width = INPUT_DRUM_WIDTH,
            };

            InternalChildren = new[]
            {
                new SkinnableDrawable(new KatsudonSkinComponentLookup(-1, KatsudonSkinComponents.PlayfieldBackgroundRight), _ => new PlayfieldBackgroundRight()),
                new Container
                {
                    Name = "Left overlay",
                    RelativeSizeAxes = Axes.Y,
                    Width = INPUT_DRUM_WIDTH,
                    BorderColour = colours.Gray0,
                    Children = new[]
                    {
                        new SkinnableDrawable(new KatsudonSkinComponentLookup(-1, KatsudonSkinComponents.PlayfieldBackgroundLeft), _ => new PlayfieldBackgroundLeft()),
                        inputDrum.CreateProxy(),
                    }
                },
                // TODO: positioning is hacky
                new PlayerScoring(twoPlayerScoring.Player1)
                {
                    Name = "Player 1 scoring",
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomCentre,
                    RelativeAnchorPosition = new osuTK.Vector2(0.25f, 0),
                    Margin = new MarginPadding { Bottom = 132 },
                    Children = new Drawable[]
                    {
                        new DefaultComboCounter
                        {
                            Name = "P1 Combo",
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            AutoSizeAxes = Axes.X,
                            Margin = new MarginPadding { Left = 330, Top = 15 },
                        },
                        new DefaultScoreCounter
                        {
                            Name = "P1 Score",
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            AutoSizeAxes = Axes.X,
                        },
                    }
                },
                new PlayerScoring(twoPlayerScoring.Player2)
                {
                    Name = "Player 2 scoring",
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.TopCentre,
                    RelativeAnchorPosition = new osuTK.Vector2(0.25f, 1),
                    Margin = new MarginPadding { Top = 100 },
                    Children = new Drawable[]
                    {
                        new DefaultComboCounter
                        {
                            Name = "P2 Combo",
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre,
                            AutoSizeAxes = Axes.X,
                            Margin = new MarginPadding { Left = 330, Top = 15 },
                        },
                        new DefaultScoreCounter
                        {
                            Name = "P2 Score",
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.BottomCentre,
                            AutoSizeAxes = Axes.X,
                        },
                    },
                },
                new SkinnableDrawable(new KatsudonSkinComponentLookup(-1, KatsudonSkinComponents.Mascot), _ => Empty())
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.TopLeft,
                    RelativePositionAxes = Axes.Y,
                    RelativeSizeAxes = Axes.None,
                    Y = 0.2f
                },
                new Container
                {
                    Name = "Right area",
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = INPUT_DRUM_WIDTH },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Elements behind hit objects",
                            RelativeSizeAxes = Axes.Y,
                            Width = hit_target_width,
                            X = hit_target_offset,
                            Children = new[]
                            {
                                new SkinnableDrawable(new KatsudonSkinComponentLookup(-1, KatsudonSkinComponents.KiaiGlow), _ => Empty())
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                // hitExplosionContainer = new Container<HitExplosion>
                                // {
                                //     RelativeSizeAxes = Axes.Both,
                                // },
                                HitTarget = new KatsudonHitTarget() // TODO: make this component skinnable
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            }
                        },
                        new Container
                        {
                            Name = "Bar line content",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = hit_target_width / 2 + hit_target_offset },
                            Children = new Drawable[]
                            {
                                UnderlayElements = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                barLinePlayfield = new BarLinePlayfield(),
                            }
                        },
                        new Container
                        {
                            Name = "Masked hit objects content",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = hit_target_width / 2 + hit_target_offset },
                            Masking = false,
                            Child = HitObjectContainer,
                        },
                        new Container
                        {
                            Name = "Overlay content",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = hit_target_width / 2 + hit_target_offset },
                            Children = new Drawable[]
                            {
                                drumRollHitContainer = new DrumRollHitContainer(),
                                kiaiExplosionContainers[0] = new Container<KiaiHitExplosion>
                                {
                                    Name = "Kiai hit explosions combined",
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                                kiaiExplosionContainers[1] = new Container<KiaiHitExplosion>
                                {
                                    Name = "Kiai hit explosions P1",
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                                kiaiExplosionContainers[2] = new Container<KiaiHitExplosion>
                                {
                                    Name = "Kiai hit explosions P2",
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                                judgementContainers[0] = new JudgementContainer<DrawableTaikoJudgement>
                                {
                                    Name = "Judgements combined",
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                                judgementContainers[1] = new JudgementContainer<DrawableTaikoJudgement>
                                {
                                    Name = "Judgements P1",
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                },
                                judgementContainers[2] = new JudgementContainer<DrawableTaikoJudgement>
                                {
                                    Name = "Judgements P2",
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.CentreLeft,
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
                new SkinnableDrawable(new KatsudonSkinComponentLookup(-1, KatsudonSkinComponents.DrumSamplePlayer), _ => new DrumSamplePlayer())
                {
                    RelativeSizeAxes = Axes.Both,
                },
                // this is added at the end of the hierarchy to receive input before taiko objects.
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


            HitResult[] usableHitResults = Enum.GetValues<HitResult>().Where(r => hitWindows.IsHitResultAllowed(r)).ToArray();

            AddInternal(judgementPooler = new JudgementPooler<DrawableTaikoJudgement>(usableHitResults));

            // foreach (var result in usableHitResults)
            //     explosionPools.Add(result, new HitExplosionPool(result));
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

        #region Pooling support

        public override void Add(HitObject h)
        {
            switch (h)
            {
                case BarLine barLine:
                    barLinePlayfield.Add(barLine);
                    break;

                case KatsudonHitObject katsudonHitObject:
                    base.Add(katsudonHitObject);
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

                    var judgement = judgementPooler.Get(result.Type, j => j.Apply(result, judgedObject));

                    if (judgement == null)
                        return;

                    int playerId = judgedObject.HitObject is KatsudonHitObject o ? o.PlayerId : -1;

                    judgementContainers[playerId + 1].Add(judgement);

                    var type = (judgedObject.HitObject as Hit)?.Type ?? Taiko.Objects.HitType.Centre;
                    addExplosion(judgedObject, result.Type, type, playerId);
                    break;
            }
        }

        private void addDrumRollHit(DrawableDrumRollTick drawableTick) =>
            drumRollHitContainer.Add(new DrawableFlyingHit(drawableTick));

        private void addExplosion(DrawableHitObject drawableObject, HitResult result, Taiko.Objects.HitType type, int playerId)
        {
            // hitExplosionContainer.Add(explosionPools[result]
            //     .Get(explosion => explosion.Apply(drawableObject)));
            if (drawableObject.HitObject.Kiai)
                kiaiExplosionContainers[playerId + 1].Add(new KiaiHitExplosion(drawableObject, type));
        }

        private partial class ProxyContainer : LifetimeManagementContainer
        {
            public void Add(Drawable proxy) => AddInternal(proxy);

            public override bool UpdateSubTreeMasking()
            {
                // DrawableHitObject disables masking.
                // Hitobject content is proxied and unproxied based on hit status and the IsMaskedAway value could get stuck because of this.
                return false;
            }
        }
    }
}
