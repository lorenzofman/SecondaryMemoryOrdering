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
		protected const string workingDir = @"C:\Users\Lorenzofman\Documents\ExternalSortingWorkingDir\";
		static void Main()
		{
			StreamReader reader = new StreamReader(workingDir + "BigFile.txt");
			StreamWriter[] writers = CreateAuxFiles(workingDir);
			PopulateFiles(writers,reader);
			CloseWriters(writers);
		}
		private static void CloseWriters(StreamWriter[] writers)
		{
			foreach(StreamWriter writer in writers)
			{
				writer.Close();
			}
		}
		private static void PopulateFiles(StreamWriter[] writers, StreamReader reader)
		{
			int currentWriter = 0;
			while (!reader.EndOfStream)
			{
				List<char> blockList = ReadRamSize(reader);
				blockList.Sort();
				foreach(char ch in blockList)
					writers[currentWriter % auxFilesNumber].Write(ch);
				currentWriter++;
			}
		}
		private static List<char> ReadRamSize(StreamReader reader)
		{
			List<char> blockList = new List<char>();
			char[] block = new char[ramSize];
			int readCount = reader.ReadBlock(block, 0, ramSize);
			for (int i = 0; i < readCount; i++)
			{
				blockList.Add(block[i]);
			}
			return blockList;
		}
		private static StreamWriter[] CreateAuxFiles(string workingDir)
		{
			StreamWriter[] writers = new StreamWriter[auxFilesNumber];
			for(int i = 0; i < auxFilesNumber; i++)
			{
				writers[i] = new StreamWriter(workingDir + "Aux" + i + ".txt");
			}
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
