using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace SPF
{
    class PAPA
    {
        private string allPath;
        private string truePath;
        private string userPath;
        private string decidePath;

        public PAPA(string allPathL, string truePathL, string userPathL, string decidePathL)
        {
            allPath = allPathL;
            truePath = truePathL;
            userPath = userPathL;
            decidePath = decidePathL;
        }

        public static string getRootDirectory()
        {
            string path = Environment.CurrentDirectory.ToString();
            path = System.IO.Directory.GetParent(path).ToString();
            path = System.IO.Directory.GetParent(path).ToString();
            path = System.IO.Directory.GetParent(path).ToString();
            path = System.IO.Directory.GetParent(path).ToString();
            path = System.IO.Directory.GetParent(path).ToString();
            return path;
        }

        public static string getPicturesDirectory()
        {
            string pictures = getRootDirectory();
            pictures += "\\pictures";
            return pictures;
        }

        public static string getMatlabDirectory()
        {
            string path = Environment.CurrentDirectory.ToString();
            path = System.IO.Directory.GetParent(path).ToString();
            path = System.IO.Directory.GetParent(path).ToString();
            path = System.IO.Directory.GetParent(path).ToString();
            path = System.IO.Directory.GetParent(path).ToString();
            path += "\\papa_matlab";
            return path;
        }

        public void Arrange()
        {
            string[] filesAll = Testing.GetFilesFromDirectory(userPath).ToArray();
            foreach (string filePath in filesAll)
            {
                string good = truePath;
                string dest = null;
                string fileName = System.IO.Path.GetFileName(filePath);

                if (fileName[0] == 'G')
                {
                    good = good + "\\" + fileName;
                    System.IO.File.Copy(filePath, good, true);
                }

                if (fileName[0] == 'G' || fileName[0] == 'B')
                {
                    dest = allPath + "\\" + fileName;
                }

                if (fileName[0] == 'D')
                {
                    dest = decidePath + "\\" + fileName;
                }

                if(dest != null)
                    System.IO.File.Move(filePath, dest);
            }

        }
        public bool Learn(){
            
            // Get list of files in folders
            string[] filesAll = Testing.GetFilesFromDirectory(allPath).ToArray();
            string[] filesTrue = Testing.GetFilesFromDirectory(truePath).ToArray();
            if (filesAll.Length <= 0 || filesTrue.Length <= 0)
                return false;

            // Make False vector list - All the files in all that are not in true.
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

            string csvPath = userPath + "\\Learn.txt";
            System.IO.StreamWriter file = new System.IO.StreamWriter(@csvPath, true);

            // Convert to vector List
            foreach (string path in filesTrue)
            {
                ImageVector vec = new ImageVector(path, null);
                file.WriteLine(vec.getAllParameters(true));
            }
            foreach (string path in filesFalse)
            {
                ImageVector vec = new ImageVector(path, null);
                file.WriteLine(vec.getAllParameters(false));
            }

            file.Close();

            MLApp.MLApp matlab = new MLApp.MLApp();
            
            string cd = "cd C:\\Users\\anoach\\Desktop\\papa_matlab";
            string exe = "albumLearnRbf1('" + userPath + "');";

            matlab.Execute(cd);
            matlab.Execute(exe);

            return true;
        }
        public bool Decide()
        {
            // Get list of files in folders
            String[] files = Testing.GetFilesFromDirectory(decidePath).ToArray();
            string csvPath = userPath + "\\Decide.txt";
            string resultPath = userPath + "\\Result.txt";

            MLApp.MLApp matlab = new MLApp.MLApp();

            string cd = "cd C:\\Users\\anoach\\Desktop\\papa_matlab";
            string exe = "albumKnnTwo('" + userPath + "');";

            matlab.Execute(cd);

            for (int i = 0; i < files.Length; i++)
            {

                System.IO.StreamWriter file = new System.IO.StreamWriter(@csvPath, false);
                ImageVector vec = new ImageVector(files[i], null);
                file.WriteLine(vec.getAllParameters(false));
                file.Close();

                matlab.Execute(exe);

                string[] readText = File.ReadAllLines(resultPath);
                if (readText[0] == "1")
                {
                    Console.WriteLine("Good Image");
                }
                else
                {
                    Console.WriteLine("Bad Image. Delete it");
                    File.Delete(files[i]);
                }


            }

           return true;

        }

        public void Test()
        {

            // Get list of files in folders
            string[] filesAll = Testing.GetFilesFromDirectory(allPath).ToArray();
            string[] filesTrue = Testing.GetFilesFromDirectory(truePath).ToArray();
            if (filesAll.Length <= 0 || filesTrue.Length <= 0)
                return;

            string[] filesFalse = Internal_ExtractFalseFiles(filesAll, filesTrue);

            Console.WriteLine("Process good images: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            ImageVector[] goodImages = Internal_GetImages(filesTrue);
            Console.WriteLine("Process bad images: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
            ImageVector[] badImages = Internal_GetImages(filesFalse);

            string goodImagesPath = userPath + "\\imagesGood.txt";
            string badImagesPath = userPath + "\\imagesBad.txt";

            System.IO.StreamWriter fileGoodImages = new System.IO.StreamWriter(goodImagesPath, false);
            System.IO.StreamWriter fileBadImages = new System.IO.StreamWriter(badImagesPath, false);
            for (int index = 0; index < goodImages.Length; index++)
            {
                fileGoodImages.WriteLine(goodImages[index].getAllParameters(true));
            }
            for (int index = 0; index < badImages.Length; index++)
            {
                fileBadImages.WriteLine(badImages[index].getAllParameters(false));
            }

            fileGoodImages.Close();
            fileBadImages.Close();

            for (double percent = 0.1; percent < 0.7; percent = percent + 0.1 )
            {
                int p = (int)(percent * 10);
                string DataPath = userPath + "\\Data_" + p + ".txt";
                System.IO.StreamWriter file = new System.IO.StreamWriter(@DataPath, false);

                for (int i = 0; i < 10; i++ )
                {
                    int[] subsetFilesTrue = Internal_ExtractSubset(goodImages.Length,percent);
                    int[] subsetFilesFalse = Internal_ExtractSubset(badImages.Length,percent);

                    Console.WriteLine("Percent = " + percent + ",Iteration =  " + i + ", Time: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));

                    Interanl_Learn(goodImages, badImages, subsetFilesTrue, subsetFilesFalse);

                    string data = "";
                    data = data + Internal_Decide(goodImages, badImages, 0) + ",\t";
                    data = data + Internal_Decide(goodImages, badImages, 1) + ",\t";
                    data = data + Internal_Decide(goodImages, badImages, 2);
                    
                    file.WriteLine(data);
                }

                file.Close();

            }
            Console.WriteLine("Done: " + string.Format("{0:HH:mm:ss tt}", DateTime.Now));
        }

        private string[] Internal_ExtractFalseFiles(string[] filesAll, string[] filesTrue)
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

        private int[] Internal_ExtractSubset(int len, double percent)
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

        private void Interanl_Learn(ImageVector[] goodImages, ImageVector[] badImages, int[] subsetFilesTrue, int[] subsetFilesFalse)
        {
            string csvPathLearn = userPath + "\\Learn.txt";
            System.IO.StreamWriter fileLearn = new System.IO.StreamWriter(@csvPathLearn, false);

            for (int index = 0; index < subsetFilesTrue.Length; index++)
            {
                fileLearn.WriteLine(goodImages[subsetFilesTrue[index]].getAllParameters(true));
            }
            for (int index = 0; index < subsetFilesFalse.Length; index++)
            {
                fileLearn.WriteLine(badImages[subsetFilesFalse[index]].getAllParameters(false));
            }

            fileLearn.Close();

            MLApp.MLApp matlab = new MLApp.MLApp();

            string cd = "cd C:\\Users\\anoach\\Desktop\\papa_matlab";

            matlab.Execute(cd);
            //TODO: choose only the algo u need...
            //matlab.Execute("albumLearnPoly5('" + userPath + "');");
            //matlab.Execute("albumLearnPolyTen('" + userPath + "');");
            //matlab.Execute("albumLearnPolyTenFive('" + userPath + "');");

            //matlab.Execute("albumLearnQuadratic('" + userPath + "');");

            matlab.Execute("albumLearnRbf1('" + userPath + "');");
            //matlab.Execute("albumLearnRbf3('" + userPath + "');");
            //matlab.Execute("albumLearnRbf5('" + userPath + "');");


        }

        private string Internal_Decide(ImageVector[] filesTrue, ImageVector[] filesFalse, int algo)
        {
            int trueAndSelected = 0;
            int FalseAndNotSelected = 0;
            int trueButNotSelected = 0;
            int falseButSelected = 0;

            string data;

            MLApp.MLApp matlab = new MLApp.MLApp();

            string cd = "cd C:\\Program Files\\MATLAB\\R2013a\\bin";
            matlab.Execute(cd);


            for (int i = 0; i < filesTrue.Length; i++)
            {
                if (Internal_DecideImage(matlab, filesTrue[i], algo))
                    trueAndSelected++;
                else
                    trueButNotSelected++;
            }

            for (int i = 0; i < filesFalse.Length; i++)
            {
                if (Internal_DecideImage(matlab, filesFalse[i], algo))
                    falseButSelected++;
                else
                    FalseAndNotSelected++;
            }

            data = trueAndSelected + "," + trueButNotSelected + "," + falseButSelected + "," + FalseAndNotSelected;
            return data;
        }

        private bool Internal_DecideImage(MLApp.MLApp matlab, ImageVector image, int algo)
        {
            string csvPathDecide    = userPath + "\\Decide.txt";
            int result = 0;

            System.IO.StreamWriter file = new System.IO.StreamWriter(@csvPathDecide, false);
            file.WriteLine(image.getAllParameters(false));
            file.Close();

            string commandBestKnn = Internal_GetCommand(2);
            string commandBestSvm = Internal_GetCommand(10);

            if (algo != 1)
            {
                matlab.Execute(commandBestKnn);
                result += Internal_GetResult();
            }

            if (algo != 0)
            {
                matlab.Execute(commandBestSvm);
                result += Internal_GetResult();
            }

            if (result == 2)
                return true;
            if ((algo != 2) && (result == 1))
                return true;

            return false;
        }

        private int Internal_GetResult(){

            string resultPath = userPath + "\\Result.txt";
            string[] readText = File.ReadAllLines(resultPath);
            if (readText[0] == "0")
                return 0;
            return 1;

        }
        private ImageVector[] Internal_GetImages(string[] imagesStr)
        {
            ImageVector[] images = new ImageVector[imagesStr.Length];
            int index = 0;
            foreach (string path in imagesStr)
            {
                Console.WriteLine("proccesing image #" + index + " out of " + imagesStr.Length);
                images[index] = new ImageVector(path, null);
                index++;
            }
            return images;
        }

        private string Internal_GetCommand(int algo)
        {
            string command = null;

            switch (algo)
            {
                case 1:
                    command = "albumKnnOne('" + userPath + "');";
                    break;
                case 2:
                    command = "albumKnnTwo('" + userPath + "');";
                    break;
                case 3:
                    command = "albumKnnThree('" + userPath + "');";
                    break;
                case 4:
                    command = "albumKnnFour('" + userPath + "');";
                    break;
                case 5:
                    command = "albumKnnFive('" + userPath + "');";
                    break;

                case 6:
                    command = "albumDecidePoly5('" + userPath + "');";
                    break;
                case 7:
                    command = "albumDecidePoly10('" + userPath + "');";
                    break;
                case 8:
                    command = "albumDecidePoly15('" + userPath + "');";
                    break;

                case 9:
                    command = "albumDecideQuadratic('" + userPath + "');";
                    break;

                case 10:
                    command = "albumDecideRbf1('" + userPath + "');";
                    break;
                case 11:
                    command = "albumDecideRbf3('" + userPath + "');";
                    break;
                case 12:
                    command = "albumDecideRbf5('" + userPath + "');";
                    break;
            }

            return command;
        }

    }
}
