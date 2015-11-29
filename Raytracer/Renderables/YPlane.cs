﻿using PixelWindowSDL;
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
        double Y;
        private static readonly Vector3D Normal = new Vector3D(0, 1, 0);

        bool hasColor;
        Color color;
        System.Drawing.Bitmap bm;

        public YPlane(double y)
        {
            this.Y = y;
            hasColor = false;
        }

        public YPlane(double y, Color color)
        {
            this.Y = y;
            this.color = color;
            hasColor = true;
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
            lock (this)
            {
                if (bm == null)
                {
                    String s = Path.GetFullPath("../../../Textures/Wood Texture 01.jpg");
                    bm = new System.Drawing.Bitmap(s);
                }
                if (hasColor)
                {
                    return color;
                }
                //return x.PMod(20) < 10 ^ z.PMod(20) < 10 ? (Color)0x4A7023 : (Color)0x78AB46;
                //return x.PMod(200) < 100 ^ z.PMod(200) < 100 ? Color.White : Color.Black;
                //return x.PMod(200) < 100 ^ z.PMod(200) < 100 ? new Color(128, 64, 0) : new Color(236, 186, 94);
                return new Color(bm.GetPixel((int)(x / 2).PMod(bm.Width), (int)(z / 2).PMod(bm.Height)).ToArgb());
            }
        }
    }
}
