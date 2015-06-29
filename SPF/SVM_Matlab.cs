using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SPF
{
    class SVM_Matlab : LearningAlgorithmML
    {
        
        private string learn, decide, cd;
        private string decide_Test;
        private string learn_Test;
        private double runConf;
        public static double CONFIDENCE_BAR = 1;

        private ImageVector[] allImages;

         



        public enum Kernel
        {
            SVM_RBF,
            SVM_QUADRIC,
            SVM_POLY,
            SVM_LINEAR,
            SVM_MLP
        }

        public SVM_Matlab(string path, double sigma, double gamma, Kernel kernel, ImageVector[] goodImages, ImageVector[] badImages)
        {
            type = "SVM";

            this.goodImages = goodImages;
            this.badImages = badImages;

            learnedTrue = new bool[goodImages.Length];
            learnedFalse = new bool[badImages.Length];

            confidenceTrue = new double[goodImages.Length];
            confidenceFalse = new double[badImages.Length];

            restartTest();

            userPath = path + "\\SVM_" + getKernel(kernel) + "_" + sigma;
            resultPath = userPath + "\\" + sigma + "_" + gamma;
            learn = GetLearnCommand(userPath, sigma, gamma, kernel);
            learn_Test = GetLearnCommand_Test(userPath, sigma, gamma, kernel,goodImages[0].getNumOfParameters());
            decide = GetDecideCommand(userPath, kernel);
            decide_Test = GetDecideCommand_Test(userPath, kernel, goodImages[0].getNumOfParameters());

            matlab = new MLApp.MLApp();
            cd = "cd " + smartAlbum.getMatlabDirectory();
            matlab.Execute(cd);
        }

        public SVM_Matlab(string path, double sigma, double gamma, Kernel kernel, ImageVector[] allImages)
        {
            type = "SVM";

            this.allImages = allImages;

            filesTrueResults = new Algorithm[allImages.Length];

            for (int i = 0; i < filesTrueResults.Length; i++)
                filesTrueResults[i] = LearningAlgorithmML.Algorithm.BAD;

            userPath = path + "\\SVM_" + getKernel(kernel) + "_" + sigma;
            resultPath = userPath + "\\" + sigma + "_" + gamma;
            learn = GetLearnCommand(userPath, sigma, gamma, kernel);
            decide = GetDecideCommand(userPath, kernel);

            matlab = new MLApp.MLApp();
            cd = "cd " + smartAlbum.getMatlabDirectory();
            matlab.Execute(cd);
        }

        public bool Learn(int[] subset, bool[] isImageGood)
        {
            Console.WriteLine("Learning from images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));

            //initLearned();

            // convert image vectors into files(for matlab)
            DataConverter.convertDataVectorsToFile(userPath, allImages, subset, isImageGood);

            string result = matlab.Execute(learn);

            if (result.StartsWith("???"))
            {
                Console.WriteLine(result);
                return false;
            }

            Console.WriteLine("finished learning images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            return true;
        }


       


        public override bool Learn(double percent, int[] subsetTrue, int[] subsetFalse)
        {
            Console.WriteLine("Learning from images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));

            // extract subset
            subsetFilesTrue = subsetTrue;
            subsetFilesFalse = subsetFalse;
            

            initLearned();

            // convert image vectors into files(for matlab)
            DataConverter.convertDataVectorsToFile(userPath, goodImages, badImages, subsetFilesTrue, subsetFilesFalse, true);

            string result = matlab.Execute(learn);

            if (result.StartsWith("???"))
            {
                Console.WriteLine(result);
                return false;
            }

            Console.WriteLine("finished learning images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            return true;
        }

        public override bool Learn_Test(double percent, int[] subsetTrue, int[] subsetFalse)
        {
            Console.WriteLine("Learning from images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));

            // extract subset
            subsetFilesTrue = subsetTrue;
            subsetFilesFalse = subsetFalse;


            initLearned();

            // convert image vectors into files(for matlab)
            DataConverter.convertDataVectorsToFile_Test(userPath, goodImages, badImages, subsetFilesTrue, subsetFilesFalse, true);

            string result = matlab.Execute(learn_Test);

            if (result.StartsWith("???"))
            {
                Console.WriteLine(result);
                return false;
            }

            Console.WriteLine("finished learning images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            return true;
        }

        public bool guiDecide()
        {
            Console.WriteLine("SVM - deciding on all images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < allImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile(userPath, allImages[i]);
                filesTrueResults[i] = runMatlabDecide();
                if (filesTrueResults[i] == Algorithm.FAIL)
                    return false;
                //confidenceTrue[i] = runConf;
            }

            return true;
        }

        public override bool Decide()
        {
            Console.WriteLine("SVM - deciding on good images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < goodImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile(userPath, goodImages[i]);
                filesTrueResults[i] = runMatlabDecide();
                if (filesTrueResults[i] == Algorithm.FAIL)
                    return false;
                confidenceTrue[i] = runConf;
            }

            Console.WriteLine("SVM - deciding on bad images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < badImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile(userPath, badImages[i]);
                filesFalseResults[i] = runMatlabDecide();
                if (filesFalseResults[i] == Algorithm.FAIL)
                    return false;
                confidenceFalse[i] = runConf;
            }

            return true;
        }

        public override bool Decide_Test()
        {
            Console.WriteLine("SVM - deciding on good images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < goodImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile_Test(userPath, goodImages[i]);
                filesTrueResults[i] = runMatlabDecide_Test();
                if (filesTrueResults[i] == Algorithm.FAIL)
                    return false;
                confidenceTrue[i] = runConf;
            }

            Console.WriteLine("SVM - deciding on bad images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < badImages.Length; i++)
            {
                DataConverter.convertImageVectorToFile_Test(userPath, badImages[i]);
                filesFalseResults[i] = runMatlabDecide_Test();
                if (filesFalseResults[i] == Algorithm.FAIL)
                    return false;
                confidenceFalse[i] = runConf;
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

            double decision = Convert.ToDouble(readText[0]);
            if (readText.Length > 1)
                runConf = Convert.ToDouble(readText[1]);
            else
                runConf = decision;

            if (runConf >= CONFIDENCE_BAR)
                return Algorithm.GOOD;
            else if (runConf < CONFIDENCE_BAR && runConf >= 0)
                return Algorithm.GOOD_NOT_SURE;
            else if (runConf > -CONFIDENCE_BAR)
                return Algorithm.BAD_NOT_SURE;
            else
                return Algorithm.BAD;
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

            double decision = Convert.ToDouble(readText[0]);
            if (readText.Length > 1)
                runConf = Convert.ToDouble(readText[1]);
            else
                runConf = decision;

            if (runConf >= CONFIDENCE_BAR)
                return Algorithm.GOOD;
            else if (runConf < CONFIDENCE_BAR && runConf >= 0)
                return Algorithm.GOOD_NOT_SURE;
            else if (runConf > -CONFIDENCE_BAR)
                return Algorithm.BAD_NOT_SURE;
            else
                return Algorithm.BAD;
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
                out avgTrueAndSelectedNotLearned,out avgTrueButNotSelectedNotLearned,out avgFalseButSelectedNotLearned,out avgFalseAndNotSelectedNotLearned,
                out avgAccuracy, out avgRecall, out avgPrecision, out avgFScore, out avgMCC, out avgAcc);

            double selectedAlbumSize = avgTrueAndSelected + avgFalseButSelected;
            double selectedAlbumSizeNotLearned = avgTrueAndSelectedNotLearned + avgFalseButSelectedNotLearned;

            string result = avgTrueAndSelected + "," + avgTrueButNotSelected + "," + avgFalseButSelected + "," + avgFalseAndNotSelected + "\n";
            result += avgTrueAndSelectedNotLearned + "\n";
            result += selectedAlbumSize + "," + selectedAlbumSizeNotLearned + "\n";
            result += avgAccuracy + ", " + avgAcc + ", " + avgRecall + ", " + avgPrecision + ", " + avgFScore + ", " + avgMCC + "\n";
            return result;
        }

        private string GetLearnCommand(string path, double sigma, double gamma, Kernel kernel)
        {
            string command = "albumLearnSVM";
            string kernelF = getKernel(kernel);

            command += "('" + path + "', " + "'" + kernelF + "', " + sigma + ", " + gamma + "," + ImageVector.NUMBER_OF_PARAMETERS + ");";
           
            return command;
        }

        private string GetLearnCommand_Test(string path, double sigma, double gamma, Kernel kernel,int numOfParam)
        {
            string command = "albumLearnSVM";
            string kernelF = getKernel(kernel);

            command += "('" + path + "', " + "'" + kernelF + "', " + sigma + ", " + gamma + "," + numOfParam + ");";

            return command;
        }
        private string GetDecideCommand(string path, Kernel kernel)
        {
            string command = "albumDecideResponseSVM";
            //string command = "albumDecideSVM";
            string kernelF = getKernel(kernel);

            command += "('" + path + "', " + "'" + kernelF + "'," + ImageVector.NUMBER_OF_PARAMETERS +");";
            return command;
        }

        private string GetDecideCommand_Test(string path, Kernel kernel,int numOfParam)
        {
            string command = "albumDecideResponseSVM";
            //string command = "albumDecideSVM";
            string kernelF = getKernel(kernel);

            command += "('" + path + "', " + "'" + kernelF + "'," + numOfParam + ");";
            return command;
        }

        private string getKernel(Kernel kernel)
        {
            string kernelF = null;
            switch (kernel)
            {
                case Kernel.SVM_POLY:
                    kernelF = "polynomial";
                    break;
                case Kernel.SVM_QUADRIC:
                    kernelF = "quadratic";
                    break;
                case Kernel.SVM_RBF:
                    kernelF = "rbf";
                    break;
                case Kernel.SVM_LINEAR:
                    kernelF = "linear";
                    break;
                case Kernel.SVM_MLP:
                    kernelF = "mlp";
                    break;
            }
            return kernelF;
        }
        
    }
}
