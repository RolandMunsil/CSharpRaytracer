using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    struct Point3D
    {
        public float X;
        public float Y;
        public float Z;

        public Point3D(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Point3D Zero
        {
            get
            {
                return new Point3D(0, 0, 0);
            }
        }

        public static Vector3D operator -(Point3D left, Point3D right)
        {
            return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public override string ToString()
        {
            return "X=" + X + " Y=" + Y + " Z=" + Z;
        }
    }
}
