using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.IO;

namespace server
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(Server));
            host.Open();
            Console.WriteLine("Cloud Drive is ready...");
            Console.ReadKey();
        }
    }

    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        String sendMessage(String command, FileContract fc);

        [OperationContract]
        void SetFile(FileContract fileContract);

        [OperationContract]
        void SendFile(FileContract fileContract);

    }

    [DataContract]
    public class FileContract
    {
        [DataMember]
        public byte[] Bytes { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string IP { get; set; }

        public string FileName
        {
            get
            {
                return Path.GetFileName(this.FilePath);
            }
        }

        public List<string> Clients
        {
            get
            {
                Clients.Add(this.IP);

                return Clients;
            }
        }
    }

    public class Server : IServer
    {
        const string dir = @"C:\To\";


        public void SendFile(FileContract fileContract)
        {
            throw new NotImplementedException();
        }

        public String sendMessage(string command, FileContract fc)
        {
            string response = "";
            switch (command)
            {
                case "login":
                    Console.WriteLine(fc.IP);
                    Console.WriteLine("Login");
                    fc.Clients.ForEach(Console.WriteLine);
                    Console.WriteLine("End Login");
                    break;
                case "Created":
                    Console.WriteLine("Created");
                    fc.Clients.ForEach(Console.WriteLine);
                    Console.WriteLine("End Created");
                    break;
            }
            return response;
        }

        public void SetFile(FileContract fileContract)
        {
            var path = Path.Combine(dir, fileContract.FilePath);
            var dirname = Path.GetDirectoryName(path);

            if (!Directory.Exists(dirname))
            {
                Directory.CreateDirectory(dirname);
            }

            File.WriteAllBytes(path, fileContract.Bytes);

        }
    }
}
