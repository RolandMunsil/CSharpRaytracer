using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    struct LinearEquation
    {
        public double slope;
        public double intercept;

        public LinearEquation(double slope, double intercept)
        {
            this.slope = slope;
            this.intercept = intercept;
        }

        public double ValueAt(double t)
        {
            return (t * slope) + intercept;
        }

        public double SolveWhenValueIs(double value)
        {
            return (value - intercept) / slope;
        }

        public static LinearEquation operator +(LinearEquation eqn, double value)
        {
            return new LinearEquation(eqn.slope, eqn.intercept + value);
        }
        public static LinearEquation operator -(LinearEquation eqn, double value)
        {
            return new LinearEquation(eqn.slope, eqn.intercept - value);
        }

        public static QuadraticEquation operator *(LinearEquation eqn1, LinearEquation eqn2)
        {
            return new QuadraticEquation(
				eqn1.slope * eqn2.slope,
				(eqn1.slope * eqn2.intercept) + (eqn2.slope * eqn1.intercept),
				eqn1.intercept * eqn2.intercept);
        }
    }

    struct QuadraticEquation
    {
        public double quadCoefficient;
        public double linearCoefficient;
        public double constant;

        public QuadraticEquation(double quadCoefficient, double linearCoefficient, double constant)
        {
            this.quadCoefficient = quadCoefficient;
            this.linearCoefficient = linearCoefficient;
            this.constant = constant;
        }

        public double ValueAt(double t)
        {
            return (t * t * quadCoefficient) + (t * linearCoefficient) + constant;
        }

        public static QuadraticEquation operator +(QuadraticEquation eqn1, QuadraticEquation eqn2)
        {
            return new QuadraticEquation(
                eqn1.quadCoefficient + eqn2.quadCoefficient,
                eqn1.linearCoefficient + eqn2.linearCoefficient,
                eqn1.constant + eqn2.constant);
        }
        public static QuadraticEquation operator -(QuadraticEquation eqn, double value)
        {
            return new QuadraticEquation(
                eqn.quadCoefficient,
                eqn.linearCoefficient,
                eqn.constant - value);
        }
    }
}
