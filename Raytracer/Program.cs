using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelWindowCSharp;

namespace Raytracer
{
    class Program
    {
        static Scene scene;
        static ARGBColor skyColor;

        public static void Main(string[] args)
        {
            using (PixelWindow window = new PixelWindow(1280, 720, "Raytracing"))
            {
                //TODO: multithreading (Parallel.ForEach?)
                for (int x = 0; x < window.ClientWidth; x++)
                {
                    for (int y = 0; y < window.ClientHeight; y++)
                    {
                        Ray ray = scene.camera.RayAtPixel(x, y, window);

                        ARGBColor color = ColorOf(ray, scene.options.maxReflections, scene.options.maxRefractions);
                        window[x, y] = color;
                    }
                }
            }
        }

        static ARGBColor ColorOf(Ray ray, int reflectionsLeft, int refractionsLeft)
        {
            Renderable.Intersection closestIntersection = Renderable.Intersection.FarthestAway;
            Renderable hitObj = null;
            foreach (Renderable obj in scene.renderedObjects)
            {
                Renderable.Intersection intersection = obj.GetNearestIntersection(ray);
                if (intersection.value < closestIntersection.value)
                {
                    closestIntersection = intersection;
                    hitObj = obj;
                }
            }

            if(hitObj == null)
            {
                //No hit
                return skyColor;
            }

            //TODO: would using a float for each channel have a significant effect? Probably not, but maybe?
            ARGBColor diffuseColor = closestIntersection.color;
            ARGBColor reflectedColor = (ARGBColor)0x00000000;
            ARGBColor refractedColor= (ARGBColor)0x00000000;

            if (reflectionsLeft > 0 && hitObj.reflectionAmount > 0)
            {
                Vector3D reflectedVector = ray.ToVector3D().Reflected(closestIntersection.normal);
                Ray reflectedRay = new Ray(ray.PointAt(closestIntersection.value), reflectedVector);
                reflectedColor = ColorOf(ray, --reflectionsLeft, refractionsLeft);

            }
            if (refractionsLeft > 0 && hitObj.refractionAmount > 0)
            {
                Vector3D refractedVector = ray.ToVector3D().Refracted(closestIntersection.normal);
                Ray refractedRay = new Ray(ray.PointAt(closestIntersection.value), refractedVector);
                refractedColor = ColorOf(ray, reflectionsLeft, --refractionsLeft);
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
    }
}
