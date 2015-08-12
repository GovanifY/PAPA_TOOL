using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GovanifY.Utility;
using System.IO;
using ns;

namespace XBB_TOOL
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("PAPA_Tool\nProgrammed by GovanifY for ChrisX930\n\n1) Extract 2) Create\n");
            string choice = Console.ReadLine();
            if (choice == "1")
            {
                Console.WriteLine("\n\nPlease enter the name of the file to extract: ");
                string arg = Console.ReadLine();
                if (File.Exists(arg))
                {

                    BinaryStream input = new BinaryStream(File.Open(arg, FileMode.Open));
                    UInt32 magic = input.ReadUInt32();
                    if (magic != 0x41504150) { Console.WriteLine("INCORRECT MAGIC!\nExiting..."); return; }
                    input.ReadUInt32();//Padding
                    UInt32 Headeroffset = input.ReadUInt32();
                    UInt32 Headersize = input.ReadUInt32();
                    UInt32 count = input.ReadUInt32();
                    count -= 1;
                    string dirname = "@" + arg + "/";
                    #region Dir creation
                    try
                    {
                        Directory.CreateDirectory(dirname);
                    }
                    catch (IOException e)
                    {
                        Console.Write("Failed creating directory: {0}", e.Message);
                    }
                    #endregion
                    for (int i = 0; i < count; i++)
                    {
                        UInt32 FileOffset = input.ReadUInt32();
                        long tmp = input.Tell();
                        UInt32 NextFileOffset = input.ReadUInt32();
                        UInt32 FileSize = NextFileOffset;
                        FileSize -= FileOffset + 5*4;
                        
                        
                        input.Seek(FileOffset, SeekOrigin.Begin);
                        Console.WriteLine("Extracting...: {0}", "@" + arg + "/" + i + ".bin");
                        UInt32 completesize = input.ReadUInt32();
                        input.ReadUInt32();
                        input.ReadUInt32();
                        input.ReadUInt32();//Constants: 3 then 14 then 18
                        UInt32 secondsize = input.ReadUInt32();
                        byte[] PAPAtmp = input.ReadBytes((int)FileSize);
                        var PAPAfs = new FileStream("@" + arg + "/" + i + ".bin", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                        PAPAfs.Write(PAPAtmp, 0, PAPAtmp.Length);
                        input.Seek(tmp, SeekOrigin.Begin);


                    }
                }
                else
                {
                    Console.WriteLine("Cannot open file!");
                }
            }
            else
            {
                if (choice == "2")
                {
                    long disposer;
                    Console.WriteLine("\n\nPlease enter the name of the file to create: ");
                    string arg = Console.ReadLine();
                    BinaryWriter output = new BinaryWriter(File.Open(Path.GetFileNameWithoutExtension(arg) + "Modded" + Path.GetExtension(arg), FileMode.Create));
                    string dirname = "@" + arg + "/";
                    output.Write((UInt32)0x41504150);//Magic!
                    output.Write((UInt32)0);//Padding

                    output.Write((UInt32)0xC);
                    output.Write((UInt32)0xADC);//Header offset and size, but always constants!

                    string[] files = Directory.GetFiles(dirname);//The files are sorted numerically by default, all hail to .NET o/(actually not)
                    NumericComparer ns = new NumericComparer();
                    Array.Sort(files, ns);
                    output.Write((UInt32)files.Length + 1);//Number of files
                    for (int i = 0; i < files.Length; i++ )
                    {
                        output.Write((UInt32)0);//FileOffset
                        //Garbages since we're going to mod this later
                    }
                    disposer = output.BaseStream.Position;
                    int y = 0;
                    foreach (string name in files)
                    {
                        byte[] file = File.ReadAllBytes(name);
                        output.Write((UInt32)file.Length + 5 * 4);//Complete Size

                        output.Write((UInt32)0x3);//Constants
                        output.Write((UInt32)0x14);//Constants
                        output.Write((UInt32)0x18);//Constants

                        output.Write((UInt32)file.Length + 4);//Second Size

                        output.Write(file);
                        //Then write file datas here
                        long tmp = output.BaseStream.Position;
                        
                        output.Seek(20 + y * 4, SeekOrigin.Begin);
                        output.Write((UInt32)disposer);

                        output.Seek((int)tmp, SeekOrigin.Begin);
                        disposer = output.BaseStream.Position;
                        y++;
                    }






                }
                else
                {
                    Console.WriteLine("Please enter a correct option!");
                }
            }

        }
    }
}
