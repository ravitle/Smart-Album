using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SPF
{
    class KNN_Matlab : LearningAlgorithmML
    {

        private string decide, cd;
        private string decide_Test;
        //private double runConf;

        public KNN_Matlab(string path, int k, ImageVector[] goodImages, ImageVector[] badImages)
        {
            type = "KNN";

            this.goodImages = goodImages;
            this.badImages = badImages;


            learnedTrue = new bool[goodImages.Length];
            learnedFalse = new bool[badImages.Length];

            restartTest();

            userPath = path + "\\KNN";
            resultPath = userPath + "\\" + k;
            
            decide = GetCommand(userPath, k);
            decide_Test = GetCommand_Test(userPath, k, goodImages[0].getNumOfParameters());
            matlab = new MLApp.MLApp();
            cd = "cd " + smartAlbum.getMatlabDirectory();
            matlab.Execute(cd);
        }

        public override bool Learn(double percent, int[] subsetTrue, int[] subsetFalse)
        {
            // extract subset
            subsetFilesTrue = subsetTrue;
            subsetFilesFalse = subsetFalse;

            initLearned();

            // convert image vectors into files(for matlab)
            DataConverter.convertDataVectorsToFile(userPath, goodImages, badImages, subsetFilesTrue, subsetFilesFalse, true);

            /*
            string learn = "albumLearnSVM";
            string kernelF = "rbf";
            int sigma = 1, gamma = 1;
            learn += "('" + userPath + "', " + "'" + kernelF + "', " + sigma + ", " + gamma + ");";
            string result = matlab.Execute(learn);
            */
            
            return true;
        }

        public override bool Learn_Test(double percent, int[] subsetTrue, int[] subsetFalse)
        {
            // extract subset
            subsetFilesTrue = subsetTrue;
            subsetFilesFalse = subsetFalse;

            initLearned();

            // convert image vectors into files(for matlab)
            DataConverter.convertDataVectorsToFile_Test(userPath, goodImages, badImages, subsetFilesTrue, subsetFilesFalse, true);

            /*
            string learn = "albumLearnSVM";
            string kernelF = "rbf";
            int sigma = 1, gamma = 1;
            learn += "('" + userPath + "', " + "'" + kernelF + "', " + sigma + ", " + gamma + ");";
            string result = matlab.Execute(learn);
            */

            return true;
        }


        public override bool Decide()
        {
            Console.WriteLine("KNN - deciding on good images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < goodImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile(userPath, goodImages[i]);
                filesTrueResults[i] = runMatlabDecide();
                if (filesTrueResults[i] == Algorithm.FAIL)
                    return false;
            }

            Console.WriteLine("KNN - deciding on bad images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < badImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile(userPath, badImages[i]);
                filesFalseResults[i] = runMatlabDecide();
                if (filesFalseResults[i] == Algorithm.FAIL)
                    return false;
            }

            return true;
        }

     /*   public override bool Decide()
        {
            Console.WriteLine("KNN - deciding on good images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < goodImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile(userPath, goodImages[i]);
                filesTrueResults[i] = runMatlabDecide();
                if (filesTrueResults[i] == Algorithm.FAIL)
                    return false;
            }

            Console.WriteLine("KNN - deciding on bad images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < badImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile(userPath, badImages[i]);
                filesFalseResults[i] = runMatlabDecide();
                if (filesFalseResults[i] == Algorithm.FAIL)
                    return false;
            }

            return true;
        }
        */
        public override bool Decide_Test()
        {
            Console.WriteLine("KNN - deciding on good images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < goodImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile_Test(userPath, goodImages[i]);
                filesTrueResults[i] = runMatlabDecide_Test();
                if (filesTrueResults[i] == Algorithm.FAIL)
                    return false;
            }

            Console.WriteLine("KNN - deciding on bad images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < badImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile_Test(userPath, badImages[i]);
                filesFalseResults[i] = runMatlabDecide_Test();
                if (filesFalseResults[i] == Algorithm.FAIL)
                    return false;
            }

            return true;
        }

        private LearningAlgorithmML.Algorithm runMatlabDecide()
        {
            string result = matlab.Execute(decide);

            if (result.StartsWith("???"))
            {
                Console.WriteLine(result);
                return LearningAlgorithmML.Algorithm.FAIL;
            }

            string resultPath = userPath + "\\Result.txt";
            string[] readText = File.ReadAllLines(resultPath);

            double conf = Convert.ToDouble(readText[0]);
            
            if (conf == ImageVector.BAD_IMAGE)
                return Algorithm.BAD;
            else if (conf == ImageVector.GOOD_IMAGE)
                return Algorithm.GOOD;

            return LearningAlgorithmML.Algorithm.FAIL;
        }

        private LearningAlgorithmML.Algorithm runMatlabDecide_Test()
        {
            string result = matlab.Execute(decide_Test);

            if (result.StartsWith("???"))
            {
                Console.WriteLine(result);
                return LearningAlgorithmML.Algorithm.FAIL;
            }

            string resultPath = userPath + "\\Result.txt";
            string[] readText = File.ReadAllLines(resultPath);

            double conf = Convert.ToDouble(readText[0]);

            if (conf == ImageVector.BAD_IMAGE)
                return Algorithm.BAD;
            else if (conf == ImageVector.GOOD_IMAGE)
                return Algorithm.GOOD;

            return LearningAlgorithmML.Algorithm.FAIL;
        }

        private void averageResults(out double avgTrueAndSelected, out double avgTrueButNotSelected, out double avgFalseButSelected, out double avgFalseAndNotSelected,
            out double avgTrueAndSelectedNotLearned, out double avgTrueButNotSelectedNotLearned, out double avgFalseButSelectedNotLearned, out double avgFalseAndNotSelectedNotLearned,
            out double avgAccuracy, out double avgRecall, out double avgPrecision, out double avgFScore, out double avgMCC, out double avgAcc)
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
            avgRecall = (double)recall / (double)decisionCount;
            avgPrecision = (double)precision / (double)decisionCount;
            avgFScore = (double)FScore / (double)decisionCount;
            avgMCC = (double)mcc / (double)decisionCount;
        }

        public override string ToString()
        {
            double avgTrueAndSelected, avgTrueButNotSelected, avgFalseButSelected, avgFalseAndNotSelected, avgAccuracy, avgAcc, avgRecall,
                avgPrecision, avgFScore, avgMCC, avgTrueAndSelectedNotLearned, avgTrueButNotSelectedNotLearned, avgFalseButSelectedNotLearned,
                avgFalseAndNotSelectedNotLearned;

            averageResults(out avgTrueAndSelected, out avgTrueButNotSelected, out avgFalseButSelected, out avgFalseAndNotSelected,
                out avgTrueAndSelectedNotLearned, out avgTrueButNotSelectedNotLearned, out avgFalseButSelectedNotLearned, out avgFalseAndNotSelectedNotLearned,
                out avgAccuracy, out avgRecall, out avgPrecision, out avgFScore, out avgMCC, out avgAcc);

            double selectedAlbumSize = avgTrueAndSelected + avgFalseButSelected;
            double selectedAlbumSizeNotLearned = avgTrueAndSelectedNotLearned + avgFalseButSelectedNotLearned;

            string result = avgTrueAndSelected + "," + avgTrueButNotSelected + "," + avgFalseButSelected + "," + avgFalseAndNotSelected + "\n";
            result += avgTrueAndSelectedNotLearned + "\n";
            result += selectedAlbumSize + "," + selectedAlbumSizeNotLearned + "\n";
            result += avgAccuracy + ", " + avgAcc + ", " + avgRecall + ", " + avgPrecision + ", " + avgFScore + ", " + avgMCC + "\n";
            return result;
        }

        private string GetCommand(string path, int k)
        {
            string command = "albumKnn";
            command += "('" + path + "', " + k + " ," + ImageVector.NUMBER_OF_PARAMETERS + ");";
            return command;
        }
        private string GetCommand_Test(string path, int k,int numOfParam)
        {
            string command = "albumKnn";
            command += "('" + path + "', " + k + " ," + numOfParam + ");";
            return command;
        }



    }
}
