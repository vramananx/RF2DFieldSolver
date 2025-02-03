using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RF2DFieldSolver
{
    public class Gauss
    {
        public static double GetCharge(Laplace laplace, ElementList list, Element e, double gridSize, double distance)
        {
            // Extend the element polygon a bit
            var integral = Polygon.Offset(e.Vertices, distance);

            double chargeSum = 0;
            for (int i = 0; i < integral.Count; i++)
            {
                var pp = integral[(i + integral.Count - 1) % integral.Count];
                var pc = integral[i];

                var increment = new PointF(pc.X - pp.X, pc.Y - pp.Y);
                var unitVector = new PointF(increment.X, increment.Y);
                var length = Math.Sqrt(unitVector.X * unitVector.X + unitVector.Y * unitVector.Y);
                unitVector.X /= (float)length;
                unitVector.Y /= (float)length;

                int points = (int)Math.Ceiling(length / gridSize);
                double stepSize = length / points;
                increment.X *= (float)(stepSize / length);
                increment.Y *= (float)(stepSize / length);

                var point = new PointF(pp.X + increment.X / 2, pp.Y + increment.Y / 2);
                for (int j = 0; j < points; j++)
                {
                    var gradient = laplace.GetGradient(point);
                    if (list != null)
                    {
                        var dielectricConstant = list.GetDielectricConstantAt(point);
                        gradient.X *= (float)dielectricConstant;
                        gradient.Y *= (float)dielectricConstant;
                    }
                    // Get amount of gradient that is perpendicular to our integration line
                    double perp = gradient.X * unitVector.Y - gradient.Y * unitVector.X;
                    perp *= stepSize / gridSize;
                    chargeSum += perp;
                    point.X += increment.X;
                    point.Y += increment.Y;
                }
            }

            if (!Polygon.IsClockwise(integral))
            {
                chargeSum *= -1;
            }

            return chargeSum;
        }
    }
}
