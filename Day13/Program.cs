var part1Sum = 0;

foreach (var machine in File.ReadAllLines("input.txt").Chunk(4))
{
    var a = Parse(machine[0], '+');
    var b = Parse(machine[1], '+');
    var prize = Parse(machine[2], '=');

    var cheap = (Button: b, Cost: 1);
    var expensive = (Button: a, Cost: 3);
    if (D2(a) > D2(b) * 3)
        (cheap, expensive) = (expensive, cheap);

    int? cost = null;
    for (var cheapCount = 100; cheapCount >= 0; cheapCount--)
    {
        var remaining = prize - cheap.Button * cheapCount;
        if (remaining.X < 0 || remaining.Y < 0)
            continue;

        var (expensiveCount, remainder) = Tuple.DivRem(remaining, expensive.Button);
        if (remainder != Tuple.Zero)
            continue;

        if (expensiveCount.X != expensiveCount.Y)
            continue;

        Console.WriteLine($"{cheapCount} {expensiveCount.X}");
        cost = cheap.Cost * cheapCount + expensive.Cost * expensiveCount.X;
    }

    if (cost is not null) part1Sum += cost.Value;
}

Console.WriteLine($"Part 1 sum: {part1Sum}");

static int D2(Tuple t) => t.X * t.X + t.Y * t.Y;

static Tuple Parse(string line, char c)
{
    var parts = line.Split(',');
    var x = parts[0].AsSpan();
    var y = parts[1].AsSpan();
    return new(int.Parse(x[(1 + x.IndexOf(c))..]), int.Parse(y[(1 + y.IndexOf(c))..]));
}

record Tuple(int X, int Y)
{
    public static Tuple operator -(Tuple a, Tuple b) => new(a.X - b.X, a.Y - b.Y);

    public static Tuple operator *(Tuple a, int b) => new(a.X * b, a.Y * b);

    public static (Tuple Quotient, Tuple Remainder) DivRem(Tuple a, Tuple b)
    {
        var (x, xr) = Math.DivRem(a.X, b.X);
        var (y, yr) = Math.DivRem(a.Y, b.Y);
        return (new(x, y), new(xr, yr));
    }

    public static Tuple Zero { get; } = new(0, 0);
}