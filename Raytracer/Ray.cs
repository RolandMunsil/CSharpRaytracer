using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Ray
    {
        private Point3D origin;
        private Vector3D direction;

        public Point3D Origin
        {
            get
            {
                return origin;
            }
        }

        public Vector3D Direction
        {
            get
            {
                return direction;
            }
        }

        public Ray(Point3D origin, Vector3D direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public Ray(Point3D origin, Point3D endPoint)
            : this(origin, endPoint - origin)
        {
        }

        public Point3D PointAt(double t)
        {
            return origin + (direction * t);
        }

        public double ValueWhenXIs(double x)
        {
            return (x - origin.x) / direction.x;
        }

        public double ValueWhenYIs(double y)
        {
            return (y - origin.y) / direction.y;
        }

        public double ValueWhenZIs(double z)
        {
            return (z - origin.z) / direction.z;
        }

        public double ValueWhenComponentIs(double value, int componentIndex)
        {
            return (value - origin[componentIndex]) / direction[componentIndex];
        }
    }
}
