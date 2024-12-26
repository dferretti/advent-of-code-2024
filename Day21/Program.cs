using System.Collections.Immutable;

var code = "029A";
var moves = "<vA<AA>>^AvAA<^A>A<v<A>>^AvA^A<vA>^A<v<A>^A>AAvA^A<v<A>A>^AAAvA<^A>A";
var state = Execute(moves);
var result = new string(state.LastRobotOutput.Select(NumPadToChar).ToArray());
Console.WriteLine($"{result}: {ParseCode(result)} x {moves.Length}");

static State Execute(ReadOnlySpan<char> moves)
{
    var state = new State { Robot1 = DPad.A, Robot2 = DPad.A, LastRobot = NumPad.A };

    foreach (var c in moves)
    {
        var input = c switch
        {
            '^' => DPad.Up,
            '<' => DPad.Left,
            'v' => DPad.Down,
            '>' => DPad.Right,
            'A' => DPad.A,
            _ => throw new InvalidOperationException("User error"),
        };

        state = Step(state, input);

        if (state.IsFail)
            throw new InvalidOperationException("User error - ended up in invalid state");
    }

    return state;
}

static State Step(State current, DPad input) => input switch
{
    DPad.Up or DPad.Left or DPad.Right or DPad.Down => current with { Robot1 = MoveDPad(current.Robot1, input) },
    DPad.A => ActOnRobot1(current),
    _ => throw new InvalidOperationException("User error"),
};

static State ActOnRobot1(State current) => current.Robot1 switch
{
    DPad.Up or DPad.Left or DPad.Right or DPad.Down => current with { Robot2 = MoveDPad(current.Robot2, current.Robot1) },
    DPad.A => ActOnRobot2(current),
    _ => throw new InvalidOperationException("User error"),
};

static State ActOnRobot2(State current) => current.Robot2 switch
{
    DPad.Up or DPad.Left or DPad.Right or DPad.Down => current with { LastRobot = MoveNumPad(current.LastRobot, current.Robot2) },
    DPad.A => current with { LastRobotOutput = current.LastRobotOutput.Add(current.LastRobot) },
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

static int ParseCode(string code) => int.Parse(code[..^1]);

enum DPad { Up, Left, Down, Right, A, Fail }

enum NumPad { One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Zero, A, Fail }

record State
{
    public DPad Robot1 { get; init; }
    public DPad Robot2 { get; init; }
    public NumPad LastRobot { get; init; }
    public ImmutableArray<NumPad> LastRobotOutput { get; init; } = [];

    public bool IsFail => Robot1 is DPad.Fail || Robot2 is DPad.Fail || LastRobot is NumPad.Fail;
}