﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelWindowSDL;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Raytracer
{
    static partial class Program
    {
        public static void Main(string[] args)
        {
            //for(int i =0; i < 60; i++)
            //{
            //    scene.renderedObjects.Add(new Sphere((Point3D)(Vector3D.RandomUnitVector() * 1000), 20, Color.Red));
            //}

            //double sphereRadius = 100;
            //int numSpheres = 40;
            //Random rand = new Random();

            //Point3D artCenter = Point3D.Zero;

            //Vector3D prevDiff = Vector3D.RandomUnitVector();
            //Point3D prevCenter = Point3D.Zero;

            //for (int i = 0; i < numSpheres; i++)
            //{
            //    Vector3D newDiff = sphereRadius * 2 * Vector3D.RandomUnitVectorFrom(prevDiff, Math.PI / 2);
            //    Point3D newCenter = prevCenter + newDiff;

            //    Color c = Color.White;
            //    if (rand.Next(10) == 0) c = new Color(255, 128, 128);
            //    //if (rand.Next(20) == 0) c = new Color(192, 192, 255);

            //    scene.renderedObjects.Add(new Sphere(newCenter, sphereRadius, c));

            //    artCenter += ((Vector3D)newCenter) / (double)numSpheres;

            //    prevDiff = newDiff;
            //    prevCenter = newCenter;
            //}

            //foreach(Renderable r in scene.renderedObjects)
            //{
            //    if(r is Sphere)
            //    {
            //        ((Sphere)r).center -= (Vector3D)artCenter;
            //    }
            //}


            InitializeScene();

            using (PixelWindow window = new PixelWindow(scene.options.imageWidth, scene.options.imageHeight, true))
            {
                bool cameraIsInsideObject = scene.renderedObjects.Any(obj => obj.Contains(scene.camera.position));
                int frameCount = 0;

                String animationDirectory = "";
                if (scene.animationOptions.saveAnimation)
                {
                    int i = 0;
                    while (Directory.Exists(scene.animationOptions.animationsBasePath + "Animation " + ++i)) ;
                    animationDirectory = "Animation " + i;
                    Directory.CreateDirectory(scene.animationOptions.animationsBasePath + animationDirectory);
                }

                do
                {
                    if (scene.animationOptions.doAnimation && scene.animationOptions.animationFunction != null)
                    {
                        scene.animationOptions.animationFunction(frameCount, (frameCount / (double)scene.animationOptions.animationFrameCount));
                        frameCount++;
                    }

                    int verticalLinesRendered = 0;

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    if (scene.options.parallelRendering)
                    {
                        Parallel.For(0, window.ClientWidth, delegate(int x, ParallelLoopState xState)
                        {
                            //if (window.IsClosed)
                            //    xState.Stop();

                            for (int y = 0; y < window.ClientHeight; y++)
                            {
                                Color pixelColor = CalculatePixelColor(x, y, window, cameraIsInsideObject);
                                //TODO: queue with separate thread?
                                lock(window)
                                {
                                    window[x, y] = pixelColor;
                                }
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

                            if (++verticalLinesRendered % 16 == 0)
                            {
                                window.UpdateClient();
                            }
                        }
                    }

                    stopWatch.Stop();

                    window.UpdateClient();

                    //ffmpeg -f image2 -framerate 30 -loop 1 -t 00:00:20 -i images4/%03d.png -vcodec libx265 -crf 0 images4.avi

                    if(scene.animationOptions.saveAnimation)
                    {
                        window.SaveClientToPNG(scene.animationOptions.animationsBasePath + animationDirectory + "/" + frameCount.ToString("000") + ".png");

                        if (frameCount == scene.animationOptions.animationFrameCount)
                        {
                            return;
                        }
                    }

                } while (scene.animationOptions.doAnimation);

                while (window.IsOpen);
            }
        }

        private static Color CalculatePixelColor(int x, int y, PixelWindow window, bool cameraIsInsideObject)
        {
            if (x == 1200 && y == 456)
            {
                //return Color.Red;
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
                    Color color = ColorOf(ray, scene.options.maxReflections, scene.options.maxRefractions, cameraIsInsideObject);
                    //stopWatch.Stop();
                    //checked { totalColorCalcTime += stopWatch.ElapsedTicks; }
                    rSum += color.red;
                    gSum += color.green;
                    bSum += color.blue;
                }
            }

            //long avgTime = (totalColorCalcTime * 2 / (scene.options.antialiasAmount * scene.options.antialiasAmount));

            Color combined = new Color
            {
                red = (byte)(rSum / (scene.options.antialiasAmount * scene.options.antialiasAmount)),
                green = (byte)(gSum / (scene.options.antialiasAmount * scene.options.antialiasAmount)),
                blue = (byte)(bSum / (scene.options.antialiasAmount * scene.options.antialiasAmount)),
                alpha = 255
            };
            //{
            //    red = (byte)avgTime,
            //    green = (byte)avgTime,
            //    blue = (byte)avgTime
            //};
            return combined;
        }

        static Color ColorOf(Ray ray, int reflectionsLeft, int refractionsLeft, bool rayIsInObject)
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
                if(scene.options.lightingEnabled)
                {
                    double brightness = scene.options.ambientLight;
                    return new Color()
                    {
                        red = (byte)(scene.skyColor.red * brightness),
                        green = (byte)(scene.skyColor.green * brightness),
                        blue = (byte)(scene.skyColor.blue * brightness)
                    };
                }
                else
                {
                    return scene.skyColor;
                }
            }

            Point3D hitPoint = ray.PointAt(closestIntersection.value);


            double totalLight;

            if (scene.options.lightingEnabled)
            {
                double ambientLight = scene.options.ambientLight;
                double lambertianShadingLight = 0;

                foreach (LightSource source in scene.lightSources)
                {
                    Vector3D vecToLight = (source.position - hitPoint);
                    if (vecToLight.LengthSquared > source.maxLitDistance * source.maxLitDistance)
                    {
                        //No light from this light source
                        continue;
                    }
                    Ray rayToLight = new Ray(hitPoint, vecToLight);

                    //bool lightIsBlocked = false;
                    double lightLetThroughAmount = 1;
                    foreach (Renderable obj in scene.renderedObjects)
                    {
                        Renderable.Intersection intersection = obj.GetNearestIntersection(rayToLight);
                        if (intersection != Renderable.Intersection.None && intersection.value <= 1)
                        {
                            lightLetThroughAmount *= obj.refractivity;
                            //lightIsBlocked = true;
                        }
                    }
                    //if (lightIsBlocked)
                    //{
                    //    //No light from this light source
                    //    continue;
                    //}

                    Vector3D n = closestIntersection.normal.Normalized();
                    Vector3D l = vecToLight.Normalized();

                    if (l.AngleTo(n) > Math.PI / 2)
                    {
                        n = -n;
                    }

                    double thisLambertianShadingLight = ((1 - scene.options.ambientLight) / scene.lightSources.Length) * Math.Max(0, Vector3D.DotProduct(n, l));

                    //Less light farther away
                    double distFrac = (vecToLight.Length / source.maxLitDistance);
                    double multiplier = Math.Min(1, 1f - (distFrac * distFrac));
                    thisLambertianShadingLight *= multiplier;
                    thisLambertianShadingLight *= lightLetThroughAmount;

                    lambertianShadingLight += thisLambertianShadingLight;
                }
                totalLight = Math.Min(1, ambientLight + lambertianShadingLight);
            }
            else
            {
                totalLight = 1;
            }

            //if litAmount is less than 1/255 the color will always end up being black anyway.
            if (totalLight < (1 / 255f))
            {
                return Color.Black;
            }

            //TODO: would using a double for each channel have a significant effect? Probably not, but maybe?
            Color diffuseColor = closestIntersection.color;
            Color reflectedColor = Color.Black;
            Color refractedColor = Color.Black;

            if (reflectionsLeft > 0 && hitObj.reflectivity > 0)
            {
                Vector3D reflectedVector = ray.Direction.Reflected(closestIntersection.normal);
                Ray reflectedRay = new Ray(hitPoint, reflectedVector);
                reflectedColor = ColorOf(reflectedRay, --reflectionsLeft, refractionsLeft, rayIsInObject);

            }
            if (refractionsLeft > 0 && hitObj.refractivity > 0)
            {
                double refractIndexFrom = rayIsInObject ? hitObj.refractionIndex : RefractionIndexes.Air;
                double refractIndexTo = rayIsInObject ? RefractionIndexes.Air : hitObj.refractionIndex;

                bool totalInternalReflection;
                Vector3D refractedVector = ray.Direction.Refracted(closestIntersection.normal, refractIndexFrom, refractIndexTo, out totalInternalReflection);
                //Ray refractedRay = new Ray(hitPoint, Vector3D.RandomUnitVectorFrom(refractedVector, Math.PI / 40));
                Ray refractedRay = new Ray(hitPoint, refractedVector);
                refractedColor = ColorOf(refractedRay, reflectionsLeft, --refractionsLeft, (!rayIsInObject) ^ totalInternalReflection);
            }

            return new Color
            {
                red = CombineChannels(diffuseColor.red, reflectedColor.red, refractedColor.red, hitObj, totalLight),
                green = CombineChannels(diffuseColor.green, reflectedColor.green, refractedColor.green, hitObj, totalLight),
                blue = CombineChannels(diffuseColor.blue, reflectedColor.blue, refractedColor.blue, hitObj, totalLight),
            };
        }

        static byte CombineChannels(byte diffuse, byte reflected, byte refracted, Renderable obj, double litAmount)
        {
            //TODO: do some sort of L*a*b* transormation instead of just multiplying it?
            return Math.Min((byte)0xFF, (byte)((diffuse * obj.DiffuseAmount * litAmount) + (reflected * obj.reflectivity * litAmount) + (refracted * obj.refractivity)));
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

        public static Scene.AnimationFunction RotateAround()
        {
            Point3D initialCameraPos = scene.camera.position;
            double y = initialCameraPos.y;
            double radius = Math.Sqrt(initialCameraPos.x * initialCameraPos.x + initialCameraPos.z * initialCameraPos.z);
            return delegate(int frameCount, double animationDoneAmount)
            {
                double angle = animationDoneAmount * Math.PI * 2;
                scene.camera.ChangePositionAndLookingAt(new Point3D(Math.Sin(angle) * radius, y, Math.Cos(angle) * radius), Point3D.Zero);
            };
        }
    }
}
