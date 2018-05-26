using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace iLynx.UI.Controls
{
    public abstract class Control : IControl
    {
        protected Control()
        {

        }

        public float Width { get; }
        public float Height { get; }
        public Vector2d Position { get; }
    }
}
