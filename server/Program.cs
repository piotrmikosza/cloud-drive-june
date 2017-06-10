using System;
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
        String sendMessage(string command, string value);

        [OperationContract]
        void SetFile(FileContract fileContract);
    
    }

    [DataContract]
    public class FileContract
    {
        [DataMember]
        public byte[] Bytes { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        public string FileName
        {
            get
            {
                return Path.GetFileName(this.FilePath);
            }
        }
    }

    public class Server : IServer
    {
        const string dir = @"C:\To\";

        public String sendMessage(string command, string value)
        {
            string response = "";
            switch (command)
            {
                case "login":
                    response = "Zalogowano";
                    Console.WriteLine("Połączył się user o adresie " + value);
                    break;
                case "logout":
                    Console.WriteLine("Rozłączył się user o adresie " + value);
                    break;
                case "add":
                    break;
                case "delete":
                    response = @"C:\From\plik.txt";
                    break;
                case "edit":
                    break;
                case "rename":
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
