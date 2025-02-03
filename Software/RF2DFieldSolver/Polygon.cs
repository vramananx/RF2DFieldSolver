using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RF2DFieldSolver
{
    public static class Polygon
    {
        public static bool SelfIntersects(List<PointF> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                int prev = i - 1;
                if (prev < 0)
                {
                    prev = vertices.Count - 1;
                }
                var p0 = vertices[prev];
                var p1 = vertices[i];
                var line0 = new Line(p0, p1);
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    int prev2 = j - 1;
                    if (prev2 < 0)
                    {
                        prev2 = vertices.Count - 1;
                    }
                    var p2 = vertices[prev2];
                    var p3 = vertices[j];
                    var line1 = new Line(p2, p3);
                    PointF intersectPoint;
                    var intersect = line0.Intersects(line1, out intersectPoint);
                    if (intersect == Line.IntersectionType.BoundedIntersection)
                    {
                        if (j == i + 1 || (i == 0 && j == vertices.Count - 1))
                        {
                            PointF common, pl1, pl2;
                            if (j == i + 1)
                            {
                                common = p1;
                                pl1 = p0;
                                pl2 = p3;
                            }
                            else
                            {
                                common = p3;
                                pl1 = p1;
                                pl2 = p2;
                            }
                            var commonIntersectDist = new Line(common, intersectPoint).Length();
                            var pl1IntersectDist = new Line(pl1, intersectPoint).Length();
                            var pl2IntersectDist = new Line(pl2, intersectPoint).Length();
                            if (commonIntersectDist < pl1IntersectDist && commonIntersectDist < pl2IntersectDist)
                            {
                                continue;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<PointF> Offset(List<PointF> vertices, double offset)
        {
            var ret = new List<PointF>();
            if (vertices.Count < 3)
            {
                return vertices;
            }

            if (IsClockwise(vertices))
            {
                offset = -offset;
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                var pp = vertices[(i + vertices.Count - 1) % vertices.Count];
                var pc = vertices[i];
                var pn = vertices[(i + vertices.Count + 1) % vertices.Count];

                var line0 = new Line(pp, pc);
                var normal0 = line0.NormalVector();
                normal0.Length = offset;
                line0.Translate(normal0.Dx, normal0.Dy);

                var line1 = new Line(pc, pn);
                var normal1 = line1.NormalVector();
                normal1.Length = offset;
                line1.Translate(normal1.Dx, normal1.Dy);

                PointF point;
                var intersect = line0.Intersects(line1, out point);
                if (intersect != Line.IntersectionType.NoIntersection)
                {
                    ret.Add(point);
                }
            }
            return ret;
        }

        public static bool IsClockwise(List<PointF> vertices)
        {
            double edgeSum = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                var pp = vertices[(i + vertices.Count - 1) % vertices.Count];
                var pc = vertices[i];
                edgeSum += (pc.X - pp.X) * (pc.Y + pp.Y);
            }
            return edgeSum > 0;
        }
    }

    public class Line
    {
        public PointF Start { get; }
        public PointF End { get; }
        public double Length { get; set; }

        public Line(PointF start, PointF end)
        {
            Start = start;
            End = end;
            Length = Math.Sqrt(Math.Pow(End.X - Start.X, 2) + Math.Pow(End.Y - Start.Y, 2));
        }

        public Line NormalVector()
        {
            return new Line(new PointF(0, 0), new PointF(-(End.Y - Start.Y), End.X - Start.X));
        }

        public void Translate(double dx, double dy)
        {
            Start = new PointF(Start.X + (float)dx, Start.Y + (float)dy);
            End = new PointF(End.X + (float)dx, End.Y + (float)dy);
        }

        public enum IntersectionType
        {
            NoIntersection,
            BoundedIntersection,
            UnboundedIntersection
        }

        public IntersectionType Intersects(Line other, out PointF intersection)
        {
            intersection = new PointF();
            float a1 = End.Y - Start.Y;
            float b1 = Start.X - End.X;
            float c1 = a1 * Start.X + b1 * Start.Y;

            float a2 = other.End.Y - other.Start.Y;
            float b2 = other.Start.X - other.End.X;
            float c2 = a2 * other.Start.X + b2 * other.Start.Y;

            float delta = a1 * b2 - a2 * b1;
            if (delta == 0)
            {
                return IntersectionType.NoIntersection;
            }

            intersection = new PointF(
                (b2 * c1 - b1 * c2) / delta,
                (a1 * c2 - a2 * c1) / delta
            );

            if (IsPointOnLine(intersection) && other.IsPointOnLine(intersection))
            {
                return IntersectionType.BoundedIntersection;
            }

            return IntersectionType.UnboundedIntersection;
        }

        private bool IsPointOnLine(PointF point)
        {
            return Math.Min(Start.X, End.X) <= point.X && point.X <= Math.Max(Start.X, End.X) &&
                   Math.Min(Start.Y, End.Y) <= point.Y && point.Y <= Math.Max(Start.Y, End.Y);
        }
    }
}
