using PixelWindowSDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Sphere : Renderable
    {
        public Point3D center;
        public double radius;

        public Color color;

        public Sphere(Point3D center, double radius, Color color)
        {
            this.center = center;
            this.radius = radius;

            this.color = color;

            //reflectionAmount = .6f;
            //refractionAmount = 0;
            //refractionIndex = 0;
            //color = (Color)0x91D9D1;
        }

        public override Intersection GetNearestIntersection(Ray ray)
        {
            return GetAllIntersections(ray).Nearest();
        }

        private Vector3D NormalAt(Point3D point)
        {
            return (point - center);
        }

        public override Intersection[] GetAllIntersections(Ray ray)
        {
            //sphere: r^2 = (x - x₁)² + (y - y₁)² + (z - z₁)²
            //Line: x = at + b
            //      y = ct + d
            //      z = et + g

            //LinearEquation combinedX = ray.XEquation - center.X;
            //LinearEquation combinedY = ray.YEquation - center.Y;
            //LinearEquation combinedZ = ray.ZEquation - center.Z;
            double adjXIntercept = ray.Origin.x - center.x;
            double adjYIntercept = ray.Origin.y - center.y;
            double adjZIntercept = ray.Origin.z - center.z;

            //Optimize these two lines.
            //QuadraticEquation combined = combinedX * combinedX + combinedY * combinedY + combinedZ * combinedZ;
            //combined -= radius * radius;

            double quadCoefficient = ray.Direction.x * ray.Direction.x +
                                    ray.Direction.y * ray.Direction.y +
                                    ray.Direction.z * ray.Direction.z;

            double linearCoefficient = ((ray.Direction.x * adjXIntercept) * 2) +
                                      ((ray.Direction.y * adjYIntercept) * 2) +
                                      ((ray.Direction.z * adjZIntercept) * 2);

            double constant = adjXIntercept * adjXIntercept +
                             adjYIntercept * adjYIntercept +
                             adjZIntercept * adjZIntercept -
                             radius * radius;

            //Find zeroes using quadratic equation
            double a = quadCoefficient;
            double b = linearCoefficient;
            double c = constant;

            double numToSqrt = (b * b) - (4 * a * c);
            if (numToSqrt < 0)
            {
                return Intersection.NoneArray;
            }

            if (a == 0) //Divide by zero not allowed
            {
                return Intersection.NoneArray;
            }
            double higherZero = (-b + Math.Sqrt(numToSqrt)) / (2 * a);
            double lowerZero = (-b - Math.Sqrt(numToSqrt)) / (2 * a);

            if (higherZero < Intersection.MinValue && lowerZero < Intersection.MinValue)
            {
                return Intersection.NoneArray;
            }
            else if (lowerZero < Intersection.MinValue)
            {
                return new Intersection[] 
                { 
                    new Intersection
                    {
                        value = higherZero,
                        color = this.color,
                        normal = NormalAt(ray.PointAt(higherZero))
                    }
                };
            }
            else
            {
                return new Intersection[] 
                { 
                    new Intersection
                    {
                        value = lowerZero,
                        color = this.color,
                        normal = NormalAt(ray.PointAt(lowerZero))
                    },
                    new Intersection
                    {
                        value = higherZero,
                        color = this.color,
                        normal = NormalAt(ray.PointAt(higherZero))
                    }
                };
            }
        }

        public override bool Contains(Point3D point)
        {
            return (point - center).LengthSquared <= radius * radius;
        }
    }
}
