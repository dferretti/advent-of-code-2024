var map = File.ReadAllLines("input.txt")
    .Select(line => line.ToCharArray().Select(c => int.Parse(c.ToString())).ToArray())
    .ToArray();
var grid = new Grid(map);

var part1Sum = 0;
var part2Sum = 0;
for (var r = 0; r < map.Length; r++)
{
    for (var c = 0; c < map[r].Length; c++)
    {
        if (map[r][c] == 0)
        {
            part1Sum += Part1Score(grid, r, c);
            part2Sum += Part2Score(grid, r, c);
        }
    }
}

Console.WriteLine($"Part 1 sum: {part1Sum}");
Console.WriteLine($"Part 2 sum: {part2Sum}");

static int Part1Score(Grid grid, int startR, int startC)
{
    return GetPeaks(grid, new(startR, startC), 0).Count;

    static HashSet<Point> GetPeaks(Grid grid, Point p, int height)
    {
        if (height == 9)
            return [p];

        var neighbors = new Point[]
        {
            new(p.R - 1, p.C),
            new(p.R + 1, p.C),
            new(p.R, p.C - 1),
            new(p.R, p.C + 1)
        };
        var peaks = new HashSet<Point>();
        foreach (var n in neighbors)
        {
            if (!grid.IsInBounds(n))
                continue;
            if (grid[n] == height + 1)
                peaks.UnionWith(GetPeaks(grid, n, height + 1));
        }

        return peaks;
    }
}

static int Part2Score(Grid grid, int startR, int startC)
{
    return GetRating(grid, new(startR, startC), 0, []);

    static int GetRating(Grid grid, Point p, int height, Dictionary<Point, int> cache)
    {
        if (cache.TryGetValue(p, out var rating))
            return rating;

        if (height == 9)
            return 1;

        var neighbors = new Point[]
        {
            new(p.R - 1, p.C),
            new(p.R + 1, p.C),
            new(p.R, p.C - 1),
            new(p.R, p.C + 1)
        };
        rating = 0;
        foreach (var n in neighbors)
        {
            if (!grid.IsInBounds(n))
                continue;
            if (grid[n] == height + 1)
                rating += GetRating(grid, n, height + 1, cache);
        }

        cache[p] = rating; // memoize
        return rating;
    }
}

record Point(int R, int C);

class Grid(int[][] map)
{
    private readonly int[][] _map = map;

    public int Rows => _map.Length;

    public int Cols => _map[0].Length;

    public int this[Point p] => _map[p.R][p.C];

    public bool IsInBounds(Point p) => p.R >= 0 && p.R < Rows && p.C >= 0 && p.C < Cols;
}