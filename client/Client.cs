using client.ServiceReference1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;

namespace client
{
    public class Client
    {
        const string dir = @"C:\From\";
        private IServer wcfClient;
        private EndpointAddress endPoint;
        private ChannelFactory<IServer> myChannelFactory;
        private List<string> files;
        private DateTime date;
        bool isItConnected = false;

        public Client()
        {
            
            this.CreateChannel();
       
            if(isItConnected)
            {
                this.SetFileWatcher();
            }

            do
            {
                if (Console.ReadKey(true).Key == ConsoleKey.F1)
                {
                    this.SendFiles(this.GetAllFiles(dir));
                }
                if (Console.ReadKey(true).Key == ConsoleKey.F2)
                {
                    IList<FileContract> fb =  wcfClient.GetFiles(date);

                    foreach(FileContract fc in fb)
                    {
                        Console.WriteLine(fc.FilePath + " " + fc.FileStatus);
                        var path = Path.Combine(dir, fc.FilePath);

                        if (fc.FileStatus == Status.New)
                        {
                            File.WriteAllBytes(path, fc.Bytes);
                        }
                        if (fc.FileStatus == Status.Deleted)
                        {
                            Console.WriteLine("Path " + path);
                            Console.WriteLine("Weszło w DELETED");
                            File.Delete(path);
                        }

                    }

                }
            } while (Console.ReadKey(true).Key != ConsoleKey.F12);


            Console.ReadKey();
        }
        
        private IServer CreateChannel()
        {
            bool invalid = true;
            //string adres;

            BasicHttpBinding myBinding = new BasicHttpBinding();

            do
            {
                //Console.WriteLine("Podaj adres serwera \n");
                //adres = Console.ReadLine();
                try
                {
                    endPoint = new EndpointAddress("http://192.168.1.100/service");
                    myChannelFactory = new ChannelFactory<IServer>(myBinding, endPoint);
                    wcfClient = myChannelFactory.CreateChannel();

                    invalid = false;
                }
                catch (UriFormatException ce)
                {
                    Console.WriteLine(ce.Message + "\n");
                    invalid = true;
                }
                catch (EndpointNotFoundException ce)
                {
                    Console.WriteLine(ce.Message + "\n");
                    invalid = true;
                }

            } while (invalid);

            isItConnected = true;

            return wcfClient;
        }

        private List<string> GetAllFiles(string sDirt)
        {
            files = new List<string>();

            try
            {
                foreach (string file in Directory.GetFiles(sDirt))
                {
                    files.Add(file);
                }
                foreach (string fl in Directory.GetDirectories(sDirt))
                {
                    files.AddRange(GetAllFiles(fl));
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            return files;
        }

        private void SendFiles(List<string> files)
        {

            files.ForEach(file =>
            {
                var bytes = File.ReadAllBytes(file);
                wcfClient.SetFile(new FileContract
                {
                    FileStatus = Status.New,
                    Bytes = bytes,
                    FilePath = file.Substring(dir.Length)
                });
            });

        }

        private void DeleteFile(string file)
        {
            wcfClient.SetFile(new FileContract
            {
                FileStatus = Status.Deleted,
                FilePath = file.Substring(dir.Length)
            });
        }

        private void SetFileWatcher()
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = dir;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
        }

        private bool isItDirectory(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return true;
            else
                return false;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if(e.ChangeType.ToString() == "Created")
            {
                date = DateTime.Now;
            }
            if (e.ChangeType.ToString() == "Deleted")
            {
                date = DateTime.Now;
                this.DeleteFile(e.FullPath);
            }
            if (e.ChangeType.ToString() == "Changed")
            {
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        private String getIP()
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST

            // Get the IP
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

            return myIP;
        }


        static void Main(string[] args)
        {

            new Client();

        }

    }
}
