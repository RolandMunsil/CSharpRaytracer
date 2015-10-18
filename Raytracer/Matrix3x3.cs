using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    struct Matrix3x3
    {
        //Order: row, column
        double[,] elements;

        public double this[int row, int column]
        {
            get
            {
                return elements[row, column];
            }
            set
            {
                elements[row, column] = value;
            }
        }

        public static Matrix3x3 RotationAboutXAxis(double theta)
        {
            double cosθ = Math.Cos(theta);
            double sinθ = Math.Sin(theta);

            Matrix3x3 matrix = new Matrix3x3();
            matrix.elements = new double[3, 3];

            matrix[0, 0] = 1;

            matrix[1, 1] = cosθ;
            matrix[1, 2] = sinθ;
            matrix[2, 1] = -sinθ;
            matrix[2, 2] = cosθ;

            return matrix;
        }

        public static Matrix3x3 RotationAboutYAxis(double theta)
        {
            double cosθ = Math.Cos(theta);
            double sinθ = Math.Sin(theta);

            Matrix3x3 matrix = new Matrix3x3();
            matrix.elements = new double[3, 3];

            matrix[1, 1] = 1;

            matrix[0, 0] = cosθ;
            matrix[0, 2] = sinθ;
            matrix[2, 0] = -sinθ;
            matrix[2, 2] = cosθ;

            return matrix;
        }

        public static Matrix3x3 RotationAboutZAxis(double theta)
        {
            double cosθ = Math.Cos(theta);
            double sinθ = Math.Sin(theta);

            Matrix3x3 matrix = new Matrix3x3();
            matrix.elements = new double[3, 3];

            matrix[2, 2] = 1;

            matrix[0, 0] = cosθ;
            matrix[0, 1] = -sinθ;
            matrix[1, 0] = sinθ;
            matrix[1, 1] = cosθ;

            return matrix;
        }

        //TODO: figure out why it seems like rotations have to be done in reverse order
        public static Matrix3x3 operator *(Matrix3x3 m1, Matrix3x3 m2)
        {
            Matrix3x3 matrix = new Matrix3x3();
            matrix.elements = new double[3, 3];

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    matrix[row, column] = m1[row, 0] * m2[0, column] +
                                          m1[row, 1] * m2[1, column] +
                                          m1[row, 2] * m2[2, column];
                }
            }

            return matrix;
        }

        public static Vector3D operator *(Matrix3x3 m1, Vector3D v)
        {
            return new Vector3D
            {
                x = (m1[0, 0] * v.x) + (m1[0, 1] * v.y) + (m1[0, 2] * v.z),
                y = (m1[1, 0] * v.x) + (m1[1, 1] * v.y) + (m1[1, 2] * v.z),
                z = (m1[2, 0] * v.x) + (m1[2, 1] * v.y) + (m1[2, 2] * v.z),

            };
        }

        public static implicit operator Matrix3x3(double[,] array2D)
        {
            if (array2D.GetLength(0) == 3 && array2D.GetLength(1) == 3)
            {
                return new Matrix3x3 { elements = array2D };
            }
            throw new InvalidCastException();
        }
    }
}
