var file = "input.txt";
var robots = File.ReadAllLines(file).Select(Parse).ToList();
var width = file == "example.txt" ? 11 : 101;
var height = file == "example.txt" ? 7 : 103;

Part1(robots, width, height);
// reset robots
robots = File.ReadAllLines(file).Select(Parse).ToList();
//Part2Attempt1(robots, width, height);
//Part2Attempt2(robots, width, height);
Part2Attempt3(robots, width, height);

static void Part1(IReadOnlyList<Robot> robots, int width, int height)
{
    //Print(robots, width, height);
    Step(robots, width, height, 100);
    //Print(robots, width, height);

    //foreach (var q in Enumerable.Range(1, 4)) Console.WriteLine($"Quadrant {q}: {QuadrantScore(robots, width, height, q)}");
    var part1Score = Enumerable.Range(1, 4).Select(q => QuadrantScore(robots, width, height, q)).Aggregate(1, (a, b) => a * b);
    Console.WriteLine($"Part 1: {part1Score}");
}

// literally just watch the console and see if i see a tree. no luck
static void Part2Attempt1(IReadOnlyList<Robot> robots, int width, int height)
{
    Console.SetWindowSize(width + 1, 63); // seems to be max height on my machine
    var step = 0;
    while (true)
    {
        step++;
        Console.Clear();
        Console.WriteLine($"Step {step}");
        Step(robots, width, height, 1);
        Print(robots, width, height, 60);
        Thread.Sleep(125);
    }
}

static void Part2Attempt2(IReadOnlyList<Robot> robots, int width, int height)
{
    var step = 0;
    while (true)
    {
        step++;
        if (step % 100 == 0) Console.WriteLine($"Step {step}");
        Step(robots, width, height, 1);

        // see if 3 consecutive lines have a horizontal set of pixels
        var pairScore = Enumerable.Range(1, height - 1)
            .Chunk(3)
            .Select(pair =>
            {
                var a = LineScore(pair[0]);
                var b = LineScore(pair[1]);
                var c = LineScore(pair[2]);
                if (c > b && b > a && c > 5)
                    return (int?)pair[0];

                return null;
            })
            .Where(x => x is not null)
            .FirstOrDefault();

        if (pairScore is not null)
        {
            Console.WriteLine($"Step {step}");
            Print(robots, width, height, 63);
            throw new Exception($"Found at {step}"); // found at XXXX. manually retrying to check for off-by-one. answer was too low
        }
    }

    // max number of consecutive pixels in a line
    int LineScore(int y)
    {
        var score = 0;
        var maxScore = 0;
        for (var x = 0; x < width; x++)
        {
            if (robots.Any(r => r.Position.X == x && r.Position.Y == y))
            {
                score++;
                maxScore = Math.Max(score, maxScore);
            }
            else
            {
                score = 0;
            }
        }
        return maxScore;
    }
}

static void Part2Attempt3(IReadOnlyList<Robot> robots, int width, int height)
{
    Console.SetWindowSize(width + 1, 63); // seems to be max height on my machine
    Console.WriteLine("Enter starting step:");
    var step = int.Parse(Console.ReadLine()!);
    Step(robots, width, height, step);
    while (true)
    {
        Console.Clear();
        Console.WriteLine($"Step {step}");
        Print(robots, width, height, 60);
        Console.ReadLine();

        step++;
        Step(robots, width, height, 1);
    }
}

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
        robot.Position = (robot.Position + (robot.Velocity * steps)).Wrap(width, height);
}

static void Print(IReadOnlyList<Robot> robots, int width, int height, int? maxHeight = null)
{
    for (var y = 0; y < (maxHeight ?? height); y++)
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

    public Position Wrap(int width, int height) => new(Mod(X, width), Mod(Y, height));

    private static int Mod(int x, int m)
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

    public required Vector Velocity { get; init; }
}