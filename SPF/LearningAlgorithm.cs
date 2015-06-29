using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPF
{
    abstract class LearningAlgorithm
    {
        public enum Algorithm
        {
            DecisionTree = 0,
            DecisionTreeNumerical = 1,
            KNN = 2
        }

        public LearningAlgorithm()
        {
        }

        //public void AddVectorsToList(ImageVector[] vectorsTrue, ImageVector[] vectorsFalse)
        //{
        //    int numOfCurrentVectors;
        //    int numOfNewVectors;

        //    // Get current and new list lengths
        //    numOfCurrentVectors = (_vectorsTrue == null) ? 0 : _vectorsTrue.Length;
        //    numOfNewVectors = vectorsTrue.Length;

        //    // Make new list
        //    ImageVector[] newListTrue = new ImageVector[numOfCurrentVectors + numOfNewVectors];
        //    for (int i = 0; i < numOfCurrentVectors; i++)
        //        newListTrue[i] = new ImageVector(_vectorsTrue[i]);
        //    for (int i = 0; i < numOfNewVectors; i++)
        //        newListTrue[numOfCurrentVectors + i] = new ImageVector(vectorsTrue[i]);

        //    // Assign to vector member
        //    _vectorsTrue = newListTrue;

        //    // Get current and new list lengths
        //    numOfCurrentVectors = (_vectorsFalse == null) ? 0 : _vectorsFalse.Length;
        //    numOfNewVectors = vectorsFalse.Length;

        //    // Make new list
        //    ImageVector[] newListFalse = new ImageVector[numOfCurrentVectors + numOfNewVectors];
        //    for (int i = 0; i < numOfCurrentVectors; i++)
        //        newListFalse[i] = new ImageVector(_vectorsFalse[i]);
        //    for (int i = 0; i < numOfNewVectors; i++)
        //        newListFalse[numOfCurrentVectors + i] = new ImageVector(vectorsFalse[i]);

        //    // Assign to vector member
        //    _vectorsFalse = newListFalse;
        //}

        //public void ClearLists()
        //{
        //    _vectorsTrue = null;
        //    _vectorsFalse = null;
        //}

        public abstract bool Train(List<ImageVector> VectorsTrue, List<ImageVector> VectorsFalse);


        public abstract bool Predict(List<ImageVector> cVectors, out double[] results);


        public abstract bool SaveData(string name);


        public abstract bool LoadData(string name);

    }
}
