using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Ray
    {
        public Ray(Point3D point, Vector3D direction);

        public Point3D PointAt(float value)
        {
            throw new NotImplementedException();
        }

        public Vector3D ToVector3D()
        {
            throw new NotImplementedException();
        }
    }
}
