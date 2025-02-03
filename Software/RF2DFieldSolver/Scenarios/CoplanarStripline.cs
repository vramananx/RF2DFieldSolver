using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.Scenarios
{
    public class CoplanarStripline : Scenario
    {
        private double width;
        private double height;
        private double gap;
        private double substrateHeightAbove;
        private double epsilonRAbove;
        private double substrateHeightBelow;
        private double epsilonRBelow;

        public CoplanarStripline()
        {
            name = "Coplanar Stripline";
            width = 0.2e-3;
            height = 35e-6;
            gap = 0.3e-3;
            substrateHeightAbove = 0.2e-3;
            epsilonRAbove = 4.1;
            substrateHeightBelow = 0.2e-3;
            epsilonRBelow = 4.1;

            parameters = new List<Parameter>
            {
                new Parameter { Name = "Trace Width (w)", Unit = "m", Prefixes = "um ", Precision = 4, Value = width },
                new Parameter { Name = "Trace Height (t)", Unit = "m", Prefixes = "um ", Precision = 4, Value = height },
                new Parameter { Name = "Gap Width (s)", Unit = "m", Prefixes = "um ", Precision = 4, Value = gap },
                new Parameter { Name = "Substrate Height (h1)", Unit = "m", Prefixes = "um ", Precision = 4, Value = substrateHeightAbove },
                new Parameter { Name = "Substrate Height (h2)", Unit = "m", Prefixes = "um ", Precision = 4, Value = substrateHeightBelow },
                new Parameter { Name = "Substrate dielectric constant (h1)", Unit = "", Prefixes = " ", Precision = 3, Value = epsilonRAbove },
                new Parameter { Name = "Substrate dielectric constant (h2)", Unit = "", Prefixes = " ", Precision = 3, Value = epsilonRBelow }
            };
        }

        protected override ElementList CreateScenario()
        {
            var list = new ElementList();

            var thickerSubstrate = Math.Max(substrateHeightAbove, substrateHeightBelow);

            if (autoArea)
            {
                xLeft = -Math.Max(thickerSubstrate * 5, Math.Max(width, gap));
                xRight = Math.Max(thickerSubstrate * 5, Math.Max(width, gap));
                yTop = substrateHeightAbove + 0.1e-3;
                yBottom = -substrateHeightBelow - 0.1e-3;
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

            var substrateAbove = new Element(Element.Type.Dielectric);
            substrateAbove.Name = "Substrate above";
            substrateAbove.EpsilonR = epsilonRAbove;
            substrateAbove.AppendVertex(new PointF((float)xLeft, 0));
            substrateAbove.AppendVertex(new PointF((float)xRight, 0));
            substrateAbove.AppendVertex(new PointF((float)xRight, (float)substrateHeightAbove));
            substrateAbove.AppendVertex(new PointF((float)xLeft, (float)substrateHeightAbove));
            list.AddElement(substrateAbove);

            var substrateBelow = new Element(Element.Type.Dielectric);
            substrateBelow.Name = "Substrate below";
            substrateBelow.EpsilonR = epsilonRBelow;
            substrateBelow.AppendVertex(new PointF((float)xLeft, 0));
            substrateBelow.AppendVertex(new PointF((float)xRight, 0));
            substrateBelow.AppendVertex(new PointF((float)xRight, (float)-substrateHeightBelow));
            substrateBelow.AppendVertex(new PointF((float)xLeft, (float)-substrateHeightBelow));
            list.AddElement(substrateBelow);

            var gndTop = new Element(Element.Type.GND);
            gndTop.Name = "Top reference";
            gndTop.AppendVertex(new PointF((float)xLeft, (float)substrateHeightAbove));
            gndTop.AppendVertex(new PointF((float)xRight, (float)substrateHeightAbove));
            gndTop.AppendVertex(new PointF((float)xRight, (float)(substrateHeightAbove + 0.1e-3)));
            gndTop.AppendVertex(new PointF((float)xLeft, (float)(substrateHeightAbove + 0.1e-3)));
            list.AddElement(gndTop);

            var gndBottom = new Element(Element.Type.GND);
            gndBottom.Name = "Bottom reference";
            gndBottom.AppendVertex(new PointF((float)xLeft, (float)-substrateHeightBelow));
            gndBottom.AppendVertex(new PointF((float)xRight, (float)-substrateHeightBelow));
            gndBottom.AppendVertex(new PointF((float)xRight, (float)(-substrateHeightBelow - 0.1e-3)));
            gndBottom.AppendVertex(new PointF((float)xLeft, (float)(-substrateHeightBelow - 0.1e-3)));
            list.AddElement(gndBottom);

            return list;
        }

        protected override Image GetImage()
        {
            return Image.FromFile("images/coplanar_stripline.png");
        }
    }
}
