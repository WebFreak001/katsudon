// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Katsudon.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm;

namespace osu.Game.Rulesets.Katsudon.Difficulty
{
    public class KatsudonDifficultyCalculator : TaikoDifficultyCalculator
    {
        public KatsudonDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            var difficultyHitObjects = new List<DifficultyHitObject>();
            var centreObjects = new List<TaikoDifficultyHitObject>();
            var rimObjects = new List<TaikoDifficultyHitObject>();
            var noteObjects = new List<TaikoDifficultyHitObject>();

            HitObject? lastObject = null;

            // Generate TaikoDifficultyHitObjects from the beatmap's hit objects.
            for (int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                if (beatmap.HitObjects[i] is not KatsudonHitObject kho)
                    continue;
                if (kho.PlayerId == 1)
                    continue;
                var hitObject = KatsudonHitObject.Unconvert(kho);
                if (lastObject is not null)
                {
                    difficultyHitObjects.Add(new TaikoDifficultyHitObject(
                        hitObject,
                        lastObject,
                        clockRate,
                        difficultyHitObjects,
                        centreObjects,
                        rimObjects,
                        noteObjects,
                        difficultyHitObjects.Count,
                        beatmap.ControlPointInfo,
                        beatmap.Difficulty.SliderMultiplier
                    ));
                }
                lastObject = hitObject;
            }

            TaikoColourDifficultyPreprocessor.ProcessAndAssign(difficultyHitObjects);
            TaikoRhythmDifficultyPreprocessor.ProcessAndAssign(noteObjects);

            return difficultyHitObjects;
        }
    }
}
