using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Katsudon.UI
{
    public partial class PlayerScoring : Container
    {
        private readonly ScoreProcessor scoreProcessor;

        public PlayerScoring(ScoreProcessor scoreProcessor)
        {
            this.scoreProcessor = scoreProcessor;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            scoreProcessor.ApplyBeatmap(parent.Get<IBeatmap>());
            dependencies.CacheAs(scoreProcessor);
            return dependencies;
        }
    }
}