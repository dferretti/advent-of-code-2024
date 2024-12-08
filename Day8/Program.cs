var chars = File.ReadAllLines("input.txt").Select(line => line.ToCharArray()).ToArray();
var grid = new Grid(chars);

var antennas = new Dictionary<char, List<Point>>();
for (var row = 0; row < grid.Rows; row++)
{
    for (var col = 0; col < grid.Cols; col++)
    {
        var c = grid[new Point(row, col)];
        if (c == '.') continue;

        if (!antennas.TryGetValue(c, out var points))
            points = antennas[c] = [];

        antennas[c].Add(new Point(row, col));
    }
}

var part1Antinodes = new HashSet<Point>();
foreach (var (freq, points) in antennas)
{
    foreach (var p1 in points)
    {
        foreach (var p2 in points)
        {
            if (p1 == p2) continue;
            var v = p2 - p1;
            var antinode = p2 + v;
            if (grid.IsInBounds(antinode))
                part1Antinodes.Add(antinode);
        }
    }
}

var part2Antinodes = new HashSet<Point>();
foreach (var (freq, points) in antennas)
{
    foreach (var p1 in points)
    {
        foreach (var p2 in points)
        {
            if (p1 == p2) continue;
            var v = (p2 - p1).Reduce();
            var antinode = p1 + v;
            while (grid.IsInBounds(antinode))
            {
                part2Antinodes.Add(antinode);
                antinode += v;
            }
        }
    }
}

Console.WriteLine($"Part 1 antinodes: {part1Antinodes.Count}");
Console.WriteLine($"Part 2 antinodes: {part2Antinodes.Count}");

record Point(int Row, int Col)
{
    public static Point operator +(Point p, Vector v) => new(p.Row + v.DR, p.Col + v.DC);

    public static Vector operator -(Point p1, Point p2) => new(p1.Row - p2.Row, p1.Col - p2.Col);
}

record Vector(int DR, int DC)
{
    public Vector Reduce()
    {
        var gcd = GCD(Math.Abs(DR), Math.Abs(DC));
        return new(DR / gcd, DC / gcd);
    }

    private static int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);
}

class Grid(char[][] chars)
{
    public int Rows { get; } = chars.Length;

    public int Cols { get; } = chars[0].Length;

    private readonly char[][] _chars = chars;

    public char this[Point p] => _chars[p.Row][p.Col];

    public bool IsInBounds(Point p) => p.Row >= 0 && p.Row < Rows && p.Col >= 0 && p.Col < Cols;
}