using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonteCarloLines
{
    public partial class Form1 : Form
    {
        //аналитическое вычисление 
        public static double countD(double A1, double B1, double A2, double B2, double x, double y)
        {
            double D = (x - A1) * (B2 - B1) - (y - B1) * (A2 - A1);
            return D;
        }

        //метод выявления пересечения/колинеарности векторов
        public static int areIntersecting(
            double v1x1, double v1y1, double v1x2, double v1y2,
            double v2x1, double v2y1, double v2x2, double v2y2
            )
        {
            double d1, d2;
            double a1, a2, b1, b2, c1, c2;

            // Convert vector 1 to a line (line 1) of infinite length.
            // We want the line in linear equation standard form: A*x + B*y + C = 0
            // See: http://en.wikipedia.org/wiki/Linear_equation
            a1 = v1y2 - v1y1;
            b1 = v1x1 - v1x2;
            c1 = (v1x2 * v1y1) - (v1x1 * v1y2);

            // Every point (x,y), that solves the equation above, is on the line,
            // every point that does not solve it, is not. The equation will have a
            // positive result if it is on one side of the line and a negative one 
            // if is on the other side of it. We insert (x1,y1) and (x2,y2) of vector
            // 2 into the equation above.
            d1 = (a1 * v2x1) + (b1 * v2y1) + c1;
            d2 = (a1 * v2x2) + (b1 * v2y2) + c1;

            // If d1 and d2 both have the same sign, they are both on the same side
            // of our line 1 and in that case no intersection is possible. Careful, 
            // 0 is a special case, that's why we don't test ">=" and "<=", 
            // but "<" and ">".
            if (d1 > 0 && d2 > 0) return 0;
            if (d1 < 0 && d2 < 0) return 0;

            // The fact that vector 2 intersected the infinite line 1 above doesn't 
            // mean it also intersects the vector 1. Vector 1 is only a subset of that
            // infinite line 1, so it may have intersected that line before the vector
            // started or after it ended. To know for sure, we have to repeat the
            // the same test the other way round. We start by calculating the 
            // infinite line 2 in linear equation standard form.
            a2 = v2y2 - v2y1;
            b2 = v2x1 - v2x2;
            c2 = (v2x2 * v2y1) - (v2x1 * v2y2);

            // Calculate d1 and d2 again, this time using points of vector 1.
            d1 = (a2 * v1x1) + (b2 * v1y1) + c2;
            d2 = (a2 * v1x2) + (b2 * v1y2) + c2;

            // Again, if both have the same sign (and neither one is 0),
            // no intersection is possible.
            if (d1 > 0 && d2 > 0) return 0;
            if (d1 < 0 && d2 < 0) return 0;

            // If we get here, only two possibilities are left. Either the two
            // vectors intersect in exactly one point or they are collinear, which
            // means they intersect in any number of points from zero to infinite.
            if ((a1 * b2) - (a2 * b1) == 0.0f) return 2;

            // If they are not collinear, they must intersect in exactly one point.
            return 1;
        }

        //класс точки
        public class Point
        {
            public double X { get; private set; }
            public double Y { get; private set; }

            public Point(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        //класс линии
        public class Line
        {
            public double A1 { get; private set; }
            public double B1 { get; private set; }
            public double A2 { get; private set; }
            public double B2 { get; private set; }
            public double D { get; private set; } //положение линии относительно тестовой точки
            public double Length { get; private set; }

            public Line(double a1, double b1, double a2, double b2, double x, double y)
            {
                this.A1 = a1;
                this.A2 = a2;
                this.B1 = b1;
                this.B2 = b2;
                this.D = countD(A1, B1, A2, B2, x, y);
                this.Length = Math.Sqrt(Math.Pow((A2 - A1), 2) + Math.Pow((B2 - B1), 2));
            }
        }

        //класс фигуры
        public class Figure
        {
            public List<Line> lines;
            public double ProbePointX;
            public double ProbePointY;

            public Figure()
            {
                lines = new List<Line>();
                ProbePointX = 0;
                ProbePointY = 0;
            }
        }

        //координаты точек
        public List<Point> BlueList;
        public List<Point> RedList;

        //контейнеры фигур
        public Figure BlueFigure;
        public Figure RedFigure;

        public Form1()
        {
            //обработчики горячих клавиш
            this.KeyPreview = true;
            this.KeyPress += new KeyPressEventHandler(b_KeyDown);
            this.KeyPress += new KeyPressEventHandler(r_KeyDown);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            chart1.Series.Add("yeslist"); //список точек фигуры для добавления
            chart1.Series["yeslist"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series.Add("nolist"); //список точек фигуры для вычитания
            chart1.Series["nolist"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series.Add("points"); //список точек при тыке по графику
            chart1.Series["points"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            chart1.Series.Add("default"); //служебный список чтобы отображалось норм
            chart1.Series["default"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series["default"].Points.AddXY(0, 0);
            chart1.Series["points"].Color = Color.ForestGreen;
            chart1.Series["yeslist"].Color = Color.Blue;
            chart1.Series["yeslist"].BorderWidth = 3;
            chart1.Series["nolist"].Color = Color.Red;
            chart1.Series["nolist"].BorderWidth = 3;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 100;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 100;
            textBox5.Text = "10000";
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndex = comboBox1.FindStringExact("Аналитически");
            BlueList = new List<Point>();
            RedList = new List<Point>();
        }

        //добавление графика 1
        private void button1_Click(object sender, EventArgs e)
        {
            double x, y;
            Double.TryParse(textBox1.Text, out x);
            Double.TryParse(textBox2.Text, out y);
            BlueList.Add(new Point(x, y));
            chart1.Series["yeslist"].Points.DataBindXY(BlueList, "X", BlueList, "Y");
        }

        //добавление графика 2
        private void button2_Click(object sender, EventArgs e)
        {
            double x, y;
            Double.TryParse(textBox3.Text, out x);
            Double.TryParse(textBox4.Text, out y);
            RedList.Add(new Point(x, y));
            chart1.Series["nolist"].Points.DataBindXY(RedList, "X", RedList, "Y");
        }

        //завершение фигуры 1
        private void button4_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button4.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;

            double x, y;
            Double.TryParse(textBox1.Text, out x);
            Double.TryParse(textBox2.Text, out y);

            BlueList.Add(new Point(BlueList[0].X, BlueList[0].Y));
            chart1.Series["yeslist"].Points.DataBindXY(BlueList, "X", BlueList, "Y");

            //формирование фигуры и входящих в нее линий
            BlueFigure = new Figure();
            BlueFigure.ProbePointX = x;
            BlueFigure.ProbePointY = y;
            for (int i = BlueList.Count - 2; i >= 0; i--)
                BlueFigure.lines.Add(new Line (BlueList[i + 1].X, BlueList[i + 1].Y, BlueList[i].X, BlueList[i].Y, BlueFigure.ProbePointX, BlueFigure.ProbePointY));
        }

        //завершение фигуры 2
        private void button5_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button5.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;

            double x, y;
            Double.TryParse(textBox3.Text, out x);
            Double.TryParse(textBox4.Text, out y);

            RedList.Add(new Point(RedList[0].X, RedList[0].Y));
            chart1.Series["nolist"].Points.DataBindXY(RedList, "X", RedList, "Y");

            //формирование фигуры и входящих в нее линий
            RedFigure = new Figure();
            RedFigure.ProbePointX = x;
            RedFigure.ProbePointY = y;
            for (int i = RedList.Count - 2; i >= 0; i--)
                RedFigure.lines.Add(new Line(RedList[i + 1].X, RedList[i + 1].Y, RedList[i].X, RedList[i].Y, RedFigure.ProbePointX, RedFigure.ProbePointY));
        }

        //рассчитать
        private void button3_Click(object sender, EventArgs e)
        {
            if (BlueFigure != null)
            {
                button3.Enabled = false;
                textBox5.Enabled = false;

                chart1.Series.Add("bluepoints");
                chart1.Series["bluepoints"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                chart1.Series["bluepoints"].Color = Color.SkyBlue;

                chart1.Series.Add("redpoints");
                chart1.Series["redpoints"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
                chart1.Series["redpoints"].Color = Color.LightPink;

                double x, y, BluePoints = 0;
                int n;
                bool probe;
                Random random = new Random();
                string selected = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);

                Int32.TryParse(textBox5.Text, out n);
                for (int i = n; i > 0; i--)//для каждой случайной точки
                {
                    probe = true;
                    x = random.NextDouble() * 100;
                    y = random.NextDouble() * 100;

                    switch (selected)
                    {
                        case "Аналитически":
                            for (int j = BlueFigure.lines.Count() - 1; j >= 0; j--)
                                if ((countD(BlueFigure.lines[j].A1, BlueFigure.lines[j].B1, BlueFigure.lines[j].A2, BlueFigure.lines[j].B2, x, y) < 0) != (BlueFigure.lines[j].D < 0))
                                    probe = false;

                            if (RedFigure != null && probe == true)
                            {
                                probe = false;
                                for (int j = RedFigure.lines.Count() - 1; j >= 0; j--)
                                    if ((countD(RedFigure.lines[j].A1, RedFigure.lines[j].B1, RedFigure.lines[j].A2, RedFigure.lines[j].B2, x, y) < 0) != (RedFigure.lines[j].D < 0))
                                        probe = true;
                            }
                            break;

                        case "Рейкастинг":
                            int intersections = 0;
                            for (int j = BlueFigure.lines.Count() - 1; j >= 0; j--)
                                if (areIntersecting(BlueFigure.lines[j].A1, BlueFigure.lines[j].B1, BlueFigure.lines[j].A2, BlueFigure.lines[j].B2, chart1.ChartAreas[0].AxisX.Minimum - 1, chart1.ChartAreas[0].AxisY.Minimum - 1, x, y) == 1)
                                    intersections++;
                            if (intersections % 2 == 0) probe = false;

                            if (RedFigure != null && probe == true)
                            {
                                probe = false;
                                intersections = 0;
                                for (int j = RedFigure.lines.Count() - 1; j >= 0; j--)
                                    if (areIntersecting(RedFigure.lines[j].A1, RedFigure.lines[j].B1, RedFigure.lines[j].A2, RedFigure.lines[j].B2, chart1.ChartAreas[0].AxisX.Minimum - 1, chart1.ChartAreas[0].AxisY.Minimum - 1, x, y) == 1)
                                        intersections++;
                                if (intersections % 2 == 0) probe = true;
                            }
                            break;
                    }

                    if (probe == true)
                    {
                        BluePoints++;
                        chart1.Series["bluepoints"].Points.AddXY(x, y);
                    }
                    else chart1.Series["redpoints"].Points.AddXY(x, y);
                }
                label7.Text = Math.Round(((double)BluePoints * ((chart1.ChartAreas[0].AxisX.Maximum - chart1.ChartAreas[0].AxisX.Minimum) * (chart1.ChartAreas[0].AxisY.Maximum - chart1.ChartAreas[0].AxisY.Minimum)) / (double)n),5).ToString();

                double perimeter = 0;
                for (int i = BlueFigure.lines.Count() - 1; i >= 0; i--)
                    perimeter += BlueFigure.lines[i].Length;
                label8.Text = Math.Round(perimeter, 5).ToString();
            }
            else System.Windows.Forms.MessageBox.Show("Введите хотя бы одну линию и укажите внутреннюю точку!");
        }

        //клик по графику
        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if(button3.Enabled == true)
            { 
                chart1.Series[2].Points.Clear();
                double x = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X);
                double y = chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y);
                chart1.Series[2].Points.AddXY(x, y);
                if (textBox1.Enabled == true) textBox1.Text = x.ToString();
                if (textBox2.Enabled == true) textBox2.Text = y.ToString();
                if (textBox3.Enabled == true) textBox3.Text = x.ToString();
                if (textBox4.Enabled == true) textBox4.Text = y.ToString();
            }
        }

        //сброс
        private void button6_Click(object sender, EventArgs e)
        {
            BlueFigure = null;
            RedFigure = null;
            BlueList = null;
            RedList = null;
            string selected = this.comboBox1.GetItemText(this.comboBox1.SelectedItem);
            Controls.Clear();
            InitializeComponent();
            Form1_Load(this, null);
            comboBox1.SelectedIndex = comboBox1.FindStringExact(selected);
            Invalidate();
        }

        ////////////горячие клавиши///////////

        private void b_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'b')
                button1_Click(sender, e);
        }

        private void r_KeyDown(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'r')
                button2_Click(sender, e);
        }
    }
}
