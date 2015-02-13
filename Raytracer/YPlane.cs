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
            if (value >= Intersection.MinValue)
            {
                return new Intersection
                {
                    value = value,
                    color = ColorAt(ray.PointAt(value).X, ray.PointAt(value).Z),
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
            return new[] { GetNearestIntersection(ray) };
        }

        public override bool Contains(Point3D point)
        {
            return point.Y == Y;
        }

        private ARGBColor ColorAt(float x, float z)
        {
            return x.PMod(20) < 10 ^ z.PMod(20) < 10 ? (ARGBColor)0xFF4A7023 : (ARGBColor)0xFF78AB46;
        }
    }
}
