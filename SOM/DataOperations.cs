using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOM
{
    class DataOperations
    {
        public static void preProcess(Field[] fields,double[][] records)
        {
            double[] temp;
            for(int i=0;i<fields.Length;i++)
            {
                //nominal field converted to numeric dummy, numeric field converted to [0,1] double
                temp=(fields[i].type == 1) ? minMax(fields[i]) : nominalToDouble(fields[i]);
                for(int j=0;j<temp.Length;j++)
                {
                    records[j][i] = temp[j];
                }
            }
        }
        private static double[] nominalToDouble(Field field)
        {
            double[] values = new double[field.data.Count];
            Dictionary<String, double> temp = new Dictionary<String, double>();
            double tempVal = 0;
            for(int i=0; i< field.data.Count;i++)
            {
                if(!temp.ContainsKey(field.data[i].ToString()))
                {
                    temp[field.data[i].ToString()] = tempVal;
                    tempVal++;
                }
                values[i] = temp[field.data[i].ToString()];
            }
            field.MapInit(temp);
            return values;
        }
        private static double[] minMax(Field field)
        {
            double[] values = new double[field.data.Count];
            for(int i=0;i<field.data.Count;i++)
            {
                values[i] = (((IConvertible)field.data[i]).ToDouble(null) - field.min) / (field.max - field.min);
                //values[i] = ((IConvertible)field.data[i]).ToDouble(null); //without minmax test
            }
            return values;
        }
    }
}
