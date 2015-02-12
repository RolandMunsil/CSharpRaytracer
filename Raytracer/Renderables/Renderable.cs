using PixelWindowCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    abstract class Renderable
    {
        //TODO: should this be precalculated or figured out once object and value is calculated?
        public struct Intersection
        {
            public float value;
            public ARGBColor color;
            public Vector3D normal;

            public Intersection FarthestAway
            {
                get
                {
                    return new Intersection { value = Single.MaxValue };
                }
            }
        }

        public float refractionAmount;
        public float reflectionAmount;
        public float DiffuseAmount
        {
            get
            {
                return 1 - (refractionAmount + reflectionAmount);
            }
        }

        public abstract Intersection GetNearestIntersection(Ray ray);
    }
}
