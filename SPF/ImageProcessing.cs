using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Drawing;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;

using System.IO;
using System.Xml;
using System.Windows.Forms;
namespace SPF
{
    static class ImageProcessing
    {

        #region Private Members

        // Loaded image
        private static Image<Bgr, byte> _imageRGB;
        private static Image<Gray, byte> _imageGray;
        private static Image<Hsv, byte> _imageHSV;
        private static Image<Gray, byte>[] _channels;


        //histogram
        private static int[] _histogram;

        // Objects found in current image
        private static Rectangle[] _faces;
        private static Rectangle[] _eyes;
        private static Rectangle[] _smiles;

        // HaarObjects
        //private static HaarCascade _haarEye;
        //private static HaarCascade _haarFace;
        //private static MCvAvgComp[] _objects;

        //Cascade Classifier Object
        private static CascadeClassifier _cascadeFace;
        private static CascadeClassifier _cascadeEyes;
        private static CascadeClassifier _cascadeSmile;

        #endregion

        #region Public Methods

        // Loading new image in RGB, HSV and Gray. Initializing image related class members
        public static bool LoadImage(string imagePath)
        {
            DeallocateImageMemory();
                    
            _imageRGB = new Image<Bgr, byte>(imagePath);    //Read the files as an 8-bit RGB image
            _imageHSV = new Image<Hsv, byte>(imagePath);   //Read the files as an 8-bit HSV image
            _imageGray = new Image<Gray, byte>(imagePath);  //Read the files as an 8-bit gray image
            
            //!!!!!!!!!!!!!!
            Console.WriteLine("path: " + imagePath);
           // _imageGray.Save("C:\\img\\realpic.JPG");
           // MCvScalar sum = CvInvoke.cvSum(_imageGray);
           // int graySum = (int)(sum.v0);// (_imageGray.Rows * _imageGray.Cols));
           // Console.WriteLine("sum real: " + graySum);


            getHistogram();
       /*     try
            {כ
                //display the image 
                 ImageViewer viewer = new ImageViewer(); //create an image viewer
                 viewer.Image = _imageRGB;
                 viewer.Show();//show the image viewer
            }
            catch (Exception)
            {
                
                throw;
            }
           
        */
            _faces = null;
            _eyes = null;
            _smiles = null;

            return true;
        }


        // Calculating avrage gray level
        public static double CalcAverageGrayLevel()
        {
            return _imageGray.GetAverage().Intensity;
        }

        // Calculating distance from the center of gravity in the loaded image, and returning the length by image 
        // length (mesured by main diagonal) percentage.
        public static int CalcTotalDistanceFromCenterOfGravity()
        {
            double centerOfGravityX;
            double centerOfGravityY;
            calcFacesCenterOfGravityByPercentage(out centerOfGravityX, out centerOfGravityY);

            // If no faces -> no center -> no distance
            if ((centerOfGravityX == -1) && (centerOfGravityY == -1))
                return -1;

            // Calc centers of faces
            int numberOfFaces = _faces.Length;
            Point[] centers = new Point[numberOfFaces];
            for (int i = 0; i < numberOfFaces; i++)
                centers[i] = new Point(_faces[i].X + (_faces[i].Width / 2), _faces[i].Y + (_faces[i].Height / 2));

            // Convert to percentage
            int imgWidth = _imageRGB.Width;
            int imgHeight = _imageRGB.Height;
            for (int i = 0; i < numberOfFaces; i++)
            {
                centers[i].X = ((centers[i].X *100)/ imgWidth);
                centers[i].Y = ((centers[i].Y *100)/ imgHeight); 
            }

            // Sum distance
            int sum = 0;
            for (int i = 0; i < centers.Length; i++)
                sum += (int)Math.Sqrt((Math.Pow(centerOfGravityX - centers[i].X, 2)) + (Math.Pow(centerOfGravityY - centers[i].Y,2)));

            return sum;
        }

        // Return the center of gravity value by percentage (reffering to X and Y axis sepratly)
        public static bool calcFacesCenterOfGravityByPercentage(out double x, out double y)
        {
            int width = _imageRGB.Width;
            int height = _imageRGB.Height;

            Point center = calcFacesCenterOfGravity();
            if ((center.X == -1) && (center.Y == -1))
            {
                x = -1;
                y = -1;
                return false;
            }
            else
            {
                x = (int)(center.X *100/ width);
                y = (int)(center.Y *100/ height);
                return true;
            }
        }

        // Calculating ratio between faces area in the image and the whole image area.
        public static double calcFacesImageAreaRatio()
        {
            // Find faces in the image if not yet found
            if (_faces == null)
                findFacesCascade();

            // Calc total faces area
            double facesArea = 0;
            for (int i = 0; i < _faces.Length; i++)
                facesArea += (_faces[i].Width * _faces[i].Height);

            // Get image area
            double imageArea = _imageRGB.Width * _imageRGB.Height;

            // return ratio
            return (facesArea / imageArea);
        }

        // Counting number of faces in the image
        public static int calcNumOfPeople()
        {
            if (_faces == null)
                findFacesCascade();
               
            //Console.WriteLine("num of people: " + _faces.Length);
            // return num of faces
            return _faces.Length;
        }

        // Checking if red-eye exisits in the loaded image
        public static double isRedEye()
        {
            _eyes = null;
            findEyesCascade();


            const double EYE_IMAGE_MARGIN_PERCENTAGE = 20.0;  // Margin in eye rectangle that redEye won't be searched
            const int DIVIDE_TO = 4;                          // Division to blocks for each dimention
            const int SUM_MULTIPLAYER = 1;                    // Multiplayer for condition

            const int HUE_MIN_RED_THRESHOLD = 336; //353
            const int HUE_MAX_RED_THRESHOLD = 13;  //12
            const double MIN_SATURATION_RED_THRESHOLD = 0.56; //0.56
            const double MAX_BRIGHTNESS_RED_THRESHOLD = 0.62; //0.62

            Rectangle ROI;                                    // Rectangle of the serached region.
            int marginX, marginY;                             // Margin length for X and Y axis
            int[,] cubes = new int[DIVIDE_TO, DIVIDE_TO];     // Matrix holding each block sum 
            int total, current;                               // Indicators
            Color c;                                          // Pixel color

            int max=0;

            // Find eyes if not yet found
            if (_eyes == null)
                findEyesCascade();
            /*string file="C:/Users/Roe/Desktop/log1.txt";
            TextWriter log = new StreamWriter(file);
            log.WriteLine("55");
            //TextWriter log = new StreamWriter("C:/Users/Roe/Desktop/log.txt");
            log.WriteLine("");
            log.WriteLine(_eyes.Length);
            log.Close();*/
            //Write(log);

            // Look for redEye in each eye found
            for (int i = 0; i < _eyes.Length-1; i++)
            {
                total = 0;

                // Initialize matrix
                for (int x = 0; x < DIVIDE_TO; x++)
                    for (int y = 0; y < DIVIDE_TO; y++)
                        cubes[x, y] = 0;

                // Crop center of the image
                marginX = (int)(EYE_IMAGE_MARGIN_PERCENTAGE / 100 * _eyes[i].Width);
                marginY = (int)(EYE_IMAGE_MARGIN_PERCENTAGE / 100 * _eyes[i].Height);
                ROI = new Rectangle(_eyes[i].X + marginX, _eyes[i].Y + marginY, _eyes[i].Width - marginX, _eyes[i].Height - marginY);
                Bitmap eye = _imageRGB.Copy(ROI).ToBitmap();
                int cWidth = ((eye.Width % DIVIDE_TO) == 0) ? eye.Width / DIVIDE_TO : (eye.Width / DIVIDE_TO) + 1;
                int cHeight = ((eye.Height % DIVIDE_TO) == 0) ? eye.Height / DIVIDE_TO : (eye.Height / DIVIDE_TO) + 1;

                // Count 'red' pixels
                for (int x = 0; x < eye.Width; x++)
                    for (int y = 0; y < eye.Height; y++)
                    {
                        // Check if passing threshold
                        c = eye.GetPixel(x, y);
                        if (((c.GetHue() <= HUE_MAX_RED_THRESHOLD) || (c.GetHue() >= HUE_MIN_RED_THRESHOLD)) &&
                            (c.GetSaturation() > MIN_SATURATION_RED_THRESHOLD) && (c.GetBrightness() < MAX_BRIGHTNESS_RED_THRESHOLD))
                        {
                            cubes[x / cWidth, y / cHeight]++;
                            total++;
                        }
                    }

                // Look for spot
                /*for (int x = 0; x < DIVIDE_TO - 1; x++)
                    for (int y = 0; y < DIVIDE_TO - 1; y++)
                    {
                        // Check (current 4 Neighbor blocks sum):(rest of the blocks sum) ratio
                        current = cubes[x, y] + cubes[x + 1, y] + cubes[x, y + 1] + cubes[x + 1, y + 1];
                        if (current > SUM_MULTIPLAYER * (total - current))
                            return 1;
                    }*/

                for (int x = 0; x < DIVIDE_TO - 1; x++)
                {
                    for (int y = 0; y < DIVIDE_TO - 1; y++)
                    {
                        // Check (current 4 Neighbor blocks sum):(rest of the blocks sum) ratio
                        current = cubes[x, y] + cubes[x + 1, y] + cubes[x, y + 1] + cubes[x + 1, y + 1];
                        if (current > SUM_MULTIPLAYER * (total - current))
                            max+=1;
                    }
                    
                }
                return max;

            }
            return 0;
        }
      
        //!!!!!!!!!!!!!!!
        //blur parameter

        private static double calcBlur(Image<Gray,float> img)
        {
            //calc standart diviation and return it
            List<double> sumList = cropImg(img);
            return calcSD(sumList);
        }

        public static double blur()
        {
            return calcBlur(_imageGray.Convert<Gray, float>());
        }

        //!!!!!!!!!!!!!!!!!!!
        public static double faceBlur()
        {
            //List<double> laplaceList = new List<double>();
            Image<Gray, float> imgCrop = null;

            int count = 0;
            //double harmonic = -1;
            List<double> stdList = new List<double>();

            // Find faces if not found yet
            if (_faces == null)
                findFacesCascade();

            foreach (Rectangle currentFace in _faces)
            {
                // Crop face
                // Image<Gray, byte> cropped = _imageGray.Copy(currentFace);
                imgCrop = _imageGray.Convert<Gray, float>().Copy(currentFace);
                //imgCrop.Save("C:\\img\\face" + count + ".JPG");
                //Console.WriteLine("cropedPic " + count );

                //calculate laplacian
                //Console.WriteLine("lap face: " + calcLaplacian(imgCrop));
                //laplaceList.Add(calcLaplacian(imgCrop));

                //like blur parameter takes face and cut 16X16
                stdList.Add(calcBlur(imgCrop));
                //List<double> sumList = cropImg(imgCrop);
                //Console.WriteLine("blur - sd: " + calcSD(sumList));
                count++;
            }

            /*if (laplaceList.Count != 0)
            {
                harmonic = calcHarmonic(laplaceList);
                Console.WriteLine("harmonic: " + harmonic);
                Console.WriteLine("max: " + calcMax(laplaceList));
            }*/

            //dealllocate images
            if (imgCrop != null)
            {
                imgCrop.Dispose();
            }

            imgCrop = null;

            GC.Collect();


            /*!!!!!!!!!!if(stdList.Count != 0 && calcMaxStd(stdList) <= 40)
            {
                return 1; //face is blury - true
            }*/

            /* foreach (double lap in laplaceList)
             {
                 if (lap <= 30)
                     return 1;  //face is blury
             }*/

            //return harmonic; //face is not blury

            //return 0; //face is not blury - false
            if (stdList.Count() != 0)
                return calcMaxStd(stdList);

            return 41;
        }
        
        //!!!!!!!!!!!!!!!!!!!!
        /* calculating harmonic avg = 4th square[(sum(Xi ^ 4)) / n]  */
       /* private static double calcHarmonic(List<double> laplaceList)
        {
            int numOfFaces = laplaceList.Count; //the number of 
            Console.WriteLine("num faces" + numOfFaces);

            List<double> tempList = new List<double>();

            //Xi ^ 4
            foreach (double d in laplaceList)
                tempList.Add(Math.Pow(d, 4));


            double sqrNum = tempList.Sum() / numOfFaces;
            //Console.WriteLine("sqrNum" + sqrNum);

            return Math.Pow(sqrNum, 0.25); //4th square root
        }*/


        //!!!!!!!!!!!!!!!!!!
        /* get max number from list*/
      /*  private static double calcMaxLap(List<double> laplaceList)
        {
            return laplaceList.Max();
        }*/

        private static double calcMaxStd(List<double> stdList)
        {
            return stdList.Max();
        }


        //!!!!!!!!!!!!!!!!
       /* public static void shadedFace()
        {
            Image<Gray, float> imgCrop = null;

            // Find faces if not found yet
            if (_faces == null)
                findFacesCascade();

            int count = 0;
            List <double> avgGrayFace = new List<double>();
            double avgGrayEntirePic = _imageGray.GetAverage().Intensity;
            Console.WriteLine("avg entire pic: " + avgGrayEntirePic);

            foreach (Rectangle currentFace in _faces)
            {
                // Crop face
                imgCrop = _imageGray.Convert<Gray, float>().Copy(currentFace);
                imgCrop.Save("C:\\img\\cropedPic" + count + ".JPG");
                avgGrayFace.Add(imgCrop.GetAverage().Intensity);

                Console.WriteLine("avg face " + count + " : " + imgCrop.GetAverage().Intensity);

                count++;
            }

            //dealllocate images
            if (imgCrop != null)
            {
                imgCrop.Dispose();
            }

            imgCrop = null;

            GC.Collect();

        }*/

       /* public static int isClosedEyesTry()
        {
            const double EYE_IMAGE_MARGIN_PERCENTAGE = 30.0;  // Margin in eye rectangle that redEye won't be searched

            Image<Gray, Byte> imgCrop = null;
            int numOfIris = 0;

            int countEyeNum = 0;
            int count = 0;

            Rectangle ROI;                                    // Rectangle of the serached region.
            int marginX, marginY;                             // Margin length for X and Y axis


            // Find eyes if not found yet
            if (_eyes == null)
                findEyesCascade();
           
            foreach (Rectangle currentEye in _eyes)
            {
                // Crop face
                // Image<Gray, byte> cropped = _imageGray.Copy(currentFace);

                Image<Gray, float> imgEye = _imageGray.Convert<Gray, float>().Copy(currentEye);
                imgEye.Save("C:\\img\\eye" + count + ".JPG");

                //crop image so we remove eyebrow
                marginX = (int)(EYE_IMAGE_MARGIN_PERCENTAGE / 100 * currentEye.Width);
                marginY = (int)(EYE_IMAGE_MARGIN_PERCENTAGE / 100 * currentEye.Height);
                ROI = new Rectangle(currentEye.X + marginX, currentEye.Y + marginY, currentEye.Width - marginX, currentEye.Height - marginY);
                imgCrop = _imageGray.Convert<Gray, Byte>().Copy(ROI);
                
                imgCrop.Save("C:\\img\\cropedPic" + count + ".JPG");
                Console.WriteLine("path: C:\\img\\cropedPic" + count + ".JPG");             
                
                if (imgCrop.Width < 20 && imgCrop.Height < 20)
                    continue;

                Image<Bgr, byte> imgColor = _imageRGB.Copy(ROI);
                imgColor.Save("C:\\img\\color" + count + ".JPG");
                imgColor._Not();
                imgColor.Save("C:\\img\\invert" + count + ".JPG");

                Bitmap b = imgColor.ToBitmap();
                Image<Gray, byte> imgGray = new Image<Gray, byte>(b);
                imgGray.Save("C:\\img\\gray" + count + ".JPG");

               // imgCrop._Not();
               // imgCrop.Save("C:\\img\\gray" + count + ".JPG");

                //image normalization
                imgGray = imgGray.ThresholdBinary(new Gray(220), new Gray(255));
                imgGray.Save("C:\\img\\binary" + count + ".JPG");

                /*imgCrop = imgCrop.Canny(100, 200);
                imgCrop.Save("C:\\img\\canny" + count + ".JPG");*/
                

               /* //invert
                imgCrop._Not();
                imgCrop.Save("C:\\img\\invert" + count + ".JPG");*/

               /* using (MemStorage storage = new MemStorage())
                {
                   // Console.WriteLine("in using");
                    for(Contour<Point> contours = imgGray.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_TREE, storage); contours != null; contours = contours.HNext)
                    {
                        //Console.WriteLine("in for");
                        Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.015, storage);
                        if (currentContour.BoundingRectangle.Width > 20)
                        {
                           // Console.WriteLine("in if");
                            CvInvoke.cvDrawContours(imgGray, contours, new MCvScalar(255), new MCvScalar(255), -1, 2, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, new Point(0,0));
                       
                        }
                   
                    }
                }
                imgGray.Save("C:\\img\\contours" + count + ".JPG");
                
                numOfIris += detectIris(imgCrop, count);

                count++;
            }
            return 0;
        }*/



        /*detect if eyes are closed or open*/
        public static int isClosedEyes()
        {
            const double EYE_IMAGE_MARGIN_PERCENTAGE = 30.0;  // Margin in eye rectangle

            Image<Gray, Byte> imgCrop = null;
            int numOfIris = 0;
            int minEyeWidth = 32, minEyeHeight = 20;

            int countEyeNum = 0;
            int count = 0;

            Rectangle ROI;                                    // Rectangle of the serached region.
            int marginX, marginY;                             // Margin length for X and Y axis
            

            // Find eyes if not found yet
            if (_eyes == null)
                findEyesCascade();
            /*
            // Look for closed eye in each found eye
            for (int i = 0; i < _eyes.Length - 1; i++)
            {
                Image<Gray,float> imgEye = _imageGray.Convert<Gray, float>().Copy(_eyes[i]);
                imgEye.Save("C:\\img\\eye" + count + ".JPG");

                // Crop center of the image
                marginX = (int)(EYE_IMAGE_MARGIN_PERCENTAGE / 100 * _eyes[i].Width);
                marginY = (int)(EYE_IMAGE_MARGIN_PERCENTAGE / 100 * _eyes[i].Height);
                ROI = new Rectangle(_eyes[i].X + marginX, _eyes[i].Y + marginY, _eyes[i].Width - marginX, _eyes[i].Height - marginY);

                imgCrop = _imageGray.Convert<Gray, float>().Copy(ROI);
                imgCrop.Save("C:\\img\\cropedPic" + count + ".JPG");

                numOfIris += detectIris(imgCrop, count);
                Console.WriteLine("num in for " + numOfIris);
                //count++;
                //Bitmap eye = _imageRGB.Copy(ROI).ToBitmap();
            }
            Console.WriteLine("num after for " + numOfIris);
            */
            foreach (Rectangle currentEye in _eyes)
            {
                Image<Gray, float> imgEye = _imageGray.Convert<Gray, float>().Copy(currentEye);
               // imgEye.Save("C:\\img\\eye" + count + ".JPG");

                //crop image so we remove eyebrow
                marginX = (int)(EYE_IMAGE_MARGIN_PERCENTAGE / 100 * currentEye.Width);
                marginY = (int)(EYE_IMAGE_MARGIN_PERCENTAGE / 100 * currentEye.Height);
                ROI = new Rectangle(currentEye.X + marginX, currentEye.Y + marginY, currentEye.Width - marginX, currentEye.Height - (marginY * 2));
                imgCrop = _imageGray.Convert<Gray, Byte>().Copy(ROI);
               
           //     imgCrop.Save("C:\\img\\cropedPic" + count + ".JPG");
           //     Console.WriteLine("path: C:\\img\\cropedPic" + count + ".JPG");

                double avgGray = imgCrop.GetAverage().Intensity;
           //     Console.WriteLine("avg gray: " + avgGray);

                if ((imgCrop.Width <= minEyeWidth && imgCrop.Height <= minEyeHeight) || avgGray <= 55)
                {
             //       count++;
                    continue;
                }


                numOfIris += detectEyeball(imgCrop); //count);

                //detectLines(imgCrop, count);
               // count++;
                countEyeNum++;
               // Console.WriteLine("eye num counter: " + countEyeNum);
            }
                       
            //dealllocate images
            if (imgCrop != null)
            {
                imgCrop.Dispose();
            }

            imgCrop = null;

            GC.Collect();

            //return numOfIris;

            int numOfPeople = calcNumOfPeople();
            //3 people in picture then all eyes must be open
            if (numOfPeople != 0 && numOfPeople < 4 && numOfIris < countEyeNum)
            //if (numOfPeople != 0 && numOfPeople < 4 && numOfIris < _eyes.Count())
            {
                return 1; //eyes are closed
            }

            //eyes are closed
            if (numOfIris == 0 && countEyeNum > 0)
            //if (numOfIris == 0 && _eyes.Count() > 0)
                return 1;

            return 0; //eyes are open

        }

        private static int detectEyeball(Image<Gray, Byte> eyeImg) //, int count)
        {

            Image<Gray, Byte> temp = eyeImg.Convert<Gray, Byte>().PyrDown().PyrUp(); //PyrDown().PyrUp() --> remove noise(smooth)
           // temp.Save("C:\\img\\tempNoise" + count + ".JPG");

            double thresh = 30.0; //20
         //   double cannyThresh = 30.0; //50

            temp = temp.Canny(thresh, thresh * 2, 3);
         //  temp.Save("C:\\img\\canny" + count + ".JPG");
            
            Gray cannyThreshold = new Gray(thresh * 2); //The higher threshold of the two passed to Canny edge detector (the lower one will be twice smaller).
            Gray accumulatorThreshold = new Gray(thresh); //Accumulator threshold at the center detection stage. The smaller it is, the more false circles may be detected. Circles, corresponding to the larger accumulator values, will be returned first
                            
            double resolution = 2; //Resolution of the accumulator used to detect centers of the circles
            double minDistance = temp.Width;  //Minimum distance between centers of the detected circles
            int minRadius = 0; //Minimal radius of the circles to search for
            int maxRadius = 0; //Maximal radius of the circles to search for
             
            CircleF[] houghCircles = temp.HoughCircles(cannyThreshold, accumulatorThreshold, resolution, minDistance, minRadius, maxRadius)[0]; //[0] --> /Get the circles from the first channel
                       
            foreach (CircleF c in houghCircles)
            {
                eyeImg.Draw(c, new Gray(15), 2);
            }

            //eyeImg.Save("C:\\img\\circle" + count + ".JPG");
            //Console.WriteLine("num of circles: " + houghCircles.Count());

            return houghCircles.Count();
        }

        
        /*
        private static void detectLines(Image<Gray, float> eyeImg, int count)
        {
           
            Image<Gray, Byte> temp = eyeImg.Convert<Gray, Byte>();
            temp.SmoothGaussian(3);
           // Image<Gray, float> imgLaplace = temp.Laplace(1);
           // imgLaplace.Save("C:\\img\\lap" + count + ".JPG");
            Image<Gray, Byte> canny = temp.Canny(50, 100);
            canny.Save("C:\\img\\canny" + count + ".JPG");

            double rhoResolution = 1; //Distance resolution in pixel-related units.
            double thetaResolution = Math.PI / 90;  //Angle resolution measured in radians
            int threshold = 50; //A line is returned by the function if the corresponding accumulator value is greater than threshold
            double minLineWidth = 100; //Minimum width of a lines
            double gapBetweenLines = 1; //Minimum gap between lines

            LineSegment2D[] lines = temp.HoughLinesBinary(rhoResolution, thetaResolution, threshold, minLineWidth, gapBetweenLines)[0];
            
            foreach (LineSegment2D l in lines)
            {
                eyeImg.Draw(l, new Gray(15), 2);
            }
            Console.WriteLine("num of lines:" + lines.Length);
            eyeImg.Save("C:\\img\\line" + count + ".JPG");

           /* Image<Gray, Byte> temp = eyeImg.Convert<Gray, Byte>();
            double cannyThreshold = 50;
            double cannyThresholdLinking = 200;
            double rhoResolution = 1; //Distance resolution in pixel-related units.
            double thetaResolution = Math.PI / 90;  //Angle resolution measured in radians
            int threshold = 50; //A line is returned by the function if the corresponding accumulator value is greater than threshold
            double minLineWidth = 100; //Minimum width of a line
            double gapBetweenLines = 1; //Minimum gap between lines

            LineSegment2D[] lines = temp.HoughLines(cannyThreshold,cannyThresholdLinking,rhoResolution,thetaResolution,threshold,minLineWidth,gapBetweenLines)[0];
           

            foreach (LineSegment2D l in lines)
            {
                eyeImg.Draw(l, new Gray(15), 2);
            }

            eyeImg.Save("C:\\img\\line" + count + ".JPG");
        }*/


        public static int numOfSmiles()
        {
            
        //    int count = 0;

            // Find smile if not found yet
            if (_smiles == null)
                findSmileCascade();

            return _smiles.Length;
            /*foreach (Rectangle currenSmile in _smiles)
            {
          //      Image<Gray, float> imgSmile = _imageGray.Convert<Gray, float>().Copy(currenSmile);
          //      imgSmile.Save("C:\\img\\smile" + count + ".JPG");
          //      count++;
            }*/
            /*if (_smiles.Count() != 0)
                return 1;

            return 0;*/
        }


        /*public static void isEyes()
        {

            int count = 0;

            // Find smile if not found yet
            if (_eyes == null)
                findEyesCascade();

            foreach (Rectangle currenteye in _eyes)
            {
                Image<Gray, float> imgEye = _imageGray.Convert<Gray, float>().Copy(currenteye);
                imgEye.Save("C:\\img\\eye" + count + ".JPG");
                count++;


            }
        }*/

        //calculates laplacian using CV method - old
       /* public static int calcLaplacianIntegral(bool crop)
        {
            Image<Gray, float> imgLaplace = _imageGray.Convert<Gray, float>();

            if (crop)
            {
                 Rectangle sizeToCrop = new Rectangle(_imageGray.Cols / 4, _imageGray.Rows / 4, _imageGray.Cols /2, _imageGray.Rows /2);
            // Crop face
             imgLaplace = _imageGray.Convert<Gray, float>().Copy(sizeToCrop);
        //     imgLaplace.Save("C:\\img\\crp.jpg");
         
            }
            //smooth the image a little bit 3x3
            imgLaplace.SmoothGaussian(3);

            //calculates laplacian
            imgLaplace = imgLaplace.Laplace(19);

             //ImageViewer viewer = new ImageViewer(); //create an image viewer
             ////display the image
             //viewer.Image = imgLaplace;
             //viewer.Show();//show the image viewer
             //imgLaplace.Save("C:\\img\\notsharpLaplace1.JPG");
             

            //calculates the integral of the laplacian image
            MCvScalar sum = CvInvoke.cvSum(imgLaplace);
            int laplaceSum = (int)((sum.v0) / (imgLaplace.Rows * imgLaplace.Cols));


            //dealllocate images
            if (imgLaplace != null)
            {
                imgLaplace.Dispose();
            }

            imgLaplace = null;


            GC.Collect();
            return Math.Abs(laplaceSum);
        }*/


        //calculates RGB gray average gray levels seperately
        public static void calcRGBAverageGrayLevel(out double red, out double green, out double blue)
        {
            _channels = _imageRGB.Split();
            blue = _channels[0].GetAverage().Intensity;
            green = _channels[1].GetAverage().Intensity;
            red = _channels[2].GetAverage().Intensity;
        }

        //calculates HSV gray average gray levels seperately
        public static void calcHSVAverageGrayLevel(out double hue, out double saturation)
        {
            Image<Gray, byte>[] channels = _imageHSV.Split();
            hue = channels[0].GetAverage().Intensity;
            saturation = channels[1].GetAverage().Intensity;

        }

        //gets variance
        public static double getVariance()
        {
            double mean = _histogram.Sum() / _histogram.Length;
            double variance = 0;

            foreach (int gray in _histogram)
            {
                variance += Math.Pow((gray - mean), 2);
            }
            variance /= _histogram.Length;
            variance /= (_imageGray.Rows * _imageGray.Cols);
            return variance;

            //    double sqrMean = 0, mean = 0;
            //    double variance = 0;
            //    //calculate mean
            //    mean = _histogram.Sum() / _histogram.Length;
            //    for (int i = 0; i < _histogram.Length; i++)
            //    {
            //        sqrMean += Math.Pow(_histogram[i], 2);
            //    }
            //    sqrMean /= _histogram.Length;

            //    //for (int i = 0; i < _histogram.Length; i++)
            //    //{
            //    //    variance += Math.Pow((i - mean), 2) * _histogram[i];
            //    //}
            //    variance = sqrMean - Math.Pow(mean, 2)/ (_imageGray.Rows * _imageGray.Cols);
            //    return variance;
            //}
        }

        //gets amount of image information (hist*log(hist))
        public static double getImageInformation()
        {
            double info = 0;

            foreach (int gray in _histogram)
            {
                if (gray != 0)
                    info += gray * Math.Log(gray);
            }
            info = info / (_imageGray.Rows * _imageGray.Cols);
            return info;

        }

        /// <summary>
        /// claculates gradient
        /// </summary>
        /// <param name="crop">should crop image. (calculate gradient only for center.)</param>
        /// <returns>integer that represents gradient value</returns>
        public static int calcGradiant(bool crop)
        {

            Image<Gray, float> imgGradiant = _imageGray.Convert<Gray, float>();
            if (crop)
            {
                Rectangle sizeToCrop = new Rectangle(_imageGray.Cols / 4, _imageGray.Rows / 4, _imageGray.Cols /2, _imageGray.Rows /2);
            // Crop face
                imgGradiant = _imageGray.Convert<Gray, float>().Copy(sizeToCrop);
                //    imgGradiant.Save("C:\\img\\crp.jpg");
           
            }


            //smooth the image a little bit
            imgGradiant.SmoothGaussian(3);


            //create filters for differential by X and by Y
            float[,] filterY = {{ 1, 1, 1},
                                {-1,-1,-1}};
            float[,] filterX = {{1,-1},
                                {1,-1},
                                {1,-1}};

            ConvolutionKernelF convFilterY = new ConvolutionKernelF(filterY),
                               convFilterX = new ConvolutionKernelF(filterX);

            Image<Gray, float> diffY = imgGradiant.Convolution(convFilterY);//.Pow(2);
            Image<Gray, float> diffX = imgGradiant.Convolution(convFilterX);//.Pow(2);


            abs(ref diffX, ref diffY);
            // abs(ref diffY);
            imgGradiant = diffX.Add(diffY);

            /*     imgGradiant.Save("C:\\img\\notsharpGradient1.JPG");
                 ////display the image
              ImageViewer viewer = new ImageViewer(); //create an image viewer
                 viewer.Image = imgGradiant;
                 viewer.Show();//show the image viewer
     */

            //calculates the integral of the gradiant image
            MCvScalar sum = CvInvoke.cvSum(imgGradiant);
            int gradientSum = (int)(sum.v0 / (imgGradiant.Rows * imgGradiant.Cols));

            //dealllocate image
            if (imgGradiant != null)
            {
                imgGradiant.Dispose();
            }
            if (diffY != null)
            {
                diffY.Dispose();
            }
            if (diffX != null)
            {
                diffX.Dispose();
            }
            imgGradiant = null;
            diffX = null;
            diffY = null;
            GC.Collect();

            return gradientSum;
        }

        //calculates laplacian using filter. should be faster than other laplacian- doesn't work!
        /*public static int calcLaplacianIntegral2() 
         { 
             ImageViewer viewer = new ImageViewer(); //create an image viewer 

             Image<Gray, float> imgLaplace = _imageGray.Convert<Gray, float>(); 
   
              //smooth the image a little bit 
              imgLaplace.SmoothGaussian(3); 
   
              //create filters for differential by X and by Y 
              float[,] filterY = new float[3, 1]; 
              float[,] filterX = new float[1, 3]; 
              filterY[0, 0] = 1; 
              filterY[1, 0] = -2; 
              filterY[2, 0] = 1; 
   
              filterX[0, 2] = 1; 
              filterX[0, 1] = -2; 
              filterX[0, 0] = 1; 
   
   
   
              ConvolutionKernelF convFilterY = new ConvolutionKernelF(filterY), 
                                 convFilterX = new ConvolutionKernelF(filterX);

              Image<Gray, float> diffY = imgLaplace.Convolution(convFilterY);
              Image<Gray, float> diffX = imgLaplace.Convolution(convFilterX); 
   
   
              imgLaplace = diffX.Add(diffY);

          
              MCvScalar sum = CvInvoke.cvSum(imgLaplace);
              int laplaceSum = (int)(sum.v0); // (imgLaplace.Rows * imgLaplace.Cols));
              //dealllocate images 
              if (imgLaplace != null) 
              { 
                  imgLaplace.Dispose(); 
              } 
             
              imgLaplace = null; 
              GC.Collect();

              return laplaceSum;
          } 
        */
        /* public static void Write(TextWriter writeTo)
        {
            
                //ImageVector vector = new ImageVector(filePath, dictionary);
                writeTo.WriteLine("sdfsfd:");


                //for (int i = 0; i < ImageVector.NUMBER_OF_PARAMETERS; i++)
                //{
                   // ImageVector.ImageParameters currentParam = ImageVector.getParameterNameByIndex(i);
                    //if (dictionary[currentParam])
                findEyes();
                        writeTo.WriteLine("* " + _eyes.Length);
                //}

        }*/

        #endregion

        #region Private Methods

        //!!!!!!!!!!!!
        //crop pictures 16x16
        private static List<double> cropImg(Image<Gray, float> img)
        {
            const int DIVIDE_TO = 4;

            Image<Gray, float> imgCrop = null; //_imageGray.Convert<Gray, float>();
            Rectangle sizeToCrop;

            List<double> sumList = new List<double>();

           // Console.WriteLine("width: " + _imageGray.Width + "height: " + _imageGray.Height);
           // Console.WriteLine("width: " + img.Width + "height: " + img.Height);

            //crop each picture to 16 parts
            for (int i = 0; i < img.Height - img.Height / DIVIDE_TO + 1; i += img.Height / DIVIDE_TO)
            {
                for (int j = 0; j < img.Width - img.Width / DIVIDE_TO + 1; j += img.Width / DIVIDE_TO)
                {
                    //set size to crop and crop picture
                    sizeToCrop = new Rectangle(j, i, img.Width / DIVIDE_TO, img.Height / DIVIDE_TO);
                    imgCrop = img.Copy(sizeToCrop);//Convert<Gray, float>().Copy(sizeToCrop);
             
                    //imgCrop.Save("C:\\img\\cropedPic" + i + "_" + j + ".JPG");
                    //Console.WriteLine("C:\\img\\cropedPic" + i + "_" + j + ".JPG");

                    sumList.Add(calcLaplacian(imgCrop));//, i, j));                   

                }
            }

            //dealllocate images
            if (imgCrop != null)
            {
                imgCrop.Dispose();
            }

            imgCrop = null;

            // Deallocate managed memory
            GC.Collect();

            return sumList;

        }
        
        //!!!!!!!!!!!!!!!
        //calculate laplacian using CV method
        private static double calcLaplacian(Image<Gray, float> img)//, int i, int j)
        {
            //smooth the image a little bit 3x3
            img.SmoothGaussian(3);

            /*specifying aperture_size=1 gives the fastest variant that is equal to convolving the image 
            with the following kernel: |0 1 0| |1 -4 1| |0 1 0|*/


            //Bitmap bmp = img.Laplace(1).ToBitmap(img.Width, img.Height);
            //Image<Gray, float> imgLaplace = new Image<Gray, float>(bmp);

            Image<Gray, float> imgLaplace = img.Laplace(1);

            /*
             float[,] k = { { 0, 1, 0 }, 
                            { 1, -4, 1 },
                            { 0, 1, 0 } };

             ConvolutionKernelF kernel = new ConvolutionKernelF(k);
             Image<Gray, float> convoluted = img * kernel;
             if(imgLaplace.Equals(convoluted))
             Console.WriteLine("**************true******************");
            */

         //   imgLaplace.Save("C:\\img\\notsharpLaplace" + i + "_" + j + ".JPG");
         //  Console.WriteLine("C:\\img\\notsharpLaplace" + i + "_" + j + ".JPG");

            imgLaplace = imgLaplace.Pow(2);
            MCvScalar sum = CvInvoke.cvSum(imgLaplace);
            double laplaceSum = (double)(sum.v0 / (imgLaplace.Rows * imgLaplace.Cols));

            //Console.WriteLine("sumlap: " + Math.Abs(laplaceSum));

            //dealllocate images
            if (imgLaplace != null)
            {
                imgLaplace.Dispose();
            }

            imgLaplace = null;

            return Math.Abs(laplaceSum);

        }
        
        //!!!!!!!!!!!!
        /*calculates standart diviation by using it's formula
        * for a finite set of numbers X1,...,Xn the standard deviation is found by taking 
        * the square root of the average of the squared differences of the values from their 
        * average value: sqrt(1/n * sigma((Xi - avg)^2)), 1 < i < n
        */
        private static double calcSD(List<double> sumList)
        {
            double avg = sumList.Average();
            double sumOfSquares = 0;

            foreach (int val in sumList)
            {
                sumOfSquares += Math.Pow((val - avg), 2);
            }
            return Math.Sqrt(sumOfSquares / (sumList.Count - 1));
        }



        // Calculating the center of gravity of the faces in the loaded image
        private static Point calcFacesCenterOfGravity()
        {
            // Find faces if not found yet
            if (_faces == null)
                findFacesCascade();

            // Calc centers of each face rectangle
            int numberOfFaces = _faces.Length;
            if (numberOfFaces == 0)
            {
                return new Point(-1, -1);
            }
            else
            {
                // Calc centers of each face rectangle
                Point[] centers = new Point[numberOfFaces];
                for (int i = 0; i < numberOfFaces; i++)
                    centers[i] = new Point(_faces[i].X + (_faces[i].Width / 2), _faces[i].Y + (_faces[i].Height / 2));

                // Sum all centers
                int avgX = 0;
                int avgY = 0;
                for (int i = 0; i < numberOfFaces; i++)
                {
                    avgX += centers[i].X;
                    avgY += centers[i].Y;
                }

                // Get center avrage
                avgX /= numberOfFaces;
                avgY /= numberOfFaces;

                // Return average
                return new Point(avgX, avgY);
            }
        }

        /*
        // Finding faces in the loaded image using haarCascade
        private static void findFaces()
        {
            // Haar parameters
            string path = Environment.CurrentDirectory.ToString() + "\\HaarCascade\\haarcascade_frontalface_default.xml";
            string HAAR_FACE_PATH = path;
            const int MIN_FACE_PIC_WIDTH_RATIO = 12; //old: 13 my good: 10
            const int MIN_FACE_PIC_HEIGHT_RATIO = 10; //old: 13 my good: 12
            const double FACE_SCALE_FACTOR = 1.05;//old: 1.1 my good:1.05
            const int FACE_MIN_NEIGHBORS = 5; //4

            // Load haar if not loaded
            if (_haarFace == null)
                _haarFace = new HaarCascade(HAAR_FACE_PATH);

            // Haar parameters objects
            Rectangle[] faces;
            Size faceMinSize = new Size(_imageGray.Width / MIN_FACE_PIC_WIDTH_RATIO, _imageGray.Height / MIN_FACE_PIC_HEIGHT_RATIO);

            // Do haar
            doHaarCascade(ref _imageGray, ref _haarFace, FACE_SCALE_FACTOR, FACE_MIN_NEIGHBORS, faceMinSize, out faces);

            // Save faces positions
            _faces = removeRedundant(faces);
        }*/

        //!!!!!!!!!!!!!!!!!!!!!!!
        // Finding faces in the loaded image using haarCascade
        private static void findFacesCascade()
        {
            // Haar parameters
            string path = Environment.CurrentDirectory.ToString() + "\\opencv\\sources\\data\\haarcascades\\haarcascade_frontalface_default.xml";
            string CASCADE_FACE_PATH = path;
            const int MIN_FACE_PIC_WIDTH_RATIO = 32; //old: 13 my good: 12 
            const int MIN_FACE_PIC_HEIGHT_RATIO = 34; //old: 13 my good: 10
            const double FACE_SCALE_FACTOR = 1.2;//old: 1.1 my good:1.05
            const int FACE_MIN_NEIGHBORS = 4; //4

            // Load cascade if not loaded
            if (_cascadeFace == null)
                _cascadeFace = new CascadeClassifier(CASCADE_FACE_PATH);

            // Haar parameters objects
            Rectangle[] faces;
            int width = Math.Max(_imageGray.Width / MIN_FACE_PIC_WIDTH_RATIO, 30);
            int height = Math.Max(_imageGray.Height / MIN_FACE_PIC_HEIGHT_RATIO, 30);
            Size faceMinSize = new Size(width, height);

            //do cascade
            faces = _cascadeFace.DetectMultiScale(_imageGray, FACE_SCALE_FACTOR, FACE_MIN_NEIGHBORS, faceMinSize, Size.Empty);
            // Save faces positions
            _faces = removeRedundant(faces);
        }

        // Finding eyes in the loaded image using haarCascade
        private static void findEyesCascade()
        {

            // Check that faces positions exists
            if (_faces == null)
                findFacesCascade();

            // Haar parameters
            string path = Environment.CurrentDirectory.ToString() + "\\opencv\\sources\\data\\haarcascades\\haarcascade_eye.xml";
            string HAAR_EYE_PATH = path;
            const int MIN_EYE_PIC_WIDTH_RATIO = 8;
            const int MIN_EYE_PIC_HEIGHT_RATIO = 8;
            const double EYE_SCALE_FACTOR = 1.1; //old - 1.4, my good - 1.1
            const int EYE_MIN_NEIGHBORS = 2;

            // Load haar if not loaded
            if (_cascadeEyes == null)
                _cascadeEyes = new CascadeClassifier(HAAR_EYE_PATH);

            // Haar parameters objects
            Rectangle[] eyes;
            Rectangle eye;
            Size eyeMinSize;


            // Find eyes in each face
            List<Rectangle> eyeList = new List<Rectangle>();
            int count = 0;

            foreach (Rectangle currentFace in _faces)
            {
                
                //detect from upper part of cropped face image
                Image<Gray, byte> faceImg = _imageGray.Convert<Gray, byte>().Copy(currentFace);
                Rectangle cropArea = new Rectangle(0, 0, currentFace.Width, currentFace.Height / 2 + currentFace.Height / 8);

               // Crop face
                Image<Gray, byte> cropped = faceImg.Copy(cropArea);
               // cropped.Save("C:\\img\\face" + count + ".jpg");
                count++;

                /*Image<Gray, byte> cropped = _imageGray.Copy(currentFace);
                cropped.Save("C:\\img\\face" + count + ".jpg");
                count++;*/

                // Look for eyes
                eyeMinSize = new Size(cropped.Width / MIN_EYE_PIC_WIDTH_RATIO, cropped.Height / MIN_EYE_PIC_HEIGHT_RATIO);

                eyes = _cascadeEyes.DetectMultiScale(cropped, EYE_SCALE_FACTOR, EYE_MIN_NEIGHBORS, eyeMinSize,Size.Empty);
                eyes = removeRedundant(eyes);

                // List found eyes
                foreach (Rectangle currentEye in eyes)
                {
                    eye = new Rectangle(currentFace.X + currentEye.X + cropArea.X, currentFace.Y + currentEye.Y + cropArea.Y, currentEye.Width, currentEye.Height);
                   
                    /*Image<Gray, byte> imgEye = faceImg.Copy(eye);
                    Console.WriteLine("eye pic path C:\\img\\eyes" + counteyes + ".JPG");
                    imgEye.Save("C:\\img\\eyes" + counteyes + ".JPG");
                    counteyes++;*/

                    eyeList.Add(eye);
                }
            }

            // Save all eyes positions
            _eyes = eyeList.ToArray();
        }


        /*
        // Finding eyes in the loaded image using haarCascade
        private static void findEyes()
        {

            // Check that faces positions exists
            if (_faces == null)
                findFacesCascade();

            // Haar parameters
            string path = Environment.CurrentDirectory.ToString() + "\\HaarCascade\\haarcascade_eye.xml";
            string HAAR_EYE_PATH = path;
            const int MIN_EYE_PIC_WIDTH_RATIO = 8;
            const int MIN_EYE_PIC_HEIGHT_RATIO = 8;
            const double EYE_SCALE_FACTOR = 1.4;
            const int EYE_MIN_NEIGHBORS = 2;

            // Load haar if not loaded
            if (_haarEye == null)
                _haarEye = new HaarCascade(HAAR_EYE_PATH);

            // Haar parameters objects
            Rectangle[] eyes;
            Rectangle eye;
            Size eyeMinSize;


            // Find eyes in each face
            List<Rectangle> eyeList = new List<Rectangle>();
            int count = 0;

            foreach (Rectangle currentFace in _faces)
            {
                // Crop face
                Image<Gray, byte> cropped = _imageGray.Copy(currentFace);

                cropped.Save("C:\\img\\face" + count + ".jpg");
                count++;

                // Look for eyes
                eyeMinSize = new Size(cropped.Width / MIN_EYE_PIC_WIDTH_RATIO, cropped.Height / MIN_EYE_PIC_HEIGHT_RATIO);
                doHaarCascade(ref cropped, ref _haarEye, EYE_SCALE_FACTOR, EYE_MIN_NEIGHBORS, eyeMinSize, out eyes);
                eyes = removeRedundant(eyes);

                // List found eyes
                foreach (Rectangle currentEye in eyes)
                {
                    eye = new Rectangle(currentFace.X + currentEye.X, currentFace.Y + currentEye.Y, currentEye.Width, currentEye.Height);
                    eyeList.Add(eye);
                }
            }

            // Save all eyes positions
            _eyes = eyeList.ToArray();
        }*/

        // Finding eyes in the loaded image using haarCascade
        private static void findSmileCascade()
        {

            // Check that faces positions exists
            if (_faces == null)
                findFacesCascade();

            // Haar parameters
            string path = Environment.CurrentDirectory.ToString() + "\\opencv\\sources\\data\\haarcascades\\haarcascade_smile.xml";
            string HAAR_SMILE_PATH = path;
            const int MIN_SMILE_PIC_WIDTH_RATIO = 8;
            const int MIN_SMILE_PIC_HEIGHT_RATIO = 8;
            const double SMILE_SCALE_FACTOR = 1.05;
            const int SMILE_MIN_NEIGHBORS = 2;

            // Load haar if not loaded
            if (_cascadeSmile == null)
                _cascadeSmile = new CascadeClassifier(HAAR_SMILE_PATH);

            
            // Haar parameters objects
            Rectangle[] smiles;
            Rectangle smile;
            Size smileMinSize;


            // Find eyes in each face
            List<Rectangle> smileList = new List<Rectangle>();
           // int count = 0;
           // int countsmile = 0;

            foreach (Rectangle currentFace in _faces)
            {
                
                //detect from lower part of cropped face image
                Image<Gray, byte> faceImg = _imageGray.Convert<Gray, byte>().Copy(currentFace);
               // Rectangle cropArea = new Rectangle(0, currentFace.Height / 2 + currentFace.Height / 8, currentFace.Width, currentFace.Height / 2 - currentFace.Height / 8);               
               // Rectangle cropArea = new Rectangle(0, currentFace.Height / 2, currentFace.Width, currentFace.Height / 2);               

                Rectangle cropArea = new Rectangle(0, currentFace.Height / 2 + currentFace.Height / 8, currentFace.Width, currentFace.Height / 2 - currentFace.Height / 8);               


                // Crop half face
                Image<Gray, byte> cropped = faceImg.Copy(cropArea);
                //cropped.Save("C:\\img\\cropArea" + count + ".JPG");
                //Console.WriteLine("crop path C:\\img\\face" + count + ".JPG");
                //count++;
                /*
                Image<Gray, byte> cropped = _imageGray.Copy(currentFace);
                cropped.Save("C:\\img\\cropArea" + count + ".JPG");
                count++;*/


                // Look for smiles
                smileMinSize = new Size(cropped.Width / MIN_SMILE_PIC_WIDTH_RATIO, cropped.Height / MIN_SMILE_PIC_HEIGHT_RATIO);

                smiles = _cascadeEyes.DetectMultiScale(cropped, SMILE_SCALE_FACTOR, SMILE_MIN_NEIGHBORS, smileMinSize , Size.Empty);
                smiles = removeRedundant(smiles);
                
                // List found eyes
                foreach (Rectangle currentSmile in smiles)
                {
                 
                    smile = new Rectangle(currentFace.X + currentSmile.X + cropArea.X, currentFace.Y + currentSmile.Y + cropArea.Y, currentSmile.Width, currentSmile.Height);
                   
                    /*Image<Gray, byte> imgSmile = faceImg.Copy(smile);
                    Console.WriteLine("smile pic path C:\\img\\smile" + countsmile + ".JPG");
                    imgSmile.Save("C:\\img\\smile" + countsmile + ".JPG");*/
             
                    smileList.Add(smile);
                    //countsmile++;
                    //Console.WriteLine("smile pic path C:\\img\\smile" + countsmile + ".JPG");
                    //countsmile++;
                }
                
            }

            // Save all smile positions
            _smiles = smileList.ToArray();
        }



        // Returns a new list of rectangles which none is contained by another
        private static Rectangle[] removeRedundant(Rectangle[] list)
        {
            if (list.Length < 2)
                return list;

            int count = list.Length;
            bool[] ok = new bool[list.Length];

            for (int i = 0; i < ok.Length; i++)
                ok[i] = true;

            for (int i = 0; i < ok.Length; i++)
                for (int j = 0; j < ok.Length; j++)
                {
                    if (i == j)
                        continue;

                    if (list[i].Contains(list[j]) && ok[i])
                    {
                        ok[i] = false;
                        count--;
                    }
                }

            Rectangle[] newList = new Rectangle[count];
            count = 0;
            for (int i = 0; i < ok.Length; i++)
            {
                if (ok[i])
                {
                    newList[count] = list[i];
                    count++;
                }
            }

            return newList;
        }

        /*
        // Perform haarCascade with the given parameters
        private static void doHaarCascade(ref Image<Gray, byte> pic, ref HaarCascade haar, double scaleFactor, int minNeighbors, Size minSize, out Rectangle[] rects)
        {
            int count = 0;
            int index = 0;
            
            // Do HaarCascade
            _objects = pic.DetectHaarCascade(haar, scaleFactor, minNeighbors, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, minSize)[0];
            
            // Count findings
            foreach (var i in _objects)
                count++;

            // put into rects array
            rects = new Rectangle[count];
            foreach (var i in _objects)
                rects[index++] = i.rect;

            // Dealocate objects memory
            _objects = null;
            GC.Collect();
        }*/


        // Deallocates memory allocated for loading image in RGB, HSV, Gray Scale
        private static void DeallocateImageMemory()
        {
            // Deallocate unmanaged memory
            if (_imageRGB != null)
            {
                _imageRGB.Dispose();
                _imageRGB = null;
            }
            if (_imageHSV != null)
            {
                _imageHSV.Dispose();
                _imageHSV = null;
            }
            if (_imageGray != null)
            {
                _imageGray.Dispose();
                _imageGray = null;
            }
            if (_channels != null)
            {
                if (_channels[0] != null)
                {
                    _channels[0].Dispose();
                    _channels[0] = null;
                }
                if (_channels[1] != null)
                {
                    _channels[1].Dispose();
                    _channels[1] = null;
                }
                if (_channels[2] != null)
                {
                    _channels[2].Dispose();
                    _channels[2] = null;
                }
            }

            // Deallocate managed memory
            GC.Collect();



        }
       
        //Abs
        private static void abs(ref Image<Gray, float> img1, ref Image<Gray, float> img2)
        {
            for (int i = 0; i < img1.Rows; i++)
                for (int j = 0; j < img1.Cols; j++)
                {
                    img1.Data[i, j, 0] = Math.Abs(img1.Data[i, j, 0]);
                    img2.Data[i, j, 0] = Math.Abs(img2.Data[i, j, 0]);
                }
        }

        //initializes histogram
        private static void getHistogram()
        {
            _histogram = new int[256];
            int bins = 256;
            int[] hsize = new int[1] { bins };
            int[] maxid = new int[1];
            int[] minid = new int[1];

            //ranges - grayscale 0 to 256
            float[] xranges = new float[2] { 0, 256 };
            IntPtr inPtr1 = new IntPtr(0);

            GCHandle gch1 = GCHandle.Alloc(xranges, GCHandleType.Pinned);

            try
            {
                inPtr1 = gch1.AddrOfPinnedObject();
            }
            finally
            {
                gch1.Free();
            }
            IntPtr[] ranges = new IntPtr[1] { inPtr1 };

            //planes to obtain the histogram, in this case just one
            IntPtr[] planes = new IntPtr[1] { _imageGray.Ptr };
            IntPtr hist;
            //get the histogram and some info about it
            hist = CvInvoke.cvCreateHist(1, hsize, Emgu.CV.CvEnum.HIST_TYPE.CV_HIST_ARRAY, ranges, 1);
            CvInvoke.cvCalcHist(planes, hist, false, IntPtr.Zero);


            //go over histogram and put it into class property
            for (int i = 0; i < hsize[0]; i++)
            {
                _histogram[i] = (int)CvInvoke.cvQueryHistValue_1D(hist, i);
            }
        }

        #endregion
    }
}
