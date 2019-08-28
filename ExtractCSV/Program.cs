using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            //SRUM table names
            const string resourceTbl = "{D10CA2FE-6FCF-4F6D-848E-B2E99266FA89}";
            const string networkTbl = "{973F5D5C-1D90-4944-BE8E-24B94231A174}";
            const string userprocessTbl = "SruDbIdMapTable";
            const string fileLoc = @"C:\Users\u1470723\Documents";
            const string location = @"C:\Users\u1470723\Downloads\sru\SRUDB.dat";

            string prefix = "";

            Console.WriteLine("Enter File Prefix:");
            prefix = Console.ReadLine();

            SRUMExtractor srumEx = new SRUMExtractor();
            srumEx.initiateSRUM(location);

            //StringBuilder processesCSV = new StringBuilder();
            //StringBuilder resourceCSV = new StringBuilder();
            //StringBuilder networkCSV = new StringBuilder();

            srumEx.getProcesses(userprocessTbl);
            srumEx.getExtendedTables(resourceTbl);
            //srumEx.getExtendedTables(networkTbl, networkCSV);

            //File.WriteAllText(fileLoc + "\\" + prefix + "Processes.csv", processesCSV.ToString());
            //File.WriteAllText(fileLoc + "\\" + prefix + "Resources.csv", resourceCSV.ToString());
            //File.WriteAllText(fileLoc + "\\" + prefix + "Network.csv", networkCSV.ToString());

            
            //networkCSV.Clear();

            foreach (Event ev in srumEx.getEvents())
            {
                Console.WriteLine(ev.toString());
            }

            Console.WriteLine("Done");
            while (true){}
        }
    }
}
