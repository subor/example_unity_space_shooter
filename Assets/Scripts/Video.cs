using System;
using System.IO;

public class Video : Panel
{
    public void UploadVideo()
    {
        String[] extensions = { "mp4" };
        Browser.OpenFile(@".\", extensions, OnFileSelected);
    }

    private void OnFileSelected(string localPath)
    {
        var filename = Path.GetFileName(localPath);
        RuyiNet.VideoService.UploadVideo(RuyiNet.ActivePlayerIndex, filename, localPath, null);
    }

    public Browser Browser;
}
