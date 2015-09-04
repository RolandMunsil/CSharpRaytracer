using PixelWindowSDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Cuboid : Renderable
    {
        Color color;

        double xLow;
        double xHigh;
        double yLow;
        double yHigh;
        double zLow;
        double zHigh;

        Tuple<double, int>[] coordsToCheck;

        public Cuboid(Point3D center, double xSize, double ySize, double zSize, Color color)
        {
            xLow = center.x - (xSize / 2);
            xHigh = center.x + (xSize / 2);

            yLow = center.y - (ySize / 2);
            yHigh = center.y + (ySize / 2);

            zLow = center.z - (zSize / 2);
            zHigh = center.z + (zSize / 2);

            this.color = color;


            coordsToCheck = new Tuple<double, int>[6]
            {
                new Tuple<double, int>(xLow,   0),
                new Tuple<double, int>(xHigh,  0),
                new Tuple<double, int>(yLow,   1),
                new Tuple<double, int>(yHigh,  1),
                new Tuple<double, int>(zLow,   2),
                new Tuple<double, int>(zHigh,  2),
            };
        }

        public override Renderable.Intersection GetNearestIntersection(Ray ray)
        {
            //TODO: I feel like this is kind of terrible.
            bool valueFound = false;
            double closestValue = Intersection.FarthestAway.value;
            int normalComponentIndex = -1;

            for(int i = 0; i < 6; i++)
            {
                double component = coordsToCheck[i].Item1;
                int componentIndex = coordsToCheck[i].Item2;

                double value = ray.ValueWhenComponentIs(component, componentIndex);
                if (value < closestValue && value >= Intersection.MinValue)
                {
                    Point3D pointToCheck = ray.PointAt(value);
                    pointToCheck[componentIndex] = component;
                    if (this.Contains(pointToCheck))
                    {
                        valueFound = true;
                        closestValue = value;
                        normalComponentIndex = componentIndex;
                    }
                }
            }

            if (!valueFound)
            {
                return Intersection.None;
            }
            else
            {
                Vector3D normal = Vector3D.Zero;
                normal[normalComponentIndex] = 1;
                return new Intersection
                {
                    value = closestValue,
                    normal = normal,
                    color = color
                };
            }
        }

        public override Renderable.Intersection[] GetAllIntersections(Ray ray)
        {
            List<Intersection> intersections = new List<Intersection>(6);

            foreach (var tuple in coordsToCheck)
            {
                double component = tuple.Item1;
                int componentIndex = tuple.Item2;

                double value = ray.ValueWhenComponentIs(component, componentIndex);
                if (value >= Intersection.MinValue)
                {
                    Point3D pointToCheck = ray.PointAt(value);
                    pointToCheck[componentIndex] = component;
                    if (this.Contains(pointToCheck))
                    {
                        Vector3D normal = Vector3D.Zero;
                        normal[componentIndex] = 1;
                        intersections.Add(new Intersection
                        {
                            value = value,
                            normal = normal,
                            color = color
                        });
                    }
                }
            }

            if (intersections.Count == 0)
            {
                return Intersection.NoneArray;
            }
            else
            {
                return intersections.ToArray();
            }
        }

        public override bool Contains(Point3D point)
        {
            return point.x >= xLow && point.x <= xHigh &&
                   point.y >= yLow && point.y <= yHigh &&
                   point.z >= zLow && point.z <= zHigh;
        }
    }
}
