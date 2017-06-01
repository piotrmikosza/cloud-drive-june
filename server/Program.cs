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
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(Server));
            host.Open();
            Console.WriteLine("Server is ready...");
            Console.ReadKey();
        }
    }
    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        String sendMessage(String command, String value);

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

        public String sendMessage(String command, String value)
        {
            string response = "";
            switch (command)
            {
                case "login":
                    response = "Zalogowano pomyślnie !";
                    Console.WriteLine("Użytkownik o nazwie " + value + " zalogował się do serwera");
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
