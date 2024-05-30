using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public interface IFileReader
{
    string ReadFile(string filePath);
}

public class FileReader : IFileReader
{
    public string ReadFile(string filePath)
    {
        try
        {
            return File.ReadAllText(filePath);
        }
        catch (IOException e)
        {
            throw new IOException("Unable to read the file: " + e.Message);
        }
    }
}

public class Program
{
    private readonly IFileReader _fileReader;

    public Program(IFileReader fileReader)
    {
        _fileReader = fileReader;
    }

    public static bool IsVowel(char c)
    {
        char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };
        return vowels.Contains(c);
    }

    public string[] GetUniqueVowelWords(string fileContent)
    {
        return fileContent
            .Split(new char[] { ' ', '\t', '\n', '\r', ',', '.', '!', '?', '|', '(', ')', '$', '=', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => Regex.Replace(word, "[^a-zA-Z]", ""))
            .Where(word => !string.IsNullOrEmpty(word) && word.All(IsVowel))
            .Distinct()
            .ToArray();
    }

    public void Run(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist: " + filePath);
                return;
            }

            string fileContent = _fileReader.ReadFile(filePath);
            Console.WriteLine("File Content:");
            Console.WriteLine(fileContent);

            string[] words = GetUniqueVowelWords(fileContent);

            Console.WriteLine("Filtered Words:");
            foreach (string word in words)
            {
                Console.WriteLine(word);
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("Unable to read the file: " + e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }
    }

    static void Main(string[] args)
    {
        string filePath = "/Users/nataliiagricisin/Documents/3 курс/II семестр/Lab2_MSTest/Lab2_MSTest/input.txt";
        var program = new Program(new FileReader());
        program.Run(filePath);
    }
}
