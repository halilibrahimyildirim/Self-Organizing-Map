using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOM
{
    class Field
    {
        public String name;
        public ArrayList data;
        public int type;
        public double max, min;
        public Dictionary<String, double> nominalMap;
        public Field(String name)
        {
            this.name = name;
            data = new ArrayList();
            min = double.MaxValue;
            max = double.MinValue;
        }
        public void MapInit(Dictionary<String, double> map)
        {
            nominalMap = map;
        }
    }
}
