namespace Day1;

internal class Program
{
    static void Main(string[] args)
    {
        var (listA, listB) = File.ReadAllLines("input.txt")
            .Aggregate(
                (new List<int>(), new List<int>()),
                (acc, line) =>
                {
                    var items = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    acc.Item1.Add(int.Parse(items[0]));
                    acc.Item2.Add(int.Parse(items[1]));
                    return acc;
                });

        listA.Sort();
        listB.Sort();
        var bCounts = new Dictionary<int, int>(listB.CountBy(x => x));

        var totalDistance = 0;
        var similarityScore = 0;
        for (var i = 0; i < listA.Count; i++)
        {
            totalDistance += Math.Abs(listA[i] - listB[i]);
            similarityScore += listA[i] * bCounts.GetValueOrDefault(listA[i], 0);
        }

        Console.WriteLine($"Total distance: {totalDistance}");
        Console.WriteLine($"Similarity score: {similarityScore}");
    }
}
