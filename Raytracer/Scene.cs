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


        static Scene scene = new Scene
        {
            skyColor = new Color(154, 206, 235),
            renderedObjects = new List<Renderable>
                {
                    //coolCubeThing,
                    //regularSphere,
                    //new YCylinder(Point3D.Zero, 200, 800, (Color)0x545454),
                    //cube1,
                    //cube2,
                    sphere2,
                    //shinySphere,
                    plane,
                    new RotatedObject(refractiveCuboid, Point3D.Zero, 0, -.4)
                    //new Sphere(Point3D.Zero, 1000, Color.Blue) { refractivity = 0.9, refractionIndex = 1},
                    //new Cuboid(Point3D.Zero, 7000, 7000, 7000, Color.White) - new Cuboid(Point3D.Zero, 6000, 6000, 6000, Color.White),
                },
            lightSources = new LightSource[]
            {
                new LightSource(new Point3D( 1000, 3000,  1000), 10000),
                new LightSource(new Point3D(-1000, 3000,  1000), 10000),
                new LightSource(new Point3D( 1000, 3000, -1000), 10000),
                new LightSource(new Point3D(-1000, 3000, -1000), 10000)
            },
            camera = new Camera(new Point3D(1800 / 1.3, 1600 / 1.3, -3200 / 1.3), new Point3D(0, 0, 0))
            {
                zoom = 1000
            },
            options = new Scene.RenderOptions
            {
                antialiasAmount = 1,
                parallelRendering = false,
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
