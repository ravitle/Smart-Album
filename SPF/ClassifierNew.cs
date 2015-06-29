using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace SPF
{
    class ClassifierNew
    {
        // Bounds used for classifing each parameter

        public static double[] CENTER_OF_GRAVITY_BOUNDS = { 0, 33, 66 };
        public static double[] DISTANCE_FROM_COG_BOUNDS = { 0, 15, 50, 100, 200 };
        public static double[] IMAGE_FACES_AREA_RATIO_BOUNDS = { 0.0001, 0.1, 0.3, 0.6, 1 };
        public static double[] RED_EYE_BOUNDS = { 0.5 };
        public static double[] AVERAGE_GRAY_LEVEL_BOUNDS = { 80, 100, 120, 150 };
        public static double[] AVERAGE_RED_LEVEL_BOUNDS = { 80, 100, 120, 160 };
        public static double[] AVERAGE_GREEN_LEVEL_BOUNDS = { 80, 110, 150 };
        public static double[] AVERAGE_BLUE_LEVEL_BOUNDS = { 80, 110, 150 };
        public static double[] AVERAGE_HUE_LEVEL_BOUNDS = { 30, 50, 80 };
        public static double[] AVERAGE_SATURATION_LEVEL_BOUNDS = { 50, 80, 110, 150 };
        public static double[] NUM_OF_PEOPLE_BOUNDS = { 1, 3, 5 };
        public static double[] EDGES_BOUNDS = { 10, 30, 70, 120 };
        public static double[] IMAGE_INFORMATION_BOUNDS = { 7.8, 8, 8.5 };
        public static double[] VARIANCE_BOUNDS = { 5, 10, 20, 60 };
        public static double[] STD_BLUR_BOUNDS = { 2 };//<2 blur, >2 not blur
        public static double[] FACE_BLUR_BOUNDS = { 0, 40, 41, 1000 }; //
        public static double[] CLOSED_EYES_BOUNDS = { 0, 1 }; //0-open,1-closed
        public static double[] NUM_OF_SMILES_BOUNDS = { 1, 3, 5 };//num of smiles

        List<int> attributeList;
        List<int> starList;
        int numOfParameters,starLineCount;
        Boolean writeFlag,deleteFlag;
        List<int> currentAttributeList = new List<int>();

        double[][] posK;
        double[][] negK;
        List<int[]> num;
        ImageVector[] subGood;
        ImageVector[] subBad;
        int bestIndex;
        public ClassifierNew()
        {
          
            this.numOfParameters = ImageVector.NUMBER_OF_PARAMETERS;
            this.attributeList = new List<int>();
            this.starList = new List<int>();
            this.starLineCount = 0;
            this.writeFlag = false;
            this.deleteFlag = false;
            this.posK = new double[numOfParameters][];
            this.negK = new double[numOfParameters][];
            this.subGood = null;
            this.subBad = null;
            this.bestIndex = -1;
            this.num = new List<int[]>();
            for(int i =0;i<num.Count;i++)
                this.num[i] = new int[2];
           

        }


        public void initArray(double[] array)
        {
            for (int i = 0; i < numOfParameters; i++)
            {
                array[i] = 0;
            }
        
        }
        /*return the size of bound Parameter array*/
        public int sizeRange(ImageVector.ImageParameters param)
        {
            double[] temp = new double[0];
            getBoundArray(param, ref temp);
            return temp.Length;
        }

        /*calculate the num of positive and negative example for each trribute bound*/
        public double[] calacPositiveNegative(ref double[] attributeRange,double value, ImageVector.ImageParameters param)
        {

           
            double[] ParamBounds = new double[0];
                getBoundArray(param, ref ParamBounds);
            int paramBoundsLength = ParamBounds.Length;
            
            //first bound
            if (value <= ParamBounds[0])
                attributeRange[0] = attributeRange[0] + 1;

            for (int i = 1; i < paramBoundsLength; i++)
            {
                if (value <= ParamBounds[i] && value > ParamBounds[i-1])
                    attributeRange[i] = attributeRange[i] + 1;   
            }
          //last bound
            if (value > ParamBounds[ParamBounds.Length - 1])
                attributeRange[attributeRange.Length - 1] = attributeRange[attributeRange.Length - 1] + 1;

            for (int i = 0; i < attributeRange.Length; i++)
                Console.WriteLine("attributeRange[" + i + "] " + attributeRange[i]);
                /*   for (int k = 0; k < paramBoundsLength; k++)
                   {
                       Console.WriteLine("attributeRange[" + k + "] : " + attributeRange[k]);
                   }
                  */
                return attributeRange;         
        }

        /*Remainder(A) = sum( k=0 to d) of ((pk+nk)/p+n)*B(pk/(pk+nk))
         when attribute A has d distinct valuse divides the traning set
         for example : gray level d = 5 0-80,80-100,100-120,120-150
         in the book: Ptrons: d=3 , none,some,full*/
        public void calcRemainder(ref double remainder, double[] numOfGood, double[] numOfBad, int size)
        {
            for (int i = 0; i < numOfGood.Length; i++) // numOfGood.length = numOfBad.length = sizeOfRange+1
            {
                double q;
                double B;
                double sumGoodBad = (numOfGood[i] + numOfBad[i]);
                if (sumGoodBad == 0)
                    q = 0;
                else
                    q = numOfGood[i] / sumGoodBad;
              //  Console.WriteLine(numOfGood[i] + "  " + numOfBad[i]);
                if (q == 0 || q == 1)
                    B = 0;
                else
                    B = -q * Math.Log(q, 2) - (1 - q) * Math.Log(1 - q, 2);

                remainder += (sumGoodBad / size) * B;

            }            
            
        }

        /*the calculation of Gain give as the information about the Importance Attribute
         * the bigger Gain is the importante the attribute is
         Gain(A) = B(p/(p+n)) - remainder(A)*/
        public void calcGain(ref double gain,double remainder,double numGood, double numBad)
        {
            double q = numGood / (numGood + numBad);
            double B;//entropy
            if(q == 0 || q == 1)
                B = 0;
            else
               B = - (q * Math.Log(q, 2)) - ((1 - q) * Math.Log(1 - q, 2));
          
            for (int i = 0; i < numOfParameters; i++)
            {
                gain = B - remainder;
            }
             
            
        
        }

     /*   public void print(double arr)
        {
            for (int i = 0; i < numOfParameters; i++)
            Console.WriteLine(i + " - "+ arr[i]);
        
        }
        */


        public void nextAtributte(ref double[] attribute, double firstValue, double value, int index, ImageVector.ImageParameters param, Boolean isGood) //index-which param
        {
            double[] ParamBounds = new double[0];
            getBoundArray(param, ref ParamBounds);

            int paramBoundsLength = ParamBounds.Length;
            
            for (int j = 0; j < paramBoundsLength; j++)
            {
                if (firstValue <= ParamBounds[j])
                {
                  
                            if (isGood)
                                calacPositiveNegative(ref attribute, value, param);
                            else
                                calacPositiveNegative(ref attribute, value, param);


                }
                Console.WriteLine("attribute [" + j + "]" + attribute[j]);

            }

        }

        //reutrns the previews star line
        private int readFromTheEnd(string attributePath)
        { 

            string temp = "";
            var lines = File.ReadAllLines(attributePath).Reverse();
            int numOfLines = lines.Count();
            foreach (string line in lines)
            {
                numOfLines--;
                if(!temp.Equals("") && line.Contains("*"))
                {
                    return numOfLines;
                }

                else if (!line.Equals("deleted") && !line.Contains("*"))
                {
                    temp = line;
                }
                
            }
            if (numOfLines == 0)
                return 0;
            return -1;
        
        }

      

        // extract specific lines by bound and attributes from goodImages and badImages files
        private void getIndexFromFile(ref ImageVector[] subGoodImages, ref ImageVector[] subBadImages, string goodPath, string badPath, string attributeFilePath)
        {
            string starLine = "";
            CurrentAttrubuteList(attributeFilePath);

            
            int sizeAttributeFile = File.ReadAllLines(attributeFilePath).Count();
            int lineNum = 0;
            string line,tempLine = "";
            
            System.IO.StreamReader fileAttribute = new System.IO.StreamReader(attributeFilePath);

            List<String> lineNunToDelete = new List<String>();

            List <ImageVector> goodList = new List<ImageVector>();
            List<ImageVector> badList = new List<ImageVector>();
            int attributeNum = -1, boundNum = -1;
            int tempCount = 0;
            string tmpLine = "";
            int prevStarLine = 0;
            int attributeCurrNum=-1;
            if (!writeFlag)//we didnt write
            {
                
               prevStarLine = readFromTheEnd(attributeFilePath);
                if (prevStarLine != -1 && prevStarLine != 0)
                {

               
                    tempCount = starList.IndexOf(prevStarLine + 1);

                    starLine = File.ReadAllLines(attributeFilePath).Skip(prevStarLine).Take(1).First();//go to the last * line
                    do
                    {

                        while ((line = fileAttribute.ReadLine()) != starLine) { tempCount++; }
                      
                        
                        
                    } while (tempCount != prevStarLine);
                    lineNum = tempCount + 1;

                }

                if (prevStarLine != -1 && currentAttributeList.Count != 0)
                {
                    while (attributeList[attributeList.Count - 1] != currentAttributeList[currentAttributeList.Count - 1])
                        attributeList.RemoveAt(attributeList.Count - 1);
                }
                
            }

          

            if (starList.Count > 1 && writeFlag)
            {
                tempCount = 0;
                starLineCount++;
                starLine = File.ReadAllLines(attributeFilePath).Skip(starList[starList.Count - 1]-1).Take(1).First();//go to the last * line
                do
                {
                    while ((line = fileAttribute.ReadLine()) != starLine) ;
                    tempCount++;
                } while (tempCount != starLineCount);         

            }

           
            
            while ((line = fileAttribute.ReadLine()) != null )
            {

                lineNum++;
                Console.WriteLine("getIndexFromFile: line " + line + " numLine: " + lineNum);

                if (line.Contains("*"))
                {
                  /*  if(starList.Count >= 1)
                        starList.Add(lineNum + starList[starList.Count - 1]);
                    else
                        starList.Add(lineNum);*/
                    break;
                    
                } 
                if (line.Contains("deleted"))
                {
                    deleteFlag = true;
                    continue;
                }
                deleteFlag = false;
                //read line from attribute file split by ',' and convert to int array
                string[] temp = line.Split(',');
                int[] attributeInfo = new int[temp.Length];

                for (int i = 0; i < temp.Length ; i++)
                    attributeInfo[i] = Int32.Parse(temp[i]);

                if (attributeNum == -1 && boundNum == -1)
                {
                    attributeNum = attributeInfo[1];
                    boundNum = attributeInfo[2];
                }
                if (attributeNum == attributeInfo[1] && boundNum == attributeInfo[2])
                {

                    if (attributeInfo[attributeInfo.Length - 1] == 1)//good img
                        tempLine = File.ReadAllLines(goodPath).Skip(attributeInfo[0]).Take(1).First();//read one specific line
                    else if (attributeInfo[attributeInfo.Length - 1] == 0)//bad img
                        tempLine = File.ReadAllLines(badPath).Skip(attributeInfo[0]).Take(1).First();//read one specific line
                    
                    lineNunToDelete.Add(line);

                    //split tempLine 
                    string[] imageParams = tempLine.Split(',');
                    double[] parameters = new double[imageParams.Length - 1];
                    for (int i = 0; i < imageParams.Length - 1; i++)//imageParam-1 the last num is for indicating pic is good or bad
                    {
                        parameters[i] = Convert.ToDouble(imageParams[i]);
                       // Console.WriteLine("getIndexFromFile: parameters[i]" + parameters[i]);
                    }

                    if (imageParams[imageParams.Length - 1] == "1")
                    {
                       // subGoodImages[goodIndex++] = new ImageVector(parameters);
                        goodList.Add(new ImageVector(parameters));
                      //  Console.WriteLine("getIndexFromFile: goodIndex " + goodIndex + ",goodList " + goodList[goodIndex]);
                    }
                    else
                    {
                        badList.Add(new ImageVector(parameters));

                      //  subBadImages[badIndex++] = new ImageVector(parameters);
                    }

                }
            
            }

            subGoodImages = goodList.ToArray();
            subBadImages = badList.ToArray();
            goodList.Clear();
            badList.Clear();

            fileAttribute.Close();

            deleteLinesFromFile(attributeFilePath,ref lineNunToDelete);
        }

        private void deleteLinesFromFile(string filePath, ref List<String> list)
        {
            
            string[] text =  File.ReadAllLines(filePath);
            for (int i = 0; i < text.Length;i++ )
                foreach (string line in list)
                {
                    if (line.Equals(text[i]))
                        text[i] = text[i].Replace(line, "deleted");

                }
            File.WriteAllLines(filePath, text);    

        }



        public List<int[]> calcAttributes(ImageVector[] imgGood, ImageVector[] imgBad,
            double[] pk, double[] nk, int attributeIndex, string goodPath, string badPath, string attributePath)
        {
            List<int[]> temp = new List<int[]>();
           temp = bestAttributes( imgGood, imgBad,  pk, nk, attributeIndex, goodPath, badPath, attributePath);
              while (!(deleteFlag && currentAttributeList.Count == 0))  
            {

                temp = bestAttributes(subGood, subBad, posK[bestIndex], negK[bestIndex], bestIndex, goodPath, badPath, attributePath);

            } //while (!(deleteFlag && currentAttributeList.Count == 0));
              return temp;
        }


        public List<int[]> bestAttributes( ImageVector[] imgGood,ImageVector[] imgBad,
             double[] pk, double[] nk, int attributeIndex, string goodPath,string badPath, string attributePath)
        {
            Boolean existFlag = false;
            
            if (attributeList.Count == 1 && num.Count == 0 )
            {
              num.Add(new int[2]);//attributeList[0],0);
              num[0][0] = attributeList[0];
            }


            if (!(deleteFlag && currentAttributeList.Count == 0))
            {
                Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
                int attributePlace = -1;

                //if attribute have changed and not in list
                if (!currentAttributeList.Contains(attributeIndex))//(!attributeList.Contains(attributeIndex))// && writeFlag)
                    attributeList.Add(attributeIndex);

                attributePlace = attributeList.IndexOf(attributeIndex);//currentAttributeList.IndexOf(attributeIndex);//
                Console.WriteLine("attributeIndex: " + attributeIndex);
                Console.WriteLine("attributePlace: " + attributePlace);


                double value;
                String strGood = "";
                String strBad = "";
                //  double[][] posK = new double[numOfParameters][];
                //  double[][] negK = new double[numOfParameters][];
                int tempCount;
                int prevStarLine = 0;
                int lineNum = 0;
                string starLine = "";
                string line = "";
                System.IO.StreamReader fileAttribute;

                if (attributePlace != -1 && attributePlace != 0 && !starList.Contains(prevStarLine + 1) && writeFlag)
                {
                    Console.WriteLine("starList[" + (attributePlace - 1) + "] " + starList[attributePlace - 1]);

                    File.ReadAllLines(attributePath).Skip(starList[attributePlace - 1] - 1).Take(1).First();

                }
                System.IO.StreamWriter file = new System.IO.StreamWriter(attributePath, true);
                // if (new FileInfo(attributePath).Length == 0)
                //     attributeList.Add(attributeIndex);


                int imgGoodIndex = 0;
                int imgBadIndex = 0;

                imgBadIndex = 0;
                imgGoodIndex = 0;
                foreach (double d in pk)
                    Console.WriteLine("pk " + d + " attribute " + convertIndexToImageParameters(attributeIndex));
                writeFlag = false;
                for (; imgGoodIndex < imgGood.Length; imgGoodIndex++)
                {
                    if (attributeList.Count == 2)
                    {
                        for (int i = 0; i < num.Count; i++)
                        {//attribute alrady exist
                            if (num[i][0] == attributeIndex)
                            {
                                existFlag = true;
                                num[i][1]++;
                            }

                        }
                        if (!existFlag)
                        {//not exist
                            num.Add(new int[2]);
                            num[num.Count - 1][0] = attributeIndex;
                            num[num.Count - 1][1]++;
                        }

                    }
                    existFlag = false;

                    value = imgGood[imgGoodIndex].getParameterByIndex(attributeIndex);
                    Console.WriteLine("value: " + value);
                    Console.WriteLine("pk [" + whichBound(value, attributeIndex) + "] = " + pk[whichBound(value, attributeIndex)]
                        + "nk [" + whichBound(value, attributeIndex) + "] = " + nk[whichBound(value, attributeIndex)]);
                    if (nk[whichBound(value, attributeIndex)] == 0)
                        Console.WriteLine("nk=0 " + attributeIndex);

                    if (pk[whichBound(value, attributeIndex)] != 0 && nk[whichBound(value, attributeIndex)] != 0)
                    {
                        writeFlag = true;
                        value = imgGood[imgGoodIndex].getParameterByIndex(attributeIndex);
                        Console.WriteLine("here");
                        strGood = imgGoodIndex + " , " + attributeIndex + " , " + whichBound(value, attributeIndex) + " , 1";
                        Console.WriteLine("strGood " + strGood);
                        Console.WriteLine("attributePlace " + attributePlace);
                        Console.WriteLine("starList.Count() " + starList.Count());

                        lineNum++;
                        file.WriteLine(strGood);//write to file

                    }

                }

                for (; imgBadIndex < imgBad.Length; imgBadIndex++)
                {
                    if (attributeList.Count == 2)
                    {
                        for (int i = 0; i < num.Count; i++)
                        {//attribute alrady exist
                            if (num[i][0] == attributeIndex)
                            {
                                num[i][1]++;
                                existFlag = true;
                            }



                        }
                        if (!existFlag)
                        {//not exist
                            num.Add(new int[2]);
                            num[num.Count - 1][0] = attributeIndex;
                            num[num.Count - 1][1]++;
                        }


                    }

                    value = imgBad[imgBadIndex].getParameterByIndex(attributeIndex);

                    if (pk[whichBound(value, attributeIndex)] != 0 && nk[whichBound(value, attributeIndex)] != 0)
                    {
                        strBad = imgBadIndex + " , " + attributeIndex + " , " + whichBound(value, attributeIndex) + " , 0";
                        writeFlag = true;
                        lineNum++;
                        file.WriteLine(strBad);//write to file
                    }

                }
                imgBadIndex++;
                imgGoodIndex++;


                Console.WriteLine("**********************************************");
                if (writeFlag)
                {
                    file.WriteLine("******");//new attirbute 
                    lineNum++;
                    if (starList.Count >= 1)
                        starList.Add(lineNum + starList[starList.Count - 1]);
                    else
                        starList.Add(lineNum);
                }

                file.Close();


                foreach (int h in attributeList)
                    Console.WriteLine("attribte list " + h);
                getIndexFromFile(ref subGood, ref subBad, goodPath, badPath, attributePath);

                for (int i = 0; i < ImageVector.NUMBER_OF_PARAMETERS; i++)
                {

                    if (!attributeList.Contains(i))
                    {

                        posK[i] = new double[sizeRange(ImageVector.getParameterNameByIndex(i)) + 1];
                        negK[i] = new double[sizeRange(ImageVector.getParameterNameByIndex(i)) + 1];


                        Console.WriteLine("posK length " + posK[i].Length + " negK length " + negK[i].Length);
                        //     Console.WriteLine("subGood.Length" + subGood.Length + "subGood[subGood.Length - 1]" + subGood[subGood.Length - 1]);
                        for (int j = 0; j < subGood.Length; j++)
                        {

                            //         Console.WriteLine("subGood" + subGood[j]+"subGood End");
                            //         Console.WriteLine("subGood[j].getParameterByIndex(i)" + subGood[j].getParameterByIndex(i));
                            calacPositiveNegative(ref posK[i], subGood[j].getParameterByIndex(i), convertIndexToImageParameters(i));
                        }
                        for (int j = 0; j < subBad.Length; j++)
                            calacPositiveNegative(ref negK[i], subBad[j].getParameterByIndex(i), convertIndexToImageParameters(i));
                    }
                }

                double[] remainder = new double[ImageVector.NUMBER_OF_PARAMETERS];
                int size = subBad.Length + subGood.Length;
                //   posK = new double[subGood.Length];
                // negK = new double[subBad.Length];

                double max = 1;
                for (int i = 0; i < ImageVector.NUMBER_OF_PARAMETERS; i++)
                {
                    if (!attributeList.Contains(i))
                    {
                        calcRemainder(ref remainder[i], posK[i], negK[i], size);
                        //  Console.WriteLine("remainder["+i+"] " +remainder[i]);

                        if (max >= remainder[i])
                        {
                            max = remainder[i];
                            bestIndex = i;

                        }
                    }

                }
                file.Close();
            }
            else
            {
                //    Console.WriteLine("max: " + max + " index: " + bestIndex);
                //     bestAttributes(subGood, subBad, posK[bestIndex], negK[bestIndex], bestIndex, goodPath, badPath, attributePath);
                Console.WriteLine("%&%&%&%&%&%&%&%&%&%&%");
                for (int i = 0; i < num.Count; i++)
                    Console.WriteLine(num[i][0] + " : " + num[i][1]);
                Console.WriteLine("%&%&%&%&%&%&%&%&%&%&%");
              
                  /*  Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!");
                    int[] te = bestAtributeByOrder();
                    for (int i = 0; i < te.Length; i++)
                        Console.WriteLine(te[i]);
                */
            }
            return num;

        }
        //return an array with the best attribute list by order of importanse
        public int[] bestAtributeByOrder(List<int[]> n)
        {
            int[] best = new int[n.Count];
            int max = 0;
            int maxAt = 0;
            int maxIndex = -1;
            best[0] = n[0][0];
            for (int j = 1; j < best.Length ; j++)
            {
                max = 0;
                for (int i = 1; i < n.Count ; i++)
                {
                    if (n[i][1] > max)
                    {
                        max = n[i][1];
                        maxAt = n[i][0];
                        maxIndex = i;
                    }
                }
                n.Remove(n[maxIndex]);
                    best[j] = maxAt;
            }
        /*    Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
            for (int m = 0; m < best.Length; m++)
                Console.WriteLine(best[m]);*/
                return best;
        }

        // the attribute that now in the tree (file)
        public void CurrentAttrubuteList(String path)
        {
            string[] lines;
            lines = File.ReadAllLines(path);
            currentAttributeList.Clear();
            foreach (string line in lines)
            {
                string[] temp = line.Split(',');
                if (line.Contains("*") || line.Equals("deleted"))
                    continue;
                if(!currentAttributeList.Contains(Int32.Parse(temp[1])))
                    currentAttributeList.Add(Int32.Parse(temp[1]));
            }
        }


        private int whichBound(double value, int Attributeindex)
        {
            int index = -1;
            double[] ParamBounds = new double[0];
            getBoundArray(convertIndexToImageParameters(Attributeindex), ref ParamBounds);
            Console.WriteLine("convertIndexToImageParameters(Attributeindex) " + convertIndexToImageParameters(Attributeindex).ToString());
            int paramBoundsLength = ParamBounds.Length;
            //first bound
            if (value <= ParamBounds[0])
                    index = 0;
            for (int i = 1; i < paramBoundsLength; i++)
            {
                if (value <= ParamBounds[i] && value > ParamBounds[i-1])
                    index = i;   
            }

          //last bound
            if (value > ParamBounds[paramBoundsLength - 1])
                index = paramBoundsLength ;
            Console.WriteLine("index: " + index + "paramBoundsLength " + paramBoundsLength);
          
            return index;
        }

        private ImageVector.ImageParameters convertIndexToImageParameters(int index)
        {
            switch (index)
            {
             /*   case 0:
                   return ImageVector.ImageParameters.averageGrayLevel;
               */ case 0:
                   return ImageVector.ImageParameters.numOfPoeple;
                case 1:
                    return ImageVector.ImageParameters.edges;
                case 2:
                    return ImageVector.ImageParameters.facesCenterOfGravityX;
          /*      case 4:
                    return ImageVector.ImageParameters.facesCenterOfGravityY;
            */    case 3:
                    return ImageVector.ImageParameters.facesImageAreaRatio;
             /*   case 6:
                    return ImageVector.ImageParameters.averageRedLevel;
              */  case 4:
                    return ImageVector.ImageParameters.averageBlueLevel;
             /*   case 8:
                    return ImageVector.ImageParameters.averageGreenLevel;
                case 9:
                    return ImageVector.ImageParameters.averageHueLevel;
              */  case 5:
                    return ImageVector.ImageParameters.averageSaturationLevel;
            /*    case 11:
                    return ImageVector.ImageParameters.distanceFromGravityCenter;
                case 12:
                    return ImageVector.ImageParameters.imageInformation;
              */  case 6:
                    return ImageVector.ImageParameters.variance;
             /*   case 14:
                    return ImageVector.ImageParameters.redEye;
                case 15:
                    return ImageVector.ImageParameters.stdBlur; //!!!!!!!!
                case 16:
                    return ImageVector.ImageParameters.faceBlur; //!!!!!!!
                case 17:
                    return ImageVector.ImageParameters.closedEye;//!!!!!!!
                case 18:
                    return ImageVector.ImageParameters.numOfSmiles; //!!!!!!!!
               */ default:
                    throw (new Exception("Parameter " + index + " does not exist."));
            }
        }


        /* Get classification bounds array of given parameter*/
        public static void getBoundArray(ImageVector.ImageParameters param, ref double[] array)
        {
            array = null;
            switch (param)
            {
             /*   case ImageVector.ImageParameters.averageGrayLevel:
                    array = AVERAGE_GRAY_LEVEL_BOUNDS;
                    break;
                case ImageVector.ImageParameters.averageGreenLevel:
                    array = AVERAGE_GREEN_LEVEL_BOUNDS;
                    break;
                case ImageVector.ImageParameters.averageRedLevel:
                    array = AVERAGE_RED_LEVEL_BOUNDS;
                    break;
            */    case ImageVector.ImageParameters.averageBlueLevel:
                    array = AVERAGE_BLUE_LEVEL_BOUNDS;
                    break;
             /*   case ImageVector.ImageParameters.averageHueLevel:
                    array = AVERAGE_HUE_LEVEL_BOUNDS;
                    break;
              */  case ImageVector.ImageParameters.averageSaturationLevel:
                    array = AVERAGE_SATURATION_LEVEL_BOUNDS;
                    break;
                case ImageVector.ImageParameters.numOfPoeple:
                    array = NUM_OF_PEOPLE_BOUNDS;
                    break;
                case ImageVector.ImageParameters.edges:
                    array = EDGES_BOUNDS;
                    break;
           /*    case ImageVector.ImageParameters.redEye:
                    array = RED_EYE_BOUNDS;
                    break;
                case ImageVector.ImageParameters.distanceFromGravityCenter:
                    array = DISTANCE_FROM_COG_BOUNDS;
                    break;
              */  case ImageVector.ImageParameters.facesImageAreaRatio:
                    array = IMAGE_FACES_AREA_RATIO_BOUNDS;
                    break;
                case ImageVector.ImageParameters.facesCenterOfGravityX:
                    array = CENTER_OF_GRAVITY_BOUNDS;
                    break;
             /*   case ImageVector.ImageParameters.facesCenterOfGravityY:
                    array = CENTER_OF_GRAVITY_BOUNDS;
                    break;
              */  case ImageVector.ImageParameters.variance:
                    array = VARIANCE_BOUNDS;
                    break;
             /*   case ImageVector.ImageParameters.imageInformation:
                    array = IMAGE_INFORMATION_BOUNDS;
                    break;
                case ImageVector.ImageParameters.stdBlur:
                    array = STD_BLUR_BOUNDS;
                    break;
                case ImageVector.ImageParameters.faceBlur: //!!!!!!!
                    array = FACE_BLUR_BOUNDS;
                    break;
                case ImageVector.ImageParameters.closedEye: //!!!!!!!
                    array = FACE_BLUR_BOUNDS;
                    break;
                case ImageVector.ImageParameters.numOfSmiles: //!!!!!!!
                    array = NUM_OF_SMILES_BOUNDS;
                    break;
               */ default:
                    throw (new Exception("Classification for " + param.ToString() + " is not implemented"));
            }
        }
    }
}
