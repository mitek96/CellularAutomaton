using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automat1D
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    class Cell
    {
        internal Color color { get; set; }
        public Point Coord;
        public Point Weight;
        public bool set;
        public int Energy;
        public bool randHit;

        public Cell(Color state, Point coord)
        {
            color = state;
            Coord = coord;
        }

        public Cell(int x, int y)
        {
            color = SystemColors.Control;
            set = false;
            randHit = false;
            Coord = new Point();
            Coord.X = x;
            Coord.Y = y;
        }
    }
}
