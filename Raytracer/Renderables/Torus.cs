using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer.Renderables
{
    class Torus : Renderable
    {
        Point3D position;

        double torusRadius;
        double tubeRadius;

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
