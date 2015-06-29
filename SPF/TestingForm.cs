using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Xml;

namespace SPF
{
    public partial class TestingForm : Form
    {
        
        #region Private Members

        // General
        private Thread _trdTest;                                                    // Thread to run testing methods from Testing class
        private System.Windows.Forms.Timer _timer;                                  // Timer that handles thread progress update
        private Dictionary<ImageVector.ImageParameters, bool> _paramDictionary;     // Representing parameter choise of 1st tab

        // Tab 1
        private bool _BrowseFolderOKTesting1;
        private bool _BrowseLogFileOKTesting1;
        
        // Tab 2
        private GraphForm.GraphPlot _graph;                                         // Holding graph for classification testing

        // Tab 3 
        private Testing.CurvePoint[] _learningCurve;                                // Holding Learning curve
        private double[] _statisticAlgoCurve;                                       // Holding Learning curve for simple statitics algorithm
        private bool _excludeTrainingSet;                               
        private Testing.CurvePoint[] _randomLearningCurve;                          // Holding Learning curve for simple random algorithm
        private GraphForm _randomSuccess, _randomFalsePositive, _randomFalseNegative; // Holding all 3 graphs created from random learning curve
        private GraphForm _Success, _FalsePositive, _FalseNegative;                 // Holding all 3 graphs created from learning curve
        private string _lastLogFolder;                                              // Holding learning curve last log folder path
        private int _k;

       
        #endregion

        #region Properties

        // Encapsultion for private fields
        internal Dictionary<ImageVector.ImageParameters, bool> ParamDictionary
        {
            get
            {
                //set dictionary parameters according to users choice
             //   _paramDictionary[ImageVector.ImageParameters.averageGrayLevel] = _lstParameters.GetItemChecked(0);
             //   _paramDictionary[ImageVector.ImageParameters.averageRedLevel] = _lstParameters.GetItemChecked(1);
                _paramDictionary[ImageVector.ImageParameters.averageBlueLevel] = _lstParameters.GetItemChecked(2);
              //  _paramDictionary[ImageVector.ImageParameters.averageGreenLevel] = _lstParameters.GetItemChecked(3);
              //  _paramDictionary[ImageVector.ImageParameters.averageHueLevel] = _lstParameters.GetItemChecked(4);
                _paramDictionary[ImageVector.ImageParameters.averageSaturationLevel] = _lstParameters.GetItemChecked(5);
             //   _paramDictionary[ImageVector.ImageParameters.edges] = _lstParameters.GetItemChecked(6);
              //  _paramDictionary[ImageVector.ImageParameters.imageInformation] = _lstParameters.GetItemChecked(7);
                _paramDictionary[ImageVector.ImageParameters.variance] = _lstParameters.GetItemChecked(8);
              //  _paramDictionary[ImageVector.ImageParameters.numOfPoeple] = _lstParameters.GetItemChecked(9);
              //  _paramDictionary[ImageVector.ImageParameters.facesCenterOfGravityX] = _lstParameters.GetItemChecked(10);
               // _paramDictionary[ImageVector.ImageParameters.facesCenterOfGravityY] = _lstParameters.GetItemChecked(11);
               // _paramDictionary[ImageVector.ImageParameters.facesImageAreaRatio] = _lstParameters.GetItemChecked(12);
               // _paramDictionary[ImageVector.ImageParameters.distanceFromGravityCenter] = _lstParameters.GetItemChecked(13);
               // _paramDictionary[ImageVector.ImageParameters.redEye] = _lstParameters.GetItemChecked(14);

                return _paramDictionary;
            }
        }

        public int K
        {
            get { return _k; }
            set { _k = value; }
        }
        #endregion

        #region Constructors

        public TestingForm()
        {
            InitializeComponent();

            // 1st testing tab
            _BrowseFolderOKTesting1 = false;
            _BrowseLogFileOKTesting1 = false;
            btnTestTesting1.Enabled = false;

            _paramDictionary = new Dictionary<ImageVector.ImageParameters, bool>();
        }

        #endregion

        #region Form Controls Listeners

        private void _cmbxCheckParameters_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < _lstParameters.Items.Count; i++)
            {
                //empty or fill parameters in list if All or None is chosen, respectively
                if (_cmbxCheckParameters.SelectedItem.Equals("All"))
                    _lstParameters.SetItemChecked(i, true);
                if (_cmbxCheckParameters.SelectedItem.Equals("None"))
                    _lstParameters.SetItemChecked(i, false);
            }
        }

        private void btnFolderTesting1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if ((fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK))
            {
                txtFolderTesting1.Text = fbd.SelectedPath;
                _BrowseFolderOKTesting1 = true;
            }

            if (_BrowseFolderOKTesting1 && _BrowseLogFileOKTesting1)
                btnTestTesting1.Enabled = true;
        }

        private void btnFileTesting1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();
            if (sfd.CheckPathExists)
            {
                txtFileTesting1.Text = sfd.FileName;
                _BrowseLogFileOKTesting1 = true;
            }
            else
            {
                _BrowseLogFileOKTesting1 = false;
            }
            if (_BrowseFolderOKTesting1 && _BrowseLogFileOKTesting1)
                btnTestTesting1.Enabled = true;
        }

        private void btnTestTesting1_Click(object sender, EventArgs e)
        {
            Thread trdTest;
            if (rBtnTextlTesting1.Checked)
                trdTest = new Thread(new ThreadStart(threadTestImageParametersToText));
            else
                trdTest = new Thread(new ThreadStart(threadTestImageParametersToXML));

            trdTest.Start();
            btnTestTesting1.Enabled = false;

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

            //create timer - set event every 0.5 seconds
            timer.Interval = 500;
            timer.Enabled = true;
            timer.Start();
            timer.Tick += new System.EventHandler(WatchParamValueTestThread);         
       }

        private void btnPlotTesting2_Click(object sender, EventArgs e)
        {
            Thread trdTest;
            trdTest = new Thread(new ThreadStart(threadTestParameterClassification));

            trdTest.Start();
            btnPlotTesting2.Enabled = false;

            _timer = new System.Windows.Forms.Timer();

            //create timer - set event every 0.5 seconds
            _timer.Interval = 500;
            _timer.Enabled = true;
            _timer.Start();
            _timer.Tick += new System.EventHandler(WatchClassificationTestThreadProgress);
        }

        private void btnBrowseTrueTesting2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if ((fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK))
            {
                txtFolderTrueTesting2.Text = fbd.SelectedPath;
            }
        }

        private void btnBrowseFalseTesting2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if ((fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK))
            {
                txtFolderFalseTesting2.Text = fbd.SelectedPath;
            }
        }

        private void btnColorTrueTesting2_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pboxColorTrueTesting2.BackColor = cd.Color;
            }
        }

        private void btnColorFalseTesting2_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pboxColorFalseTesting2.BackColor = cd.Color;
            }
        }

        private void cmbParameter1Testing2_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtParamXTesting2.Text = cmbParameter1Testing2.SelectedIndex.ToString();
        }

        private void cmbParameter2Testing2_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtParamYTesting2.Text = cmbParameter2Testing2.SelectedIndex.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if ((fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK))
            {
                txtFolderTrueTesting3.Text = fbd.SelectedPath;
            }
        }

        private void btnBrowseFalseTesting3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if ((fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK))
            {
                txtFolderAllTesting3.Text = fbd.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.ShowDialog();
            if (sfd.CheckPathExists)
            {
                txtFileTesting1.Text = sfd.FileName;
            }
        }

        private void btnTestTesting3_Click(object sender, EventArgs e)
        {
            // Start thread
            _trdTest = new Thread(threadCalcLearningCurve);
            _trdTest.Start();

            //create timer - set event every 0.5 seconds
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 500;
            _timer.Enabled = true;
            _timer.Start();
            _timer.Tick += new System.EventHandler(WatchLCThreadProgress);
        }

        
        private void _btnDistribution_Click(object sender, EventArgs e)
        {

        }

        private bool countValsAndWriteXML(List<char>[] dist, string name)
        {
            // Create an XmlWriterSettings object with the correct options. 
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.OmitXmlDeclaration = true;

            // Open text file for writing
            XmlWriter xml = XmlWriter.Create(name + "_parameter_distributions.xml", settings);
            xml.WriteStartDocument(true);
            xml.WriteStartElement(name + "_parameter_distributions");
            
            for (int par = 0; par < dist.Length; par++)
            {
                Dictionary<char, int> classificationAmounts = new Dictionary<char, int>();
                int counter = 1;
                if (dist[par] != null)
                {
                    char val = dist[par][0];
                    for (int vec = 1; vec < dist[par].Count; vec++)
                    {
                        if (val == dist[par][vec])
                            counter++;
                        else
                        {
                            //save the counter for parameter value
                            classificationAmounts[val] = counter;
                            val = dist[par][vec];
                            counter = 1;
                        }
                    }
                    classificationAmounts[val] = counter;

                    // Write XML
                    string parameterName = ImageVector.getParameterNameByIndex(par).ToString();
                    xml.WriteStartElement(parameterName);

                    for (int i = 0; i < classificationAmounts.Count; i++)
                    {
                        xml.WriteElementString(classificationAmounts.Keys.ElementAt(i).ToString(), classificationAmounts.Values.ElementAt(i).ToString());
                    }

                    xml.WriteEndElement();
                }
            }

            xml.WriteEndElement();
            xml.WriteEndDocument();
            xml.Close();
            return true;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string myPath = _lastLogFolder;
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = myPath;
            prc.Start();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(txtNumOfPointsTesting3.Text);
                if ((num < 1) || (num > 20))
                {
                    MessageBox.Show("Number of points in the X-Axis must be between 3 to 20");
                    txtNumOfPointsTesting3.Text = "3";
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Number of points in the X-Axis must be a number");
                txtNumOfPointsTesting3.Text = "3";
            }
        }

       #endregion

        #region Method for threads

        private void threadTestImageParametersToText()
        {
           Testing.WritePicturesParametersToFile(txtFolderTesting1.Text, txtFileTesting1.Text, ParamDictionary);
        }

        private void threadTestImageParametersToXML()
        {
           Testing.WritePicturesParametersToXML(txtFolderTesting1.Text, txtFileTesting1.Text, ParamDictionary);
        }

        private void threadTestParameterClassification()
        {
            // Get folders
            string folderTrue = txtFolderTrueTesting2.Text;
            string folderFalse = txtFolderFalseTesting2.Text;

            // Get wanted parameters
            int parXindex = Convert.ToInt16(txtParamXTesting2.Text);
            int parYindex = Convert.ToInt16(txtParamYTesting2.Text);

            // Get wanted colors
            Color cTrue = pboxColorTrueTesting2.BackColor;
            Color cFalse = pboxColorFalseTesting2.BackColor;

            // Construct graph
            GraphForm.GraphPlot graph = Testing.makeGraph(folderTrue, folderFalse, parXindex, parYindex, cTrue, cFalse);

            _graph = graph;
        }

        private void threadCalcLearningCurve()
        {
            // Make Learning & Deciding objects
            const string USERNAME = "TestLearningCurve";
           
             DataLearning learning;
             if (_rdoKNN.Checked)
             {
                 //K = int.Parse(_txtK.Text);
                 K = 1;
                 learning = new DataLearning(USERNAME, LearningAlgorithm.Algorithm.KNN, ParamDictionary, K);
             }
             else if (_rdoTree.Checked)
             {
                 K = 0;
                 learning = new DataLearning(USERNAME, LearningAlgorithm.Algorithm.DecisionTree, ParamDictionary, K);
             }
             else // if (_rdoTreeNum.Checked)
             {
                 K = 0;
                 learning = new DataLearning(USERNAME, LearningAlgorithm.Algorithm.DecisionTreeNumerical, ParamDictionary, K);
             }

             DecisionMaking deciding = new DecisionMaking(USERNAME, learning.Algorithm, ParamDictionary);

            // Load vectors to repository
            learning.Repository.loadList();

            // Scan pictures
            Testing.scanPicturesIntoReporistory(txtFolderAllTesting3.Text, txtFolderTrueTesting3.Text, learning.Repository, ParamDictionary);

            // Set X-Axis points for learning curve
            int nPoints = Convert.ToInt16(txtNumOfPointsTesting3.Text);
            int[] percent = new int[nPoints];
            for (int i = 0; i < nPoints; i++)
                if (!chkExclude.Checked)
                    percent[i] = (int)(((i + 1) * 100) / (nPoints));
                else
                    percent[i] = (int)(((i + 1) * 100) / (nPoints+1));

            // Set method for handeling identical vectors with different choice
            double[] simpleAlgorithm = new double[3];

            // Set method for handeling identical vectors with different choice
            Testing.HandleIdenticalMethod identicalMethod = Testing.HandleIdenticalMethod.Ignore;
            if (rdoSameRemove.Checked)
                identicalMethod = Testing.HandleIdenticalMethod.Remove;

            // Set if training set should be excluded from testing set
            _excludeTrainingSet = chkExclude.Checked;

            // Calc learning curves
  
            _learningCurve = Testing.LearningCurve(txtFolderAllTesting3.Text, txtFolderTrueTesting3.Text, learning, deciding, percent, out simpleAlgorithm, identicalMethod, _excludeTrainingSet);
            _statisticAlgoCurve = simpleAlgorithm;
            _trdTest.Abort();
        }

        #endregion

        #region Progress-Check Methods

        /* Updating progress of parameter value test method thread */
        private void WatchParamValueTestThread(object source, EventArgs e)
        {   
           pbTesting.Value = Testing.Progress;
           if (Testing.Progress==0)
               btnTestTesting1.Enabled = true;
        }

        /* Updating progress of classification test method thread */
        private void WatchClassificationTestThreadProgress(object source, EventArgs e)
        {
            pbTesting2.Value = Testing.Progress;
            if (_graph != null)
            {
                _timer.Stop();

                // Draw
                GraphForm graphForm = new GraphForm(_graph);
                graphForm.Show();

            }
        }

        /* Updating progress of LearningCurve method thread */
        private void WatchLCThreadProgress(object source, EventArgs e)
        {
            pbTesting3.Value = Testing.Progress;
            plTesting3.Text = Testing.ProgressString;

            if (!_trdTest.IsAlive)
            {
                // Plot learning curves
                PlotLearningCurve(_learningCurve, out _Success, out _FalseNegative, out _FalsePositive, _statisticAlgoCurve, _excludeTrainingSet);

                // Plot random algorithm learning curves
              //  PlotRandomAlgoLearningCurve(_randomLearningCurve, out _randomSuccess,  out _randomFalseNegative, out _randomFalsePositive, _statisticAlgoCurve);
                // Save log and curves
               // Boolean saveRandom = true;
                saveLogAndCurves();

                _timer.Enabled = false;

                pbTesting3.Value = Testing.Progress;
                plTesting3.Text = Testing.ProgressString;
            }

        }

        #endregion

        #region Testing-Related Methods

        private void PlotLearningCurve(Testing.CurvePoint[] learningCurve, out GraphForm graphFormS, out GraphForm graphFormFn, out GraphForm graphFormFp, double[] simpleAlgorithm, bool excludeLearnedFromTestingSet)
       {
           // Create 3 new graphs
           GraphForm.GraphPlot graphSuccess = new GraphForm.GraphPlot("TrainingSet", "Success");
           GraphForm.GraphPlot graphFN = new GraphForm.GraphPlot("TrainingSet", "False Negative");
           GraphForm.GraphPlot graphFP = new GraphForm.GraphPlot("TrainingSet", "False Positive");
           graphSuccess.setUsedForLearningCurve(true);
           graphFN.setUsedForLearningCurve(true);
           graphFP.setUsedForLearningCurve(true);

           // add point (0,0)
           graphSuccess.addPoint(0, 0, "", Color.Black);
           graphFN.addPoint(0, 0, "", Color.Black);
           graphFP.addPoint(0, 0, "", Color.Black);
           foreach (Testing.CurvePoint p in learningCurve)
           {
               graphSuccess.addPoint(p.getTrainingSetPercentage(), p.getAvgPercentageCorrect(), Math.Round(p.getStdPercentageCorrect(), 2).ToString(), Color.Black);
               graphFN.addPoint(p.getTrainingSetPercentage(), p.getAvgPercentageFalseNegative(), Math.Round(p.getStdPercentageFalseNegative(), 2).ToString(), Color.Black);
               graphFP.addPoint(p.getTrainingSetPercentage(), p.getAvgPercentageFalsePositive(), Math.Round(p.getStdPercentageFalsePositive(), 2).ToString(), Color.Black);
           }
           // add point (100,100) or (100,0)
           graphSuccess.addPoint(100, 100, "", Color.Black);
           graphFN.addPoint(100, 0, "", Color.Black);
           graphFP.addPoint(100, 0, "", Color.Black);

           //add comparison line for statistical simple algorithms
           string partially = (excludeLearnedFromTestingSet) ? "" : "partially_statistical";
           graphSuccess.addComparisonLines(simpleAlgorithm[0], "statistical", partially);
           graphFN.addComparisonLines(simpleAlgorithm[1], "statistical", partially);
           graphFP.addComparisonLines(simpleAlgorithm[2], "statistical", partially);

           // Draw
           graphFormS = new GraphForm(graphSuccess);
           graphFormFn = new GraphForm(graphFN);
           graphFormFp = new GraphForm(graphFP);
           graphFormS.Show();
           graphFormFn.Show();
           graphFormFp.Show();
       }

   
        public bool saveLogAndCurves()
       {
           try
           {
               // Create Dir, save log file and images
               string str;
               string dirname = DateTime.Now.ToString().Replace('/', '_');
               dirname = dirname.Replace(':', '_');
               dirname = dirname.Replace(' ', '_');
               if (_rdoKNN.Checked)
                   str="KNN_";
               else
                   str ="Tree_";
               dirname = "LC_" + str+ dirname;

               Directory.CreateDirectory(dirname);
               Testing.WriteLearningCurveToLog(_learningCurve, dirname + "\\" + "Log.xml");
               _Success.saveImageToFile(dirname + "\\" + "CurveSuccess.bmp");
               _FalsePositive.saveImageToFile(dirname + "\\" + "CurveFalsePositive.bmp");
               _FalseNegative.saveImageToFile(dirname + "\\" + "Curve FalseNegative.bmp");
       
               _lastLogFolder = dirname;
               button1.Enabled = true;
               
               return true;
           }
           catch (Exception)
           {
               return false;
           }
       }

        #endregion

        private void button2_Click_1(object sender, EventArgs e)
        {
            const string USERNAME = "TestLearningCurve";
            VectorRepository repository = new VectorRepository(USERNAME);
            // Load vectors to repository
            repository.loadList();

            //use vectors already in repository
            // Scan pictures
            //Testing.scanPicturesIntoReporistory(txtFolderAllTesting3.Text, txtFolderTrueTesting3.Text, learning.Repository, ParamDictionary3);
            // int paramsCount = 0;
            foreach (ImageVector.ImageParameters parameter in ParamDictionary.Keys)
            {
                if (_paramDictionary[parameter])
                {
                    //paramsCount++;
                }
            }

            //array of lists. each list is for 1 parameter, inside are the values of all vectors for this parameter 
            List<char>[] trueDist = new List<char>[ImageVector.NUMBER_OF_PARAMETERS];
            List<char>[] falseDist = new List<char>[ImageVector.NUMBER_OF_PARAMETERS];

            //scan lists and check value of each parameter in dictionary
            foreach (ImageVector.ImageParameters parameter in ParamDictionary.Keys)
            {
                if (_paramDictionary[parameter])
                {
                    trueDist[(int)parameter] = new List<char>();
                    foreach (ImageVector vec in repository.VectorListTrue)
                    {
                        char val = Classifier.getParameterClassification(parameter, vec.getParameter(parameter));
                        trueDist[(int)parameter].Add(val);
                    }
                    falseDist[(int)parameter] = new List<char>();
                    foreach (ImageVector vec in repository.VectorListFalse)
                    {
                        char val = Classifier.getParameterClassification(parameter, vec.getParameter(parameter));
                        falseDist[(int)parameter].Add(val);
                    }
                }
            }
            List<char>[] allDist = new List<char>[ImageVector.NUMBER_OF_PARAMETERS];



            //make order in arrays and count values
            for (int i = 0; i < trueDist.Length; i++)
            {
                if (trueDist[i] != null)
                {
                    allDist[i] = new List<char>(trueDist[i]);
                    allDist[i].AddRange(falseDist[i]);
                    allDist[i].Sort();
                    trueDist[i].Sort();
                    falseDist[i].Sort();
                }
            }
            countValsAndWriteXML(allDist, "all");
            countValsAndWriteXML(trueDist, "true");
            countValsAndWriteXML(falseDist, "false");

        }




    }
}
