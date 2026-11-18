using Microsoft.VisualBasic.FileIO;

namespace WitchDrawer.Native.Files;

public static class RecycleBin
{
    public static void Send(string path, bool isDirectory)
    {
        if (isDirectory)
            FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
        else
            FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
    }
}
