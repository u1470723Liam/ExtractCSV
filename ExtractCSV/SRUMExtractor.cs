﻿using Microsoft.Isam.Esent.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace ExtractCSV
{
    class SRUMExtractor
    {
        //JET values for accessing the SRU tables after connection as been established
        private JET_INSTANCE instance;
        private JET_SESID sesid;
        private JET_DBID dbid;
        private JET_TABLEID userprocessid, extendedid;
        private JET_RETINFO retinf = new JET_RETINFO();
        private JET_RETINFO largeretinf = new JET_RETINFO();
        private SRUMTools userUnpack = new SRUMTools();
        private List<Event> events = new List<Event>();
        private List<AppUser> appUsers = new List<AppUser>();

        public SRUMExtractor()
        {
            largeretinf.itagSequence = 1;
        }

        public void initiateSRUM(string srumLocation)
        {
            int pageSize;
            Api.JetGetDatabaseFileInfo(srumLocation, out pageSize, JET_DbInfo.PageSize);
            string instanceName = Guid.NewGuid().ToString();
            Api.JetCreateInstance(out instance, instanceName);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.DatabasePageSize, pageSize, null);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, null);
            Api.JetInit(ref instance);
            Api.JetBeginSession(instance, out sesid, null, null);
            Api.JetAttachDatabase(sesid, srumLocation, AttachDatabaseGrbit.ReadOnly);
            Api.OpenDatabase(sesid, srumLocation, out dbid, OpenDatabaseGrbit.ReadOnly);
        }

        public void close()
        {
            Api.JetCloseFileInstance(instance, JET_HANDLE.Nil);
        }

        public void getProcesses(string tblName, StringBuilder sb)
        {
            Api.OpenTable(sesid, dbid, tblName, OpenTableGrbit.None, out userprocessid);

            int recordsMoved = 0;
            IEnumerable<ColumnInfo> cols = Api.GetTableColumns(sesid, userprocessid);
            Api.JetMove(sesid, userprocessid, JET_Move.First, 0);

            sb.AppendLine("IdIndex, IdType, IdBlob");

            do
            {
                byte type = 5;
                long id = 0;
                string name = "";
                byte[] blobBuffer = new byte[256];
                SruId SRUId = new SruId();
                foreach (ColumnInfo ci in cols)
                {
                    byte[] buffer = new byte[1024];
                    int read = 0;
                    if (ci.Coltyp == JET_coltyp.Long || ci.Coltyp.ToString() == "15")
                    {
                        Api.JetRetrieveColumn(sesid, userprocessid, ci.Columnid, buffer, 256, out read, RetrieveColumnGrbit.None, retinf);
                        id = getIntValue(buffer);

                    }
                    else if (ci.Coltyp == JET_coltyp.UnsignedByte)
                    {
                        Api.JetRetrieveColumn(sesid, userprocessid, ci.Columnid, buffer, 256, out read, RetrieveColumnGrbit.None, retinf);
                        type = getByteValue(buffer);
                    }
                    else if (ci.Coltyp == JET_coltyp.LongBinary)
                    {
                        Api.JetRetrieveColumn(sesid, userprocessid, ci.Columnid, blobBuffer, 256, out read, RetrieveColumnGrbit.None, largeretinf);
                    }
                }

                if (type == 0)
                {
                    string hexString = BitConverter.ToString(blobBuffer, 0);
                    string trim = Regex.Replace(hexString, @"\00-00", "");
                    byte[] bytes = Encoding.Unicode.GetBytes(trim);
                    string binarystring = Encoding.Unicode.GetString(bytes);
                    byte[] data = FromHex(binarystring);
                    name = Encoding.Unicode.GetString(data);
                }
                else if (type == 1)
                {
                    string hexString = BitConverter.ToString(blobBuffer, 0);

                    string trim = Regex.Replace(hexString, @"\00", "");
                    byte[] bytes = Encoding.Unicode.GetBytes(trim);
                    string binarystring = Encoding.Unicode.GetString(bytes);
                    byte[] data = FromHex(binarystring);
                    name = Encoding.Unicode.GetString(data);
                }
                else if (type == 2)
                {
                    string hexString = BitConverter.ToString(blobBuffer, 0);
                    string trim = Regex.Replace(hexString, @"\00-00", "");
                    byte[] bytes = Encoding.Unicode.GetBytes(trim);
                    string binarystring = Encoding.Unicode.GetString(bytes);
                    byte[] data = FromHex(binarystring);
                    name = Encoding.Unicode.GetString(data);
                }
                else if (type == 3)
                {
                    name = convertToSID(blobBuffer);
                }

                string chars = "1234567890abcdefghijklmnopqrstuvwxyz[]/\\";
                char[] anyOf = chars.ToCharArray();
                int lastCharInd = name.LastIndexOfAny(anyOf);

                if (lastCharInd >=0)
                {
                    name = name.Substring(0, lastCharInd +1);
                }
                
                SRUId = new SruId()
                {
                    name = name,
                    id = id,
                    type = type
                };

                AppUser appUser = new AppUser()
                {
                    id = id,
                    name = name,
                    type = type
                };

                appUsers.Add(appUser);
                string newLine = string.Format("{0},{1},{2}", SRUId.id, SRUId.type, SRUId.name);
                sb.AppendLine(newLine);
                recordsMoved++;
            } while (Api.TryMove(sesid, userprocessid, JET_Move.Next, 0));
            Console.WriteLine("Records Extracted from " + tblName + ": " + recordsMoved);
        }
        
        //Original dynamic csv extraction method
        public void getExtendedTablesAll(string tblName, StringBuilder sb)
        {
            Api.OpenTable(sesid, dbid, tblName, OpenTableGrbit.None, out extendedid);

            bool first = true;
            int recordsMoved = 0;
            IEnumerable<ColumnInfo> cols = Api.GetTableColumns(sesid, extendedid);
            ICollection<ColumnInfo> colsDet = cols.ToList();

            Api.JetMove(sesid, extendedid, JET_Move.First, 0);

            do
            {
                string csvstring = "";
                string csvheader = "";
                string[] colNames = new string[colsDet.Count];
                string[] colDeets = new string[colsDet.Count];

                
                int read = 0;
                int arrayPosition = 0;

                foreach (ColumnInfo ci in cols)
                {
                    byte[] buffer = new byte[128];
                    Api.JetRetrieveColumn(sesid, extendedid, ci.Columnid, buffer, 128, out read, RetrieveColumnGrbit.None, retinf);
                    colNames[arrayPosition] = ci.Name;
                    //Console.WriteLine(ci.Name + " " + ci.Coltyp);
                    switch (ci.Coltyp)
                    {
                        case JET_coltyp.Long:
                            colDeets[arrayPosition] = getIntValue(buffer).ToString();
                            break;
                        case JET_coltyp.Binary:
                            colDeets[arrayPosition] = getIntValue(buffer).ToString();
                            break;
                        case JET_coltyp.DateTime:
                            colDeets[arrayPosition] = getDateTimeValue(buffer).ToString();
                            break;
                        default:
                           if (ci.Coltyp.ToString() == "15")
                            {
                                colDeets[arrayPosition] = getLongValue(buffer).ToString();
                            }
                            else
                            {
                                colDeets[arrayPosition] = ci.Coltyp.ToString() + " data type is not handled.";
                            }
                            break;
                    }
                    arrayPosition++;
                }

                if (first)
                {
                    foreach (string col in colNames)
                    {
                         csvheader += col + ",";
                    }
                    csvheader = csvheader.Remove(csvheader.Length -1);
                    sb.AppendLine(csvheader);
                }

                foreach (string col in colDeets)
                {
                    csvstring += col + ",";
                }

                csvstring = csvstring.Remove(csvstring.Length -1);
                sb.AppendLine(csvstring);
                first = false;
                recordsMoved++;
            } while (Api.TryMove(sesid, extendedid, JET_Move.Next, 0));

            Console.WriteLine("Records Extracted from " + tblName + ": " + recordsMoved);
        }

        private Int64 getLongValue(byte[] buffer)
        {
            Int64 val = (Int64)userUnpack.Unpack("q", subArray(buffer, 0, 8))[0];
            return val;
        }

        private byte getByteValue(byte[] buffer)
        {
            byte val = buffer[0]; 
            return val;
        }

        private short getShortValue(byte[] buffer)
        {
            short val = (short)userUnpack.Unpack("s", subArray(buffer, 0, 2))[0];
            return val;
        }

        private ushort getUShortValue(byte[] buffer)
        {
            ushort val = (ushort)userUnpack.Unpack("S", subArray(buffer, 0, 2))[0];
            return val;
        }

        private Int32 getIntValue(byte[] buffer)
        {
            Int32 val = (Int32)userUnpack.Unpack("i", subArray(buffer, 0, 4))[0];
            return val;
        }

        private UInt32 getUIntValue(byte[] buffer)
        {
            UInt32 na = (UInt32)userUnpack.Unpack("I", subArray(buffer, 0, 4))[0];
            return na;
        }

        private DateTime getDateTimeValue(byte[] buffer)
        {
            double na = (double)userUnpack.Unpack("d", subArray(buffer, 0, 8))[0];
            DateTime timestamp = c(na);
            
            return timestamp;
        }

        private byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        private string convertToSID(byte[] buffer)
        {
            uint[] strSIDComponent = new uint[] { buffer[0] };
            string sidString = "S";
            if (buffer.Length >= 8)
            {
                int subauthorityCount = buffer[1];

                SRUMTools userUnpack = new SRUMTools();

                uint identifierUnpack = (UInt16)userUnpack.Unpack(">H", subArray(buffer, 2, 2))[0];
                identifierUnpack <<= 32;
                UInt32 na = (UInt32)userUnpack.Unpack(">I", subArray(buffer, 4, 4))[0];
                identifierUnpack |= (uint)na;

                strSIDComponent = strSIDComponent.Concat(new uint[] { (uint)identifierUnpack }).ToArray();

                int start = 8;

                for (int i = 0; i < subauthorityCount; i++)
                {
                    byte[] authority = subArray(buffer, start, 4);

                    if (authority.Length < 4)
                    {
                        Console.WriteLine("Error: Authority length less than 4.");
                        break;
                    }
                    UInt32 intCast = (UInt32)userUnpack.Unpack("I", authority)[0];
                    strSIDComponent = strSIDComponent.Concat(new uint[] { (uint)intCast }).ToArray();
                    start += 4;
                }

                foreach (uint x in strSIDComponent)
                {
                    sidString += "-" + x.ToString();
                }
            }
            return sidString;
        }

        private byte[] subArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        private DateTime c(double d)
        {
            return DateTime.FromOADate(d);
        }

        private DateTime newDateMethod(byte[] buffer)
        {
            return DateTime.FromBinary(getLongValue(buffer));
        }
        
        public List<Event> getEvents()
        {
            return events;
        }
    }
}
