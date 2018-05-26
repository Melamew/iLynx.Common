using System.Collections.Generic;
using iLynx.UI.Shaders;

namespace iLynx.UI.Shapes
{
    public interface IShape : IRenderable
    {
        IEnumerable<IVertexShader> VertexShaders { get; }

        IEnumerable<IFragmentShader> FragmentShaders { get; }
    }
}
