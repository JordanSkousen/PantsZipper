using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;

namespace PantsZipper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();//new[] {"", @"D:\Downloads\jdk-21_linux-x64_bin.tar.gz" };//
            if (args.Length > 1)
            {
                var file = args[1];
                labelProgress.Content = $"Expanding \"{Path.GetFileName(file)}\"...";
                var worker = new AsyncWorker(Dispatcher.CurrentDispatcher, (o, asyncWorker) =>
                    {
                        try
                        {
                            var folderName = $"{Path.GetDirectoryName(file)}\\{Path.GetFileNameWithoutExtension(file)}";
                            var index = 2;
                            while (Directory.Exists(folderName))
                            {
                                folderName = $"{Path.GetDirectoryName(file)}\\{Path.GetFileNameWithoutExtension(file)} ({index++})";
                            }

                            var fileSize = new FileInfo(file).Length;
                            long copied = 0;
                            switch (Path.GetExtension(file))
                            {
                                case ".zip":
                                    Directory.CreateDirectory(folderName);
                                    using (var stream = File.Open(file, FileMode.Open))
                                    using (var archive = new ZipArchive(stream))
                                    {
                                        foreach (var entry in archive.Entries)
                                        {
                                            var subDirs = entry.FullName.Split('/').ToList().Map(path => path.RemoveIllegalPathChars().TrimEnd('.').Trim());
                                            var fullName = string.Join("/", subDirs);
                                            var name = entry.Name.RemoveIllegalPathChars();
                                            for (var i = 0; i < subDirs.Count - 1; i++)
                                            {
                                                Directory.CreateDirectory(Path.Combine(folderName,
                                                    string.Join("\\", subDirs.Take(i + 1))));
                                            }

                                            if (string.IsNullOrEmpty(name))
                                                continue;

                                            try
                                            {
                                                entry.ExtractToFile(Path.Combine(folderName, fullName));
                                                copied += entry.CompressedLength;
                                                asyncWorker.ReportProgress(copied / (float)fileSize * 100);
                                            }
                                            catch
                                            {
                                                //ignore
                                            }
                                        }
                                    }
                                    break;
                                case ".bz2":
                                    if (file.ToLower().EndsWith(".tar.bz2"))
                                    {
                                        folderName = $"{Path.GetDirectoryName(file)}\\{Path.GetFileName(file).TrimEnd(".tar.bz2".ToCharArray())}";
                                        index = 2;
                                        while (Directory.Exists(folderName))
                                        {
                                            folderName = $"{Path.GetDirectoryName(file)}\\{Path.GetFileName(file).TrimEnd(".tar.bz2".ToCharArray())} ({index++})";
                                        }
                                        var tempFile =
                                            Path.Combine(
                                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                Path.GetFileNameWithoutExtension(file));
                                        ExtractBZ2(file, tempFile);
                                        asyncWorker.ReportProgress(50.0);
                                        ExtractTar(tempFile, folderName);
                                        asyncWorker.ReportProgress(100.0);
                                        File.Delete(tempFile);
                                    }
                                    else
                                        ExtractBZ2(file, folderName);
                                    break;
                                case ".gz":
                                    if (file.ToLower().EndsWith(".tar.gz"))
                                    {
                                        folderName = $"{Path.GetDirectoryName(file)}\\{Path.GetFileName(file).TrimEnd(".tar.gz".ToCharArray())}";
                                        index = 2;
                                        while (Directory.Exists(folderName))
                                        {
                                            folderName = $"{Path.GetDirectoryName(file)}\\{Path.GetFileName(file).TrimEnd(".tar.gz".ToCharArray())} ({index++})";
                                        }
                                        var tempFile =
                                            Path.Combine(
                                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                Path.GetFileNameWithoutExtension(file));
                                        ExtractGZip(file, tempFile);
                                        asyncWorker.ReportProgress(50.0);
                                        ExtractTar(tempFile, folderName);
                                        asyncWorker.ReportProgress(100.0);
                                        File.Delete(tempFile);
                                    }
                                    else
                                        ExtractGZip(file, folderName);
                                    break;
                                case ".tar":
                                    ExtractTar(file, folderName);
                                    break;
                                default:
                                    asyncWorker.StopWithError($"Unknown file type \"*{Path.GetExtension(file)}\".");
                                    break;
                            }
                            asyncWorker.ReportProgress((object)folderName);
                        }
                        catch (Exception ex)
                        {
                            asyncWorker.StopWithError(ex.Message);
                        }
                    },
                    (progress) =>
                    {
                        progressBar.Value = progress.ProgressPercentage;
                        labelProgress.Content = $"Expanding \"{Path.GetFileName(file)}\" ({(int)progress.ProgressPercentage}%)...";
                    },
                    (progress) =>
                    {
                        if (progress.Type == AsyncWorker.ProgressType.FinishedWithError)
                        {
                            MessageBox.Show("Failed to expand file: " + progress.ProgressText);
                        }

                        System.Diagnostics.Process.Start(progress.Argument.ToString());
                        Close();
                    });
                worker.Run();
            }
            else
            {
                MessageBox.Show("To use PantsZipper, right click a compressed file (*.zip, *.tar) and click Open With > PantsZipper.");
                Close();
            }
        }

        void ExtractBZ2(string file, string outputFile)
        {
            using (var inStream = File.Open(file, FileMode.Open))
            using (var outStream = File.Open(outputFile, FileMode.Create))
            {
                BZip2.Decompress(inStream, outStream, false);
            }
        }

        void ExtractTar(string file, string outputDir)
        {
            using (var inStream = File.Open(file, FileMode.Open))
            using (var tarStream = ICSharpCode.SharpZipLib.Tar.TarArchive.CreateInputTarArchive(inStream, Encoding.UTF8))
            {
                tarStream.ExtractContents(outputDir, false);
            }
        }
        void ExtractGZip(string file, string outputDir)
        {
            using (var inStream = File.Open(file, FileMode.Open))
            using (var outStream = File.Open(outputDir, FileMode.Create))
            {
                GZip.Decompress(inStream, outStream, false);
            }
        }
    }

    public static class XTK
    {
        public static string RemoveIllegalPathChars(this string s)
        {
            Regex r = new Regex(
                $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))}]");
            return r.Replace(s, "");
        }

        public static List<T> Map<T>(this List<T> list, Func<T, T> mapFunction)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = mapFunction(list[i]);
            }

            return list;
        }
    }
}
