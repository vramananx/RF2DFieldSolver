using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.Scenarios
{
    public class DifferentialMicrostrip : Scenario
    {
        private double width;
        private double height;
        private double gap;
        private double substrateHeight;
        private double epsilonR;

        public DifferentialMicrostrip()
        {
            name = "Differential Microstrip";
            width = 0.3e-3;
            height = 35e-6;
            gap = 0.2e-3;
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

            if (ui.AutoArea.Checked)
            {
                ui.XLeft.Value = -Math.Max(substrateHeight * 5, width + gap / 2);
                ui.XRight.Value = Math.Max(substrateHeight * 5, width + gap / 2);
                ui.YTop.Value = substrateHeight * 5;
                ui.YBottom.Value = -substrateHeight - 0.1e-3;
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
            substrate.EpsilonR = epsilonR;
            substrate.AppendVertex(new PointF((float)ui.XLeft.Value, 0));
            substrate.AppendVertex(new PointF((float)ui.XRight.Value, 0));
            substrate.AppendVertex(new PointF((float)ui.XRight.Value, (float)-substrateHeight));
            substrate.AppendVertex(new PointF((float)ui.XLeft.Value, (float)-substrateHeight));
            list.AddElement(substrate);

            var gnd = new Element(ElementType.GND);
            gnd.AppendVertex(new PointF((float)ui.XLeft.Value, (float)-substrateHeight));
            gnd.AppendVertex(new PointF((float)ui.XRight.Value, (float)-substrateHeight));
            gnd.AppendVertex(new PointF((float)ui.XRight.Value, (float)(-substrateHeight - 0.1e-3)));
            gnd.AppendVertex(new PointF((float)ui.XLeft.Value, (float)(-substrateHeight - 0.1e-3)));
            list.AddElement(gnd);

            return list;
        }

        protected override Image GetImage()
        {
            return Image.FromFile(":/images/microstrip_differential.png");
        }
    }
}
