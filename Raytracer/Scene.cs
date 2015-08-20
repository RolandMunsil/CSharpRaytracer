using PixelWindowCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    /// <summary>
    /// Contains all information necessary to render an image.
    /// </summary>
    class Scene
    {
        //Should these even be in a struct? I think maybe im overcomplicating things.
        public struct RenderOptions
        {
            public int antialiasAmount;
            public bool parallelRendering;
            public bool lightingEnabled;
            public float ambientLight;
            public int maxReflections;
            public int maxRefractions;
            public int imageWidth;
            public int imageHeight;
        }
        public ARGBColor skyColor;
        public Renderable[] renderedObjects;
        public LightSource[] lightSources;
        public Camera camera;
        public RenderOptions options;
    }
}
