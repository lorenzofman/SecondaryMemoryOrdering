﻿using System;
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

		protected const int auxFilesNumber = 3;

		protected const string workingDir = @"C:\Users\Lorenzofman\Documents\ExternalSortingWorkingDir\";

		private static long fileSize = 0;

		static void Main()
		{
			string mainFilePath = workingDir + "BigFile.txt";
			Intercalate(mainFilePath);
		}

		private static void Intercalate(string mainFilePath)
		{
			StreamReader mainReader = new StreamReader(mainFilePath);
			fileSize = mainReader.BaseStream.Length;
			StreamWriter[] writers = CreateAuxFiles(workingDir, 0,auxFilesNumber);
			PopulateAuxFiles(writers, ramSize, mainReader);
			int intercalationsRequired = (int)Math.Ceiling(Math.Log((float)fileSize/ramSize,auxFilesNumber));
			for(int i = 0; i < intercalationsRequired; i++)
			{
				StreamReader[] readers = SwitchStreams(writers);
				writers = CreateAuxFiles(workingDir,(i+1)*auxFilesNumber, (i+2)*auxFilesNumber);
				IntercalateAuxFiles(writers, readers,(i+1)*ramSize);
			}
			CloseStreamWriters(writers);
		}

		private static void IntercalateAuxFiles(StreamWriter[] writers, StreamReader[] readers, int blockSize)
		{
			int iterations = (int)Math.Ceiling((double) fileSize / blockSize);
			for (int i = 0; i < iterations; i++)
			{
				MergeIteration(writers[i % auxFilesNumber], readers,blockSize);
			}
		}

		private static void MergeIteration(StreamWriter output, StreamReader[] readers, int blockSize)
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

		private static void PopulateAuxFiles(StreamWriter[] writers,int readSize, StreamReader reader)
		{
			int currentWriter = 0;
			while (!reader.EndOfStream)
			{
				int size = Min(readSize, ramSize);
				char[] block = new char[size];
				for (int i = 0; i < readSize; i += size)
				{
					int validCharCount = reader.ReadBlock(block, 0, size);

					block = SortArray(block, validCharCount);
					writers[currentWriter % auxFilesNumber].Write(block);
				}
				currentWriter++;
			}
		}

		private static char[] SortArray(char[] array, int validCharCount)
		{
			List<char> list = new List<char>();
			for(int i = 0; i< validCharCount; i++)
			{
				list.Add(array[i]);
			}
			list.Sort();
			return list.ToArray();
		}

		private static int Min (int a, int b)
		{
			return (a > b) ? b : a;
		}

		private static char[] ReadFromStream(StreamReader reader, int readSize)
		{
			int size = Min(readSize, ramSize);
			char[] block = new char[size];
			for (int i = 0; i < readSize; i += size)
			{
				reader.ReadBlock(block, 0, size);
			}
			return block;
		}

		private static StreamWriter[] CreateAuxFiles(string workingDir,int startIdx, int endIdx)
		{
			StreamWriter[] writers = new StreamWriter[auxFilesNumber];
			for(int i = startIdx; i < endIdx; i++)
			{
				writers[i - startIdx] = new StreamWriter(workingDir + "Aux" + i + ".txt");
			}
			return writers;
		}

		private static StreamReader[] SwitchStreams(StreamWriter[] writers)
		{
			StreamReader[] readers = new StreamReader[writers.Length];
			for (int i = 0; i < writers.Length; i++)
			{
				string filename = (writers[i].BaseStream as FileStream).Name;
				writers[i].Close();
				readers[i] = new StreamReader(filename);
			}
			return readers;
		}

	}
}
