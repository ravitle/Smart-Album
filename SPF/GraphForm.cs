using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SPF
{
    public partial class GraphForm : Form
    {

        #region Private Members

        private GraphPlot _graph;

        #endregion

        #region Constructors

        public GraphForm(GraphPlot graph)
        {
            InitializeComponent();
            _graph = graph;
            graphTimer.Enabled = true;
        }

        #endregion

        #region Form Control Listeners

        private void GraphForm_Load(object sender, EventArgs e)
        {
            Graphics graphics = pboxGraph.CreateGraphics();
            _graph.Plot(ref graphics, pboxGraph.Width, pboxGraph.Height);
            graphTimer.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        #endregion 

        #region Methods

        public void saveImageToFile(string filename)
        {
            Bitmap bmp = new Bitmap(pboxGraph.Width, pboxGraph.Height);
            Graphics graphics = Graphics.FromImage(bmp);
            _graph.Plot(ref graphics, pboxGraph.Width, pboxGraph.Height);
            bmp.Save(filename);
        }

        // Plot every 8 sec
        private void graphTimer_Tick(object sender, EventArgs e)
        {
            graphTimer.Interval = 8000;
            this.Show();
            Graphics graphics = pboxGraph.CreateGraphics();
            _graph.Plot(ref graphics, pboxGraph.Width, pboxGraph.Height);
        }

        #endregion

        #region Auxiliary Classes

        public class GraphPlot
        {

            #region Private Members

            private string _xName;
            private string _yName;
            private List<double> _x;
            private List<double> _y;
            private List<Color> _colors;
            private List<string> _names;
            private List<double> _classificationsX;
            private List<string> _classificationsNamesX;
            private List<double> _classificationsY;
            private List<string> _classificationsNamesY;
            private double _comparisonLineY;
            private string _comparisonLineStr1, _comparisonLineStr2;
            private bool _usedForLearningCurve;

            #endregion

            #region Constructor

            // Constructor. AxisX and AxisY are the strings representing each Axis.
            public GraphPlot(string AxisX, string AxisY)
            {
                _xName = AxisX;
                _yName = AxisY;
                _classificationsX = new List<double>();
                _classificationsNamesX = new List<string>();
                _classificationsY = new List<double>();
                _classificationsNamesY = new List<string>();
                _x = new List<double>();
                _y = new List<double>();
                _colors = new List<Color>();
                _names = new List<string>();
                _usedForLearningCurve = false;
            }

            #endregion

            #region Public Methods

            public void setUsedForLearningCurve(bool value)
            {
                _comparisonLineY = -1;
                _usedForLearningCurve = value;
            }

            public void addComparisonLines(double lineX, string comparisonStr1, string comparisonStr2)
            {

                _comparisonLineY = lineX;   //the point where to draw the line (parallel to x)
                _comparisonLineStr1 = comparisonStr1;
                _comparisonLineStr2 = comparisonStr2;
            }

            // Adds classifications.upTo is the classification boundry value, classChar is the classifications
            // representing strings. classChar should have one item more than upTo.
            public void addXClassification(List<double> upTo, List<string> classChar)
            {
                _classificationsX = upTo;
                _classificationsNamesX = classChar;
            }

            // Adds classifications.upTo is the classification boundry value, classChar is the classifications
            // representing strings. classChar should have one item more than upTo.
            public void addYClassification(List<double> upTo, List<string> classChar)
            {
                _classificationsY = upTo;
                _classificationsNamesY = classChar;
            }

            // Adds a point to the graph.
            public bool addPoint(double x, double y, string name, Color color)
            {
                _x.Add(x);
                _y.Add(y);
                _names.Add(name);
                _colors.Add(color);
                return true;
            }

            // Drawing graph on a given PictureBox pb
            public bool Plot(ref Graphics graphics, int width, int height)
            {
                const double AXIS_MARGIN = 0.05;
                const double MIN_MAX_MARGIN_FROM_GRAPH_BORDER = 0.05;
                const int AXIS_DIVISION = 10;

                // Paint Background
                SolidBrush brush = new SolidBrush(Color.White);
                graphics.FillRectangle(brush, new Rectangle(0, 0, width, height));

                // Paint XY Axis
                brush.Color = Color.Black;
                Pen pen = new Pen(brush, 2);
                graphics.DrawLine(pen, (int)(width * AXIS_MARGIN), height - (int)(height * AXIS_MARGIN), (int)(width * AXIS_MARGIN),(int)(height*AXIS_MARGIN));
                graphics.DrawLine(pen, (int)(width * AXIS_MARGIN), height - (int)(height * AXIS_MARGIN), width - (int)(width*AXIS_MARGIN), height - (int)(height * AXIS_MARGIN));

                // Write Axis names
                Font f = new Font("Tahoma", 10);
                graphics.DrawString(_xName, f, brush, new Point(width - (8 * _xName.Length), height - 17));
                graphics.DrawString(_yName, f, brush, new Point(2, 2));

                if (_x.Count == 0)
                    return false;
                
                // Check boundries
                double minX = _x[0];
                double minY = _y[0];
                double maxX = _x[0];
                double maxY = _y[0];
                for (int i = 1; i < _x.Count; i++)
                {
                    if (_x[i] < minX)
                        minX = _x[i];
                    if (_x[i] > maxX)
                        maxX = _x[i];
                    if (_y[i] < minY)
                        minY = _y[i];
                    if (_y[i] > maxY)
                        maxY = _y[i];
                }

                // Set graph boundries
                double graphBorderMinX;
                double graphBorderMaxX;
                double graphBorderMinY;
                double graphBorderMaxY;
                if (_usedForLearningCurve)
                {
                    graphBorderMinX = 0;
                    graphBorderMaxX = 100;
                    graphBorderMinY = 0;
                    graphBorderMaxY = 100;
                }
                else
                {
                    graphBorderMinX = minX - (maxX - minX) * (MIN_MAX_MARGIN_FROM_GRAPH_BORDER);
                    graphBorderMaxX = maxX + (maxX - minX) * (MIN_MAX_MARGIN_FROM_GRAPH_BORDER);
                    graphBorderMinY = minY - (maxY - minY) * (MIN_MAX_MARGIN_FROM_GRAPH_BORDER);
                    graphBorderMaxY = maxY + (maxY - minY) * (MIN_MAX_MARGIN_FROM_GRAPH_BORDER);
                }

                double oneStepX = (width - 2 * (width * AXIS_MARGIN))/11;
                double oneStepY = (height - 2 * (height * AXIS_MARGIN))/11;

                f = new Font("Tahoma", 7);
                double stepValueX = (graphBorderMaxX - graphBorderMinX) / 9;
                double stepValueY = (graphBorderMaxY - graphBorderMinY) / 9;
                
                int currentStep = (int)((width * AXIS_MARGIN) + oneStepX);
                double currentValue;
                for (int i = 2; i < AXIS_DIVISION + 2; i++)
                {
                    pen.Color = Color.LightGray;
                    pen.Width = 1;
                    graphics.DrawLine(pen, currentStep, height - (int)(height * AXIS_MARGIN), currentStep, (int)((height * AXIS_MARGIN) + oneStepY - 5));
                    pen.Color = Color.Black;
                    pen.Width = 2;
                    graphics.DrawLine(pen, currentStep, height - (int)(height * AXIS_MARGIN) - 5, currentStep, height - (int)(height * AXIS_MARGIN) + 5);
                    currentValue = graphBorderMinX + (stepValueX* (i-2));
                    graphics.DrawString(doubleToString(currentValue), f, brush, new Point(currentStep, height - (int)(height * AXIS_MARGIN) + 5));
                    currentStep = (int)((width * AXIS_MARGIN) + oneStepX*i);
                }
                currentStep = (int)((height * AXIS_MARGIN) + oneStepY);
                for (int i = 2; i < AXIS_DIVISION + 2; i++)
                {
                    pen.Color = Color.LightGray;
                    pen.Width = 1;
                    graphics.DrawLine(pen, (int)(width * AXIS_MARGIN), currentStep, (int)(width - oneStepX - (width * AXIS_MARGIN) + 5), currentStep);
                    pen.Color = Color.Black;
                    pen.Width = 2;
                    graphics.DrawLine(pen, (int)(width * AXIS_MARGIN) - 5, currentStep, (int)(width * AXIS_MARGIN) + 5, currentStep);
                    currentValue = graphBorderMaxY - (stepValueY * (i- 2));
                    graphics.DrawString(doubleToString(currentValue), f, brush, new Point(2, currentStep));
                    currentStep = (int)((height * AXIS_MARGIN) + oneStepY*i);
                }

                double startX = (width * AXIS_MARGIN) + oneStepX;
                double startY =  height - (height * AXIS_MARGIN) - oneStepX;

                // Check classification values
                if ((_classificationsNamesX.Count - 1 == _classificationsX.Count)
                    || (_classificationsNamesY.Count - 1 == _classificationsY.Count))
                {

                    // Draw classifications
                    f = new Font("Tahoma", 12);
                    pen.Width = 2;
                    brush.Color = Color.LightSeaGreen;
                    double lastClassificationPosX = (width * AXIS_MARGIN);
                    double lastClassificationPosY = height - (height * AXIS_MARGIN);

                    // Remove irelevant classifications
                    for (int i = 0; i < _classificationsX.Count; i++)
                    {
                        if (_classificationsX[i] < graphBorderMinX)
                        {
                            _classificationsX.RemoveAt(i);
                            _classificationsNamesX.RemoveAt(i);
                        }
                        if (_classificationsX[i] > graphBorderMaxX)
                        {
                            _classificationsX.RemoveAt(i);
                            _classificationsNamesX.RemoveAt(i + 1);
                        }
                    }
                    for (int i = 0; i < _classificationsY.Count; i++)
                    {
                        if (_classificationsY[i] < graphBorderMinY)
                        {
                            _classificationsY.RemoveAt(i);
                            _classificationsNamesY.RemoveAt(i);
                        }
                        if (_classificationsY[i] > graphBorderMaxY)
                        {
                            _classificationsY.RemoveAt(i);
                            _classificationsNamesY.RemoveAt(i + 1);
                        }

                    }

                    // Draw X calssifications
                    pen.Color = Color.LightBlue;
                    graphics.DrawString(_classificationsNamesX[0], f, brush, new Point((int)lastClassificationPosX + 2, (int)(height * AXIS_MARGIN)));
                    for (int i = 0; i < _classificationsX.Count; i++)
                    {
                        lastClassificationPosX = (int)((((_classificationsX[i] - graphBorderMinX) / stepValueX) * oneStepX) + startX);
                        graphics.DrawLine(pen, (int)lastClassificationPosX, height - (int)(height * AXIS_MARGIN), (int)lastClassificationPosX, (int)((height * AXIS_MARGIN)));
                        graphics.DrawString(_classificationsNamesX[i + 1], f, brush, new Point((int)lastClassificationPosX + 2, (int)(height * AXIS_MARGIN)));
                    }

                    // Draw Y calssifications
                    for (int i = 0; i < _classificationsY.Count; i++)
                    {
                        lastClassificationPosY = (int)((startY - ((_classificationsY[i] - graphBorderMinY) / stepValueY) * oneStepY));
                        graphics.DrawLine(pen, (int)(width * AXIS_MARGIN), (int)lastClassificationPosY, (int)(width - (width * AXIS_MARGIN)), (int)lastClassificationPosY);
                        graphics.DrawString(_classificationsNamesY[i], f, brush, new Point((int)(width - oneStepX - (width * AXIS_MARGIN)), (int)lastClassificationPosY + 2));
                    }
                    graphics.DrawString(_classificationsNamesY[_classificationsNamesY.Count - 1], f, brush, new Point((int)(width - oneStepX - (width * AXIS_MARGIN) + 5), (int)(height * AXIS_MARGIN)));
                }

                // Draw points
                f = new Font("Tahoma", 10);
                int xPos = (int)startX;
                int yPos = (int)startY;
                int xPrev = -1;
                int yPrev = -1;
                for (int i = 0; i < _x.Count; i++)
                {
                    brush.Color = _colors[i];
                    xPrev = xPos;
                    yPrev = yPos;
                    xPos = (int)((((_x[i] - graphBorderMinX) / stepValueX) * oneStepX) + startX);
                    yPos = (int)((startY - ((_y[i] - graphBorderMinY) / stepValueY) * oneStepY));
                    graphics.FillEllipse(brush, xPos - 5, yPos - 5, 10, 10);
                    if ((_usedForLearningCurve) && (i > 0))
                        graphics.DrawLine(new Pen(brush), xPrev, yPrev, xPos, yPos);
                    brush.Color = Color.Black;
                    graphics.DrawString(_names[i], f, brush, new Point(xPos, yPos));
                }

                if ((_usedForLearningCurve) && (_comparisonLineY >= 0) && (_comparisonLineY <= 100))
                {
                    // Draw comparison line of statistical simple algorithm
                    brush.Color = Color.Red;
                    xPos = (int)(startX);
                    yPos = (int)((startY - ((_comparisonLineY - graphBorderMinY) / stepValueY) * oneStepY));
                    graphics.FillEllipse(brush, xPos - 5, yPos - 5, 10, 10);
                   // brush.Color = Color.Black;
                   // graphics.DrawString(_comparisonLineStr1, f, brush, new Point(xPos, yPos));
                    brush.Color = Color.Red;
                    xPrev = xPos;
                    yPrev = yPos;
                    xPos = (int)((((100 - graphBorderMinX) / stepValueX) * oneStepX) + startX);
                    yPos = (int)((startY - ((_comparisonLineY - graphBorderMinY) / stepValueY) * oneStepY));
                    graphics.FillEllipse(brush, xPos - 5, yPos - 5, 10, 10);
                   // brush.Color = Color.Black;
                    graphics.DrawString(_comparisonLineStr1, f, brush, new Point(xPos, yPos));
                   // brush.Color = Color.Red;
                    graphics.DrawLine(new Pen(brush), xPrev, yPrev, xPos, yPos);

                    // Draw comparison line of partially statistical simple-algorithm
                    if (_comparisonLineStr2 != String.Empty)
                    {
                        brush.Color = Color.DarkMagenta;
                        xPos = (int)(startX);
                        yPos = (int)((startY - ((_comparisonLineY - graphBorderMinY) / stepValueY) * oneStepY));
                        graphics.FillEllipse(brush, xPos - 5, yPos - 5, 10, 10);
                        //brush.Color = Color.Black;
                        //graphics.DrawString(_comparisonLineStr2, f, brush, new Point(xPos, yPos));
                        // brush.Color = Color.Magenta;
                        xPrev = xPos;
                        yPrev = yPos;
                        xPos = (int)((((100 - graphBorderMinX) / stepValueX) * oneStepX) + startX);
                        if (_yName.Equals("Success"))
                            yPos = (int)(graphBorderMaxY);
                        else
                            yPos = (int)(startY);
                        graphics.FillEllipse(brush, xPos - 5, yPos - 5, 10, 10);
                        // brush.Color = Color.Black;
                        graphics.DrawString(_comparisonLineStr2, f, brush, new Point(xPos, yPos));
                        // brush.Color = Color.Magenta;
                        graphics.DrawLine(new Pen(brush), xPrev, yPrev, xPos, yPos);
                    }
                }

                
          

                return true;
            }

            #endregion

            #region Auxiliary Methods

            // toString for double with a 1 digit percision
            private string doubleToString(double num)
            {
                string str;
                str = num.ToString();
                str = str.Substring(0, str.IndexOf('.') + 2);
                return str;
            }

            #endregion

        }

        #endregion

    }
}
