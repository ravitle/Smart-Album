using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Forms;
namespace SPF
{

    //don't empty lists ever!!
    class VectorRepository
    {
        private string _name;
        private BinaryFormatter _serializer;
        private Stream _trueStream, _falseStream;

        private List<ImageVector> _vectorListTrue;
        internal List<ImageVector> VectorListTrue
        {
            get { return _vectorListTrue; }
            set { _vectorListTrue = value; }
        }

        private List<ImageVector> _vectorListFalse;
        internal List<ImageVector> VectorListFalse
        {
            get { return _vectorListFalse; }
            set { _vectorListFalse = value; }
        }


        public VectorRepository(string name)
        {
            _name = name;
            _vectorListTrue = new List<ImageVector>();
            _vectorListFalse = new List<ImageVector>();
            _serializer = new BinaryFormatter();
            //create directory for the vector data
            try
            {
                if (!Directory.Exists(name + "'sVectorData"))
                {
                    Directory.CreateDirectory(name + "'sVectorData");
                }
            }
            catch (Exception)
            {

                throw;
            }



        }

        /// <summary>
        /// adds vectors  of true images/false images to the correct lists. if recieves null- doesn't change lists
        /// </summary>
        /// <param name="vectorsTrue">new true vectors to add to repository</param>
        /// <param name="vectorsFalse">new false vectors to add to repository</param>
        /// <returns>true if adds vectors to list succesfully</returns>
        public bool AddToList(List<ImageVector> vectorsTrue, List<ImageVector> vectorsFalse)
        {

            try
            {
                if (!loadList())
                    return false;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);    //for now
            }


            if (vectorsTrue != null)
            {
                if (_vectorListTrue == null)
                    _vectorListTrue = new List<ImageVector>();

                foreach (ImageVector vector in vectorsTrue)
                    _vectorListTrue.Add(vector);
            }
            if (vectorsFalse != null)
            {
                if (_vectorListFalse == null)
                    _vectorListFalse = new List<ImageVector>();
                foreach (ImageVector vector in vectorsFalse)
                    _vectorListFalse.Add(vector);
            }
            if (!SaveList())
                return false;
            return true;
        }

        /// <summary>
        /// saves inside lists to file and keeps inside lists full
        /// </summary>
        /// <returns>true if was serialized list successfully. else false.</returns>
        private bool SaveList()
        {
            try
            {
                //delete the existing files
                if (!deleteFiles())
                    return false;
                //open streams to files
                if (!openStreams())
                    return false;


                //serilize vectors to binary files
                if (_vectorListTrue != null)
                {
                    _serializer.Serialize(_trueStream, _vectorListTrue);
                    // _vectorListTrue.Clear();
                }

                if (_vectorListFalse != null)
                {
                    _serializer.Serialize(_falseStream, _vectorListFalse);
                    // _vectorListFalse.Clear();
                }
            }
            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
                return false;
            }

            finally
            {
                _trueStream.Close();
                _falseStream.Close();
            }
            return true;
        }

        /// <summary>
        /// gets imageVectors from the files into true/false vector properties. 
        /// outgoing vector lists could be null if there was an exception
        /// </summary>
        /// <returns>true if loads lists succesfully. else false.</returns>
        public bool loadList()
        {
            try
            {
                //open streams to files
                if (!openStreams())
                    return false;

                //get the vectors from the files
                if (_trueStream.Length != 0)
                {
                    _vectorListTrue = _serializer.Deserialize(_trueStream) as List<ImageVector>;

                }
                if (_falseStream.Length != 0)
                {
                    _vectorListFalse = _serializer.Deserialize(_falseStream) as List<ImageVector>;
                }

            }
            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
                return false;
            }
            finally
            {
                _trueStream.Close();
                _falseStream.Close();
            }
            return true;
        }

        /// <summary>
        /// deletes the data files leaving repository and inside lists empty
        /// </summary>
        /// <returns>true if succesfuly deleted files and lists</returns>
        public bool delete()
        {
            clearLists();
            if (deleteFiles())
                return true;
            return false;
        }

        private bool deleteFiles()
        {
            try
            {
                File.Delete(_name + "'sVectorData\\" + _name + "'s_True_Vectors");
                File.Delete(_name + "'sVectorData\\" + _name + "'s_False_Vectors");
            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }

        //open streams to files
        private bool openStreams()
        {
            try
            {
                _trueStream = File.Open(_name + "'sVectorData\\" + _name + "'s_True_Vectors", FileMode.OpenOrCreate);
                _falseStream = File.Open(_name + "'sVectorData\\" + _name + "'s_False_Vectors", FileMode.OpenOrCreate);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///  clears lists
        /// </summary>
        private void clearLists()
        {
            _vectorListTrue.Clear();
            _vectorListFalse.Clear();
        }

        /// <summary>
        /// gets imageVector that was created for given image path.
        /// should be called only after lists are loaded.
        /// </summary>
        /// <param name="path">path of image</param>
        /// <returns>the vector. null if image doesn't exist in repository</returns>
        public ImageVector getVectorByPath(string path)
        {
            //check only picture name
            foreach (ImageVector v in VectorListTrue)
            {
                string a = v.Path.Substring(v.Path.LastIndexOf("\\") + 1);
                string b = path.Substring(path.LastIndexOf("\\") + 1);

                if (v.Path.Substring(v.Path.LastIndexOf("\\") + 1).Equals(path.Substring(path.LastIndexOf("\\") + 1)))
                    return v;
            }
            foreach (ImageVector v in VectorListFalse)
            {
                if (v.Path.Substring(v.Path.LastIndexOf("\\") + 1).Equals(path.Substring(path.LastIndexOf("\\") + 1)))
                    return v;
            }
            return null;
        }
    }
}
