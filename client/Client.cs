using client.ServiceReference1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Linq;
using System.Threading;

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
        FileSystemWatcher watcher = new FileSystemWatcher();
        int count = 0;
        DateTime tmpDate;

        public Client()
        {
            
            this.ConnectToService();
       
            if(isItConnected)
            {
                this.SetFileWatcher();

                do
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.F2)
                    {
                        watcher.EnableRaisingEvents = false;
                        if(this.GetListFileContract().Count > 0)
                        {
                            foreach (FileContract fileContract in this.GetListFileContract())
                            {
                                var path = Path.Combine(dir, fileContract.FilePath);
                                var dirname = Path.GetDirectoryName(path);

                                if (fileContract.FileStatus == Status.New)
                                {
                                    if (!Directory.Exists(dirname))
                                    {
                                        Directory.CreateDirectory(dirname);
                                    }
                                    File.WriteAllBytes(path, fileContract.Bytes);
                                    files.Add(path);
                                }
                                else if (fileContract.FileStatus == Status.Modified)
                                {
                                    Console.WriteLine("1");
                                    if(!Directory.Exists(dirname))
                                    {
                                        Console.WriteLine("2");
                                        Directory.CreateDirectory(dirname);
                                    }
                                    if (!files.Any(file => file == path))
                                    {
                                        files.Add(path);
                                    }
                                    File.WriteAllBytes(path, fileContract.Bytes);
                                }
                                else if (fileContract.FileStatus == Status.Deleted)
                                {
                                    this.DeleteFiles(path, false);
                                    this.DeleteDirectory();
                                }
                                else if (fileContract.FileStatus == Status.Renamed)
                                {
                                    var oldPath = Path.Combine(dir, fileContract.OldFilePath);
                                    var oldDirName = Path.GetDirectoryName(oldPath);

                                    var oldFileName = Path.GetFileName(oldPath);
                                    var fileName = Path.GetFileName(path);

                                    if (files.Any(file => file == oldPath) && oldFileName != fileName)
                                    {
                                        var newPath = Path.Combine(dir, fileContract.FilePath);

                                        this.RenameFiles(oldPath, newPath, false);
                                    }
                                    else
                                    {
                                        if (Directory.Exists(oldDirName))
                                        {
                                            this.RenameFiles(oldDirName, dirname, false);
                                            break;
                                        } else
                                        {
                                            Directory.CreateDirectory(dirname);
                                            File.WriteAllBytes(path, fileContract.Bytes);
                                        }
                                    }
                                }
                            }
                            this.DisplayFilesList();
                        }

                        watcher.EnableRaisingEvents = true;
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.F12);
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
                    endPoint = new EndpointAddress("http://192.168.1.3/service");
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
            try
            {
                listFileContract = new List<FileContract>();
                listFileContract = wcfClient.GetFiles(this.GetLastModificationDate(this.files));
            }
            catch (EndpointNotFoundException ce)
            {
                Console.WriteLine(ce.Message + "\n");
            }
            return listFileContract;
        }

        private DateTime GetLastModificationDate(List<string> files)
        {
            List<DateTime> dates = new List<DateTime>();
            if (files.Count() != 0)
            {
                foreach (string file in files)
                {
                    dates.Add(File.GetLastWriteTime(file));
                }
                lastModificationDate = dates.Max();

                return lastModificationDate;
            } else
            {
                return new DateTime();
            }
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
            try
            {

                var bytes = File.ReadAllBytes(file);

                wcfClient.SetFile(new FileContract
                {
                    FileStatus = Status.New,
                    Bytes = bytes,
                    FilePath = file.Substring(dir.Length)
                });
                files.Add(file);
                File.SetLastWriteTime(file, DateTime.Now);


            }
            catch (FileNotFoundException ce)
            {
                Console.WriteLine(ce);

            }
            catch (IOException)
            {
                Console.WriteLine("Another user is already using this file.");
                this.SendFile(file);
            }
        }

        private void SendEditFile(string file)
        {
            try
            {
                var bytes = File.ReadAllBytes(file);

                wcfClient.SetFile(new FileContract
                {
                    FileStatus = Status.Modified,
                    Bytes = bytes,
                    FilePath = file.Substring(dir.Length)
                });
                File.SetLastWriteTime(file, DateTime.Now);
            }
            catch (IOException)
            {
                Console.WriteLine("Another user is already using this file.");
                this.SendEditFile(file);
            }
        }

        private void DeleteDirectory()
        {
            if (Directory.Exists(dir))
            {
                //Delete all child Directories if they are empty

                foreach (string subdirectory in Directory.GetDirectories(dir))
                {
                    string[] file = Directory.GetFiles(subdirectory, "*.*");

                    if (file.Length == 0)
                        Directory.Delete(subdirectory);
                }
            }
        }

        private void DeleteFiles(string directory, bool toSend)
        {
            bool itIsDirectory = false;

            foreach (string file in files)
            {
                if(file.Equals(directory))
                {
                    itIsDirectory = false;
                    
                    if(toSend)
                    {
                        wcfClient.SetFile(new FileContract
                        {
                            FileStatus = Status.Deleted,
                            FilePath = file.Substring(dir.Length)
                        });
                    } else
                    {
                        File.Delete(directory);
                    }
                    files.Remove(file);
                    break;
                } else
                {
                    itIsDirectory = true;
                }
            }
            if(itIsDirectory)
            {
                Console.WriteLine("WESZŁO");
                Console.ReadLine();
                foreach (string file in files)
                {
                    if (file.Contains(directory))
                    {
                        if(toSend)
                        {
                            wcfClient.SetFile(new FileContract
                            {
                                FileStatus = Status.Deleted,
                                FilePath = file.Substring(dir.Length)
                            });
                        } else
                        {
                            Directory.Delete(directory, true);
                            break;
                        }
                    }
                }
                files.RemoveAll(file => file.Contains(directory));
            }
        }

        private void RenameFiles(string oldPath, string newPath, bool toSend)
        {

            bool itIsDirectory = false;

            foreach (string file in files)
            {
                if (file.Equals(oldPath))
                {
                    itIsDirectory = false;

                    if(toSend)
                    {
                        wcfClient.SetFile(new FileContract
                        {
                            FileStatus = Status.Renamed,
                            FilePath = oldPath.Substring(dir.Length),
                            OldFilePath = newPath.Substring(dir.Length)
                        });
                    } else
                    {
                        File.Move(oldPath, newPath);
                    }

                    int index = files.FindIndex(f => f == oldPath);
                    files[index] = newPath;
                    File.SetLastWriteTime(newPath, DateTime.Now);
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
                if(toSend)
                {
                    wcfClient.SetFile(new FileContract
                    {
                        FileStatus = Status.Renamed,
                        FilePath = oldPath.Substring(dir.Length),
                        OldFilePath = newPath.Substring(dir.Length)
                    });
                } else
                {
                    Directory.Move(oldPath, newPath);
                }
                foreach (int i in indexes)
                {
                    var cutDir = files[i].Substring(oldPath.Length);
                    files[i] = newPath + cutDir;
                    File.SetLastWriteTime(files[i], DateTime.Now);
                }
            }
        }

        private void SetFileWatcher()
        {
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
            Console.WriteLine("Wciśnij F2 by zsynchronizować pliki z serwera");
            Console.WriteLine("Lista plików lokalnie: " + files.Count());
            this.files.ForEach(x =>
            {
                Console.WriteLine(x + " " + File.GetLastWriteTime(x) + ":" + File.GetLastWriteTime(x).Millisecond);
            });

        }

        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType.ToString() == "Created")
            {
                tmpDate = File.GetLastWriteTime(e.FullPath);
                Console.WriteLine(tmpDate);
                lastModificationDate = DateTime.Now;
                if (!this.isItDirectory(e.FullPath))
                {
                    this.SendFile(e.FullPath);
                }
                    
               
                this.DisplayFilesList();
            }
            if (e.ChangeType.ToString() == "Deleted")
            {

                lastModificationDate = DateTime.Now;
                this.DeleteFiles(e.FullPath, true);

                this.DisplayFilesList();
            }
            if (e.ChangeType.ToString() == "Changed")
            {
               
            }
            
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            this.RenameFiles(e.OldFullPath, e.FullPath, true);

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
