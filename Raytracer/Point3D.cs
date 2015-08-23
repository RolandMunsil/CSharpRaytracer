using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    [DebuggerDisplay("({x}, {y}, {z})")]
    struct Point3D
    {
        public float x;
        public float y;
        public float z;

        public Point3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        //I feel like maybe this is a horrible hacky thing.
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public static Point3D Zero
        {
            get
            {
                return new Point3D(0, 0, 0);
            }
        }

        public static Point3D operator +(Point3D left, Vector3D right)
        {
            return new Point3D(left.x + right.x, left.y + right.y, left.z + right.z);
        }

        public static Vector3D operator -(Point3D left, Point3D right)
        {
            return new Vector3D(left.x - right.x, left.y - right.y, left.z - right.z);
        }

        public static Point3D operator *(Point3D vector, float multiplier)
        {
            return new Point3D(vector.x * multiplier, vector.y * multiplier, vector.z * multiplier);
        }

        public override string ToString()
        {
            return "X=" + x + " Y=" + y + " Z=" + z;
        }
    }
}
