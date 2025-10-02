using LibGit2Sharp;
using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public static class DisplayExtensions
{
    public static object[] ToConsole(this GitChanges changes)
    {
        if (changes.Total == 0)
            return [FG.DarkGrey, "No changes"]; //.InDarkGrey()];

        return [
            //changes.Added     == 0 ? "Add0".InDarkGreen() : $"Add{changes.Added}".OnDarkGreen(), "/",
            //changes.Staged    == 0 ? "Stg0".InGreen() : $"Stg{changes.Staged}".InDarkGrey().OnGreen(), "/",
            //changes.Removed   == 0 ? "Rem0".InRed() : $"Rem{changes.Removed}".OnRed(), "/",
            //changes.Untracked == 0 ? "Unt0".InDarkRed() : $"Unt{changes.Untracked}".OnDarkRed(), "/",
            //changes.Modified  == 0 ? "Mod0".InRed() : $"Mod{changes.Modified}".OnRed(), "/",
            //changes.Missing   == 0 ? "Mis0".InMagenta() : $"Mis{changes.Missing}".OnMagenta()
            //,

            ..Render(changes.Added,     "Add", FG.DarkGreen, BG.DarkGreen), "/",
            ..Render(changes.Staged,    "Stg", FG.Green, BG.Green + FG.DarkGrey ), "/",
            ..Render(changes.Removed,   "Rem", FG.Red, BG.DarkGreen), "/",
            ..Render(changes.Untracked, "Unt", FG.DarkRed, BG.DarkGreen), "/",
            ..Render(changes.Modified,  "Mod", FG.Red, BG.DarkGreen), "/",
            ..Render(changes.Missing,   "Mis", FG.Magenta, BG.DarkGreen),
        ];
    }

    private static object[] Render(int changeCount, string displayTitle, AnsiCode noChangeFormat, AnsiCode changeFormat)
        => [changeCount == 0 ? noChangeFormat : changeFormat,
            displayTitle + changeCount.ToString(),
            Style.Default];

    public static object[] ToConsole(this GitTracking tracking)
    {
        if (!tracking.IsTracking)
            return [FG.DarkGrey, " ---- ", FG.Default];

        return [
            //$" +{tracking.Ahead}".InYellow(),
            //$"-{tracking.Behind} ".InMagenta(),
            FG.Yellow, $" +{tracking.Ahead}",
            FG.Magenta, $"-{tracking.Behind} ",
            FG.Default
            ];
    }

    public static object[] ToConsole(this MergeResult result) =>
        result.Status switch
        {
            //MergeStatus.UpToDate => ["Already up to date".InDarkGreen()],
            //MergeStatus.FastForward => ["Updated with Fast Forward".InGreen()],
            //MergeStatus.NonFastForward => ["Updated with Non-Fast Forward".InGreen()],
            //MergeStatus.Conflicts => ["Merge Conflicts!".InRed()]
            MergeStatus.UpToDate => [FG.DarkGreen, "Already up to date", FG.Default],
            MergeStatus.FastForward => [FG.Green, "Updated with Fast Forward", FG.Default],
            MergeStatus.NonFastForward => [FG.Green, "Updated with Non-Fast Forward", FG.Default],
            MergeStatus.Conflicts => [FG.Red, "Merge Conflicts!", FG.Default]
        };
}
