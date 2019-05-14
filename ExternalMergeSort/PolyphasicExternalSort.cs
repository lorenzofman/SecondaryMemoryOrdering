using System;
using System.IO;

namespace ExternalMergeSort
{
    public class PolyphasicExternalSort : ExternalSort
    {
        public override void ExternallySort()
        {
            PolyphasicMerge();
        }

        private void PolyphasicMerge()
        {
            if (maxAuxFilesNumber < 3)
            {
                throw new Exception("Number of aux files should be equal or greater than three (3)");
            }
            StreamReader mainReader = new StreamReader(mainFileName);
            fileSize = mainReader.BaseStream.Length;
            // Less 1 because the original is going to be used as well
            StreamWriter[] writers = CreateAuxFiles(maxAuxFilesNumber, workingDir);
            PolyphasicInitialPopulate(mainReader, writers[0], writers[1]);
            AuxiliarFiles auxiliarFiles = new AuxiliarFiles(writers);
            PolyphasicIteration(auxiliarFiles);
            auxiliarFiles.End(workingDir + Path.GetFileNameWithoutExtension(mainFileName));
        }

        private void PolyphasicIteration(AuxiliarFiles auxiliarFiles)
        {
            long fibThatFit = NextFib(fileSize);
            for (long i = NextFib(2, out long a, out long b); i <= fibThatFit; i = NextFib(i + 1, out a, out b))
            {
                MergeTwoFiles(auxiliarFiles, a, b);
                auxiliarFiles.Next();
            }
        }

        private bool AnyFinish(AuxiliarFiles aux)
        {
            return aux.Reader.EndOfStream || aux.Intercalator.EndOfStream;
        }

        private bool CharsFlushed(char a, char b)
        {
            return a == char.MaxValue && b == char.MaxValue;
        }

        private void MergeTwoFiles(AuxiliarFiles aux, long readerBlockSize, long intercalatorBlockSize)
        {
            bool readerIsUpdated = false, intercalatorIsUpdated = false;
            char readerPointerValue = char.MaxValue, intercalatorPointerValue = char.MaxValue;
            long readerCharsRead = 0, intercalatorCharsRead = 0;
            while (!AnyFinish(aux) || !CharsFlushed(readerPointerValue, intercalatorPointerValue))
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

        private  void PolyphasicInitialPopulate(StreamReader main, StreamWriter firstWriter, StreamWriter secondWriter)
        {
            NextFib(fileSize, out long secondSize, out long firstSize);
            for (long i = 0; i < firstSize; i++)
            {
                secondWriter.Write((char)main.Read());
            }
            for (long i = firstSize; i < fileSize; i++)
            {
                firstWriter.Write((char)main.Read());
            }
            for (long i = fileSize; i < firstSize + secondSize; i++)
            {
                firstWriter.Write(Utils.NoDataChar);
            }
        }

        private long NextFib(long size)
        {
            return NextFib(size, out _, out _);
        }

        private long NextFib(long size, out long a, out long b)
        {
            a = 0;
            b = 1;
            long c;
            while (size > a + b)
            {
                c = a + b;
                a = b;
                b = c;
            }
            return a + b;
        }

        public PolyphasicExternalSort(string workingDir, string mainFileName, int mainMemorySize, int maxAuxFilesNumber) : base(workingDir, mainFileName, mainMemorySize, maxAuxFilesNumber)
        {

        }

    }
}
