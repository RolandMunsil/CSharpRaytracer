using PixelWindowCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Cuboid : Renderable
    {
        ARGBColor color;

        float xLow;
        float xHigh;
        float yLow;
        float yHigh;
        float zLow;
        float zHigh;

        public Cuboid(Point3D center, float xSize, float ySize, float zSize, ARGBColor color)
        {
            xLow = center.x - (xSize / 2);
            xHigh = center.x + (xSize / 2);

            yLow = center.y - (ySize / 2);
            yHigh = center.y + (ySize / 2);

            zLow = center.z - (zSize / 2);
            zHigh = center.z + (zSize / 2);

            this.color = color;
        }

        public override Renderable.Intersection GetNearestIntersection(Ray ray)
        {
            //TODO: I feel like this is kind of terrible.

            List<Tuple<float, int>> coordsToCheck = new List<Tuple<float, int>>(6)
            {
                new Tuple<float, int>(xLow,   0),
                new Tuple<float, int>(xHigh,  0),
                new Tuple<float, int>(yLow,   1),
                new Tuple<float, int>(yHigh,  1),
                new Tuple<float, int>(zLow,   2),
                new Tuple<float, int>(zHigh,  2),
            };

            bool valueFound = false;
            float closestValidValue = Intersection.FarthestAway.value;
            int normalComponentIndex = -1;

            int hackySign = 0;

            foreach (var tuple in coordsToCheck)
            {
                float component = tuple.Item1;
                int componentIndex = tuple.Item2;

                float value = ray.ValueWhenComponentIs(component, componentIndex);
                if (value < closestValidValue && value >= Intersection.MinValue)
                {
                    Point3D pointToCheck = ray.PointAt(value);
                    pointToCheck[componentIndex] = component;
                    if (this.Contains(pointToCheck))
                    {
                        hackySign = Math.Sign(pointToCheck[componentIndex]);

                        valueFound = true;
                        closestValidValue = value;
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
                normal[normalComponentIndex] = 1 * hackySign;
                return new Intersection
                {
                    value = closestValidValue,
                    normal = normal,
                    color = color
                };
            }
        }

        public override Renderable.Intersection[] GetAllIntersections(Ray ray)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(Point3D point)
        {
            return point.x >= xLow && point.x <= xHigh &&
                   point.y >= yLow && point.y <= yHigh &&
                   point.z >= zLow && point.z <= zHigh;
        }
    }
}
