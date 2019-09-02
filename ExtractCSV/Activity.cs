using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractCSV
{
    class Activity
    {
        public int id;
        public string items;

        public string toCSV()
        {
            string retVal = "";
            retVal += items.ToString();

            return retVal;
        }
    }
}
