using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    public class Client
    {
        const string dir = @"C:\From\";
        private ServerReference.IServer wcfClient;
        private EndpointAddress endPoint;

        public Client()
        {
            this.createChannel();
            wcfClient.sendMessage("login", new ServerReference.FileContract
            {
                IP = this.getIP()
            });
            this.GetAllFiles(dir);
            this.SetFileWatcher();
            Console.ReadKey();
        }
        private ServerReference.IServer getWcfClient()
        {
            return wcfClient;
        }
        

        private ServerReference.IServer createChannel()
        {
            bool invalid = true;
            BasicHttpBinding myBinding = new BasicHttpBinding();
            do
            {
                Console.WriteLine("Podaj adres serwera");
                String adres = Console.ReadLine();
                try
                {
                    endPoint = new EndpointAddress(adres);
                    invalid = false;
                }
                catch
                {
                    Console.WriteLine("Zły adres, spróbuj ponownie");
                }
            } while (invalid);

            
            ChannelFactory<ServerReference.IServer> myChannelFactory = new ChannelFactory<ServerReference.IServer>(myBinding, endPoint);

            wcfClient = myChannelFactory.CreateChannel();

            return wcfClient;
        }

        private void closeChannel()
        {
            ((IClientChannel)this.wcfClient).Close();
            Console.WriteLine("Kanał zamknięty");
        }

        private List<string> GetAllFiles(string sDirt)
        {
            List<string> files = new List<string>();

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

        private void sendIp(string ip)
        {

        }

        private void SendFiles(List<string> files)
        {
            files.ForEach(file =>
            {
                var bytes = File.ReadAllBytes(file);
                wcfClient.SetFile(new ServerReference.FileContract
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
        private String getIP()
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST

            // Get the IP
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

            return myIP;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine(e.ChangeType.ToString());
            if (e.ChangeType.ToString() == "Created")
            {
                wcfClient.sendMessage("Created", new ServerReference.FileContract
                {
                    IP = this.getIP()
                });
                var files = this.GetAllFiles(dir);
                this.SendFiles(files);
            }

        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        static void Main(string[] args)
        {
            new Client();            
        }

    }
}
