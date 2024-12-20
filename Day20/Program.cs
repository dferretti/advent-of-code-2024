var grid = new Grid(File.ReadAllLines("input.txt")
    .Select(line => line.ToCharArray())
    .ToArray());

var distances = new Dictionary<Position, int> { [grid.Start] = 0 };
var directions = new[] { Vector.Up, Vector.Down, Vector.Left, Vector.Right };

for (var pos = grid.Start; pos != grid.End;)
{
    foreach (var dir in directions)
    {
        var newPos = pos + dir;
        if (grid[newPos] == '#' || distances.ContainsKey(newPos)) continue;
        distances[newPos] = distances[pos] + 1;
        pos = newPos;
        break;
    }
}

var cheats = new Dictionary<(Position, Vector), int>();
foreach (var (pos, dist) in distances)
{
    foreach (var dir in directions)
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

record Position(int R, int C)
{
    public static Position operator +(Position a, Vector b) => new(a.R + b.DR, a.C + b.DC);
}

record Vector(int DR, int DC)
{
    public static Vector Up { get; } = new Vector(-1, 0);
    public static Vector Down { get; } = new Vector(1, 0);
    public static Vector Left { get; } = new Vector(0, -1);
    public static Vector Right { get; } = new Vector(0, 1);
}

class Grid(char[][] chars)
{
    public Position Start { get; }
        = chars.SelectMany((row, r) => row.Select((cell, c) => (Cell: cell, Position: new Position(r, c)))).Single(x => x.Cell == 'S').Position;

    public Position End { get; }
        = chars.SelectMany((row, r) => row.Select((cell, c) => (Cell: cell, Position: new Position(r, c)))).Single(x => x.Cell == 'E').Position;

    public char this[Position pos] => chars[pos.R][pos.C];
}