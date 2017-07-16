using System;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.ServiceModel.Description;
using System.Net;
using System.IO.MemoryMappedFiles;

namespace server
{


    public class Program
    {
        const string dir = @"C:\To\";

        public Program()
        {
            RunServer();
        }

        static void Main(string[] args)
        {
            new Program();
        }

        private void RunServer()
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            ClearFolder(dir);

            var hostName = Dns.GetHostName();
            var myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            var baseAddress = "http://" + myIP;

            ServiceHost host = new ServiceHost(typeof(Server)/*, new Uri(baseAddress)*/);

            host.Open();

            Console.WriteLine("Server is ready...");
            Console.ReadLine();
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



    [DataContract]
    public class FileContract
    {
        [DataMember]
        public byte[] Bytes { get; set; }

        [DataMember]
        public FileStream FileStream { get; set; } 

        [DataMember]
        public Stream Stream { get; set; }

        [DataMember]
        public string OldFilePath { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public Status FileStatus { get; set; }

        [DataMember]
        public DateTime LastModification { get; set; }
    }

    [DataContract]
    public enum Status
    {
        [EnumMember]
        New,

        [EnumMember]
        Deleted,

        [EnumMember]
        Renamed,

        [EnumMember]
        Modified
    }

    public class Server : IServer
    {
        
        const string dir = @"C:\To\";

        private static List<FileContract> FilesDb { get; set; } = new List<FileContract>();
        private static int bajty = 0;

        public IList<FileContract> GetFiles(DateTime lastModification)
        {
            return FilesDb
                .Where(file => file.LastModification > lastModification)
                .ToList();
        }

        private void DisplayFilesList()
        {
            Console.Clear();
            Console.WriteLine("Liczba zapamiętanych plikow na serwerze: " + FilesDb.Count());
            FilesDb.ForEach(x => {
                Console.WriteLine("Path: " + x.FilePath);
                Console.WriteLine("Old Path: " + x.OldFilePath);
                Console.WriteLine("Status: " + x.FileStatus);
                Console.WriteLine("Last modification: " + x.LastModification + ":" + x.LastModification.Millisecond);
                Console.WriteLine("_________________________________________________________");
            });
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
                case Status.Renamed:
                    this.SetRenamedFile(fileContract);
                    break;
                case Status.Modified:
                    this.SetModifiedFile(fileContract);
                    break;
                default:
                    throw new Exception("Status not matching!");
            }
        }

        private async void SetNewFile(FileContract fileContract)
        {
            var path = Path.Combine(dir, fileContract.FilePath);
            var dirname = Path.GetDirectoryName(path);

            if (!Directory.Exists(dirname))
            {
                Directory.CreateDirectory(dirname);
            }

            //File.WriteAllBytes(path, fileContract.Bytes);
            FileStream fsNew;
            
            if(FilesDb.Any(file => file.FilePath == fileContract.FilePath))
            {
                var file = FilesDb.Single(f => f.FilePath == fileContract.FilePath);

                bajty += fileContract.Bytes.Length;

                if(file.FileStatus == Status.New)
                {
                    fsNew = new FileStream(@"C:/To/file0", FileMode.Append, FileAccess.Write);
                    fsNew.Write(fileContract.Bytes, bajty, fileContract.Bytes.Length);
                    fsNew.Flush();
                    fsNew.Close();
                    fsNew.Dispose();

                }
            } else
            {
                fsNew = new FileStream(@"C:/To/file0", FileMode.Create, FileAccess.Write);
                fsNew.Write(fileContract.Bytes, 0, fileContract.Bytes.Length);
                fsNew.Flush();
                fsNew.Close();
                fsNew.Dispose();
                FilesDb.Add(fileContract);
            }



            /*fileContract.LastModification = DateTime.Now;
            fileContract.LastModification = fileContract.LastModification.AddTicks(-fileContract.LastModification.Ticks % TimeSpan.TicksPerSecond);

            if (FilesDb.Any(file => file.FilePath == fileContract.FilePath))
            {
                var file = FilesDb.Single(f => f.FilePath == fileContract.FilePath);
                file.FileStatus = fileContract.FileStatus;
            } else
            {
                
            }*/

            //this.DisplayFilesList();
        }

        private void DeleteDirectory(string sDir)
        {
            if (Directory.Exists(sDir))
            {
                foreach (var subdirectory in Directory.GetDirectories(sDir))
                {
                    if (Directory.GetFileSystemEntries(subdirectory).Length == 0)
                    {
                        Directory.Delete(subdirectory);
                    }
                    else
                    {
                        DeleteDirectory(subdirectory);
                    }
                }
            }
        }

        private void SetDeletedFile(FileContract fileContract)
        {
            var path = Path.Combine(dir, fileContract.FilePath);
            File.Delete(path);
            DeleteDirectory(dir);

            if (FilesDb.Any(file => file.FilePath == fileContract.FilePath))
            {
                var file = FilesDb.Single(f => f.FilePath == fileContract.FilePath);

                file.FileStatus = Status.Deleted;
                file.LastModification = DateTime.Now;
                fileContract.LastModification = fileContract.LastModification.AddTicks(-fileContract.LastModification.Ticks % TimeSpan.TicksPerSecond);
            }

            this.DisplayFilesList();
        }

        private void SetRenamedFile(FileContract fileContract)
        {
            var oldPath = Path.Combine(dir, fileContract.FilePath);
            var newPath = Path.Combine(dir, fileContract.OldFilePath);

            if (FilesDb.Any(file => file.FilePath == fileContract.FilePath))
            {
                File.Move(oldPath, newPath);

                var file = FilesDb.Single(f => f.FilePath == fileContract.FilePath);
               
                file.FileStatus = Status.Renamed;
                file.LastModification = DateTime.Now;
                fileContract.LastModification = fileContract.LastModification.AddTicks(-fileContract.LastModification.Ticks % TimeSpan.TicksPerSecond);
                var tmp = file.FilePath;
                file.FilePath = fileContract.OldFilePath;
                file.OldFilePath = tmp;

            } else
            {
                Directory.Move(oldPath, newPath);

                List<int> indexes = new List<int>();

                FilesDb.ForEach(x =>
                {
                    if (x.FilePath.Contains(fileContract.FilePath))
                    {
                        indexes.Add(FilesDb.IndexOf(x));
                    }
                });

                foreach (int i in indexes)
                {
                    var cutDir = FilesDb[i].FilePath.Substring(fileContract.FilePath.Length);
                    var tmp = FilesDb[i].FilePath;
                    FilesDb[i].FilePath = fileContract.OldFilePath + cutDir;
                    FilesDb[i].OldFilePath = tmp;
                    FilesDb[i].FileStatus = Status.Renamed;
                    FilesDb[i].LastModification = DateTime.Now;
                    FilesDb[i].LastModification = FilesDb[i].LastModification.AddTicks(-FilesDb[i].LastModification.Ticks % TimeSpan.TicksPerSecond);
                }
            }

            this.DisplayFilesList();
        }

        private void SetModifiedFile(FileContract fileContract)
        {
            var path = Path.Combine(dir, fileContract.FilePath);

            File.WriteAllBytes(path, fileContract.Bytes);

            fileContract.LastModification = DateTime.Now;
            fileContract.LastModification = fileContract.LastModification.AddTicks(-fileContract.LastModification.Ticks % TimeSpan.TicksPerSecond);

            if (FilesDb.Any(file => file.FilePath == fileContract.FilePath))
            {
                var file = FilesDb.Single(f => f.FilePath == fileContract.FilePath);
                file.FileStatus = fileContract.FileStatus;
                file.LastModification = fileContract.LastModification;
                file.Bytes = fileContract.Bytes;
            }

            this.DisplayFilesList();
        }

        public string SendMessage(string command, string value)
        {
            string response = "";
            switch (command)
            {
                case "login":
                    response = "Zalogowano pomyślnie ! Wciśnij F2 by synchronizować pliki";
                    Console.WriteLine("Użytkownik o adresie " + value + " zalogował się do serwera");
                    break;

                case "check":
                    break;
            }
            return response;
        }

    }
}
