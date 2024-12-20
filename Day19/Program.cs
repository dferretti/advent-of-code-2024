using System.Buffers;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

var lines = File.ReadAllLines("input.txt");

var availablePatterns = lines[0]
    .Split(',')
    .Select(x => x.Trim())
    .OrderByDescending(x => x.Length)
    .ToArray();
// var searchValues = SearchValues.Create(availablePatterns, StringComparison.Ordinal);


var count = 0;
var part1Count = 0;

var combined = string.Join("|", availablePatterns);
var regex = new Regex($"^({combined})+$", RegexOptions.Compiled | RegexOptions.NonBacktracking, TimeSpan.FromSeconds(5));

foreach (var design in lines.Skip(2))
{
    count++;
    Console.WriteLine($"Solving {count}: {design}");
    try
    {
        if (regex.IsMatch(design))
        {
            Console.WriteLine($"Solved: {design}");
            part1Count++;
        }
    }
    catch { Console.WriteLine($"Timed out: {design}"); }
}

Console.WriteLine($"Part 1: {part1Count}");
