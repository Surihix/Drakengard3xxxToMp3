using System;
using System.Buffers.Binary;
using System.IO;

namespace Drakengard3xxxToMp3
{
    internal class Program
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

                using (FileStream xxxFile = new FileStream(inFile, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader xxxReader = new BinaryReader(xxxFile))
                    {
                        xxxReader.BaseStream.Position = 0;
                        var getHeaderChars = xxxReader.ReadChars(8);
                        var foundHeader = string.Join("", getHeaderChars).Replace("\0", "");

                        if (!foundHeader.Contains("SEDBSSCF"))
                        {
                            ErrorExit("This is not a valid Drakengard 3 audio file!");
                        }

                        BEReader(xxxReader, 112, out uint audioInfoPos);
                        BEReader(xxxReader, audioInfoPos, out uint audioSize);
                        BEReader(xxxReader, audioInfoPos + 24, out uint audioStart);

                        var xxxFilePath = Path.GetFullPath(inFile);
                        var xxxFileDir = Path.GetDirectoryName(xxxFilePath);
                        var xxxName = Path.GetFileNameWithoutExtension(inFile);

                        if (File.Exists(xxxFileDir + "\\" + xxxName + ".mp3"))
                        {
                            File.Delete(xxxFileDir + "\\" + xxxName + ".mp3");
                        }

                        using (FileStream outAudioFile = new FileStream(xxxFileDir + "\\" + xxxName + ".mp3", FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            xxxFile.Seek(audioInfoPos + 32 + audioStart, SeekOrigin.Begin);

                            byte[] audioBuffer = new byte[audioSize];
                            var audioBytesToRead = xxxFile.Read(audioBuffer, 0, audioBuffer.Length);
                            outAudioFile.Write(audioBuffer, 0, audioBytesToRead);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorExit("" + ex);
            }
        }

        static void ErrorExit(string errorMsg)
        {
            Console.WriteLine("Error: " + errorMsg);
            Console.ReadLine();
            Environment.Exit(0);
        }

        static void BEReader(BinaryReader ReaderName, uint ReaderPos, out uint VarName)
        {
            ReaderName.BaseStream.Position = ReaderPos;
            byte[] GetVarName = ReaderName.ReadBytes((int)ReaderName.BaseStream.Length);
            VarName = BinaryPrimitives.ReadUInt32BigEndian(GetVarName.AsSpan());
        }
    }
}