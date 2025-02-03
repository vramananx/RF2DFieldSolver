using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

public class Laplace
{
    private bool calculationRunning;
    private bool resultReady;
    private ElementList list;
    private double grid;
    private int threads;
    private double threshold;
    private Lattice lattice;
    private bool groundedBorders;
    private bool ignoreDielectric;
    private PointF topLeft;
    private PointF bottomRight;
    private int lastPercent;
    private Thread thread;

    public Laplace()
    {
        calculationRunning = false;
        resultReady = false;
        list = null;
        grid = 1e-5;
        threads = 1;
        threshold = 1e-6;
        lattice = null;
        groundedBorders = true;
        ignoreDielectric = false;
    }

    public void SetArea(PointF topLeft, PointF bottomRight)
    {
        if (calculationRunning)
        {
            return;
        }
        this.topLeft = topLeft;
        this.bottomRight = bottomRight;
    }

    public void SetGrid(double grid)
    {
        if (calculationRunning)
        {
            return;
        }
        if (grid > 0)
        {
            this.grid = grid;
        }
    }

    public void SetThreads(int threads)
    {
        if (calculationRunning)
        {
            return;
        }
        if (threads > 0)
        {
            this.threads = threads;
        }
    }

    public void SetThreshold(double threshold)
    {
        if (calculationRunning)
        {
            return;
        }
        if (threshold > 0)
        {
            this.threshold = threshold;
        }
    }

    public void SetGroundedBorders(bool gnd)
    {
        if (calculationRunning)
        {
            return;
        }
        groundedBorders = gnd;
    }

    public void SetIgnoreDielectric(bool ignore)
    {
        if (calculationRunning)
        {
            return;
        }
        ignoreDielectric = ignore;
    }

    public bool StartCalculation(ElementList list)
    {
        if (calculationRunning)
        {
            return false;
        }
        calculationRunning = true;
        resultReady = false;
        lastPercent = 0;
        Info("Laplace calculation starting");
        if (lattice != null)
        {
            lattice = null;
        }
        this.list = list;

        // start the calculation thread
        thread = new Thread(CalcThread);
        thread.Start();
        Info("Laplace thread started");
        return true;
    }

    public void AbortCalculation()
    {
        if (!calculationRunning)
        {
            return;
        }
        // request abort of calculation
        lattice.Abort = true;
    }

    public double GetPotential(PointF p)
    {
        if (!resultReady)
        {
            return double.NaN;
        }
        var pos = CoordToRect(p);
        // convert to integers and shift by the added outside boundary of NaNs
        int index_x = (int)Math.Round(pos.X) + 1;
        int index_y = (int)Math.Round(pos.Y) + 1;
        if (index_x < 0 || index_x >= lattice.Dim.X || index_y < 0 || index_y >= lattice.Dim.Y)
        {
            return double.NaN;
        }
        var c = lattice.Cells[index_x + index_y * lattice.Dim.X];
        return c.Value;
    }

    public LineF GetGradient(PointF p)
    {
        var ret = new LineF(p, p);
        if (!resultReady)
        {
            return ret;
        }
        var pos = CoordToRect(p);
        // convert to integers and shift by the added outside boundary of NaNs
        int index_x = (int)Math.Floor(pos.X) + 1;
        int index_y = (int)Math.Floor(pos.Y) + 1;

        if (index_x < 0 || index_x + 1 >= lattice.Dim.X || index_y < 0 || index_y + 1 >= lattice.Dim.Y)
        {
            return ret;
        }
        // calculate gradient
        var c_floor = lattice.Cells[index_x + index_y * lattice.Dim.X];
        var c_x = lattice.Cells[index_x + 1 + index_y * lattice.Dim.X];
        var c_y = lattice.Cells[index_x + (index_y + 1) * lattice.Dim.X];
        var grad_x = c_x.Value - c_floor.Value;
        var grad_y = c_y.Value - c_floor.Value;
        ret.P2 = new PointF(p.X + (float)grad_x, p.Y + (float)grad_y);
        return ret;
    }

    public void InvalidateResult()
    {
        resultReady = false;
    }

    private PointF CoordFromRect(Rect pos)
    {
        return new PointF((float)(pos.X * grid + topLeft.X), (float)(pos.Y * grid + bottomRight.Y));
    }

    private Rect CoordToRect(PointF pos)
    {
        return new Rect((pos.X - topLeft.X) / grid, (pos.Y - bottomRight.Y) / grid);
    }

    private Bound Boundary(Bound bound, Rect pos)
    {
        var coord = CoordFromRect(pos);
        bound.Value = 0;
        bound.Cond = Condition.None;

        bool isBorder = Math.Abs(coord.X - topLeft.X) < 1e-6 || Math.Abs(coord.X - bottomRight.X) < 1e-6 || Math.Abs(coord.Y - topLeft.Y) < 1e-6 || Math.Abs(coord.Y - bottomRight.Y) < 1e-6;

        // handle borders
        if (groundedBorders && isBorder)
        {
            bound.Value = 0;
            bound.Cond = Condition.Dirichlet;
            return bound;
        }
        else
        {
            // find the matching polygon
            foreach (var e in list.Elements)
            {
                if (e.Type == ElementType.Dielectric)
                {
                    // skip, dielectric has no influence on boundary and trace/GND should always take priority
                    continue;
                }
                var poly = e.ToPolygon();
                if (poly.Contains(coord))
                {
                    // this polygon defines the boundary at these coordinates
                    switch (e.Type)
                    {
                        case ElementType.GND:
                            bound.Value = 0;
                            bound.Cond = Condition.Dirichlet;
                            return bound;
                        case ElementType.TracePos:
                            bound.Value = 1.0;
                            bound.Cond = Condition.Dirichlet;
                            return bound;
                        case ElementType.TraceNeg:
                            bound.Value = -1.0;
                            bound.Cond = Condition.Dirichlet;
                            return bound;
                        case ElementType.Dielectric:
                        case ElementType.Last:
                            return bound;
                    }
                }
            }
        }
        return bound;
    }

    private double Weight(Rect pos)
    {
        if (ignoreDielectric)
        {
            return 1.0;
        }

        var coord = CoordFromRect(pos);
        return Math.Sqrt(list.GetDielectricConstantAt(coord));
    }

    private void CalcThread()
    {
        Info("Creating lattice");
        var size = new Rect((bottomRight.X - topLeft.X) / grid, (topLeft.Y - bottomRight.Y) / grid);
        var dim = new Point((int)((bottomRight.X - topLeft.X) / grid), (int)((topLeft.Y - bottomRight.Y) / grid));
        lattice = new Lattice(size, dim, Boundary, Weight, this);
        if (lattice != null)
        {
            Info("Lattice creation complete");
        }
        else
        {
            Error("Lattice creation failed");
            return;
        }

        var conf = new Config((byte)threads, 10, threshold);
        if (conf.Threads > lattice.Dim.Y / 5)
        {
            conf.Threads = lattice.Dim.Y / 5;
        }
        conf.Distance = lattice.Dim.Y / threads;
        Info("Starting calculation threads");
        var it = lattice.ComputeThreaded(conf, CalcProgressFromDiff);
        calculationRunning = false;
        if (lattice.Abort)
        {
            Warning("Laplace calculation aborted");
            resultReady = false;
            Percentage(0);
            CalculationAborted();
        }
        else
        {
            Info("Laplace calculation complete, took " + it + " iterations");
            resultReady = true;
            Percentage(100);
            CalculationDone();
        }
    }

    private void CalcProgressFromDiff(double diff)
    {
        // diff is expected to go down from 1.0 to the threshold with exponential decay
        double endTime = Math.Pow(-Math.Log(threshold), 6);
        double currentTime = Math.Pow(-Math.Log(diff), 6);
        double percent = currentTime * 100 / endTime;
        if (percent > 100)
        {
            percent = 100;
        }
        else if (percent < lastPercent)
        {
            percent = lastPercent;
        }
        lastPercent = percent;
        Percentage(percent);
    }

    public event Action<string> Info;
    public event Action<string> Warning;
    public event Action<string> Error;
    public event Action<int> Percentage;
    public event Action CalculationDone;
    public event Action CalculationAborted;
}
