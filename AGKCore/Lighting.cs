using System.Runtime.InteropServices;
using AgkSharp;

namespace AGKCore.Lighting
{
    namespace Basic
    {
        public class DirectionalLight
        {
            public AGKVector4 Ambient;
            public AGKVector4 Diffuse;
            public AGKVector4 Specular;
            public AGKVector3 Direction;
        }
    }
}
