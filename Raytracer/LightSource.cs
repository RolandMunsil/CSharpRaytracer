using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class LightSource
    {
        public Point3D position;
        public double maxLitDistance;
        //Color lightColor;

        public LightSource();
        public LightSource(double x, double y, double z)
            : this(new Point3D(x, y, z)) { }

        public LightSource(Point3D position, double maxLitDistance = 0)
        {
            this.position = position;
            this.maxLitDistance = maxLitDistance;
        }
    }
}
