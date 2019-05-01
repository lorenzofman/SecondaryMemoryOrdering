using System;
using System.IO;

namespace ExternalMergeSort
{
	class ExternalSort
	{
		/// <summary>
		/// When on doesn't create aux files that won't be used, the advantage, however, of create the aux files is that you can trace down every iteration 
		/// </summary>
		public static bool createOnlyNecessaryAuxFiles = true;

		protected const int ramSize = 5;

		protected const int maxAuxFilesNumber = 3;

		protected const string workingDir = @"C:\Users\Lorenzofman\Documents\ExternalSortingWorkingDir\";

		private static long fileSize = 0;

		private static int auxNumber;

		private static int blockSize;

		private static int AuxNumber
		{
			get
			{
				return auxNumber++;
			}
		}

		static void Main()
		{
			string mainFilePath = workingDir + "BigFile.txt";
			FMeansInterleaving(mainFilePath);
		}
		private static void PolyphasicInterleaving (string mainFilePath)
		{

		}
		private static void FMeansInterleaving(string mainFilePath)
		{
			StreamReader mainReader = new StreamReader(mainFilePath);
			fileSize = mainReader.BaseStream.Length;
			blockSize = ramSize;
			StreamWriter[] writers = CreateAuxFiles(workingDir, blockSize);
			PopulateAuxFiles(writers, ramSize, mainReader);
			int intercalationsRequired = (int)Math.Ceiling(Math.Log((float)fileSize/ramSize,maxAuxFilesNumber));
			for(int i = 0; i < intercalationsRequired; i++)
			{ 
				StreamReader[] readers = SwitchStreams(writers);
				//int blockSize = (int)Math.Pow(ramSize, i) * ramSize;
				writers = CreateAuxFiles(workingDir, blockSize * maxAuxFilesNumber);
				int x = (int)Math.Pow(2,i) * ramSize;
				IntercalateAuxFiles(writers, readers, blockSize);
				blockSize *= maxAuxFilesNumber;
			}
			CloseStreamWriters(writers);
		}

		private static void IntercalateAuxFiles(StreamWriter[] writers, StreamReader[] readers, int blockSize)
		{
			int newBlockSize = blockSize * ramSize;
			int iterations = (int)Math.Ceiling((double) fileSize / (blockSize * maxAuxFilesNumber));
			for (int i = 0; i < iterations; i++)
			{
				MergeIteration(writers[i % maxAuxFilesNumber], readers, (i+1)*blockSize);
			}
		}

		private static void MergeIteration(StreamWriter output, StreamReader[] readers,int max)
		{
			while(SmallestChar(readers, max, out char smallest))
			{ 
				output.Write(smallest);
			}
		}

		private static bool SmallestChar(StreamReader[] readers, int max, out char smallest)
		{
			smallest = char.MaxValue;
			int streamIdx = -1;
			for (int i = 0; i < readers.Length; i++)
			{
				if (readers[i].BaseStream.Position >= max)
				{
					continue;
				}
				char ch = (char)(readers[i].BaseStream).ReadByte();
				if (ch == char.MaxValue)
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
			if (streamIdx != -1)
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
					block = SelectionSort(block, validCharCount);
					writers[currentWriter % maxAuxFilesNumber].Write(block,0,validCharCount);
				}
				currentWriter++;
			}
		}

		private static char[] SelectionSort(char[] array, int validCharCount)
		{
			for(int i = 0; i < validCharCount; i++)
			{
				int idx = i;
				char min = array[i];
				for (int j = i+1; j < validCharCount; j++)
				{
					if(array[j] < min)
					{
						idx = j;
						min = array[j];
					}
				}
				array[idx] = array[i];
				array[i] = min;
			}
			return array;
		}

		private static int Min (int a, int b)
		{
			return (a > b) ? b : a;
		}


		private static StreamWriter[] CreateAuxFiles(string workingDir,int currentBlockSize)
		{
			int minAuxFiles = Min((int)Math.Ceiling((float)fileSize / currentBlockSize), maxAuxFilesNumber);
			if(minAuxFiles == 1)
			{
				StreamWriter[] writer = new StreamWriter[1];
				writer[0] = new StreamWriter(workingDir + "Output.txt");
				return writer;
			}
			int auxFiles = createOnlyNecessaryAuxFiles ? minAuxFiles : maxAuxFilesNumber;
			StreamWriter[] writers = new StreamWriter[auxFiles];
			for(int i = 0; i < auxFiles; i++)
			{
				writers[i] = new StreamWriter(workingDir + "Aux" + AuxNumber + ".txt");
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
				readers[i].BaseStream.Position = 0;
				readers[i].DiscardBufferedData();
				readers[i].BaseStream.Seek(0, SeekOrigin.Begin);
			}
			return readers;
		}

	}
}
