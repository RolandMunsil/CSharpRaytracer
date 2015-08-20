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

        public Point3D PointAt(float t)
        {
            return origin + (direction * t);
        }

        public float ValueWhenXIs(float x)
        {
            return (x - origin.X) / direction.X;
        }

        public float ValueWhenYIs(float y)
        {
            return (y - origin.Y) / direction.Y;
        }

        public float ValueWhenZIs(float z)
        {
            return (z - origin.Z) / direction.Z;
        }
    }
}
