/*
 * Class:       CS 4308 Section 
 * Term:        Spring 2023
 * Name:        John Blake
 * Instructor:   Sharon Perry
 * Project:     Deliverable P1 Scanner  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS4308_Project
{
    //Each token that the scanner will look for should have a unique code and a pattern to use for matching
    public class ScannerToken
    {
        // The regular expression pattern. If we want a scanner token for not equals, the pattern should be "~=" in Julia.
        public string pattern;
        // The internal ID assigned to this token. Used for parsing.
        public int ID;
        public ScannerToken()
        {
            pattern = "";
            ID = 0;
        }
        public ScannerToken(string p, int id)
        {
            pattern = p;
            ID = id;
        }
        public override string ToString()
        {
            return "Token ID: " + ID;
        }
    }
}
