using System;
using System.Windows.Forms;
using System.Diagnostics;
using CanyonExtractor.Data;
using CanyonExtractor.Controllers;

namespace CanyonExtractor.Forms
{
    public partial class ExtractorDemo : Form
    {
        string shpFile = "";
        string canyonFolder = @"H:\GorgeRecognize\";
        string canyonName = "canyon";
        double distance = 1000;//WT
        int enclosurePara = 30;
        int code1 = 0;
        int code2 = 0;

        public ExtractorDemo()
        {
            InitializeComponent();
            textBox5.Text = canyonFolder;
            comboBox1.SelectedIndex = 1;
            comboBox2.SelectedIndex = 0;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            canyonFolder = textBox5.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            shpFile = textBox4.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            InputData inputData = new InputData();
            textBox4.Text = inputData.GetFilename("shapefile|*.shp");
            shpFile = textBox4.Text;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            InputData inputData = new InputData();
            textBox5.Text = inputData.GetFoldername() + @"\";
            canyonFolder = textBox5.Text;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            FeatureDispose featureDispose = new FeatureDispose();
            featureDispose.DoIdentify(shpFile, canyonFolder, canyonName, distance,
                enclosurePara, code1, code2);
            stopwatch.Stop();
            TimeSpan timeSpan = stopwatch.Elapsed;//record total time
            TimeSpan apiSpan = ElevationAPI.stopwatch.Elapsed;
            MessageBox.Show("totol time：" + timeSpan.ToString() + "\n"
                + "data reading time：" + apiSpan.ToString());
        }

        public void DoIdentify()
        {
            FeatureDispose featureDispose = new FeatureDispose();
            featureDispose.DoIdentify(shpFile, canyonFolder, canyonName, distance,
                enclosurePara, code1, code2);
        }    

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            code1 = comboBox1.SelectedIndex;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            code2 = comboBox2.SelectedIndex;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            canyonName = textBox3.Text;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            distance = Convert.ToInt32(textBox6.Text);
        }

        private void Button11_Click(object sender, EventArgs e)
        {
            if (ElevationAPI.TestApi())
                MessageBox.Show("connect success!");
            else
                MessageBox.Show("connect error!");
        }

        private void TextBox11_TextChanged(object sender, EventArgs e)
        {
            ElevationAPI.url = textBox11.Text;
        }

        private void Button10_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            InputData inputData = new InputData();
            textBox9.Text = inputData.GetFilename("shapefile|*.shp");
            shpFile = textBox9.Text;
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            InputData inputData = new InputData();
            textBox10.Text = inputData.GetFoldername() + @"\";
            canyonFolder = textBox10.Text;
        }

        private void TextBox9_TextChanged(object sender, EventArgs e)
        {
            shpFile = textBox9.Text;
        }

        private void TextBox10_TextChanged(object sender, EventArgs e)
        {
            canyonFolder = textBox10.Text;
        }

        private void TextBox8_TextChanged(object sender, EventArgs e)
        {
            distance = Convert.ToInt32(textBox8.Text);
        }

        private void TextBox7_TextChanged(object sender, EventArgs e)
        {
            canyonName = textBox7.Text;
        }

        private void ComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            code1 = comboBox4.SelectedIndex;
        }

        private void ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            code2 = comboBox3.SelectedIndex;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            FeatureDispose featureDispose = new FeatureDispose();
            featureDispose.DoIdentify(shpFile, canyonFolder, canyonName, distance,
                enclosurePara, code1, code2);
            stopwatch.Stop();
            TimeSpan timeSpan = stopwatch.Elapsed;//record total time
            TimeSpan apiSpan = ElevationAPI.stopwatch.Elapsed;
            MessageBox.Show("totol time：" + timeSpan.ToString() + "\n"
                + "data reading time：" + apiSpan.ToString());
        }
    }
}
