using System;
using System.Collections;
using System.IO;

namespace ExternalMergeSort
{
    public class Queue<T>
    {
        private Queue queue;
        public void Enqueue(T obj)
        {
            queue.Enqueue(obj);
        }
        public T Peek()
        {
            return (T) queue.Peek();
        }
        public T Dequeue()
        {
            return (T) queue.Dequeue();
        }
        public int Count
        {
            get
            {
                return queue.Count;
            }
        }
        public Queue()
        {
            queue = new Queue();
        }
    }

    public class AuxiliarFiles
    {
        private StreamReader reader;
        private StreamReader intercalator;
        private Queue<StreamWriter> others;
        public AuxiliarFiles(StreamWriter[] auxFiles)
        {
            this.reader = ExternalSort.SwitchStream(auxFiles[0]);
            this.intercalator = ExternalSort.SwitchStream(auxFiles[1]);
            others = new Queue<StreamWriter>();
            for(int i = 2; i < auxFiles.Length; i++)
                others.Enqueue(auxFiles[i]);
        }
        public void Next()
        {
            others.Enqueue(ExternalSort.SwitchStream(reader));
            reader = intercalator;
            intercalator = ExternalSort.SwitchStream(others.Dequeue());
        }
        private void CreateOutput(string wd, StreamReader finalUnfiltered)
        {
            StreamWriter output = new StreamWriter(wd + "Output.txt");
            for (int i = 0; i < finalUnfiltered.BaseStream.Length; i++)
            {
                char readChar = (char)finalUnfiltered.Read();
                if (readChar == ExternalSort.NoDataChar)
                    break;
                output.Write(readChar);
            }
            output.Close();
        }
        public void End(string wd)
        {
            CreateOutput(wd, intercalator);
            reader.Close();
            intercalator.Close();
            while(others.Count > 0)
            {
                StreamWriter sw = others.Dequeue();
                sw.Close();
            }
        }

        public StreamReader Reader
        {
            get
            {
                return this.reader;
            }
        }
        public StreamReader Intercalator
        {
            get
            {
                return this.intercalator;
            }
        }
        public StreamWriter Writer
        {
            get
            {
                return others.Peek();
            }
        }
    }
	public class ExternalSort
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

        public static readonly char NoDataChar = (char)(char.MaxValue - 1);
		private static int AuxNumber
		{
			get
			{
				return auxNumber++;
			}
		}
        static bool useFmeans = false;
        static void Main()
		{
            string mainFilePath = workingDir + "BigFile.txt";
            if (useFmeans)
            {
                FMeansMerge(mainFilePath);
            }
            else
            {
                PolyphasicMerge(mainFilePath);
            }
		}
		private static void PolyphasicMerge (string mainFilePath)
		{
            if(maxAuxFilesNumber < 2)
            {
                throw new Exception("Number of aux files should be superior to two (2)");
            }
            StreamReader mainReader = new StreamReader(mainFilePath);
            fileSize = mainReader.BaseStream.Length;
            // Less 1 because the original is going to be used as well
            StreamWriter[] writers = CreateAuxFiles(maxAuxFilesNumber, workingDir);
            PolyphasicInitialPopulate(mainReader, writers[0], writers[1]);
            AuxiliarFiles auxiliarFiles = new AuxiliarFiles(writers);
            PolyphasicIteration(auxiliarFiles);
            auxiliarFiles.End(workingDir);
        }
        private static void PolyphasicIteration(AuxiliarFiles auxiliarFiles)
        {
            long a, b;
            long fibThatFit = NextFib(fileSize);
            for (long i = NextFib(2, out a, out b); i <= fibThatFit; i = NextFib(i+1, out a, out b))
            {
                MergeTwoFiles(auxiliarFiles, a, b);
                auxiliarFiles.Next();
            }
        }
        private static bool AnyFinish(AuxiliarFiles aux)
        {
            return aux.Reader.EndOfStream || aux.Intercalator.EndOfStream;
        }
        private static bool CharsFlushed(char a, char b)
        {
            return a == char.MaxValue && b == char.MaxValue;
        }
        private static void MergeTwoFiles(AuxiliarFiles aux, long readerBlockSize, long intercalatorBlockSize)
        {
            bool readerIsUpdated = false, intercalatorIsUpdated = false;
            char readerPointerValue = char.MaxValue, intercalatorPointerValue = char.MaxValue;
            long readerCharsRead = 0, intercalatorCharsRead = 0;
            while(!AnyFinish(aux) || !CharsFlushed(readerPointerValue,intercalatorPointerValue))
            {
                if (readerIsUpdated == false)
                {
                    if (readerCharsRead < readerBlockSize)
                    {
                        readerPointerValue = (char)aux.Reader.Read();
                        readerIsUpdated = true;
                        readerCharsRead++;
                    }
                    else
                    {
                        readerPointerValue = char.MaxValue;
                    }
                }
                if (intercalatorIsUpdated == false)
                {
                    if (intercalatorCharsRead < intercalatorBlockSize)
                    {
                        intercalatorPointerValue = (char)aux.Intercalator.Read();
                        intercalatorIsUpdated = true;
                        intercalatorCharsRead++;
                    }
                    else
                    {
                        intercalatorPointerValue = char.MaxValue;
                    }
                }
                if (readerPointerValue == char.MaxValue && intercalatorPointerValue == char.MaxValue)
                {
                    readerCharsRead = 0;
                    intercalatorCharsRead = 0;
                    continue;
                }
                if (readerPointerValue < intercalatorPointerValue)
                {
                    aux.Writer.Write(readerPointerValue);
                    readerIsUpdated = false;
                }
                else
                {
                    aux.Writer.Write(intercalatorPointerValue);
                    intercalatorIsUpdated = false;
                }
            }
        }
        private static void PolyphasicInitialPopulate(StreamReader main, StreamWriter firstWriter, StreamWriter secondWriter)
        {
            NextFib(fileSize, out long secondSize, out long firstSize);
            for(long i = 0; i < firstSize; i++)
            {
                secondWriter.Write((char)main.Read());
            }
            for(long i = firstSize; i < fileSize; i++)
            {
                firstWriter.Write((char)main.Read());
            }
            for (long i = fileSize; i < firstSize + secondSize; i++)
            {
                firstWriter.Write(NoDataChar);
            }
        }
        public static long NextFib(long size)
        {
            return NextFib(size, out _, out _);
        }

        public static long NextFib(long size, out long a, out long b)
        {
            a = 0;
            b = 1;
            long c;
            while(size > a + b)
            {
                c = a + b;
                a = b;
                b = c;
            }
            return a+b;
        }
		private static void FMeansMerge(string mainFilePath)
		{
			StreamReader mainReader = new StreamReader(mainFilePath);
			fileSize = mainReader.BaseStream.Length;
			blockSize = ramSize;
			StreamWriter[] writers = CreateAuxFiles(workingDir, ramSize);
			PopulateAuxFiles(writers, ramSize, mainReader);
			int intercalationsRequired = (int)Math.Ceiling(Math.Log((float)fileSize/ramSize,maxAuxFilesNumber));
			for(int i = 0; i < intercalationsRequired; i++)
			{ 
				StreamReader[] readers = SwitchStreams(writers);
				//int blockSize = (int)Math.Pow(ramSize, i) * ramSize;
				writers = CreateAuxFiles(workingDir, blockSize * maxAuxFilesNumber);
				IntercalateAuxFiles(writers, readers, blockSize);
				blockSize *= maxAuxFilesNumber;
			}
			CloseStreamWriters(writers);
		}

		private static void IntercalateAuxFiles(StreamWriter[] writers, StreamReader[] readers, int blockSize)
		{
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
            return CreateAuxFiles(auxFiles, workingDir);
		}
        private static StreamWriter[] CreateAuxFiles(int numberOfFiles, string workingDir)
        {
            StreamWriter[] writers = new StreamWriter[numberOfFiles];
            for (int i = 0; i < numberOfFiles; i++)
            {
                writers[i] = new StreamWriter(workingDir + "Aux" + AuxNumber + ".txt");
            }
            return writers;
        }
        public static StreamReader SwitchStream(StreamWriter writer)
        {
            string filename = (writer.BaseStream as FileStream).Name;
            writer.Close();
            StreamReader reader = new StreamReader(filename);
            reader.BaseStream.Position = 0;
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            return reader;
        }
        public static StreamWriter SwitchStream(StreamReader reader)
        {
            string filename = (reader.BaseStream as FileStream).Name;
            reader.Close();
            StreamWriter writer = new StreamWriter(filename);
            return writer;
        }

        public static StreamReader[] SwitchStreams(StreamWriter[] writers)
		{
			StreamReader[] readers = new StreamReader[writers.Length];
			for (int i = 0; i < writers.Length; i++)
			{
				readers[i] = SwitchStream(writers[i]);
			}
			return readers;
		}
	}
}
