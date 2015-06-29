using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace SPF
{
    class smartIntersection : LearningAlgorithmML
    {

        private string cd;

        private SVM_Matlab[] algos;
        private KNN_Matlab knn;



        public enum Kernel
        {
            SVM_RBF,
            SVM_QUADRIC,
            SVM_POLY,
            SVM_LINEAR,
            SVM_MLP
        }

        public smartIntersection(string path, ImageVector[] goodImages, ImageVector[] badImages)
        {
            knn = null;
            algos = new SVM_Matlab[2];
            algos[0] = new SVM_Matlab(path, 1, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            //algos[1] = new SVM_Matlab(path, 1.25, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            //algos[1] = new SVM_Matlab(path, 1.5, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            //algos[3] = new SVM_Matlab(path, 1.75, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            algos[1] = new SVM_Matlab(path, 2, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            //algos[0] = new SVM_Matlab(path, 4, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            //algos[0] = new SVM_Matlab(path, 8, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            //algos[2] = new SVM_Matlab(path, 16, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            //algos[0] = new SVM_Matlab(path, 32, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            //algos[4] = new SVM_Matlab(path, 64, 1, SVM_Matlab.Kernel.SVM_RBF, goodImages, badImages);
            //algos[1] = new SVM_Matlab(path, 16, 1, SVM_Matlab.Kernel.SVM_LINEAR, goodImages, badImages);
            //algos[2] = new SVM_Matlab(path, 16, 1, SVM_Matlab.Kernel.SVM_MLP, goodImages, badImages);
            //algos[1] = new SVM_Matlab(path, 16, 1, SVM_Matlab.Kernel.SVM_QUADRIC, goodImages, badImages);
            //algos[1] = new SVM_Matlab(path, 2, 1, SVM_Matlab.Kernel.SVM_POLY, goodImages, badImages);
            //algos[0] = new SVM_Matlab(path, 3, 1, SVM_Matlab.Kernel.SVM_POLY, goodImages, badImages);
            //algos[1] = new SVM_Matlab(path, 4, 1, SVM_Matlab.Kernel.SVM_POLY, goodImages, badImages);
            //algos[2] = new SVM_Matlab(path, 5, 1, SVM_Matlab.Kernel.SVM_POLY, goodImages, badImages);
            knn = new KNN_Matlab(path, 2, goodImages, badImages);
            totalImages = 0;

            this.goodImages = goodImages;
            this.badImages = badImages;

            learnedTrue = new bool[goodImages.Length];
            learnedFalse = new bool[badImages.Length];
            restartTest();

            userPath = path + "\\test";
            resultPath = userPath + "\\test";


            matlab = new MLApp.MLApp();
            cd = "cd " + smartAlbum.getMatlabDirectory();
            matlab.Execute(cd);
        }

        public override bool Learn_Test(double percent, int[] subsetTrue, int[] subsetFalse)
        {
            return true;
        }
        public override bool Learn(double percent, int[] subsetTrue, int[] subsetFalse)
        {
            //Console.WriteLine("Learning from images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));

            // extract subset
            subsetFilesTrue = subsetTrue;
            subsetFilesFalse = subsetFalse;

            initLearned();

            bool result = false;
            for (int i = 0; i < algos.Length; i++)
                result = algos[i].Learn(percent, subsetFilesTrue, subsetFilesFalse);

            if (knn != null)
                knn.Learn(percent, subsetFilesTrue, subsetFilesFalse);
                
            return result;
        }

        public override bool Decide_Test()
        { return true; }

        public override bool Decide()
        {
            for (int i = 0; i < algos.Length; i++)
                algos[i].Decide();
            if (knn != null)
                knn.Decide();

            //Console.WriteLine("deciding on good images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < goodImages.Length; i++)
            {
                if (checkOrAlgo(Algorithm.FAIL, true, i))
                {
                    filesTrueResults[i] = Algorithm.FAIL;
                    return false;
                }
                else if (checkOrAlgo(Algorithm.GOOD, true, i))
                    filesTrueResults[i] = Algorithm.GOOD;
                else if (checkOrAlgo(Algorithm.BAD, true, i))
                    filesTrueResults[i] = Algorithm.BAD;
                else if (checkAndAlgo(Algorithm.GOOD_NOT_SURE, true, i, algos.Length))
                {
                    if (knn != null)
                    {
                        if (knn.FilesFalseResults[i] == Algorithm.GOOD)
                            filesTrueResults[i] = Algorithm.GOOD;
                    }
                    else
                        filesTrueResults[i] = Algorithm.GOOD;
                }
                else if (checkAndAlgo(Algorithm.BAD_NOT_SURE, true, i, algos.Length))
                {
                    if (knn != null)
                    {
                        if (knn.FilesFalseResults[i] == Algorithm.BAD)
                            filesTrueResults[i] = Algorithm.BAD;
                    }
                    else
                        filesTrueResults[i] = Algorithm.BAD;
                }
                else
                    filesTrueResults[i] = Algorithm.BAD;
            }

            ////Console.WriteLine("deciding on bad images Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            for (int i = 0; i < badImages.Length; i++)
            {
                if (checkOrAlgo(Algorithm.FAIL, false, i))
                {
                    filesTrueResults[i] = Algorithm.FAIL;
                    return false;
                }
                else if (checkOrAlgo(Algorithm.GOOD, false, i))
                    FilesFalseResults[i] = Algorithm.GOOD;
                else if (checkOrAlgo(Algorithm.BAD, false, i))
                    FilesFalseResults[i] = Algorithm.BAD;
                else if (checkAndAlgo(Algorithm.GOOD_NOT_SURE, false, i, algos.Length))
                {
                    if (knn != null)
                    {
                        if (knn.FilesFalseResults[i] == Algorithm.GOOD)
                            filesFalseResults[i] = Algorithm.GOOD;
                    }
                    else
                        filesFalseResults[i] = Algorithm.GOOD;
                }
                else if (checkAndAlgo(Algorithm.BAD_NOT_SURE, false, i, algos.Length))
                {
                    if (knn != null)
                    {
                        if (knn.FilesFalseResults[i] == Algorithm.BAD)
                            filesFalseResults[i] = Algorithm.BAD;
                    }
                    else
                        filesFalseResults[i] = Algorithm.BAD;
                }
                else
                    filesFalseResults[i] = Algorithm.BAD;
            }

            return true;
        }

        private bool checkOrAlgo(Algorithm algorithm, bool isTrue, int i)
        {
            if (isTrue)
            {
                for (int j = 0; j < algos.Length; j++)
                {
                    if (algos[j].FilesTrueResults[i] == algorithm)
                        return true;
                }
            }
            else
            {
                for (int j = 0; j < algos.Length; j++)
                {
                    if (algos[j].FilesFalseResults[i] == algorithm)
                        return true;
                }
            }

            return false;
        }

        private bool checkAndAlgo(Algorithm algorithm, bool isTrue, int i, int min)
        {
            int count = 0;
            if (isTrue)
            {
                for (int j = 0; j < algos.Length; j++)
                {
                    if (algos[j].FilesTrueResults[i] == algorithm)
                        count++;
                }
            }
            else
            {
                for (int j = 0; j < algos.Length; j++)
                {
                    if (algos[j].FilesFalseResults[i] == algorithm)
                        count++;
                }
            }

            if (count >= min)
                return true;

            return false;
        }

        public override void restartTest()
        {
            base.restartTest();
            for (int i = 0; i < algos.Length; i++)
                algos[i].restartTest();
            if (knn != null)
                knn.restartTest();
        }

        public override void checkDecision()
        {
            for (int i = 0; i < algos.Length; i++)
                algos[i].checkDecision();
            if (knn != null)
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

            //goodMeasure += calcMeasureNL();
            accuracy += calcNotLearnedAccuracy();
            acc += calcTotalAccuracy();
            double rec = calcRecall();
            double pre = calcPrecision();
            recall += rec;
            precision += pre;
            FScore +=  calcFScore(pre, rec);
            mcc += calcMCC();
            Console.WriteLine("\nFScore average - " + FScore / decisionCount);
            Console.WriteLine("MCC average - " + mcc / decisionCount);
            Console.WriteLine("Accuracy average - " + this.Acc / decisionCount);
            Console.WriteLine("Recall average - " + this.Recall / decisionCount);
            Console.WriteLine("Precision average - " + this.Precision / decisionCount);

        }

        private double calcRecall(double TSNL, double TNSNL)
        {
            double result = (double)TSNL / ((double)TSNL + (double)TNSNL);
            return result;
        }

        private double calcPrecision(double TSNL, double FSNL)
        {
            double result = (double)TSNL / ((double)TSNL + (double)FSNL);
            return result;
        }

        private double calcNotLearnedAccuracy(double TSNL, double FNSNL, double TNSNL, double FSNL)
        {
            double result = (((double)TSNL + (double)FNSNL) / ((double)TSNL + (double)TNSNL + (double)FSNL + (double)FNSNL));
            return result;
        }

        private double calcTotalAccuracy(double TS, double FNS)
        {
            double result = (((double)TS + (double)FNS) / ((double)totalImages));
            return result;
        }

        private double calcMeasure(double TSNL, double FSNL)
        {
            double result = (double)TSNL / ((double)TSNL + (double)FSNL);
            return result;
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
            double avgTrueAndSelected, avgTrueButNotSelected, avgFalseButSelected, avgFalseAndNotSelected,
                avgAccuracy, avgAcc, avgRecall, avgPrecision, avgFScore, avgMCC,
                avgTrueAndSelectedNotLearned, avgTrueButNotSelectedNotLearned, avgFalseButSelectedNotLearned,
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


            for (int i = 0; i < algos.Length; i++)
                result += "\n" + algos[i].ToString();

            if (knn != null)
                result += "\n" + knn.ToString();

            return result;
        }

    }
}
