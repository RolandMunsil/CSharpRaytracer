using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer
{
    struct LinearEquation
    {
        public float slope;
        public float intercept;

        public LinearEquation(float slope, float intercept)
        {
            this.slope = slope;
            this.intercept = intercept;
        }

        public float ValueAt(float t)
        {
            return (t * slope) + intercept;
        }

        public float SolveWhenValueIs(float value)
        {
            return (value - intercept) / slope;
        }

        public static LinearEquation operator +(LinearEquation eqn, float value)
        {
            return new LinearEquation(eqn.slope, eqn.intercept + value);
        }
        public static LinearEquation operator -(LinearEquation eqn, float value)
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
        public float quadCoefficient;
        public float linearCoefficient;
        public float constant;

        public QuadraticEquation(float quadCoefficient, float linearCoefficient, float constant)
        {
            this.quadCoefficient = quadCoefficient;
            this.linearCoefficient = linearCoefficient;
            this.constant = constant;
        }

        public float ValueAt(float t)
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
        public static QuadraticEquation operator -(QuadraticEquation eqn, float value)
        {
            return new QuadraticEquation(
                eqn.quadCoefficient,
                eqn.linearCoefficient,
                eqn.constant - value);
        }
    }
}
