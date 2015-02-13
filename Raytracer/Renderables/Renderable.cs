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

            public static Intersection FarthestAway
            {
                get
                {
                    return new Intersection { value = Single.MaxValue };
                }
            }

            public static Intersection None
            {
                get
                {
                    return new Intersection
                    {
                        value = Single.NaN,
                        color = (ARGBColor)0xDEADBEEF,
                        normal = new Vector3D(Single.NaN, Single.NaN, Single.NaN)
                    };
                }
            }

            public const float MinValue = (1 / 1024f);

            public static bool operator ==(Intersection intersection1, Intersection intersection2)
            {
                return intersection1.value == intersection2.value && intersection1.color == intersection2.color && intersection1.normal == intersection2.normal;
            }
            public static bool operator !=(Intersection intersection1, Intersection intersection2)
            {
                return !(intersection1 == intersection2);
            }
            public override int GetHashCode()
            {
                int hash = 23;
                hash = hash * 41 + value.GetHashCode();
                hash = hash * 41 + color.GetHashCode();
                hash = hash * 41 + normal.GetHashCode();
                return hash;
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Intersection))
                {
                    return false;
                }
                return (Intersection)obj == this;
            }
        }

        public float refractionAmount;
        public float reflectionAmount;
        public float refractionIndex;
        public float DiffuseAmount
        {
            get
            {
                return 1 - (refractionAmount + reflectionAmount);
            }
        }

        public abstract Intersection GetNearestIntersection(Ray ray);

        public abstract Intersection[] GetAllIntersections(Ray ray);

        public abstract bool Contains(Point3D point);
    }
}
