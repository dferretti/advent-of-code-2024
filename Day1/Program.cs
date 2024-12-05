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

        var totalDistance = 0;
        for (var i = 0; i < listA.Count; i++)
        {
            totalDistance += Math.Abs(listA[i] - listB[i]);
        }

        Console.WriteLine($"Total distance: {totalDistance}");
    }
}
