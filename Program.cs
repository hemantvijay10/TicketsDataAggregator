using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace TicketsDataAggregator
{
    internal class Program
    {
        private static readonly Dictionary<string, CultureInfo> _domainCultures = new()
        {
            { ".com", new CultureInfo("en-US") },
            { ".fr",  new CultureInfo("fr-FR") },
            { ".jp",  new CultureInfo("ja-JP") }
        };

        private static void Main(string[] args)
        {
            string folderPath = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
                ? args[0]
                : PromptForFolder();

            if (!Directory.Exists(folderPath))
            {
                Console.Error.WriteLine($"Folder not found: {folderPath}");
                return;
            }

            var tickets = ReadAllTickets(folderPath);

            if (tickets.Count == 0)
            {
                Console.WriteLine("No tickets were found or parsed. Exiting without writing file.");
                Console.WriteLine("Press any key to close.");
                Console.ReadKey(intercept: true);
                return;
            }

            string outputPath = Path.Combine(folderPath, "aggregatedTickets.txt");
            WriteAggregatedFile(tickets, outputPath);

            Console.WriteLine($"Successfully wrote {tickets.Count} tickets to:\n  {outputPath}");
            Console.WriteLine("Press any key to close.");
            Console.ReadKey(intercept: true);
        }

        private static string PromptForFolder()
        {
            Console.Write("Enter the folder path containing your ticket PDFs: ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        private static List<(string Title, DateTime When)> ReadAllTickets(string folderPath)
        {
            var result = new List<(string, DateTime)>();
            foreach (var file in Directory.EnumerateFiles(folderPath, "*.pdf"))
            {
                try
                {
                    result.AddRange(ParsePdfTickets(file));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Warning: could not parse '{file}': {ex.Message}");
                }
            }
            return result;
        }

        private static IEnumerable<(string Title, DateTime When)> ParsePdfTickets(string pdfPath)
        {
            using var doc = PdfDocument.Open(pdfPath);
            foreach (Page page in doc.GetPages())
            {
                var lines = page.Text
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l))
                    .ToArray();

                for (int i = 0; i + 3 < lines.Length; i++)
                {
                    if (lines[i].StartsWith("Movie Title:", StringComparison.OrdinalIgnoreCase)
                     && lines[i + 1].StartsWith("Website:", StringComparison.OrdinalIgnoreCase)
                     && lines[i + 2].StartsWith("Date:", StringComparison.OrdinalIgnoreCase)
                     && lines[i + 3].StartsWith("Time:", StringComparison.OrdinalIgnoreCase))
                    {
                        string title = lines[i].Substring(12).Trim();
                        string url = lines[i + 1].Substring(8).Trim();
                        string dateText = lines[i + 2].Substring(5).Trim();
                        string timeText = lines[i + 3].Substring(5).Trim();

                        var culture = GetCultureFromUrl(url);
                        if (DateTime.TryParse($"{dateText} {timeText}", culture,
                                              DateTimeStyles.None,
                                              out var dtUtc))
                        {
                            yield return (title, dtUtc.ToUniversalTime());
                        }
                        i += 3; // skip ahead
                    }
                }
            }
        }

        private static CultureInfo GetCultureFromUrl(string url)
        {
            foreach (var kvp in _domainCultures)
            {
                if (url.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }
            return CultureInfo.InvariantCulture;
        }

        private static void WriteAggregatedFile(
            List<(string Title, DateTime When)> tickets,
            string outputPath)
        {
            using var writer = new StreamWriter(outputPath, append: false);
            foreach (var (title, when) in tickets)
            {
                string ts = when.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                writer.WriteLine($"{title} | {ts}");
            }
        }
    }
}
