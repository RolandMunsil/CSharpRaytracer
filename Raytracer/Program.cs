using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelWindowCSharp;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Raytracer
{
    static class Program
    {
        static ARGBColor niceBlue =   new ARGBColor(26, 128, 255);
        static ARGBColor niceYellow = new ARGBColor(255, 240, 26);
        static ARGBColor niceRed =    new ARGBColor(255, 76, 26);
        static ARGBColor niceGreen =  new ARGBColor(76, 255, 26);
        static ARGBColor goodGray =   new ARGBColor(196, 196, 196);

        static CSGObject coolCubeThing = (new Cuboid(Point3D.Zero, 800, 800, 800, niceBlue)
                                  {
                                      reflectivity = 0.1
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

        static YPlane plane = new YPlane(-400.01f)
        {
            reflectivity = .7f,
        };

        static Sphere regularSphere = new Sphere(Point3D.Zero, 400, niceGreen);

        static Scene scene = new Scene
            {
                skyColor = new ARGBColor(154, 206, 235),
                renderedObjects = new Renderable[]
                {
                    coolCubeThing,
                    regularSphere,
                    plane
                },
                lightSources = new LightSource[]
                {
                    new LightSource
                    {
                        position = new Point3D(0, 1000, -1000),
                        maxLitDistance = 3000
                    }
                },
                camera = new Camera(new Point3D(500, 900, -1600), new Point3D(0, 0, 0), Camera.Projection.Perspective)
                {
                    //put them here instead of in the constructor for clarity
                    focalLength = 100,
                    zoom = 10f
                },
                options = new Scene.RenderOptions
                {
                    antialiasAmount = 1,
                    parallelRendering = true,
                    lightingEnabled = true,
                    ambientLight = 0.5f,
                    maxReflections = 16,
                    maxRefractions = 16,

                    imageWidth = 1600,
                    imageHeight = 900,

                    animationFunction = delegate(int frameCount)
                    {

                        //double angle = (Math.PI * 2) * (frameCount / (double)(10.0 * 30));
                        //scene.camera.ChangePositionAndLookingAt(new Point3D(1700 * Math.Cos(angle), 900, 1700 * Math.Sin(angle)), new Point3D(0, 0, 0));

                        //double angle = (Math.PI * 2) * (frameCount / 10.0);
                        //scene.camera.ChangePositionAndLookingAt(new Point3D(0, Math.Sin(angle) * 1600, Math.Cos(angle) * 1600), new Point3D(0, 0, 0));
                    }
                }
            };

        public static double airRefractIndex = 1f;

        public static void Main(string[] args)
        {
            //Directory.CreateDirectory("images");

            using (PixelWindow window = new PixelWindow(scene.options.imageWidth, scene.options.imageHeight, "Raytracing"))
            {
                bool cameraIsInsideObject = scene.renderedObjects.Any(obj => obj.Contains(scene.camera.position));
                int frameCount = 0;

                do
                {
                    if (scene.options.animationFunction != null)
                    {
                        scene.options.animationFunction(frameCount++);
                    }

                    int verticalLinesRendered = 0;

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    if (scene.options.parallelRendering)
                    {
                        Parallel.For(0, window.ClientWidth, delegate(int x, ParallelLoopState xState)
                        {
                            if (window.IsClosed)
                                xState.Stop();

                            for (int y = 0; y < window.ClientHeight; y++)
                            {
                                window[x, y] = CalculatePixelColor(x, y, window, cameraIsInsideObject);
                            }

                            //I realize the increment isn't thread safe but I figure it's not that important that it is - these updates are purely to make the render less boring to watch
                            if (++verticalLinesRendered % 64 == 0)
                            {
                                lock (window)
                                {
                                    window.UpdateClient();
                                }
                            }
                        });
                    }
                    else
                    {
                        for (int x = 0; x < window.ClientWidth; x++)
                        {
                            for (int y = 0; y < window.ClientHeight; y++)
                            {
                                window[x, y] = CalculatePixelColor(x, y, window, cameraIsInsideObject);
                            }

                            if (++verticalLinesRendered % 64 == 0)
                            {
                                window.UpdateClient();
                            }
                        }
                    }

                    stopWatch.Stop();

                    window.UpdateClient();

                    //ffmpeg -f image2 -framerate 30 -i %03d.png -vcodec libx264 foo.avi
                    //window.BackBuffer.Save("images/" + frameCount.ToString("000") + ".png");

                    //if (frameCount == 300)
                    //{
                    //    break;
                    //}

                } while (scene.options.animationFunction != null);

                while (!window.IsClosed);
            }
        }

        private static ARGBColor CalculatePixelColor(int x, int y, PixelWindow window, bool cameraIsInsideObject)
        {
            if (x == 800 && y == 456)
            {
                //return ARGBColor.Red;
                //Debugger.Break();
            }

            //long totalColorCalcTime = 0;
            //Stopwatch stopWatch = new Stopwatch();

            int rSum = 0;
            int gSum = 0;
            int bSum = 0;

            for (int subX = 0; subX < scene.options.antialiasAmount; subX++)
            {
                for (int subY = 0; subY < scene.options.antialiasAmount; subY++)
                {
                    Ray ray = scene.camera.RayAtPixel(x + (subX / (double)scene.options.antialiasAmount), y + (subY / (double)scene.options.antialiasAmount), window);

                    //stopWatch.Restart();
                    ARGBColor color = ColorOf(ray, scene.options.maxReflections, scene.options.maxRefractions, cameraIsInsideObject);
                    //stopWatch.Stop();
                    //checked { totalColorCalcTime += stopWatch.ElapsedTicks; }
                    rSum += color.red;
                    gSum += color.green;
                    bSum += color.blue;
                }
            }

            //long avgTime = (totalColorCalcTime * 2 / (scene.options.antialiasAmount * scene.options.antialiasAmount));

            ARGBColor combined = new ARGBColor
            {
                red = (byte)(rSum / (scene.options.antialiasAmount * scene.options.antialiasAmount)),
                green = (byte)(gSum / (scene.options.antialiasAmount * scene.options.antialiasAmount)),
                blue = (byte)(bSum / (scene.options.antialiasAmount * scene.options.antialiasAmount)),
                reserved = 0
            };
            //{
            //    red = (byte)avgTime,
            //    green = (byte)avgTime,
            //    blue = (byte)avgTime
            //};
            return combined;
        }

        static ARGBColor ColorOf(Ray ray, int reflectionsLeft, int refractionsLeft, bool rayIsInObject)
        {
            Renderable.Intersection closestIntersection = Renderable.Intersection.FarthestAway;
            Renderable hitObj = null;
            foreach (Renderable obj in scene.renderedObjects)
            {
                Renderable.Intersection intersection = obj.GetNearestIntersection(ray);
                if (intersection != Renderable.Intersection.None && intersection.value < closestIntersection.value)
                {
                    closestIntersection = intersection;
                    hitObj = obj;
                }
            }

            if(hitObj == null)
            {
                //No hit
                return scene.options.lightingEnabled ? (ARGBColor)0x00000000 : scene.skyColor;
            }

            Point3D hitPoint = ray.PointAt(closestIntersection.value);


            double ambientLight = scene.options.ambientLight;
            double lambertianShadingLight = 0;

            if (scene.options.lightingEnabled)
            {
                foreach (LightSource source in scene.lightSources)
                {
                    Vector3D vecToLight = (source.position - hitPoint);
                    if (vecToLight.LengthSquared > source.maxLitDistance * source.maxLitDistance)
                    {
                        //No light from this light source
                        continue;
                    }
                    Ray rayToLight = new Ray(hitPoint, vecToLight);

                    bool lightIsBlocked = false;
                    foreach (Renderable obj in scene.renderedObjects)
                    {
                        Renderable.Intersection intersection = obj.GetNearestIntersection(rayToLight);
                        if (intersection != Renderable.Intersection.None && intersection.value <= 1)
                        {
                            lightIsBlocked = true;
                            break;
                        }
                    }
                    if (lightIsBlocked)
                    {
                        //No light from this light source
                        continue;
                    }

                    Vector3D n = closestIntersection.normal.Normalized();
                    Vector3D l = vecToLight.Normalized();

                    if (l.AngleTo(n) > Math.PI / 2)
                    {
                        n = -n;
                    }

                    lambertianShadingLight = Math.Max(0, Vector3D.DotProduct(n, l));

                    //Less light farther away
                    double distFrac = (vecToLight.Length / source.maxLitDistance);
                    double multiplier = Math.Min(1, 1f - (distFrac * distFrac));
                    lambertianShadingLight *= multiplier;
                }
            }

            double totalLight = Math.Min(1, ambientLight + lambertianShadingLight);

            //if litAmount is less than 1/255 the color will always end up being black anyway.
            if (totalLight < (1 / 255f))
            {
                return ARGBColor.Black;
            }

            //TODO: would using a double for each channel have a significant effect? Probably not, but maybe?
            ARGBColor diffuseColor = closestIntersection.color;
            ARGBColor reflectedColor = (ARGBColor)0x00000000;
            ARGBColor refractedColor = (ARGBColor)0x00000000;

            if (reflectionsLeft > 0 && hitObj.reflectivity > 0)
            {
                Vector3D reflectedVector = ray.Direction.Reflected(closestIntersection.normal);
                Ray reflectedRay = new Ray(hitPoint, reflectedVector);
                reflectedColor = ColorOf(reflectedRay, --reflectionsLeft, refractionsLeft, rayIsInObject);

            }
            if (refractionsLeft > 0 && hitObj.refractivity > 0)
            {
                double refractIndexFrom = rayIsInObject ? hitObj.refractionIndex : airRefractIndex;
                double refractIndexTo = rayIsInObject ? airRefractIndex : hitObj.refractionIndex;

                bool totalInternalReflection;
                Vector3D refractedVector = ray.Direction.Refracted(closestIntersection.normal, refractIndexFrom, refractIndexTo, out totalInternalReflection);
                Ray refractedRay = new Ray(hitPoint, refractedVector);
                refractedColor = ColorOf(refractedRay, reflectionsLeft, --refractionsLeft, (!rayIsInObject) ^ totalInternalReflection);
            }

            return new ARGBColor
            {
                red = CombineChannels(diffuseColor.red, reflectedColor.red, refractedColor.red, hitObj, totalLight),
                green = CombineChannels(diffuseColor.green, reflectedColor.green, refractedColor.green, hitObj, totalLight),
                blue = CombineChannels(diffuseColor.blue, reflectedColor.blue, refractedColor.blue, hitObj, totalLight),
                reserved = 0
            };
        }

        static byte CombineChannels(byte diffuse, byte refracted, byte reflected, Renderable obj, double litAmount)
        {
            //TODO: do some sort of L*a*b* transormation instead of just multiplying it?
            return Math.Min((byte)0xFF, (byte)((diffuse * obj.DiffuseAmount * litAmount) + (refracted * obj.reflectivity * litAmount) + (reflected * obj.refractivity * litAmount)));
        }

        public static Renderable.Intersection Nearest(this IList<Renderable.Intersection> intersections)
        {
            if (intersections.Count < 3)
            {
                if (intersections.Count == 1)
                {
                    return intersections[0];
                }
                if (intersections.Count == 2)
                {
                    return intersections[0].value < intersections[1].value ? intersections[0] : intersections[1];
                }
                else //intersections.Length == 0
                {
                    return Renderable.Intersection.None;
                }
            }
            else
            {
                Renderable.Intersection nearest = intersections.ElementAt(0);
                foreach (Renderable.Intersection i in intersections)
                {
                    if (i.value < nearest.value)
                    {
                        nearest = i;
                    }
                }
                return nearest;
            }
        }

        public static double PMod(this double dividend, double divisor)
        {
            return ((dividend % divisor) + divisor) % divisor;
        }
    }
}
