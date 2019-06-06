using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Automat1D
{
    public partial class Form1 : Form
    {
        private Automaton automaton;
        private int size;
        System.Timers.Timer myTimer = new System.Timers.Timer();

        public Form1()
        {
            InitializeComponent();
            EnableDoubleBuffering();
            automaton = new Automaton(panel1, myTimer, label4, label17);
            myTimer.Elapsed += new ElapsedEventHandler(myEvent);
            myTimer.Interval = 100;
            InitializeComboBox();
            comboBox1.SelectedIndex = 0;
            comboBox2.DataSource = Enum.GetValues(typeof(Neighborhood));
        }

        private void InitializeComboBox()
        {
            ArrayList ColorList = new ArrayList();
            Type colorType = typeof(Color);
            PropertyInfo[] propInfoList = colorType.GetProperties(BindingFlags.Static |
                                          BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (PropertyInfo c in propInfoList)
            {
                comboBox1.Items.Add(c.Name);
            }
        }

        public void EnableDoubleBuffering()
        {
            this.SetStyle(ControlStyles.DoubleBuffer |
               ControlStyles.UserPaint |
               ControlStyles.AllPaintingInWmPaint,
               true);
            this.UpdateStyles();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            myTimer.Enabled = false;
            //if (textBox1.TextLength <= 0) size = 0;
            //else size = int.Parse(textBox1.Text);
            size = (int)numericUpDown1.Value;
            int iterations;
            automaton.radius = (float)numericUpDown4.Value;
            //if (textBox2.TextLength <= 0) iterations = 0;
            //else iterations = int.Parse(textBox2.Text);
            iterations = (int)numericUpDown2.Value;
            myTimer.Interval = (int)numericUpDown3.Value;
            automaton.CreateGrid(size, iterations);
            automaton.DrawGrid();
            label4.BackColor = Color.Red;
            automaton.neighborhood = (Neighborhood)comboBox2.SelectedItem;
            automaton.periodic = checkBox1.Checked;
            label11.Text = "0";
            automaton.RandWeights();
            panel1.Invalidate();
        }

        private void Step()
        {
            automaton.Run();
            automaton.radius= (float)numericUpDown4.Value;

            panel1.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if (textBox3.TextLength <= 0) myTimer.Interval = 100;
            //else myTimer.Interval = int.Parse(textBox3.Text);
            myTimer.Interval = (int)numericUpDown3.Value;
            myTimer.Enabled = !(myTimer.Enabled);
            if (myTimer.Enabled) label4.BackColor = Color.Green;
            else label4.BackColor = Color.Red;
        }

        private void myEvent(object source, ElapsedEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Step();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            label4.Text = elapsedMs.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            myTimer.Enabled = false;
            label4.BackColor = Color.Red;
            Step();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //if (textBox5.TextLength <= 0) automaton.numberOfCellsToRand = 0;
            //else automaton.numberOfCellsToRand = int.Parse(textBox5.Text);
            automaton.numberOfCellsToRand = (int)numericUpDown5.Value;
            if (automaton.numberOfCellsToRand > automaton.cellsInRow * automaton.iterations) automaton.numberOfCellsToRand = automaton.cellsInRow * automaton.iterations;
            label11.Text = automaton.RandomCells().ToString();
            myTimer.Enabled = false;
            label4.BackColor = Color.Red;

            panel1.Invalidate();
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null)
            {
                label5.Text = ("X= " + mouseEventArgs.X + " Y= " + mouseEventArgs.Y);
                automaton.FillWithClick(mouseEventArgs.X, mouseEventArgs.Y, Color.FromName(comboBox1.SelectedItem.ToString()));
                automaton.radius= (float)numericUpDown4.Value;
            }
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground(); // this fixes an issue of mouseover changing to bold

            if (e.Index >= 0)
            {
                Graphics g = e.Graphics;
                Rectangle rect = e.Bounds;

                string colorName = ((ComboBox)sender).Items[e.Index].ToString();
                Font colorFont = new Font("Microsoft Sans Serif", 8);


                Color rectColor = Color.FromName(colorName);
                Brush rectBrush = new SolidBrush(rectColor);
                g.FillRectangle(rectBrush, 0, 0,
                                rect.Width, rect.Height);
                g.DrawString(colorName, colorFont, Brushes.Black, rect.X, rect.Top);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            automaton.periodic = checkBox1.Checked;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            automaton.neighborhood = (Neighborhood)comboBox2.SelectedItem;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //if (textBox4.TextLength <= 0) automaton.radius = 0;
            //else automaton.radius = float.Parse(textBox4.Text);
            automaton.radius = (float)numericUpDown4.Value;
            automaton.numberOfCellsToRand = (int)numericUpDown5.Value;
            if (automaton.numberOfCellsToRand > automaton.cellsInRow * automaton.iterations) automaton.numberOfCellsToRand = automaton.cellsInRow * automaton.iterations;
            label11.Text = automaton.RandWithRadius().ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int uniX, uniY;
            //if (textBox6.TextLength <= 0) uniX = 0;
            //else uniX = int.Parse(textBox6.Text);
            uniX = (int)numericUpDown6.Value;
            //if (textBox7.TextLength <= 0) uniY = 0;
            //else uniY = int.Parse(textBox7.Text);
            uniY = (int)numericUpDown7.Value;
            if (uniX != 0 && uniY != 0) automaton.SetUniformly(uniX, uniY);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            automaton.MonteCarlo((int)numericUpDown9.Value, (double)numericUpDown8.Value);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            automaton.drawEnergyBorder();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            automaton.DRX((int)numericUpDown10.Value, (double)numericUpDown11.Value, (double)numericUpDown12.Value);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            automaton.drawDissMap();
        }
    }
}
