/*
 
  This Source Code is subject to the terms of the APACHE
  LICENSE 2.0. You can obtain a copy of the terms at
  https://www.apache.org/licenses/LICENSE-2.0
  Copyright (C) 2018 Invise Labs

  Learn more about Invise Labs and our projects by visiting: http://invi.se/labs

*/

using System;
using System.Collections.Generic;
using System.Management;

namespace SmartProber
{
    public class HDD
    {
        public int Index { get; set; }
        public bool IsOK { get; set; }
        public string Model { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public string Serial { get; set; }
        public Dictionary<int, Smart> Attributes = new Dictionary<int, Smart>() {
                {0x00, new Smart("Invalid")},
                {0x01, new Smart("Raw read error rate")},
                {0x02, new Smart("Throughput performance")},
                {0x03, new Smart("Spinup time")},
                {0x04, new Smart("Start/Stop count")},
                {0x05, new Smart("Reallocated sector count")},
                {0x06, new Smart("Read channel margin")},
                {0x07, new Smart("Seek error rate")},
                {0x08, new Smart("Seek timer performance")},
                {0x09, new Smart("Power-on hours count")},
                {0x0A, new Smart("Spinup retry count")},
                {0x0B, new Smart("Calibration retry count")},
                {0x0C, new Smart("Power cycle count")},
                {0x0D, new Smart("Soft read error rate")},
                {0xB0, new Smart("Erase fail count")},
                {0xB6, new Smart("Erase fail count")},
                {0xB5, new Smart("Program fail count total")},
                {0xB7, new Smart("Runtime Bad Block")},
                {0xB8, new Smart("End-to-End error")},
                {0xBE, new Smart("Airflow Temperature")},
                {0xBF, new Smart("G-sense error rate")},
                {0xC0, new Smart("Power-off retract count")},
                {0xC1, new Smart("Load/Unload cycle count")},
                {0xC2, new Smart("HDD temperature")},
                {0xC3, new Smart("Hardware ECC recovered")},
                {0xC4, new Smart("Reallocation count")},
                {0xC5, new Smart("Current pending sector count")},
                {0xC6, new Smart("Offline scan uncorrectable count")},
                {0xC7, new Smart("UDMA CRC error rate")},
                {0xC8, new Smart("Write error rate")},
                {0xC9, new Smart("Soft read error rate")},
                {0xCA, new Smart("Data Address Mark errors")},
                {0xCB, new Smart("Run out cancel")},
                {0xCC, new Smart("Soft ECC correction")},
                {0xCD, new Smart("Thermal asperity rate (TAR)")},
                {0xCE, new Smart("Flying height")},
                {0xCF, new Smart("Spin high current")},
                {0xD0, new Smart("Spin buzz")},
                {0xD1, new Smart("Offline seek performance")},
                {0xDC, new Smart("Disk shift")},
                {0xDD, new Smart("G-sense error rate")},
                {0xDE, new Smart("Loaded hours")},
                {0xDF, new Smart("Load/unload retry count")},
                {0xE0, new Smart("Load friction")},
                {0xE1, new Smart("Load/Unload cycle count")},
                {0xE2, new Smart("Load-in time")},
                {0xE3, new Smart("Torque amplification count")},
                {0xE4, new Smart("Power-off retract count")},
                {0xE6, new Smart("GMR head amplitude")},
                {0xE7, new Smart("Temperature")},
                {0xF0, new Smart("Head flying hours")},
                {0xFA, new Smart("Read error retry rate")},
                {0xFC, new Smart("Newly added bad flash block")},
                {0xFE, new Smart("Free fall protection")},
            };

    }

    public class Smart
    {
        public bool HasData
        {
            get
            {
                if (Current == 0 && Worst == 0 && Threshold == 0 && Data == 0)
                    return false;
                return true;
            }
        }
        public string Attribute { get; set; }
        public int Current { get; set; }
        public int Worst { get; set; }
        public int Threshold { get; set; }
        public int Data { get; set; }
        public bool IsOK { get; set; }

        public Smart()
        {

        }

        public Smart(string attributeName)
        {
            this.Attribute = attributeName;
        }
    }

    public class HDDSmart
    {
        public static Dictionary<int, HDD> drivesDict = new Dictionary<int, HDD>();
        public static bool ErrorThrown = false;

        public static void Load()
        {
            try
            {
                drivesDict.Clear();

                /* RETRIEVE LIST OF DRIVES ON COMPUTER. THIS RETURNS USBS AND VIRTUAL DVD DRIVES AS WELL. */                  
                var ddMos = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

                /* EXTRACT MODEL AND INTERFACE INFORMATION */
                int dIndex = 0;
                foreach (ManagementObject drive in ddMos.Get())
                {
                    try
                    {
                        var hdd = new HDD();
                        string model = "?";
                        string type = "?";
                        string size = "?";
                        try { model = drive["Model"].ToString().Trim(); } catch { }
                        try { type = drive["InterfaceType"].ToString().Trim(); } catch { }
                        hdd.Model = model;
                        hdd.Type = type;

                        //- Let's do some crazy math to get the actual size of the HDD
                        try
                        {
                            ulong toalsize = Convert.ToUInt64(drive.Properties["Size"].Value.ToString());
                            double toal = Convert.ToDouble(toalsize / (1024 * 1024));
                            int t = Convert.ToInt32(Math.Ceiling(toal / 1024).ToString());
                            size = t.ToString() + " GB";
                        }
                        catch { }
                        hdd.Size = size;

                        drivesDict.Add(dIndex, hdd);
                        dIndex++;
                    }
                    catch { }
                }

                var pmMos = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

                /* RETRIEVE HDD SERIAL NUMBER */
                dIndex = 0;
                foreach (ManagementObject drive in pmMos.Get())
                {
                    /* BECAUSE ALL PHYSICAL MEDIA WILL BE RETURNED WE NEED TO EXIT
                     * AFTER THE HARD DRIVES SERIAL INFO IS EXTRACTED */
                    if (dIndex >= drivesDict.Count)
                    { break; }

                    drivesDict[dIndex].Serial = drive["SerialNumber"] == null ? "None" : drive["SerialNumber"].ToString().Trim();
                    dIndex++;
                }

                /* GET WMI ACCESS TO HDD */
                var mos = new ManagementObjectSearcher("Select * from Win32_DiskDrive");
                mos.Scope = new ManagementScope(@"\root\wmi");

                /* CHECK IF SMART REPORTS THE DRIVE IS FAILING */
                mos.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictStatus");
                dIndex = 0;
                foreach (ManagementObject drive in mos.Get())
                {
                    drivesDict[dIndex].IsOK = (bool)drive.Properties["PredictFailure"].Value == false;
                    dIndex++;
                }

                /* RETRIVE ATTRIBUTE FLAGS, VALUE WORSTE AND VENDOR DATA INFORMATION */
                mos.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictData");
                dIndex = 0;
                foreach (ManagementObject data in mos.Get())
                {
                    Byte[] bytes = (Byte[])data.Properties["VendorSpecific"].Value;
                    for (int i = 0; i < 30; ++i)
                    {
                        try
                        {
                            int id = bytes[i * 12 + 2];

                            int flags = bytes[i * 12 + 4]; /* LEAST SIGNIFICANT STATUS BYTE, +3 MOST SIGNIFICANT BYTE, BUT NOT USED SO IGNORED
                                                            * BOOL ADVISORY = (FLAGS & 0X1) == 0X0;
                                                            * FLAGS CAUSING CONDITIONAL ISSUES, SO REMOVED. */

                            bool failureImminent = (flags & 0x1) == 0x1;
                            /* BOOL ONLINEDATACOLLECTION = (FLAGS & 0X2) == 0X2; */

                            int value = bytes[i * 12 + 5];
                            int worst = bytes[i * 12 + 6];
                            int vendordata = BitConverter.ToInt32(bytes, i * 12 + 7);
                            if (id == 0) { continue; }
                            if (!drivesDict.ContainsKey(dIndex)) { continue; }
                            if (!drivesDict[dIndex].Attributes.ContainsKey(id)) { continue; }

                            var attr = drivesDict[dIndex].Attributes[id];
                            attr.Current = value;
                            attr.Worst = worst;
                            attr.Data = vendordata;
                            attr.IsOK = failureImminent == false;
                        }
                        catch
                        { /* GIVEN KEY DOES NOT EXIST IN ATTRIBUTE COLLECTION (ATTRIBUTE NOT IN THE DICTIONARY OF ATTRIBUTES) */ }
                    }
                    dIndex++;
                }

                /* RETREIVE THRESHOLD VALUES FOREACH ATTRIBUTE */
                mos.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictThresholds");
                dIndex = 0;
                foreach (ManagementObject data in mos.Get())
                {
                    Byte[] bytes = (Byte[])data.Properties["VendorSpecific"].Value;
                    for (int i = 0; i < 30; ++i)
                    {
                        try
                        {

                            int id = bytes[i * 12 + 2];
                            int thresh = bytes[i * 12 + 3];
                            if (id == 0) continue;
                            else if (!drivesDict.ContainsKey(dIndex)) { continue; }
                            else if (!drivesDict[dIndex].Attributes.ContainsKey(id)) { continue; }

                            var attr = drivesDict[dIndex].Attributes[id];
                            attr.Threshold = thresh;
                        }
                        catch
                        { /* GIVEN KEY DOES NOT EXIST IN ATTRIBUTE COLLECTION (ATTRIBUTE NOT IN THE DICTIONARY OF ATTRIBUTES) */ }
                    }

                    dIndex++;
                }
            }
            catch (ManagementException ex)
            { /* TO DO, LOGGING WILL GO HERE SOME DAY. */ }
        }
    }
}
