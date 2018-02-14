/*
 
  This Source Code is subject to the terms of the APACHE
  LICENSE 2.0. You can obtain a copy of the terms at
  https://www.apache.org/licenses/LICENSE-2.0
  Copyright (C) 2018 Invise Labs

  Learn more about Invise Labs and our projects by visiting: http://invi.se/labs

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartProber
{
    class Program
    {
        static void Main(string[] args)
        {
            /* DON'T USE MAIN FOR ANYTHING OTHER THAN JUST CALLING */
            OnLoad();
        }

        /* VARIABLES */
        private static List<String> smarts = new List<String>();
        private static bool checkFailed = false;
        private static bool fatalError = false;

        private static void OnLoad()
        {
            /* CALL A POPULATION OF ALL STORAGE MECHANISMS AND RETURN SMART DATA */
            RefreshDrives();

            /* WHAT IS OUR EXIT CODE */
            if (checkFailed) { Console.WriteLine(" CHECK FAILED;"); Environment.Exit(-3); }
            else if (fatalError) { Console.WriteLine("** Error encountered during execution of script. Do you have admin privs?"); }

            /* OTHERWISE EXIT W/ CHECK PASS STATUS */
            Console.WriteLine(" CHECK PASSED;");
            Environment.Exit(0);
        }
    

        private static void RefreshDrives()
        {
            try
            {
                /* LOCAL VARIABLES */
                bool warn = false;

                try
                {
                    /* POPULATE INFORMATION FROM WMI */
                    HDDSmart.Load();

                    foreach (var drive in HDDSmart.drivesDict)
                    {
                        bool hasData = false; /* HAS DATA WE CARE ABOUT */

                        int health = 100;
                        string model = "–";
                        string size = "–";
                        string status = "–";
                        string reall = "0";
                        string reallcount = "0";
                        string pending = "0";
                        string bad = "0";
                        string pwrhr = "–";
                        string pwrcnt = "–";
                        string issues = "";

                        try
                        {
                            if (drive.Value.Model != null && drive.Value.Model != "")
                            { model = drive.Value.Model; }

                            if (drive.Value.Size != null)
                            { size = drive.Value.Size; }

                            /* CHECK FOR ATTRIBUTES WE CARE ABOUT LOGGING */
                            foreach (var attr in drive.Value.Attributes)
                            {
                                try
                                {
                                    string attribLower = attr.Value.Attribute.ToLower();

                                    /* IF > OR < BLOCKS USE MATH AND LIKLIHOOD TO DETERMINE IMMINENT FAILURE CHANCE. */

                                    if (attr.Value.HasData)
                                    {
                                        if (attribLower.Contains("on hours count")) /* POWER ON HOURS */
                                        {
                                            hasData = true;
                                            pwrhr = attr.Value.Data.ToString();

                                            if (attr.Value.Data >= 30000)
                                            { health -= 25; warn = true; issues += "hours count;"; }
                                            else if (attr.Value.Data >= 20000)
                                            { health -= 20; issues += "hours count;"; }
                                            else if (attr.Value.Data >= 10000)
                                            { health -= 10; issues += "hours count;"; }
                                            else if (attr.Value.Data >= 7000)
                                            { health -= 5; issues += "hours count;"; }
                                        }
                                        else if (attribLower.Contains("cycle count")) /* POWER COUNT */
                                        {
                                            hasData = true;
                                            pwrcnt = attr.Value.Data.ToString();

                                            if (attr.Value.Data >= 12000)
                                            { health -= 15; issues += "cycle count;"; }
                                            else if (attr.Value.Data >= 8000)
                                            { health -= 10; issues += "cycle count;"; }
                                        }
                                        else if (attribLower.Contains("stop count")) /* POWER COUNT */
                                        {
                                            hasData = true;

                                            if (pwrcnt != "–") /* IF POWER COUNT HAS ALREADY BEEN CHECKED, DETERMINE WHICH ONE TO USE */
                                            {
                                                long cnt = Convert.ToInt64(pwrcnt);
                                                if (attr.Value.Data > 0 && attr.Value.Data < cnt)
                                                { pwrcnt = attr.Value.Data.ToString(); }
                                            }
                                            else
                                            {
                                                pwrcnt = attr.Value.Data.ToString();
                                                if (attr.Value.Data >= 12000)
                                                { health -= 15; issues += "cycle count;"; }
                                                else if (attr.Value.Data >= 8000)
                                                { health -= 10; issues += "cycle count;"; }
                                            }
                                        }
                                        else if (attribLower.Contains("reallocated")) /* SECTORS DETERMINED BAD, AND WERE MOVED. CAUSES DATA CORRUPTION. */
                                        {
                                            hasData = true;
                                            reall = attr.Value.Data.ToString();

                                            if (attr.Value.Data > 0) { warn = true; }

                                            if (attr.Value.Data >= 60)
                                            { health -= 90; }
                                            else if (attr.Value.Data >= 50)
                                            { health -= 80; }
                                            else if (attr.Value.Data >= 40)
                                            { health -= 70; }
                                            else if (attr.Value.Data >= 30)
                                            { health -= 60; }
                                            else if (attr.Value.Data >= 20)
                                            { health -= 50; }
                                            else if (attr.Value.Data >= 10)
                                            { health -= 40; }
                                            else if (attr.Value.Data > 5)
                                            { health -= 15; }
                                            else if (attr.Value.Data > 0)
                                            { health -= 10; }
                                        }
                                        else if (attribLower.Contains("fail count"))
                                        {
                                            /* FOR SSDS */
                                            hasData = true;
                                            try
                                            {
                                                if (pending != "0") { int p = attr.Value.Data + Convert.ToInt32(pending); pending = p.ToString(); }
                                                else { pending = attr.Value.Data.ToString(); }
                                            }
                                            catch { pending = attr.Value.Data.ToString(); }

                                            if (attr.Value.Data > 0) { warn = true; }
                                            if (attr.Value.Data >= 30)
                                            { health -= 30; }
                                            else if (attr.Value.Data >= 20)
                                            { health -= 25; }
                                            else if (attr.Value.Data >= 10)
                                            { health -= 20; }
                                            else if (attr.Value.Data > 5)
                                            { health -= 15; }
                                            else if (attr.Value.Data > 0)
                                            { health -= 10; }
                                        }
                                        else if (attribLower.Contains("bad block"))
                                        {
                                            /* FOR SSDS */
                                            hasData = true;
                                            bad = attr.Value.Data.ToString();

                                            if (attr.Value.Data > 0) { warn = true; }
                                            if (attr.Value.Data >= 30)
                                            { health -= 30; }
                                            else if (attr.Value.Data >= 20)
                                            { health -= 25; }
                                            else if (attr.Value.Data >= 10)
                                            { health -= 20; }
                                            else if (attr.Value.Data > 5)
                                            { health -= 15; }
                                            else if (attr.Value.Data > 0)
                                            { health -= 10; }
                                        }
                                        else if (attribLower.Contains("bad flash block"))
                                        {
                                            /* FOR SSDS */
                                            hasData = true;
                                            try
                                            {
                                                if (bad != "0") { int b = attr.Value.Data + Convert.ToInt32(bad); bad = b.ToString(); }
                                                else { bad = attr.Value.Data.ToString(); }
                                            }
                                            catch { bad = attr.Value.Data.ToString(); }

                                            if (attr.Value.Data > 0) { warn = true; }
                                            if (attr.Value.Data >= 30)
                                            { health -= 30; }
                                            else if (attr.Value.Data >= 20)
                                            { health -= 25; }
                                            else if (attr.Value.Data >= 10)
                                            { health -= 20; }
                                            else if (attr.Value.Data > 5)
                                            { health -= 15; }
                                            else if (attr.Value.Data > 0)
                                            { health -= 10; }
                                        }
                                        else if (attribLower.Contains("uncorrectable")) /* UNCORRECTABLE SECTORS, HENCE THE NAME, ARE LITERALLY TOSSED OUT. DATA THERE? OH WELL. GONE NOW. */
                                        {
                                            hasData = true;
                                            if (bad != "0") { bad += "-" + attr.Value.Data.ToString(); }
                                            else { bad = attr.Value.Data.ToString(); }

                                            if (attr.Value.Data > 0) { warn = true; }
                                            if (attr.Value.Data >= 30)
                                            { health -= 50; }
                                            else if (attr.Value.Data >= 20)
                                            { health -= 40; }
                                            else if (attr.Value.Data >= 10)
                                            { health -= 20; }
                                            else if (attr.Value.Data > 5)
                                            { health -= 15; }
                                            else if (attr.Value.Data > 0)
                                            { health -= 10; }
                                        }
                                        else if (attribLower.Contains("pending sector")) /* SECTORS FLAGGED WAITING FOR VALIDATION. NEWER DRIVES USE THIS TO INDICATE 
                                                                                          * THEY'VE ALREADY VALIDATED A SECTOR IS DEAD. EVEN 1 IS VERY BAD NEWS. */
                                        {
                                            hasData = true;
                                            if (pending != "0") { pending += "-" + attr.Value.Data.ToString(); }
                                            else { pending = attr.Value.Data.ToString(); }

                                            if (attr.Value.Data > 0) { warn = true; }
                                            if (attr.Value.Data >= 10)
                                            { health -= 70; }
                                            else if (attr.Value.Data > 5)
                                            { health -= 45; }
                                            else if (attr.Value.Data > 0)
                                            { health -= 40; }
                                        }
                                        else if (attribLower.Contains("reallocation")) //- Attempts to reallocate bad sectors. If less than reallocated, corruption likely.
                                        {
                                            hasData = true;
                                            reall = attr.Value.Data.ToString();

                                            if (attr.Value.Data > 0) { warn = true; }
                                            if (attr.Value.Data >= 30)
                                            { health -= 50; }
                                            else if (attr.Value.Data >= 20)
                                            { health -= 40; }
                                            else if (attr.Value.Data >= 10)
                                            { health -= 30; }
                                            else if (attr.Value.Data > 5)
                                            { health -= 25; }
                                            else if (attr.Value.Data > 0)
                                            { health -= 20; }
                                        }
                                    }

                                    //Console.WriteLine("{0}\t {1}\t {2}\t {3}\t " + attr.Value.Data + " " + ((attr.Value.IsOK) ? "OK" : ""), attr.Value.Attribute, attr.Value.Current, attr.Value.Worst, attr.Value.Threshold);
                                }
                                catch (Exception ex) { }
                            }
                        }
                        catch (Exception ex) { }

                        /* CORRECT HEALTH SCORING */
                        if (health < 0)
                        { health = 5; }
                        else if (health > 100)
                        { health = 100; }

                        /* DETERMINE PASS OR FAIL STATUS */
                        try
                        {
                            if (health < 95 && !(reall == "0" && reallcount == "0"
                            && pending == "0" && bad == "0"))
                            { status = "FAIL " + health + "%"; }
                            else { status = "PASS " + health + "%"; }

                            if (status == "?")
                            {
                                if (drive.Value.IsOK) { status = "PASS"; }
                                else { status = "FAIL"; }
                            }
                            else if (status == "FAIL") { }
                            else if (status == "PASS") { }
                        }
                        catch { status = "?"; }

                        /* ADD IT TO THE LIST */
                        if (!model.Contains("USB"))
                        {
                            string thisHDD = drive.Value.Size + " " + model + ",status=" + status + ", pwrhr=" + pwrhr +
                                            ", pwrcnt=" + pwrcnt + ", reall=" + reall + "-" + reallcount + ", pending=" + pending + ", bad=" + bad + ";";

                            smarts.Add(thisHDD); /* ADD TO A LIST IN CASE WE WANT TO DO SOMETHING WITH THIS LATER */
                            Console.WriteLine(thisHDD);

                            if (warn) { checkFailed = true; } /* FAIL CHECK IF WE HAVE FAILURE PREDICTION INDICATORS PRESENT */
                        }
                    }
                }
                catch (Exception ex) { }
            }
            catch { }
        }
    }
}
