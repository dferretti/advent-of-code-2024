var file = "input.txt";
var robots = File.ReadAllLines(file).Select(Parse).ToList();
var width = file == "example.txt" ? 11 : 101;
var height = file == "example.txt" ? 7 : 103;

//Print(robots, width, height);
Step(robots, width, height, 100);
//Print(robots, width, height);

//foreach (var q in Enumerable.Range(1, 4)) Console.WriteLine($"Quadrant {q}: {QuadrantScore(robots, width, height, q)}");
var part1Score = Enumerable.Range(1, 4).Select(q => QuadrantScore(robots, width, height, q)).Aggregate(1, (a, b) => a * b);
Console.WriteLine($"Part 1: {part1Score}");

static int QuadrantScore(IReadOnlyList<Robot> robots, int width, int height, int quadrant)
{
    var (minX, maxX, minY, maxY) = quadrant switch
    {
        1 => (0, width / 2, 0, height / 2),
        2 => (1 + (width / 2), width, 0, height / 2),
        3 => (0, width / 2, 1 + (height / 2), height),
        4 => (1 + (width / 2), width, 1 + (height / 2), height),
        _ => throw new ArgumentOutOfRangeException(nameof(quadrant))
    };

    return robots.Count(r => r.Position.X >= minX && r.Position.X < maxX && r.Position.Y >= minY && r.Position.Y < maxY);
}


static void Step(IReadOnlyList<Robot> robots, int width, int height, int steps)
{
    foreach (var robot in robots)
    {
        var newPos = robot.Position + (robot.Velocity * steps);
        var n = newPos.Mod(width, height);
        robot.Position = (robot.Position + (robot.Velocity * steps)).Mod(width, height);
    }
}

static void Print(IReadOnlyList<Robot> robots, int width, int height)
{
    for (var y = 0; y < height; y++)
    {
        for (var x = 0; x < width; x++)
        {
            var count = robots.Count(r => r.Position.X == x && r.Position.Y == y);
            Console.Write(count switch { 0 => ".", _ => count.ToString() });
        }
        Console.WriteLine();
    }
    Console.WriteLine("---");
}

static Robot Parse(string line)
{
    var parts = line.Split(" ");
    var p = ParsePart(parts[0]);
    var v = ParsePart(parts[1]);
    return new() { Position = new(p.X, p.Y), Velocity = new(v.X, v.Y) };

    static (int X, int Y) ParsePart(string part)
    {
        var parts = part.Split('=', ',');
        return (int.Parse(parts[1]), int.Parse(parts[2]));
    }
}

record Position(int X, int Y)
{
    public static Position operator +(Position p, Vector v) => new(p.X + v.X, p.Y + v.Y);

    public Position Mod(int width, int height) => new(M(X, width), M(Y, height));

    private static int M(int x, int m)
    {
        var r = x % m;
        return r < 0 ? r + m : r;
    }
}

record Vector(int X, int Y)
{
    public static Vector operator *(Vector v, int n) => new(v.X * n, v.Y * n);
}

class Robot
{
    public required Position Position { get; set; }

    public required Vector Velocity { get; set; }
}