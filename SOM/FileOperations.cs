using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SOM
{
    class FileOperations
    {
        public static (double[][],Field[],string) Read()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            String[] input,temp;
            int fieldCount, recordCount;
            int i,j;
            Field[] fields=null;
            double[][] records = null;
            String tempStr;
            try
            {
                if (ofd.CheckFileExists && ofd.FileName != "")
                {
                    StreamReader sr = new StreamReader(ofd.FileName);
                    CultureInfo.CurrentCulture = new CultureInfo("en-GB", false);//for float numbers
                    input = sr.ReadToEnd().Split('\n');
                    temp = input[0].Split(',');
                    fieldCount = temp.Length;
                    recordCount = input.Length - 1;
                    records = new double[recordCount][];
                    fields = new Field[fieldCount];
                    for (i = 0; i < fieldCount; i++)
                    {
                        temp[i] = temp[i].Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                        fields[i] = new Field(temp[i]);//init fields name
                        tempStr = input[1].Split(',')[i];
                        if (double.TryParse(tempStr, out double val))//if field is nominal type 0, otherwise type 1
                            fields[i].type = 1;
                        else
                            fields[i].type = 0;
                    }
                    //read records and find max-min values of each field
                    for (i = 1; i < recordCount + 1; i++)
                    {
                        temp = input[i].Split(',');
                        records[i - 1] = new double[fieldCount];
                        for (j = 0; j < fieldCount; j++)
                        {
                            if (fields[j].type == 1)
                            {
                                records[i - 1][j] = double.Parse(temp[j]);
                                fields[j].data.Add(records[i - 1][j]);
                                if (records[i - 1][j] > fields[j].max)
                                    fields[j].max = records[i - 1][j];
                                if (records[i - 1][j] < fields[j].min)
                                    fields[j].min = records[i - 1][j];
                            }
                            else
                                fields[j].data.Add(temp[j]);
                        }
                    }
                }
                else
                    throw new FileNotFoundException("Please choose file");
            }
            catch (FileNotFoundException err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return (records, fields, ofd.FileName);
        }
    }
}
