using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    struct Vector3D
    {
        public double x;
        public double y;
        public double z;

        public Vector3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3D Zero
        {
            get
            {
                return new Vector3D(0, 0, 0);
            }
        }

        static Random rand = new Random();

        /// <summary>
        /// Returns a random unit vector.
        /// </summary>
        /// <returns></returns>
        public static Vector3D RandomUnitVector(double maxAngleFromUnitZ = Math.PI)
        {
            double thing = (Math.Cos(Math.PI - maxAngleFromUnitZ) + 1) / 2;
            double randOneNegativeOne = 2 * (rand.NextDouble() * thing) - 1;
            double rotYZ = Math.Acos(randOneNegativeOne) - Math.PI / 2;
            double rotXZ = rand.NextDouble() * (Math.PI * 2);

            return Matrix3x3.RotationAboutXAxis(-Math.PI / 2) * ((Matrix3x3.RotationAboutYAxis(rotXZ) * Matrix3x3.RotationAboutXAxis(rotYZ)) * new Vector3D(0, 0, 1));
        }

        public static Vector3D RandomUnitVectorFrom(Vector3D vector, double maxAngleFromVector)
        {
            Vector3D randomVector = RandomUnitVector(maxAngleFromVector);
            return Matrix3x3.RotationAboutYAxis(vector.AngleAboutYAxis) * Matrix3x3.RotationAboutXAxis(vector.AngleFromHorizontalPlane) * randomVector;
        }

        public static Vector3D RandomVectorInUnitSphere()
        {
            return RandomUnitVector() * (Math.Pow(rand.NextDouble(), 1.0 / 3.0));
        }

        //I feel like maybe this is a horrible hacky thing.
        public double this[int index]
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

        /// <summary>
        /// Some example values:
        /// if the vector points directly +x, this will return π/2
        /// if the vector points directly +z, this will return 0
        /// if the vector points directly -x, this will return -π/2,
        /// etc.
        /// </summary>
        public double AngleAboutYAxis
        {
            get
            {
                return Math.Atan2(x, z);
            }
        }

        public double AngleFromHorizontalPlane
        {
            get
            {
                //return (Math.PI / 2) - (this.AngleTo(new Vector3D(0, 1, 0)));
                return (Math.PI / 2) - Math.Acos(this.y / this.Length);
            }
        }

        public double LengthSquared
        {
            get
            {
                return x * x + y * y + z * z;
            }
        }

        public double Length
        {
            get
            {
                return Math.Sqrt(LengthSquared);
            }
        }

        public Vector3D Normalized()
        {
            return this / this.Length;
        }

        public void Normalize()
        {
            this /= this.Length;
        }

        public static double DotProduct(Vector3D vector1, Vector3D vector2)
        {
            return (vector1.x * vector2.x) + (vector1.y * vector2.y) + (vector1.z * vector2.z);
        }

        public static Vector3D CrossProduct(Vector3D v1, Vector3D v2)
        {
            Vector3D v = new Vector3D(
                v1.y * v2.z - v2.y * v1.z,
                v1.z * v2.x - v2.z * v1.x,
                v1.x * v2.y - v2.x * v1.y
                );

            return v;
        }

        public Vector3D Reflected(Vector3D surfaceNormal)
        {
            Vector3D normalizedNormal = surfaceNormal.Normalized();
            double dotProduct = DotProduct(this, normalizedNormal);
            Vector3D dunno = (normalizedNormal * 2) * dotProduct;

            return this - dunno;
        }
        //http://en.wikipedia.org/wiki/Snell's_law#Vector_form
        public Vector3D Refracted(Vector3D surfaceNormal, double refractIndexFrom, double refractIndexTo, out bool totalInternalReflection)
        {
            double n1 = refractIndexFrom;
            double n2 = refractIndexTo;

            Vector3D n = surfaceNormal.Normalized();
            Vector3D l = this.Normalized();

            double cosθ1 = Math.Min(1, DotProduct(-n, l)); //Min for rounding errors
            if (cosθ1 <= 0)
            {
                n = -n;
                cosθ1 = DotProduct(-n, l);
            }

            double sinθ2 = (n1 / n2) * Math.Sqrt(1 - (cosθ1 * cosθ1));

            if (1 - (sinθ2 * sinθ2) < 0)
            {
                totalInternalReflection = true;
                return this.Reflected(surfaceNormal);
            }

            double cosθ2 = Math.Sqrt(1 - (sinθ2 * sinθ2));
            Vector3D vrefract = l * (n1 / n2) + n * ((n1 / n2) * cosθ1 - cosθ2);
            totalInternalReflection = false;

            return vrefract;
        }

        public static double AngleBetween(Vector3D vec1, Vector3D vec2)
        {
            return Math.Acos(DotProduct(vec1, vec2) / (vec1.Length * vec2.Length));
        }

        public double AngleTo(Vector3D vec)
        {
            return AngleBetween(this, vec);
        }

        public static bool operator ==(Vector3D vector1, Vector3D vector2)
        {
            return vector1.x == vector2.x && vector1.y == vector2.y && vector1.z == vector2.z;
        }
        public static bool operator !=(Vector3D vector1, Vector3D vector2)
        {
            return !(vector1 == vector2);
        }
        public override int GetHashCode()
        {
            int hash = 13;
            hash = hash * 47 + x.GetHashCode();
            hash = hash * 47 + y.GetHashCode();
            hash = hash * 47 + z.GetHashCode();
            return hash;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3D))
            {
                return false;
            }
            return (Vector3D)obj == this;
        }

        public static Vector3D operator +(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(vector1.x + vector2.x, vector1.y + vector2.y, vector1.z + vector2.z);
        }
        public static Vector3D operator -(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.x - right.x, left.y - right.y, left.z - right.z);
        }
        public static Vector3D operator /(Vector3D vector, double divisor)
        {
            return new Vector3D(vector.x / divisor, vector.y / divisor, vector.z / divisor);
        }
        public static Vector3D operator *(Vector3D vector, double multiplier)
        {
            return new Vector3D(vector.x * multiplier, vector.y * multiplier, vector.z * multiplier);
        }

        public static Vector3D operator *(double multiplier, Vector3D vector)
        {
            return vector * multiplier;
        }

        public static Vector3D operator -(Vector3D vector)
        {
            return new Vector3D(-vector.x, -vector.y, -vector.z);
        }

        public static explicit operator Point3D(Vector3D vector)
        {
            return new Point3D(vector.x, vector.y, vector.z);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }
    }
}
