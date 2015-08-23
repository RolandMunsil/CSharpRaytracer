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
            Or,
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

            this.reflectivity = obj1.reflectivity;
            this.refractivity = obj1.refractivity;
            this.refractionIndex = obj1.refractionIndex;
        }

        public override Intersection GetNearestIntersection(Ray ray)
        {
            Intersection[] obj1Intersections = renderable1.GetAllIntersections(ray);
            Intersection[] obj2Intersections = renderable2.GetAllIntersections(ray);

            if (obj1Intersections.Length == 0 && obj2Intersections.Length == 0)
            {
                return Intersection.None;
            }

            //List<Intersection> validIntersections = new List<Intersection>(obj1Intersections.Length + obj2Intersections.Length);

            Intersection closest = Intersection.FarthestAway;
            //TODO: a lot of this code is very similar - is there a way to put it in a helper function without significantly impacting performance?
            switch (operation)
            {
                case Operation.Or:
                    //Don't add points that are inside the other renderable

                    //If the only intersections are with the 1st object, we don't need to check contains or anything
                    if (obj1Intersections.Length == 0)
                    {
                        return obj2Intersections.Nearest();
                    }
                    //If the only intersections are with the 2nd object, we don't need to check contains or anything
                    if (obj2Intersections.Length == 0)
                    {
                        return obj1Intersections.Nearest();
                    }

                    foreach(Intersection intersection in obj1Intersections)
                    {
                        if (intersection.value < closest.value)
                        {
                            Point3D intersectionPoint = ray.PointAt(intersection.value);
                            if (!renderable2.Contains(intersectionPoint))
                            {
                                closest = intersection;
                            }
                        }
                    }

                    foreach (Intersection intersection in obj2Intersections)
                    {
                        if (intersection.value < closest.value)
                        {
                            Point3D intersectionPoint = ray.PointAt(intersection.value);
                            if (!renderable1.Contains(intersectionPoint))
                            {
                                closest = intersection;
                            }
                        }
                    }
                    break;
                case Operation.And:
                    //Only add points that are inside the other renderable

                    //If the ray only passes through one object, there can't be any intersection.
                    if (obj1Intersections.Length == 0 || obj2Intersections.Length == 0)
                    {
                        return Intersection.None;
                    }

                    foreach(Intersection intersection in obj1Intersections)
                    {
                        if (intersection.value < closest.value)
                        {
                            Point3D intersectionPoint = ray.PointAt(intersection.value);
                            if (renderable2.Contains(intersectionPoint))
                            {
                                closest = intersection;
                            }
                        }
                    }

                    foreach (Intersection intersection in obj2Intersections)
                    {
                        if (intersection.value < closest.value)
                        {
                            Point3D intersectionPoint = ray.PointAt(intersection.value);
                            if (renderable1.Contains(intersectionPoint))
                            {
                                closest = intersection;
                            }
                        }
                    }
                    break;
                case Operation.ExclusiveOr:
                    //All points are valid

                    //If the only intersections are with the 1st object, we don't need to check contains or anything
                    if (obj1Intersections.Length == 0)
                    {
                        return obj2Intersections.Nearest();
                    }
                    //If the only intersections are with the 2nd object, we don't need to check contains or anything
                    if (obj2Intersections.Length == 0)
                    {
                        return obj1Intersections.Nearest();
                    }

                    foreach (Intersection intersection in obj1Intersections)
                    {
                        if (intersection.value < closest.value)
                        {
                            closest = intersection;
                        }
                    }

                    foreach (Intersection intersection in obj2Intersections)
                    {
                        if (intersection.value < closest.value)
                        {
                            closest = intersection;
                        }
                    }
                    break;
                case Operation.FirstWithoutSecond:
                    //Only add points that are either:
                    //  On the surface of the first and not inside the second
                    //  On the surface of the second and inside the first

                    //If the ray does not pass through the first object, none of the points could possibly be inside of it
                    if (obj1Intersections.Length == 0)
                    {
                        return Intersection.None;
                    }
                    //If the only intersections are with the 1st object, we don't need to check contains or anything
                    else if (obj2Intersections.Length == 0)
                    {
                        return obj1Intersections.Nearest();
                    }

                    foreach(Intersection intersection in obj1Intersections)
                    {
                        if (intersection.value < closest.value)
                        {
                            Point3D intersectionPoint = ray.PointAt(intersection.value);
                            if (!renderable2.Contains(intersectionPoint))
                            {
                                closest = intersection;
                            }
                        }
                    }

                    foreach (Intersection intersection in obj2Intersections)
                    {
                        if (intersection.value < closest.value)
                        {
                            Point3D intersectionPoint = ray.PointAt(intersection.value);
                            if (renderable1.Contains(intersectionPoint))
                            {
                                closest = intersection;
                            }
                        }
                    }
                    break;
            }

            //return validIntersections.ToArray().Nearest();
            if(closest == Intersection.FarthestAway)
            {
                return Intersection.None;
            }
            return closest;
        }

        public override Renderable.Intersection[] GetAllIntersections(Ray ray)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(Point3D point)
        {
            switch (operation)
            {
                case Operation.Or:
                    return renderable1.Contains(point) || renderable2.Contains(point);
                case Operation.And:
                    return renderable1.Contains(point) && renderable2.Contains(point);
                case Operation.ExclusiveOr:
                    return renderable1.Contains(point) ^ renderable2.Contains(point);
                case Operation.FirstWithoutSecond:
                    return renderable1.Contains(point) && !renderable2.Contains(point);
                default:
                    throw new NotImplementedException("Contains() not implemented for " + operation);
            }
        }
    }
}
