using System;

namespace iLynx.TestBench.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class AspMvcAreaMasterLocationFormatAttribute : Attribute
    {
        public AspMvcAreaMasterLocationFormatAttribute(string format) { }
    }
}