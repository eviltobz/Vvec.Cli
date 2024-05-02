using System.IO.MemoryMappedFiles;
using System.Text;

namespace Vvec.Prj.Logic;

public class IO : IIO
{
    public string[] GetSubfolders(string path) =>
        Directory.GetDirectories(path);

    public void ReturnPathToShell(string? path)
    {
        using var file = MemoryMappedFile.OpenExisting("Vvec.Prj", MemoryMappedFileRights.ReadWrite);
        using var accessor = file.CreateViewAccessor();
        var bytes = path is null ? new byte[0] : Encoding.UTF8.GetBytes(path);
        var length = (Int16)bytes.Length;
        accessor.Write(0, length);
        accessor.WriteArray(2, bytes, 0, bytes.Length);
    }

}
