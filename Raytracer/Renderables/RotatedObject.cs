using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    class RotatedObject : Renderable
    {
        Renderable baseObject;
        Point3D centerOfRotation;
        Matrix3x3 rotationMatrix;
        Matrix3x3 inverseRotationMatrix;

        public RotatedObject(Renderable baseObject, Point3D centerOfRotation, double rotationAboutY, double rotationAboutX)
        {
            this.baseObject = baseObject;
            this.centerOfRotation = centerOfRotation;
            rotationMatrix = Matrix3x3.RotationAboutYAxis(rotationAboutY) * Matrix3x3.RotationAboutXAxis(rotationAboutX);
            inverseRotationMatrix = Matrix3x3.RotationAboutXAxis(-rotationAboutX) * Matrix3x3.RotationAboutYAxis(-rotationAboutY);

            this.reflectivity = baseObject.reflectivity;
            this.refractionIndex = baseObject.refractionIndex;
            this.refractivity = baseObject.refractivity;
        }

        public override Renderable.Intersection GetNearestIntersection(Ray ray)
        {
            Ray adjRay = new Ray(RotatePointAroundCenter(ray.Origin, inverseRotationMatrix), inverseRotationMatrix * ray.Direction);
            Intersection intersection = baseObject.GetNearestIntersection(adjRay);
            intersection.normal = rotationMatrix * intersection.normal;
            return intersection;
        }

        public override Renderable.Intersection[] GetAllIntersections(Ray ray)
        {
            Ray adjRay = new Ray(RotatePointAroundCenter(ray.Origin, inverseRotationMatrix), inverseRotationMatrix * ray.Direction);
            Intersection[] intersections = baseObject.GetAllIntersections(adjRay);
            for(int i = 0; i < intersections.Length; i++)
            {
                intersections[i].normal = rotationMatrix * intersections[i].normal;
            }
            return intersections;
        }

        public override bool Contains(Point3D point)
        {
            Point3D rotatedPoint = RotatePointAroundCenter(point, inverseRotationMatrix);
            return baseObject.Contains(rotatedPoint);
        }

        private Point3D RotatePointAroundCenter(Point3D point, Matrix3x3 rotationMatrix)
        {
            return centerOfRotation + (rotationMatrix * (point - centerOfRotation));
        }
    }
}
