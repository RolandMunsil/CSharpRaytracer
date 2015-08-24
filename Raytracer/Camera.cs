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
        public enum Projection
        {
            //TODO: implement orthographic projection
            Orthographic,
            Perspective,
        }

        public Point3D position;
        public double facingAngleHoriz;
        public double facingAngleVert;

        Projection projection;

        //todo: figure out more about this. size, camera as plane, camera as point, etc.
        public double focalLength;
        public double zoom;

        public Camera(Point3D position, Point3D lookingAt, Projection projection)
        {
            this.position = position;
            Vector3D direction = lookingAt - position;
            facingAngleHoriz = direction.AngleXZ;
            facingAngleVert = direction.AngleFromHorizontalPlane;

            this.projection = projection;
        }

        public Ray RayAtPixel(double x, double y, PixelWindow window)
        {
            double sortaFov = 1 / focalLength;
            double adjX = x - (window.ClientWidth / 2.0);
            double adjY = y - (window.ClientHeight / 2.0);

            adjX /= zoom;
            adjY /= zoom;

            Vector3D direction = new Vector3D(adjX * sortaFov, adjY * sortaFov, 1);
            direction.Rotate(facingAngleHoriz, facingAngleVert);
            return new Ray(position, direction);
        }

        //TODO: better name
        public void ChangePositionAndLookingAt(Point3D newPosition, Point3D newLookingAt)
        {
            this.position = newPosition;
            Vector3D direction = newLookingAt - newPosition;
            facingAngleHoriz = direction.AngleXZ;
            facingAngleVert = direction.AngleFromHorizontalPlane;
        }
    }
}
