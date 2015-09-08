using PixelWindowSDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class YCylinder : Renderable
    {
        double centerX;
        double centerZ;
        double radius;

        double yTop;
        double yBottom;

        Color color;

        public YCylinder(Point3D center, double radius, double height, Color color)
        {
            centerX = center.x;
            centerZ = center.z;
            this.radius = radius;
            yTop = center.y + height / 2;
            yBottom = center.y - height / 2;
            this.color = color;
        }

        public override Renderable.Intersection GetNearestIntersection(Ray ray)
        {
            return GetAllIntersections(ray).Nearest();
        }

        public override Renderable.Intersection[] GetAllIntersections(Ray ray)
        {
            List<Intersection> intersections = new List<Intersection>(4);

            if (ray.Direction.y != 0)
            {
                double val1 = ray.ValueWhenYIs(yTop);
                double val2 = ray.ValueWhenYIs(yBottom);

                double closerVal = val1 < val2 ? val1 : val2;
                double fartherVal = val1 < val2 ? val2 : val1;

                Point3D closerPoint = ray.PointAt(closerVal);
                Point3D fartherPoint = ray.PointAt(fartherVal);

                bool closerIsValid = IsWithinCircle(closerPoint) && closerVal > Intersection.MinValue;
                bool fartherIsValid = IsWithinCircle(fartherPoint) && fartherVal > Intersection.MinValue;

                if (closerIsValid)
                {
                    intersections.Add(new Intersection
                    {
                        value = closerVal,
                        normal = new Vector3D(0, 1, 0),
                        color = color
                    });
                }
                if(fartherIsValid)
                {
                    intersections.Add(new Intersection
                    {
                        value = fartherVal,
                        normal = new Vector3D(0, 1, 0),
                        color = color
                    });
                }
            }

            //Now the circle\
            //Circle: r^2 = (x - x1)^2 + (z - z1)^2
            //Line: x = at + b
            //      z = ct + d

            double adjXIntercept = ray.Origin.x - this.centerX;
            double adjZIntercept = ray.Origin.z - this.centerZ;

            double quadCoefficient = ray.Direction.x * ray.Direction.x +
                                    ray.Direction.z * ray.Direction.z;

            double linearCoefficient = ((ray.Direction.x * adjXIntercept) * 2) +
                                      ((ray.Direction.z * adjZIntercept) * 2);

            double constant = adjXIntercept * adjXIntercept +
                             adjZIntercept * adjZIntercept -
                             radius * radius;

            //Find zeroes using quadratic equation
            double a = quadCoefficient;
            double b = linearCoefficient;
            double c = constant;

            double numToSqrt = (b * b) - (4 * a * c);
            if (numToSqrt < 0)
            {
                return intersections.ToArray();
            }

            if (a == 0) //Divide by zero not allowed
            {
                return intersections.ToArray();
            }
            double higherZero = (-b + Math.Sqrt(numToSqrt)) / (2 * a);
            double lowerZero = (-b - Math.Sqrt(numToSqrt)) / (2 * a);

            if (higherZero > Intersection.MinValue && IsBetweenYBounds(ray.PointAt(higherZero)))
            {
                intersections.Add(new Intersection
                {
                    value = higherZero,
                    normal = NormalAt(ray.PointAt(higherZero)),
                    color = color
                });
            }
            if (lowerZero > Intersection.MinValue && IsBetweenYBounds(ray.PointAt(lowerZero)))
            {
                intersections.Add(new Intersection
                {
                    value = lowerZero,
                    normal = NormalAt(ray.PointAt(lowerZero)),
                    color = color
                });
            }

            return intersections.ToArray();
        }

        private Vector3D NormalAt(Point3D point)
        {
            return new Vector3D(point.x - this.centerX, 0, point.z - this.centerZ);
        }

        public override bool Contains(Point3D point)
        {
            return IsBetweenYBounds(point) && IsWithinCircle(point);
            
        }

        private bool IsBetweenYBounds(Point3D point)
        {
            return point.y <= yTop && point.y >= yBottom;
        }

        private bool IsWithinCircle(Point3D point)
        {
            double dx = point.x - centerX;
            double dz = point.z - centerZ;
            return dx * dx + dz * dz <= radius * radius;
        }
    }
}
