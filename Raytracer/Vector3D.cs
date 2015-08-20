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
        public float x;
        public float y;
        public float z;

        public Vector3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
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
                return (float)Math.Atan2(x, z);
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
                return (float)Math.Atan2(onVerticalPlane.y, onVerticalPlane.z);
            }
        }

        public float LengthSquared
        {
            get
            {
                return x * x + y * y + z * z;
            }
        }

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
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
            return (vector1.x * vector2.x) + (vector1.y * vector2.y) + (vector1.z * vector2.z);
        }

        public void Rotate(float rotationXZ, float rotationYZ)
        {
            //todo: optimize the heck out of this function

            if (rotationYZ != 0)
            {
                //First do vertical
                float lengthYZ = (float)Math.Sqrt(y * y + z * z);
                float angleYZ = (float)Math.Atan2(y, z);

                float newAngle = angleYZ + rotationYZ;
                this.y = (float)Math.Sin(newAngle) * lengthYZ;
                this.z = (float)Math.Cos(newAngle) * lengthYZ;
            }

            if (rotationXZ != 0)
            {
                //Now do horizontal
                float lengthXZ = (float)Math.Sqrt(x * x + z * z);
                float angleXZ = (float)Math.Atan2(x, z);

                float newAngle = angleXZ + rotationXZ;
                this.x = (float)Math.Sin(newAngle) * lengthXZ;
                this.z = (float)Math.Cos(newAngle) * lengthXZ;
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
            ndoti = N.x * I.x + N.y * I.y + N.z * I.z;     // 3 mul, 2 add
            ndoti2 = ndoti * ndoti;                    // 1 mul
            if (ndoti >= 0.0) { b = r; b2 = r2; } else { b = invr; b2 = invr2; }
            D2 = 1.0f - b2 * (1.0f - ndoti2);

            if (D2 >= 0.0f)
            {
                if (ndoti >= 0.0f)
                    a = b * ndoti - (float)Math.Sqrt(D2); // 2 mul, 3 add, 1 sqrt
                else
                    a = b * ndoti + (float)Math.Sqrt(D2);
                T.x = a * N.x - b * I.x;     // 6 mul, 3 add
                T.y = a * N.y - b * I.y;     // ----totals---------
                T.z = a * N.z - b * I.z;     // 12 mul, 8 add, 1 sqrt!
            }
            else
            {
                // total internal reflection
                // this usually doesn't happen, so I don't count it.
                two_ndoti = ndoti + ndoti;         // +1 add
                T.x = two_ndoti * N.x - I.x;      // +3 adds, +3 muls
                T.y = two_ndoti * N.y - I.y;
                T.z = two_ndoti * N.z - I.z;
            }
            return T;
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
        public static Vector3D operator /(Vector3D vector, float divisor)
        {
            return new Vector3D(vector.x / divisor, vector.y / divisor, vector.z / divisor);
        }
        public static Vector3D operator *(Vector3D vector, float multiplier)
        {
            return new Vector3D(vector.x * multiplier, vector.y * multiplier, vector.z * multiplier);
        }

        public static Vector3D operator -(Vector3D vector)
        {
            return new Vector3D(-vector.x, -vector.y, -vector.z);
        }

        public override string ToString()
        {
            return "X=" + x + " Y=" + y + " Z=" + z;
        }
    }
}
