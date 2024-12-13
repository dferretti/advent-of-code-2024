Part1();
Part2();

static void Part1()
{
    var part1Sum = 0L;

    foreach (var machine in File.ReadAllLines("input.txt").Chunk(4))
    {
        var a = Parse(machine[0], '+');
        var b = Parse(machine[1], '+');
        var prize = Parse(machine[2], '=');

        var cheap = (Button: b, Cost: 1);
        var expensive = (Button: a, Cost: 3);
        if (D2(a) > D2(b) * 3)
            (cheap, expensive) = (expensive, cheap);

        long? cost = null;
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

            // Console.WriteLine($"{cheapCount} {expensiveCount.X}");
            cost = cheap.Cost * cheapCount + expensive.Cost * expensiveCount.X;
        }

        if (cost is not null) part1Sum += cost.Value;
    }

    Console.WriteLine($"Part 1 sum: {part1Sum}");

    static long D2(Tuple t) => t.X * t.X + t.Y * t.Y;
}

static void Part2()
{
    var part2Sum = 0L;

    foreach (var machine in File.ReadAllLines("input.txt").Chunk(4))
    {
        var a = Parse(machine[0], '+');
        var b = Parse(machine[1], '+');
        var prize = Parse(machine[2], '=') + new Tuple(10000000000000L, 10000000000000L);

        var c1 = TrySolve(a, 3, b, 1, prize);
        var c2 = TrySolve(b, 1, a, 3, prize);
        if (c1 is not null && c2 is not null)
        {
            var cost = Math.Min(c1.Value, c2.Value);
            part2Sum += cost;
        }
        else if (c1 is not null)
        {
            part2Sum += c1.Value;
        }
        else if (c2 is not null)
        {
            part2Sum += c2.Value;
        }
    }

    Console.WriteLine($"Part 2 sum: {part2Sum}");

    static long? TrySolve(Tuple a, long aCost, Tuple b, long bCost, Tuple prize)
    {
        // line 1 - from origin: y = M1x; M1 = a.Y / a.B
        // line 2 - to prize: (y - prize.Y) = M2(x - prize.X); M2 = b.Y / b.X
        // line 2 - to prize: y = M2x + (prize.Y - M2 * prize.X); M2 = b.Y / b.X; y=M2x+d; d=(prize.Y - M2 * prize.X)
        // intersection x: (d-0) / (M1 - M2)
        // check if exact int
        var m1 = (double)a.Y / a.X;
        var m2 = (double)b.Y / b.X;
        var d = prize.Y - m2 * prize.X;
        var x = d / (m1 - m2);
        var aCount = (long)(x / a.X);

        // shh, accounting for floating point errors - try a few points around aCount in case our division was not exact
        return Enumerable.Range(-10, 20).Select(i => TrySolve(a, aCost, b, bCost, prize, aCount + i)).FirstOrDefault(x => x is not null);

        static long? TrySolve(Tuple a, long aCost, Tuple b, long bCost, Tuple prize, long aCount)
        {
            var remaining = prize - a * aCount;
            var (bCount, remainder) = Tuple.DivRem(remaining, b);
            if (remainder == Tuple.Zero && bCount.X == bCount.Y)
            {
                // check we have exact match
                if (a * aCount + b * bCount.X == prize)
                    return (aCount * aCost) + (bCount.X * bCost);
            }

            return null;
        }
    }
}

static Tuple Parse(string line, char c)
{
    var parts = line.Split(',');
    var x = parts[0].AsSpan();
    var y = parts[1].AsSpan();
    return new(long.Parse(x[(1 + x.IndexOf(c))..]), long.Parse(y[(1 + y.IndexOf(c))..]));
}

record Tuple(long X, long Y)
{
    public static Tuple operator -(Tuple a, Tuple b) => new(a.X - b.X, a.Y - b.Y);

    public static Tuple operator *(Tuple a, long b) => new(a.X * b, a.Y * b);

    public static Tuple operator +(Tuple a, Tuple b) => new(a.X + b.X, a.Y + b.Y);

    public static (Tuple Quotient, Tuple Remainder) DivRem(Tuple a, Tuple b)
    {
        var (x, xr) = Math.DivRem(a.X, b.X);
        var (y, yr) = Math.DivRem(a.Y, b.Y);
        return (new(x, y), new(xr, yr));
    }

    public static Tuple Zero { get; } = new(0, 0);
}