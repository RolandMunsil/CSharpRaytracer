using PixelWindowSDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    //TODO: should camera rays all originate from a single point? Should they originate from a plane?
    //TODO: Orthographic camera
    class Camera
    {
        public Point3D position;
        Matrix3x3 rotationMatrix;
        public double zoom;

        public Camera(Point3D position, Point3D lookingAt)
        {
            this.position = position;
            Vector3D direction = lookingAt - position;

            rotationMatrix = Matrix3x3.RotationAboutYAxis(direction.AngleAboutYAxis) * Matrix3x3.RotationAboutXAxis(direction.AngleFromHorizontalPlane);
        }

        public Ray RayAtPixel(double x, double y, PixelWindow window)
        {
            double adjX = x - (window.ClientWidth / 2.0);
            double adjY = y - (window.ClientHeight / 2.0);

            adjX /= zoom;
            adjY /= zoom;

            return new Ray(position, rotationMatrix * new Vector3D(adjX, adjY, 1));
        }

        //TODO: better name
        public void ChangePositionAndLookingAt(Point3D newPosition, Point3D newLookingAt)
        {
            this.position = newPosition;
            Vector3D direction = newLookingAt - newPosition;
            double facingAngleHoriz = direction.AngleAboutYAxis;
            double facingAngleVert = direction.AngleFromHorizontalPlane;

            rotationMatrix = Matrix3x3.RotationAboutYAxis(facingAngleHoriz) * Matrix3x3.RotationAboutXAxis(facingAngleVert);
        }
    }
}
