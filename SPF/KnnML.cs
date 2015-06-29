using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows.Forms;


namespace SPF
{
    class KnnML : LearningAlgorithmML
    {

        private int k;
        private KNearest knn;

        public KnnML(string path, int k, ImageVector[] goodImages, ImageVector[] badImages)
        {
            this.k = k;

            type = "KNN";

            knn = new KNearest();

            this.goodImages = goodImages;
            this.badImages = badImages;

            learnedTrue = new bool[goodImages.Length];
            learnedFalse = new bool[badImages.Length];

            confidenceTrue = new double[goodImages.Length];
            confidenceFalse = new double[badImages.Length];

            restartTest();

            userPath = path + "\\KNN";
            resultPath = userPath + "\\" + k;
        }

        public override bool Learn(double percent, int[] subsetTrue, int[] subsetFalse)
        {
            // extract subset
            subsetFilesTrue = subsetTrue;
            subsetFilesFalse = subsetFalse;

            initLearned();

            // convert image vectors into Matrix(for Emgu)
            Matrix<float> trainData, response;
            DataConverter.convertDataVectorsToMatrix(goodImages, badImages, subsetFilesTrue, subsetFilesFalse, out trainData, out response, true);
            DataConverter.normalizeMatrix(trainData);

            knn.Train(trainData, response, null, false, k, false);

            return true;
        }

        public override bool Decide()
        {
            Console.WriteLine("KNN - deciding on good images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            if (!Predict(true))
                return false;
            //for (int i = 0; i < goodImages.Length; i++)
            //{
            //    Matrix<float> sample;
            //    DataConverter.convertImageVectorToMatrix(goodImages[i], out sample);
            //    filesTrueResults[i] = Predict(sample);
            //    if (filesTrueResults[i] == Algorithm.FAIL)
            //        return false;
            //}

            Console.WriteLine("KNN - deciding on bad images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            if (!Predict(false))
                return false;
            //for (int i = 0; i < badImages.Length; i++)
            //{
            //    Matrix<float> sample;
            //    DataConverter.convertImageVectorToMatrix(badImages[i], out sample);
            //    filesFalseResults[i] = Predict(sample);
            //    if (filesFalseResults[i] == Algorithm.FAIL)
            //        return false;
            //}

            return true;
        }

        private bool Predict(bool isTrue)
        {
            bool result = true;
            Matrix<float> results;
            if (isTrue)
            {
                results = new Matrix<float>(goodImages.Length, 1);
                Matrix<float> sample;
                DataConverter.convertImageVectorToMatrix(goodImages, out sample);
                DataConverter.normalizeMatrix(sample);

                knn.FindNearest(sample, k, results, null, null, null);
            }
            else
            {
                results = new Matrix<float>(badImages.Length, 1);
                Matrix<float> sample;
                DataConverter.convertImageVectorToMatrix(badImages, out sample);
                DataConverter.normalizeMatrix(sample);

                knn.FindNearest(sample, k, results, null, null, null);
            }
            
            for (int i=0; i<results.Rows; i++)
            {
                if (results.Data[i, 0] == ImageVector.GOOD_IMAGE)
                {
                    if (isTrue)
                        filesTrueResults[i] = Algorithm.GOOD;
                    else
                        filesFalseResults[i] = Algorithm.GOOD;
                }
                else if (results.Data[i, 0] == ImageVector.BAD_IMAGE)
                {
                    if (isTrue)
                        filesTrueResults[i] = Algorithm.BAD;
                    else
                        filesFalseResults[i] = Algorithm.BAD;
                }
                else
                {
                    result = false;
                    if (isTrue)
                        filesTrueResults[i] = Algorithm.FAIL;
                    else
                        filesFalseResults[i] = Algorithm.FAIL;
                }
            }

            return result;
        }

        private Algorithm Predict(Matrix<float> sample)
        {
            Matrix<float> neighborResponses = new Matrix<float>(sample.Rows, k);
            float response = knn.FindNearest(sample, k, null, null, neighborResponses, null);
            // compute the number of neighbors representing the majority
            int count = 0;
            for (int i = 0; i < k; i++)
            {
                if (neighborResponses.Data[0, i] == response)
                    count++;
            }

            if (response == ImageVector.GOOD_IMAGE)
            {
                return Algorithm.GOOD;
            }
            else if (response == ImageVector.BAD_IMAGE)
            {
                return Algorithm.BAD;
            }

            MessageBox.Show(response.ToString());

            return Algorithm.FAIL;
        }

        private void averageResults(out double avgTrueAndSelected, out double avgTrueButNotSelected, out double avgFalseButSelected, out double avgFalseAndNotSelected,
            out double avgTrueAndSelectedNotLearned, out double avgTrueButNotSelectedNotLearned, out double avgFalseButSelectedNotLearned, out double avgFalseAndNotSelectedNotLearned,
            out double avgAccuracy, out double avgRecall, out double avgPrecision, out double avgFScore, out double avgAcc, out double avgMeasure)
        {
            avgTrueAndSelected = (double)trueAndSelected / (double)decisionCount;
            avgTrueButNotSelected = (double)trueButNotSelected / (double)decisionCount;
            avgFalseButSelected = (double)falseButSelected / (double)decisionCount;
            avgFalseAndNotSelected = (double)falseAndNotSelected / (double)decisionCount;

            avgTrueAndSelectedNotLearned = (double)trueAndSelectedNotLearned / (double)decisionCount;
            avgTrueButNotSelectedNotLearned = (double)trueButNotSelectedNotLearned / (double)decisionCount;
            avgFalseButSelectedNotLearned = (double)falseButSelectedNotLearned / (double)decisionCount;
            avgFalseAndNotSelectedNotLearned = (double)falseAndNotSelectedNotLearned / (double)decisionCount;

            avgAccuracy = (double)accuracy / (double)decisionCount;
            avgAcc = (double)acc / (double)decisionCount;
            avgMeasure = (double)goodMeasure / (double)decisionCount;
            avgRecall = (double)recall / (double)decisionCount;
            avgPrecision = (double)precision / (double)decisionCount;
            avgFScore = (double)FScore / (double)decisionCount;
        }

        public override string ToString()
        {
            double avgTrueAndSelected, avgTrueButNotSelected, avgFalseButSelected, avgFalseAndNotSelected, avgAccuracy, avgAcc, avgMeasure, avgRecall,
                avgPrecision, avgFScore, avgTrueAndSelectedNotLearned, avgTrueButNotSelectedNotLearned, avgFalseButSelectedNotLearned,
                avgFalseAndNotSelectedNotLearned;

            averageResults(out avgTrueAndSelected, out avgTrueButNotSelected, out avgFalseButSelected, out avgFalseAndNotSelected,
                out avgTrueAndSelectedNotLearned, out avgTrueButNotSelectedNotLearned, out avgFalseButSelectedNotLearned, out avgFalseAndNotSelectedNotLearned,
                out avgAccuracy, out avgRecall, out avgPrecision, out avgFScore, out avgAcc, out avgMeasure);

            double selectedAlbumSize = avgTrueAndSelected + avgFalseButSelected;
            double selectedAlbumSizeNotLearned = avgTrueAndSelectedNotLearned + avgFalseButSelectedNotLearned;

            string result = avgTrueAndSelected + "," + avgTrueButNotSelected + "," + avgFalseButSelected + "," + avgFalseAndNotSelected + "\n";
            result += avgTrueAndSelectedNotLearned + "\n";
            result += selectedAlbumSize + "," + selectedAlbumSizeNotLearned + "\n";
            result += avgAccuracy + ", " + avgAcc + ", " + avgMeasure + ", " + avgRecall + ", " + avgPrecision + ", " + avgFScore + "\n";
            return result;
        }


    }
}
