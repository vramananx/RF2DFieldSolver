using System;
using System.Drawing;

namespace Util
{
    public static class Util
    {
        public static T Scale<T>(T value, T fromLow, T fromHigh, T toLow, T toHigh, bool logFrom = false, bool logTo = false) where T : IConvertible
        {
            double normalized;
            if (logFrom)
            {
                normalized = Math.Log10(Convert.ToDouble(value) / Convert.ToDouble(fromLow)) / Math.Log10(Convert.ToDouble(fromHigh) / Convert.ToDouble(fromLow));
            }
            else
            {
                normalized = (Convert.ToDouble(value) - Convert.ToDouble(fromLow)) / (Convert.ToDouble(fromHigh) - Convert.ToDouble(fromLow));
            }
            if (logTo)
            {
                value = (T)Convert.ChangeType(Convert.ToDouble(toLow) * Math.Pow(10.0, normalized * Math.Log10(Convert.ToDouble(toHigh) / Convert.ToDouble(toLow))), typeof(T));
            }
            else
            {
                value = (T)Convert.ChangeType(normalized * (Convert.ToDouble(toHigh) - Convert.ToDouble(toLow)) + Convert.ToDouble(toLow), typeof(T));
            }
            return value;
        }

        public static double DistanceToLine(PointF point, PointF l1, PointF l2, out PointF closestLinePoint, out double pointRatio)
        {
            var M = new PointF(l2.X - l1.X, l2.Y - l1.Y);
            var t0 = (M.X * (point.X - l1.X) + M.Y * (point.Y - l1.Y)) / (M.X * M.X + M.Y * M.Y);
            PointF closestPoint;
            if (t0 <= 0)
            {
                closestPoint = l1;
                t0 = 0;
            }
            else if (t0 >= 1)
            {
                closestPoint = l2;
                t0 = 1;
            }
            else
            {
                closestPoint = new PointF(l1.X + t0 * M.X, l1.Y + t0 * M.Y);
            }
            closestLinePoint = closestPoint;
            pointRatio = t0;
            return Math.Sqrt(Math.Pow(point.X - closestPoint.X, 2) + Math.Pow(point.Y - closestPoint.Y, 2));
        }

        public static Color GetIntensityGradeColor(double intensity)
        {
            if (intensity < -1.0)
            {
                return Color.Blue;
            }
            else if (intensity > 1.0)
            {
                return Color.White;
            }
            else if (intensity >= -1.0 && intensity <= 1.0)
            {
                int hue = (int)Scale(intensity, -1.0, 1.0, 240, 0);
                int saturation = 255;
                int value = (int)(Math.Abs(intensity) * 255);
                return ColorFromHSV(hue, saturation, value);
            }
            else
            {
                return Color.Black;
            }
        }

        private static Color ColorFromHSV(int hue, int saturation, int value)
        {
            double h = hue / 360.0;
            double s = saturation / 100.0;
            double v = value / 100.0;

            int hi = (int)(h * 6);
            double f = h * 6 - hi;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            v *= 255;
            p *= 255;
            q *= 255;
            t *= 255;

            if (hi == 0)
                return Color.FromArgb(255, (int)v, (int)t, (int)p);
            else if (hi == 1)
                return Color.FromArgb(255, (int)q, (int)v, (int)p);
            else if (hi == 2)
                return Color.FromArgb(255, (int)p, (int)v, (int)t);
            else if (hi == 3)
                return Color.FromArgb(255, (int)p, (int)q, (int)v);
            else if (hi == 4)
                return Color.FromArgb(255, (int)t, (int)p, (int)v);
            else
                return Color.FromArgb(255, (int)v, (int)p, (int)q);
        }
    }
}
