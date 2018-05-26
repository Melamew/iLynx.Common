using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using iLynx.Common;
using OpenTK.Graphics.OpenGL4;

namespace iLynx.UI.Shaders
{
    public abstract class ShaderBase : IShader
    {
        private readonly ShaderType shaderType;
        private readonly string source;

        protected ShaderBase(ShaderType shaderType, string source)
        {
            this.shaderType = shaderType;
            this.source = Guard.IsNull(() => source);
        }

        public virtual int Compile()
        {
            var glShader = GL.CreateShader(shaderType);
            GL.ShaderSource(glShader, source);
            GL.CompileShader(glShader);
            return glShader;
        }
    }
    public class VertexShader : ShaderBase
    {
        public VertexShader(string source) : base(ShaderType.VertexShader, source) { }
    }
}
