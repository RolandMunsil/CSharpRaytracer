using PixelWindowSDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class Camera
    {
        public Point3D position;
        //public double facingAngleHoriz;
        //public double facingAngleVert;

        Matrix3x3 rotationMatrix;

        //todo: figure out more about this. size, camera as plane, camera as point, etc.
        public double zoom;

        public Camera(Point3D position, Point3D lookingAt)
        {
            this.position = position;
            Vector3D direction = lookingAt - position;
            //facingAngleHoriz = direction.AngleXZ;
            //facingAngleVert = direction.AngleFromHorizontalPlane;

            rotationMatrix = Matrix3x3.RotationAboutYAxis(direction.AngleXZ) * Matrix3x3.RotationAboutXAxis(direction.AngleFromHorizontalPlane);
        }

        public Ray RayAtPixel(double x, double y, PixelWindow window)
        {
            double adjX = x - (window.ClientWidth / 2.0);
            double adjY = y - (window.ClientHeight / 2.0);

            adjX /= zoom;
            adjY /= zoom;
            Vector3D direction = new Vector3D(adjX, adjY, 1);

            //Vector3D direction = new Vector3D(0, 0, 1);

            //double angleX = (adjY / (window.ClientHeight / 2.0)) * (Math.PI / 4) * (900.0/1600.0);
            //double angleY = (adjX / (window.ClientWidth / 2.0)) * (Math.PI / 4);

            //direction = (Matrix3x3.RotationAboutYAxis(angleY) * Matrix3x3.RotationAboutXAxis(angleX)) * direction;

            //direction.Rotate(facingAngleHoriz, facingAngleVert);
            return new Ray(position, rotationMatrix * direction);
        }

        //TODO: better name
        public void ChangePositionAndLookingAt(Point3D newPosition, Point3D newLookingAt)
        {
            this.position = newPosition;
            Vector3D direction = newLookingAt - newPosition;
            double facingAngleHoriz = direction.AngleXZ;
            double facingAngleVert = direction.AngleFromHorizontalPlane;

            rotationMatrix = Matrix3x3.RotationAboutYAxis(facingAngleHoriz) * Matrix3x3.RotationAboutXAxis(facingAngleVert);
        }
    }
}
