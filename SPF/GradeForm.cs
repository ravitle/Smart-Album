using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SPF
{
    public partial class GradeForm : Form
    {
        public static int i;
        public static double[] user_rating;
        public static string[] allFiles;

        public GradeForm()
        {
            InitializeComponent();

            allFiles = Testing.GetFilesFromDirectory(MainForm.truefold).ToArray();
            int NumOfFiles = allFiles.Count();
            i = 0;
            label2.Text = (i+1).ToString();
            label4.Text = (NumOfFiles).ToString();
            user_rating = new double[NumOfFiles];
            for (int k = 0; k < NumOfFiles; k++)
                user_rating[k] = 0;
            FileStream stream = new FileStream(allFiles[i], FileMode.Open, FileAccess.Read);
            pictureBox1.Image = Image.FromStream(stream);
            stream.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(user_rating[i].ToString());
            char[] ch = new char[3];
            bool is_good=false;
            if (_textuser.Text.Length <= 3)
            {
                ch = _textuser.Text.ToCharArray();
                is_good = true;
                if ((ch.Length == 3) && ((ch[0] != '1') && (ch[0] != '0')))
                {
                    is_good = false;
                    _textuser.Clear();
                }
                if ((ch.Length == 3) && (ch[0] == '1') && (ch[1] != '0') && (ch[2] != '0'))
                {
                    is_good = false;
                    _textuser.Clear();
                }
            }
            else
                _textuser.Clear();

            if (is_good)
            {
                for (int i = 0; i < ch.Length; i++)
                {
                    if ((ch[i] >= '0') && (ch[i] <= '9') || (ch[i] == '.') || (ch[i] == '-'))
                        is_good = true;
                    else
                    {
                        is_good = false;
                        _textuser.Clear();
                        break;
                    }

                }

                if (is_good)
                {
                    if (_textuser.Text.Length != 0)
                        user_rating[i] = Convert.ToDouble(_textuser.Text);
                    else
                        user_rating[i] = 0;
                    i++;
                    label2.Text = (i + 1).ToString();
                    label4.Text = (allFiles.Count()).ToString();
                    _textuser.Clear();
                    _btnnext.Enabled = false;
                    if (i < user_rating.Length)
                    {
                        FileStream stream = new FileStream(allFiles[i], FileMode.Open, FileAccess.Read);
                        pictureBox1.Image = Image.FromStream(stream);
                        stream.Close();
                    }
                    else
                    {
                        MessageBox.Show("The pictures are over");
                    }
                }
                else
                    MessageBox.Show("please enter only a numbers");
            }
            else
                MessageBox.Show("select a grade 0-100");

            
           
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void _textuser_TextChanged(object sender, EventArgs e)
        {
            
            _btnnext.Enabled = true;
            
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void GradeForm_Load(object sender, EventArgs e)
        {

        }

       
    }
}
