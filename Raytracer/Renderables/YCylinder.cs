using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Renderables
{
    class YCylinder : Renderable
    {
        double x;
        double z;
        double radius;

        double yTop;
        double yBottom;

        public override Renderable.Intersection GetNearestIntersection(Ray ray)
        {
            throw new NotImplementedException();
        }

        public override Renderable.Intersection[] GetAllIntersections(Ray ray)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(Point3D point)
        {
            throw new NotImplementedException();
        }
    }
}
