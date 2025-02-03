using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.Scenarios
{
    public class Stripline : Scenario
    {
        private double width;
        private double height;
        private double substrateHeightAbove;
        private double epsilonRAbove;
        private double substrateHeightBelow;
        private double epsilonRBelow;

        public Stripline()
        {
            name = "Stripline";
            width = 0.2e-3;
            height = 35e-6;
            substrateHeightAbove = 0.2e-3;
            epsilonRAbove = 4.1;
            substrateHeightBelow = 0.2e-3;
            epsilonRBelow = 4.1;

            parameters.Add(new Parameter { Name = "Trace Width (w)", Unit = "m", Prefixes = "um ", Precision = 4, Value = width });
            parameters.Add(new Parameter { Name = "Trace Height (t)", Unit = "m", Prefixes = "um ", Precision = 4, Value = height });
            parameters.Add(new Parameter { Name = "Substrate Height (h1)", Unit = "m", Prefixes = "um ", Precision = 4, Value = substrateHeightAbove });
            parameters.Add(new Parameter { Name = "Substrate Height (h2)", Unit = "m", Prefixes = "um ", Precision = 4, Value = substrateHeightBelow });
            parameters.Add(new Parameter { Name = "Substrate dielectric constant (h1)", Unit = "", Prefixes = " ", Precision = 3, Value = epsilonRAbove });
            parameters.Add(new Parameter { Name = "Substrate dielectric constant (h2)", Unit = "", Prefixes = " ", Precision = 3, Value = epsilonRBelow });
        }

        protected override ElementList CreateScenario()
        {
            var list = new ElementList();

            var thickerSubstrate = Math.Max(substrateHeightAbove, substrateHeightBelow);

            if (ui.Controls["Auto Area"].Text == "Checked")
            {
                ui.Controls["X Left"].Text = (-Math.Max(thickerSubstrate * 5, width)).ToString();
                ui.Controls["X Right"].Text = (Math.Max(thickerSubstrate * 5, width)).ToString();
                ui.Controls["Y Top"].Text = (substrateHeightAbove + 0.1e-3).ToString();
                ui.Controls["Y Bottom"].Text = (-substrateHeightBelow - 0.1e-3).ToString();
            }

            var trace = new Element(Element.Type.TracePos);
            trace.AppendVertex(new PointF((float)(-width / 2), 0));
            trace.AppendVertex(new PointF((float)(width / 2), 0));
            trace.AppendVertex(new PointF((float)(width / 2), (float)height));
            trace.AppendVertex(new PointF((float)(-width / 2), (float)height));
            list.AddElement(trace);

            var substrate = new Element(Element.Type.Dielectric);
            substrate.Name = "Substrate above";
            substrate.EpsilonR = epsilonRAbove;
            substrate.AppendVertex(new PointF(float.Parse(ui.Controls["X Left"].Text), 0));
            substrate.AppendVertex(new PointF(float.Parse(ui.Controls["X Right"].Text), 0));
            substrate.AppendVertex(new PointF(float.Parse(ui.Controls["X Right"].Text), (float)substrateHeightAbove));
            substrate.AppendVertex(new PointF(float.Parse(ui.Controls["X Left"].Text), (float)substrateHeightAbove));
            list.AddElement(substrate);

            substrate = new Element(Element.Type.Dielectric);
            substrate.Name = "Substrate below";
            substrate.EpsilonR = epsilonRBelow;
            substrate.AppendVertex(new PointF(float.Parse(ui.Controls["X Left"].Text), 0));
            substrate.AppendVertex(new PointF(float.Parse(ui.Controls["X Right"].Text), 0));
            substrate.AppendVertex(new PointF(float.Parse(ui.Controls["X Right"].Text), (float)-substrateHeightBelow));
            substrate.AppendVertex(new PointF(float.Parse(ui.Controls["X Left"].Text), (float)-substrateHeightBelow));
            list.AddElement(substrate);

            var gnd = new Element(Element.Type.GND);
            gnd.Name = "Top reference";
            gnd.AppendVertex(new PointF(float.Parse(ui.Controls["X Left"].Text), (float)substrateHeightAbove));
            gnd.AppendVertex(new PointF(float.Parse(ui.Controls["X Right"].Text), (float)substrateHeightAbove));
            gnd.AppendVertex(new PointF(float.Parse(ui.Controls["X Right"].Text), (float)(substrateHeightAbove + 0.1e-3)));
            gnd.AppendVertex(new PointF(float.Parse(ui.Controls["X Left"].Text), (float)(substrateHeightAbove + 0.1e-3)));
            list.AddElement(gnd);

            gnd = new Element(Element.Type.GND);
            gnd.Name = "Bottom reference";
            gnd.AppendVertex(new PointF(float.Parse(ui.Controls["X Left"].Text), (float)-substrateHeightBelow));
            gnd.AppendVertex(new PointF(float.Parse(ui.Controls["X Right"].Text), (float)-substrateHeightBelow));
            gnd.AppendVertex(new PointF(float.Parse(ui.Controls["X Right"].Text), (float)(-substrateHeightBelow - 0.1e-3)));
            gnd.AppendVertex(new PointF(float.Parse(ui.Controls["X Left"].Text), (float)(-substrateHeightBelow - 0.1e-3)));
            list.AddElement(gnd);

            return list;
        }

        protected override Image GetImage()
        {
            return Image.FromFile(":/images/stripline.png");
        }
    }
}
