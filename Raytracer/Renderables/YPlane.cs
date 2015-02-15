using PixelWindowCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class YPlane : Renderable
    {
        float Y;
        private static readonly Vector3D Normal = new Vector3D(0, 1, 0);

        public YPlane(float y)
        {
            this.Y = y;
            this.reflectionAmount = 0;
            this.refractionAmount = 0;
        }

        public override Intersection GetNearestIntersection(Ray ray)
        {
            if(ray.YEquation.slope == 0)
            {
                if(ray.YEquation.intercept == Y)
                {
                    return new Intersection
                    {
                        value = 0,
                        normal = YPlane.Normal,
                        color = ColorAt(ray.Origin.X, ray.Origin.Z)
                    };
                }
                else
                {
                    return Intersection.None;
                }
            }

            float value = ray.YEquation.SolveWhenValueIs(Y);
            Point3D point = ray.PointAt(value);
            if (value >= Intersection.MinValue)
            {
                return new Intersection
                {
                    value = value,
                    color = ColorAt(point.X, point.Z),
                    normal = YPlane.Normal
                };
            }
            else
            {
                return Intersection.None;
            }
        }

        public override Intersection[] GetAllIntersections(Ray ray)
        {
            Intersection i = GetNearestIntersection(ray);
            if (i == Intersection.None)
            {
                return Intersection.NoneArray;
            }
            else
            {
                return new[] { i };
            }
        }

        public override bool Contains(Point3D point)
        {
            return point.Y == Y;
        }

        private ARGBColor ColorAt(float x, float z)
        {
            //return x.PMod(20) < 10 ^ z.PMod(20) < 10 ? (ARGBColor)0xFF4A7023 : (ARGBColor)0xFF78AB46;
            return x.PMod(20) < 10 ^ z.PMod(20) < 10 ? (ARGBColor)0xFFFFFFFF : (ARGBColor)0xFF000000;
        }
    }
}
