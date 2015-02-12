using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Scene
    {
        //Should these even be in a struct? I think maybe im overcomplicating things.
        public struct RenderOptions
        {
            public int antialiasAmount;
            public bool lightingEnabled;
            public int maxReflections;
            public int maxRefractions;
        }

        public Renderable[] renderedObjects;
        public Camera camera;
        public RenderOptions options;


    }
}
