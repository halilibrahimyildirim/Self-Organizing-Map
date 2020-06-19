using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOM
{
    class SOM
    {
        public static Field[] fields;
        public static double[][] records;
        public static Node[][] outputs;
        public static int outputR = 10, outputC = 10;//10x10 matrix output
        static double sLR = 0.1, eLR = 0.01;//starting and ending learning rate
        static double learnRatio = sLR / eLR;
        static double sSigma = (outputC+outputR)/2, fSigma = 0.1;//starting and ending sigma
        static double sRatio = sSigma / fSigma;
        public static int maxIter;
        public static void Start()
        {
            InitOutput();
            int winI, winJ, temp;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());//for better randomize
            List<int> indexList = new List<int>();
            for (int i = 0; i < records.Length; i++)
                indexList.Add(i);
            int change;
            for (int iter = 0; iter < maxIter; iter++)
            {
                for (int i = 0; i < records.Length; i++)
                {
                    temp = rnd.Next(indexList.Count - i);//select random record
                    (winI, winJ) = FindBMU(temp);//find BMU
                    UpdateWeights(winI, winJ, temp, iter);//update weights
                    //put selected random to end of list and select new random by ignoring it
                    change = indexList[temp];
                    indexList[temp] = indexList[indexList.Count - i-1];
                    indexList[indexList.Count - i-1] = change;
                }
            }
            for (int i = 0; i < records.Length; i++)//find which record belongs which output node
            {
                (winI, winJ) = FindBMU(i);
                outputs[winI][winJ].records.Add(i);
            }
        }

        private static void InitOutput()
        {
            //initialize random weights for output nodes
            Random rnd = new Random(Guid.NewGuid().GetHashCode()); ;
            outputs = new Node[outputR][];
            double[] weights = new double[fields.Length];
            for (int i = 0; i < outputR; i++)
            {
                outputs[i] = new Node[outputC];
                for (int j = 0; j < outputC; j++)
                {
                    for (int k = 0; k < fields.Length; k++)
                    {
                        if (fields[k].type == 1)
                            weights[k] = rnd.NextDouble();
                        else
                            weights[k] = rnd.Next(fields[k].nominalMap.Count);
                    }
                    outputs[i][j] = new Node(weights);
                }
            }
        }

        public static double EuclideanDist(double[] vector1, double[] vector2)
        {
            double temp = 0, x, y;
            for (int k = 0; k < vector1.Length; k++)
            {
                x = vector1[k];
                y = vector2[k];
                if (fields[k].type == 1)
                    temp += (x - y) * (x - y);
                else
                    if (x != y)
                    temp += 1;
            }
            //return temp;
            return Math.Sqrt(temp);
        }

        //using same decay formula for learning rate and sigma because its good and smooth
        private static double CalcLearningRate(int iter)
        {
            return sLR * Math.Exp(-iter / (maxIter / Math.Log(learnRatio)));
        }

        private static double CalcSigma(int iter)
        {
            return sSigma * Math.Exp(-iter / (maxIter / Math.Log(sRatio)));
        }

        private static double CalcH(double dist, int iter)
        {
            return Math.Exp(-dist / (2 * Math.Pow(CalcSigma(iter),2)));
        }

        private static void UpdateWeights(int winI, int winJ, int recNo, int iter)
        {
            double deltaWeight;
            for (int i = 0; i < outputR; i++)
            {
                for (int j = 0; j < outputC; j++)
                {
                    for (int k = 0; k < fields.Length; k++)
                    {
                        //update all weights by formula
                        if (fields[k].type == 1)
                        {
                            deltaWeight = records[recNo][k] - outputs[i][j].weights[k];
                            outputs[i][j].weights[k] += deltaWeight * CalcLearningRate(iter) *
                                CalcH(EuclideanDist(outputs[winI][winJ].weights, outputs[i][j].weights), iter);
                        }
                        else
                        {
                            //if field is nominal directly change weights
                            outputs[i][j].weights[k] = outputs[winI][winJ].weights[k];
                        }
                    }
                }
            }
        }

        private static (int, int) FindBMU(int recNo)
        {
            int r = 0, c = 0;
            double best = double.MaxValue, temp;
            for (int i = 0; i < outputR; i++)
            {
                for (int j = 0; j < outputC; j++)
                {
                    //find output node with min distance to record[recNo]
                    temp = EuclideanDist(records[recNo], outputs[i][j].weights);
                    if (temp < best)
                    {
                        r = i; c = j;
                        best = temp;
                    }
                }
            }
            return (r, c);
        }
    }
}
