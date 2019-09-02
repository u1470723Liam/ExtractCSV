﻿using System;
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
            List<Activity> actionsId = new List<Activity>();
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

            StringBuilder processedEvents = new StringBuilder();

            StringBuilder resourceCSV = new StringBuilder();
            StringBuilder networkCSV = new StringBuilder();

            srumEx.getProcesses(userprocessTbl);
            srumEx.getExtendedTables(resourceTbl);
            //srumEx.getExtendedTables(networkTbl, networkCSV);


            List<Event> events = srumEx.getEvents();


            //Process type
            //for (int i = 0; i < events.Count; i++)
            //{
            //    Event ev = events[i];

            //    List<Event> subEvents = events.FindAll(x => (x.userid == ev.userid) && (x.timestamp.Month == ev.timestamp.Month) && (x.timestamp.Day == ev.timestamp.Day) && (x.timestamp.Hour == ev.timestamp.Hour) && (x.type == ev.type));

            //    actionsId.Add(new Event()
            //    {
            //        id = i + 1,
            //        userid = ev.userid,
            //        type = ev.type,
            //        app = ev.app,
            //        timestamp = ev.timestamp

            //    });

            //    foreach (Event subEvent in subEvents)
            //    {
            //        if (subEvent != ev)
            //        {
            //            actionsId.Add(new Event()
            //            {
            //                id = i + 1,
            //                userid = subEvent.userid,
            //                type = subEvent.type,
            //                app = subEvent.app,
            //                timestamp = subEvent.timestamp

            //            });
            //        }


            //        events.Remove(subEvent);
            //    }
            //}

            //Process without type
            for (int i = 0; i < events.Count; i++)
            {
                Event ev = events[i];

                if(ev.userid == 243)
                {
                    List<Event> subEvents = events.FindAll(x => (x.userid == ev.userid) && (x.timestamp.Month == ev.timestamp.Month) && (x.timestamp.Day == ev.timestamp.Day) && (x.timestamp.Hour == ev.timestamp.Hour) && (x.type == ev.type));

                    Activity act = new Activity()
                    {
                        id = i + 1,
                        items = ev.app
                    };


                    foreach (Event subEvent in subEvents)
                    {
                        if (subEvent != ev)
                        {
                            act.items += " " + subEvent.app;
                        }
                        events.Remove(subEvent);
                    }

                    actionsId.Add(act);
                }
               
            }



            foreach (Activity act in actionsId)
            {
                if(act.items.ToCharArray()[0] == ' ')
                {
                    act.items.Remove(0,1);
                }

                processedEvents.AppendLine(act.toCSV());
            }

            File.WriteAllText(fileLoc + "\\" + prefix + "Preprocessed.tab", processedEvents.ToString());

            //Write data to csv.
            //File.WriteAllText(fileLoc + "\\" + prefix + "Resources.csv", resourceCSV.ToString());
            //File.WriteAllText(fileLoc + "\\" + prefix + "Network.csv", networkCSV.ToString());

            Console.WriteLine("Done");
            while (true) { }
        }
    }
}
