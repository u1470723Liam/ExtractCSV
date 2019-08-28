using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractCSV
{
    class Event
    {
        public long userid;
        public string app;
        public DateTime timestamp;

        public string toString()
        {
            string retVal = "";

            retVal += userid + ", ";
            retVal += app + ", ";
            retVal += timestamp.ToString();

            return retVal;
        }
    }
}
