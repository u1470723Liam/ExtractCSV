using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractCSV
{
    class Event
    {
        public int id;
        public long userid;
        public int type;
        public string app;
        public DateTime timestamp;

        public string toString()
        {
            string retVal = "";

            retVal += id + ", ";
            retVal += userid + ", ";
            retVal += app + ", ";
            retVal += type + ", ";
            retVal += timestamp.ToString();

            return retVal;
        }

        public string toStringNoType()
        {
            string retVal = "";

            retVal += id + ", ";
            retVal += userid + ", ";
            retVal += app + ", ";
            retVal += type + ", ";
            retVal += timestamp.ToString();

            return retVal;
        }
    }
}
