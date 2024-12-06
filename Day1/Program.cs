using System.Runtime.InteropServices;

var bCounts = new Dictionary<int, int>();
var (listA, listB) = File.ReadAllLines("input.txt")
    .Aggregate(
        (new List<int>(), new List<int>()),
        (acc, line) =>
        {
            var items = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var a = int.Parse(items[0]);
            var b = int.Parse(items[1]);
            acc.Item1.Add(a);
            acc.Item2.Add(b);
            CollectionsMarshal.GetValueRefOrAddDefault(bCounts, b, out _)++;
            return acc;
        });

listA.Sort();
listB.Sort();

var totalDistance = 0;
var similarityScore = 0;
for (var i = 0; i < listA.Count; i++)
{
    totalDistance += Math.Abs(listA[i] - listB[i]);
    similarityScore += listA[i] * bCounts.GetValueOrDefault(listA[i], 0);
}

Console.WriteLine($"Total distance: {totalDistance}");
Console.WriteLine($"Similarity score: {similarityScore}");