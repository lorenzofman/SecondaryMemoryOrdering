using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalMergeSort
{
	class ExternalSort
	{
		protected const int ramSize = 3;

		protected const int auxFilesNumber = 2;

		protected const string workingDir = @"C:\Users\Lorenzofman\Documents\ExternalSortingWorkingDir\";

		private static long fileSize = 0;
		static void Main()
		{
			string mainFilePath = workingDir + "BigFile.txt";
			StreamReader mainReader = new StreamReader(mainFilePath);
			fileSize = mainReader.BaseStream.Length;
			StreamWriter[] writers = CreateAuxFiles(workingDir, out string[] paths);
			PopulateAuxFiles(writers,mainReader);
			StreamWriter mainWriter = SwitchStream(mainReader);
			StreamReader[] streamReaders = SwitchStreams(writers);
			MergeAuxFiles(mainWriter, streamReaders);
			CloseStreamWriters(writers);
			mainWriter.Close();	

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
			reader.Close();
			StreamWriter writer = new StreamWriter(path,false);
			return writer;
		}

		private static void MergeAuxFiles(StreamWriter output, StreamReader[] readers)
		{
			for (int max = ramSize; max < fileSize; max += ramSize)
			{
				while (SmallestChar(readers, max, out char smallest))
				{
					output.Write(smallest);
				}
			}
		}

		private static bool SmallestChar(StreamReader[] readers,int max, out char smallest)
		{
			smallest = char.MaxValue;
			int streamIdx = -1;
			for(int i = 0; i < readers.Length; i++)
			{
				if(readers[i].BaseStream.Position >= max)
				{
					continue;
				}
				char ch = (char)(readers[i].BaseStream).ReadByte();
				if(ch == char.MaxValue)
				{
					continue;
				}
				if (ch < smallest)
				{
					smallest = ch;
					streamIdx = i;
				}
				readers[i].BaseStream.Seek(-1, SeekOrigin.Current);
			}
			if(streamIdx != -1)
				readers[streamIdx].BaseStream.Seek(1, SeekOrigin.Current);
			if (smallest == char.MaxValue)
				return false;
			return true;
		}

		
		private static void CloseStreamWriters(StreamWriter[] writers)
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

	}
}
