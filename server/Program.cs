using System;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace server
{
    public class Program
    {
        public Program()
        {
            this.ClearFolder(@"C:\To");
            ServiceHost host = new ServiceHost(typeof(Server));
            host.Open();

            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            new Program();
        }

        private void ClearFolder(string folderName)
        {
            DirectoryInfo dirName = new DirectoryInfo(folderName);

            foreach (FileInfo fileInfo in dirName.GetFiles())
            {
                fileInfo.Delete();
            }

            foreach (DirectoryInfo directoryInfo in dirName.GetDirectories())
            {
                ClearFolder(directoryInfo.FullName);
                directoryInfo.Delete();
            }
        }
    }

    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        IList<FileContract> GetFiles(DateTime lastModification);

        [OperationContract]
        void SetFile(FileContract fileContract);

        [OperationContract]
        string SendMessage(string command, string value);
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class FileContract
    {
        [DataMember]
        public byte[] Bytes { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public Status FileStatus { get; set; }

        [DataMember]
        public DateTime LastModification { get; set; }

        [DataMember]
        public User User { get; set; }

        public string FileName
        {
            get
            {
                return Path.GetFileName(this.FilePath);
            }
        }
    }

    [DataContract]
    public enum Status
    {
        [EnumMember]
        New,

        [EnumMember]
        Deleted
    }

    [DataContract]
    public class User
    {
        [DataMember]
        public string Ip { get; set; }
    }

    public class Server : IServer
    {
        const string dir = @"C:\To\";

        private static List<FileContract> FilesDb { get; set; } = new List<FileContract>();

        public IList<FileContract> GetFiles(DateTime lastModification)
        {
            return FilesDb
                .Where(file => file.LastModification >= lastModification)
                .ToList();
        }

        public void SetFile(FileContract fileContract)
        {
            switch (fileContract.FileStatus)
            {
                case Status.New:
                    this.SetNewFile(fileContract);
                    break;
                case Status.Deleted:
                    this.SetDeletedFile(fileContract);
                    break;
                default:
                    throw new Exception("Status not matching!");
            }
        }

        private void SetNewFile(FileContract fileContract)
        {
            var path = Path.Combine(dir, fileContract.FilePath);
            var dirname = Path.GetDirectoryName(path);

            if (!Directory.Exists(dirname))
            {
                Directory.CreateDirectory(dirname);
            }

            File.WriteAllBytes(path, fileContract.Bytes);

            fileContract.LastModification = DateTime.Now;
            FilesDb.Add(fileContract);

            Console.Clear();
            FilesDb.ForEach(x => { Console.Write("File name: " + x.FileName); Console.Write(" File status: " + x.FileStatus + "\n"); });

        }

        private void SetDeletedFile(FileContract fileContract)
        {
            var path = Path.Combine(dir, fileContract.FilePath);
            File.Delete(path);

            if (FilesDb.Any(file => file.FilePath == fileContract.FilePath))
            {
                var file = FilesDb.Single(f => f.FilePath == fileContract.FilePath);

                file.FileStatus = Status.Deleted;
                file.LastModification = DateTime.Now;
            }

            Console.Clear();
            FilesDb.ForEach(x => { Console.Write("File name: " + x.FileName); Console.Write(" File status: " + x.FileStatus + "\n"); });

        }

        public string SendMessage(string command, string value)
        {
            string response = "";
            switch (command)
            {
                case "login":
                    response = "Zalogowano pomyślnie !";
                    Console.WriteLine("Użytkownik o adresie " + value + " zalogował się do serwera");
                    break;
            }
            return response;
        }

    }
}
