using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace SPF
{
    abstract class LearningAlgorithmML
    {

        protected static MLApp.MLApp matlab;

        protected string type;
        protected ImageVector[] goodImages, badImages;
        protected bool[] learnedTrue, learnedFalse;
        //arrays that contains if a picture is good or bad where each index equals the index in goodImages and badImages
        protected LearningAlgorithmML.Algorithm[] filesTrueResults, filesFalseResults;

        protected double[] confidenceTrue, confidenceFalse;

        public double[] ConfidenceTrue
        {
            get { return confidenceTrue; }
        }

        public double[] ConfidenceFalse
        {
            get { return confidenceFalse; }
        }

        public Algorithm[] FilesTrueResults
        {
            get { return filesTrueResults; }
        }

        public Algorithm[] FilesFalseResults
        {
            get { return filesFalseResults; }
        }

        protected int[] subsetFilesTrue, subsetFilesFalse;

        protected int trueAndSelected;
        protected int trueAndSelectedNotLearned;
        protected int falseAndNotSelected;
        protected int falseAndNotSelectedNotLearned;
        protected int trueButNotSelected;
        protected int trueButNotSelectedNotLearned;
        protected int falseButSelected;
        protected int falseButSelectedNotLearned;

        protected int TSNL = 0, TNSNL = 0, FSNL = 0, FNSNL = 0;
        protected int TS = 0, TNS = 0, FS = 0, FNS = 0;

        protected double precision, recall, accuracy, FScore, goodMeasure, acc, mcc;
        protected int decisionCount;
        
        public double MCC
        {
            get { return mcc; }
        }

        public double F1Score
        {
            get { return FScore; }
        }

        public double Acc
        {
            get { return acc; }
        }

        public double Precision
        {
            get { return precision; }
        }

        public double Recall
        {
            get { return recall; }
        }

        protected int totalImages, totalTrue, TotalFalse;

        public enum Algorithm
        {
            GOOD,
            GOOD_NOT_SURE,
            BAD,
            BAD_NOT_SURE,
            FAIL
        }

        protected Algorithm[] algorithm;

        protected string userPath, resultPath;

        public Algorithm[] Algo
        {
            get { return algorithm; }
        }

        public string UserPath
        {
            get { return userPath; }
        }

        public string ResultPath
        {
            get { return resultPath; }
        }

        protected double calcRecallNL()
        {
            double result = (double)TSNL / ((double)TSNL + (double)TNSNL);
            return result;
        }

        protected double calcPrecisionNL()
        {
            double result = (double)TSNL / ((double)TSNL + (double)FSNL);
            return result;
        }

        protected double calcRecall()
        {
            double result = (double)TS / ((double)TS + (double)TNS);
            return result;
        }

        protected double calcPrecision()
        {
            double result = (double)TS / ((double)TS + (double)FS);
            return result;
        }

        protected double calcNotLearnedAccuracy()
        {
            double result = (((double)TSNL + (double)FNSNL) / ((double)TSNL + (double)TNSNL + (double)FSNL + (double)FNSNL));
            return result;
        }

        protected double calcTotalAccuracy()
        {
            double result = (((double)TS + (double)FNS) / ((double)totalImages));
            return result;
        }

        protected double calcMeasureNL()
        {
            double result = (double)TSNL / ((double)TSNL + (double)FSNL);
            return result;
        }

        protected double calcFScore(double pre, double rec)
        {
            double b = Math.Pow(0.5, 2);
            double result = (1 + b) * pre * rec;
            result /= ((b * pre) + rec);
            if (double.IsNaN(result))
                return 0;
            return result;
        }

        protected double calcMCC()
        {
            //double temp1 = TS + FNS;
            //temp1 -= (FS + TNS);
            //double temp2 = TS + FS;
            //temp2 *= (TS + FS);
            //temp2 *= (TS + TNS);
            //temp2 *= (FNS + FS);
            //temp2 *= (FNS + TNS);
            double N = FS + TS + TNS + FNS;
            double S = (TS + TNS) / N;
            double P = (TS + FS) / N;
            double mcc = TS / N;
            mcc -= (S * P);

            //temp2 = Math.Sqrt(temp2);
            //temp1 /= temp2;

            double temp = Math.Sqrt(P * S * (1 - S) * (1 - P));
            if (temp == 0)
                return 0;
            mcc /= temp;
            return mcc;
        }

        protected double calcMeasure()
        {
            double result = (double)TS / ((double)TS + (double)FS);
            return result;
        }

        protected void resetCounters()
        {
            TSNL = 0;
            TNSNL = 0;
            FSNL = 0;
            FNSNL = 0;
            TS = 0;
            TNS = 0;
            FS = 0;
            FNS = 0;
        }

        public abstract bool Learn(double percent, int[] subsetTrue, int[] subsetFalse);

        public abstract bool Learn_Test(double percent, int[] subsetTrue, int[] subsetFalse);

        public abstract bool Decide();
        public abstract bool Decide_Test();

        public override abstract string ToString();

        public virtual void checkDecision()
        {
            decisionCount++;
            totalImages = filesFalseResults.Length + filesTrueResults.Length;
            totalTrue = filesTrueResults.Length;
            TotalFalse = filesFalseResults.Length;
            resetCounters();


            //if (!Directory.Exists(resultPath))
            //    Directory.CreateDirectory(resultPath);
            //string confFile = resultPath + "\\confidence.txt";
            //System.IO.StreamWriter file = new System.IO.StreamWriter(@confFile, false);
            //String decision;

            for (int i = 0; i < filesTrueResults.Length; i++)
            {

                //decision = "1, ";
                //if (filesTrueResults[i] == Algorithm.GOOD)
                //    decision += "1, ";
                //else
                //    decision += "0, ";
                //if (learnedTrue[i])
                //    decision += "1, ";
                //else
                //    decision += "0, ";
                //decision += ConfidenceTrue[i];
                //file.WriteLine(decision);

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

                //decision = "0, ";
                //if (filesFalseResults[i] == Algorithm.GOOD)
                //    decision += "1, ";
                //else
                //    decision += "0, ";
                //if (learnedFalse[i])
                //    decision += "1, ";
                //else
                //    decision += "0, ";
                //decision += ConfidenceFalse[i];
                //file.WriteLine(decision);

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
            //file.Close();

            trueAndSelectedNotLearned += TSNL;
            trueAndSelected += TS;
            trueButNotSelectedNotLearned += TNSNL;
            trueButNotSelected += TNS;
            falseButSelected += FS;
            falseButSelectedNotLearned += FSNL;
            falseAndNotSelected += FNS;
            falseAndNotSelectedNotLearned += FNSNL;

            accuracy += calcNotLearnedAccuracy();
            acc += calcTotalAccuracy();
            recall += calcRecall();
            precision += calcPrecision();
            FScore += calcFScore(calcPrecision(), calcRecall());
            mcc += calcMCC();

            Console.WriteLine("\n" + type + " FScore average - " + FScore / decisionCount);
            Console.WriteLine(type + " MCC average - " + this.mcc / decisionCount);
            Console.WriteLine(type + " Accuracy average - " + this.Acc / decisionCount);
            Console.WriteLine(type + " Recall average - " + this.Recall / decisionCount);
            Console.WriteLine(type + " Precision average - " + this.Precision / decisionCount);
        }

        protected virtual bool checkAlgorithm(Algorithm algorithm)
        {
            if (algorithm == Algorithm.GOOD || algorithm == Algorithm.GOOD_NOT_SURE)
                    return true;

            return false;
        }

        public virtual void initLearned()
        {
            for (int i = 0; i < filesTrueResults.Length; i++)
                filesTrueResults[i] = LearningAlgorithmML.Algorithm.BAD;
            for (int i = 0; i < filesFalseResults.Length; i++)
                filesFalseResults[i] = LearningAlgorithmML.Algorithm.BAD;

            for (int i = 0; i < learnedTrue.Length; i++)
                learnedTrue[i] = false;
            for (int i = 0; i < learnedFalse.Length; i++)
                learnedFalse[i] = false;

            for (int j = 0; j < subsetFilesTrue.Length; j++)
                learnedTrue[subsetFilesTrue[j]] = true;

            for (int j = 0; j < subsetFilesFalse.Length; j++)
                learnedFalse[subsetFilesFalse[j]] = true;
        }

        public virtual void restartTest()
        {
            recall = 0;
            precision = 0;
            accuracy = 0;
            acc = 0;
            FScore = 0;
            mcc = 0;
            goodMeasure = 0;

            decisionCount = 0;
            trueAndSelected = 0;
            trueAndSelectedNotLearned = 0;
            falseAndNotSelected = 0;
            falseAndNotSelectedNotLearned = 0;
            trueButNotSelected = 0;
            trueButNotSelectedNotLearned = 0;
            falseButSelected = 0;
            falseButSelectedNotLearned = 0;

            filesTrueResults = new Algorithm[goodImages.Length];
            filesFalseResults = new Algorithm[badImages.Length];

            for (int i = 0; i < learnedTrue.Length; i++)
                learnedTrue[i] = false;
            for (int i = 0; i < learnedFalse.Length; i++)
                learnedFalse[i] = false;
        }

        public virtual void Quit()
        {

        }

    }
}
