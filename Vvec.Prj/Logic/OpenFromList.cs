using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public class OpenFromList(IConsole cons, Config config, IOpenDirect openDirect)
{
    public string? ChoosePath(string[] projectFolders)
    {
        cons.WriteLine("Shortcuts".InGreen());
        foreach (var shortcut in config.Shortcuts)
        {
            cons.WriteLine("  [", shortcut.Key.InYellow(), "] ", shortcut.Value.Value);
        }

        cons.WriteLine($"Project Folders in {config.ProjectRoot}".InGreen());
        for (int i = 0; i < projectFolders.Length; i++)
        {
            var directory = projectFolders[i];
            cons.WriteLine("  [", i.InYellow(), "] ", directory.Substring(config.ProjectRoot!.Value.Length));
        }

        var selection = cons.StartPrompt("Select folder").GetFreeText();

        if (string.IsNullOrWhiteSpace(selection))
            return null;

        var isIndex = int.TryParse(selection, out int index);
        if (isIndex && index >= 0 && index < projectFolders.Length)
        {
            return projectFolders[index];
        }

        return openDirect.FindPath(selection, projectFolders);
    }
}
