using PixelWindowCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Camera
    {
        public Point3D location;
        public Vector3D facingDirection;

        //camera type (orthographic / perspective)
        //zoom / focal length

        public Ray RayAtPixel(int x, int y, PixelWindow window)
        {
            throw new NotImplementedException();
        }
    }
}
