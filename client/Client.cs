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

        public Client()
        {
            this.createChannel(this.setAddress());
            Console.WriteLine(this.wcfClient.sendMessage("login", getComputerName());
            this.GetAllFiles(dir);
            this.SetFileWatcher();
            Console.ReadKey();
        }
        private ServerReference.IServer getWcfClient()
        {
            return wcfClient;
        }

        private EndpointAddress setAddress()
        {
            Console.WriteLine("Zaloguj się do Cloud Drive");
            String adres = Console.ReadLine();

            EndpointAddress myEndpoint = new EndpointAddress(adres);

            //Dodać wyjątki że nie udało się zalogować
            Console.WriteLine("Adres ustawiony");

            return myEndpoint;
        }

        private ServerReference.IServer createChannel(EndpointAddress myEndpoint)
        {
            BasicHttpBinding myBinding = new BasicHttpBinding();

            ChannelFactory<ServerReference.IServer> myChannelFactory = new ChannelFactory<ServerReference.IServer>(myBinding, myEndpoint);

            wcfClient = myChannelFactory.CreateChannel();

            //Dodać wyjątki że nie udało się utworzyć kanału
            Console.WriteLine("Kanał utworzony");

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
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Filter = "*.*";
            watcher.Created += (obj, e) =>
            {
                var files = this.GetAllFiles(dir);
                this.SendFiles(files);
            };
            watcher.Changed += (obj, e) => { };
            watcher.Deleted += (obj, e) => { };

            watcher.EnableRaisingEvents = true;
        }
        private String getComputerName()
        {
            string name = Environment.MachineName;
            return name;
        }

        static void Main(string[] args)
        {
            new Client();            
        }

    }
}
