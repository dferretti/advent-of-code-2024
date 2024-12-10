var map = File.ReadAllLines("input.txt")
    .Select(line => line.ToCharArray().Select(c => int.Parse(c.ToString())).ToArray())
    .ToArray();
var grid = new Grid(map);

var part1Sum = 0;
for (var r = 0; r < map.Length; r++)
{
    for (var c = 0; c < map[r].Length; c++)
    {
        if (map[r][c] == 0)
            part1Sum += Part1Score(grid, r, c);
    }
}

Console.WriteLine($"Part 1 sum: {part1Sum}");

static int Part1Score(Grid grid, int startR, int startC)
{
    return GetPeaks(grid, new Point(startR, startC), 0).Count;

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

record Point(int R, int C);

class Grid
{
    private readonly int[][] _map;

    public int Rows => _map.Length;

    public int Cols => _map[0].Length;

    public Grid(int[][] map) => _map = map;

    public int this[Point p] => _map[p.R][p.C];

    public bool IsInBounds(Point p) => p.R >= 0 && p.R < Rows && p.C >= 0 && p.C < Cols;
}