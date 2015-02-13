using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class CSGObject : Renderable
    {
        public enum Operation
        {
            OuterShellOnly,
            And,
            ExclusiveOr,
            FirstWithoutSecond,
        }

        Renderable renderable1;
        Renderable renderable2;
        Operation operation;

        public CSGObject(Renderable obj1, Renderable obj2, Operation operation)
        {
            this.renderable1 = obj1;
            this.renderable2 = obj2;
            this.operation = operation;

            this.reflectionAmount = obj1.reflectionAmount;
            this.refractionAmount = obj1.refractionAmount;
            this.refractionIndex = obj1.refractionIndex;
        }

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
