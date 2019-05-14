using System;
using System.IO;

namespace ExternalMergeSort
{
    public abstract class ExternalSort
	{
		/// <summary>
		/// When true doesn't create aux files that won't be used, the advantage, however, of create the aux files is that you can trace down every iteration 
		/// </summary>
		protected static bool createOnlyNecessaryAuxFiles = true;

		protected int mainMemorySize;

		protected int maxAuxFilesNumber;

		protected string workingDir;

        protected string mainFileName;

		protected static long fileSize = 0;

        protected static int auxNumber;

        protected static int blockSize;

		protected static int AuxNumber
		{
			get
			{
				return auxNumber++;
			}
		}	

		protected static bool SmallestChar(StreamReader[] readers, int max, out char smallest)
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

        protected virtual StreamWriter[] CreateAuxFiles(int numberOfFiles, string workingDir)
        {
            StreamWriter[] writers = new StreamWriter[numberOfFiles];
            for (int i = 0; i < numberOfFiles; i++)
            {
                writers[i] = new StreamWriter(workingDir + "Aux" + AuxNumber + ".txt");
            }
            return writers;
        }

        public abstract void ExternallySort();

        public ExternalSort (string workingDir, string mainFileName, int mainMemorySize, int maxAuxFilesNumber)
        {
            this.workingDir = workingDir + "\\";
            this.mainFileName = mainFileName;
            this.mainMemorySize = mainMemorySize;
            this.maxAuxFilesNumber = maxAuxFilesNumber;
        }
	}
}
