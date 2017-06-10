using client.ServerReference;
using System;
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
        private BasicHttpBinding myBinding;
        private ChannelFactory<IServer> myChannelFactory;
        private List<string> files;
        bool isItConnected = false;

        public Client()
        {
            
            this.CreateChannel();
       
            if(isItConnected)
            {
                this.SetFileWatcher();
            }
            this.GetAllFiles(dir);
           
            Console.ReadKey();
        }
        
        private IServer CreateChannel()
        {
            bool invalid = true;
            string adres;

            myBinding = new BasicHttpBinding();

            do
            {
                //Console.WriteLine("Podaj adres serwera \n");
                //adres = Console.ReadLine();
                try
                {
                    endPoint = new EndpointAddress("http://piotr-komputer/service");
                    myChannelFactory = new ChannelFactory<IServer>(myBinding, endPoint);
                    wcfClient = myChannelFactory.CreateChannel();

                    Console.WriteLine(this.wcfClient.sendMessage(("login"), this.getIP()));

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
                    Bytes = bytes,
                    FilePath = file.Substring(dir.Length)
                });
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

        private void DeleteFile(string path)
        {
            if(files.Contains(path))
            {
                Console.WriteLine("Usunięto plik " + path);
                File.Delete(path);
                files.Remove(path);
            }
        }

        private void DeleteDirectory(string path)
        {
            if(files.Contains(path))
            {
                Console.WriteLine("Usunięto folder" + path);
                Directory.Delete(path, true);
                files.Remove(path);
            }
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

                Console.WriteLine("Utworzono plik");
                files = this.GetAllFiles(dir);
                this.SendFiles(files);
            }
            if(e.ChangeType.ToString() == "Deleted")
            {
                if(this.isItDirectory(e.FullPath))
                {
                    this.DeleteDirectory(this.wcfClient.sendMessage("delete", e.FullPath))
                } else
                {
                    this.DeleteFile(this.wcfClient.sendMessage("delete", e.FullPath))
                }
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

        private void CloseChannel()
        {
            ((IClientChannel)this.wcfClient).Close();
        }

        static void Main(string[] args)
        {
            new Client();            
        }

    }
}
