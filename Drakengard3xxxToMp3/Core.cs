using System;
using System.IO;

namespace Drakengard3xxxToMp3
{
    internal class Core
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                ErrorExit("InFile is not specified in the argument!");
            }

            try
            {
                var inFile = args[0];

                if (!File.Exists(inFile))
                {
                    ErrorExit("Specified InFile in the argument is missing!");
                }

                using (var xxxStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
                {
                    using (var xxxReader = new BinaryReader(xxxStream))
                    {
                        xxxReader.BaseStream.Position = 0;
                        var getHeaderChars = xxxReader.ReadChars(8);
                        var foundHeader = string.Join("", getHeaderChars).Replace("\0", "");

                        if (!foundHeader.Contains("SEDBSSCF"))
                        {
                            ErrorExit("This is not a valid Drakengard 3 audio file!");
                        }

                        xxxReader.BaseStream.Position = 112;
                        var audioInfoPos = xxxReader.ReadBytesUInt32(true);

                        xxxReader.BaseStream.Position = audioInfoPos;
                        var audioSize = xxxReader.ReadBytesUInt32(true);

                        xxxReader.BaseStream.Position = audioInfoPos + 24;
                        var audioStart = xxxReader.ReadBytesUInt32(true);

                        var outMp3File = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}.mp3");

                        if (File.Exists(outMp3File))
                        {
                            File.Delete(outMp3File);
                        }

                        using (var outMp3Stream = new FileStream(outMp3File, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            xxxStream.Seek(audioInfoPos + 32 + audioStart, SeekOrigin.Begin);
                            xxxStream.CopyStreamTo(outMp3Stream, audioSize, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorExit("" + ex);
            }
        }

        private static void ErrorExit(string errorMsg)
        {
            Console.WriteLine("Error: " + errorMsg);
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}