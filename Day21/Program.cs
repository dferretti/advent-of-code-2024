using System.Collections.Immutable;

var part1Sum = 0;
var part2Sum = 0;

foreach (var code in File.ReadAllLines("example.txt"))
{
    if (code != "379A") continue;
    var part1 = Part1(code);

    // verify
    var state = Execute(code, 2, part1);
    if (code != state.LastRobotOutput)
        throw new InvalidOperationException($"Path does not produce expected output: {code} != {state.LastRobotOutput}");

    Console.WriteLine($"{code}: {ParseCode(code)} x {part1.Count}");
    Console.WriteLine(string.Join("", part1.Select(DPadToChar)));
    part1Sum += ParseCode(code) * part1.Count;

    var length1 = FindShortestSequenceLength(code, 2);

    // verify
    //if (part1.Count != length1)
      //  throw new InvalidOperationException($"New length does not match: {part1.Count} != {length1}");

    /*var path2 = FindShortestSequence(code, 2);
    state = Execute(code, 2, path2);
    if (code != state.LastRobotOutput)
        throw new InvalidOperationException($"Path does not produce expected output: {code} != {state.LastRobotOutput}");

    Console.WriteLine($"{code}: {ParseCode(code)} x {path2.Count}");
    part2Sum += ParseCode(code) * path2.Count;*/
}

Console.WriteLine($"Part 1: {part1Sum}");
Console.WriteLine($"Part 2: {part2Sum}");

/*var moves = "v<<A>>^AvA^Av<<A>>^AAv<A<A>>^AAvAA<^A>Av<A>^AA<A>Av<A<A>>^AAAvA<^A>A";
var s = Execute("379A", 2, moves.Select(CharToDPad));
Console.WriteLine(moves);
Console.WriteLine(moves.Length);
Console.WriteLine(s.LastRobotOutput);
Console.WriteLine("----");

moves = "<v<A>>^AvA^A<vA<AA>>^AAvA^<A>AAvA^A<vA>^AA<A>Av<<A>A>^AAAvA<^A>A";
s = Execute("379A", 2, moves.Select(CharToDPad));
Console.WriteLine(moves);
Console.WriteLine(moves.Length);
Console.WriteLine(s.LastRobotOutput);*/
Test("v<<A>>^AvA^Av<<A>>^AAv<A<A>>^AAvAA<^A>Av<A>^AA<A>Av<A<A>>^AAAvA<^A>A");
Test("<v<A>>^AvA^A<vA<AA>>^AAvA^<A>AAvA^A<vA>^AA<A>Av<<A>A>^AAAvA<^A>A");

static void Test(string moves)
{
    Console.WriteLine("---");
    var s = Execute("379A", 2, moves.Select(CharToDPad));
    Console.WriteLine(moves);
    Console.WriteLine(moves.Length);
    Console.WriteLine(s.LastRobotOutput);
}

static int FindShortestSequenceLength(string code, int numDPadRobots)
{
    var current = NumPad.A;
    var robot1Path = new List<DPad>();
    foreach (var c in code)
    {
        var next = CharToNumPad(c);
        var robot1 = ShortestNumPadPath(current, next);
        robot1Path.AddRange(robot1);
        robot1Path.Add(DPad.A);
        //Console.WriteLine($"{current} -> {next}: {string.Join("", robot1.Select(DPadToChar))}");

        current = next;
    }

    Console.WriteLine($"robotn-1 path: {string.Join("", robot1Path.Select(DPadToChar))}");

    var dCurrent = DPad.A;
    var robot2Path = new List<DPad>();
    foreach (var next in robot1Path)
    {
        var robot2 = ShortestDPadPath(dCurrent, next);
        robot2Path.AddRange(robot2);
        robot2Path.Add(DPad.A);
        //Console.WriteLine($"{dCurrent} -> {next}: {string.Join("", robot2.Select(DPadToChar))}");
        dCurrent = next;
    }

    Console.WriteLine($"robotn path: {string.Join("", robot2Path.Select(DPadToChar))}");

    dCurrent = DPad.A;
    var result = new List<DPad>();
    foreach (var next in robot2Path)
    {
        var robot2 = ShortestDPadPath(dCurrent, next);
        result.AddRange(robot2);
        result.Add(DPad.A);
        //Console.WriteLine($"{dCurrent} -> {next}: {string.Join("", robot2.Select(DPadToChar))}");
        dCurrent = next;
    }

    Console.WriteLine($"robotn+1 path: {string.Join("", result.Select(DPadToChar))}");

    return result.Count;
}

static List<DPad> Part1(string code)
{
    var start = new State(2);
    var openSet = new PriorityQueue<State, int>();
    openSet.Enqueue(start, 0);
    var cameFrom = new Dictionary<State, (State State, DPad Input)>();
    var gScore = new Dictionary<State, int> { [start] = 0 };

    while (openSet.TryDequeue(out var current, out _))
    {
        if (current.LastRobotOutput == code)
            return ReconstructPath(cameFrom, current);

        foreach (var input in Enum.GetValues<DPad>())
        {
            if (input is DPad.Fail)
                continue;

            var neighbor = Step(current, input);
            if (!neighbor.IsValid(code))
                continue;

            if (!gScore.TryGetValue(current, out var gScoreCurrent))
                continue;

            var tentativeGScore = gScoreCurrent + 1;
            if (tentativeGScore < gScore.GetValueOrDefault(neighbor, int.MaxValue))
            {
                cameFrom[neighbor] = (current, input);
                gScore[neighbor] = tentativeGScore;
                var fScore = tentativeGScore + Heuristic(neighbor, code);
                openSet.Remove(neighbor, out _, out _);
                openSet.Enqueue(neighbor, fScore);
            }
        }
    }

    throw new InvalidOperationException("No path found");

    static int Heuristic(State current, string targetCode) => targetCode.Length - current.LastRobotOutput.Length;

    static List<DPad> ReconstructPath(Dictionary<State, (State State, DPad Input)> cameFrom, State current)
    {
        var path = new List<DPad>();
        while (cameFrom.TryGetValue(current, out var x))
        {
            path.Insert(0, x.Input);
            current = x.State;
        }

        return path;
    }
}

static State Execute(string code, int numDPadRobots, IEnumerable<DPad> moves)
{
    var state = new State(numDPadRobots);

    foreach (var input in moves)
    {
        state = Step(state, input);

        if (!state.IsValid(code))
            throw new InvalidOperationException("User error - ended up in invalid state");
    }

    return state;
}

static State Step(State current, DPad input) => input switch
{
    DPad.Up or DPad.Left or DPad.Right or DPad.Down => current with { Robots = current.Robots.SetItem(0, MoveDPad(current.Robots[0], input)) },
    DPad.A => ActOnDPadRobot(current, 1),
    _ => throw new InvalidOperationException("User error"),
};

static State ActOnDPadRobot(State current, int index) => current.Robots[index - 1] switch
{
    DPad.Up or DPad.Left or DPad.Right or DPad.Down => current with { Robots = current.Robots.SetItem(index, MoveDPad(current.Robots[index], current.Robots[index - 1])) },
    DPad.A when index < current.Robots.Length - 1 => ActOnDPadRobot(current, index + 1),
    DPad.A => ActOnLastRobot(current),
    _ => throw new InvalidOperationException("User error"),
};

static State ActOnLastRobot(State current) => current.Robots[^1] switch
{
    DPad.Up or DPad.Left or DPad.Right or DPad.Down => current with { LastRobot = MoveNumPad(current.LastRobot, current.Robots[^1]) },
    DPad.A => current with { LastRobotOutput = current.LastRobotOutput + NumPadToChar(current.LastRobot) },
    _ => throw new InvalidOperationException("User error"),
};

static DPad MoveDPad(DPad start, DPad input) => (start, input) switch
{
    (DPad.Up, DPad.Right) => DPad.A,
    (DPad.Up, DPad.Down) => DPad.Down,
    (DPad.A, DPad.Left) => DPad.Up,
    (DPad.A, DPad.Down) => DPad.Right,
    (DPad.Left, DPad.Right) => DPad.Down,
    (DPad.Down, DPad.Left) => DPad.Left,
    (DPad.Down, DPad.Up) => DPad.Up,
    (DPad.Down, DPad.Right) => DPad.Right,
    (DPad.Right, DPad.Left) => DPad.Down,
    (DPad.Right, DPad.Up) => DPad.A,
    _ => DPad.Fail
};

static NumPad MoveNumPad(NumPad start, DPad input) => (start, input) switch
{
    (NumPad.Seven, DPad.Right) => NumPad.Eight,
    (NumPad.Seven, DPad.Down) => NumPad.Four,
    (NumPad.Eight, DPad.Left) => NumPad.Seven,
    (NumPad.Eight, DPad.Right) => NumPad.Nine,
    (NumPad.Eight, DPad.Down) => NumPad.Five,
    (NumPad.Nine, DPad.Left) => NumPad.Eight,
    (NumPad.Nine, DPad.Down) => NumPad.Six,

    (NumPad.Four, DPad.Up) => NumPad.Seven,
    (NumPad.Four, DPad.Right) => NumPad.Five,
    (NumPad.Four, DPad.Down) => NumPad.One,
    (NumPad.Five, DPad.Left) => NumPad.Four,
    (NumPad.Five, DPad.Up) => NumPad.Eight,
    (NumPad.Five, DPad.Right) => NumPad.Six,
    (NumPad.Five, DPad.Down) => NumPad.Two,
    (NumPad.Six, DPad.Up) => NumPad.Nine,
    (NumPad.Six, DPad.Left) => NumPad.Five,
    (NumPad.Six, DPad.Down) => NumPad.Three,

    (NumPad.One, DPad.Up) => NumPad.Four,
    (NumPad.One, DPad.Right) => NumPad.Two,
    (NumPad.Two, DPad.Left) => NumPad.One,
    (NumPad.Two, DPad.Right) => NumPad.Three,
    (NumPad.Two, DPad.Down) => NumPad.Zero,
    (NumPad.Two, DPad.Up) => NumPad.Five,
    (NumPad.Three, DPad.Left) => NumPad.Two,
    (NumPad.Three, DPad.Up) => NumPad.Six,
    (NumPad.Three, DPad.Down) => NumPad.A,

    (NumPad.Zero, DPad.Up) => NumPad.Two,
    (NumPad.Zero, DPad.Right) => NumPad.A,
    (NumPad.A, DPad.Left) => NumPad.Zero,
    (NumPad.A, DPad.Up) => NumPad.Three,

    _ => NumPad.Fail,
};

static char NumPadToChar(NumPad num) => num switch
{
    NumPad.One => '1',
    NumPad.Two => '2',
    NumPad.Three => '3',
    NumPad.Four => '4',
    NumPad.Five => '5',
    NumPad.Six => '6',
    NumPad.Seven => '7',
    NumPad.Eight => '8',
    NumPad.Nine => '9',
    NumPad.Zero => '0',
    NumPad.A => 'A',
    _ => throw new InvalidOperationException("User error"),
};

static char DPadToChar(DPad dpad) => dpad switch
{
    DPad.Up => '^',
    DPad.Left => '<',
    DPad.Down => 'v',
    DPad.Right => '>',
    DPad.A => 'A',
    _ => throw new InvalidOperationException("User error"),
};

static DPad CharToDPad(char c) => c switch
{
    '^' => DPad.Up,
    '<' => DPad.Left,
    'v' => DPad.Down,
    '>' => DPad.Right,
    'A' => DPad.A,
    _ => throw new InvalidOperationException("User error"),
};

static NumPad CharToNumPad(char c) => c switch
{
    '1' => NumPad.One,
    '2' => NumPad.Two,
    '3' => NumPad.Three,
    '4' => NumPad.Four,
    '5' => NumPad.Five,
    '6' => NumPad.Six,
    '7' => NumPad.Seven,
    '8' => NumPad.Eight,
    '9' => NumPad.Nine,
    '0' => NumPad.Zero,
    'A' => NumPad.A,
    _ => throw new InvalidOperationException("User error"),
};

static int ParseCode(string code) => int.Parse(code[..^1]);

static DPad[] ShortestNumPadPath(NumPad start, NumPad end)
{
    var (startR, startC) = GetPos(start);
    var (endR, endC) = GetPos(end);
    return Enumerable.Empty<DPad>()
        .Concat(Enumerable.Repeat(DPad.Up, Math.Clamp(startR - endR, 0, 3)))
        .Concat(Enumerable.Repeat(DPad.Right, Math.Clamp(endC - startC, 0, 2)))
        .Concat(Enumerable.Repeat(DPad.Left, Math.Clamp(startC - endC, 0, 2)))
        .Concat(Enumerable.Repeat(DPad.Down, Math.Clamp(endR - startR, 0, 3)))
        .ToArray();

    static (int R, int C) GetPos(NumPad n) => n switch
    {
        NumPad.Seven => (0, 0),
        NumPad.Eight => (0, 1),
        NumPad.Nine => (0, 2),
        NumPad.Four => (1, 0),
        NumPad.Five => (1, 1),
        NumPad.Six => (1, 2),
        NumPad.One => (2, 0),
        NumPad.Two => (2, 1),
        NumPad.Three => (2, 2),
        NumPad.Zero => (3, 1),
        NumPad.A => (3, 2),
        _ => throw new InvalidOperationException($"Unexpected numpad: {n}"),
    };
}

static DPad[] ShortestDPadPath(DPad start, DPad end)
{
    var (startR, startC) = GetPos(start);
    var (endR, endC) = GetPos(end);
    return Enumerable.Empty<DPad>()
        .Concat(Enumerable.Repeat(DPad.Down, Math.Clamp(endR - startR, 0, 1)))
        .Concat(Enumerable.Repeat(DPad.Left, Math.Clamp(startC - endC, 0, 2)))
        .Concat(Enumerable.Repeat(DPad.Right, Math.Clamp(endC - startC, 0, 2)))
        .Concat(Enumerable.Repeat(DPad.Up, Math.Clamp(startR - endR, 0, 1)))
        .ToArray();

    static (int R, int C) GetPos(DPad d) => d switch
    {
        DPad.Up => (0, 1),
        DPad.A => (0, 2),
        DPad.Left => (1, 0),
        DPad.Down => (1, 1),
        DPad.Right => (1, 2),
        _ => throw new InvalidOperationException($"Unexpected dpad: {d}"),
    };
}

enum DPad { Up, Left, Down, Right, A, Fail }

enum NumPad { One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Zero, A, Fail }

sealed record State(int numDPadRobots)
{
    public ImmutableArray<DPad> Robots { get; init; } = Enumerable.Repeat(DPad.A, numDPadRobots).ToImmutableArray();
    public NumPad LastRobot { get; init; } = NumPad.A;
    public string LastRobotOutput { get; init; } = string.Empty;

    public bool IsValid(string targetCode)
    {
        if (Robots.Any(x => x is DPad.Fail) || LastRobot is NumPad.Fail)
            return false;

        return targetCode == string.Empty || targetCode.StartsWith(LastRobotOutput);
    }

    public override int GetHashCode() => HashCode.Combine(LastRobot, LastRobotOutput, Robots.Length);

    public bool Equals(State? other) => other is not null
        && other.LastRobot == LastRobot
        && other.LastRobotOutput == LastRobotOutput
        && other.Robots.SequenceEqual(Robots);
}

record State2(int numDPadRobots)
{
    public ImmutableArray<DPadRobot> Robots { get; init; } = Enumerable.Repeat(new DPadRobot(), numDPadRobots).ToImmutableArray();
    public NumPadRobot LastRobot { get; init; } = new NumPadRobot();

    public State2 Step(DPad input) => input switch
    {
        DPad.Up or DPad.Left or DPad.Right or DPad.Down
            => this with { Robots = Robots.SetItem(0, Robots[0]
                with { Current = MoveDPad(Robots[0].Current, input) }) },
        DPad.A => ActOnDPadRobot(1) with { Robots = Robots.SetItem(0, Robots[0]
                with { Outputs = Robots[0].Outputs.Add(Robots[0].Current) }) },
        _ => throw new InvalidOperationException("User error"),
    };

    private State2 ActOnDPadRobot(int index) => Robots[index - 1].Current switch
    {
        DPad.Up or DPad.Left or DPad.Right or DPad.Down
            => this with
            {
                Robots = Robots.SetItem(index, Robots[index] with
                {
                    Current = MoveDPad(Robots[index].Current, Robots[index - 1].Current),
                })
            },
        DPad.A when index < Robots.Length - 1 => ActOnDPadRobot(index + 1) with
        {
            Robots = Robots.SetItem(index, Robots[index] with
            {
                Outputs = Robots[index].Outputs.Add(Robots[index].Current),
            }),
        },
        DPad.A => ActOnLastRobot(),
        _ => throw new InvalidOperationException("User error"),
    };

    private State2 ActOnLastRobot() => Robots[^1].Current switch
    {
        DPad.Up or DPad.Left or DPad.Right or DPad.Down
            => this with
            {
                LastRobot = LastRobot with
                {
                    Current = MoveNumPad(LastRobot.Current, Robots[^1].Current),
                    Outputs = LastRobot.Outputs.Add(LastRobot.Current),
                }
            },
        DPad.A => this with { LastRobot = LastRobot with { Outputs = LastRobot.Outputs.Add(LastRobot.Current) } },
        _ => throw new InvalidOperationException("User error"),
    };

    static DPad MoveDPad(DPad start, DPad input) => (start, input) switch
    {
        (DPad.Up, DPad.Right) => DPad.A,
        (DPad.Up, DPad.Down) => DPad.Down,
        (DPad.A, DPad.Left) => DPad.Up,
        (DPad.A, DPad.Down) => DPad.Right,
        (DPad.Left, DPad.Right) => DPad.Down,
        (DPad.Down, DPad.Left) => DPad.Left,
        (DPad.Down, DPad.Up) => DPad.Up,
        (DPad.Down, DPad.Right) => DPad.Right,
        (DPad.Right, DPad.Left) => DPad.Down,
        (DPad.Right, DPad.Up) => DPad.A,
        _ => DPad.Fail
    };

    static NumPad MoveNumPad(NumPad start, DPad input) => (start, input) switch
    {
        (NumPad.Seven, DPad.Right) => NumPad.Eight,
        (NumPad.Seven, DPad.Down) => NumPad.Four,
        (NumPad.Eight, DPad.Left) => NumPad.Seven,
        (NumPad.Eight, DPad.Right) => NumPad.Nine,
        (NumPad.Eight, DPad.Down) => NumPad.Five,
        (NumPad.Nine, DPad.Left) => NumPad.Eight,
        (NumPad.Nine, DPad.Down) => NumPad.Six,

        (NumPad.Four, DPad.Up) => NumPad.Seven,
        (NumPad.Four, DPad.Right) => NumPad.Five,
        (NumPad.Four, DPad.Down) => NumPad.One,
        (NumPad.Five, DPad.Left) => NumPad.Four,
        (NumPad.Five, DPad.Up) => NumPad.Eight,
        (NumPad.Five, DPad.Right) => NumPad.Six,
        (NumPad.Five, DPad.Down) => NumPad.Two,
        (NumPad.Six, DPad.Up) => NumPad.Nine,
        (NumPad.Six, DPad.Left) => NumPad.Five,
        (NumPad.Six, DPad.Down) => NumPad.Three,

        (NumPad.One, DPad.Up) => NumPad.Four,
        (NumPad.One, DPad.Right) => NumPad.Two,
        (NumPad.Two, DPad.Left) => NumPad.One,
        (NumPad.Two, DPad.Right) => NumPad.Three,
        (NumPad.Two, DPad.Down) => NumPad.Zero,
        (NumPad.Two, DPad.Up) => NumPad.Five,
        (NumPad.Three, DPad.Left) => NumPad.Two,
        (NumPad.Three, DPad.Up) => NumPad.Six,
        (NumPad.Three, DPad.Down) => NumPad.A,

        (NumPad.Zero, DPad.Up) => NumPad.Two,
        (NumPad.Zero, DPad.Right) => NumPad.A,
        (NumPad.A, DPad.Left) => NumPad.Zero,
        (NumPad.A, DPad.Up) => NumPad.Three,

        _ => NumPad.Fail,
    };
}

record DPadRobot
{
    public DPad Current { get; init; } = DPad.A;
    public ImmutableList<DPad> Outputs { get; init; } = [];
}

record NumPadRobot
{
    public NumPad Current { get; init; } = NumPad.A;
    public ImmutableList<NumPad> Outputs { get; init; } = [];
}