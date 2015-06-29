using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;

namespace SPF
{
    static class Testing
    {
        #region Private Members

        private static int _progress;               // Progress percentage indication for running local method
        private static string _progressStr;         // Progress string indication for running local method

        #endregion

        #region Public Members

        public enum HandleIdenticalMethod
        {
            Ignore = 0,
            Remove = 1
        }

        #endregion

        #region Properties

        // Encapsulation for private members
        internal static int Progress
        {
            get { return Testing._progress; }
            set { Testing._progress = value; }
        }                  
        internal static string ProgressString
        {
            get { return Testing._progressStr; }
            set { Testing._progressStr = "Progress: " + value; }
        }

        #endregion

        #region Auxiliary Methods

        /* Returning new list which is the substraction of substract from all. Done by filename only. */
        public static List<string> SubstractListsByFileName(List<string> all, List<string> substract)
        {
            string tmp1, tmp2;
            List<string> ret = new List<string>(all);
            for (int i = 0; i < substract.Count; i++)
                for (int j = 0; j < ret.Count; j++)
                {
                    tmp1 = substract[i].Substring(substract[i].LastIndexOf("\\") + 1);
                    tmp2 = ret[j].Substring(ret[j].LastIndexOf("\\") + 1);
                    if (tmp1.Equals(tmp2))
                    {
                        ret.RemoveAt(j);
                        break;
                    }
                }
            return ret;
        }

        /// <summary>
        /// gets wanted precentage of files from given directory
        /// </summary>
        /// <param name="allFiles">all files of set</param>
        /// <param name="precent">wanted precentage</param>
        /// <returns>list of paths of files</returns>
        private static List<string> getRandomImages(List<string> allFiles, int precent)
        {
            List<string> chosenImages = new List<string>();

            //get wanted precentage of images from all-folder.
            int precentage = (int)(allFiles.Count * precent / 100);
            Random rand = new Random();

            //not so efficient
            while (chosenImages.Count < precentage)
            {
                int temp = rand.Next(allFiles.Count);
                if (!chosenImages.Contains(allFiles.ElementAt(temp)))
                    chosenImages.Add(allFiles.ElementAt(temp));
            }
            return chosenImages;
        }


        #endregion

        #region Testing-Related Methods

        // Scan image to reporistory
        public static bool scanPicturesIntoReporistory(string folderAll, string folderTrue, VectorRepository rep, Dictionary<ImageVector.ImageParameters,bool> parameterList)
        {
            // Get list of files in folders    
            ProgressString = "Starting picture scan..";

            List<string> allFiles, falseFiles, trueFiles;
            try
            {
                allFiles = LoadImages(folderAll).ToList();
                trueFiles = LoadImages(folderTrue).ToList();
                
                falseFiles = SubstractListsByFileName(allFiles, trueFiles);
                trueFiles = SubstractListsByFileName(allFiles, falseFiles);  // In order for the path to be via 'allFiles' folder
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return false;
            }

            int numOfFiles = allFiles.Count();
            int completed = 0;
            _progress = 0;

            // Scan files
            ImageVector existing;
            List<ImageVector> newTrue = new List<ImageVector>();
            List<ImageVector> newFalse = new List<ImageVector>();      
            foreach (string path in trueFiles)
            {
                ProgressString = "Checking if " + path + "is already scanned..";
                existing = rep.getVectorByPath(path);
                if (existing == null && File.Exists(path))
                {
                    ProgressString = "Scanning " + path;
                    ImageVector vec = new ImageVector(path, parameterList);
                    newTrue.Add(vec);
                }
                completed++;
                _progress = (int)((completed * 100) / numOfFiles);
            }
            foreach (string path in falseFiles)
            {
                ProgressString = "Checking if " + path + "is already scanned..";
                existing = rep.getVectorByPath(path);
                if (existing == null && File.Exists(path))
                {
                    ProgressString = "Scanning " + path;
                    ImageVector vec = new ImageVector(path, parameterList);
                    newFalse.Add(vec);
                }
                completed++;
                _progress = (int)((completed * 100) / numOfFiles);
            }
            ProgressString = "Scan done. Saving scanned pictures.";

            // Add to repository
            rep.AddToList(newTrue, newFalse);

            ProgressString = "Done scanning";
            return true;
        }

        /// <summary>
        /// creates learning curve after learning+deciding about given precentage of pictures according to percent[]
        /// </summary>
        /// <param name="folderAll">folder of all images</param>
        /// <param name="folderTrue">folder of true images</param>
        /// <param name="learn">data learning object for learning</param>
        /// <param name="decide">decision making object for deciding</param>
        /// <param name="percent">percentages to use for learning (percent.length is number of points in learning curve)</param>
        /// <returns>true if succeeded creating the curve</returns>
        public static CurvePoint[] LearningCurve(string folderAll, string folderTrue, DataLearning learning, DecisionMaking deciding, int[] percent, out double[] simpleAlgo,
            Testing.HandleIdenticalMethod identical, bool excludeTrainingSet)
        {
            _progress = 0;
            int itrCount=0;
            simpleAlgo = new double[3];
            //randomLearningCurve = null;
            const int ITERATIONS_PER_POINT = 5;

            // Make All, True & False files lists
            List<string> allFiles, falseFiles, trueFiles;
            try
            {
                allFiles = LoadImages(folderAll).ToList();
                trueFiles = LoadImages(folderTrue).ToList();
                falseFiles = SubstractListsByFileName(allFiles, trueFiles);
                trueFiles = SubstractListsByFileName(allFiles, falseFiles);  // In order for the path to be via 'allFiles' folder
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return null;
            }

            // Handle duplicates
            ProgressString = "Checking for identicals with different choise";
            HandleIdenticalVectorWithDifferentChoise(ref allFiles, ref trueFiles, ref falseFiles, learning.Repository, identical);

            //create curve of percent.length number of points
            ProgressString = "Calculating Curves";
            CurvePoint[] learningCurve = new CurvePoint[percent.Length];            //for main graph
            //randomLearningCurve = new CurvePoint[percent.Length];      //for random algorithm graph
            for (int k = 0; k < percent.Length; k++)
            {
                learningCurve[k] = new CurvePoint(percent[k]);
               // randomLearningCurve[k] = new CurvePoint(percent[k]);
                for (int itr = 0; itr < ITERATIONS_PER_POINT; itr++)
                {
                    //get wanted amount of randome images
                    List<string> chosenFiles;
                    do chosenFiles = getRandomImages(allFiles, percent[k]);
                    while (SubstractListsByFileName(chosenFiles, trueFiles).Count == 0 || SubstractListsByFileName(chosenFiles, falseFiles).Count == 0); // incase only true or only false was chosen

                    //get new lists of true and false files (Used for learning in current curve point)
                    List<string> subsetTrueFiles = SubstractListsByFileName(chosenFiles, falseFiles);
                    List<string> subsetFalseFiles = SubstractListsByFileName(chosenFiles, trueFiles);

                    //-------------------------------------
                    //learn from subsets
                    learning.LearnForTesting(chosenFiles, subsetTrueFiles);

                    //-------------------------------------
                    //decide for all files
                    double[] results;
                    deciding.DecideForTesting(allFiles, learning.Repository, out results);

                   
                    //-------------------------------------
                    //make true and false lists according to results
                    List<string> resultsTrue = new List<string>();
                    List<string> resultsFalse = new List<string>();
                    for (int i = 0; i < results.Length; i++)
                        if (results[i]>0) //*******here deside for testing*******/
                            resultsTrue.Add(allFiles[i]);
                        else
                            resultsFalse.Add(allFiles[i]);

                    //-------------------------------------
                    //calculate success precentage
                    CurvePoint.SingleCurvePointCalc currentIteration = new CurvePoint.SingleCurvePointCalc(trueFiles, falseFiles, subsetTrueFiles, subsetFalseFiles, resultsTrue, resultsFalse, excludeTrainingSet);
                    learningCurve[k].AddSingleCurvePointCalc(currentIteration);

                    //-------------------------------------
                    //get simple algorithms calculations
                     double simpleFalseNegative, 
                           simpleFalsePositive,
                           simpleSuccess;
                     Testing.SimpleAlgorithm.CalcStatisticalAlgorithm(allFiles.Count, trueFiles.Count, out simpleSuccess, out simpleFalseNegative, out simpleFalsePositive);
                    simpleAlgo[0] = simpleSuccess*100;
                    simpleAlgo[1] = simpleFalseNegative * 100;
                    simpleAlgo[2] = simpleFalsePositive * 100;

                    // Update progress indicators
                    itrCount++;
                    _progress = (int)((itrCount * 100)/(ITERATIONS_PER_POINT * percent.Length));
                }
            }

            ProgressString = "Done Calculating Curves";
            return learningCurve;
        }

        //private static string[] GetfilesFromDirectory(string folder)
        //{
        //    List<string> files = Directory.GetFiles(folder).ToList();
        //    List<String> removeList = new List<string>();
        //    for (int i = 0; i < files.Count; i++)
        //    {
        //        if(files[i].EndsWith("ini"))
        //            removeList.Add(files[i]);
      
        //    }
        //    foreach (string path in removeList)
        //    {
        //        files.Remove(path);
        //    }
        //    return files.ToArray();
        //}

        private static int HandleIdenticalVectorWithDifferentChoise(ref List<string> aFiles, ref List<string> tFiles, ref List<string> fFiles, VectorRepository rep, Testing.HandleIdenticalMethod identical)
        {
            if (identical == Testing.HandleIdenticalMethod.Ignore)
                return -1;

            ImageVector tVector;
            ImageVector fVector;
            string tVectorC;
            string fVectorC;
            bool same;
            int count = 0;

            for (int t = 0; t < tFiles.Count; t++)
                for (int f = 0; f < fFiles.Count; f++)
                {
                    if (tFiles[t] == "" || fFiles[f] == "")
                        continue;

                    tVector = null;
                    fVector = null;
                    same = true;

                    // Load Images as vectors
                    tVector = rep.getVectorByPath(tFiles[t]);
                    fVector = rep.getVectorByPath(fFiles[f]);
                    if (tVector == null || fVector == null)
                        throw (new Exception("Unexpected Error: a picture was not found in repository"));

                    // Classify
                    tVectorC = Classifier.ClassifyVector(tVector);
                    fVectorC = Classifier.ClassifyVector(fVector);

                    // Check similarity
                    for (int c=0; c < ImageVector.NUMBER_OF_PARAMETERS; c++)
                        if (tVectorC[c] != fVectorC[c])
                        {
                            same = false;
                            break;
                        }

                    // Handle identical vectors (Mark as empty for removel)
                    if (same)
                    {
                        count++;
                        switch (identical)
                        {
                            case Testing.HandleIdenticalMethod.Remove:
                                aFiles.Remove(tFiles[t]);
                                aFiles.Remove(fFiles[f]);
                                tFiles[t] = "";
                                fFiles[f] = "";
                                break;
                        }             
                    }
                }

            // Remove marked
            while (tFiles.Remove(""));
            while (fFiles.Remove(""));

            return count;
        }
        
        /* Writing to file parameters of one picture */
        public static bool WriteSinglePicParametersToFile(string filePath, Dictionary<ImageVector.ImageParameters, bool> dictionary, TextWriter writeTo)
        {
            try
            {
                ImageVector vector = new ImageVector(filePath, dictionary);
                writeTo.WriteLine(filePath + ":");
      

                for (int i = 0; i < ImageVector.NUMBER_OF_PARAMETERS; i++)
                {
                    ImageVector.ImageParameters currentParam = ImageVector.getParameterNameByIndex(i);
                    /*if (dictionary[currentParam])
                        writeTo.WriteLine("* 554422" + currentParam.ToString() + ": " + vector.getParameterByIndex(i).ToString());*/
                }

                return true;
            }
            catch (FileNotFoundException)
            {
                writeTo.WriteLine(filePath + ": File not found");
                return false;
            }
        }

        /* Writing to text file parameters of pictures in a given folder */
        public static bool WritePicturesParametersToFile(string folderPath, string filePath, Dictionary<ImageVector.ImageParameters, bool> dictionary)
        {
            _progress = 0;

            // Get list of files in the folder (assuming all are images)
            string[] files = LoadImages(folderPath);

            int numOfFiles = files.Length;
            int filesProcessed = 0;

            // Open text file for writing
            TextWriter log = new StreamWriter(filePath);
            //log.WriteLine("554422");

            // Write each image parameters to file
            foreach (string file in files)
            {
                WriteSinglePicParametersToFile(file, dictionary, log);
                log.WriteLine("");
                filesProcessed++;
                _progress = (filesProcessed * 100) / numOfFiles;
            }
            log.Close();

            _progress = 0;
            return true;
        }

        /* Writing to XML parameters of pictures in a given folder */
        public static bool WritePicturesParametersToXML(string folderPath, string filePath, Dictionary<ImageVector.ImageParameters, bool> dictionary)
        {
            ImageVector vector;
            _progress = 0;

            // Get list of files in the folder (assuming all are images)
            string[] files = LoadImages(folderPath);

            int numOfFiles = files.Length;
            int filesProcessed = 0;

            // Create an XmlWriterSettings object with the correct options. 
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.OmitXmlDeclaration = true;

            // Open text file for writing
            XmlWriter xml = XmlWriter.Create(filePath, settings);

            xml.WriteStartDocument(true);

            string folderTitle = folderPath.Substring(folderPath.LastIndexOf('\\') + 1);
            xml.WriteStartElement(folderTitle);

            // Write each image parameters to file
            foreach (string file in files)
            {
                // Convert pic to vector
                vector = new ImageVector(file, dictionary);

                // Write XML
                string title = file.Substring(file.LastIndexOf('\\') + 1);
                xml.WriteStartElement(title);

                for (int i = 0; i < ImageVector.NUMBER_OF_PARAMETERS; i++)
                {
                    ImageVector.ImageParameters currentParam = ImageVector.getParameterNameByIndex(i);
                    if (dictionary[currentParam])
                        xml.WriteElementString(currentParam.ToString(), vector.getParameterByIndex(i).ToString());
                }

                xml.WriteEndElement();

                filesProcessed++;
                _progress = (filesProcessed * 100) / numOfFiles;
            }
            xml.WriteEndElement();
            xml.WriteEndDocument();
            xml.Close();

            _progress = 0;

            return true;
        }

        /* Writing log file for given learning curve object */
        public static void WriteLearningCurveToLog(Testing.CurvePoint[] lc, string filename)
        {
            int itrN;
            StreamWriter file = new StreamWriter(filename);
            file.WriteLine("<?xml version=\"1.0\"?>");
            file.WriteLine("<LearningCurve>");
            for (int p = 0; p < lc.Length; p++)
            {
                file.WriteLine("\t<CurvePoint>");
                file.WriteLine("\t\t<TrainingSetPercentage> " + lc[p].getTrainingSetPercentage().ToString() + " </TrainingSetPercentage>");
                file.WriteLine("\t\t<AveragePercentageCorrect> " + lc[p].getAvgPercentageCorrect().ToString() + " </AveragePercentageCorrect>");
                file.WriteLine("\t\t<AveragePercentageFalseNegative> " + lc[p].getAvgPercentageFalseNegative().ToString() + " </AveragePercentageFalseNegative>");
                file.WriteLine("\t\t<AveragePercentageFalsePositive> " + lc[p].getAvgPercentageFalsePositive().ToString() + " </AveragePercentageFalsePositive>");
                file.WriteLine("\t\t<StandardDeviationCorrect> " + lc[p].getStdPercentageCorrect().ToString() + " </StandardDeviationCorrect>");
                file.WriteLine("\t\t<StandardDeviationFalseNegative> " + lc[p].getStdPercentageFalseNegative().ToString() + " </StandardDeviationFalseNegative>");
                file.WriteLine("\t\t<StandardDeviationFalsePositive> " + lc[p].getStdPercentageFalsePositive().ToString() + " </StandardDeviationFalsePositive>");
                file.WriteLine("\t\t<Iterations>");
                itrN = 0;
                foreach (Testing.CurvePoint.SingleCurvePointCalc itr in lc[p].getPointIterations())
                {
                    List<string> at, af, tt, tf, rt, rf;
                    itr.getWholeFileLists(out at, out af);
                    itr.getTrainingSetFileLists(out tt, out tf);
                    itr.getResultsFileLists(out rt, out rf);
                    file.WriteLine("\t\t\t<Iteration " + (++itrN).ToString() + ">");
                    file.WriteLine("\t\t\t\t<PercentageCorrect> " + itr.getPercentageCorrect().ToString() + " </AveragePercentageCorrect>");
                    file.WriteLine("\t\t\t\t<PercentageFalseNegative> " + itr.getPercentageFalseNegative().ToString() + " </AveragePercentageFalseNegative>");
                    file.WriteLine("\t\t\t\t<PercentageFalsePositive> " + itr.getPercentageFalsePositive().ToString() + " </AveragePercentageFalsePositive>");
                    file.WriteLine("\t\t\t\t<ActualLists>");
                    file.WriteLine("\t\t\t\t\t<True>");
                    foreach (string str in at)
                        file.WriteLine("\t\t\t\t\t\t<File>" + str + "</File>");
                    file.WriteLine("\t\t\t\t\t</True>");
                    file.WriteLine("\t\t\t\t\t<False>");
                    foreach (string str in af)
                        file.WriteLine("\t\t\t\t\t\t<File>" + str + "</File>");
                    file.WriteLine("\t\t\t\t\t</False>");
                    file.WriteLine("\t\t\t\t</ActualLists>");
                    file.WriteLine("\t\t\t\t<TrainitSet>");
                    file.WriteLine("\t\t\t\t\t<True>");
                    foreach (string str in tt)
                        file.WriteLine("\t\t\t\t\t\t<File>" + str + "</File>");
                    file.WriteLine("\t\t\t\t\t</True>");
                    file.WriteLine("\t\t\t\t\t<False>");
                    foreach (string str in tf)
                        file.WriteLine("\t\t\t\t\t\t<File>" + str + "</File>");
                    file.WriteLine("\t\t\t\t\t</False>");
                    file.WriteLine("\t\t\t\t</TrainitSet>");
                    file.WriteLine("\t\t\t\t<Results>");
                    file.WriteLine("\t\t\t\t\t<True>");
                    foreach (string str in rt)
                        file.WriteLine("\t\t\t\t\t\t<File>" + str + "</File>");
                    file.WriteLine("\t\t\t\t\t</True>");
                    file.WriteLine("\t\t\t\t\t<False>");
                    foreach (string str in rf)
                        file.WriteLine("\t\t\t\t\t\t<File>" + str + "</File>");
                    file.WriteLine("\t\t\t\t\t</False>");
                    file.WriteLine("\t\t\t\t</Results>");
                    file.WriteLine("\t\t\t</Iteration>");
                }
                file.WriteLine("\t\t</Iterations>");
                file.WriteLine("\t</CurvePoint>");
            }
            file.WriteLine("</LearningCurve>");
            file.Close();
        }

        /* Making classification graphs for given photos */
        public static GraphForm.GraphPlot makeGraph(string folderTruePath, string folderFalsePath, int parX, int parY, Color cTrue, Color cFalse)
        {
            _progress = 0;

            // Get list of files in the folderer (assuming all are images)
            string[] filesTrue = LoadImages(folderTruePath);
            string[] filesFalse = LoadImages(folderFalsePath);
            int numOfFiles = filesTrue.Length + filesFalse.Length;
            int filesProcessed = 0;

            // Build dictionary for wanted parameters
            Dictionary<ImageVector.ImageParameters, bool> parameters = new Dictionary<ImageVector.ImageParameters, bool>();
            parameters.Add(ImageVector.getParameterNameByIndex(parX), true);
            parameters.Add(ImageVector.getParameterNameByIndex(parY), true);
            for (int i = 0; i < ImageVector.NUMBER_OF_PARAMETERS; i++)
                if (!parameters.ContainsKey(ImageVector.getParameterNameByIndex(i)))
                    parameters.Add(ImageVector.getParameterNameByIndex(i), false);

            // Convert images to vectors
            List<ImageVector> trueList = new List<ImageVector>();
            List<ImageVector> falseList = new List<ImageVector>();
            foreach (string picPath in filesTrue)
            {
                trueList.Add(new ImageVector(picPath, parameters));
                filesProcessed++;
                Progress = (int)((filesProcessed / numOfFiles) * 100);
            }
            foreach (string picPath in filesFalse)
            {
                falseList.Add(new ImageVector(picPath, parameters));
                filesProcessed++;
                Progress = (int)((filesProcessed / numOfFiles) * 100);
            }

            // Construct graph
            string par1name = ImageVector.getParameterNameByIndex(parX).ToString();
            string par2name = ImageVector.getParameterNameByIndex(parY).ToString();
            GraphForm.GraphPlot graph = new GraphForm.GraphPlot(par1name, par2name);

            // Add points to graph
            double valueX, valueY;
            for (int i = 0; i < trueList.Count; i++)
            {
                valueX = trueList[i].getParameterByIndex(parX);
                valueY = trueList[i].getParameterByIndex(parY);
                graph.addPoint(valueX, valueY, i.ToString(), cTrue);
            }
            for (int i = 0; i < falseList.Count; i++)
            {
                valueX = falseList[i].getParameterByIndex(parX);
                valueY = falseList[i].getParameterByIndex(parY);
                graph.addPoint(valueX, valueY, i.ToString(), cFalse);
            }

            // Get classifications from classifier class
            double[] boundsX = new double[0];
            double[] boundsY = new double[0];
            Classifier.getBoundArray(ImageVector.getParameterNameByIndex(parX), ref boundsX);
            Classifier.getBoundArray(ImageVector.getParameterNameByIndex(parY), ref boundsY);

            // Convert bounds array to list object and create classification representing char lists
            List<double> bX = new List<double>();
            List<double> bY = new List<double>();
            List<string> bXnames = new List<string>();
            List<string> bYnames = new List<string>();
            int chr_a = 97;
            for (int i = 0; i < boundsX.Length; i++)
            {
                bX.Add(boundsX[i]);
                bXnames.Add(Convert.ToChar(chr_a + i).ToString());
            }
            bXnames.Add(Convert.ToChar(chr_a + boundsX.Length).ToString());
            for (int i = 0; i < boundsY.Length; i++)
            {
                bY.Add(boundsY[i]);
                bYnames.Add(Convert.ToChar(chr_a + i).ToString());
            }
            bYnames.Add(Convert.ToChar(chr_a + boundsY.Length).ToString());

            // Add classification to graph
            graph.addXClassification(bX, bXnames);
            graph.addYClassification(bY, bYnames);

            return graph;
        }

        #endregion

        #region Auxiliary Classes

        /* Represents a point in a learning curve. */
        public class CurvePoint
        {
            private double _TrainingSetPercentage;
            private double _AvgPercentageCorrect;
            private double _AvgPercentageFalsePositive;
            private double _AvgPercentageFalseNegative;
            private double _StdPercentageCorrect;
            private double _StdPercentageFalsePositive;
            private double _StdPercentageFalseNegative;
            private List<SingleCurvePointCalc> _PointCalcs;

            public CurvePoint(int TrainingSetPercentage)
            {
                _TrainingSetPercentage = TrainingSetPercentage;
                _PointCalcs = new List<SingleCurvePointCalc>();
            }

            public List<SingleCurvePointCalc> getPointIterations()
            {
                return _PointCalcs;
            }

            public void AddSingleCurvePointCalc(SingleCurvePointCalc pointCalc)
            {
                _PointCalcs.Add(pointCalc);
                CalcPercentages();
            }

            private void CalcPercentages()
            {
                _AvgPercentageCorrect = 0;
                _AvgPercentageFalsePositive = 0;
                _AvgPercentageFalseNegative = 0;
                _StdPercentageCorrect = 0;
                _StdPercentageFalsePositive = 0;
                _StdPercentageFalseNegative = 0;

                // Calc Average
                foreach (SingleCurvePointCalc p in _PointCalcs)
                {
                    _AvgPercentageCorrect += p.getPercentageCorrect();
                    _AvgPercentageFalsePositive += p.getPercentageFalsePositive();
                    _AvgPercentageFalseNegative += p.getPercentageFalseNegative();
                }
                _AvgPercentageCorrect /= _PointCalcs.Count;
                _AvgPercentageFalsePositive /= _PointCalcs.Count;
                _AvgPercentageFalseNegative /= _PointCalcs.Count;
                
                // Calc Standard Deviation
                foreach (SingleCurvePointCalc p in _PointCalcs)
                {
                    _StdPercentageCorrect += Math.Pow(p.getPercentageCorrect() - _AvgPercentageCorrect, 2);
                    _StdPercentageFalsePositive += Math.Pow(p.getPercentageFalsePositive() - _AvgPercentageFalsePositive, 2);
                    _StdPercentageFalseNegative += Math.Pow(p.getPercentageFalseNegative() - _AvgPercentageFalseNegative, 2);
                }
                _StdPercentageCorrect = Math.Sqrt(_StdPercentageCorrect / _PointCalcs.Count);
                _StdPercentageFalsePositive = Math.Sqrt(_StdPercentageFalsePositive / _PointCalcs.Count);
                _StdPercentageFalseNegative = Math.Sqrt(_StdPercentageFalseNegative / _PointCalcs.Count);
            }

            public double getTrainingSetPercentage()
            {
                return _TrainingSetPercentage;
            }

            public double getAvgPercentageCorrect()
            {
                return _AvgPercentageCorrect;
            }

            public double getAvgPercentageFalsePositive()
            {
                return _AvgPercentageFalsePositive;
            }

            public double getAvgPercentageFalseNegative()
            {
                return _AvgPercentageFalseNegative;
            }

            public double getStdPercentageCorrect()
            {
                return _StdPercentageCorrect;
            }

            public double getStdPercentageFalsePositive()
            {
                return _StdPercentageFalsePositive;
            }

            public double getStdPercentageFalseNegative()
            {
                return _StdPercentageFalseNegative;
            }

            public class SingleCurvePointCalc
            {
                private double _PercentageCorrect;
                private double _PercentageFalsePositive;
                private double _PercentageFalseNegative;

                private List<string> _AllTrue;
                private List<string> _AllFalse;
                private List<string> _TrainingSetTrue;
                private List<string> _TrainingSetFalse;
                private List<string> _ResultsTrue;
                private List<string> _ResultsFalse;
                private bool _excludeTrainingSet;

                public SingleCurvePointCalc(List<string> all_true, List<string> all_false, 
                                            List<string> train_true, List<string> train_false, 
                                            List<string> res_true, List<string> res_false,
                                            bool excludeTrainingSet)
                {
                    _AllTrue = all_true;
                    _AllFalse = all_false;
                    _TrainingSetTrue = train_true;
                    _TrainingSetFalse = train_false;
                    _ResultsTrue = res_true;
                    _ResultsFalse = res_false;
                    _excludeTrainingSet = excludeTrainingSet;

                    if (!_excludeTrainingSet)
                        CalcPercentages();
                    else
                        CalcPercentagesExcludingTrainingSet();
                }

                private bool CalcPercentages()
                {
                    int falsePosCount = 0, falseNegCount = 0;

                    foreach (string pos in _ResultsTrue)
                    {
                        //counts false positives
                        if (!_AllTrue.Contains(pos))
                            falsePosCount++;       
                    }
                    foreach (string pos in _ResultsFalse)
                    {
                        //counts false negative
                        if (!_AllFalse.Contains(pos))
                            falseNegCount++;       
                    }

                    int totalNumOfFiles = _AllTrue.Count + _AllFalse.Count;

                    _PercentageFalsePositive = ((double)falsePosCount * 100) / _ResultsTrue.Count;
                    _PercentageFalseNegative = ((double)falseNegCount * 100) / _ResultsFalse.Count;
                    _PercentageCorrect = (((double)(totalNumOfFiles - falsePosCount - falseNegCount) * 100) / totalNumOfFiles);
                    
                    return true;
                }

                private bool CalcPercentagesExcludingTrainingSet()
                {
                    int falsePosCount = 0, falseNegCount = 0;

                    foreach (string pos in _ResultsTrue)
                    {
                        //counts false positives
                        if ((!isInTrainingSet(pos)) && (!_AllTrue.Contains(pos)))
                            falsePosCount++;
                    }
                    foreach (string neg in _ResultsFalse)
                    {
                        //counts false negative
                        if ((!isInTrainingSet(neg)) && (!_AllFalse.Contains(neg)))
                            falseNegCount++;
                    }

                    int totalNumOfFiles = _AllTrue.Count + _AllFalse.Count;
                    int trainingSetCount = _TrainingSetFalse.Count + _TrainingSetTrue.Count;
                    _PercentageFalsePositive = ((double)falsePosCount * 100) / (_AllTrue.Count - _TrainingSetTrue.Count);
                    _PercentageFalseNegative = ((double)falseNegCount * 100) / (_AllFalse.Count - _TrainingSetFalse.Count);
                    _PercentageCorrect = (((double)(totalNumOfFiles - falsePosCount - falseNegCount - trainingSetCount) * 100) / (totalNumOfFiles - trainingSetCount));

                    return true;
                }

                public bool isInTrainingSet(string path)
                {
                    if (_TrainingSetTrue.Contains(path))
                        return true;
                    if (_TrainingSetFalse.Contains(path))
                        return true;
                    return false;
                }

                public double getPercentageCorrect()
                {
                    return _PercentageCorrect;
                }
                public double getPercentageFalsePositive()
                {
                    return _PercentageFalsePositive;
                }
                public double getPercentageFalseNegative()
                {
                    return _PercentageFalseNegative;
                }

                public void getWholeFileLists(out List<string> trueList, out List<string> falseList)
                {
                    trueList = _AllTrue;
                    falseList = _AllFalse;
                }

                public void getTrainingSetFileLists(out List<string> trueList, out List<string> falseList)
                {
                    trueList = _TrainingSetTrue;
                    falseList = _TrainingSetFalse;
                }

                public void getResultsFileLists(out List<string> trueList, out List<string> falseList)
                {
                    trueList = _ResultsTrue;
                    falseList = _ResultsFalse;
                }
                 
            }
        }

        /* Represents simple algorithm calculations. */
        public static class SimpleAlgorithm
        {
            //statistical analisys of simple algorithm
            public static void CalcStatisticalAlgorithm(int allListCount, int trueListCount, out double success, out double falseNegative, out double falsePositive)
            {
                double trueProbability, falseListCount;
                trueProbability = ((double)trueListCount / allListCount);
                falseListCount = allListCount - trueListCount;

                falsePositive = trueProbability;
                falseNegative = 1 - trueProbability;
                success = ((trueProbability*trueListCount) + (1-trueProbability)*falseListCount)/allListCount;
            }

        }
        #endregion

        /// <summary>
        /// All images pathes from images folder
        /// </summary>
        /// <param name="absoluteAddres">true for loading fiie system path, false for loading relative url</param>
        /// <returns></returns>

        internal static String[] LoadImages(String picturesLocation)
        {

            List<String> arrPicsUrl = new List<String>();
            arrPicsUrl.AddRange(GetFilesFromDirectory(picturesLocation).ToArray());
            
                return arrPicsUrl.ToArray();         


        }

        static readonly List<String> IMAGE_EXTENSIONS = new List<string>() { "jpg", "bmp", "png" };
        /// <summary>
        /// Gets all images from a directory (non recursive)
        /// </summary>
        /// <param name="directory">the directory that contains images</param>
        /// <returns>list with the pathes of images</returns>
        internal static List<String> GetFilesFromDirectory(String directory)
        {
            List<String> arrRet = new List<string>();
            arrRet.AddRange(Directory.GetFiles(directory));
            for (int i = arrRet.Count - 1; i >= 0; i--)
            {

                String currFile = arrRet[i];
                int extIdx = currFile.LastIndexOf(".");
                String currentExtention = currFile.Substring(extIdx + 1,currFile.Length -(extIdx + 1));

                if (extIdx == -1 || !IMAGE_EXTENSIONS.Contains(currentExtention.ToLower()))
                {
                    arrRet.RemoveAt(i);
                    continue;
                }
            }
            return arrRet;
        }
    }
}
