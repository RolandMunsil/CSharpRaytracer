using PixelWindowSDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    abstract class Renderable
    {
        public struct Intersection
        {
            public double value;
            public Color color;
            public Vector3D normal;

            public static readonly Intersection FarthestAway = new Intersection { value = Double.MaxValue };

            public static readonly Intersection None = 
                new Intersection
                {
                    value = Double.NaN,
                    color = (Color)0xDEDBEF,
                    normal = new Vector3D(Double.NaN, Double.NaN, Double.NaN)
                };

            public static readonly Intersection[] NoneArray = { };

            public const double MinValue = (1 / 32768.0);

            public static bool operator ==(Intersection intersection1, Intersection intersection2)
            {
                return intersection1.value == intersection2.value && intersection1.color == intersection2.color && intersection1.normal == intersection2.normal;
            }
            public static bool operator !=(Intersection intersection1, Intersection intersection2)
            {
                return intersection1.value != intersection2.value || intersection1.color != intersection2.color || intersection1.normal != intersection2.normal;
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

        public double refractivity = 0;
        public double reflectivity = 0;
        public double refractionIndex = 0;
        public double DiffuseAmount
        {
            get
            {
                return 1 - (refractivity + reflectivity);
            }
        }

        public abstract Intersection GetNearestIntersection(Ray ray);

        public abstract Intersection[] GetAllIntersections(Ray ray);

        public abstract bool Contains(Point3D point);

        public static CSGObject operator |(Renderable renderable1, Renderable renderable2)
        {
            return new CSGObject(renderable1, renderable2, CSGObject.Operation.Or);
        }
        public static CSGObject operator &(Renderable renderable1, Renderable renderable2)
        {
            return new CSGObject(renderable1, renderable2, CSGObject.Operation.And);
        }
        public static CSGObject operator ^(Renderable renderable1, Renderable renderable2)
        {
            return new CSGObject(renderable1, renderable2, CSGObject.Operation.ExclusiveOr);
        }
        public static CSGObject operator -(Renderable renderable1, Renderable renderable2)
        {
            return new CSGObject(renderable1, renderable2, CSGObject.Operation.FirstWithoutSecond);
        }
    }
}
