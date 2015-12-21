using PixelWindowSDL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class YPlane : Renderable
    {
        private enum ColoringMode
        {
            SingleColor,
            Checkerboard,
            Texture
        }

        double Y;
        private static readonly Vector3D Normal = new Vector3D(0, 1, 0);

        ColoringMode coloringMode;
        Color color;
        System.Drawing.Bitmap bm;

        public YPlane(double y)
        {
            this.Y = y;
            coloringMode = ColoringMode.Checkerboard;
        }

        public YPlane(double y, String textureFileName)
        {
            this.Y = y;
            coloringMode = ColoringMode.Texture;
            String s = Path.GetFullPath("../../../Textures/" + textureFileName);
            bm = new System.Drawing.Bitmap(s);
        }

        public YPlane(double y, Color color)
        {
            this.Y = y;
            coloringMode = ColoringMode.SingleColor;
            this.color = color;
        }

        public override Intersection GetNearestIntersection(Ray ray)
        {
            if(ray.Direction.y == 0)
            {
                if(ray.Origin.y == Y)
                {
                    return new Intersection
                    {
                        value = 0,
                        normal = YPlane.Normal,
                        color = ColorAt(ray.Origin.x, ray.Origin.z)
                    };
                }
                else
                {
                    return Intersection.None;
                }
            }

            double value = ray.ValueWhenYIs(Y);
            Point3D point = ray.PointAt(value);
            if (value >= Intersection.MinValue)
            {
                return new Intersection
                {
                    value = value,
                    color = ColorAt(point.x, point.z),
                    normal = YPlane.Normal
                };
            }
            else
            {
                return Intersection.None;
            }
        }

        public override Intersection[] GetAllIntersections(Ray ray)
        {
            Intersection i = GetNearestIntersection(ray);
            if (i == Intersection.None)
            {
                return Intersection.NoneArray;
            }
            else
            {
                return new[] { i };
            }
        }

        public override bool Contains(Point3D point)
        {
            return point.y == Y;
        }

        private Color ColorAt(double x, double z)
        {
            switch(coloringMode)
            {
                case ColoringMode.SingleColor: return color;
                case ColoringMode.Checkerboard: return x.PMod(200) < 100 ^ z.PMod(200) < 100 ? Color.White : Color.Black;
                case ColoringMode.Texture:
                {
                    lock (bm)
                    {
                        return new Color(bm.GetPixel((int)(x / 2).PMod(bm.Width), (int)(z / 2).PMod(bm.Height)).ToArgb());
                    }
                } 
                default: throw new InvalidOperationException("coloringMode has not been initialized.");
            }
        }
    }
}
