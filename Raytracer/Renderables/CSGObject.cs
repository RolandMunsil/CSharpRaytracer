using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class CSGObject : Renderable
    {
        enum Operation
        {
            OuterShellOnly,
            Intersection,
            ExclusiveOr,
            FirstWithoutSecond,
        }

        Renderable renderable1;
        Renderable renderable2;
    }
}
