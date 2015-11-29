using PixelWindowSDL;
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
        /// <summary>
        /// A function that is called before every frame.
        /// </summary>
        public delegate void AnimationFunction(int frameCount, double animationDoneAmount);

        public struct RenderOptions
        {
            public int antialiasAmount;
            public bool parallelRendering;
            public bool lightingEnabled;
            public double ambientLight;
            public int maxReflections;
            public int maxRefractions;
            public int imageWidth;
            public int imageHeight;
        }
        public struct AnimationOptions
        {
            public bool doAnimation;
            public AnimationFunction animationFunction;
            public bool saveAnimation;
            public int animationFrameCount;
            public String animationsBasePath;
        }

        public Color skyColor;
        public IList<Renderable> renderedObjects;
        public LightSource[] lightSources;
        public Camera camera;
        public RenderOptions options;
        public AnimationOptions animationOptions;
    }

    static partial class Program
    {
        public static class RefractionIndexes
        {
            public const double Air = 1;
            public const double Water = 1.333;
            public const double Glass = 1.51;
        }

        static Color niceBlue = new Color(26, 128, 255);
        static Color niceYellow = new Color(255, 240, 26);
        static Color niceRed = new Color(255, 76, 26);
        static Color niceGreen = new Color(76, 255, 26);
        static Color goodGray = new Color(196, 196, 196);

        static CSGObject coolCubeThing = (new Cuboid(Point3D.Zero, 800, 800, 800, niceBlue)
        {
            refractivity = 0.9,
            refractionIndex = 1.51
        } -
                                  (new Sphere(new Point3D(0, 0, 0), 500, niceBlue) |
                                    new Sphere(new Point3D(400, 400, -400), 192.820323027550917, niceBlue) |
                                    new Sphere(new Point3D(-400, 400, -400), 192.820323027550917, niceBlue) |
                                    new Sphere(new Point3D(400, -400, -400), 192.820323027550917, niceBlue) |
                                    new Sphere(new Point3D(-400, -400, -400), 192.820323027550917, niceBlue) |
                                    new Sphere(new Point3D(400, 400, 400), 192.820323027550917, niceBlue) |
                                    new Sphere(new Point3D(-400, 400, 400), 192.820323027550917, niceBlue) |
                                    new Sphere(new Point3D(400, -400, 400), 192.820323027550917, niceBlue)
                                  ));

        static Sphere shinySphere = new Sphere(new Point3D(600, -300, -600), 100, (Color)0x545454) { reflectivity = 0.7 };

        static List<Renderable> wineGlassObjects = new List<Renderable>
        {
            //Table
            new YPlane(-500.01),
            //Wine glass
            new Sphere(Point3D.Zero, 500, Color.Black) { reflectivity = 0.0, refractivity = 0.9, refractionIndex = RefractionIndexes.Glass }
                - new Sphere(Point3D.Zero, 490, Color.White)
                - new Cuboid(new Point3D(0, 500, 0), 1500, 400, 1500, Color.White),
            //Wine
            new Sphere(Point3D.Zero, 489, new Color(0x660000)) {refractivity = 0.7, refractionIndex = RefractionIndexes.Water}
                - new Cuboid(new Point3D(0, 500, 0), 1500, 1200, 1500, new Color(0x660000)),
        };

        static Scene scene = new Scene
        {
            skyColor = new Color(154, 206, 235),
            renderedObjects = wineGlassObjects,
            lightSources = new LightSource[]
            {
                //new LightSource(new Point3D( 1000, 3000,  1000), 10000),
                //new LightSource(new Point3D(-1000, 3000,  1000), 10000),
                //new LightSource(new Point3D( 1000, 3000, -1000), 10000),
                new LightSource(new Point3D(-1000, 3000, -1000), 10000)
            },
            camera = new Camera(new Point3D(1400, 1000, -2500), new Point3D(0, 0, 0))
            {
                zoom = 1700
            },
            options = new Scene.RenderOptions
            {
                antialiasAmount = 1,
                parallelRendering = true,
                lightingEnabled = true,
                ambientLight = 0.5f,
                maxReflections = 16,
                maxRefractions = 16,

                imageWidth = 900,
                imageHeight = 900
            },
            animationOptions = new Scene.AnimationOptions
            {
                doAnimation = false,
                animationFunction = null, //Defaults to RotateAround (TODO: figure out less hacky way to do this)
                saveAnimation = false,
                animationsBasePath = "../../../Renders/",
                animationFrameCount = 90
            }
        };

    }
}
