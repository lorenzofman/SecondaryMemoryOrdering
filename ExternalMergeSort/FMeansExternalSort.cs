using System;
using System.IO;

namespace ExternalMergeSort
{
    public class FMeansExternalSort : ExternalSort
    {
        public override void ExternallySort()
        {
            FMeansMerge();
        }

        private void FMeansMerge()
        {
            StreamReader mainReader = new StreamReader(mainFileName);
            fileSize = mainReader.BaseStream.Length;
            blockSize = mainMemorySize;
            StreamWriter[] writers = CreateAuxFiles(mainMemorySize,workingDir);
            PopulateAuxFiles(writers, mainMemorySize, mainReader);
            int intercalationsRequired = (int)Math.Ceiling(Math.Log((float)fileSize / mainMemorySize, maxAuxFilesNumber));
            for (int i = 0; i < intercalationsRequired; i++)
            {
                StreamReader[] readers = Utils.SwitchStreams(writers);
                //int blockSize = (int)Math.Pow(ramSize, i) * ramSize;
                writers = CreateAuxFiles(blockSize * maxAuxFilesNumber,workingDir);
                IntercalateAuxFiles(writers, readers, blockSize);
                blockSize *= maxAuxFilesNumber;
            }
            CloseStreamWriters(writers);
        }

        private void PopulateAuxFiles(StreamWriter[] writers, int readSize, StreamReader reader)
        {
            int currentWriter = 0;
            while (!reader.EndOfStream)
            {
                int size = Utils.Min(readSize, mainMemorySize);
                char[] block = new char[size];
                for (int i = 0; i < readSize; i += size)
                {
                    int validCharCount = reader.ReadBlock(block, 0, size);
                    block = Utils.SelectionSort(block, validCharCount);
                    writers[currentWriter % maxAuxFilesNumber].Write(block, 0, validCharCount);
                }
                currentWriter++;
            }
        }

        private void IntercalateAuxFiles(StreamWriter[] writers, StreamReader[] readers, int blockSize)
        {
            int iterations = (int)Math.Ceiling((double)fileSize / (blockSize * maxAuxFilesNumber));
            for (int i = 0; i < iterations; i++)
            {
                MergeIteration(writers[i % maxAuxFilesNumber], readers, (i + 1) * blockSize);
            }
        }

        private void MergeIteration(StreamWriter output, StreamReader[] readers, int max)
        {
            while (SmallestChar(readers, max, out char smallest))
            {
                output.Write(smallest);
            }
        }

        private void CloseStreamWriters(StreamWriter[] writers)
        {
            foreach (StreamWriter writer in writers)
            {
                writer.Close();
            }
        }

        protected override StreamWriter[] CreateAuxFiles(int currentBlockSize, string workingDir)
        {
            int minAuxFiles = Utils.Min((int)Math.Ceiling((float)fileSize / currentBlockSize), maxAuxFilesNumber);
            if (minAuxFiles == 1)
            {
                StreamWriter[] writer = new StreamWriter[1];
                writer[0] = new StreamWriter(workingDir + Path.GetFileNameWithoutExtension(mainFileName) + "Output.txt");
                return writer;
            }
            int auxFiles = createOnlyNecessaryAuxFiles ? minAuxFiles : maxAuxFilesNumber;
            return base.CreateAuxFiles(auxFiles, workingDir);
        }

        public FMeansExternalSort(string workingDir, string mainFileName, int mainMemorySize, int maxAuxFilesNumber) : base(workingDir, mainFileName, mainMemorySize, maxAuxFilesNumber)
        {

        }
    }
}
