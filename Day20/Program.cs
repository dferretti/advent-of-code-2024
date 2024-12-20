var grid = new Grid(File.ReadAllLines("input.txt")
    .Select(line => line.ToCharArray())
    .ToArray());

var distances = new Dictionary<Position, int> { [grid.Start] = 0 };

for (var pos = grid.Start; pos != grid.End;)
{
    foreach (var dir in Vector.Directions)
    {
        var newPos = pos + dir;
        if (grid[newPos] == '#' || distances.ContainsKey(newPos)) continue;
        distances[newPos] = distances[pos] + 1;
        pos = newPos;
        break;
    }
}

Part1(distances);
Part2(distances);

static void Part1(IReadOnlyDictionary<Position, int> distances)
{
    var cheats = new Dictionary<(Position, Vector), int>();
    foreach (var (pos, dist) in distances)
    {
        foreach (var dir in Vector.Directions)
        {
            var newPos = pos + dir + dir;
            if (!distances.TryGetValue(newPos, out var newDist)) continue;
            var cheatAmount = (newDist - dist) - 2;
            if (cheatAmount <= 0) continue;
            cheats[(pos, dir)] = cheatAmount;
        }
    }

    //foreach (var (cheatAmount, count) in cheats.GroupBy(d => d.Value).Select(g => (g.Key, Count: g.Count())).OrderBy(x => x.Key))
    //Console.WriteLine($"There are {count} cheats that save {cheatAmount} picoseconds");
    var part1 = cheats
        .GroupBy(d => d.Value)
        .Select(g => (CheatAmount: g.Key, Count: g.Count()))
        .Where(x => x.CheatAmount >= 100)
        .Sum(x => x.Count);
    Console.WriteLine($"Part 1: {part1}");
}

static void Part2(IReadOnlyDictionary<Position, int> distances)
{
    var cheats = new Dictionary<(Position, Position), int>();
    foreach (var (p1, startD) in distances)
    {
        foreach (var (p2, endD) in distances)
        {
            var manhattan = Position.ManhattanDistance(p1, p2);
            if (manhattan > 20) continue;

            var cheatAmount = (endD - startD) - manhattan;
            if (cheatAmount <= 0) continue;

            cheats[(p1, p2)] = cheatAmount;
        }
    }

    //foreach (var (cheatAmount, count) in cheats.GroupBy(d => d.Value).Select(g => (g.Key, Count: g.Count())).OrderBy(x => x.Key))
    //Console.WriteLine($"There are {count} cheats that save {cheatAmount} picoseconds");
    var part2 = cheats
        .GroupBy(d => d.Value)
        .Select(g => (CheatAmount: g.Key, Count: g.Count()))
        .Where(x => x.CheatAmount >= 100)
        .Sum(x => x.Count);
    Console.WriteLine($"Part 2: {part2}");
}

record Position(int R, int C)
{
    public static Position operator +(Position a, Vector b) => new(a.R + b.DR, a.C + b.DC);

    public static int ManhattanDistance(Position a, Position b) => Math.Abs(a.R - b.R) + Math.Abs(a.C - b.C);   
}

record Vector(int DR, int DC)
{
    public static Vector Up { get; } = new Vector(-1, 0);
    public static Vector Down { get; } = new Vector(1, 0);
    public static Vector Left { get; } = new Vector(0, -1);
    public static Vector Right { get; } = new Vector(0, 1);
    public static IReadOnlyList<Vector> Directions { get; } = [Up, Down, Left, Right];
}

class Grid(char[][] chars)
{
    public Position Start { get; }
        = chars.SelectMany((row, r) => row.Select((cell, c) => (Cell: cell, Position: new Position(r, c)))).Single(x => x.Cell == 'S').Position;

    public Position End { get; }
        = chars.SelectMany((row, r) => row.Select((cell, c) => (Cell: cell, Position: new Position(r, c)))).Single(x => x.Cell == 'E').Position;

    public char this[Position pos] => chars[pos.R][pos.C];
}