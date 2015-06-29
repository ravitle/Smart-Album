using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Emgu.CV;

namespace SPF
{
    static class DataConverter
    {

        public static int[] Internal_ExtractSubset(int len, double percent)
        {
            int subsetLength = (int)(len * percent);
            int[] subset = new int[subsetLength];
            Random rand = new Random();
            int randomNum;

            for (int index = 0; index < subsetLength; index++)
            {
                subset[index] = -1;
            }


            for (int index = 0; index < subsetLength; )
            {
                randomNum = rand.Next(len);
                bool found = false;
                for (int i = 0; i < index; i++)
                {
                    if (subset[i] == randomNum)
                    {
                        found = true;
                        break;
                    }
                }
                if (found == false)
                {
                    subset[index] = randomNum;
                    index++;
                }
            }
            return subset;
        }

      

        public static void copyImagesToFolder(string toFolder, string[] images)
        {
            if (!Directory.Exists(toFolder))
            {
                Directory.CreateDirectory(toFolder);
            }

            foreach (string image in images)
            {
                string[] folders = image.Split('\\');
                string dest = toFolder + "\\" + folders[folders.Length - 1];
                if (!File.Exists(dest))
                    File.Copy(image, dest);
            }
        }

        public static void convertDataVectorsToFile(string path, ImageVector[] images, int[] subset, bool[] isImageGood)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string csvPathLearn = path + "\\Learn.txt";
            System.IO.StreamWriter fileLearn = new System.IO.StreamWriter(@csvPathLearn, false);

            int length = subset.Length;

            for (int index = 0; index < length; index++)
            {
                if (isImageGood[index])
                    fileLearn.WriteLine(images[subset[index]].getAllParameters(true));
                else
                    fileLearn.WriteLine(images[subset[index]].getAllParameters(false));
            }
        }

        public static void convertDataVectorsToFile(string path, ImageVector[] goodImages, ImageVector[] badImages, int[] subsetFilesTrue, int[] subsetFilesFalse, bool subset)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string csvPathLearn = path + "\\Learn.txt";
            System.IO.StreamWriter fileLearn = new System.IO.StreamWriter(@csvPathLearn, false);


            int length;
            if (subset)
                length = subsetFilesTrue.Length;
            else
                length = goodImages.Length;

            for (int index = 0; index < length; index++)
            {
                if (subset)
                    fileLearn.WriteLine(goodImages[subsetFilesTrue[index]].getAllParameters(true));
                else
                    fileLearn.WriteLine(goodImages[index].getAllParameters(true));
            }

            if (subset)
                length = subsetFilesFalse.Length;
            else
                length = badImages.Length;

            for (int index = 0; index < length; index++)
            {
                if (subset)
                    fileLearn.WriteLine(badImages[subsetFilesFalse[index]].getAllParameters(false));
                else
                    fileLearn.WriteLine(badImages[index].getAllParameters(false));
            }

            fileLearn.Close();
        }

        public static void convertDataVectorsToFile_Test(string path, ImageVector[] goodImages, ImageVector[] badImages, int[] subsetFilesTrue, int[] subsetFilesFalse, bool subset)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string csvPathLearn = path + "\\Learn.txt";
            System.IO.StreamWriter fileLearn = new System.IO.StreamWriter(@csvPathLearn, false);


            int length;
            if (subset)
                length = subsetFilesTrue.Length;
            else
                length = goodImages.Length;

            for (int index = 0; index < length; index++)
            {
                if (subset)
                    fileLearn.WriteLine(goodImages[subsetFilesTrue[index]].getAllParameters_Test(true,goodImages[index].getNumOfParameters()));
                else
                    fileLearn.WriteLine(goodImages[index].getAllParameters_Test(true, goodImages[index].getNumOfParameters()));
            }

            if (subset)
                length = subsetFilesFalse.Length;
            else
                length = badImages.Length;

            for (int index = 0; index < length; index++)
            {
                if (subset)
                    fileLearn.WriteLine(badImages[subsetFilesFalse[index]].getAllParameters_Test(false,badImages[index].getNumOfParameters()));
                else
                    fileLearn.WriteLine(badImages[index].getAllParameters_Test(false, badImages[index].getNumOfParameters()));
            }

            fileLearn.Close();
        }

        public static void convertDataVectorsToMatrix(ImageVector[] goodImages, ImageVector[] badImages, int[] subsetFilesTrue, int[] subsetFilesFalse, out Matrix<float> mat, out Matrix<float> response, bool subset)
        {
            int prepCount = ImageVector.NUMBER_OF_PARAMETERS;

            int trueVectorCount = goodImages.Length;
            int falseVectorCount = badImages.Length;
            if (subset)
            {
                trueVectorCount = subsetFilesTrue.Length;
                falseVectorCount = subsetFilesFalse.Length;
            }

            int vectorCount = trueVectorCount + falseVectorCount;
            mat = new Matrix<float>(vectorCount, prepCount);
            response = new Matrix<float>(vectorCount, 1);

            int i;
            for (i = 0; i < trueVectorCount; i++)
            {
                for (int j = 0; j < prepCount; j++)
                {
                    int index = i;
                    if (subset)
                        index = subsetFilesTrue[i];

                    mat[i, j] = (float)goodImages[index].getParameterByIndex(j);
                }
                response[i, 0] = (float)ImageVector.GOOD_IMAGE;
            }

            for (; i < vectorCount; i++)
            {
                for (int j = 0; j < prepCount; j++)
                {
                    int index = i - trueVectorCount;
                    if (subset)
                        index = subsetFilesFalse[index];

                    mat[i, j] = (float)badImages[index].getParameterByIndex(j);
                }
                response[i, 0] = (float)ImageVector.BAD_IMAGE;
            }
        }

        /*
         * copy imageVector into a file
         */
        public static void convertImageVectorToFile(string path, ImageVector image)
        {
            string algoPath = path;
            string csvPathDecide = algoPath + "\\Decide.txt";

            System.IO.StreamWriter file = new System.IO.StreamWriter(@csvPathDecide, false);
            file.WriteLine(image.getAllParameters(false));
            file.Close();
        }

        public static void convertImageVectorToFile_Test(string path, ImageVector image)
        {
            string algoPath = path;
            string csvPathDecide = algoPath + "\\Decide.txt";

            System.IO.StreamWriter file = new System.IO.StreamWriter(@csvPathDecide, false);
            file.WriteLine(image.getAllParameters_Test(false,image.getNumOfParameters()));
            file.Close();
        }


        /*
         * copy one imageVector into matrix 
         */
        public static void convertImageVectorToMatrix(ImageVector image, out Matrix<float> imageCopy)
        {
            int prepCount = ImageVector.NUMBER_OF_PARAMETERS;

            imageCopy = new Matrix<float>(1, prepCount);
            for (int i = 0; i < prepCount; i++)
                imageCopy.Data[0, i] = (float)image.getParameterByIndex(i);

        }

        /*
         * copy an array of imageVector into matrix
         */
        public static void convertImageVectorToMatrix(ImageVector[] images, out Matrix<float> sample)
        {
            sample= new Matrix<float>(images.Length, ImageVector.NUMBER_OF_PARAMETERS);
            for (int i = 0; i < images.Length; i++ )
                for (int j = 0; j < ImageVector.NUMBER_OF_PARAMETERS; j++)
                    sample.Data[i, j] = (float)images[i].getParameterByIndex(j);
        }

        public static void normalizeMatrix(Matrix<float> images)
        {
            for (int i=0; i<images.Rows;i++)
            {
                float min, max;
                findMinAndMax(images, i, out min, out max);
                for (int j=0; j<ImageVector.NUMBER_OF_PARAMETERS; j++)
                    images[i, j] = norm(min, max, images[i, j]);
            }
        }

        private static void findMinAndMax(Matrix<float> image, int row, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            for (int i=0; i<ImageVector.NUMBER_OF_PARAMETERS; i++)
            {
                if (image.Data[row, i] < min)
                    min = image[row, i];
                if (image[row, i] > max)
                    max = image[row, i];
            }
        }

        private static float norm(float min, float max, float value)
        {
            return (value - min) / (max - min);
        }

        public static string[] Internal_ExtractFalseFiles(string[] filesAll, string[] filesTrue)
        {
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
            return filesFalse;
        }

        private static int[] createSubsetFromBool(bool[] isGoodImages, bool isGood, int length)
        {
            int[] subset = new int[length];
            int index = 0;
            for (int i = 0; i < isGoodImages.Length; i++)
            {
                if (isGoodImages[i] == isGood)
                    subset[index++] = i;
            }
            return subset;
        }



    }
}
