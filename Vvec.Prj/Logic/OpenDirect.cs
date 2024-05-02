using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;


public class OpenDirect(IConsole cons, Config config) : IOpenDirect
{
    public string? FindPath(string hint, string[] projectFolders)
    {
        var normalisedHint = hint.ToUpper();
        var normalisedShortcuts = config.Shortcuts.ToDictionary(x => x.Key.ToUpper(), x => x.Value);
        var found = normalisedShortcuts.TryGetValue(normalisedHint, out var path);
        if (found)
            return path;

        var partialShortcuts = normalisedShortcuts
            .Where(x => x.Key.Contains(normalisedHint))
            .Select(x => x.Value.Value)
            .ToArray();
        if (partialShortcuts.Length == 1)
            return partialShortcuts[0];
        if (partialShortcuts.Length > 1)
        {
            ReportMultipleMatches("shortcut", hint, partialShortcuts);
            return null;
        }

        var normalisedPaths = projectFolders.Select(x => new { Key = x.Substring(config.ProjectRoot!.Value.Length).ToUpper(), Path = x });

        var fromProject = normalisedPaths.FirstOrDefault(x => x.Key == normalisedHint);
        if (fromProject is not null)
            return fromProject.Path;

        var partialFromProject = normalisedPaths
            .Where(x => x.Key.Contains(normalisedHint))
            .Select(x => x.Path)
            .ToArray();
        if (partialFromProject.Length == 1)
            return partialFromProject[0];
        if (partialFromProject.Length > 1)
        {
            ReportMultipleMatches("path", hint, partialFromProject);
            return null;
        }

        cons.WriteLine("No match for '".InRed(), hint, "'".InRed());
        return null;
    }

    private void ReportMultipleMatches(string title, string hint, string[] matches)
    {
        cons.WriteLine($"Multiple matches were found for {title} '".InRed(), hint, "'".InRed())
            .Write("  ");
        for (int i = 0; i < matches.Length; i++)
        {
            var match = matches[i];
            cons.Write(match.InYellow());
            if (i + 1 < matches.Length)
                cons.Write(", ");
        }
        cons.WriteLine();
    }
}
