using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SPF
{
    static class Classifier
    {
        // Bounds used for classifing each parameter
        public static double[] CENTER_OF_GRAVITY_BOUNDS = { 0, 33, 66 };
        public static double[] DISTANCE_FROM_COG_BOUNDS = { 0, 15, 50, 100, 200 };
        public static double[] IMAGE_FACES_AREA_RATIO_BOUNDS = { 0.0001, 0.1, 0.3, 0.6, 1 };
        public static double[] RED_EYE_BOUNDS = { 0.5 };
        public static double[] AVERAGE_GRAY_LEVEL_BOUNDS = { 80,100,120, 150 };
        public static double[] AVERAGE_RED_LEVEL_BOUNDS = { 80,100,120, 160 };
        public static double[] AVERAGE_GREEN_LEVEL_BOUNDS = { 80, 110, 150 };
        public static double[] AVERAGE_BLUE_LEVEL_BOUNDS = { 80, 110,150 };
        public static double[] AVERAGE_HUE_LEVEL_BOUNDS = { 30, 50, 80 };
        public static double[] AVERAGE_SATURATION_LEVEL_BOUNDS = { 50,80,110, 150 };
        public static double[] NUM_OF_PEOPLE_BOUNDS = { 1, 3, 5 };
        public static double[] EDGES_BOUNDS = { 10, 30, 70, 120 };
        public static double[] IMAGE_INFORMATION_BOUNDS = { 7.8, 8, 8.5 };
        public static double[] VARIANCE_BOUNDS = { 5, 10, 20, 60 };

        public static double[] order_remainder = new double[ImageVector.NUMBER_OF_PARAMETERS];
        public static double[] remainder = new double[ImageVector.NUMBER_OF_PARAMETERS];
        //public static double[] user_rating;
        

        /* Classifies two lists of image vectors */
        public static void Classify(List<ImageVector> vectorsTrue, List<ImageVector> vectorsFalse,
            out string[] cVectorsTrue, out string[] cVectorsFalse)
        {
            cVectorsTrue = new string[vectorsTrue.Count];
            cVectorsFalse = new string[vectorsFalse.Count];
            //user_rating = new double[cVectorsTrue.Count()];
            int size = ImageVector.NUMBER_OF_PARAMETERS;
            double[] how_much_pictures = new double[size * 2];
            double[] how_much_good = new double[size * 2];

           
            for (int i = 0; i < size * 2; i++)
            {
                how_much_good[i] = 0;
                how_much_pictures[i] = 0;
                if (i < ImageVector.NUMBER_OF_PARAMETERS)
                    remainder[i] = 0;
            }
            for (int i = 0; i < vectorsTrue.Count; i++)
                cVectorsTrue[i] = ClassifyVector(vectorsTrue[i], ref how_much_pictures, ref how_much_good, true);
                //cVectorsTrue[i] = ClassifyVector(vectorsTrue[i]);
            for (int i = 0; i < vectorsFalse.Count; i++)
                cVectorsFalse[i] = ClassifyVector(vectorsFalse[i], ref how_much_pictures, ref how_much_good, false);
                //cVectorsFalse[i] = ClassifyVector(vectorsFalse[i]);

        
       //     DecisionTree.calc_remainder(how_much_pictures, how_much_good, cVectorsTrue.Length, cVectorsFalse.Length, ref remainder);
            //remainder(cVectorsTrue, cVectorsFalse, ImageVector.getParameterNameByIndex);
        }

        /* Calssifies one given image vector */
        public static string ClassifyVector(ImageVector vector, ref double[] how_much_pictures, ref double[] how_much_good, bool how_is_picture)
        {
            bool paramExist;
            

            string result = String.Empty;

            for (int j = 0; j < ImageVector.NUMBER_OF_PARAMETERS; j++)
            {
                vector.DicParameter.TryGetValue(ImageVector.getParameterNameByIndex(j) , out paramExist);
                if (paramExist)
                {
                    result += getParameterClassification(ImageVector.getParameterNameByIndex(j), vector.getParameterByIndex(j));
                    Preparation_remainder(j, ImageVector.getParameterNameByIndex(j), how_is_picture, vector.getParameterByIndex(j), ref  how_much_pictures, ref how_much_good);
                }
            }


            //System.Windows.Forms.MessageBox.Show("classify " + result);
            return result;
        }

        /* Calssifies one given image vector
         * the old function for testing only*/
        public static string ClassifyVector(ImageVector vector)
        {
            bool paramExist;
            string result = String.Empty;

            for (int i = 0; i < ImageVector.NUMBER_OF_PARAMETERS; i++)
            {
                vector.DicParameter.TryGetValue(ImageVector.getParameterNameByIndex(i), out paramExist);
                if (paramExist)
                    result += getParameterClassification(ImageVector.getParameterNameByIndex(i), vector.getParameterByIndex(i));
            }

            return result;
        }

        /* Classifies one parameter with a given value */
        public static char getParameterClassification(ImageVector.ImageParameters param, double value)
        {
            double[] bounds = new double[0];
            getBoundArray(param, ref bounds);
            return ClassifyValueByBounds(value, bounds);
        }

        /* Get classification bounds array of given parameter */
        public static void getBoundArray(ImageVector.ImageParameters param, ref double[] array)
        {
            array = null;
            switch (param)
            {
         /*       case ImageVector.ImageParameters.averageGrayLevel:
                    array = AVERAGE_GRAY_LEVEL_BOUNDS;
                    break;
              case ImageVector.ImageParameters.averageGreenLevel:
                    array = AVERAGE_GREEN_LEVEL_BOUNDS;
                    break;
                case ImageVector.ImageParameters.averageRedLevel:
                    array = AVERAGE_RED_LEVEL_BOUNDS;
                    break;
       */         case ImageVector.ImageParameters.averageBlueLevel:
                    array = AVERAGE_BLUE_LEVEL_BOUNDS;
                    break;
        /*      case ImageVector.ImageParameters.averageHueLevel:
                    array = AVERAGE_HUE_LEVEL_BOUNDS;
                    break;
          */     case ImageVector.ImageParameters.averageSaturationLevel:
                    array = AVERAGE_SATURATION_LEVEL_BOUNDS;
                    break;
                case ImageVector.ImageParameters.numOfPoeple:
                    array = NUM_OF_PEOPLE_BOUNDS;
                    break;
                        case ImageVector.ImageParameters.edges:
                               array = EDGES_BOUNDS;
                               break;
                         /*               case ImageVector.ImageParameters.redEye:
                                           array = RED_EYE_BOUNDS;
                                           break;
                                       case ImageVector.ImageParameters.distanceFromGravityCenter:
                                           array = DISTANCE_FROM_COG_BOUNDS;
                                           break;
                                */       case ImageVector.ImageParameters.facesImageAreaRatio:
                                           array = IMAGE_FACES_AREA_RATIO_BOUNDS;
                                           break;
                                      case ImageVector.ImageParameters.facesCenterOfGravityX:
                                           array = CENTER_OF_GRAVITY_BOUNDS;
                                           break;
                             /*          case ImageVector.ImageParameters.facesCenterOfGravityY:
                                           array = CENTER_OF_GRAVITY_BOUNDS;
                                           break;
                              */         case ImageVector.ImageParameters.variance:
                    array = VARIANCE_BOUNDS;
                    break;
         /*       case ImageVector.ImageParameters.imageInformation:
                    array = IMAGE_INFORMATION_BOUNDS;
                    break;
           */     default:
                    throw (new Exception("Classification for " + param.ToString() + " is not implemented"));
            }
        }

        /* Return a char representing classification by given bounds. Starting from 'a' and increasing with each bound       */
        /* Values smaller than the first bound in array are represented with char 'a'. Values bigger or equal to first bound */
        /* but smaller than the second will be represented with char 'b' and so on. Last char representation are values      */
        /* bigger or equal to the last bound on array                                                                        */
        private static char ClassifyValueByBounds(double value, double[] bounds)
        {
            const int CHR_A = 97;    // 'a' 
            int N = bounds.Length;

            // Bounds not given
            if (N == 0)
                throw (new Exception("Bounds for classficiation are missing"));
     
            // Classify
            for (int i = 0; i < N; i++)
            {
                if (value < bounds[i])
                    return Convert.ToChar(CHR_A + i);
                //System.Windows.Forms.MessageBox.Show(ImageVector.getParameterNameByIndex(i) + bounds[i].ToString());
            }
            return (Convert.ToChar(CHR_A + N));  // Higest classification
        }
        
        /* function that will help to calculate the remainder
         * how_much_picture array contain the number of good picture in the i's place and the number of bad pictures in the (i+1) palce for every parameter
         * how_much_good array contain the number of true values for parameter i in cell i and the number of false values for parameter i in cell i+1*/
        public static void Preparation_remainder(int i, ImageVector.ImageParameters param, bool how_is_picture, double value, ref double[] how_much_pictures, ref double[] how_much_good)
       {
            i *= 2;
            double[] bounds = new double[0];
            getBoundArray(param, ref bounds);
            int N = bounds.Length;
            int k = 0;
            

            // Bounds not given
            if (N == 0)
                throw (new Exception("Bounds for classficiation are missing"));

            for (int j = 0; j < N; j++)
            {
                if (value < bounds[j])
                    k++;
            }

            if (k >= (N / 2))
                how_much_good[i]=how_much_good[i] + 1;
            else
                how_much_good[i + 1]= how_much_good[i+1]+1;

            
            if (how_is_picture)//picture is good
                how_much_pictures[i]=how_much_pictures[i]+1; //adding one good picture to parameter place
            else
                how_much_pictures[i + 1]=how_much_pictures[i+1]+1;//adding one bad picture to parameter place

            //System.Windows.Forms.MessageBox.Show("many pictures: " + how_much_pictures[i].ToString() + "many good: " + how_much_good[i].ToString());

       }//eof Preparation_remainder


    }

     
}
