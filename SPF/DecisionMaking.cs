using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace SPF
{
    class DecisionMaking
    {
        public int DecisionListcount;

        private LearningAlgorithm _algorithm;

        internal LearningAlgorithm Algorithm
        {
            get { return _algorithm; }
            set { _algorithm = value; }
        }
        private Dictionary<ImageVector.ImageParameters, bool> _parameterList;

        internal Dictionary<ImageVector.ImageParameters, bool> ParameterList
        {
            get { return _parameterList; }
            set { _parameterList = value; }
        }
        private String _name;

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private bool is_weight;

        public bool Weight
        {
            get { return is_weight; }
            set { is_weight = value; }
        }

        public DecisionMaking()
        {
        }


        public DecisionMaking(string name, LearningAlgorithm algorithm, Dictionary<ImageVector.ImageParameters, bool> parameterList)
        {
            // Set name
            Name = name;

            //connect to already existing algorithm instance
            Algorithm = algorithm; 

            //Set parameter list dictionary
            ParameterList = new Dictionary<ImageVector.ImageParameters, bool>(parameterList);

            //Weight = weight; 
        }


        /// <summary>
        /// predicts for all images in folder if true or false
        /// </summary>
        /// <param name="folder">path to folder with images to predict</param>
        /// <param name="res">keeps for each image if true or falde</param>
        /// <returns>true if decision was successful. else false.</returns>
        public bool Decide(string folder, out double [] res)
        {
            res = null;
            // Get list of files in folders
            String[] files;
            try
            {
                files = Testing.GetFilesFromDirectory(folder).ToArray();//Directory.GetFiles(folder);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return false;
            }

            DecisionListcount = 0;
            String[] cVectors= new string[files.Length];
            List<ImageVector> vectors = new List<ImageVector>();
            for(int i = 0; i < files.Length; i++)
            {
                string path = "C:\\Users\\anoach\\Desktop\\PAPA\\csv"+i+".txt";
                System.IO.StreamWriter file = new System.IO.StreamWriter(@path, true);
                //get imageVectors
                ImageVector vec = new ImageVector(files[i], ParameterList);
                
                vectors.Add(new ImageVector(vec));

                //Convert to clasiffied vector
                DecisionListcount++;
                file.WriteLine(vec.getAllParameters(false));
                file.Close();
            }

            double[] results;
            //load algorithm data and predict
            //if (!_algorithm.LoadData())
            //{
            //    return false;
            //}

            if (!Algorithm.Predict(vectors, out results))
                return false;

            res = results;
            return true;
        }

        /// <summary>
        /// predicts for all images in folder if true or false (For testing, using given repository to retrive already scanned images)
        /// </summary>
        /// <param name="folder">path to folder with images to predict</param>
        /// <param name="res">keeps for each image if true or falde</param>
        /// <returns>true if decision was successful. else false.</returns>
        public bool DecideForTesting(List<string> allFiles, VectorRepository rep, out double[] res)
        {
            res = null;
            // Get list of files in folders
            String[] files = allFiles.ToArray();
            DecisionListcount = 0;
            String[] cVectors = new string[files.Length];
            double[] results;
            ImageVector vector;
            List<ImageVector> vectorList = new List<ImageVector>();

            

            for (int i = 0; i < files.Length; i++)
            {
                //get imageVectors
                vector = rep.getVectorByPath(files[i]);
                if (vector == null)
                    vector = new ImageVector(files[i], ParameterList);

                vectorList.Add(vector);
                DecisionListcount++;
            }


            //load algorithm data and predict
            //if (!_algorithm.LoadData())
            //{
            //    return false;
            //}

            if (!Algorithm.Predict(vectorList, out results))
                return false;

            res = results;
            return true;
        }
    }
}
