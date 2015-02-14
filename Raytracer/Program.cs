using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelWindowCSharp;
using System.Diagnostics;
using System.Threading;

namespace Raytracer
{
    static class Program
    {
        static Sphere middleSphere = new Sphere(new Point3D(0, 0, 0), 40)
        {
            reflectionAmount = .6f,
            color = (ARGBColor)0xFF91D9D1
        };
        static Sphere cutoutSphere = new Sphere(new Point3D(0, 0, -40), 30)
        {
            reflectionAmount = .6f,
            color = (ARGBColor)0xFF910000
        };

        static Scene scene = new Scene
            {
                skyColor = (ARGBColor)0xFFB2FFFF,
                renderedObjects = new Renderable[]
                {
                    //new Sphere(new Point3D(0, 0, 0), 40)
                    //{
                    //    reflectionAmount = .6f,
                    //    color = (ARGBColor)0xFF91D9D1
                    //},
                    middleSphere - cutoutSphere,
                    new YPlane(-40)
                    {
                        reflectionAmount = .3f,
                    }
                },
                camera = new Camera(new Point3D(75, 75, -75), new Point3D(0, 0, 0), Camera.Projection.Perspective)
                {
                    //put them here instead of in the constructor for clarity
                    focalLength = 700,
                    zoom = 1
                },
                options = new Scene.RenderOptions
                {
                    antialiasAmount = 4,
                    lightingEnabled = false,
                    maxReflections = 10,
                    maxRefractions = 10,

                    imageWidth = 1600,
                    imageHeight = 900
                }
            };

        public static void Main(string[] args)
        {
            using (PixelWindow window = new PixelWindow(scene.options.imageWidth, scene.options.imageHeight, "Raytracing"))
            {
                int updateCount = 0;

                Parallel.For(0, window.ClientWidth, delegate(int x)
                {
                    //for (int x = 0; x < window.ClientWidth; x++)
                    //{
                    Parallel.For(0, window.ClientHeight, delegate(int y)
                    {
                        //for (int y = 0; y < window.ClientHeight; y++)
                        //{
                        int rSum = 0;
                        int gSum = 0;
                        int bSum = 0;
                        //long totalColorCalcTime = 0;

                        //Stopwatch stopWatch = new Stopwatch();
                        //Antialiasing
                        for (int subX = 0; subX < scene.options.antialiasAmount; subX++)
                        {
                            for (int subY = 0; subY < scene.options.antialiasAmount; subY++)
                            {
                                Ray ray = scene.camera.RayAtPixel(x + (subX / (float)scene.options.antialiasAmount), y + (subY / (float)scene.options.antialiasAmount), window);

                                //stopWatch.Restart();
                                ARGBColor color = ColorOf(ray, scene.options.maxReflections, scene.options.maxRefractions);
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

                        lock (window)
                        {
                            window[x, y] = combined;
                            //window.UpdateClient();
                        }


                    });

                    //I realize the increment isn't thread safe but I figure it's not that important that it is.
                    if (++updateCount % 32 == 0)
                    {
                        lock (window)
                        {
                            window.UpdateClient();
                        }
                    }
                });
                window.UpdateClient();

                while (!window.IsClosed) ;
            }
        }

        static ARGBColor ColorOf(Ray ray, int reflectionsLeft, int refractionsLeft)
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
                return scene.skyColor;
            }

            //TODO: would using a float for each channel have a significant effect? Probably not, but maybe?
            ARGBColor diffuseColor = closestIntersection.color;
            ARGBColor reflectedColor = (ARGBColor)0x00000000;
            ARGBColor refractedColor = (ARGBColor)0x00000000;

            if (reflectionsLeft > 0 && hitObj.reflectionAmount > 0)
            {
                Vector3D reflectedVector = ray.ToVector3D().Reflected(closestIntersection.normal);
                Ray reflectedRay = new Ray(ray.PointAt(closestIntersection.value), reflectedVector);
                reflectedColor = ColorOf(reflectedRay, --reflectionsLeft, refractionsLeft);

            }
            if (refractionsLeft > 0 && hitObj.refractionAmount > 0)
            {
                //TODO: figure out way to track whether ray is indside or outside, refraction indexes, etc.

                //Vector3D refractedVector = ray.ToVector3D().Refracted(closestIntersection.normal);
                //Ray refractedRay = new Ray(ray.PointAt(closestIntersection.value), refractedVector);
                //refractedColor = ColorOf(refractedRay, reflectionsLeft, --refractionsLeft);
            }

            return new ARGBColor
            {
                red = CombineChannels(diffuseColor.red, reflectedColor.red, refractedColor.red, hitObj),
                green = CombineChannels(diffuseColor.green, reflectedColor.green, refractedColor.green, hitObj),
                blue = CombineChannels(diffuseColor.blue, reflectedColor.blue, refractedColor.blue, hitObj),
                reserved = 0
            };
        }

        static byte CombineChannels(byte diffuse, byte refracted, byte reflected, Renderable obj)
        {
            return Math.Min((byte)0xFF, (byte)((diffuse * obj.DiffuseAmount) + (refracted * obj.reflectionAmount) + (reflected * obj.refractionAmount)));
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

        public static float PMod(this float dividend, float divisor)
        {
            return ((dividend % divisor) + divisor) % divisor;
        }
    }
}
