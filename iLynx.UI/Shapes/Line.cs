using System;
using System.Collections.Generic;
using System.Text;
using iLynx.UI.Shaders;

namespace iLynx.UI.Shapes
{
    public class Line : IShape
    {
        public IEnumerable<IVertexShader> VertexShaders { get; }
        public IEnumerable<IFragmentShader> FragmentShaders { get; }
    }
}
