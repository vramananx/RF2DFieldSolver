using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.Scenarios
{
    public class CoplanarMicrostrip : Scenario
    {
        private double width;
        private double gap;
        private double height;
        private double substrateHeight;
        private double epsilonR;

        public CoplanarMicrostrip()
        {
            name = "Coplanar Microstrip";
            width = 0.3e-3;
            gap = 0.3e-3;
            height = 35e-6;
            substrateHeight = 0.2e-3;
            epsilonR = 4.1;

            parameters = new List<Parameter>
            {
                new Parameter { Name = "Trace Width (w)", Unit = "m", Prefixes = "um ", Precision = 4, Value = width },
                new Parameter { Name = "Trace Height (t)", Unit = "m", Prefixes = "um ", Precision = 4, Value = height },
                new Parameter { Name = "Gap Width (s)", Unit = "m", Prefixes = "um ", Precision = 4, Value = gap },
                new Parameter { Name = "Substrate Height (h)", Unit = "m", Prefixes = "um ", Precision = 4, Value = substrateHeight },
                new Parameter { Name = "Substrate dielectric constant", Unit = "", Prefixes = " ", Precision = 3, Value = epsilonR }
            };
        }

        protected override ElementList CreateScenario()
        {
            var list = new ElementList();

            if (autoArea)
            {
                xLeft = -Math.Max(substrateHeight * 5, Math.Max(width, gap));
                xRight = Math.Max(substrateHeight * 5, Math.Max(width, gap));
                yTop = substrateHeight * 5;
                yBottom = -substrateHeight - 0.1e-3;
            }

            var trace = new Element(Element.Type.TracePos);
            trace.AppendVertex(new PointF((float)(-width / 2), 0));
            trace.AppendVertex(new PointF((float)(width / 2), 0));
            trace.AppendVertex(new PointF((float)(width / 2), (float)height));
            trace.AppendVertex(new PointF((float)(-width / 2), (float)height));
            list.AddElement(trace);

            var gnd1 = new Element(Element.Type.GND);
            gnd1.AppendVertex(new PointF((float)xLeft, 0));
            gnd1.AppendVertex(new PointF((float)(-width / 2 - gap), 0));
            gnd1.AppendVertex(new PointF((float)(-width / 2 - gap), (float)height));
            gnd1.AppendVertex(new PointF((float)xLeft, (float)height));
            list.AddElement(gnd1);

            var gnd2 = new Element(Element.Type.GND);
            gnd2.AppendVertex(new PointF((float)(width / 2 + gap), 0));
            gnd2.AppendVertex(new PointF((float)xRight, 0));
            gnd2.AppendVertex(new PointF((float)xRight, (float)height));
            gnd2.AppendVertex(new PointF((float)(width / 2 + gap), (float)height));
            list.AddElement(gnd2);

            var substrate = new Element(Element.Type.Dielectric);
            substrate.EpsilonR = epsilonR;
            substrate.AppendVertex(new PointF((float)xLeft, 0));
            substrate.AppendVertex(new PointF((float)xRight, 0));
            substrate.AppendVertex(new PointF((float)xRight, (float)-substrateHeight));
            substrate.AppendVertex(new PointF((float)xLeft, (float)-substrateHeight));
            list.AddElement(substrate);

            var gnd = new Element(Element.Type.GND);
            gnd.AppendVertex(new PointF((float)xLeft, (float)-substrateHeight));
            gnd.AppendVertex(new PointF((float)xRight, (float)-substrateHeight));
            gnd.AppendVertex(new PointF((float)xRight, (float)(-substrateHeight - 0.1e-3)));
            gnd.AppendVertex(new PointF((float)xLeft, (float)(-substrateHeight - 0.1e-3)));
            list.AddElement(gnd);

            return list;
        }

        protected override Image GetImage()
        {
            return Image.FromFile("images/coplanar_microstrip.png");
        }
    }
}
