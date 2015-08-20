using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    [DebuggerDisplay("({X}, {Y}, {Z})")]
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

        ///// <summary>
        ///// Some example values:
        ///// if the vector points directly +y, this will return π/2
        ///// if the vector points directly +z, this will return 0
        ///// if the vector points directly -y, this will return -π/2,
        ///// etc.
        ///// </summary>
        //public float AngleYZ
        //{
        //    get
        //    {
        //        return (float)Math.Atan2(Y, Z);
        //    }
        //}

        public float AngleFromHorizontalPlane
        {
            get
            {
                Vector3D onVerticalPlane = this.Rotated(-AngleXZ, 0);
                return (float)Math.Atan2(onVerticalPlane.Y, onVerticalPlane.Z);
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
            //todo: optimize the heck out of this function

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

        public Vector3D Rotated(float rotationXZ, float rotationYZ)
        {
            //todo: optimize the heck out of this function

            Vector3D copy = this;
            copy.Rotate(rotationXZ, rotationYZ);
            return copy;
        }

        public Vector3D Reflected(Vector3D surfaceNormal)
        {
            Vector3D normalizedNormal = surfaceNormal.Normalized();
            float dotProduct = DotProduct(this, normalizedNormal);
            Vector3D dunno = (normalizedNormal * 2) * dotProduct;

            return this - dunno;
        }
        //http://en.wikipedia.org/wiki/Snell's_law#Vector_form
        public Vector3D Refracted(Vector3D surfaceNormal, float refractIndexFrom, float refractIndexTo, out bool totalInternalReflection)
        {
            //totalInternalReflection = false;
            //return RefractSlow(surfaceNormal, this, refractIndexFrom, refractIndexTo);

            return RefractedV2(surfaceNormal, refractIndexFrom, refractIndexTo, out totalInternalReflection);

            //Vector3D normal = (surfaceNormal).Normalized();
            //Vector3D thisNormalized = this.Normalized();

            //float cos1 = DotProduct(thisNormalized, -normal);
            //if (cos1 < 0)
            //{
            //    normal = -normal;
            //    cos1 = DotProduct(thisNormalized, normal);
            //}

            //float refractRatio = refractIndexFrom / refractIndexTo;

            //float otherSine = refractRatio * (float)Math.Sqrt(1 - (cos1 * cos1));

            //if (otherSine > 1) //Total internal reflection
            //{
            //    totalInternalReflection = true;
            //    Vector3D ret = this.Reflected(normal);
            //    return ret;
            //}
            //else
            //{
            //    float cos2 = (float)Math.Sqrt(1 - (otherSine * otherSine));
            //    float nMult = (refractRatio * cos1) - cos2;

            //    totalInternalReflection = false;
            //    Vector3D ret = (thisNormalized * refractRatio) + (normal * nMult);
            //    return ret;
            //}
        }

        public Vector3D RefractedV2(Vector3D surfaceNormal, float refractIndexFrom, float refractIndexTo, out bool totalInternalReflection)
        {
            float n1 = refractIndexFrom;
            float n2 = refractIndexTo;

            Vector3D n = surfaceNormal.Normalized();
            Vector3D l = this.Normalized();

            float cosθ1 = DotProduct(-n, l);
            if (cosθ1 <= 0)
            {
                n = -n;
                l = this.Normalized();

                cosθ1 = DotProduct(-n, l);
            }

            float sinθ2 = (n1 / n2) * (float)Math.Sqrt(1 - (cosθ1 * cosθ1));

            if (1 - (sinθ2 * sinθ2) < 0) { totalInternalReflection = true; return this.Reflected(surfaceNormal); }

            float cosθ2 = (float)Math.Sqrt(1 - (sinθ2 * sinθ2));
            Vector3D vrefract = l * (n1 / n2) + n * ((n1 / n2) * cosθ1 - cosθ2);
            totalInternalReflection = false;
            return vrefract;
        }

        Vector3D RefractSlow(Vector3D N, Vector3D I, float ki, float kr)
        {
            float r = ki / kr, r2 = r * r;
            float invr = kr / ki, invr2 = invr * invr;

            float ndoti, two_ndoti, ndoti2, a, b, b2, D2;
            Vector3D T = new Vector3D();
            ndoti = N.X * I.X + N.Y * I.Y + N.Z * I.Z;     // 3 mul, 2 add
            ndoti2 = ndoti * ndoti;                    // 1 mul
            if (ndoti >= 0.0) { b = r; b2 = r2; } else { b = invr; b2 = invr2; }
            D2 = 1.0f - b2 * (1.0f - ndoti2);

            if (D2 >= 0.0f)
            {
                if (ndoti >= 0.0f)
                    a = b * ndoti - (float)Math.Sqrt(D2); // 2 mul, 3 add, 1 sqrt
                else
                    a = b * ndoti + (float)Math.Sqrt(D2);
                T.X = a * N.X - b * I.X;     // 6 mul, 3 add
                T.Y = a * N.Y - b * I.Y;     // ----totals---------
                T.Z = a * N.Z - b * I.Z;     // 12 mul, 8 add, 1 sqrt!
            }
            else
            {
                // total internal reflection
                // this usually doesn't happen, so I don't count it.
                two_ndoti = ndoti + ndoti;         // +1 add
                T.X = two_ndoti * N.X - I.X;      // +3 adds, +3 muls
                T.Y = two_ndoti * N.Y - I.Y;
                T.Z = two_ndoti * N.Z - I.Z;
            }
            return T;
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
            return new Vector3D(-vector.X, -vector.Y, -vector.Z);
        }

        public override string ToString()
        {
            return "X=" + X + " Y=" + Y + " Z=" + Z;
        }
    }
}
