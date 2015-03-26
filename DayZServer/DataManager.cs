﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace DayZServer
{
    class DataManager
    {
        public string defaultPath;
        public string[] dirs;
        public string json;
        public string servername;
        public string IPAddress;
        public string version;
        public string appDataPath;
        public string path;
        public string dayzapppath;
        public string dayzpath;
        public string serverhistorypath;
        public string currentserverpath;
        public List<Server> server_list;
        
         public DataManager()
        {
            defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString();
            dirs = Directory.GetFiles(defaultPath + @"\DayZ", "*.DayZProfile");
            dirs = dirs.Where(w => w != dirs[1]).ToArray();
            string configpath = dirs[0];
            //Console.WriteLine("config", configpath);
            json = System.IO.File.ReadAllText(configpath);
            servername = json.Split(new string[] { "lastMPServerName=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\";" }, StringSplitOptions.None)[0].Trim();
            IPAddress = json.Split(new string[] { "lastMPServer=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\";" }, StringSplitOptions.None)[0].Trim();
            version = json.Split(new string[] { "version=" }, StringSplitOptions.None)[1].Split(new string[] { ";" }, StringSplitOptions.None)[0].Trim();
            appDataPath = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            path = System.IO.Path.Combine(appDataPath, "DayZServer");
            dayzapppath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path, "dayzapppath.txt");
            dayzpath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "steam", "SteamApps", "common", "DayZ", "DayZ.exe");
            serverhistorypath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path, "dayzhistory.txt");
            currentserverpath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path, "dayzserver.txt");
            //Console.WriteLine("Server History Path: " + serverhistorypath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                if (!File.Exists(dayzapppath))
                {
                    writeAppPath(dayzpath);
                }
            }
        }

         public class RootObject
         {
             public List<Server> Server { get; set; }
         }

        public class Server
        {
            public string ServerName { get; set; }
            public string IP_Address { get; set; }
            public DateTime Date { get; set; }
            public string Favorite { get; set; }
            public string Current { get; set; }
        }

        public void writeServerHistoryList()
        {
            if (File.Exists(serverhistorypath))
            {
                string temphistory;
                try
                {
                    using (StreamReader sreader = new StreamReader(serverhistorypath))
                    {
                        temphistory = sreader.ReadToEnd();
                        sreader.Close();
                        string matchIdToFindServer = servername;
                        string matchIdToFindIPAddress = IPAddress;
                        List<Server> customData = JsonConvert.DeserializeObject<List<Server>>(temphistory);
                        Server match = customData.FirstOrDefault(x => x.ServerName == servername);
                        int index = customData.FindIndex(x => x.ServerName == servername);

                        if (match != null)
                        {
                            match.Date = DateTime.Now;
                            match.IP_Address = IPAddress;
                            match.Current = "1";
                            Server replacement = match;
                            customData[index] = replacement;
                            File.Delete(serverhistorypath);
                            string listjson = JsonConvert.SerializeObject(customData.ToArray());
                            using (StreamWriter sw = File.CreateText(serverhistorypath))
                            {
                                sw.Write(listjson);
                                sw.Close();
                            } 
                        }
                        else
                        {
                            customData.Add(new Server()
                            {
                                ServerName = servername,
                                IP_Address = IPAddress,
                                Date = DateTime.Now,
                                Favorite = "1",
                             });

                            string listjson = JsonConvert.SerializeObject(customData.ToArray());
                            using (StreamWriter sw = File.CreateText(serverhistorypath))
                            {
                                sw.Write(listjson);
                                sw.Close();
                            } 
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception" + e);
                }
            }
            else
            {
                try
                {
                    using (StreamWriter sw = File.CreateText(serverhistorypath))
                    {
                        var server_items = new List<Server>();
                        server_items.Add(new Server()
                        {
                            ServerName = servername,
                            IP_Address = IPAddress,
                            Date = DateTime.Now,
                            Favorite = "0",
                            Current = "1"
                        });

                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(sw, server_items);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public List<Server> getServerList(string serverhistorypath)
        {
            if (File.Exists(serverhistorypath))
            {
                string temphistory;
                try
                {
                    using (StreamReader sreader = new StreamReader(serverhistorypath))
                    {
                        temphistory = sreader.ReadToEnd();
                        server_list = JsonConvert.DeserializeObject<List<Server>>(temphistory);
                        return server_list;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception" + e);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public void writeAppPath(string dayzpath)
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                string path = System.IO.Path.Combine(appDataPath, "DayZServer");
                string dayzapppath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path, "dayzapppath.txt");

                using (StreamWriter sw = File.CreateText(dayzapppath))
                {
                    if (sw.BaseStream != null)
                    {
                        sw.WriteLine(dayzpath);
                        sw.Close();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        String readAppPath(string dayzapppath)
        {
            if (File.Exists(dayzapppath))
            {
                try
                {
                    using (StreamReader sw = new StreamReader(dayzapppath))
                    {
                        String line = sw.ReadToEnd();
                        return line;
                    }
                }
                catch (Exception e)
                {
                    return dayzpath;
                }
            }
            else
            {
                writeAppPath(dayzpath);
                return dayzpath;
            }
        }
    }
}