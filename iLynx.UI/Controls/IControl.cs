using OpenTK;

namespace iLynx.UI.Controls
{
    public interface IControl : IRenderable
    {
        float Width { get; }
        float Height { get; }
        Vector2d Position { get; }
    }
}
