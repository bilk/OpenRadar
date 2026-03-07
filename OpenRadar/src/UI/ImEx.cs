using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;


namespace OpenRadar;

public static class ImEx
{
    public static void CentreText(string text, Vector4 colour, bool centreY = false)
    {
        var childWindowSize = ImGui.GetContentRegionAvail();
        var textSize = ImGui.CalcTextSize(text);
        ImGui.SetCursorPosX(ImGui.GetCursorPos().X + (childWindowSize.X - textSize.X) / 2);
        if (centreY)
            ImGui.SetCursorPosY((childWindowSize.Y - textSize.Y) / 2);
        Text(text, colour);
    }

    public static void TableColumn(string label, float width)
        => ImGui.TableSetupColumn(label, ImGuiTableColumnFlags.None, ImGuiHelpers.GlobalScale * width);

    public static Vector2 ImageSize(Vector2 size)
        => new(size.X * ImGuiHelpers.GlobalScale, size.Y * ImGuiHelpers.GlobalScale);

    public static void Image(ISharedImmediateTexture texture, Vector2 size)
       => Image(texture.GetWrapOrEmpty(), size);

    public static void Image(IDalamudTextureWrap texture, Vector2 size)
        => ImGui.Image(texture.Handle, ImageSize(size), Vector2.Zero, Vector2.One, Vector4.One with { W = 0.50f });

    public static void HoverToolTip(string message, bool handCursor = false)
    {
        if (!ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) return;
        
        if (handCursor) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        using var ToolTip = ImRaii.Tooltip();

        if (!ToolTip) return;

        Text(message);
    }

    public static void ClickableTextLink(string url)
    {
        if (!ImGui.IsItemClicked()) return;

        Dalamud.Utility.Util.OpenLink(url);
    }

    public static void Text<T>(T payload, Vector4? col = null)
    {
        using var _ = col.HasValue ? ImRaii.PushColor(ImGuiCol.Text, col.Value) : null;
        ImGui.TextUnformatted(payload?.ToString() ?? "-");
    }    
}

public static class Col
{
    public static readonly Vector4 fGrey     = new(0.333f, 0.333f, 0.333f, 1f);
    public static readonly Vector4 fGreen    = new(0.118f, 1f, 0f, 1f);
    public static readonly Vector4 fBlue     = new(0f, 0.439f, 1f, 1f);
    public static readonly Vector4 fPurple   = new(0.639f, 0.208f, 0.933f, 1f);
    public static readonly Vector4 fOrange   = new(1f, 0.502f, 0f, 1f);
    public static readonly Vector4 fPink     = new(0.887f, 0.408f, 0.659f, 1f);
    
    public static readonly Vector4 fGold     = new(0.898f, 0.8f, 0.502f, 1f);
    public static readonly Vector4 LowRed    = new(1f,0f,0f,0.5f);
    public static readonly Vector4 Green     = new(0f,1f,0f,1f);
    public static readonly Vector4 Red       = new (1f, 0f, 0f, 1f);
    public static readonly Vector4 LowGrey   = new(1f, 1f, 1f, 0.25f);
    public static readonly Vector4 Cyan      = new(0.4f, 0.8f, 1f, 1f);
    public static readonly Vector4 Orange    = new (1f, 0.7f, 0.2f, 1f);
}