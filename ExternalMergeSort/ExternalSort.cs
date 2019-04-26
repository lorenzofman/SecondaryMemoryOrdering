using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalMergeSort
{
	class ExternalSort
	{
		protected const int ramSize = 3;
		/// <summary>
		/// Min value is 2
		/// </summary>
		protected const int auxFilesNumber = 2;
		static void Main()
		{
			StreamWriter[] writers = CreateAuxFiles("BigFile.txt", out StreamReader reader);
			PopulateFiles(writers,reader);
		}
		private static void PopulateFiles(StreamWriter[] writers, StreamReader reader)
		{
			while (!reader.EndOfStream)
			{
				char[] block = ReadRamSize(reader);
			}
		}
		private static char[] ReadRamSize(StreamReader reader)
		{
			char[] block = new char[ramSize];
			reader.ReadBlock(block, 0, ramSize);
			return block;
		}
		private static StreamWriter[] CreateAuxFiles(string path, out StreamReader reader)
		{
			reader = new StreamReader(path);
			StreamWriter[] writers = new StreamWriter[auxFilesNumber + 1];
			for(int i = 0; i < auxFilesNumber; i++)
			{
				writers[i] = new StreamWriter("Aux" + i);
			}
			writers[auxFilesNumber] = new StreamWriter(path);
			return writers;
		}

		private static string ConvertListOfCharToString(List<char> list)
		{
			string str = "";
			foreach (char c in list)
				str += c;
			return str;
		}

		private static List<char> CreateRandomText()
		{
			return new List<char>("LorenzoSchwertnerKaufmann");
		}

	}
}
