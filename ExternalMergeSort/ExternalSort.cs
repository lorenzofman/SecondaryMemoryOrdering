using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExternalMergeSort
{
	class ExternalSort
	{
		protected const int ramSize = 3;

		protected const int auxFilesNumber = 2;
		protected const string workingDir = @"C:\Users\Lorenzofman\Documents\ExternalSortingWorkingDir\";
		static void Main()
		{
			string mainFilePath = workingDir + "BigFile.txt";
			StreamReader mainReader = new StreamReader(mainFilePath);
			StreamWriter[] writers = CreateAuxFiles(workingDir, out string[] paths);
			PopulateAuxFiles(writers,mainReader);
			StreamWriter mainWriter = SwitchStream(mainReader);
			MergeAuxFiles(mainWriter, paths);
			CloseAuxFiles(writers);

		}

		private static StreamReader[] SwitchStreams(StreamWriter[] writers)
		{
			StreamReader[] streamReaders = new StreamReader[writers.Length];
			for(int i = 0; i < writers.Length; i++)
			{
				streamReaders[i] = SwitchStream(writers[i]);
			}
			return streamReaders;
		}

		private static StreamWriter[] SwitchStreams(StreamReader[] readers)
		{
			StreamWriter[] streamWriters = new StreamWriter[readers.Length];
			for (int i = 0; i < readers.Length; i++)
			{
				streamWriters[i] = SwitchStream(readers[i]);
			}
			return streamWriters;
		}

		private static StreamReader SwitchStream(StreamWriter writer)
		{
			string path = (writer.BaseStream as FileStream).Name;
			writer.Close();
			return new StreamReader(path);
		}

		private static StreamWriter SwitchStream(StreamReader reader)
		{
			string path = (reader.BaseStream as FileStream).Name;
			reader.BaseStream.SetLength(0);
			reader.Close();
			return new StreamWriter(path);
		}

		private static void MergeAuxFiles(StreamWriter output, string[] paths)
		{
			FileStream[] streams = new FileStream[paths.Length];
			for(int i = 0; i < paths.Length; i++)
			{
				streams[i] = new FileStream(paths[i], FileMode.Open);
			}
			while (SmallestChar(streams, out char smallest))
			{
				output.Write(smallest);
			}
		}


		private static bool SmallestChar(FileStream[] streams, out char smallest)
		{
			smallest = char.MaxValue;
			int streamIdx = 0;
			for(int i = 0; i < streams.Length; i++)
			{
				char ch = (char)streams[i].ReadByte();
				if(ch == -1)
				{
					continue;
				}
				if (ch < smallest)
				{
					smallest = ch;
					streamIdx = i;
				}
				streams[i].Seek(-1, SeekOrigin.Current);
			}
			streams[streamIdx].Seek(1, SeekOrigin.Current);
			if (smallest == char.MaxValue)
				return false;
			return true;
		}

		private static void CloseAuxFiles(StreamWriter[] writers)
		{
			foreach(StreamWriter writer in writers)
			{
				writer.Close();
			}
		}

		private static void PopulateAuxFiles(StreamWriter[] writers, StreamReader reader)
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

		private static StreamWriter[] CreateAuxFiles(string workingDir, out string[] paths)
		{
			paths = new string[auxFilesNumber];
			StreamWriter[] writers = new StreamWriter[auxFilesNumber];
			for(int i = 0; i < auxFilesNumber; i++)
			{
				paths[i] = workingDir + "Aux" + i + ".txt";
				writers[i] = new StreamWriter(paths[i]);
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
