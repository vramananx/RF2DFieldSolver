using System;
using System.Drawing;

namespace RF2DFieldSolver
{
    public static class Util
    {
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
                int hue = (int)(240 * (1 - (intensity + 1) / 2));
                int saturation = 255;
                int brightness = (int)(Math.Abs(intensity) * 255);
                return ColorFromHSV(hue, saturation, brightness);
            }
            else
            {
                return Color.Black;
            }
        }

        private static Color ColorFromHSV(int hue, int saturation, int brightness)
        {
            double h = hue / 360.0;
            double s = saturation / 255.0;
            double v = brightness / 255.0;

            int i = (int)Math.Floor(h * 6);
            double f = h * 6 - i;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0: return Color.FromArgb((int)(v * 255), (int)(t * 255), (int)(p * 255));
                case 1: return Color.FromArgb((int)(q * 255), (int)(v * 255), (int)(p * 255));
                case 2: return Color.FromArgb((int)(p * 255), (int)(v * 255), (int)(t * 255));
                case 3: return Color.FromArgb((int)(p * 255), (int)(q * 255), (int)(v * 255));
                case 4: return Color.FromArgb((int)(t * 255), (int)(p * 255), (int)(v * 255));
                case 5: return Color.FromArgb((int)(v * 255), (int)(p * 255), (int)(q * 255));
                default: return Color.Black;
            }
        }

        public static T Scale<T>(T value, T fromLow, T fromHigh, T toLow, T toHigh, bool logFrom = false, bool logTo = false) where T : IConvertible
        {
            double normalized;
            double val = value.ToDouble(null);
            double fromL = fromLow.ToDouble(null);
            double fromH = fromHigh.ToDouble(null);
            double toL = toLow.ToDouble(null);
            double toH = toHigh.ToDouble(null);

            if (logFrom)
            {
                normalized = Math.Log10(val / fromL) / Math.Log10(fromH / fromL);
            }
            else
            {
                normalized = (val - fromL) / (fromH - fromL);
            }

            if (logTo)
            {
                val = toL * Math.Pow(10.0, normalized * Math.Log10(toH / toL));
            }
            else
            {
                val = normalized * (toH - toL) + toL;
            }

            return (T)Convert.ChangeType(val, typeof(T));
        }
    }
}
