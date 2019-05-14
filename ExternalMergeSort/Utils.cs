using System.IO;

namespace ExternalMergeSort
{
    public class Utils
    {
        public static readonly char NoDataChar = (char)(char.MaxValue - 1);

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

        public static char[] SelectionSort(char[] array, int validCharCount)
        {
            for (int i = 0; i < validCharCount; i++)
            {
                int idx = i;
                char min = array[i];
                for (int j = i + 1; j < validCharCount; j++)
                {
                    if (array[j] < min)
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

        public static int Min(int a, int b)
        {
            return (a > b) ? b : a;
        }
    }

}
