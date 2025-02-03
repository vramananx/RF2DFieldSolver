using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.Scenarios
{
    public class CoplanarDifferentialStripline : Scenario
    {
        private double width;
        private double height;
        private double gapTrace;
        private double gapCoplanar;
        private double substrateHeightAbove;
        private double epsilonRAbove;
        private double substrateHeightBelow;
        private double epsilonRBelow;

        public CoplanarDifferentialStripline()
        {
            name = "Coplanar Differential Stripline";
            width = 0.2e-3;
            height = 35e-6;
            gapTrace = 0.2e-3;
            gapCoplanar = 0.3e-3;
            substrateHeightAbove = 0.2e-3;
            epsilonRAbove = 4.1;
            substrateHeightBelow = 0.2e-3;
            epsilonRBelow = 4.1;

            parameters.Add(new Parameter("Trace Width (w)", "m", "um ", 4, ref width));
            parameters.Add(new Parameter("Trace Height (t)", "m", "um ", 4, ref height));
            parameters.Add(new Parameter("Gap Width (s1)", "m", "um ", 4, ref gapTrace));
            parameters.Add(new Parameter("Gap Width (s2)", "m", "um ", 4, ref gapCoplanar));
            parameters.Add(new Parameter("Substrate Height (h1)", "m", "um ", 4, ref substrateHeightAbove));
            parameters.Add(new Parameter("Substrate Height (h2)", "m", "um ", 4, ref substrateHeightBelow));
            parameters.Add(new Parameter("Substrate dielectric constant (h1)", "", " ", 3, ref epsilonRAbove));
            parameters.Add(new Parameter("Substrate dielectric constant (h2)", "", " ", 3, ref epsilonRBelow));
        }

        protected override ElementList CreateScenario()
        {
            var list = new ElementList();

            var thickerSubstrate = Math.Max(substrateHeightAbove, substrateHeightBelow);

            if (ui.AutoArea.Checked)
            {
                ui.XLeft.Value = -Math.Max(thickerSubstrate * 5, Math.Max((width + gapTrace / 2) * 3, gapCoplanar * 4));
                ui.XRight.Value = Math.Max(thickerSubstrate * 5, Math.Max((width + gapTrace / 2) * 3, gapCoplanar * 4));
                ui.YTop.Value = substrateHeightAbove + 0.1e-3;
                ui.YBottom.Value = -substrateHeightBelow - 0.1e-3;
            }

            var trace = new Element(Element.Type.TracePos);
            trace.AppendVertex(new PointF((float)(-width - gapTrace / 2), 0));
            trace.AppendVertex(new PointF((float)(-gapTrace / 2), 0));
            trace.AppendVertex(new PointF((float)(-gapTrace / 2), (float)height));
            trace.AppendVertex(new PointF((float)(-width - gapTrace / 2), (float)height));
            list.AddElement(trace);

            var trace2 = new Element(Element.Type.TraceNeg);
            trace2.AppendVertex(new PointF((float)(gapTrace / 2), 0));
            trace2.AppendVertex(new PointF((float)(gapTrace / 2 + width), 0));
            trace2.AppendVertex(new PointF((float)(gapTrace / 2 + width), (float)height));
            trace2.AppendVertex(new PointF((float)(gapTrace / 2), (float)height));
            list.AddElement(trace2);

            var gnd1 = new Element(Element.Type.GND);
            gnd1.AppendVertex(new PointF((float)ui.XLeft.Value, 0));
            gnd1.AppendVertex(new PointF((float)(-gapTrace / 2 - width - gapCoplanar), 0));
            gnd1.AppendVertex(new PointF((float)(-gapTrace / 2 - width - gapCoplanar), (float)height));
            gnd1.AppendVertex(new PointF((float)ui.XLeft.Value, (float)height));
            list.AddElement(gnd1);

            var gnd2 = new Element(Element.Type.GND);
            gnd2.AppendVertex(new PointF((float)(gapTrace / 2 + width + gapCoplanar), 0));
            gnd2.AppendVertex(new PointF((float)ui.XRight.Value, 0));
            gnd2.AppendVertex(new PointF((float)ui.XRight.Value, (float)height));
            gnd2.AppendVertex(new PointF((float)(gapTrace / 2 + width + gapCoplanar), (float)height));
            list.AddElement(gnd2);

            var substrateAbove = new Element(Element.Type.Dielectric);
            substrateAbove.Name = "Substrate above";
            substrateAbove.EpsilonR = epsilonRAbove;
            substrateAbove.AppendVertex(new PointF((float)ui.XLeft.Value, 0));
            substrateAbove.AppendVertex(new PointF((float)ui.XRight.Value, 0));
            substrateAbove.AppendVertex(new PointF((float)ui.XRight.Value, (float)substrateHeightAbove));
            substrateAbove.AppendVertex(new PointF((float)ui.XLeft.Value, (float)substrateHeightAbove));
            list.AddElement(substrateAbove);

            var substrateBelow = new Element(Element.Type.Dielectric);
            substrateBelow.Name = "Substrate below";
            substrateBelow.EpsilonR = epsilonRBelow;
            substrateBelow.AppendVertex(new PointF((float)ui.XLeft.Value, 0));
            substrateBelow.AppendVertex(new PointF((float)ui.XRight.Value, 0));
            substrateBelow.AppendVertex(new PointF((float)ui.XRight.Value, (float)-substrateHeightBelow));
            substrateBelow.AppendVertex(new PointF((float)ui.XLeft.Value, (float)-substrateHeightBelow));
            list.AddElement(substrateBelow);

            var gndTop = new Element(Element.Type.GND);
            gndTop.Name = "Top reference";
            gndTop.AppendVertex(new PointF((float)ui.XLeft.Value, (float)substrateHeightAbove));
            gndTop.AppendVertex(new PointF((float)ui.XRight.Value, (float)substrateHeightAbove));
            gndTop.AppendVertex(new PointF((float)ui.XRight.Value, (float)(substrateHeightAbove + 0.1e-3)));
            gndTop.AppendVertex(new PointF((float)ui.XLeft.Value, (float)(substrateHeightAbove + 0.1e-3)));
            list.AddElement(gndTop);

            var gndBottom = new Element(Element.Type.GND);
            gndBottom.Name = "Bottom reference";
            gndBottom.AppendVertex(new PointF((float)ui.XLeft.Value, (float)-substrateHeightBelow));
            gndBottom.AppendVertex(new PointF((float)ui.XRight.Value, (float)-substrateHeightBelow));
            gndBottom.AppendVertex(new PointF((float)ui.XRight.Value, (float)(-substrateHeightBelow - 0.1e-3)));
            gndBottom.AppendVertex(new PointF((float)ui.XLeft.Value, (float)(-substrateHeightBelow - 0.1e-3)));
            list.AddElement(gndBottom);

            return list;
        }

        protected override Image GetImage()
        {
            return Image.FromFile(":/images/coplanar_stripline_differential.png");
        }
    }
}
