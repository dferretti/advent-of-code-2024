using System.Collections.Immutable;

var part1Sum = 0;
var part2Sum = 0;

foreach (var code in File.ReadAllLines("example.txt"))
{
    var path1 = FindShortestSequence(code, 2);

    // verify
    var state = Execute(code, 2, path1);
    if (code != state.LastRobotOutput)
        throw new InvalidOperationException($"Path does not produce expected output: {code} != {state.LastRobotOutput}");

    Console.WriteLine($"{code}: {string.Join("", path1.Select(DPadToChar))}: {ParseCode(code)} x {path1.Count}");
    part1Sum += ParseCode(code) * path1.Count;

    var path2 = FindShortestSequence(code, 25);
    state = Execute(code, 25, path2);
    if (code != state.LastRobotOutput)
        throw new InvalidOperationException($"Path does not produce expected output: {code} != {state.LastRobotOutput}");

    Console.WriteLine($"{code}: {string.Join("", path2.Select(DPadToChar))}: {ParseCode(code)} x {path2.Count}");
    part2Sum += ParseCode(code) * path2.Count;
}

Console.WriteLine($"Part 1: {part1Sum}");
Console.WriteLine($"Part 2: {part2Sum}");

static List<DPad> FindShortestSequence(string code, int numDPadRobots)
{
    var start = new State(numDPadRobots);
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

static int ParseCode(string code) => int.Parse(code[..^1]);

enum DPad { Up, Left, Down, Right, A, Fail }

enum NumPad { One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Zero, A, Fail }

sealed record State(int numDPadRobots)
{
    public ImmutableArray<DPad> Robots { get; init; } = Enumerable.Range(0, numDPadRobots).Select(_ => DPad.A).ToImmutableArray();
    public NumPad LastRobot { get; init; } = NumPad.A;
    public string LastRobotOutput { get; init; } = string.Empty;

    public bool IsValid(string targetCode)
    {
        if (Robots.Any(x => x is DPad.Fail) || LastRobot is NumPad.Fail)
            return false;

        return targetCode.StartsWith(LastRobotOutput);
    }

    public override int GetHashCode() => HashCode.Combine(LastRobot, LastRobotOutput, Robots.Length);

    public bool Equals(State? other) => other is not null
        && other.LastRobot == LastRobot
        && other.LastRobotOutput == LastRobotOutput
        && other.Robots.SequenceEqual(Robots);
}