using client.ServiceReference1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Linq;
using System.Diagnostics;
using System.Configuration;

namespace client
{
    public class Client
    {
        const string dir = @"C:\From\";
        private ServerClient wcfClient;
        private EndpointAddress endPoint;
        private List<string> listMainFiles = new List<string>();
        private List<string> listEditedFiles = new List<string>();
        private List<string> listSendedFiles = new List<string>(); 
        private IList<FileContract> listFileContract;
        private DateTime lastModificationDate, modificationDate;
        bool isItConnected = false;
        bool invalid = true;
        FileSystemWatcher watcher = new FileSystemWatcher();
        bool let = false;

        public Client()
        {

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            ConnectToService();
            SetFileWatcher();

            var allFiles = GetAllFiles(dir);

            if (isItConnected)
            {
                var watch = Stopwatch.StartNew();

                SendFiles(allFiles);

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                TimeSpan t = TimeSpan.FromMilliseconds(elapsedMs);
                string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds,
                                        t.Milliseconds);


                listMainFiles.AddRange(allFiles);
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
                
                if (Console.ReadKey(true).Key == ConsoleKey.F4)
                {
                    FileStream fs = new FileStream(@"C:\From\file", FileMode.CreateNew);
                    fs.Seek(2048L * 1024 * 512, SeekOrigin.Begin);
                    fs.WriteByte(0);
                    fs.Close();
                }

                if (Console.ReadKey(true).Key == ConsoleKey.F2)
                {
                    if(modificationDate > lastModificationDate)
                    {
                        SendFiles(listSendedFiles);
                        
                        if(listEditedFiles.Count() != 0)
                        {
                            SendEditFile(listEditedFiles);
                            listEditedFiles.Clear();
                        }

                        lastModificationDate = modificationDate;
                        listMainFiles.AddRange(listSendedFiles);
                        listSendedFiles.Clear();
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
                                listMainFiles.Add(path);
                                Console.WriteLine(path);
                            }
                            else if (fileContract.FileStatus == Status.Modified)
                            {
                                if (!Directory.Exists(dirname))
                                {
                                    Directory.CreateDirectory(dirname);
                                }
                                if (!listMainFiles.Any(file => file == path))
                                {
                                    listMainFiles.Add(path);
                                }
                                File.WriteAllBytes(path, fileContract.Bytes);
                            }
                            else if (fileContract.FileStatus == Status.Deleted)
                            {
                                DeleteFiles(path, false);
                                DeleteDirectory(dir);
                            }
                            else if (fileContract.FileStatus == Status.Renamed)
                            {
                                var oldPath = Path.Combine(dir, fileContract.OldFilePath);
                                var oldDirName = Path.GetDirectoryName(oldPath);

                                var oldFileName = Path.GetFileName(oldPath);
                                var fileName = Path.GetFileName(path);

                                if (listMainFiles.Any(file => file == oldPath) && oldFileName != fileName)
                                {
                                    var newPath = Path.Combine(dir, fileContract.FilePath);

                                    RenameFiles(oldPath, newPath, false);
                                }
                                else
                                {
                                    if (Directory.Exists(oldDirName))
                                    {
                                        RenameFiles(oldDirName, dirname, false);
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
                    lastModificationDate = DateTime.Now;
                    watcher.EnableRaisingEvents = true;
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.F12);
        }

        private void ConnectToService()
        {
            //string adres;

            do
            {
                //Console.WriteLine("Podaj adres serwera");
                //adres = Console.ReadLine();

                try
                {
                    endPoint = new EndpointAddress("http://192.168.1.6");
                    wcfClient = new ServerClient("basicEndpoint", endPoint);
                    Console.WriteLine(wcfClient.SendMessage("login", getIP()));
                    invalid = false;
                }
                catch (UriFormatException)
                {
                    Console.WriteLine("Błędny format adresu \n");
                    invalid = true;
                }
                catch (EndpointNotFoundException)
                {
                    Console.WriteLine("Nieprawidłowy adres \n");
                    invalid = true;
                }
                catch (ProtocolException)
                {
                    Console.WriteLine("Brak utworzonej usługi pod podanym adresem \n");
                    invalid = true;
                }

            } while (invalid);

            isItConnected = true;
        }
   
        private IList<FileContract> GetListFileContract()
        {
            try
            {
                listFileContract = new List<FileContract>();
                listFileContract = wcfClient.GetFiles(lastModificationDate);
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

        private void SendFile(string file)
        {
            var bytes = File.ReadAllBytes(file);
            wcfClient.SetFile(new FileContract
            {
                FileStatus = Status.New,
                Bytes = bytes,
                FilePath = file.Substring(dir.Length)
            });


            var watch = Stopwatch.StartNew();


            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            TimeSpan t = TimeSpan.FromMilliseconds(elapsedMs);
            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
        }

        private void SendEditFile(List<string> files)
        {

            files.ForEach(file =>
            {
                var bytes = File.ReadAllBytes(file);

                wcfClient.SetFile(new FileContract
                {
                    FileStatus = Status.Modified,
                    Bytes = bytes,
                    FilePath = file.Substring(dir.Length)
                });

                File.SetLastWriteTime(file, DateTime.Now);

            });
            
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

            foreach (string file in listMainFiles)
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
                    listMainFiles.Remove(file);
                    break;
                } else
                {
                    itIsDirectory = true;
                }
            }
            if(itIsDirectory)
            {
                foreach (string file in listMainFiles)
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
                listMainFiles.RemoveAll(file => file.Contains(directory));
            }
        }

        private void RenameFiles(string oldPath, string newPath, bool toSend)
        {

            bool itIsDirectory = false;

            foreach (string file in listMainFiles)
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

                    int index = listMainFiles.FindIndex(f => f == oldPath);
                    listMainFiles[index] = newPath;
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
                foreach (string file in listMainFiles)
                {
                    if (file.Contains(oldPath))
                    {
                        indexes.Add(listMainFiles.IndexOf(file));
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
                    var cutDir = listMainFiles[i].Substring(oldPath.Length);
                    listMainFiles[i] = newPath + cutDir;
                    File.SetLastWriteTime(listMainFiles[i], DateTime.Now);
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
            Console.WriteLine("Lista plików lokalnie: " + listMainFiles.Count());
            listMainFiles.ForEach(x =>
            {
                Console.WriteLine(x + " " + File.GetLastWriteTime(x) + ":" + File.GetLastWriteTime(x).Millisecond);
            });
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created) {
                modificationDate = DateTime.Now;
                if (!isItDirectory(e.FullPath))
                {
                    let = true;
                    listSendedFiles.Add(e.FullPath);
                    Console.WriteLine("Wysyłam");
                    SendFile(e.FullPath);
                    Console.WriteLine("Koniec wysyłania");
                    Console.ReadLine();
                }
            }
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                lastModificationDate = DateTime.Now;
                DeleteFiles(e.FullPath, true);
            }
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                if(let)
                {
                    let = false;
                } else
                {
                    let = true;
                    if(File.Exists(e.FullPath))
                    {
                        if (listMainFiles.Any(file => file == e.FullPath))
                        {
                            var file = listMainFiles.Single(f => f == e.FullPath);
                            modificationDate = DateTime.Now;
                            if (!listEditedFiles.Any(f => f == e.FullPath))
                                listEditedFiles.Add(e.FullPath);
                        }
                    }
                }

            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            RenameFiles(e.OldFullPath, e.FullPath, true);
            DisplayFilesList();
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
