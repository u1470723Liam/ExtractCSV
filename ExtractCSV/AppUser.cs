using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractCSV
{
    class AppUser
    {
        public long id;
        public string name;
        public int type;


        public string getName()
        {
            return name;
        }

        public long getID()
        {
            return id;
        }

        public int getType()
        {
            return type;
        }
    }
}
