namespace Vvec.Prj.Logic;

public interface IOpenDirect
{
    //string? FindPath(string hint, string[] projectFolders, bool overrideRootFolder);
    string? FindPath(string hint, string[] projectFolders, string rootFolder);
}
