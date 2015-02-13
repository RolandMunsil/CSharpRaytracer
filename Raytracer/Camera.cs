﻿using PixelWindowCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Camera
    {
        //TODO: should the camera be like a plane or a point?

        public enum Projection
        {
            Orthographic,
            Perspective,
        }

        public Point3D position;
        public float facingAngleHoriz;
        public float facingAngleVert;

        Projection projection;

        //todo: figure out more about this. size, camera as plane, camera as point, etc.
        float focalLength;
        float zoom;

        public Camera(Point3D position, Point3D lookingAt, Projection projection, float focalLength, float zoom)
        {
            this.position = position;
            Vector3D direction = lookingAt - position;
            facingAngleHoriz = direction.AngleXZ;
            facingAngleVert = direction.AngleYZ;

            this.projection = projection;
            this.focalLength = focalLength;
            this.zoom = zoom;
        }

        public Ray RayAtPixel(int x, int y, PixelWindow window)
        {
            float sortaFov = 1 / focalLength;
            float adjX = x - (window.ClientWidth / 2f);
            float adjY = y - (window.ClientHeight / 2f);

            adjX /= zoom;
            adjY /= zoom;

            Vector3D direction = new Vector3D(adjX * sortaFov, adjY * sortaFov, 1);
            direction.Rotate(facingAngleHoriz, facingAngleVert);
            return new Ray(position, direction);
        }
    }
}
