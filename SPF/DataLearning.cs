using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SPF
{
    class DataLearning
    {
        public int Listcount;

        private string _name;
        private LearningAlgorithm _algorithm;

        internal LearningAlgorithm Algorithm
        {
            get { return _algorithm; }
        }
        private VectorRepository _repository;

        internal VectorRepository Repository
        {
            get { return _repository; }
            set { _repository = value; }
        }
        private Dictionary<ImageVector.ImageParameters, bool> _parameterList;

        public DataLearning(string name, LearningAlgorithm.Algorithm algorithm, Dictionary<ImageVector.ImageParameters, bool> parameterList, int K)
        {
            // Set name
            _name = name;

            // Create repository and try to load vectors (if exists)
            _repository = new VectorRepository(_name);
            //   _repository.loadList(); 

            // Create new algorithm instance
            /*  if (algorithm == LearningAlgorithm.Algorithm.DecisionTree)
                  _algorithm = new DecisionTree();
                  _algorithm = new DecisionTreeNumerical();
              if (algorithm == LearningAlgorithm.Algorithm.DecisionTreeNumerical)
                  if (algorithm == LearningAlgorithm.Algorithm.KNN)
                  _algorithm = new KNN(K,_repository);
                         */

            // Set parameter list dictionary
            _parameterList = new Dictionary<ImageVector.ImageParameters, bool>(parameterList);
        }

        public bool Learn(string folderAll, string folderTrue)
        {
            // Get list of files in folders
            string[] filesAll = Testing.GetFilesFromDirectory(folderAll).ToArray();//Directory.GetFiles(folderAll);
            string[] filesTrue = Testing.GetFilesFromDirectory(folderTrue).ToArray(); //Directory.GetFiles(folderTrue);
            if (filesAll.Length <= 0)
                return false;

            // Make False vector list
            int index=0;
            string[] filesFalse = new string[filesAll.Length - filesTrue.Length];
            bool exist;
            string fn_all, fn_true;
            for (int i = 0; i < filesAll.Length; i++)
            {
                fn_all = filesAll[i].Substring(filesAll[i].LastIndexOf("\\")+1);
                exist = false;
                for (int j = 0; j < filesTrue.Length; j++)
                {
                    fn_true = filesTrue[j].Substring(filesTrue[j].LastIndexOf("\\") + 1);
                    if (fn_true.Equals(fn_all))
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist)
                    continue;

                filesFalse[index++] = filesAll[i];
            }

            // Convert to vector List
            List<ImageVector> vectorsTrue = new List<ImageVector>();
            List<ImageVector> vectorsFalse = new List<ImageVector>();
            List<ImageVector> newTrue = new List<ImageVector>();
            List<ImageVector> newFalse = new List<ImageVector>();
            Listcount = 0;
            _repository.loadList();
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\anoach\Desktop\PAPA\csv.txt", true);
            foreach (string path in filesTrue)
            {

                ImageVector existing = _repository.getVectorByPath(path);
                if (existing != null)
                {
                    vectorsTrue.Add(new ImageVector(existing));
                    file.WriteLine(existing.getAllParameters(true));        
                }
                else
                {
                    ImageVector vec = new ImageVector(path, _parameterList);
                    vectorsTrue.Add(new ImageVector(vec));
                    newTrue.Add(vec);
                    file.WriteLine(vec.getAllParameters(true));      

                }
                Listcount++;
            }
            foreach (string path in filesFalse)
            {
                ImageVector existing = _repository.getVectorByPath(path);
                if (existing != null)
                {
                    vectorsFalse.Add(new ImageVector(existing));
                    file.WriteLine(existing.getAllParameters(false));        
                }
                else
                {
                    ImageVector vec = new ImageVector(path, _parameterList);
                    vectorsFalse.Add(new ImageVector(vec));
                    newFalse.Add(vec);
                    file.WriteLine(vec.getAllParameters(false));        
                }
                Listcount++;
            }

            file.Close();
            
            // Push new vectors to repository (Only vectors that was not on the repository)
            _repository.AddToList(newTrue, newFalse);

            // Push vectors to algorithm (Learn by given pictures only)
            bool success = _algorithm.Train(vectorsTrue, vectorsFalse);

            return success;
        }

        /* Learn given pictures. differ from Learn() since learning is according to given pictures only. 
         * Repository is used only for fast retrival of already scanned images */
        public bool LearnForTesting(List<string> filesAll_list, List<string> filesTrue_list)
        {
            // Get list of files in folders         
            string[] filesAll = filesAll_list.ToArray<string>();
            string[] filesTrue = filesTrue_list.ToArray<string>();

            if (filesAll.Length <= 0)
                return false;

            // Make False vector list
            int index = 0;
            string[] filesFalse = new string[filesAll.Length - filesTrue.Length];
            bool exist;
            string fn_all, fn_true;
            for (int i = 0; i < filesAll.Length; i++)
            {
                fn_all = filesAll[i].Substring(filesAll[i].LastIndexOf("\\") + 1);
                exist = false;
                for (int j = 0; j < filesTrue.Length; j++)
                {
                    fn_true = filesTrue[j].Substring(filesTrue[j].LastIndexOf("\\") + 1);
                    if (fn_true.Equals(fn_all))
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist)
                    continue;

                filesFalse[index++] = filesAll[i];
            }

            // Convert to vector List
            List<ImageVector> vectorsTrue = new List<ImageVector>();
            List<ImageVector> vectorsFalse = new List<ImageVector>();
            List<ImageVector> newTrue = new List<ImageVector>();
            List<ImageVector> newFalse = new List<ImageVector>();        
            Listcount = 0;
            _repository.loadList();
            foreach (string path in filesTrue)
            {
                ImageVector existing = _repository.getVectorByPath(path);
                if (existing != null)
                    vectorsTrue.Add(new ImageVector(existing));
                else
                {
                    ImageVector vec = new ImageVector(path, _parameterList); 
                    vectorsTrue.Add(new ImageVector(vec));
                    newTrue.Add(vec);
                }
                Listcount++;
            }
            foreach (string path in filesFalse)
            {
                ImageVector existing = _repository.getVectorByPath(path);
                if (existing != null)
                    vectorsFalse.Add(new ImageVector(existing));
                else
                {
                    ImageVector vec = new ImageVector(path, _parameterList);
                    vectorsFalse.Add(new ImageVector(vec));
                    newFalse.Add(vec);
                }
                Listcount++;
            }

            // Push new vectors to repository (Only vectors that was not on the repository)
            _repository.AddToList(newTrue, newFalse);

            // Push vectors to algorithm (Learn by given pictures only)
            bool success = _algorithm.Train(vectorsTrue, vectorsFalse);

            return success;
        }
    }
}
