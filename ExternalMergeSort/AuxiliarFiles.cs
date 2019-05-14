using System.IO;

namespace ExternalMergeSort
{
    public class AuxiliarFiles
    {
        private StreamReader reader;

        private StreamReader intercalator;

        private readonly Queue<StreamWriter> others;

        public AuxiliarFiles(StreamWriter[] auxFiles)
        {
            this.reader = Utils.SwitchStream(auxFiles[0]);
            this.intercalator = Utils.SwitchStream(auxFiles[1]);
            others = new Queue<StreamWriter>();
            for (int i = 2; i < auxFiles.Length; i++)
                others.Enqueue(auxFiles[i]);
        }

        public void Next()
        {
            others.Enqueue(Utils.SwitchStream(reader));
            reader = intercalator;
            intercalator = Utils.SwitchStream(others.Dequeue());
        }

        private void CreateOutput(string wd, StreamReader finalUnfiltered)
        {
            StreamWriter output = new StreamWriter(wd + "Output.txt");
            for (int i = 0; i < finalUnfiltered.BaseStream.Length; i++)
            {
                char readChar = (char)finalUnfiltered.Read();
                if (readChar == Utils.NoDataChar)
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
            while (others.Count > 0)
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
}
