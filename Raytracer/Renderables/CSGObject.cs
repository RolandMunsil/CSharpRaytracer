using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class CSGObject : Renderable
    {
        enum Operation
        {
            OuterShellOnly,
            And,
            ExclusiveOr,
            FirstWithoutSecond,
        }

        Renderable renderable1;
        Renderable renderable2;
        Operation operation;

        public override Intersection GetNearestIntersection(Ray ray)
        {
            Intersection[] obj1Intersections = renderable1.GetAllIntersections(ray);
            Intersection[] obj2Intersections = renderable2.GetAllIntersections(ray);

            List<Intersection> validIntersections = new List<Intersection>(obj1Intersections.Length + obj2Intersections.Length);

            //TODO: there are lotsa ways this can be optimized. Right now it's just written to be short and clear.
            switch (operation)
            {
                case Operation.OuterShellOnly:
                    //Don't add points that are inside the other renderable
                    validIntersections.AddRange(obj1Intersections.Where(i => !renderable2.Contains(ray.PointAt(i.value))));
                    validIntersections.AddRange(obj2Intersections.Where(i => !renderable1.Contains(ray.PointAt(i.value))));
                    break;
                case Operation.And:
                    //Only add points that are inside the other renderable
                    validIntersections.AddRange(obj1Intersections.Where(i => renderable2.Contains(ray.PointAt(i.value))));
                    validIntersections.AddRange(obj2Intersections.Where(i => renderable1.Contains(ray.PointAt(i.value))));
                    break;
                case Operation.ExclusiveOr:
                    //All points are valid
                    validIntersections.AddRange(obj1Intersections);
                    validIntersections.AddRange(obj2Intersections);
                    break;
                case Operation.FirstWithoutSecond:
                    //Only add points that are either:
                    //  On the surface of the first and not inside the second
                    //  On the surface of the second and inside the first
                    validIntersections.AddRange(obj1Intersections.Where(i => !renderable2.Contains(ray.PointAt(i.value))));
                    validIntersections.AddRange(obj2Intersections.Where(i => renderable1.Contains(ray.PointAt(i.value))));
                    break;
            }

            Intersection minIntersection = validIntersections[0];
            for (int i = 1; i < validIntersections.Count; i++)
            {
                if (validIntersections[i].value < minIntersection.value)
                {
                    minIntersection = validIntersections[i];
                }
            }
            return validIntersections.ToArray().Nearest();
        }

        public override Renderable.Intersection[] GetAllIntersections(Ray ray)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(Point3D point)
        {
            throw new NotImplementedException();
        }
    }
}
