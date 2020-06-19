using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM
{
    class Node
    {
        public double[] weights;
        public List<int> records;
        public Node(double[] weights)
        {
            records = new List<int>();
            this.weights = new double[weights.Length];
            for (int i=0;i<weights.Length;i++)
                this.weights[i] = weights[i];
        }
    }
}
