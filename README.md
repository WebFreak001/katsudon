# Katsudon

Ruleset for [osu!](https://github.com/ppy/osu) for 2 local players to play taiko on the same map on the same machine at the same time.

For taiko maps to show, currently needs https://github.com/ppy/osu/pull/23875 applied to osu. Commented lines in [osu.Game.Rulesets.Katsudon.csproj](osu.Game.Rulesets.Katsudon/osu.Game.Rulesets.Katsudon.csproj) show how to use the locally checked out osu! instead of osu! from nuget.

A lot of the code is copy-pasted from taiko, but I tried to forward calls to taiko to avoid code duplication. For this initial version I don't put major effort into this yet though and copy-paste code from taiko whenever issues arise.

Due to the code copying, this project is under the same license as the osu! repository.

