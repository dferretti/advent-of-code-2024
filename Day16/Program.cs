using System.Collections.Immutable;

var chars = File.ReadAllLines("input.txt")
    .Select(line => line.ToCharArray().Select(Parse).ToArray())
    .ToArray();
var grid = new Grid(chars);

Console.WriteLine($"Part 1 score: {GetMinScore(grid)}");

static int GetMinScore(Grid grid)
{
    Dictionary<(Position, Direction), Path> cache = [];
    State root = new(grid.Start, Direction.Right, new());
    Stack<State> stack = new([root]); // move the stack to the heap lol

    var count = 0;
    while (stack.TryPop(out var state))
    {
        // if we've been here before, with a better score, bail
        if (cache.TryGetValue((state.Position, state.Direction), out var cached) && cached.Cost <= state.Path.Cost) continue;

        cache[(state.Position, state.Direction)] = state.Path;

        count++;
        if (count % 100_000 == 0)
            Console.WriteLine($"{count} - ({state.Position.R},{state.Position.C}) {state.Direction}");

        if (state.Position == grid.End)
        {
            state.SetScore(state.Path.Cost);
            continue;
        }

        if (grid[state.Position] == Tile.Wall) continue;

        var nextPos = state.Position + state.Direction;
        if (grid[nextPos] != Tile.Wall)
            stack.Push(state.Step(nextPos, state.Direction, new Move(state.Direction)));

        var d2 = Clockwise(state.Direction);
        stack.Push(state.Step(state.Position, d2, new Rotate(d2)));

        var d3 = CounterClockwise(state.Direction);
        stack.Push(state.Step(state.Position, d3, new Rotate(d3)));
    }

    return root.Score ?? throw new InvalidOperationException("No path found from S to E");

    // old recursive method that would stack overflow
    /*return GetMinScore(grid, grid.Start, Direction.Right, new(), []).Value;

    static int? GetMinScore(Grid grid, Position pos, Direction dir, Path path, Dictionary<(Position, Direction), Path> cache)
    {
        if (pos == grid.End) return path.Score;

        if (grid[pos] == Tile.Wall) return null;

        // if we've been here before, with a better score, bail
        if (cache.TryGetValue((pos, dir), out var cached) && cached.Score <= path.Score) return null;

        cache[(pos, dir)] = path;

        var next = new List<int?>();

        var nextPos = pos + dir;
        if (grid[nextPos] != Tile.Wall)
            next.Add(GetMinScore(grid, nextPos, dir, path.AddAction(new Move(dir)), cache));

        next.Add(GetMinScore(grid, pos, Clockwise(dir), path.AddAction(new Rotate(Clockwise(dir))), cache));
        next.Add(GetMinScore(grid, pos, CounterClockwise(dir), path.AddAction(new Rotate(CounterClockwise(dir))), cache));

        return next.Where(next => next.HasValue).Min();
    }*/
}

static Tile Parse(char c) => c switch
{
    'S' => Tile.Start,
    'E' => Tile.End,
    '.' => Tile.Empty,
    '#' => Tile.Wall,
    _ => throw new InvalidOperationException()
};

static Direction Clockwise(Direction direction) => direction switch
{
    Direction.Up => Direction.Right,
    Direction.Right => Direction.Down,
    Direction.Down => Direction.Left,
    Direction.Left => Direction.Up,
    _ => throw new InvalidOperationException()
};

static Direction CounterClockwise(Direction direction) => direction switch
{
    Direction.Up => Direction.Left,
    Direction.Left => Direction.Down,
    Direction.Down => Direction.Right,
    Direction.Right => Direction.Up,
    _ => throw new InvalidOperationException()
};

enum Tile { Start, Empty, Wall, End }
enum Direction { Up, Down, Left, Right }
record Position(int R, int C)
{
    public static Position operator +(Position a, Direction d) => d switch
    {
        Direction.Up => new Position(a.R - 1, a.C),
        Direction.Down => new Position(a.R + 1, a.C),
        Direction.Left => new Position(a.R, a.C - 1),
        Direction.Right => new Position(a.R, a.C + 1),
        _ => throw new InvalidOperationException()
    };
}
interface Action;
record Move(Direction Direction) : Action;
record Rotate(Direction NewDirection) : Action;
class Grid(Tile[][] chars)
{
    public Tile this[Position pos] => chars[pos.R][pos.C];

    public Position Start { get; } = chars
        .SelectMany((row, r) => row.Select((tile, c) => (Tile: tile, Position: new Position(r, c))))
        .Single(x => x.Tile is Tile.Start)
        .Position;

    public Position End { get; } = chars
        .SelectMany((row, r) => row.Select((tile, c) => (Tile: tile, Position: new Position(r, c))))
        .Single(x => x.Tile is Tile.End)
        .Position;
}

class Path
{
    public int Cost { get; }
    public ImmutableList<Action> Actions { get; }

    public Path() : this(0, ImmutableList<Action>.Empty) { }
    private Path(int cost, ImmutableList<Action> actions)
    {
        Cost = cost;
        Actions = actions;
    }
    public Path AddAction(Action action) => new(Cost + (action is Move ? 1 : 1000), Actions.Add(action));
}

record State(Position Position, Direction Direction, Path Path)
{
    public int? Score { get; private set; }
    public State? Parent { get; private set; }
    public State Step(Position pos, Direction dir, Action action)
        => new(pos, dir, Path.AddAction(action)) { Parent = this };
    public void SetScore(int score)
    {
        Score = Score is null ? score : Math.Min(Score.Value, score);
        Parent?.SetScore(score);
    }
}
