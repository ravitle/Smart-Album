namespace SPF
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this._lblUser = new System.Windows.Forms.Label();
            this._txtUser = new System.Windows.Forms.TextBox();
            this._btnTest = new System.Windows.Forms.Button();
            this._lblParameters = new System.Windows.Forms.Label();
            this._btnUser = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._rdoKNNweight = new System.Windows.Forms.RadioButton();
            this._lstParameters = new System.Windows.Forms.CheckedListBox();
            this._rdoKNN = new System.Windows.Forms.RadioButton();
            this._rdoTree = new System.Windows.Forms.RadioButton();
            this._lblAlgorithm = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._lblTrainProgress = new System.Windows.Forms.Label();
            this._progressBarTrain = new System.Windows.Forms.ProgressBar();
            this._txtFolderTrue = new System.Windows.Forms.TextBox();
            this._txtFolderAll = new System.Windows.Forms.TextBox();
            this._btnTrain = new System.Windows.Forms.Button();
            this._btnBrowseTrue = new System.Windows.Forms.Button();
            this._lblTrueFolder = new System.Windows.Forms.Label();
            this._btnBrowseAll = new System.Windows.Forms.Button();
            this._lblAllFolder = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._btnPredictUser = new System.Windows.Forms.Button();
            this._txtPredictFolder = new System.Windows.Forms.TextBox();
            this._btnBrowsePredict = new System.Windows.Forms.Button();
            this._btnPredict = new System.Windows.Forms.Button();
            this._lblPredictProgress = new System.Windows.Forms.Label();
            this._progressBarPredict = new System.Windows.Forms.ProgressBar();
            this._lblPredict = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblUser
            // 
            this._lblUser.AutoSize = true;
            this._lblUser.Location = new System.Drawing.Point(27, 22);
            this._lblUser.Name = "_lblUser";
            this._lblUser.Size = new System.Drawing.Size(84, 13);
            this._lblUser.TabIndex = 5;
            this._lblUser.Text = "Enter your name";
            // 
            // _txtUser
            // 
            this._txtUser.Location = new System.Drawing.Point(30, 48);
            this._txtUser.Name = "_txtUser";
            this._txtUser.Size = new System.Drawing.Size(208, 20);
            this._txtUser.TabIndex = 6;
            this._txtUser.TextChanged += new System.EventHandler(this._txtUser_TextChanged);
            // 
            // _btnTest
            // 
            this._btnTest.Location = new System.Drawing.Point(250, 504);
            this._btnTest.Name = "_btnTest";
            this._btnTest.Size = new System.Drawing.Size(294, 23);
            this._btnTest.TabIndex = 18;
            this._btnTest.Text = "Testing Interface";
            this._btnTest.UseVisualStyleBackColor = true;
            this._btnTest.Click += new System.EventHandler(this._btnTest_Click);
            // 
            // _lblParameters
            // 
            this._lblParameters.AutoSize = true;
            this._lblParameters.Location = new System.Drawing.Point(-241, 417);
            this._lblParameters.Name = "_lblParameters";
            this._lblParameters.Size = new System.Drawing.Size(207, 13);
            this._lblParameters.TabIndex = 23;
            this._lblParameters.Text = "Choose Parameters for Image Proccessing";
            // 
            // _btnUser
            // 
            this._btnUser.Enabled = false;
            this._btnUser.Location = new System.Drawing.Point(30, 504);
            this._btnUser.Name = "_btnUser";
            this._btnUser.Size = new System.Drawing.Size(208, 23);
            this._btnUser.TabIndex = 26;
            this._btnUser.Text = "Next step";
            this._btnUser.UseVisualStyleBackColor = true;
            this._btnUser.Click += new System.EventHandler(this._btnUser_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._rdoKNNweight);
            this.groupBox1.Controls.Add(this._lstParameters);
            this.groupBox1.Controls.Add(this._rdoKNN);
            this.groupBox1.Controls.Add(this._rdoTree);
            this.groupBox1.Controls.Add(this._lblAlgorithm);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(30, 105);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(208, 393);
            this.groupBox1.TabIndex = 32;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "choose your atributes";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(48, 126);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(15, 14);
            this.checkBox1.TabIndex = 34;
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(69, 126);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 33;
            this.label2.Text = "All of the parameters";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(184, 13);
            this.label1.TabIndex = 32;
            this.label1.Text = "Choose Image processing parameters";
            // 
            // _rdoKNNweight
            // 
            this._rdoKNNweight.AutoSize = true;
            this._rdoKNNweight.Location = new System.Drawing.Point(48, 80);
            this._rdoKNNweight.Name = "_rdoKNNweight";
            this._rdoKNNweight.Size = new System.Drawing.Size(104, 17);
            this._rdoKNNweight.TabIndex = 31;
            this._rdoKNNweight.Text = "KNN with weight";
            this._rdoKNNweight.UseVisualStyleBackColor = true;
            this._rdoKNNweight.CheckedChanged += new System.EventHandler(this._rdoKNNweight_CheckedChanged);
            // 
            // _lstParameters
            // 
            this._lstParameters.BackColor = System.Drawing.SystemColors.MenuBar;
            this._lstParameters.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._lstParameters.FormattingEnabled = true;
            this._lstParameters.Items.AddRange(new object[] {
            "Average Gray Level",
            "Average Red Level",
            "Average Blue Level",
            "Average Green Level",
            "Average Hue Level",
            "Average Saturation Level",
            "Edges",
            "Image Information",
            "Variance",
            "Num of People",
            "Faces Center Of Gravity X",
            "Faces Center Of Gravity Y",
            "Faces Image Area Ratio",
            "Distance From Gravity Center",
            "Red Eye"});
            this._lstParameters.Location = new System.Drawing.Point(48, 162);
            this._lstParameters.Name = "_lstParameters";
            this._lstParameters.Size = new System.Drawing.Size(160, 225);
            this._lstParameters.TabIndex = 29;
            this._lstParameters.SelectedIndexChanged += new System.EventHandler(this._lstParameters_SelectedIndexChanged);
            // 
            // _rdoKNN
            // 
            this._rdoKNN.AutoSize = true;
            this._rdoKNN.Location = new System.Drawing.Point(48, 57);
            this._rdoKNN.Name = "_rdoKNN";
            this._rdoKNN.Size = new System.Drawing.Size(48, 17);
            this._rdoKNN.TabIndex = 28;
            this._rdoKNN.Text = "KNN";
            this._rdoKNN.UseVisualStyleBackColor = true;
            this._rdoKNN.CheckedChanged += new System.EventHandler(this._rdoKNN_CheckedChanged);
            // 
            // _rdoTree
            // 
            this._rdoTree.AutoSize = true;
            this._rdoTree.Checked = true;
            this._rdoTree.Location = new System.Drawing.Point(48, 34);
            this._rdoTree.Name = "_rdoTree";
            this._rdoTree.Size = new System.Drawing.Size(91, 17);
            this._rdoTree.TabIndex = 27;
            this._rdoTree.TabStop = true;
            this._rdoTree.Text = "Decision Tree";
            this._rdoTree.UseVisualStyleBackColor = true;
            // 
            // _lblAlgorithm
            // 
            this._lblAlgorithm.AutoSize = true;
            this._lblAlgorithm.Location = new System.Drawing.Point(6, 18);
            this._lblAlgorithm.Name = "_lblAlgorithm";
            this._lblAlgorithm.Size = new System.Drawing.Size(133, 13);
            this._lblAlgorithm.TabIndex = 26;
            this._lblAlgorithm.Text = "Choose Learning Algorithm";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._lblTrainProgress);
            this.groupBox2.Controls.Add(this._progressBarTrain);
            this.groupBox2.Controls.Add(this._txtFolderTrue);
            this.groupBox2.Controls.Add(this._txtFolderAll);
            this.groupBox2.Controls.Add(this._btnTrain);
            this.groupBox2.Controls.Add(this._btnBrowseTrue);
            this.groupBox2.Controls.Add(this._lblTrueFolder);
            this.groupBox2.Controls.Add(this._btnBrowseAll);
            this.groupBox2.Controls.Add(this._lblAllFolder);
            this.groupBox2.Location = new System.Drawing.Point(250, 22);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(294, 252);
            this.groupBox2.TabIndex = 33;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Let me learn";
            // 
            // _lblTrainProgress
            // 
            this._lblTrainProgress.AutoSize = true;
            this._lblTrainProgress.BackColor = System.Drawing.SystemColors.ControlDark;
            this._lblTrainProgress.Location = new System.Drawing.Point(127, 231);
            this._lblTrainProgress.Name = "_lblTrainProgress";
            this._lblTrainProgress.Size = new System.Drawing.Size(21, 13);
            this._lblTrainProgress.TabIndex = 38;
            this._lblTrainProgress.Text = "0%";
            // 
            // _progressBarTrain
            // 
            this._progressBarTrain.BackColor = System.Drawing.SystemColors.Control;
            this._progressBarTrain.Location = new System.Drawing.Point(19, 218);
            this._progressBarTrain.Name = "_progressBarTrain";
            this._progressBarTrain.Size = new System.Drawing.Size(240, 26);
            this._progressBarTrain.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBarTrain.TabIndex = 37;
            // 
            // _txtFolderTrue
            // 
            this._txtFolderTrue.Location = new System.Drawing.Point(19, 146);
            this._txtFolderTrue.Name = "_txtFolderTrue";
            this._txtFolderTrue.Size = new System.Drawing.Size(240, 20);
            this._txtFolderTrue.TabIndex = 36;
            this._txtFolderTrue.TextChanged += new System.EventHandler(this._txtFolderTrue_TextChanged);
            // 
            // _txtFolderAll
            // 
            this._txtFolderAll.Location = new System.Drawing.Point(19, 73);
            this._txtFolderAll.Name = "_txtFolderAll";
            this._txtFolderAll.Size = new System.Drawing.Size(240, 20);
            this._txtFolderAll.TabIndex = 35;
            // 
            // _btnTrain
            // 
            this._btnTrain.Enabled = false;
            this._btnTrain.Location = new System.Drawing.Point(19, 179);
            this._btnTrain.Name = "_btnTrain";
            this._btnTrain.Size = new System.Drawing.Size(240, 27);
            this._btnTrain.TabIndex = 34;
            this._btnTrain.Text = "Train the program";
            this._btnTrain.UseVisualStyleBackColor = true;
            this._btnTrain.Click += new System.EventHandler(this._btnTrain_Click);
            // 
            // _btnBrowseTrue
            // 
            this._btnBrowseTrue.Location = new System.Drawing.Point(19, 117);
            this._btnBrowseTrue.Name = "_btnBrowseTrue";
            this._btnBrowseTrue.Size = new System.Drawing.Size(142, 23);
            this._btnBrowseTrue.TabIndex = 33;
            this._btnBrowseTrue.Text = "Browse...";
            this._btnBrowseTrue.UseVisualStyleBackColor = true;
            this._btnBrowseTrue.Click += new System.EventHandler(this._btnBrowseTrue_Click);
            // 
            // _lblTrueFolder
            // 
            this._lblTrueFolder.AutoSize = true;
            this._lblTrueFolder.Location = new System.Drawing.Point(16, 101);
            this._lblTrueFolder.Name = "_lblTrueFolder";
            this._lblTrueFolder.Size = new System.Drawing.Size(200, 13);
            this._lblTrueFolder.TabIndex = 32;
            this._lblTrueFolder.Text = "Select a folder of true pictures for training";
            // 
            // _btnBrowseAll
            // 
            this._btnBrowseAll.Location = new System.Drawing.Point(19, 44);
            this._btnBrowseAll.Name = "_btnBrowseAll";
            this._btnBrowseAll.Size = new System.Drawing.Size(142, 23);
            this._btnBrowseAll.TabIndex = 31;
            this._btnBrowseAll.Text = "Browse...";
            this._btnBrowseAll.UseVisualStyleBackColor = true;
            this._btnBrowseAll.Click += new System.EventHandler(this._btnBrowseAll_Click);
            // 
            // _lblAllFolder
            // 
            this._lblAllFolder.AutoSize = true;
            this._lblAllFolder.Location = new System.Drawing.Point(16, 28);
            this._lblAllFolder.Name = "_lblAllFolder";
            this._lblAllFolder.Size = new System.Drawing.Size(192, 13);
            this._lblAllFolder.TabIndex = 30;
            this._lblAllFolder.Text = "Select a folder of all pictures for training";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._btnPredictUser);
            this.groupBox3.Controls.Add(this._txtPredictFolder);
            this.groupBox3.Controls.Add(this._btnBrowsePredict);
            this.groupBox3.Controls.Add(this._btnPredict);
            this.groupBox3.Controls.Add(this._lblPredictProgress);
            this.groupBox3.Controls.Add(this._progressBarPredict);
            this.groupBox3.Controls.Add(this._lblPredict);
            this.groupBox3.Location = new System.Drawing.Point(250, 276);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(294, 222);
            this.groupBox3.TabIndex = 34;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "It\'s time to Decide";
            // 
            // _btnPredictUser
            // 
            this._btnPredictUser.Enabled = false;
            this._btnPredictUser.Location = new System.Drawing.Point(20, 100);
            this._btnPredictUser.Name = "_btnPredictUser";
            this._btnPredictUser.Size = new System.Drawing.Size(238, 29);
            this._btnPredictUser.TabIndex = 43;
            this._btnPredictUser.Text = "Rate the pictures";
            this._btnPredictUser.UseVisualStyleBackColor = true;
            this._btnPredictUser.Click += new System.EventHandler(this._btnPredictUser_Click);
            // 
            // _txtPredictFolder
            // 
            this._txtPredictFolder.Location = new System.Drawing.Point(21, 74);
            this._txtPredictFolder.Name = "_txtPredictFolder";
            this._txtPredictFolder.Size = new System.Drawing.Size(240, 20);
            this._txtPredictFolder.TabIndex = 42;
            this._txtPredictFolder.TextChanged += new System.EventHandler(this._txtPredictFolder_TextChanged);
            // 
            // _btnBrowsePredict
            // 
            this._btnBrowsePredict.Location = new System.Drawing.Point(21, 46);
            this._btnBrowsePredict.Name = "_btnBrowsePredict";
            this._btnBrowsePredict.Size = new System.Drawing.Size(142, 22);
            this._btnBrowsePredict.TabIndex = 41;
            this._btnBrowsePredict.Text = "Browse...";
            this._btnBrowsePredict.UseVisualStyleBackColor = true;
            this._btnBrowsePredict.Click += new System.EventHandler(this._btnBrowsePredict_Click);
            // 
            // _btnPredict
            // 
            this._btnPredict.Location = new System.Drawing.Point(19, 142);
            this._btnPredict.Name = "_btnPredict";
            this._btnPredict.Size = new System.Drawing.Size(240, 26);
            this._btnPredict.TabIndex = 40;
            this._btnPredict.Text = "Let the program Predict";
            this._btnPredict.UseVisualStyleBackColor = true;
            this._btnPredict.Click += new System.EventHandler(this._btnPredict_Click);
            // 
            // _lblPredictProgress
            // 
            this._lblPredictProgress.AutoSize = true;
            this._lblPredictProgress.BackColor = System.Drawing.SystemColors.ControlDark;
            this._lblPredictProgress.Location = new System.Drawing.Point(127, 196);
            this._lblPredictProgress.Name = "_lblPredictProgress";
            this._lblPredictProgress.Size = new System.Drawing.Size(21, 13);
            this._lblPredictProgress.TabIndex = 37;
            this._lblPredictProgress.Text = "0%";
            // 
            // _progressBarPredict
            // 
            this._progressBarPredict.Location = new System.Drawing.Point(19, 184);
            this._progressBarPredict.Name = "_progressBarPredict";
            this._progressBarPredict.Size = new System.Drawing.Size(240, 25);
            this._progressBarPredict.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBarPredict.TabIndex = 36;
            this._progressBarPredict.Click += new System.EventHandler(this._progressBarPredict_Click);
            // 
            // _lblPredict
            // 
            this._lblPredict.AutoSize = true;
            this._lblPredict.Location = new System.Drawing.Point(16, 30);
            this._lblPredict.Name = "_lblPredict";
            this._lblPredict.Size = new System.Drawing.Size(122, 13);
            this._lblPredict.TabIndex = 33;
            this._lblPredict.Text = "Select a folder to predict";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(30, 74);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(208, 25);
            this.button1.TabIndex = 35;
            this.button1.Text = "Next step";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 535);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._btnUser);
            this.Controls.Add(this._lblParameters);
            this.Controls.Add(this._btnTest);
            this.Controls.Add(this._txtUser);
            this.Controls.Add(this._lblUser);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Tag = "";
            this.Text = "Tishlerating";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _lblUser;
        private System.Windows.Forms.TextBox _txtUser;
        private System.Windows.Forms.Button _btnTest;
        private System.Windows.Forms.Label _lblParameters;
        private System.Windows.Forms.Button _btnUser;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox _lstParameters;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label _lblTrainProgress;
        private System.Windows.Forms.ProgressBar _progressBarTrain;
        private System.Windows.Forms.TextBox _txtFolderTrue;
        private System.Windows.Forms.TextBox _txtFolderAll;
        private System.Windows.Forms.Button _btnTrain;
        private System.Windows.Forms.Button _btnBrowseTrue;
        private System.Windows.Forms.Label _lblTrueFolder;
        private System.Windows.Forms.Button _btnBrowseAll;
        private System.Windows.Forms.Label _lblAllFolder;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox _txtPredictFolder;
        private System.Windows.Forms.Button _btnBrowsePredict;
        private System.Windows.Forms.Button _btnPredict;
        private System.Windows.Forms.Label _lblPredictProgress;
        private System.Windows.Forms.ProgressBar _progressBarPredict;
        private System.Windows.Forms.Label _lblPredict;
        private System.Windows.Forms.RadioButton _rdoKNN;
        private System.Windows.Forms.RadioButton _rdoTree;
        private System.Windows.Forms.Label _lblAlgorithm;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RadioButton _rdoKNNweight;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button _btnPredictUser;
    }
}

