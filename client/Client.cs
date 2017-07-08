using client.ServiceReference1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Linq;
using System.Diagnostics;

namespace client
{
    public class Client
    {
        const string dir = @"C:\From\";
        private IServer wcfClient;
        private EndpointAddress endPoint;
        private ChannelFactory<IServer> myChannelFactory;
        private List<string> files = new List<string>();
        private List<string> sendFiles = new List<string>();
        private List<string> receiveFiles = new List<string>();
        private IList<FileContract> listFileContract;
        private DateTime lastModificationDate, modificationDate;
        bool isItConnected = false;
        bool invalid = true;
        FileSystemWatcher watcher = new FileSystemWatcher();
        private static int i = 0, j = 0;
        bool fileIsLocked = true;

        public Client()
        {
            
            ConnectToService();
            this.SetFileWatcher();

            var allFiles = GetAllFiles(dir);
            if(isItConnected)
            {
                SendFiles(allFiles);
                files.AddRange(allFiles);
                lastModificationDate = DateTime.Now;
            } else
            {
                ConnectToService();
            }

            do
            {
                /*Console.WriteLine("Aplikacja chodzi");
                try
                {
                    wcfClient.SendMessage("check", "");
                }
                catch (EndpointNotFoundException)
                {
                    Console.WriteLine("Serwer jest wyłączony. Sprawdź połączenie serwera!");
                    invalid = true;
                    isItConnected = false;
                    ConnectToService();
                }*/

                if (Console.ReadKey(true).Key == ConsoleKey.F1)
                {
                    DisplayFilesList();
                }
                if (Console.ReadKey(true).Key == ConsoleKey.F2)
                {
                    if(modificationDate > lastModificationDate)
                    {
                        SendFiles(sendFiles);
                        lastModificationDate = modificationDate;
                        files.AddRange(sendFiles);
                        sendFiles.Clear();
                    }
                    DisplayFilesList();
                }

                if (Console.ReadKey(true).Key == ConsoleKey.F3)
                {
                    watcher.EnableRaisingEvents = false;

                    if (GetListFileContract().Count > 0)
                    {
                        foreach (FileContract fileContract in GetListFileContract())
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
                                Console.WriteLine(path);
                            }
                            /*else if (fileContract.FileStatus == Status.Modified)
                            {
                                Console.WriteLine("1");
                                if (!Directory.Exists(dirname))
                                {
                                    Console.WriteLine("2");
                                    Directory.CreateDirectory(dirname);
                                }
                                if (!files.Any(file => file == path))
                                {
                                    files.Add(path);
                                }
                                File.WriteAllBytes(path, fileContract.Bytes);
                            }*/
                            else if (fileContract.FileStatus == Status.Deleted)
                            {
                                this.DeleteFiles(path, false);
                                this.DeleteDirectory(dir);
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
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(dirname);
                                        File.WriteAllBytes(path, fileContract.Bytes);
                                    }
                                }
                            }
                        }
                        //DisplayFilesList();
                    }
                    watcher.EnableRaisingEvents = true;
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.F12);
        }

        private IServer ConnectToService()
        {
            //string adres;
            
            do
            {
                //Console.WriteLine("Podaj adres serwera \n");
                //adres = Console.ReadLine();
                try
                {
                    endPoint = new EndpointAddress("http://192.168.0.13/service");
                    //endPoint = new EndpointAddress(adres);
                    myChannelFactory = new ChannelFactory<IServer>(endPoint.ToString());
                    wcfClient = new ServerClient();
                    Console.WriteLine(wcfClient.SendMessage("login", this.getIP()));
                    invalid = false;
                }
                catch (UriFormatException ce)
                {
                    Console.WriteLine(ce.Message + "\n");
                    invalid = true;
                }
                catch (EndpointNotFoundException)
                {
                    Console.WriteLine("Serwer jest wyłączony. Sprawdź połączenie serwera!");
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
                listFileContract = wcfClient.GetFiles(new DateTime(2008, 3, 1, 7, 0, 0));
            }
            catch (EndpointNotFoundException ce)
            {
                Console.WriteLine(ce.Message + "\n");
            }
            return listFileContract;
        }

        private DateTime GetModificationDate(List<string> files)
        {
            List<DateTime> dates = new List<DateTime>();
            if (files.Count() != 0)
            {
                foreach (var file in files)
                {
                    dates.Add(File.GetLastWriteTime(file));
                }
                lastModificationDate = dates.Max();

                return lastModificationDate;
            } else
            {
                return new DateTime(2008, 3, 1, 7, 0, 0);
            }
        }

        private List<string> GetAllFiles(string sourceDir)
        {
            var localFiles = new List<string>();
            try
            {
                foreach (string file in Directory.GetFiles(sourceDir))
                {
                    localFiles.Add(file);
                }
                foreach (string fl in Directory.GetDirectories(sourceDir))
                {
                    localFiles.AddRange(GetAllFiles(fl));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return localFiles;
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

        private string GetSizeString(long length)
        {
            long B = 0, KB = 1024, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            double size = length;
            string suffix = nameof(B);

            if (length >= TB)
            {
                size = Math.Round((double)length / TB, 2);
                suffix = nameof(TB);
            }
            else if (length >= GB)
            {
                size = Math.Round((double)length / GB, 2);
                suffix = nameof(GB);
            }
            else if (length >= MB)
            {
                size = Math.Round((double)length / MB, 2);
                suffix = nameof(MB);
            }
            else if (length >= KB)
            {
                size = Math.Round((double)length / KB, 2);
                suffix = nameof(KB);
            }

            return $"{size} {suffix}";
        }

        private void SendFile(string file)
        {
            files.Add(file);

            byte[] bytes = null;

            i++;
            Console.WriteLine("Licznik i " + i);
            Console.WriteLine("e.FullPath " + file);

            do
            {
                try
                {
                    bytes = File.ReadAllBytes(file);
                    fileIsLocked = false;
                    Console.WriteLine("File is not locked " + i);
                }
                catch (IOException)
                {
                    Console.WriteLine("Plik jest używany przez inny proces " + i);
                    fileIsLocked = true;
                }
            } while (fileIsLocked);

            Console.WriteLine(bytes.Length);
            Console.WriteLine(fileIsLocked);

            var watch = Stopwatch.StartNew();

            wcfClient.SetFile(new FileContract
            {
                FileStatus = Status.New,
                Bytes = bytes,
                FilePath = file.Substring(dir.Length)
            });

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            TimeSpan t = TimeSpan.FromMilliseconds(elapsedMs);
            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
            if(!watch.IsRunning)
                Console.WriteLine(answer);
            //watch.Reset();
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
                //this.SendEditFile(file);
            }
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

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created) {
                modificationDate = DateTime.Now;
                if (!isItDirectory(e.FullPath))
                    sendFiles.Add(e.FullPath);
            }
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                lastModificationDate = DateTime.Now;
                this.DeleteFiles(e.FullPath, true);
                this.DisplayFilesList();
            }
            if (e.ChangeType == WatcherChangeTypes.Changed) {

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
