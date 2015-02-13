using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    struct Vector3D
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3D(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        /// <summary>
        /// Some example values:
        /// if the vector points directly +x, this will return π/2
        /// if the vector points directly +z, this will return 0
        /// if the vector points directly -x, this will return -π/2,
        /// etc.
        /// </summary>
        public float AngleXZ
        {
            get
            {
                return (float)Math.Atan2(X, Z);
            }
        }

        /// <summary>
        /// Some example values:
        /// if the vector points directly +y, this will return π/2
        /// if the vector points directly +z, this will return 0
        /// if the vector points directly -y, this will return -π/2,
        /// etc.
        /// </summary>
        public float AngleYZ
        {
            get
            {
                return (float)Math.Atan2(Y, Z);
            }
        }

        public float LengthSquared
        {
            get
            {
                return X * X + Y * Y + Z * Z;
            }
        }

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
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

        public static float DotProduct(Vector3D vector1, Vector3D vector2)
        {
            return (vector1.X * vector2.X) + (vector1.Y * vector2.Y) + (vector1.Z * vector2.Z);
        }

        public void Rotate(float rotationXZ, float rotationYZ)
        {
            if (rotationYZ != 0)
            {
                //First do vertical
                float lengthYZ = (float)Math.Sqrt(Y * Y + Z * Z);
                float angleYZ = (float)Math.Atan2(Y, Z);

                float newAngle = angleYZ + rotationYZ;
                this.Y = (float)Math.Sin(newAngle) * lengthYZ;
                this.Z = (float)Math.Cos(newAngle) * lengthYZ;
            }

            if (rotationXZ != 0)
            {
                //Now do horizontal
                float lengthXZ = (float)Math.Sqrt(X * X + Z * Z);
                float angleXZ = (float)Math.Atan2(X, Z);

                float newAngle = angleXZ + rotationXZ;
                this.X = (float)Math.Sin(newAngle) * lengthXZ;
                this.Z = (float)Math.Cos(newAngle) * lengthXZ;
            }
        }

        public Vector3D Reflected(Vector3D surfaceNormal)
        {
            Vector3D normalizedNormal = surfaceNormal.Normalized();
            float dotProduct = DotProduct(this, normalizedNormal);
            Vector3D dunno = (normalizedNormal * 2) * dotProduct;

            return this - dunno;
        }
        //http://en.wikipedia.org/wiki/Snell's_law#Vector_form
        public Vector3D Refracted(Vector3D surfaceNormal, float refractIndexFrom, float refractIndexTo)
        {
            Vector3D negNormal = (-surfaceNormal).Normalized();
            Vector3D thisNormalized = this.Normalized();

            float cos1 = DotProduct(thisNormalized, negNormal);
            if (cos1 < 0)
            {
                cos1 = DotProduct(thisNormalized, surfaceNormal.Normalized());
            }

            float refractRatio = refractIndexFrom / refractIndexTo;
            float otherSine = refractRatio * (float)Math.Sqrt(1 - (cos1 * cos1));

            if (otherSine > 1) //Total internal reflection
            {
                //return new RefractionInfo(true, GetReflected(surfaceNormal));
                return this.Reflected(surfaceNormal);
            }
            else
            {
                float cos2 = (float)Math.Sqrt(1 - (otherSine * otherSine));
                float nMult = (refractRatio * cos1) - cos2;

                return (thisNormalized * refractRatio) + (surfaceNormal * nMult);

                //return new RefractionInfo(false,
                //        Vector3D.Add(
                //                Vector3D.Multiply(thisNormalized, refractRatio),
                //                Vector3D.Multiply(surfaceNormal, nMult)
                //                )
                //            );
            }
        }

        public static bool operator ==(Vector3D vector1, Vector3D vector2)
        {
            return vector1.X == vector2.X && vector1.Y == vector2.Y && vector1.Z == vector2.Z;
        }
        public static bool operator !=(Vector3D vector1, Vector3D vector2)
        {
            return !(vector1 == vector2);
        }
        public override int GetHashCode()
        {
            int hash = 13;
            hash = hash * 47 + X.GetHashCode();
            hash = hash * 47 + Y.GetHashCode();
            hash = hash * 47 + Z.GetHashCode();
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
            return new Vector3D(vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z);
        }
        public static Vector3D operator -(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static Vector3D operator /(Vector3D vector, float divisor)
        {
            return new Vector3D(vector.X / divisor, vector.Y / divisor, vector.Z / divisor);
        }
        public static Vector3D operator *(Vector3D vector, float multiplier)
        {
            return new Vector3D(vector.X * multiplier, vector.Y * multiplier, vector.Z * multiplier);
        }

        public static Vector3D operator -(Vector3D vector)
        {
            return new Vector3D(-vector.X, -vector.Y, vector.Z);
        }

        public override string ToString()
        {
            return "X=" + X + " Y=" + Y + " Z=" + Z;
        }
    }
}
