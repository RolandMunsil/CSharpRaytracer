using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    [DebuggerDisplay("({x}, {y}, {z})")]
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
        public double AngleXZ
        {
            get
            {
                return Math.Atan2(x, z);
            }
        }

        ///// <summary>
        ///// Some example values:
        ///// if the vector points directly +y, this will return π/2
        ///// if the vector points directly +z, this will return 0
        ///// if the vector points directly -y, this will return -π/2,
        ///// etc.
        ///// </summary>
        //public double AngleYZ
        //{
        //    get
        //    {
        //        return Math.Atan2(Y, Z);
        //    }
        //}

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
                return Math.Sqrt(x * x + y * y + z * z);
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

        public Vector3D Reflected(Vector3D surfaceNormal)
        {
            Vector3D n = surfaceNormal.Normalized();
            Vector3D l = this.Normalized();

            double cosθ1 = DotProduct(-n, l);
            if (cosθ1 <= 0)
            {
                n = -n;
                cosθ1 = DotProduct(-n, l);
            }
            else
            {
                if (DotProduct(n, l) > 0)
                {
                    Debugger.Break();
                }
            }

            return l + 2 * cosθ1 * n;

            //Vector3D normalizedNormal = surfaceNormal.Normalized();
            //double dotProduct = DotProduct(this, normalizedNormal);
            //Vector3D dunno = (normalizedNormal * 2) * dotProduct;

            //return this - dunno;
        }

        //Alright so clearly something else is causing the problem. The refraction equation is correct but something else is screwing it up.
        //Idea: compare Java calculations to this calculations on the same scene.

        //http://en.wikipedia.org/wiki/Snell's_law#Vector_form
        public Vector3D Refracted(Vector3D surfaceNormal, double refractIndexFrom, double refractIndexTo, out bool totalInternalReflection)
        {
            //totalInternalReflection = false;
            //return RefractSlow(surfaceNormal, this, refractIndexFrom, refractIndexTo);

            return RefractedV2(surfaceNormal, refractIndexFrom, refractIndexTo, out totalInternalReflection);

            //Vector3D normal = (surfaceNormal).Normalized();
            //Vector3D thisNormalized = this.Normalized();

            //double cos1 = DotProduct(thisNormalized, -normal);
            //if (cos1 < 0)
            //{
            //    normal = -normal;
            //    cos1 = DotProduct(thisNormalized, normal);
            //}

            //double refractRatio = refractIndexFrom / refractIndexTo;

            //double otherSine = refractRatio * Math.Sqrt(1 - (cos1 * cos1));

            //if (otherSine > 1) //Total internal reflection
            //{
            //    totalInternalReflection = true;
            //    Vector3D ret = this.Reflected(normal);
            //    return ret;
            //}
            //else
            //{
            //    double cos2 = Math.Sqrt(1 - (otherSine * otherSine));
            //    double nMult = (refractRatio * cos1) - cos2;

            //    totalInternalReflection = false;
            //    Vector3D ret = (thisNormalized * refractRatio) + (normal * nMult);
            //    return ret;
            //}
        }

        public Vector3D RefractedJava(Vector3D surfaceNormal, double refractIndexFrom, double refractIndexTo, out bool totalInternalReflection)
        {
            Vector3D negNormal = new Vector3D(-surfaceNormal.x, -surfaceNormal.y, -surfaceNormal.z);
            negNormal.Normalize();
            this.Normalize();

            double cos1 = Vector3D.DotProduct(this, negNormal);
            if (cos1 < 0)
            {
                cos1 = Vector3D.DotProduct(this, surfaceNormal.Normalized());

                //int k = 1 / 0;
            }

            double refractRatio = refractIndexFrom / refractIndexTo;
            double otherSine = refractRatio * Math.Sqrt(1 - (cos1 * cos1));

            if (otherSine > 1)
            {
                //Total internal reflection
                totalInternalReflection = true;
                return ReflectedJava(surfaceNormal);
            }
            else
            {
                double cos2 = Math.Sqrt(1 - (otherSine * otherSine));
                double nMult = (refractRatio * cos1) - cos2;

                totalInternalReflection = false;
                return (this * refractRatio) + (surfaceNormal * nMult);

                //return new RefractionInfo(false,
                //        Vector3D.Add(
                //                Vector3D.Multiply(this, refractRatio),
                //                Vector3D.Multiply(surfaceNormal, nMult)
                //                )
                //            );
            }
        }

        public Vector3D ReflectedJava(Vector3D surfaceNormal)
        {
            Vector3D normalizedNormal = surfaceNormal.Normalized();
            double dotProduct = DotProduct(this, normalizedNormal);
            Vector3D dunno = (normalizedNormal * 2) * dotProduct;

            return this - dunno;
        }

        public Vector3D RefractedV2(Vector3D surfaceNormal, double refractIndexFrom, double refractIndexTo, out bool totalInternalReflection)
        {
            double n1 = refractIndexFrom;
            double n2 = refractIndexTo;

            Vector3D n = surfaceNormal.Normalized();
            Vector3D l = this.Normalized();

            double cosθ1 = DotProduct(-n, l);
            if (cosθ1 <= 0)
            {
                n = -n;
                cosθ1 = DotProduct(-n, l);
            }
            else
            {
                if (DotProduct(n, l) > 0)
                {
                    Debugger.Break();
                }
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


            double inSnell = n1 * Math.Sin(AngleBetween(n, -l));
            double outSnell = n2 * Math.Sin(AngleBetween(n, -vrefract));
            if (Math.Abs(inSnell - outSnell) > 0.01)
            {
                Debugger.Break();
            }


            return vrefract;
        }

        Vector3D RefractSlow(Vector3D N, Vector3D I, double ki, double kr)
        {
            double r = ki / kr, r2 = r * r;
            double invr = kr / ki, invr2 = invr * invr;

            double ndoti, two_ndoti, ndoti2, a, b, b2, D2;
            Vector3D T = new Vector3D();
            ndoti = N.x * I.x + N.y * I.y + N.z * I.z;     // 3 mul, 2 add
            ndoti2 = ndoti * ndoti;                    // 1 mul
            if (ndoti >= 0.0) { b = r; b2 = r2; } else { b = invr; b2 = invr2; }
            D2 = 1.0f - b2 * (1.0f - ndoti2);

            if (D2 >= 0.0f)
            {
                if (ndoti >= 0.0f)
                    a = b * ndoti - Math.Sqrt(D2); // 2 mul, 3 add, 1 sqrt
                else
                    a = b * ndoti + Math.Sqrt(D2);
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

        public override string ToString()
        {
            return "X=" + x + " Y=" + y + " Z=" + z;
        }
    }
}
