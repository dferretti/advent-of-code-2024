﻿/*
 * flatten the grid of chars into a long string:
 * - first by taking each line and joining it with "|"
 * - then take each column and join with "|"
 * - then take each down-right-diagonal and join with "|"
 * - then each up-right-diagonal
 * Search for occurences of the word and the word in reverse
 */

using System.Buffers;
using System.Text;

var original = File.ReadAllLines("input.txt")
    .Select(line => line.ToCharArray())
    .ToArray();

var rows = original.Length;
var cols = original[0].Length;

var capacityEstimate = (rows + 1) * (cols + 1) * 4;
var haystackBuilder = new StringBuilder(capacityEstimate);

// copy rows to builder
foreach (var line in original)
{
    haystackBuilder.Append(line);
    haystackBuilder.Append('|');
}

// copy columns to builder
for (int c = 0; c < cols; c++)
{
    for (int r = 0; r < rows; r++)
        haystackBuilder.Append(original[r][c]);
    haystackBuilder.Append('|');
}

// copy down-right diagonals to builder
// evision the table is extended to the left, and we start at -cols and take each diagonal line from the top
/*
 * original table:
 * A B C
 * A B C
 * A B C
 * 
 * envisioned table:
 * . . . A B C
 * . . . A B C
 * . . . A B C
 * 
 * lets us grab lines like: 
 * . .\.\A B C
 * . . .\A\B C
 * . . . A\B\C
 */
for (int startC = -cols; startC < cols; startC++)
{
    // TODO: could be optimized to skip lines that are too short
    for (int r = 0; r < rows; r++)
    {
        int c = startC + r;
        if (c < 0 || c >= cols)
            continue;
        haystackBuilder.Append(original[r][c]);
    }
    haystackBuilder.Append('|');
}

// copy up-right diagonals to builder
for (int startc = -cols; startc < cols; startc++)
{
    for (int r = rows - 1; r >= 0; r--)
    {
        int c = startc + (rows - 1 - r);
        if (c < 0 || c >= cols)
            continue;
        haystackBuilder.Append(original[r][c]);
    }
    haystackBuilder.Append('|');
}

var haystack = haystackBuilder.ToString().AsSpan();
var needle = SearchValues.Create(["XMAS", "SAMX"], StringComparison.Ordinal);
var count = Count(haystack, needle);
Console.WriteLine($"Count: {count}");

// from https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-9/#strings-arrays-spans
static int Count(ReadOnlySpan<char> haystack, SearchValues<string> needle)
{
    var count = 0;
    var index = 0;
    while ((index = haystack.IndexOfAny(needle)) >= 0)
    {
        count++;
        haystack = haystack.Slice(index + 1);
    }
    return count;
}