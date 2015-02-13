using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Ray
    {
        public LinearEquation XEquation;
        public LinearEquation YEquation;
        public LinearEquation ZEquation;

        public Point3D Origin
        {
            get
            {
                return new Point3D(XEquation.intercept, YEquation.intercept, ZEquation.intercept);
            }
        }

        public Ray(Point3D origin, Vector3D direction)
        {
            XEquation = new LinearEquation(direction.X, origin.X);
            YEquation = new LinearEquation(direction.Y, origin.Y);
            ZEquation = new LinearEquation(direction.Z, origin.Z);
        }

        public Point3D PointAt(float t)
        {
            return new Point3D(XEquation.ValueAt(t), YEquation.ValueAt(t), ZEquation.ValueAt(t));
        }

        public Vector3D ToVector3D()
        {
            return new Vector3D(XEquation.slope, YEquation.slope, ZEquation.slope);
        }
    }
}
