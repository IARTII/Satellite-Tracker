using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SatelliteTracker
{
    public class TLESett
    {
        public string TLEname = "";
        public string TLEcolor = "";
        public string TLE1 = "";
        public string TLE2 = "";

        public void SaveToFile(string destenation)
        {
            File.AppendAllText(destenation, TLEname + "\n");
            File.AppendAllText(destenation, TLEcolor + "\n");
            File.AppendAllText(destenation, TLE1 + "\n");
            File.AppendAllText(destenation, TLE2 + "\n");
        }
    }
}
