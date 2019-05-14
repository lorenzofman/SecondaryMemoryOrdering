using System;
using System.IO;

namespace ExternalMergeSort
{
    public class EntryPoint
    {
        private static void Main()
        {
            ExternalSort externalSort;
            char externalType;
            string consoleInput;
            do
            {
                Console.WriteLine("Type P (Polyphasic External Sort) or F (F Means External Sort)");
                consoleInput = Console.ReadLine();
                externalType = (consoleInput.Length == 1) ? consoleInput[0] : '\0';
            } while (externalType != 'P' && externalType != 'F' && externalType != 'p' && externalType != 'f');

            string workingDir = Directory.GetCurrentDirectory();
            string mainFileName = GetMainFileName();
            int mainMemorySize = GetMainMemorySize();
            int auxFiles = GetAuxFilesNumber();

            if (externalType == 'P' || externalType == 'p')
            {
                externalSort = new PolyphasicExternalSort(workingDir, mainFileName, mainMemorySize, auxFiles);
            }
            else
            {
                externalSort = new FMeansExternalSort(workingDir, mainFileName, mainMemorySize, auxFiles);
            }
            externalSort.ExternallySort();
        }

        private static string GetMainFileName()
        {
            string fileName;
            do
            {
                Console.WriteLine("Current Dir: " + Directory.GetCurrentDirectory());
                Console.WriteLine("File name: ");
                fileName = Console.ReadLine();
            } while (!File.Exists(Directory.GetCurrentDirectory() + "\\" + fileName));
            return fileName;
        }

        private static int GetMainMemorySize()
        {
            int size;
            string input;
            do
            {
                Console.WriteLine("Main Memory Size: ");
                input = Console.ReadLine();
            } while (!int.TryParse(input,out size));
            return size;
        }

        private static int GetAuxFilesNumber()
        {
            int auxFilesNumber;
            string input;
            do
            {
                Console.WriteLine("Aux File Number: (>= 3)");
                input = Console.ReadLine();
            } while (!int.TryParse(input, out auxFilesNumber) && auxFilesNumber >= 3);
            return auxFilesNumber;
        }
    }

}
