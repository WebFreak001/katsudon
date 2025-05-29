// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Katsudon.Beatmaps;
using osu.Game.Rulesets.Katsudon.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Localisation;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Scoring;
using System.Linq;
using osu.Game.Rulesets.Taiko;
using osu.Game.Skinning;
using osu.Game.Rulesets.Katsudon.Objects;
using osu.Game.Rulesets.Katsudon.Skinning.Argon;
using osu.Game.Rulesets.Katsudon.Skinning.Legacy;
using osu.Game.Rulesets.Filter;
using osu.Game.Rulesets.Katsudon.Scoring;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Katsudon
{
    public class KatsudonRuleset : Ruleset
    {
        // Leave this line intact. It will bake the correct version into the ruleset on each build/release.
        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;

        public const string SHORT_NAME = "katsudon";

        private TaikoRuleset? subRuleset = null;
        private TaikoRuleset getTaikoRuleset()
        {
            return subRuleset ??= new TaikoRuleset();
        }

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
            => new DrawableKatsudonRuleset(this, beatmap, mods);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap)
            => new KatsudonBeatmapConverter(getTaikoRuleset().CreateBeatmapConverter(beatmap), beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap)
            => getTaikoRuleset().CreateDifficultyCalculator(beatmap);

        public override IEnumerable<Mod> GetModsFor(ModType type)
            => getTaikoRuleset().GetModsFor(type);

        public override string ShortName => SHORT_NAME;

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => [
            new KeyBinding(InputKey.Q, KatsudonAction.P1_LeftRim),
            new KeyBinding(InputKey.A, KatsudonAction.P1_LeftCentre),
            new KeyBinding(InputKey.X, KatsudonAction.P1_RightCentre),
            new KeyBinding(InputKey.C, KatsudonAction.P1_RightRim),

            new KeyBinding(InputKey.Comma, KatsudonAction.P2_LeftRim),
            new KeyBinding(InputKey.Period, KatsudonAction.P2_LeftCentre),
            new KeyBinding(InputKey.Quote, KatsudonAction.P2_RightCentre),
            new KeyBinding(InputKey.BracketRight, KatsudonAction.P2_RightRim),
        ];

        public override Drawable CreateIcon() => new KatsudonRulesetIcon(this);

        public override ScoreProcessor CreateScoreProcessor()
            => new KatsudonScoreProcessor();

        public override HealthProcessor CreateHealthProcessor(double drainStartTime)
            => new KatsudonHealthProcessor();

        public override ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap)
        {
            switch (skin)
            {
                case ArgonSkin:
                    return new KatsudonArgonSkinTransformer(skin);

                case LegacySkin:
                    return new KatsudonLegacySkinTransformer(skin);
            }

            return null;
        }

        public override PerformanceCalculator CreatePerformanceCalculator()
            => getTaikoRuleset().CreatePerformanceCalculator();

        public override IRulesetFilterCriteria CreateRulesetFilterCriteria()
        {
            return new KatsudonFilterCriteria();
        }

        public override IRulesetConvertSupport GetRulesetMapConvertSupport()
        {
            return new KatsudonConvertSupport();
        }

        public override string Description => "katsudon";

        public override string PlayingVerb => "Bashing drums";

        // public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new TaikoReplayFrame();
        // public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new TaikoRulesetConfigManager(settings, RulesetInfo);
        // public override RulesetSettingsSubsection CreateSettings() => new TaikoSettingsSubsection(this);

        protected override IEnumerable<HitResult> GetValidHitResults()
        {
            return [
                HitResult.Great,
                HitResult.Ok,

                HitResult.SmallBonus,
                HitResult.LargeBonus,
            ];
        }

        public override LocalisableString GetDisplayNameForHitResult(HitResult result)
        {
            return getTaikoRuleset().GetDisplayNameForHitResult(result);
        }

        public override StatisticItem[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap)
        {
            var timedHitEventsP1 = score.HitEvents.Where(e => e.HitObject is Hit h && h.IsPlayer1).ToList();
            var timedHitEventsP2 = score.HitEvents.Where(e => e.HitObject is Hit h && h.IsPlayer2).ToList();

            return [
                new StatisticItem("P1 Performance Breakdown", () => new PerformanceBreakdownChart(score, playableBeatmap)
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                }),
                new StatisticItem("P2 Performance Breakdown", () => new PerformanceBreakdownChart(score, playableBeatmap)
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                }),
                new StatisticItem("P1 Timing Distribution", () => new HitEventTimingDistributionGraph(timedHitEventsP1)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 250
                }, true),
                new StatisticItem("P2 Timing Distribution", () => new HitEventTimingDistributionGraph(timedHitEventsP2)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 250
                }, true),
                new StatisticItem(string.Empty, () => new SimpleStatisticTable(3, new SimpleStatisticItem[]
                {
                    new AverageHitError(timedHitEventsP1),
                    new UnstableRate(timedHitEventsP1)
                }), true),
                new StatisticItem(string.Empty, () => new SimpleStatisticTable(3, new SimpleStatisticItem[]
                {
                    new AverageHitError(timedHitEventsP2),
                    new UnstableRate(timedHitEventsP2)
                }), true),
            ];
        }
    }
}
