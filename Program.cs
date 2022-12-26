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
                Console.WriteLine("Error: InFile is not specified in the argument");
                Console.ReadLine();
                return;
            }

            string InFile = args[0];

            if (!File.Exists(InFile))
            {
                Console.WriteLine("Error: Specified InFile in the argument is missing");
                Console.ReadLine();
                return;
            }


            using (FileStream XXXFile = new FileStream(InFile, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader XXXReader = new BinaryReader(XXXFile))
                {
                    XXXReader.BaseStream.Position = 0;
                    var GetHeader = XXXReader.ReadChars(8);
                    string Header = string.Join("", GetHeader).Replace("\0", "");

                    if (Header.Contains("SEDBSSCF"))
                    {
                        BEReader(XXXReader, 544, out byte[] GetAudioSize, out uint AudioSize);
                        BEReader(XXXReader, 568, out byte[] GetAudioStart, out uint AudioStart);

                        XXXFile.Seek(576, SeekOrigin.Begin);
                        XXXFile.Seek(AudioStart, SeekOrigin.Current);

                        string XXXFileDir = Path.GetDirectoryName(InFile);
                        string XXXName = Path.GetFileNameWithoutExtension(InFile);

                        if (File.Exists(XXXFileDir + "\\" + XXXName + ".mp3"))
                        {
                            File.Delete(XXXFileDir + "\\" + XXXName + ".mp3");
                        }

                        using (FileStream Mp3File = new FileStream(XXXFileDir + "\\" + XXXName + ".mp3", FileMode.Append,
                            FileAccess.Write))
                        {
                            XXXFile.CopyTo(Mp3File);

                            long Mp3Size = Mp3File.Length;
                            long AdjustMp3Size = Mp3Size - AudioSize;
                            Mp3File.SetLength(Math.Max(0, Mp3File.Length - AdjustMp3Size));
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: This is not a valid audio file!");
                        Console.ReadLine();
                        return;
                    }
                }
            }
        }



        static void BEReader(BinaryReader ReaderName, uint ReaderPos, out byte[] GetVarName, out uint VarName)
        {
            ReaderName.BaseStream.Position = ReaderPos;
            GetVarName = ReaderName.ReadBytes((int)ReaderName.BaseStream.Length);
            VarName = BinaryPrimitives.ReadUInt32BigEndian(GetVarName.AsSpan());
        }
    }
}