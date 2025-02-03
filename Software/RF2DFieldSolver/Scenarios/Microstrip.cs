using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.Scenarios
{
    public class Microstrip : Scenario
    {
        private double width;
        private double height;
        private double substrateHeight;
        private double e_r;

        public Microstrip()
        {
            name = "Microstrip";
            width = 0.3e-3;
            height = 35e-6;
            substrateHeight = 0.2e-3;
            e_r = 4.1;

            parameters = new List<Parameter>
            {
                new Parameter { Name = "Trace Width (w)", Unit = "m", Prefixes = "um ", Precision = 4, Value = width },
                new Parameter { Name = "Trace Height (t)", Unit = "m", Prefixes = "um ", Precision = 4, Value = height },
                new Parameter { Name = "Substrate Height (h)", Unit = "m", Prefixes = "um ", Precision = 4, Value = substrateHeight },
                new Parameter { Name = "Substrate dielectric constant", Unit = "", Prefixes = " ", Precision = 3, Value = e_r }
            };
        }

        protected override ElementList CreateScenario()
        {
            var list = new ElementList();

            if (ui.AutoArea.Checked)
            {
                ui.XLeft.Value = -Math.Max(substrateHeight * 5, width);
                ui.XRight.Value = Math.Max(substrateHeight * 5, width);
                ui.YTop.Value = substrateHeight * 5;
                ui.YBottom.Value = -substrateHeight - 0.1e-3;
            }

            var trace = new Element(Element.Type.TracePos);
            trace.AppendVertex(new PointF((float)-width / 2, 0));
            trace.AppendVertex(new PointF((float)width / 2, 0));
            trace.AppendVertex(new PointF((float)width / 2, (float)height));
            trace.AppendVertex(new PointF((float)-width / 2, (float)height));
            list.AddElement(trace);

            var substrate = new Element(Element.Type.Dielectric);
            substrate.EpsilonR = e_r;
            substrate.AppendVertex(new PointF((float)ui.XLeft.Value, 0));
            substrate.AppendVertex(new PointF((float)ui.XRight.Value, 0));
            substrate.AppendVertex(new PointF((float)ui.XRight.Value, (float)-substrateHeight));
            substrate.AppendVertex(new PointF((float)ui.XLeft.Value, (float)-substrateHeight));
            list.AddElement(substrate);

            var gnd = new Element(Element.Type.GND);
            gnd.AppendVertex(new PointF((float)ui.XLeft.Value, (float)-substrateHeight));
            gnd.AppendVertex(new PointF((float)ui.XRight.Value, (float)-substrateHeight));
            gnd.AppendVertex(new PointF((float)ui.XRight.Value, (float)-substrateHeight - 0.1e-3));
            gnd.AppendVertex(new PointF((float)ui.XLeft.Value, (float)-substrateHeight - 0.1e-3));
            list.AddElement(gnd);

            return list;
        }

        protected override Image GetImage()
        {
            return Image.FromFile(":/images/microstrip.png");
        }
    }
}
