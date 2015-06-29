using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace SPF
{
    class KNN_SVM : LearningAlgorithmML
    {
        
        private string learn, decideSVM, decideKNN, cd;
        private KNN_Matlab knn;
        private SVM_Matlab svm;


        private double Svmprecision, Svmrecall, Svmaccuracy, SvmFScore, SvmgoodMeasure, Svmacc;
        private double Knnprecision, Knnrecall, Knnaccuracy, KnnFScore, KnngoodMeasure, Knnacc;


        public enum Kernel
        {
            SVM_RBF,
            SVM_QUADRIC,
            SVM_POLY,
            SVM_LINEAR,
            SVM_MLP
        }

        public KNN_SVM(string path, double sigma, double gamma, Kernel kernel, int k, ImageVector[] goodImages, ImageVector[] badImages)
        {
            type = "KNN & SVM";
            svm = new SVM_Matlab(path, sigma, gamma, (SVM_Matlab.Kernel)kernel, goodImages, badImages);
            knn = new KNN_Matlab(path, k, goodImages, badImages);

            totalImages = 0;

            this.goodImages = goodImages;
            this.badImages = badImages;

            learnedTrue = new bool[goodImages.Length];
            learnedFalse = new bool[badImages.Length];
            restartTest();

            userPath = path + "\\KNN_SVM";
            resultPath = userPath + "\\sigma_" + sigma + "_gamma_" + gamma;
            
            learn = GetLearnCommand(userPath, sigma, gamma, kernel);
            decideSVM = GetDecideCommandSVM(userPath, kernel);
            decideKNN = GetDecideCommandKNN(userPath, k);

            matlab = new MLApp.MLApp();
            cd = "cd " + smartAlbum.getMatlabDirectory();
            matlab.Execute(cd);
        }

        private void initLearned()
        {
            for (int i = 0; i < filesTrueResults.Length; i++)
                filesTrueResults[i] = LearningAlgorithmML.Algorithm.BAD;
            for (int i = 0; i < filesFalseResults.Length; i++)
                filesFalseResults[i] = LearningAlgorithmML.Algorithm.BAD;

            for (int i = 0; i < learnedTrue.Length; i++)
                learnedTrue[i] = false;
            for (int i = 0; i < learnedFalse.Length; i++)
                learnedFalse[i] = false;

            for (int i = 0; i < goodImages.Length; i++)
            {
                for (int j = 0; j < subsetFilesTrue.Length; j++)
                {
                    if (subsetFilesTrue[j] == i)
                    {
                        learnedTrue[i] = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < badImages.Length; i++)
            {
                for (int j = 0; j < subsetFilesFalse.Length; j++)
                {
                    if (subsetFilesFalse[j] == i)
                    {
                        learnedFalse[i] = true;
                        break;
                    }
                }
            }
        }

        public override bool Learn_Test(double percent, int[] subsetTrue, int[] subsetFalse)
        {
            //Console.WriteLine("Learning from images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));

            // extract subset
            subsetFilesTrue = subsetTrue;
            subsetFilesFalse = subsetFalse;

            initLearned();

            // convert image vectors into files(for matlab)
            //DataConverter.convertDataVectorsToFile(userPath, goodImages, badImages, subsetFilesTrue, subsetFilesFalse, true);

            bool result = svm.Learn_Test(percent, subsetFilesTrue, subsetFilesFalse);
            result = knn.Learn_Test(percent, subsetFilesTrue, subsetFilesFalse);
            //string result = matlab.Execute(learn);

            //Console.WriteLine("finished learning images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            return result;
        }

        public override bool Learn(double percent, int[] subsetTrue, int[] subsetFalse)
        {
            //Console.WriteLine("Learning from images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));

            // extract subset
            subsetFilesTrue = subsetTrue;
            subsetFilesFalse = subsetFalse;

            initLearned();

            // convert image vectors into files(for matlab)
            //DataConverter.convertDataVectorsToFile(userPath, goodImages, badImages, subsetFilesTrue, subsetFilesFalse, true);

            bool result = svm.Learn(percent, subsetFilesTrue, subsetFilesFalse);
            result = knn.Learn(percent, subsetFilesTrue, subsetFilesFalse);
            //string result = matlab.Execute(learn);

            //Console.WriteLine("finished learning images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            return result;
        }

        public override bool Decide_Test()
        {
            svm.Decide_Test();
            knn.Decide_Test();

            //Console.WriteLine("deciding on good images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < goodImages.Length; i++)
            {
                if (svm.FilesTrueResults[i] == Algorithm.FAIL || knn.FilesTrueResults[i] == Algorithm.FAIL)
                {
                    filesTrueResults[i] = Algorithm.FAIL;
                    return false;
                }
                else if (svm.FilesTrueResults[i] == Algorithm.GOOD)
                    filesTrueResults[i] = Algorithm.GOOD;
                else if (svm.FilesTrueResults[i] == Algorithm.BAD)
                    filesTrueResults[i] = Algorithm.BAD;
                else if (svm.FilesTrueResults[i] == Algorithm.GOOD_NOT_SURE && knn.FilesTrueResults[i] == Algorithm.GOOD)
                    filesTrueResults[i] = Algorithm.GOOD;
                else if (svm.FilesTrueResults[i] == Algorithm.BAD_NOT_SURE && knn.FilesTrueResults[i] == Algorithm.BAD)
                    filesTrueResults[i] = Algorithm.BAD;
                else
                    filesTrueResults[i] = Algorithm.BAD;
            }

            ////Console.WriteLine("deciding on bad images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < badImages.Length; i++)
            {
                if (svm.FilesFalseResults[i] == Algorithm.FAIL || knn.FilesFalseResults[i] == Algorithm.FAIL)
                {
                    filesTrueResults[i] = Algorithm.FAIL;
                    return false;
                }
                else if (svm.FilesFalseResults[i] == Algorithm.GOOD)
                    filesFalseResults[i] = Algorithm.GOOD;
                else if (svm.FilesFalseResults[i] == Algorithm.BAD)
                    filesFalseResults[i] = Algorithm.BAD;
                else if (svm.FilesFalseResults[i] == Algorithm.GOOD_NOT_SURE && knn.FilesFalseResults[i] == Algorithm.GOOD)
                    filesFalseResults[i] = Algorithm.GOOD;
                else if (svm.FilesFalseResults[i] == Algorithm.BAD_NOT_SURE && knn.FilesFalseResults[i] == Algorithm.BAD)
                    filesFalseResults[i] = Algorithm.BAD;
                else
                    filesFalseResults[i] = Algorithm.BAD;
            }

            return true;
        }

        public override bool Decide()
        {
            svm.Decide();
            knn.Decide();

            //Console.WriteLine("deciding on good images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < goodImages.Length; i++)
            {
                if (svm.FilesTrueResults[i] == Algorithm.FAIL || knn.FilesTrueResults[i] == Algorithm.FAIL)
                {
                    filesTrueResults[i] = Algorithm.FAIL;
                    return false;
                }
                else if (svm.FilesTrueResults[i] == Algorithm.GOOD)
                    filesTrueResults[i] = Algorithm.GOOD;
                else if (svm.FilesTrueResults[i] == Algorithm.BAD)
                    filesTrueResults[i] = Algorithm.BAD;
                else if (svm.FilesTrueResults[i] == Algorithm.GOOD_NOT_SURE && knn.FilesTrueResults[i] == Algorithm.GOOD)
                    filesTrueResults[i] = Algorithm.GOOD;
                else if (svm.FilesTrueResults[i] == Algorithm.BAD_NOT_SURE && knn.FilesTrueResults[i] == Algorithm.BAD)
                    filesTrueResults[i] = Algorithm.BAD;
                else
                    filesTrueResults[i] = Algorithm.BAD;
            }

            ////Console.WriteLine("deciding on bad images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < badImages.Length; i++)
            {
                if (svm.FilesFalseResults[i] == Algorithm.FAIL || knn.FilesFalseResults[i] == Algorithm.FAIL)
                {
                    filesTrueResults[i] = Algorithm.FAIL;
                    return false;
                }
                else if (svm.FilesFalseResults[i] == Algorithm.GOOD)
                    filesFalseResults[i] = Algorithm.GOOD;
                else if (svm.FilesFalseResults[i] == Algorithm.BAD)
                    filesFalseResults[i] = Algorithm.BAD;
                else if (svm.FilesFalseResults[i] == Algorithm.GOOD_NOT_SURE && knn.FilesFalseResults[i] == Algorithm.GOOD)
                    filesFalseResults[i] = Algorithm.GOOD;
                else if (svm.FilesFalseResults[i] == Algorithm.BAD_NOT_SURE && knn.FilesFalseResults[i] == Algorithm.BAD)
                    filesFalseResults[i] = Algorithm.BAD;
                else
                    filesFalseResults[i] = Algorithm.BAD;
            }

            return true;
        }

        public override void restartTest()
        {
            svm.restartTest();
            knn.restartTest();

            recall = 0;
            precision = 0;
            accuracy = 0;
            acc = 0;
            FScore = 0;
            goodMeasure = 0;

            Svmrecall = 0;
            Svmprecision = 0;
            Svmaccuracy = 0;
            Svmacc = 0;
            SvmFScore = 0;
            SvmgoodMeasure = 0;

            Knnrecall = 0;
            Knnprecision = 0;
            Knnaccuracy = 0;
            Knnacc = 0;
            KnnFScore = 0;
            KnngoodMeasure = 0;

            decisionCount = 0;
            trueAndSelected = 0;
            trueAndSelectedNotLearned = 0;
            falseAndNotSelected = 0;
            falseAndNotSelectedNotLearned = 0;
            trueButNotSelected = 0;
            trueButNotSelectedNotLearned = 0;
            falseButSelected = 0;
            falseButSelectedNotLearned = 0;


            filesTrueResults = new LearningAlgorithmML.Algorithm[goodImages.Length];
            filesFalseResults = new LearningAlgorithmML.Algorithm[badImages.Length];

            for (int i = 0; i < learnedTrue.Length; i++)
                learnedTrue[i] = false;
            for (int i = 0; i < learnedFalse.Length; i++)
                learnedFalse[i] = false;
        }

        public override void checkDecision()
        {

            svm.checkDecision();
            knn.checkDecision();


            decisionCount++;
            totalImages = filesFalseResults.Length + filesTrueResults.Length;
            totalTrue = filesTrueResults.Length;
            TotalFalse = filesFalseResults.Length;
            resetCounters();

            for (int i = 0; i < filesTrueResults.Length; i++)
            {
                if (checkAlgorithm(filesTrueResults[i]))
                {
                    if (!learnedTrue[i])
                        TSNL++;//trueAndSelectedNotLearned++;
                    TS++;//trueAndSelected++;
                }
                else
                {
                    if (!learnedTrue[i])
                        TNSNL++;//trueButNotSelectedNotLearned++;
                    TNS++;//trueButNotSelected++;
                }
            }

            for (int i = 0; i < filesFalseResults.Length; i++)
            {
                if (checkAlgorithm(filesFalseResults[i]))
                {
                    if (!learnedFalse[i])
                        FSNL++;//falseButSelectedNotLearned++;
                    FS++;//falseButSelected++;
                }
                else
                {
                    if (!learnedFalse[i])
                        FNSNL++;//falseAndNotSelectedNotLearned++;
                    FNS++;//falseAndNotSelected++;
                }
            }

            trueAndSelectedNotLearned += TSNL;
            trueAndSelected += TS;
            trueButNotSelectedNotLearned += TNSNL;
            trueButNotSelected += TNS;
            falseButSelected += FS;
            falseButSelectedNotLearned += FSNL;
            falseAndNotSelected += FNS;
            falseAndNotSelectedNotLearned += FNSNL;

            goodMeasure += calcMeasure();
            accuracy += calcNotLearnedAccuracy();
            acc += calcTotalAccuracy();
            double rec = calcRecall();
            double pre = calcPrecision();
            recall += rec;
            precision += pre;
            FScore += calcFScore(pre, rec);

            Console.WriteLine("\n" + type + " FScore average - " + this.FScore / decisionCount);
            Console.WriteLine(type + " Accuracy average - " + this.Acc / decisionCount);
            Console.WriteLine(type + " Recall average - " + this.Recall / decisionCount);
            Console.WriteLine(type + " Precision average - " + this.Precision / decisionCount);

        }

        private void averageResults(out double avgTrueAndSelected, out double avgTrueButNotSelected, out double avgFalseButSelected, out double avgFalseAndNotSelected,
            out double avgTrueAndSelectedNotLearned, out double avgTrueButNotSelectedNotLearned, out double avgFalseButSelectedNotLearned, out double avgFalseAndNotSelectedNotLearned,
            out double avgAccuracy, out double avgRecall, out double avgPrecision, out double avgFScore, out double avgAcc, out double avgMeasure,
            out double avgSvmAccuracy, out double avgSvmRecall, out double avgSvmPrecision, out double avgSvmFScore, out double avgSvmAcc, out double avgSvmMeasure,
            out double avgKnnAccuracy, out double avgKnnRecall, out double avgKnnPrecision, out double avgKnnFScore, out double avgKnnAcc, out double avgKnnMeasure)
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

            avgSvmAccuracy = (double)Svmaccuracy / (double)decisionCount;
            avgSvmAcc = (double)Svmacc / (double)decisionCount;
            avgSvmMeasure = (double)SvmgoodMeasure / (double)decisionCount;
            avgSvmRecall = (double)Svmrecall / (double)decisionCount;
            avgSvmPrecision = (double)Svmprecision / (double)decisionCount;
            avgSvmFScore = (double)SvmFScore / (double)decisionCount;

            avgKnnAccuracy = (double)Knnaccuracy / (double)decisionCount;
            avgKnnAcc = (double)Knnacc / (double)decisionCount;
            avgKnnMeasure = (double)KnngoodMeasure / (double)decisionCount;
            avgKnnRecall = (double)Knnrecall / (double)decisionCount;
            avgKnnPrecision = (double)Knnprecision / (double)decisionCount;
            avgKnnFScore = (double)KnnFScore / (double)decisionCount;
        }

        public override string ToString()
        {
            double avgTrueAndSelected, avgTrueButNotSelected, avgFalseButSelected, avgFalseAndNotSelected,
                avgAccuracy, avgAcc, avgMeasure, avgRecall, avgPrecision, avgFScore,
                avgSvmAccuracy, avgSvmAcc, avgSvmMeasure, avgSvmRecall, avgSvmPrecision, avgSvmFScore,
                avgKnnAccuracy, avgKnnAcc, avgKnnMeasure, avgKnnRecall, avgKnnPrecision, avgKnnFScore,
                avgTrueAndSelectedNotLearned, avgTrueButNotSelectedNotLearned, avgFalseButSelectedNotLearned,
                avgFalseAndNotSelectedNotLearned;

            averageResults(out avgTrueAndSelected, out avgTrueButNotSelected, out avgFalseButSelected, out avgFalseAndNotSelected,
                out avgTrueAndSelectedNotLearned, out avgTrueButNotSelectedNotLearned, out avgFalseButSelectedNotLearned, out avgFalseAndNotSelectedNotLearned,
                out avgAccuracy, out avgRecall, out avgPrecision, out avgFScore, out avgAcc, out avgMeasure,
                out avgSvmAccuracy, out avgSvmRecall, out avgSvmPrecision, out avgSvmFScore, out avgSvmAcc, out avgSvmMeasure,
                out avgKnnAccuracy, out avgKnnRecall, out avgKnnPrecision, out avgKnnFScore, out avgKnnAcc, out avgKnnMeasure);

            double selectedAlbumSize = avgTrueAndSelected + avgFalseButSelected;
            double selectedAlbumSizeNotLearned = avgTrueAndSelectedNotLearned + avgFalseButSelectedNotLearned;

            string result = avgTrueAndSelected + "," + avgTrueButNotSelected + "," + avgFalseButSelected + "," + avgFalseAndNotSelected + "\n";
            result += avgTrueAndSelectedNotLearned + "\n";
            result += selectedAlbumSize + "," + selectedAlbumSizeNotLearned + "\n";
            result += avgAccuracy + ", " + avgAcc + ", " + avgRecall + ", " + avgPrecision + ", " + avgFScore + "\n";

            result += "****************\n";
            result += "\n" + svm.ToString();
            result += "****************\n";
            result += "\n" + knn.ToString();

            return result;
        }

        public override void Quit()
        {
            matlab.Quit();
        }

        private string GetLearnCommand(string path, double sigma, double gamma, Kernel kernel)
        {
            string command = "albumLearnSVM";
            string kernelF = getKernel(kernel);

            command += "('" + path + "', " + "'" + kernelF + "', " + sigma + ", " + gamma + ");";
            return command;
        }

        private string GetDecideCommandSVM(string path, Kernel kernel)
        {
            string command = "albumDecideSVM";
            string kernelF = getKernel(kernel);

            command += "('" + path + "', " + "'" + kernelF + "');";
            return command;
        }

        private string GetDecideCommandKNN(string path, int k)
        {
            string command = "albumKnn";

            command += "('" + path + "', " + k + ");";
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
