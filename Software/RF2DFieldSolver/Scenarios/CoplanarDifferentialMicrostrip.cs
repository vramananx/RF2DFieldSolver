using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RF2DFieldSolver.Scenarios
{
    public class CoplanarDifferentialMicrostrip : Scenario
    {
        private double width;
        private double height;
        private double gapTrace;
        private double gapCoplanar;
        private double substrateHeight;
        private double epsilonR;

        public CoplanarDifferentialMicrostrip()
        {
            Name = "Coplanar Differential Microstrip";
            width = 0.3e-3;
            height = 35e-6;
            gapTrace = 0.2e-3;
            gapCoplanar = 0.3e-3;
            substrateHeight = 0.2e-3;
            epsilonR = 4.1;

            Parameters = new List<Parameter>
            {
                new Parameter { Name = "Trace Width (w)", Unit = "m", Prefixes = "um ", Precision = 4, Value = width },
                new Parameter { Name = "Trace Height (t)", Unit = "m", Prefixes = "um ", Precision = 4, Value = height },
                new Parameter { Name = "Gap Width (s1)", Unit = "m", Prefixes = "um ", Precision = 4, Value = gapTrace },
                new Parameter { Name = "Gap Width (s2)", Unit = "m", Prefixes = "um ", Precision = 4, Value = gapCoplanar },
                new Parameter { Name = "Substrate Height (h)", Unit = "m", Prefixes = "um ", Precision = 4, Value = substrateHeight },
                new Parameter { Name = "Substrate dielectric constant", Unit = "", Prefixes = " ", Precision = 3, Value = epsilonR }
            };
        }

        protected override ElementList CreateScenario()
        {
            var list = new ElementList();

            if (AutoArea)
            {
                XLeft = -Math.Max(Math.Max(substrateHeight * 5, (width + gapTrace / 2) * 3), gapCoplanar * 4);
                XRight = Math.Max(Math.Max(substrateHeight * 5, (width + gapTrace / 2) * 3), gapCoplanar * 4);
                YTop = substrateHeight * 5;
                YBottom = -substrateHeight - 0.1e-3;
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
            gnd1.AppendVertex(new PointF((float)XLeft, 0));
            gnd1.AppendVertex(new PointF((float)(-gapTrace / 2 - width - gapCoplanar), 0));
            gnd1.AppendVertex(new PointF((float)(-gapTrace / 2 - width - gapCoplanar), (float)height));
            gnd1.AppendVertex(new PointF((float)XLeft, (float)height));
            list.AddElement(gnd1);

            var gnd2 = new Element(Element.Type.GND);
            gnd2.AppendVertex(new PointF((float)(gapTrace / 2 + width + gapCoplanar), 0));
            gnd2.AppendVertex(new PointF((float)XRight, 0));
            gnd2.AppendVertex(new PointF((float)XRight, (float)height));
            gnd2.AppendVertex(new PointF((float)(gapTrace / 2 + width + gapCoplanar), (float)height));
            list.AddElement(gnd2);

            var substrate = new Element(Element.Type.Dielectric);
            substrate.EpsilonR = epsilonR;
            substrate.AppendVertex(new PointF((float)XLeft, 0));
            substrate.AppendVertex(new PointF((float)XRight, 0));
            substrate.AppendVertex(new PointF((float)XRight, (float)-substrateHeight));
            substrate.AppendVertex(new PointF((float)XLeft, (float)-substrateHeight));
            list.AddElement(substrate);

            var gnd = new Element(Element.Type.GND);
            gnd.AppendVertex(new PointF((float)XLeft, (float)-substrateHeight));
            gnd.AppendVertex(new PointF((float)XRight, (float)-substrateHeight));
            gnd.AppendVertex(new PointF((float)XRight, (float)(-substrateHeight - 0.1e-3)));
            gnd.AppendVertex(new PointF((float)XLeft, (float)(-substrateHeight - 0.1e-3)));
            list.AddElement(gnd);

            return list;
        }

        protected override Image GetImage()
        {
            return Image.FromFile("images/coplanar_microstrip_differential.png");
        }
    }
}
