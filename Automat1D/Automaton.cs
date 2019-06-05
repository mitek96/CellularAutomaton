using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Automat1D
{
    class Automaton
    {

        public Cell[,] grid;
        Cell[,] arr;
        float offset;
        public int iterations;
        public float cellSize;
        public int cellsInRow;
        public DoubleBufferedPanel panel;
        Pen p;
        private Bitmap buffer;
        public Graphics g;
        private object _locker = new object();
        private object _locker2 = new object();
        Random rand;
        SolidBrush brush, brushEmpty;
        int[] countTypes;
        Cell[] neighbors;
        int[,] energyBorder;
        public Neighborhood neighborhood { get; set; }
        public bool periodic { get; set; }
        System.Timers.Timer myTimer;
        bool full;
        Label timerLabel;
        public float radius { get; set; }
        bool[,] radiusArr;
        public int numberOfCellsToRand { get; set; }



        public Automaton(DoubleBufferedPanel panel, System.Timers.Timer timer, Label timerLabel)
        {
            buffer = new Bitmap(panel.Width, panel.Height);
            this.panel = panel;
            this.panel.Paint += new PaintEventHandler(Panel_Paint);
            //countTypes = new int[8];
            //neighbors = new Cell[8];
            p = new Pen(Color.Black, 1.0f);
            brush = new SolidBrush(Color.Red);
            brushEmpty = new SolidBrush(SystemColors.Control);
            rand = new Random();
            myTimer = timer;
            this.timerLabel = timerLabel;
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(buffer, 0, 0);
        }

        public void CreateGrid(int size, int iterations)
        {
            grid = new Cell[iterations, size];
            arr = new Cell[iterations, size];
            neighbors = new Cell[iterations * size];
            countTypes = new int[iterations * size];
            radiusArr = new bool[iterations, size];
            energyBorder = new int[iterations, size];
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    grid[i, j] = new Cell(j, i);
                    arr[i, j] = new Cell(j, i);
                    radiusArr[i, j] = new bool();
                    energyBorder[i, j] = new int();
                }
            }
            cellsInRow = size;
            this.iterations = iterations;
        }

        public void SetUniformly(int uniX, int uniY)
        {
            int cX = 0, cY = 0;

            for (int i = (int)Math.Ceiling(iterations / (double)uniY) / 2; i < iterations; i = i + (int)Math.Ceiling(iterations / (double)uniY) - 1)
            {
                for (int j = (int)Math.Ceiling((cellsInRow / (double)uniX)) / 2; j < cellsInRow; j = j + (int)Math.Ceiling(cellsInRow / (double)uniX) - 1)
                {
                    SetState(i, j, true, Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)));
                    cX++;
                    if (cX >= uniX) break;
                }
                cX = 0;
                cY++;
                if (cY >= uniY) break;
            }
            panel.Invalidate();
        }

        public int RandWithRadius()
        {
            int iterY = 0, iterX = 0;
            bool hit = false;
            int countHits = 0;
            float radiusCalculated = radius * cellSize;
            if (numberOfCellsToRand > iterations * cellsInRow) numberOfCellsToRand = iterations * cellsInRow;
            for (int i = 0; i < numberOfCellsToRand; ++i)
            {
                int randCellY = rand.Next(iterations);
                int randCellX = rand.Next(cellsInRow);
                int k;
                for (int j = 0; j < iterations; ++j)
                {

                    for (k = 0; k < cellsInRow; ++k)
                    {
                        if (j + randCellY > iterations - 1) iterY = j + randCellY - iterations;
                        else iterY = j + randCellY;
                        if (k + randCellX > cellsInRow - 1) iterX = k + randCellX - cellsInRow;
                        else iterX = k + randCellX;

                        if (radiusArr[iterY, iterX] == false)
                        {
                            SetCellsWithinRadius(grid[iterY, iterX], radius);
                            SetState(iterY, iterX, true, Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)));
                            hit = true;
                            countHits++;
                            g.DrawEllipse(Pens.Green, (grid[iterY, iterX].Coord.X * cellSize) + offset + (cellSize / 2) - radiusCalculated, (grid[iterY, iterX].Coord.Y * cellSize) + (cellSize / 2) - radiusCalculated,
                                radiusCalculated + radiusCalculated, radiusCalculated + radiusCalculated);
                            break;
                        }

                    }
                    if (hit) break;
                }
                hit = false;
            }

            panel.Invalidate();
            return countHits;

        }

        private int safeIteratorX(int x, BoundaryCondition condition)
        {
            if (condition == BoundaryCondition.periodic)
            {
                if (x > cellsInRow - 1) return x % cellsInRow;
                if (x < 0)
                {
                    int tmp = -x % cellsInRow;
                    if (tmp == 0) return 0;
                    return cellsInRow - (-x % cellsInRow);
                }
                return x;
            }
            else
            {
                if (x > cellsInRow - 1) return cellsInRow - 1;
                if (x < 0) return 0;
                return x;
            }
        }

        private int safeIteratorY(int y, BoundaryCondition condition)
        {
            if (condition == BoundaryCondition.periodic)
            {
                if (y > iterations - 1) return y % iterations;
                if (y < 0)
                {
                    int tmp = -y % iterations;
                    if (tmp == 0) return 0;
                    return iterations - (-y % iterations);
                }
                return y;
            }
            else
            {
                if (y > iterations - 1) return iterations - 1;
                if (y < 0) return 0;
                return y;
            }
        }

        private void SetCellsWithinRadius(Cell cell, float radius)
        {
            int r = (int)Math.Ceiling(radius);

            double toR = radius * radius;

            for (int i = cell.Coord.Y - r; i <= cell.Coord.Y + r; ++i)
            {
                for (int j = cell.Coord.X - r; j <= cell.Coord.X + r; ++j)
                {
                    if (((j - cell.Coord.X) * (j - cell.Coord.X) + (i - cell.Coord.Y) * (i - cell.Coord.Y)) <= toR)
                    {
                        SetStateForRadiusRand(safeIteratorY(i, BoundaryCondition.periodic), safeIteratorX(j, BoundaryCondition.periodic));
                    }
                }
            }
        }

        public int RandomCells()
        {
            int iterY = 0, iterX = 0;
            bool hit = false;
            int countHits = 0;
            for (int i = 0; i < numberOfCellsToRand; ++i)
            {
                int randCellY = rand.Next(iterations);
                int randCellX = rand.Next(cellsInRow);
                int k;
                for (int j = 0; j < iterations; ++j)
                {

                    for (k = 0; k < cellsInRow; ++k)
                    {
                        if (j + randCellY > iterations - 1) iterY = j + randCellY - iterations;
                        else iterY = j + randCellY;
                        if (k + randCellX > cellsInRow - 1) iterX = k + randCellX - cellsInRow;
                        else iterX = k + randCellX;

                        if (grid[iterY, iterX].set == false)
                        {
                            SetState(iterY, iterX, true, Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)));
                            hit = true;
                            countHits++;
                            break;
                        }

                    }
                    if (hit) break;
                }
                hit = false;
            }
            return countHits;
        }

        public void DrawGrid()
        {
            buffer = new Bitmap(panel.Width, panel.Height);
            g = Graphics.FromImage(buffer);
            g.Clear(panel.BackColor);
            int columns = cellsInRow;
            int rows = iterations;

            float cellSizeWidth = ((float)panel.Width) / columns;
            float cellSizeHeight = ((float)panel.Height) / rows;
            cellSize = cellSizeWidth < cellSizeHeight ? cellSizeWidth : cellSizeHeight;
            cellSize = (float)Math.Floor(cellSize);
            offset = ((panel.Width - (columns * cellSize)) / 2.0f) - 1.0f;
            offset = (float)Math.Floor(offset);

            for (int y = 0; y <= rows; ++y)
            {
                g.DrawLine(p, 0 + offset, (y * cellSize), (columns * cellSize) + offset, (y * cellSize));
            }

            for (int x = 0; x <= columns; ++x)
            {
                g.DrawLine(p, (x * cellSize) + offset, 0, (x * cellSize) + offset, (rows * cellSize));
            }

        }

        private void FillCell(Cell cell, Color color)
        {
            lock (_locker)
            {
                brush.Color = color;
                g.FillRectangle(brush, (cell.Coord.X * cellSize) + offset + 1.0f, (cell.Coord.Y * cellSize) + 1.0f, cellSize - 1.0f, cellSize - 1.0f);
            }
        }

        public void FillWithClick(int x, int y, Color color)
        {
            int coordX = (x - (int)offset) / (int)cellSize;
            int coordY = y / (int)cellSize;
            if (coordX >= 0 && coordX < cellsInRow && coordY >= 0 && coordY < iterations)
            {
                if (grid[coordY, coordX].set) SetState(coordY, coordX, false, SystemColors.Control);
                else
                {
                    SetState(coordY, coordX, true, color);
                    //SetCellsWithinRadius(grid[coordY, coordX], radius);
                    float radiusCalculated = radius * cellSize;
                    g.DrawEllipse(Pens.Green, (grid[coordY, coordX].Coord.X * cellSize) + offset + (cellSize / 2) - radiusCalculated, (grid[coordY, coordX].Coord.Y * cellSize) + (cellSize / 2) - radiusCalculated,
                        radiusCalculated + radiusCalculated, radiusCalculated + radiusCalculated);
                }

                panel.Invalidate();
            }

        }

        private void FillCellIteration(Cell cell, int iteration)
        {
            g.FillRectangle(new SolidBrush(cell.color), (cell.Coord.X * cellSize) + offset + 1.0f, (iteration * cellSize) + 1.0f, cellSize - 1.0f, cellSize - 1.0f);
        }

        public void Run()
        {
            lock (_locker2)
            {
                full = true;
                for (int i = 0; i < iterations; ++i)
                {

                    for (int j = 0; j < cellsInRow; ++j)
                    {
                        arr[i, j].color = grid[i, j].color;
                        arr[i, j].set = grid[i, j].set;

                        if (grid[i, j].set) continue;
                        full = false;
                        arr[i, j].color = RandCellReturnColor(AutomatonRules(grid[i, j]));
                        if (arr[i, j].color == SystemColors.Control) arr[i, j].set = false;
                        else arr[i, j].set = true;
                    }
                }

                if (full)
                {
                    myTimer.Stop();
                    timerLabel.BackColor = Color.Red;
                }

                for (int i = 0; i < iterations; ++i)
                {
                    for (int j = 0; j < cellsInRow; ++j)
                    {
                        SetState(i, j, arr[i, j].set, arr[i, j].color);

                        //  g.DrawEllipse(Pens.Green, grid[i,j].Weight.X - 10, grid[i,j].Weight.Y - 10, 10 + 10, 10 + 10);
                        //g.DrawEllipse(Pens.Green, (grid[i, j].Coord.X * cellSize) + offset + (cellSize / 2) - radiusCalculated, (grid[i, j].Coord.Y * cellSize) + (cellSize / 2) - radiusCalculated,
                        //    radiusCalculated + radiusCalculated, radiusCalculated + radiusCalculated);
                    }
                }
            }
        }

        private int AutomatonRules(Cell cell)
        {
            if (neighborhood == Neighborhood.VonNeumann)
            {
                return ChooseNeighbor_VonNeumann(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else if (neighborhood == Neighborhood.Moore)
            {
                return ChooseNeighbor_Moore(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else if (neighborhood == Neighborhood.Radial)
            {
                return ChooseNeighbor_Radial(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption, radius);
            }
            else if (neighborhood == Neighborhood.PentTop)
            {
                return ChooseNeighbor_Pent_Top(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else if (neighborhood == Neighborhood.PentBottom)
            {
                return ChooseNeighbor_Pent_Bottom(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else if (neighborhood == Neighborhood.PentLeft)
            {
                return ChooseNeighbor_Pent_Left(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else if (neighborhood == Neighborhood.PentRight)
            {
                return ChooseNeighbor_Pent_Right(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else if (neighborhood == Neighborhood.HexLeft)
            {
                return ChooseNeighbor_Hex_Left(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else if (neighborhood == Neighborhood.HexRight)
            {
                return ChooseNeighbor_Hex_Right(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else if (neighborhood == Neighborhood.PentRand)
            {
                return ChooseNeighbor_Pent_Rand(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else if (neighborhood == Neighborhood.HexRand)
            {
                return ChooseNeighbor_Hex_Rand(cell, periodic ? BoundaryCondition.periodic : BoundaryCondition.sorption);
            }
            else return -1;
        }

        private Color RandCellReturnColor(int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                countTypes[i] = 0;
            }

            for (int i = 0; i < amount; ++i)
            {
                if (neighbors[i].set)
                {
                    countTypes[i]++;
                    for (int j = 0; j < i; ++j)
                    {
                        if (neighbors[i].color == neighbors[j].color)
                        {
                            countTypes[j]++;
                            countTypes[i]--;
                            break;
                        }
                    }
                }
            }

            int max = countTypes[0];
            int countMax = 0;
            int indexMax = 0;
            for (int i = 1; i < amount; ++i)
            {
                if (countTypes[i] > max)
                {
                    max = countTypes[i];
                    indexMax = i;
                }
            }

            if (max == 0) return SystemColors.Control;

            for (int i = 0; i < amount; ++i)
            {
                if (countTypes[i] == max)
                {
                    countTypes[i] = -1;
                    countMax++;
                }
            }

            if (countMax == 1) return neighbors[indexMax].color;

            countMax = rand.Next(countMax);
            int countToRand = 0;

            for (int i = 0; i < amount; ++i)
            {
                if (countTypes[i] == -1)
                {
                    if (countToRand == countMax) return neighbors[i].color;
                    countToRand++;
                }

            }

            return SystemColors.ControlDarkDark;
        }

        private int CountAliveNeighbors(Cell cell)
        {
            int alive = 0;

            if (grid[(cell.Coord.Y - 1) < 0 ? iterations - 1 : cell.Coord.Y - 1, (cell.Coord.X - 1) < 0 ? cellsInRow - 1 : cell.Coord.X - 1].set) alive++;  //top left
            if (grid[(cell.Coord.Y - 1) < 0 ? iterations - 1 : cell.Coord.Y - 1, cell.Coord.X].set) alive++;    //top
            if (grid[(cell.Coord.Y - 1) < 0 ? iterations - 1 : cell.Coord.Y - 1, (cell.Coord.X + 1) > cellsInRow - 1 ? 0 : cell.Coord.X + 1].set) alive++;  //top right
            if (grid[cell.Coord.Y, (cell.Coord.X - 1) < 0 ? cellsInRow - 1 : cell.Coord.X - 1].set) alive++;    //center left
            if (grid[cell.Coord.Y, (cell.Coord.X + 1) > cellsInRow - 1 ? 0 : cell.Coord.X + 1].set) alive++;    //center right
            if (grid[(cell.Coord.Y + 1) > iterations - 1 ? 0 : cell.Coord.Y + 1, (cell.Coord.X - 1) < 0 ? cellsInRow - 1 : cell.Coord.X - 1].set) alive++;  //down left
            if (grid[(cell.Coord.Y + 1) > iterations - 1 ? 0 : cell.Coord.Y + 1, cell.Coord.X].set) alive++;    //down
            if (grid[(cell.Coord.Y + 1) > iterations - 1 ? 0 : cell.Coord.Y + 1, (cell.Coord.X + 1) > cellsInRow - 1 ? 0 : cell.Coord.X + 1].set) alive++;  //down right

            return alive;
        }

        private int ChooseNeighbor_VonNeumann(Cell cell, BoundaryCondition condition)
        {
            int count = 4;

            neighbors[0] = NeighborCell(cell, NeighborDirection.top, condition);
            neighbors[1] = NeighborCell(cell, NeighborDirection.left, condition);
            neighbors[2] = NeighborCell(cell, NeighborDirection.right, condition);
            neighbors[3] = NeighborCell(cell, NeighborDirection.bottom, condition);

            return count;
        }

        private int ChooseNeighbor_Moore(Cell cell, BoundaryCondition condition)
        {
            int count = 8;

            neighbors[0] = NeighborCell(cell, NeighborDirection.top, condition);
            neighbors[1] = NeighborCell(cell, NeighborDirection.left, condition);
            neighbors[2] = NeighborCell(cell, NeighborDirection.right, condition);
            neighbors[3] = NeighborCell(cell, NeighborDirection.bottom, condition);
            neighbors[4] = NeighborCell(cell, NeighborDirection.topLeft, condition);
            neighbors[5] = NeighborCell(cell, NeighborDirection.topRight, condition);
            neighbors[6] = NeighborCell(cell, NeighborDirection.bottomRight, condition);
            neighbors[7] = NeighborCell(cell, NeighborDirection.bottomLeft, condition);

            return count;
        }

        private int ChooseNeighbor_Hex_Left(Cell cell, BoundaryCondition condition)
        {
            int count = 6;

            neighbors[0] = NeighborCell(cell, NeighborDirection.top, condition);
            neighbors[1] = NeighborCell(cell, NeighborDirection.left, condition);
            neighbors[2] = NeighborCell(cell, NeighborDirection.right, condition);
            neighbors[3] = NeighborCell(cell, NeighborDirection.bottom, condition);
            neighbors[4] = NeighborCell(cell, NeighborDirection.bottomLeft, condition);
            neighbors[5] = NeighborCell(cell, NeighborDirection.topRight, condition);

            return count;
        }

        private int ChooseNeighbor_Hex_Right(Cell cell, BoundaryCondition condition)
        {
            int count = 6;

            neighbors[0] = NeighborCell(cell, NeighborDirection.top, condition);
            neighbors[1] = NeighborCell(cell, NeighborDirection.left, condition);
            neighbors[2] = NeighborCell(cell, NeighborDirection.right, condition);
            neighbors[3] = NeighborCell(cell, NeighborDirection.bottom, condition);
            neighbors[4] = NeighborCell(cell, NeighborDirection.bottomRight, condition);
            neighbors[5] = NeighborCell(cell, NeighborDirection.topLeft, condition);

            return count;
        }

        private int ChooseNeighbor_Pent_Left(Cell cell, BoundaryCondition condition)
        {
            int count = 5;

            neighbors[0] = NeighborCell(cell, NeighborDirection.top, condition);
            neighbors[1] = NeighborCell(cell, NeighborDirection.left, condition);
            neighbors[2] = NeighborCell(cell, NeighborDirection.topLeft, condition);
            neighbors[3] = NeighborCell(cell, NeighborDirection.bottom, condition);
            neighbors[4] = NeighborCell(cell, NeighborDirection.bottomLeft, condition);

            return count;
        }

        private int ChooseNeighbor_Pent_Right(Cell cell, BoundaryCondition condition)
        {
            int count = 5;

            neighbors[0] = NeighborCell(cell, NeighborDirection.top, condition);
            neighbors[1] = NeighborCell(cell, NeighborDirection.right, condition);
            neighbors[2] = NeighborCell(cell, NeighborDirection.topRight, condition);
            neighbors[3] = NeighborCell(cell, NeighborDirection.bottom, condition);
            neighbors[4] = NeighborCell(cell, NeighborDirection.bottomRight, condition);

            return count;
        }

        private int ChooseNeighbor_Pent_Top(Cell cell, BoundaryCondition condition)
        {
            int count = 5;

            neighbors[0] = NeighborCell(cell, NeighborDirection.top, condition);
            neighbors[1] = NeighborCell(cell, NeighborDirection.left, condition);
            neighbors[2] = NeighborCell(cell, NeighborDirection.topRight, condition);
            neighbors[3] = NeighborCell(cell, NeighborDirection.right, condition);
            neighbors[4] = NeighborCell(cell, NeighborDirection.topLeft, condition);

            return count;
        }

        private int ChooseNeighbor_Pent_Bottom(Cell cell, BoundaryCondition condition)
        {
            int count = 5;

            neighbors[0] = NeighborCell(cell, NeighborDirection.bottom, condition);
            neighbors[1] = NeighborCell(cell, NeighborDirection.left, condition);
            neighbors[2] = NeighborCell(cell, NeighborDirection.bottomRight, condition);
            neighbors[3] = NeighborCell(cell, NeighborDirection.right, condition);
            neighbors[4] = NeighborCell(cell, NeighborDirection.bottomLeft, condition);

            return count;
        }

        private int ChooseNeighbor_Pent_Rand(Cell cell, BoundaryCondition condition)
        {
            int which = rand.Next(4);

            switch (which)
            {
                case 0:
                    return ChooseNeighbor_Pent_Bottom(cell, condition);
                case 1:
                    return ChooseNeighbor_Pent_Top(cell, condition);
                case 2:
                    return ChooseNeighbor_Pent_Right(cell, condition);
                case 3:
                    return ChooseNeighbor_Pent_Left(cell, condition);
            }

            return ChooseNeighbor_Pent_Left(cell, condition);
        }

        private int ChooseNeighbor_Hex_Rand(Cell cell, BoundaryCondition condition)
        {
            int which = rand.Next(2);

            switch (which)
            {
                case 0:
                    return ChooseNeighbor_Hex_Left(cell, condition);
                case 1:
                    return ChooseNeighbor_Hex_Right(cell, condition);
            }

            return ChooseNeighbor_Hex_Left(cell, condition);
        }

        private int ChooseNeighbor_Radial(Cell cell, BoundaryCondition condition, float radius) //to do: not periodic
        {
            if (radius > iterations) radius = iterations;
            if (radius > cellsInRow) radius = cellsInRow;
            float baseRadius = radius * cellSize;
            int countPicked = 0;

            int r = (int)Math.Ceiling(radius);

            double toR = radius * radius;

            for (int i = cell.Coord.Y - r; i <= cell.Coord.Y + r; ++i)
            {
                for (int j = cell.Coord.X - r; j <= cell.Coord.X + r; ++j)
                {
                    if (((j - cell.Coord.X) * (j - cell.Coord.X) + (i - cell.Coord.Y) * (i - cell.Coord.Y)) <= toR)
                    {
                        //SetStateForRadiusRand(safeIteratorX(i, BoundaryCondition.periodic), safeIteratorY(j, BoundaryCondition.periodic));
                        int yy = safeIteratorY(i, BoundaryCondition.periodic);
                        int xx = safeIteratorX(j, BoundaryCondition.periodic);
                        if (CountDistance(cell, grid[yy, xx]) < baseRadius)
                        {
                            neighbors[countPicked] = grid[yy, xx];
                            countPicked++;
                            if (countPicked >= cellsInRow * iterations) break;
                        }
                    }
                }

                if (countPicked >= cellsInRow * iterations) break;
            }

            return countPicked;
        }

        private Cell NeighborCell(Cell cell, NeighborDirection direction, BoundaryCondition condition)
        {
            bool outTopY = false, outBottomY = false, outLeftX = false, outRightX = false;

            if ((cell.Coord.Y - 1) < 0) outTopY = true;
            else if ((cell.Coord.Y + 1) > iterations - 1) outBottomY = true;

            if ((cell.Coord.X - 1) < 0) outLeftX = true;
            else if ((cell.Coord.X + 1) > cellsInRow - 1) outRightX = true;

            Cell empty = new Cell(-1, -1);

            switch (direction)
            {
                case NeighborDirection.topLeft:
                    if (condition == BoundaryCondition.periodic)
                        return grid[outTopY ? iterations - 1 : cell.Coord.Y - 1, outLeftX ? cellsInRow - 1 : cell.Coord.X - 1];
                    else
                    {
                        if (outTopY || outLeftX) return empty;
                        else return grid[cell.Coord.Y - 1, cell.Coord.X - 1];
                    }

                case NeighborDirection.top:
                    if (condition == BoundaryCondition.periodic)
                        return grid[outTopY ? iterations - 1 : cell.Coord.Y - 1, cell.Coord.X];
                    else
                    {
                        if (outTopY) return empty;
                        else return grid[cell.Coord.Y - 1, cell.Coord.X];
                    }

                case NeighborDirection.topRight:
                    if (condition == BoundaryCondition.periodic)
                        return grid[outTopY ? iterations - 1 : cell.Coord.Y - 1, outRightX ? 0 : cell.Coord.X + 1];
                    else
                    {
                        if (outTopY || outRightX) return empty;
                        else return grid[cell.Coord.Y - 1, cell.Coord.X + 1];
                    }

                case NeighborDirection.left:
                    if (condition == BoundaryCondition.periodic)
                        return grid[cell.Coord.Y, outLeftX ? cellsInRow - 1 : cell.Coord.X - 1];
                    else
                    {
                        if (outLeftX) return empty;
                        else return grid[cell.Coord.Y, cell.Coord.X - 1];
                    }

                case NeighborDirection.right:
                    if (condition == BoundaryCondition.periodic)
                        return grid[cell.Coord.Y, outRightX ? 0 : cell.Coord.X + 1];
                    else
                    {
                        if (outRightX) return empty;
                        else return grid[cell.Coord.Y, cell.Coord.X + 1];
                    }
                case NeighborDirection.bottomLeft:
                    if (condition == BoundaryCondition.periodic)
                        return grid[outBottomY ? 0 : cell.Coord.Y + 1, outLeftX ? cellsInRow - 1 : cell.Coord.X - 1];
                    else
                    {
                        if (outBottomY || outLeftX) return empty;
                        else return grid[cell.Coord.Y + 1, cell.Coord.X - 1];
                    }
                case NeighborDirection.bottom:
                    if (condition == BoundaryCondition.periodic)
                        return grid[outBottomY ? 0 : cell.Coord.Y + 1, cell.Coord.X];
                    else
                    {
                        if (outBottomY) return empty;
                        else return grid[cell.Coord.Y + 1, cell.Coord.X];
                    }
                case NeighborDirection.bottomRight:
                    if (condition == BoundaryCondition.periodic)
                        return grid[outBottomY ? 0 : cell.Coord.Y + 1, outRightX ? 0 : cell.Coord.X + 1];
                    else
                    {
                        if (outBottomY || outRightX) return empty;
                        else return grid[cell.Coord.Y + 1, cell.Coord.X + 1];
                    }
                default:
                    return empty;
            }
        }

        public void SetState(int iteration, int index, bool set, Color color)
        {
            lock (_locker2)
            {
                if (index < cellsInRow)
                {
                    grid[iteration, index].color = color;
                    grid[iteration, index].set = set;
                    FillCell(grid[iteration, index], color);
                }
            }
        }

        public void SetStateForRadiusRand(int iteration, int index)
        {
            if (index < cellsInRow)
            {
                radiusArr[iteration, index] = true;
                //FillCell(grid[iteration, index], Color.Red);
            }
        }

        private void RandCellWeight(Cell cell)
        {
            cell.Weight.X = rand.Next((int)cellSize - 1);
            cell.Weight.Y = rand.Next((int)cellSize - 1);
            int r = 1;
            cell.Weight.X = (cell.Coord.X * (int)cellSize) + (int)offset + 1 + cell.Weight.X;
            cell.Weight.Y = (cell.Coord.Y * (int)cellSize) + 1 + cell.Weight.Y;
            g.DrawEllipse(Pens.Green, cell.Weight.X - r, cell.Weight.Y - r, r + r, r + r);
        }

        public void RandWeights()
        {
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < cellsInRow; ++j)
                {
                    RandCellWeight(grid[i, j]);
                }
            }
        }

        private double CountDistance(Cell cellBase, Cell cell)
        {
            double centerX = (cellBase.Coord.X * (int)cellSize) + (int)offset + (cellSize / 2);
            double centerY = (cellBase.Coord.Y * (int)cellSize) + (cellSize / 2);

            return Math.Sqrt(Math.Pow(cell.Weight.X - centerX, 2) + Math.Pow(cell.Weight.Y - centerY, 2));
        }

        public void drawEnergyBorder()
        {
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < cellsInRow; ++j)
                {
                    if (energyBorder[i, j]>0)
                    {
                        FillCell(grid[i, j], Color.FromArgb(energyBorder[i,j]+100));
                        
                    }
                        
                    else FillCell(grid[i, j], Color.Blue);
                }
            }
            panel.Invalidate();
        }

        public void MonteCarlo(int iterCount, double kT)
        {
            int numberOfCellsToRand = cellsInRow * iterations;

            for (int iter = 0; iter < iterCount; ++iter)
            {
                for (int i = 0; i < iterations; ++i)
                {
                    for (int j = 0; j < cellsInRow; ++j)
                    {
                        grid[i, j].randHit = false;
                        energyBorder[i, j] = 0;
                    }
                }

                int iterY = 0, iterX = 0;
                bool hit = false;
                int countHits = 0;
                for (int i = 0; i < numberOfCellsToRand; ++i)
                {
                    int randCellY = rand.Next(iterations);
                    int randCellX = rand.Next(cellsInRow);
                    int k;
                    for (int j = 0; j < iterations; ++j)
                    {
                        for (k = 0; k < cellsInRow; ++k)
                        {
                            if (j + randCellY > iterations - 1) iterY = j + randCellY - iterations;
                            else iterY = j + randCellY;
                            if (k + randCellX > cellsInRow - 1) iterX = k + randCellX - cellsInRow;
                            else iterX = k + randCellX;

                            if (grid[iterY, iterX].randHit == false)
                            {
                                
                                int countNeighbors = AutomatonRules(grid[iterY, iterX]);
                                int EnergyBase = 0;
                                int EnergyNew = 0;
                                
                                for(int neighbor=0;neighbor<countNeighbors;++neighbor)
                                {
                                    if (neighbors[neighbor].color != grid[iterY, iterX].color) EnergyBase++;
                                }

                                if(EnergyBase>0) energyBorder[iterY, iterX] = EnergyBase;

                                int randNeighbor = rand.Next(countNeighbors);

                                for (int neighbor = 0; neighbor < countNeighbors; ++neighbor)
                                {
                                    if (neighbors[neighbor].color != neighbors[randNeighbor].color) EnergyNew++;
                                }

                                if (EnergyNew - EnergyBase <= 0)
                                {
                                    grid[iterY, iterX].color = neighbors[randNeighbor].color;
                                    FillCell(grid[iterY, iterX], grid[iterY, iterX].color);
                                }
                                    
                                else
                                {
                                    double probability = Math.Exp(-(EnergyNew - EnergyBase) / kT);
                                    if(rand.NextDouble() <= probability ? true : false)
                                    {
                                        grid[iterY, iterX].color = neighbors[randNeighbor].color;
                                        FillCell(grid[iterY, iterX], grid[iterY, iterX].color);
                                    }           
                                }

                                grid[iterY, iterX].randHit = true;
                                hit = true;
                                countHits++;
                                break;
                            }

                        }
                        if (hit) break;
                    }
                    hit = false;
                }
                panel.Invalidate();
                //return countHits;
            }
        }
    }
}
