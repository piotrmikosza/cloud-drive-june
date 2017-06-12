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
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(Server));
            host.Open();

            Console.ReadLine();
        }
    }

    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        void SetFile(FileContract fileContract);

        [OperationContract]
        IList<FileContract> GetFiles(DateTime LastModification);
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

        /*[DataMember]
        public string Directory { get; set; }*/

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
       /* [EnumMember]
        NewDirectory,*/

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

        public IList<FileContract> GetFiles(DateTime LastModification)
        {
            return FilesDb
                .Where(file => file.LastModification >= LastModification)
                .ToList();
        }

        public void SetFile(FileContract fileContract)
        {
            switch (fileContract.FileStatus)
            {
                /*case Status.NewDirectory:
                    this.SetNewDirectory(fileContract);
                    break;*/
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

       /* private void SetNewDirectory(FileContract fileContract)
        {
            var path = Path.Combine(dir, fileContract.Directory);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }*/

        private void SetNewFile(FileContract fileContract)
        {
            var path = Path.Combine(dir, fileContract.FilePath);
            var dirname = Path.GetDirectoryName(path);

            Console.WriteLine("fC.FilePath " + fileContract.FilePath);
            Console.WriteLine("Path " + path);
            Console.WriteLine("Dirname " + dirname);

            if (!Directory.Exists(dirname))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllBytes(path, fileContract.Bytes);

            fileContract.LastModification = DateTime.Now;
            FilesDb.Add(fileContract);

            FilesDb.ForEach(x => { Console.WriteLine("File name " + x.FileName); Console.WriteLine("Last mod " + x.LastModification); });

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
        }
    }
}
