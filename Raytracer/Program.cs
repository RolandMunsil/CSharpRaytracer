﻿using System;
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
            reflectivity = .6f,
            color = (ARGBColor)0xFF91D9D1
        };
        static Sphere cutoutSphere = new Sphere(new Point3D(0, 20, -20), 30)
        {
            reflectivity = .6f,
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
                        reflectivity = .3f,
                    }
                },
                lightSources = new LightSource[]
                {
                    new LightSource
                    {
                        position = new Point3D(50, 50, 0),
                        maxLitDistance = 300
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
                    lightingEnabled = true,
                    ambientLight = 0.3f,
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
                //for (int frame = 1; frame < 32; frame++)
                //{
                //double angle = (frame / (double)240) * (Math.PI * 2);
                //scene.camera = new Camera(new Point3D(107 * (float)Math.Sin(angle), 75, 107 * (float)Math.Cos(angle)), new Point3D(0, 0, 0), Camera.Projection.Perspective)
                //{
                //    //put them here instead of in the constructor for clarity
                //    focalLength = 700,
                //    zoom = 1
                //};

                int updateCount = 0;

                //Stopwatch sw = new Stopwatch();
                //sw.Start();

                Parallel.For(0, window.ClientWidth, delegate(int x, ParallelLoopState xState)
                {
                //for (int x = 0; x < window.ClientWidth; x++)
                //{
                    if (window.IsClosed)
                    {
                        xState.Stop();
                    }

                    Parallel.For(0, window.ClientHeight, delegate(int y, ParallelLoopState yState)
                    {
                    //for (int y = 0; y < window.ClientHeight; y++)
                    //{
                        if (window.IsClosed)
                        {
                            yState.Stop();
                        }

                            
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

                        window[x, y] = combined;


                    });

                    //I realize the increment isn't thread safe but I figure it's not that important that it is.
                    if (++updateCount % 16 == 0)
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
                return scene.options.lightingEnabled ? (ARGBColor)0x00000000 : scene.skyColor;
            }

            Point3D hitPoint = ray.PointAt(closestIntersection.value);

            float litAmount = 1f;
            if (scene.options.lightingEnabled)
            {
                litAmount = scene.options.ambientLight;

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
                        if (intersection != Renderable.Intersection.None && intersection.value < 1)
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
                    else
                    {
                        //Less light farther away
                        litAmount += 1f - (vecToLight.Length / source.maxLitDistance);

                        if (litAmount >= 1f) //Can't get any brighter
                        {
                            litAmount = 1f;
                            break;
                        }
                    }
                }
            }

            //if litAmount is less than 1/255 the color will always end up being black anyway.
            if (litAmount < (1 / 255f))
            {
                return ARGBColor.Black;
            }

            //TODO: would using a float for each channel have a significant effect? Probably not, but maybe?
            ARGBColor diffuseColor = closestIntersection.color;
            ARGBColor reflectedColor = (ARGBColor)0x00000000;
            ARGBColor refractedColor = (ARGBColor)0x00000000;

            if (reflectionsLeft > 0 && hitObj.reflectivity > 0)
            {
                Vector3D reflectedVector = ray.ToVector3D().Reflected(closestIntersection.normal);
                Ray reflectedRay = new Ray(hitPoint, reflectedVector);
                reflectedColor = ColorOf(reflectedRay, --reflectionsLeft, refractionsLeft);

            }
            if (refractionsLeft > 0 && hitObj.refractivity > 0)
            {
                //TODO: figure out way to track whether ray is indside or outside, refraction indexes, etc.

                //Vector3D refractedVector = ray.ToVector3D().Refracted(closestIntersection.normal);
                //Ray refractedRay = new Ray(hitPoint, refractedVector);
                //refractedColor = ColorOf(refractedRay, reflectionsLeft, --refractionsLeft);
            }

            return new ARGBColor
            {
                red = CombineChannels(diffuseColor.red, reflectedColor.red, refractedColor.red, hitObj, litAmount),
                green = CombineChannels(diffuseColor.green, reflectedColor.green, refractedColor.green, hitObj, litAmount),
                blue = CombineChannels(diffuseColor.blue, reflectedColor.blue, refractedColor.blue, hitObj, litAmount),
                reserved = 0
            };
        }

        static byte CombineChannels(byte diffuse, byte refracted, byte reflected, Renderable obj, float litAmount)
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

        public static float PMod(this float dividend, float divisor)
        {
            return ((dividend % divisor) + divisor) % divisor;
        }
    }
}
