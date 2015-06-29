using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;
using Emgu.CV;
using Emgu.CV.UI;
using System.IO;


namespace SPF
{
    class KNN : LearningAlgorithm
    {

        private int _K;
        private Matrix<float> _data,            //matrix that keeps training set 
                              _response;        //matrix that keeps classification of training set vectors
        // private ImageViewer _viewer;
        KNearest _knn;
        private VectorRepository _repository, _testingRepository;

        public KNN(int wantedK, VectorRepository rep)
        {
            //k is usually 10
            _K = wantedK;
            _knn = new KNearest();
            _repository = rep;
        }

        /// <summary>
        /// trains algorithm
        /// </summary>
        /// <param name="cVectorsTrue"></param>
        /// <param name="cVectorsFalse"></param>
        /// <returns>true always</returns>
        public override bool Train(List<ImageVector> vectorsTrue, List<ImageVector> vectorsFalse)
        {
           //delete the test repository if it exists
            if(Directory.Exists("knnTest'sVectorData"))
                Directory.Delete("knnTest'sVectorData", true);

           
             //create repository only for knn learning
            _testingRepository = new VectorRepository("knnTest");
            _testingRepository.delete();
            _testingRepository.AddToList(vectorsTrue, vectorsFalse);
            
            return true;
        }


        public override bool Predict(List<ImageVector> Vectors, out double[] results)
        {
            Boolean forTesting = true;
            string[] cVectors = new string[Vectors.Count];
            for (int i = 0; i < Vectors.Count; i++)
                cVectors[i] = Classifier.ClassifyVector(Vectors[i]);

            Image<Bgr, Byte> img = new Image<Bgr, byte>(500, 500);
            Matrix<float> sample;
            Matrix<float> res, neighborResponses;
            Matrix<float> kNearestNeighbors = new Matrix<float>(_K,cVectors.Length); ;
            results = new double[cVectors.Length];
            res = new Matrix<float>(1,1);
            neighborResponses = new Matrix<float>(1, _K);
            //dist = new Matrix<float>(1, _K);
         

          
           
                //load knn repository
             if (forTesting)
                 _testingRepository.loadList();
             
             else
                _repository.loadList();
            

            //calculate proportion of false vs. true in repository
            int f,t;
            if (forTesting)
            {
                f = _testingRepository.VectorListFalse.Count;
                t = _testingRepository.VectorListTrue.Count;
            }
            else
            {
                f = _repository.VectorListFalse.Count;
                t = _repository.VectorListTrue.Count;
            }
                double proportion = t / f; //TODO:use this


            string[] cVectorsTrue, cVectorsFalse;
            if(forTesting)
                Classifier.Classify(_testingRepository.VectorListTrue, _testingRepository.VectorListFalse, out cVectorsTrue, out cVectorsFalse);
            else
                Classifier.Classify(_repository.VectorListTrue, _repository.VectorListFalse, out cVectorsTrue, out cVectorsFalse);

                // Push vectors to algorithm
               
                convertDataVectorsToMatrix(cVectorsTrue, cVectorsFalse, out _data, out _response);
                using (_knn = new KNearest(_data, _response, null, false, _K))
                {
                    //   }
                    int tr=0;

                    for (int i = 0; i < cVectors.Length; i++)
                    {
                        // Convert vector i to matrix
                        convertSampleToMatrix(cVectors[i], out sample);

                        //Matrix<float> nearestNeighbors = new Matrix<float>(K* sample.Rows, sample.Cols);

                        // estimates the response and get the neighbors' labels
                        try
                        {
                            float response = _knn.FindNearest(sample, _K, res, null, neighborResponses, null);
                            if (response == 1)
                            {
                                //System.Windows.Forms.MessageBox.Show("distance " + dist.ToString());
                                tr++;
                                
                            }
                            double accuracy = 0;//grade of picture
                            double distance = 0;
                            //double power = 0;
                            //double good = 0;
                            // compute the number of neighbors representing the majority


  

                            int place = 0;
                            for (int k = 0; k < _K; k++)
                            {
                                if (neighborResponses.Data[0, k] == response)
                                    accuracy++;
                            }
                            if ((accuracy >= _K / 2))
                            {
                                if (MainForm.weight == true)
                                {
                                    distance = find_weight_distance(cVectors[place], sample, _data);
                                    place++;
                                }
                                else
                                {
                                    distance = find_distance(cVectors[place], sample, _data);
                                    place++;
                                }
                            }
                            else
                            {
                                distance = 1000;
                                place++;
                            }
                            results[i] = distance;
                            accuracy = 0;
                        

                        }
                        catch (Exception e)
                        {
                            System.Windows.Forms.MessageBox.Show(e.Message);        
                        }
                    }
                    //System.Windows.Forms.MessageBox.Show(tr+" true images found");
                }
                
            return true;
        }


        public override bool SaveData(string name)
        {
            try
            {
                _knn.Save("knn-" + name);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public override bool LoadData(string name)
        {
            try
            {
                _knn.Load("knn-" + name);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private void convertDataVectorsToMatrix(string[] strTrue, string[] strFalse, out Matrix<float> mat, out Matrix<float> response)
        {
            int propCount = strTrue[0].Length;
            int trueVectorCount = strTrue.Length;
            int falseVectorCount = strFalse.Length;
            int vectorCount = trueVectorCount + falseVectorCount;
            mat = new Matrix<float>(vectorCount, propCount);
            response = new Matrix<float>(vectorCount, 1);

            for (int v = 0; v < trueVectorCount; v++)
                for (int p = 0; p < propCount; p++)
                {
                    mat[v, p] = (byte)strTrue[v][p];
                    response[v, 0] = System.Convert.ToInt32(1);
                }
            for (int v = trueVectorCount; v < vectorCount; v++)
                for (int p = 0; p < propCount; p++)
                {
                    mat[v, p] = (byte)strFalse[v - trueVectorCount][p];
                    response[v, 0] = System.Convert.ToInt32(0);
                }
        }


        private void convertSampleToMatrix(String str, out Matrix<float> samples)
        {
            int propCount = str.Length;

            samples = new Matrix<float>(1, propCount);

            for (int p = 0; p < propCount; p++)
                samples[0, p] = (byte)str[p];
        }

        private double find_distance(String str, Matrix<float> sample, Matrix<float> data)
        {
            double result = 0;
          
            double sample_case = 0;
            double data_case = 0;
            int propCount = str.Length;

            for (int i = 0; i < propCount; i++)
            {
                sample_case += sample.Data[0, i];
                data_case += data.Data[0, i];
            }

            sample_case /= propCount;
            data_case /= propCount;

            result = Math.Pow((sample_case - data_case), 2);

            //System.Windows.Forms.MessageBox.Show("sample case: " + sample_case.ToString() +"\n data case: " + data_case.ToString());


            return Math.Sqrt(result);
        }

        public double find_weight_distance(String str, Matrix<float> sample, Matrix<float> data)
        {
            double result = 0;
  
            double sample_case = 0;
            double data_case = 0;
            int propCount = str.Length;
            for (int i = 0; i < propCount; i++)
            {
                sample_case += sample.Data[0, i];
                if (Classifier.order_remainder[i] > 0 && Classifier.order_remainder[i] < 1)
                    sample_case *= Classifier.order_remainder[i];
                data_case += data.Data[0, i];
                if (Classifier.order_remainder[i] > 0 && Classifier.order_remainder[i] < 1)
                    data_case *= Classifier.order_remainder[i];
            }

            sample_case /= propCount;
            data_case /= propCount;

            result = Math.Pow((sample_case - data_case), 2);

            //System.Windows.Forms.MessageBox.Show("sample case: " + sample_case.ToString() +"\n data case: " + data_case.ToString());


            return Math.Sqrt(result);
        }
        

    }
}
