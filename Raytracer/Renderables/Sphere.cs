using PixelWindowCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Sphere : Renderable
    {
        Point3D center;
        float radius;

        public ARGBColor color;

        public Sphere(Point3D center, float radius)
        {
            this.center = center;
            this.radius = radius;

            //reflectionAmount = .6f;
            //refractionAmount = 0;
            //refractionIndex = 0;
            //color = (ARGBColor)0xFF91D9D1;
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
            float adjXIntercept = ray.Origin.x - center.x;
            float adjYIntercept = ray.Origin.y - center.y;
            float adjZIntercept = ray.Origin.z - center.z;

            //Optimize these two lines.
            //QuadraticEquation combined = combinedX * combinedX + combinedY * combinedY + combinedZ * combinedZ;
            //combined -= radius * radius;

            float quadCoefficient = ray.Direction.x * ray.Direction.x +
                                    ray.Direction.y * ray.Direction.y +
                                    ray.Direction.z * ray.Direction.z;

            float linearCoefficient = ((ray.Direction.x * adjXIntercept) * 2) +
                                      ((ray.Direction.y * adjYIntercept) * 2) +
                                      ((ray.Direction.z * adjZIntercept) * 2);

            float constant = adjXIntercept * adjXIntercept +
                             adjYIntercept * adjYIntercept +
                             adjZIntercept * adjZIntercept -
                             radius * radius;

            //Find zeroes using quadratic equation
            float a = quadCoefficient;
            float b = linearCoefficient;
            float c = constant;

            float numToSqrt = (b * b) - (4 * a * c);
            if (numToSqrt < 0)
            {
                return Intersection.NoneArray;
            }

            if (a == 0) //Divide by zero not allowed
            {
                return Intersection.NoneArray;
            }
            float higherZero = (-b + (float)Math.Sqrt(numToSqrt)) / (2 * a);
            float lowerZero = (-b - (float)Math.Sqrt(numToSqrt)) / (2 * a);

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
