
using Dalamud.Interface.Textures.TextureWraps;

namespace OpenRadar;

public static class Util
{
    public static IDalamudTextureWrap? GetJobIcon(uint? jobId)
    {
        if (jobId != null)
        {
            var jobTexture = Svc.Texture.GetFromGame("ui/icon/062000/0621" + jobId + ".tex").GetWrapOrEmpty();
            return jobTexture;
        }
        return null;
    }
}