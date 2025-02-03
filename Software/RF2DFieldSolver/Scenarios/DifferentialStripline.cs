using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.Scenarios
{
    public class DifferentialStripline : Scenario
    {
        private double width;
        private double height;
        private double gap;
        private double substrateHeightAbove;
        private double epsilonRAbove;
        private double substrateHeightBelow;
        private double epsilonRBelow;

        public DifferentialStripline()
        {
            name = "Differential Stripline";
            width = 0.2e-3;
            height = 35e-6;
            gap = 0.2e-3;
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

            if (ui.AutoArea.Checked)
            {
                ui.XLeft.Value = -Math.Max(thickerSubstrate * 5, (width + gap / 2) * 3);
                ui.XRight.Value = Math.Max(thickerSubstrate * 5, (width + gap / 2) * 3);
                ui.YTop.Value = substrateHeightAbove + 0.1e-3;
                ui.YBottom.Value = -substrateHeightBelow - 0.1e-3;
            }

            var trace = new Element(ElementType.TracePos);
            trace.AppendVertex(new PointF((float)(-width - gap / 2), 0));
            trace.AppendVertex(new PointF((float)(-gap / 2), 0));
            trace.AppendVertex(new PointF((float)(-gap / 2), (float)height));
            trace.AppendVertex(new PointF((float)(-width - gap / 2), (float)height));
            list.AddElement(trace);

            var trace2 = new Element(ElementType.TraceNeg);
            trace2.AppendVertex(new PointF((float)(gap / 2), 0));
            trace2.AppendVertex(new PointF((float)(gap / 2 + width), 0));
            trace2.AppendVertex(new PointF((float)(gap / 2 + width), (float)height));
            trace2.AppendVertex(new PointF((float)(gap / 2), (float)height));
            list.AddElement(trace2);

            var substrate = new Element(ElementType.Dielectric);
            substrate.Name = "Substrate above";
            substrate.EpsilonR = epsilonRAbove;
            substrate.AppendVertex(new PointF((float)ui.XLeft.Value, 0));
            substrate.AppendVertex(new PointF((float)ui.XRight.Value, 0));
            substrate.AppendVertex(new PointF((float)ui.XRight.Value, (float)substrateHeightAbove));
            substrate.AppendVertex(new PointF((float)ui.XLeft.Value, (float)substrateHeightAbove));
            list.AddElement(substrate);

            substrate = new Element(ElementType.Dielectric);
            substrate.Name = "Substrate below";
            substrate.EpsilonR = epsilonRBelow;
            substrate.AppendVertex(new PointF((float)ui.XLeft.Value, 0));
            substrate.AppendVertex(new PointF((float)ui.XRight.Value, 0));
            substrate.AppendVertex(new PointF((float)ui.XRight.Value, (float)-substrateHeightBelow));
            substrate.AppendVertex(new PointF((float)ui.XLeft.Value, (float)-substrateHeightBelow));
            list.AddElement(substrate);

            var gnd = new Element(ElementType.GND);
            gnd.Name = "Top reference";
            gnd.AppendVertex(new PointF((float)ui.XLeft.Value, (float)substrateHeightAbove));
            gnd.AppendVertex(new PointF((float)ui.XRight.Value, (float)substrateHeightAbove));
            gnd.AppendVertex(new PointF((float)ui.XRight.Value, (float)(substrateHeightAbove + 0.1e-3)));
            gnd.AppendVertex(new PointF((float)ui.XLeft.Value, (float)(substrateHeightAbove + 0.1e-3)));
            list.AddElement(gnd);

            gnd = new Element(ElementType.GND);
            gnd.Name = "Bottom reference";
            gnd.AppendVertex(new PointF((float)ui.XLeft.Value, (float)-substrateHeightBelow));
            gnd.AppendVertex(new PointF((float)ui.XRight.Value, (float)-substrateHeightBelow));
            gnd.AppendVertex(new PointF((float)ui.XRight.Value, (float)(-substrateHeightBelow - 0.1e-3)));
            gnd.AppendVertex(new PointF((float)ui.XLeft.Value, (float)(-substrateHeightBelow - 0.1e-3)));
            list.AddElement(gnd);

            return list;
        }

        protected override Image GetImage()
        {
            return Image.FromFile(":/images/stripline_differential.png");
        }
    }
}
