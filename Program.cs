using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExiledInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "";
            if (args.Length != 1)
            {
                Console.WriteLine("Please input your SCP:SL Directory (managed folder):");
                path = Console.ReadLine();
            }
            else
                path = args[0].Replace("\"", "");

            // Get the latest version
            new WebClient().DownloadFile("https://github.com/galaxy119/EXILED/releases/download/EXILED.tar.gz", "EXILED.tar.gz");


            ExtractTarGz("EXILED.tar.gz", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            if (!Directory.Exists(path))
                throw new ArgumentException("The provided Managed folder does not exist.");

            File.Move(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Assembly-CSharp.dll"), path);

            Console.WriteLine("Complete!");

        }

        // stole this hahahahahhahahaha

        private static void ExtractTarGz(string filename, string outputDir)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                ExtractTarGz(stream, outputDir);
            }
        }

        private static void ExtractTarGz(Stream stream, string outputDir)
        {
            using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
            {
                const int chunk = 4096;
                using (MemoryStream memStr = new MemoryStream())
                {
                    int read;
                    byte[] buffer = new byte[chunk];
                    do
                    {
                        read = gzip.Read(buffer, 0, chunk);
                        memStr.Write(buffer, 0, read);
                    } while (read == chunk);

                    memStr.Seek(0, SeekOrigin.Begin);
                    ExtractTar(memStr, outputDir);
                }
            }
        }

        private static void ExtractTar(Stream stream, string outputDir)
        {
            byte[] buffer = new byte[100];
            while (true)
            {
                try
                {
                    stream.Read(buffer, 0, 100);
                    string name = Encoding.ASCII.GetString(buffer).Trim('\0');
                    if (string.IsNullOrWhiteSpace(name))
                        break;
                    stream.Seek(24, SeekOrigin.Current);
                    stream.Read(buffer, 0, 12);
                    long size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), 8);

                    stream.Seek(376L, SeekOrigin.Current);

                    string output = Path.Combine(outputDir, name);
                    if (!Directory.Exists(Path.GetDirectoryName(output)))
                        Directory.CreateDirectory(Path.GetDirectoryName(output));
                    if (!name.Equals("./", StringComparison.InvariantCulture))
                    {
                        using (FileStream str = File.Open(output, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            byte[] buf = new byte[size];
                            stream.Read(buf, 0, buf.Length);
                            str.Write(buf, 0, buf.Length);
                        }
                    }

                    long pos = stream.Position;

                    long offset = 512 - (pos % 512);
                    if (offset == 512)
                        offset = 0;

                    stream.Seek(offset, SeekOrigin.Current);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
