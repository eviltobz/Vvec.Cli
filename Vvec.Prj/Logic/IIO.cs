namespace Vvec.Prj.Logic;

public interface IIO
{
    string[] GetSubfolders(string path);

    void ReturnPathToShell(string? path);
}
