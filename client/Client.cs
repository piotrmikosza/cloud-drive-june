using client.ServiceReference1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Linq;

namespace client
{
    public class Client
    {
        const string dir = @"C:\From\";
        private IServer wcfClient;
        private EndpointAddress endPoint;
        private ChannelFactory<IServer> myChannelFactory;
        private List<string> files = new List<string>();
        private IList<FileContract> listFileContract;
        private static DateTime lastModificationDate;
        bool isItConnected = false;

        public Client()
        {
            
            this.ConnectToService();
       
            if(isItConnected)
            {
                //this.GetAllFiles(dir);
                this.SetFileWatcher();
                Console.ReadKey();

                /*foreach (FileContract fileContract in this.GetListFileContract())
                {
                    Console.WriteLine(fileContract.FilePath + " " + fileContract.FileStatus);
                    var path = Path.Combine(dir, fileContract.FilePath);
                    if (fileContract.FileStatus == Status.New)
                    {
                        File.WriteAllBytes(path, fileContract.Bytes);
                    }
                }*/

            }

        }

        private IServer ConnectToService()
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
                    endPoint = new EndpointAddress("http://piotr-komputer/service");
                    //endPoint = new EndpointAddress(adres);
                    myChannelFactory = new ChannelFactory<IServer>(myBinding, endPoint);
                    wcfClient = myChannelFactory.CreateChannel();
                    Console.WriteLine(wcfClient.SendMessage("login", this.getIP()));
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

        private IList<FileContract> GetListFileContract()
        {
            listFileContract = wcfClient.GetFiles(this.GetLastModificationDate(this.files));
            return listFileContract;
        }

        private DateTime GetLastModificationDate(List<string> files)
        {
            List<DateTime> dates = new List<DateTime>();

            foreach (string file in files)
            {
                dates.Add(File.GetLastWriteTime(file));
            }

            lastModificationDate = dates.Max();

            return lastModificationDate;
        }

        private List<string> GetAllFiles(string sourceDir)
        {
            try
            {
                foreach (string file in Directory.GetFiles(sourceDir))
                {
                    files.Add(file);
                }
                foreach (string fl in Directory.GetDirectories(sourceDir))
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

        private void SendFile(string file)
        {
            var bytes = File.ReadAllBytes(file);
            wcfClient.SetFile(new FileContract
            {
                FileStatus = Status.New,
                Bytes = bytes,
                FilePath = file.Substring(dir.Length)
            });
            files.Add(file);
        }

        private void DeleteFile(string file)
        {
            wcfClient.SetFile(new FileContract
            {
                FileStatus = Status.Deleted,
                FilePath = file.Substring(dir.Length)
            });
        }

        private void DeleteFiles(string directory)
        {
            bool itIsDirectory = false;

            foreach (string file in files)
            {
                if(file.Equals(directory))
                {
                    itIsDirectory = false;
                    wcfClient.SetFile(new FileContract
                    {
                        FileStatus = Status.Deleted,
                        FilePath = file.Substring(dir.Length)
                    });
                    files.Remove(file);
                    break;
                } else
                {
                    itIsDirectory = true;
                }
            }
            if(itIsDirectory)
            {
                foreach (string file in files)
                {
                    if (file.Contains(directory))
                    {
                        wcfClient.SetFile(new FileContract
                        {
                            FileStatus = Status.Deleted,
                            FilePath = file.Substring(dir.Length)
                        });
                    }
                }
                files.RemoveAll(file => file.Contains(directory));
            }
        }

        private void RenameFiles(string oldPath, string newPath)
        {

            bool itIsDirectory = false;

            foreach (string file in files)
            {
                if (file.Equals(oldPath))
                {
                    itIsDirectory = false;
                    wcfClient.SetFile(new FileContract
                    {
                        FileStatus = Status.Renamed,
                        FilePath = oldPath.Substring(dir.Length),
                        NewFilePath = newPath.Substring(dir.Length)
                    });
                    int index = files.FindIndex(f => f == oldPath);
                    files[index] = newPath;
                    break;
                }
                else
                {
                    itIsDirectory = true;
                }
            }
            if (itIsDirectory)
            {
                List<int> indexes = new List<int>();
                foreach (string file in files)
                {
                    if (file.Contains(oldPath))
                    {
                        indexes.Add(files.IndexOf(file));
                    }
                }
                wcfClient.SetFile(new FileContract
                {
                    FileStatus = Status.Renamed,
                    FilePath = oldPath.Substring(dir.Length),
                    NewFilePath = newPath.Substring(dir.Length)
                });
                foreach (int i in indexes)
                {
                    var cutDir = files[i].Substring(oldPath.Length);
                    files[i] = newPath + cutDir;
                }
            }
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

        private void DisplayFilesList()
        {
            Console.Clear();
            Console.WriteLine("Lista plików");
            this.files.ForEach(Console.WriteLine);
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if(e.ChangeType.ToString() == "Created")
            {
                lastModificationDate = DateTime.Now;
                if(!this.isItDirectory(e.FullPath))
                    this.SendFile(e.FullPath);

                this.DisplayFilesList();
            }
            if (e.ChangeType.ToString() == "Deleted")
            {
                lastModificationDate = DateTime.Now;
                this.DeleteFiles(e.FullPath);

                this.DisplayFilesList();
            }
            if (e.ChangeType.ToString() == "Changed") { }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            this.RenameFiles(e.OldFullPath, e.FullPath);
            this.DisplayFilesList();
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
