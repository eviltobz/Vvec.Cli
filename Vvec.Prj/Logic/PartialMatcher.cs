using Microsoft.CodeAnalysis;
using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public class PartialMatcher
{
    private readonly IConsole cons;
    public PartialMatcher(IConsole console)
    {
        cons = console;
    }

    public (T match, bool multipleMatches) Find<T>(string title, string target, Dictionary<string, T> candidates) where T : class
    {
        var normalisedTarget = target.ToUpper();
        var normalisedKeys = candidates.ToDictionary(x => x.Key.ToUpper(), x => x.Key);
        var found = normalisedKeys.TryGetValue(normalisedTarget, out var actual);
        if (found)
            return (candidates[actual!], false);

        var partialCandidates = normalisedKeys
            .Where(x => x.Key.Contains(normalisedTarget))
            .ToArray();
        if (partialCandidates.Length == 1)
            return (candidates[partialCandidates[0].Value!], false);

        if (partialCandidates.Length > 1)
        {
            ReportMultipleMatches(title, target, partialCandidates.Select(kvp => kvp.Value).ToArray());
            return (null, true);
        }

        return (null, false);
    }

    private void ReportMultipleMatches(string title, string target, string[] matches)
    {
        cons.WriteLine(FG.Red, $"Multiple matches were found for {title} '", FG.Default, target, FG.Red, "'", FG.Default)
            .Write("  ");
        for (int i = 0; i < matches.Length; i++)
        {
            var match = matches[i];
            cons.Write(FG.Yellow, match, FG.Default);
            if (i + 1 < matches.Length)
                cons.Write(", ");
        }
        cons.WriteLine();
    }
}
