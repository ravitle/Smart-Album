using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
//using System.Windows.Forms.Timer;


namespace SPF
{

    public partial class MainForm : Form
    {

        public String User
        {
            get
            {
                return _txtUser.Text;
            }
            set { _txtUser.Text = value; }
        }

        public String PredictFolder
        {
            get { return _txtPredictFolder.Text; }
            set { _txtPredictFolder.Text = value; }
        }

        public String TrueFolder
        {
            get { return _txtFolderTrue.Text; }
            set { _txtFolderTrue.Text = value; }
        }

        public String AllFolder
        {
            get { return _txtFolderAll.Text; }
            set { _txtFolderAll.Text = value; }

        }

        private int _numAllFiles;
        public int NumAllFiles
        {
            get { return _numAllFiles; }
            set { _numAllFiles = value; }
        }

        private int _numPredictFiles;
        public int NumPredictFiles
        {
            get { return _numPredictFiles; }
            set { _numPredictFiles = value; }

        }
        public static string truefold;
        private bool is_user_rate = false;

        public static bool weight;
        private LearningAlgorithm.Algorithm SelectedAlgorithm
        {
            get
            {
                LearningAlgorithm.Algorithm selectedAlgorithm = LearningAlgorithm.Algorithm.DecisionTree;
                if (this._rdoKNN.Checked)
                {
                    selectedAlgorithm = LearningAlgorithm.Algorithm.KNN;
                    weight = false;
                }
                if (this._rdoKNNweight.Checked)
                {
                    selectedAlgorithm = LearningAlgorithm.Algorithm.KNN;
                    weight = true;
                }
                return selectedAlgorithm;
            }
        }

        private Dictionary<ImageVector.ImageParameters, bool> _parametersDictionary;
        internal Dictionary<ImageVector.ImageParameters, bool> ParametersDictionary
        {
            get
            {
                //set dictionary parameters according to users choice
            //    _parametersDictionary[ImageVector.ImageParameters.averageGrayLevel] = _lstParameters.GetItemChecked(0);
    //            _parametersDictionary[ImageVector.ImageParameters.averageRedLevel] = _lstParameters.GetItemChecked(1);
                _parametersDictionary[ImageVector.ImageParameters.averageBlueLevel] = _lstParameters.GetItemChecked(2);
     //           _parametersDictionary[ImageVector.ImageParameters.averageGreenLevel] = _lstParameters.GetItemChecked(3);
      //          _parametersDictionary[ImageVector.ImageParameters.averageHueLevel] = _lstParameters.GetItemChecked(4);
                _parametersDictionary[ImageVector.ImageParameters.averageSaturationLevel] = _lstParameters.GetItemChecked(5);
          //      _parametersDictionary[ImageVector.ImageParameters.edges] = _lstParameters.GetItemChecked(6);
       //         _parametersDictionary[ImageVector.ImageParameters.imageInformation] = _lstParameters.GetItemChecked(7);
                _parametersDictionary[ImageVector.ImageParameters.variance] = _lstParameters.GetItemChecked(8);
          //      _parametersDictionary[ImageVector.ImageParameters.numOfPoeple] = _lstParameters.GetItemChecked(9);
        //        _parametersDictionary[ImageVector.ImageParameters.facesCenterOfGravityX] = _lstParameters.GetItemChecked(10);
         //       _parametersDictionary[ImageVector.ImageParameters.facesCenterOfGravityY] = _lstParameters.GetItemChecked(11);
          //      _parametersDictionary[ImageVector.ImageParameters.facesImageAreaRatio] = _lstParameters.GetItemChecked(12);
          //      _parametersDictionary[ImageVector.ImageParameters.distanceFromGravityCenter] = _lstParameters.GetItemChecked(13);
          //      _parametersDictionary[ImageVector.ImageParameters.redEye] = _lstParameters.GetItemChecked(14);

                return _parametersDictionary;
            }
            set
            {
                _parametersDictionary = value;
            }
        }

        private KNN _knn;
        private int K;
       
           
      //  private DecisionTree _tree;
        private DataLearning _data;
        private DecisionMaking _decision;
        private System.Windows.Forms.Timer _timer;

        private FolderBrowserDialog fbd;
        private FolderBrowserDialog Fbd 
        {
            get
            {
                if (fbd == null)
                {
                    fbd = new FolderBrowserDialog();
                }
                return fbd;
            }
        }



        public MainForm()
        {
            InitializeComponent();
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            _timer = new System.Windows.Forms.Timer();
            _decision = new DecisionMaking();
            _btnBrowseAll.Enabled = false;
            _btnBrowseTrue.Enabled = false;
            _btnTrain.Enabled = false;
            _btnPredict.Enabled = false;
            _btnBrowsePredict.Enabled = false;
            _parametersDictionary = new Dictionary<ImageVector.ImageParameters, bool>();
        }


        private void _btnBrowseAll_Click(object sender, EventArgs e)
        {
            
            if (Fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _txtFolderAll.Text = Fbd.SelectedPath;
            }

        }


        private void _btnBrowseTrue_Click(object sender, EventArgs e)
        {
            if (Fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _txtFolderTrue.Text = Fbd.SelectedPath;
            }

        }


        private void _btnTrain_Click(object sender, EventArgs e)
        {
            //set number of all files for timer 
            //       if (NumAllFiles == 0)
            {
                string[] filesAll = Testing.GetFilesFromDirectory(AllFolder).ToArray();
                NumAllFiles = filesAll.Count();
            }
            //create thread for training. 
            Thread trdTrain = new Thread(new ThreadStart(train));
            trdTrain.Start();

            

            _btnTrain.Enabled = false;
            _txtFolderAll.Enabled = false;
            _txtFolderTrue.Enabled = false;
            //create timer - set event every 0.5 seconds
            _timer.Interval = 500;
            _timer.Enabled = true;
            _timer.Start();
            _timer.Tick += new System.EventHandler(OnTimerEventTrain);
        }


        //handles timer event
        public void OnTimerEventTrain(object source, EventArgs e)
        {
            //updates progress bar value


            _progressBarTrain.Value = (int)(Math.Round((_data.Listcount * 100 / (decimal)NumAllFiles), MidpointRounding.AwayFromZero));
            _lblTrainProgress.Text = _progressBarTrain.Value + "%";
            if (_progressBarTrain.Value == 100)
            {
                _btnTrain.Enabled = true;
                _txtFolderAll.Enabled = true;
                _txtFolderTrue.Enabled = true;
                //_timer.Enabled      = false;
                _btnBrowsePredict.Enabled = true;
            }
        }


        //start function of training thread
        private void train()
        {
            if (!_data.Learn(AllFolder, TrueFolder))
            {
                MessageBox.Show("error while learning");
            }
            else
            {
                MessageBox.Show("learning stage finished successfully!");
                if (!(SelectedAlgorithm == LearningAlgorithm.Algorithm.KNN))
                {
                    _data.Algorithm.SaveData(User);
                }// _btnBrowsePredict.Enabled = true;

            }
            // _txtFolderAll.Enabled = true;
            //  _txtFolderTrue.Enabled = true;
        }


        private void _btnBrowsePredict_Click(object sender, EventArgs e)
        {
            if (Fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _txtPredictFolder.Text = Fbd.SelectedPath;
                //_btnPredict.Enabled = true;
            }
        }


        private void _btnPredict_Click(object sender, EventArgs e)
        {
            if (PredictFolder.Equals(""))
            {
                MessageBox.Show("must insert Predict folder");
                return;
            }

            //set number of predict files for timer 
            if (NumPredictFiles == 0)
            {
                string[] files = Testing.GetFilesFromDirectory(PredictFolder).ToArray();
                NumPredictFiles = files.Count();
            }


            //create thread for training. 
            Thread trdPredict = new Thread(new ThreadStart(predict));
            trdPredict.Start();

            /*if (_chkRate.Checked)
            {
                truefold = PredictFolder;
                GradeForm grade = new GradeForm();
                grade.Visible = true;
            }*/

            _btnPredict.Enabled = false;
            _txtPredictFolder.Enabled = false;
            _timer.Interval = 500;
            _timer.Enabled = true;
            _timer.Start();
            _timer.Tick += new System.EventHandler(OnTimerEventPredict);



        }


        public void OnTimerEventPredict(object source, EventArgs e)
        {
            //updates progress bar value
            _progressBarPredict.Value = (int)(Math.Round((_decision.DecisionListcount * 100 / (decimal)NumPredictFiles), MidpointRounding.AwayFromZero));
            _lblPredictProgress.Text = _progressBarPredict.Value + "%";
            if (_progressBarPredict.Value == 100)
            {

                _txtPredictFolder.Enabled = true;
                // _timer.Enabled      = false;
               //*** _btnPredict.Enabled = true;
            }
        }


        //start function of predicting thread
        private void predict()
        {
            //set parameters for decision
            _decision.Algorithm = _data.Algorithm;
            _decision.Name = User;
            _decision.ParameterList = ParametersDictionary;
            /*if (_rdoKNNweight.Checked)
                _decision.weight = true;
            else
                _decision.Weight = false;*/
            
            double[] result= new double[PredictFolder.Length];

            if (!_decision.Decide(PredictFolder, out result))
            {
                MessageBox.Show("failed");
            }
            else
            {
                //create directory for predicted true imgs
                String strDirectory = "C:\\img\\" + User + " imgs";
                Directory.CreateDirectory(strDirectory);
                string[] files = Directory.GetFiles(PredictFolder);
                string name;
                //System.Windows.Forms.MessageBox.Show("got true response22");

                int[] random_rating = new int[result.Length];

                GetRandomRating(result, ref random_rating);

                int place = 1;
                int[] rating = new int[result.Length]; // the ranking
                int[] user_rate = new int[result.Length];
                double[] user_rating = new double[result.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    rating[i] = 0;
                    user_rate[i] = 0;
                    user_rating[i] = 0;
                }
                GetRatingByGrade(result, place, true, ref rating);

                place = 1;

                /*if (!(is_user_rate))
                {
                    GetRatingByGrade(user_rating, place, false, ref user_rate);
                }*/
                if ((_rdoKNN.Checked) || (_rdoKNNweight.Checked))
                    GetRatingByGrade(GradeForm.user_rating, place, false, ref user_rate);
                
                for (int i = 0; i < result.Length; i++)
                {
                    name = files[i].Substring(files[i].LastIndexOf("\\") + 1);// need to take off
                    if (result[i] < 1000)// here decide if true
                    {
                        //name = files[i].Substring(files[i].LastIndexOf("\\") + 1);
                        /*
                         * name of file
                         */
                        string name1= rating[i]+ "_";
                        string name2 = "Place ";
                        if ((_rdoKNN.Checked) || (_rdoKNNweight.Checked))
                            name = name2 + name1 + name;
                        String FileToCopy = Path.Combine(strDirectory, name);
                        File.Copy(files[i], FileToCopy, true);
                        //System.Windows.Forms.MessageBox.Show(result[i].ToString()+i);    
                    }
                    //System.Windows.Forms.MessageBox.Show("picture: " + name + "result: " + result[i].ToString() + "random: " + random_rating[i].ToString());
                    //System.Windows.Forms.MessageBox.Show("picture: " + name + " place: " + rating[i].ToString() + "random: " + random_rating[i].ToString());
                    //System.Windows.Forms.MessageBox.Show("picture: " + name + " place: " + rating[i].ToString()+ " your place: " +user_rate[i]);

                    
                }
                if ((_rdoKNN.Checked) || (_rdoKNNweight.Checked))
                    WriteToHTML(files, strDirectory, result, rating, user_rate, random_rating);

                
                if (DialogResult.Yes == MessageBox.Show("Predict finished successfully. \n Do you wish to open result folder?", "Open result folder?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    System.Diagnostics.Process.Start("explorer.exe", strDirectory);
                }
            }
        }//eof predict




        private void _btnUser_Click(object sender, EventArgs e)
        {
            if (User.Equals("") || _lstParameters.CheckedItems.Count == 0)
                MessageBox.Show("Fill User, Learning Algorithm and Parameter fields");
            else
            {
                //check if user already exists
                Boolean isNewUser = true;
                if (Directory.Exists(User + "'sVectorData"))
                {
                    String[] files = Directory.GetFiles(User + "'sVectorData");
                    //if empty directory
                    if (files.Length != 0)
                        isNewUser = false;
                    else
                    {
                        //check if files (only 2 vector files) have data in them
                        foreach (string file in files)
                        {
                            Stream s = File.Open(file, FileMode.Open);
                            if (s.Length == 0)
                            {
                                isNewUser = true;
                                s.Close();
                                break;
                            }
                            s.Close();
                        }
                    }

                }
                K = 1;
                _data = new DataLearning(User, SelectedAlgorithm, ParametersDictionary, K);
                if (SelectedAlgorithm == LearningAlgorithm.Algorithm.KNN)
                {
                    _knn = new KNN(K, _data.Repository);

                    _btnBrowsePredict.Enabled = true;
                    //_btnPredict.Enabled = true;

                }
                /*if (_rdoKNNweight.Checked)
                    _btnPredictUser.Enabled = true;*/
                //else
                //{
                //    _tree = new DecisionTree();
                //}

                //if user exists: loads repository and algorithm. and enables to train or predict.
                if (!isNewUser)
                {
                    if (!(SelectedAlgorithm == LearningAlgorithm.Algorithm.KNN))
                    {
                        _data.Algorithm.LoadData(User);
                      
                    }
                    _data.Repository.loadList();
                    _btnBrowsePredict.Enabled = true;
                    //_btnPredict.Enabled = true;

                }
                //if doesn't exist: enables only to train. files will be created
                _btnBrowseAll.Enabled = true;
                _btnBrowseTrue.Enabled = true;
                //_btnTrain.Enabled = true;


            }
        }

        private void _btnTest_Click(object sender, EventArgs e)
        {
            TestingForm testing = new TestingForm();
            testing.Enabled = true;
            testing.Visible = true;
        }

      

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void _rdoKNNweight_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void _rdoKNN_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (User.Equals(""))
                MessageBox.Show("Dear User, please enter your name");
            else
            {
                MessageBox.Show("hello " + User + ", now choose your learning algorithm \n and your image processing parameters");
                groupBox1.Enabled = true;
                _btnUser.Enabled = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < _lstParameters.Items.Count; i++)
            {
                //empty or fill parameters in list if All or None is chosen, respectively
                if (checkBox1.Checked)
                    _lstParameters.SetItemChecked(i, true);
                else
                    _lstParameters.SetItemChecked(i, false);
            }
        }

        private void _txtUser_TextChanged(object sender, EventArgs e)
        {

        }


        private void GetRatingByGrade(double[] result,int place, bool is_down, ref int[] rating)
        {


            if (is_down)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    for (int h = 0; h < i; h++)
                    {
                        if (result[h] != 100)
                        {
                            if ((result[i] <= result[h])&& (rating[h]>0))
                            {
                                rating[h]++;
                            }
                            else
                            {
                                place++;
                            }
                        }
                        else
                        {
                            rating[h] = 0;
                        }

                    }
                    rating[i] = place;
                    place = 1;
                }
            }//eof is_down

            else
            {
                for (int i = 0; i < result.Length; i++)
                {
                    for (int h = 0; h < i; h++)
                    {
                        if (result[h] != -1)
                        {
                            if ((result[i] >= result[h]) && (rating[h] > 0))
                            {
                                rating[h]++;
                            }
                            else
                            {
                                place++;
                            }
                        }
                        else
                        {
                            rating[h] = 0;
                        }

                    }
                    rating[i] = place;
                    place = 1;
                }
            }
        }

        private void GetRandomRating(double[] result, ref int[] random_rating)
        {
            Random rnd = new Random();


            int j = 0;
            int count = result.Length + 1;
            for (int i = 0; i < result.Length; i++)
            {
                random_rating[i] = rnd.Next(1, count);
                while (j < i)
                {
                    if (random_rating[i] == random_rating[j])
                    {
                        random_rating[i] = rnd.Next(1, count);
                        j = 0;
                    }
                    else
                        j++;
                }
                j = 0;
            }
        }

        private void WriteToHTML(string[] pic_name, string dir, double[] result, int[] rating, int[] user_rate, int[] random_rating)
       {
           StreamWriter stream = new StreamWriter("C:\\img\\"+ User +" imgs\\result.html");
            stream.WriteLine("<html>");
            stream.WriteLine("<head>");
            stream.WriteLine("<title> Rating result</title>");
            stream.WriteLine("</head>");
            stream.WriteLine("<body text='white'>");

            //create the table

            stream.WriteLine("<table border='2'>");
            stream.WriteLine("<tr>");
            stream.WriteLine("<th bgcolor='black'>Picture name</th>");
            stream.WriteLine("<th bgcolor='black'>Your rating</th>");
            stream.WriteLine("<th bgcolor='black'>Program rating</th>");
            //stream.WriteLine("<th bgcolor='darkred'>Weighted rating</th>");
            stream.WriteLine("<th bgcolor='black'>Random rating</th>");
            stream.WriteLine("</tr>");
            for (int i = 0; i < result.Length; i++)
            {
                stream.WriteLine("<tr>");
                stream.WriteLine("<th bgcolor='darkblue'>" + pic_name[i].Substring(pic_name[i].LastIndexOf("\\") + 1) + "</th>");
                stream.WriteLine("<th bgcolor='blue'>" + user_rate[i] + "</th>");
                stream.WriteLine("<th bgcolor='blue'>" + rating[i] + "</th>");
                //stream.WriteLine("<th bgcolor='darkgreen'>" + rating[i] + "</th>");
                stream.WriteLine("<th bgcolor='blue'>" + random_rating[i] + "</th>");
                stream.WriteLine("</tr>");
            }

            stream.WriteLine("</body>");
            stream.WriteLine("</html>");
            stream.Close();

        }

        private void _btnPredictUser_Click(object sender, EventArgs e)
        {
            truefold = PredictFolder;
            is_user_rate = true;
            GradeForm grade = new GradeForm();
            grade.Visible = true;

            _btnPredict.Enabled = true;
            _btnPredictUser.Enabled = false;
        }

        private void _txtPredictFolder_TextChanged(object sender, EventArgs e)
        {
            if (_rdoTree.Checked)
                _btnPredict.Enabled = true;
            else
                _btnPredictUser.Enabled = true;
            
        }

        private void _progressBarPredict_Click(object sender, EventArgs e)
        {

        }

        private void _txtFolderTrue_TextChanged(object sender, EventArgs e)
        {
            _btnTrain.Enabled = true;
        }

        private void _lstParameters_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


    }
}
