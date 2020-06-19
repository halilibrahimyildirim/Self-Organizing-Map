using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SOM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)//Select File button
        {
            button2.Enabled = false;
            button3.Enabled = false;
            string fileName;
            (SOM.records, SOM.fields, fileName) = FileOperations.Read();
            string[] parsed = fileName.Split('\\');
            label1.Text = "File Name: " + parsed[parsed.Length-1];
            if(fileName!="")
                button2.Enabled = true;
        }

        Visualizer vs;

        private void button2_Click(object sender, EventArgs e)//Start SOM button
        {
            button3.Enabled = false;
            if (!int.TryParse(textBox1.Text, out SOM.maxIter))
                SOM.maxIter = 100;
            DataOperations.preProcess(SOM.fields, SOM.records);
            SOM.Start();
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)//Visualize button
        {
            vs.printU_Matrix();
        }

        private void Form1_Load(object sender, EventArgs e)//when form load hexagons will be ready
        {
            int r = SOM.outputR * 2 - 1;
            int c = SOM.outputC * 2 - 1;
            vs = new Visualizer(pictureBox1, r, c,pictureBox3);
        }

        private void pictureBox2_Click(object sender, EventArgs e)//İnfo button
        {
            String info = "100 Iteration is recommended, higher values will cause more execution time.\n\n" +
                "If you click on any 'output' hexagon, you can see some informations about that node like;\n" +
                "+Record Count\n" +
                "+Records No\n" +
                "+Weights(Kept all values as double because otherwise all weights looks same)";
            MessageBox.Show(info,"Information",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }
    }
}
