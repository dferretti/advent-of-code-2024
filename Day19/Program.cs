using System.Buffers;
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
var part2Count = 0L;

var combined = string.Join("|", availablePatterns);
var regex = new Regex($"^({combined})+$", RegexOptions.Compiled, TimeSpan.FromSeconds(5));
var cache = new Dictionary<string, long>();

foreach (var design in lines.Skip(2))
{
    count++;
    Console.WriteLine($"Solving {count}: {design}");
    try
    {
        var matches = regex.Matches(design);

        if (regex.IsMatch(design))
        {
            Console.Write($"Solved: {design}");
            var combos = CountCombinations(design, availablePatterns, cache);
            part1Count++;
            part2Count += combos;
            Console.WriteLine($" - {combos}");
        }
    }
    catch { Console.WriteLine($"Timed out: {design}"); }
}

Console.WriteLine($"Part 1: {part1Count}");
Console.WriteLine($"Part 2: {part2Count}");

static long CountCombinations(ReadOnlySpan<char> design, string[] availablePatterns, Dictionary<string, long> cache)
{
    if (design.Length == 0)
        return 1;

    var key = design.ToString();
    if (cache.TryGetValue(key, out var cached))
        return cached;

    var count = 0L;
    foreach (var pattern in availablePatterns)
    {
        if (design.StartsWith(pattern))
            count += CountCombinations(design[pattern.Length..], availablePatterns, cache);
    }

    cache[key] = count;
    return count;
}
