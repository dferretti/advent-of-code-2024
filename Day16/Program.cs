using System.Collections.Immutable;

var chars = File.ReadAllLines("input.txt")
    .Select(line => line.ToCharArray().Select(Parse).ToArray())
    .ToArray();
var grid = new Grid(chars);

//BruteForce(grid);
Dijkstra(grid);

static void Dijkstra(Grid grid)
{
    var pq = new PriorityQueue<Node, int>();

    var source = new Node(grid.Start, Direction.Right);
    var dist = new Dictionary<Node, int> { [source] = 0 };
    var prev = new Dictionary<Node, List<Node>>();
    var ends = new List<Node>();
    pq.Enqueue(source, 0);

    for (var r = 0; r < grid.Rows; r++)
    {
        for (var c = 0; c < grid.Cols; c++)
        {
            var pos = new Position(r, c);
            if (grid[pos] == Tile.Wall) continue;

            foreach (var dir in Enum.GetValues<Direction>())
            {
                var node = new Node(pos, dir);

                if (node == source) continue;

                dist[node] = int.MaxValue;
                pq.Enqueue(node, int.MaxValue);

                if (grid[pos] == Tile.End)
                    ends.Add(node);
            }
        }
    }

    while (pq.TryDequeue(out var node, out var priority))
    {
        foreach (var n in GetNeighbors(grid, node))
        {
            var alt = dist[node] + (n.Direction == node.Direction ? 1 : 1000);
            if (alt <= dist[n])
            {
                dist[n] = alt;
                if (!prev.TryGetValue(n, out var prevs))
                    prev[n] = prevs = [];
                prevs.Add(node);

                // https://github.com/dotnet/runtime/issues/44871#issuecomment-1868915208 - pq.Update(n, alt);
                pq.Remove(n, out _, out _);
                pq.Enqueue(n, alt);
            }
        }
    }

    var minScore = ends.Min(end => dist[end]);
    Console.WriteLine($"Part 1 score: {minScore}");
    var positions = new HashSet<Position>();
    foreach (var end in ends)
    {
        if (dist[end] > minScore) continue;

        Backtrace(prev, end, positions);

        static void Backtrace(Dictionary<Node, List<Node>> prev, Node node, HashSet<Position> positions)
        {
            positions.Add(node.Position);

            if (!prev.TryGetValue(node, out var prevs)) return;
            foreach (var p in prevs)
                Backtrace(prev, p, positions);
        }
    }
    Console.WriteLine($"Part 2 score: {positions.Count}");

    static IEnumerable<Node> GetNeighbors(Grid grid, Node node)
    {
        var nextPos = node.Position + node.Direction;
        if (grid[nextPos] != Tile.Wall)
            yield return new Node(nextPos, node.Direction);
        yield return new Node(node.Position, Clockwise(node.Direction));
        yield return new Node(node.Position, CounterClockwise(node.Direction));
    }
}

static void BruteForce(Grid grid)
{
    var result = GetMinScore(grid);
    Console.WriteLine($"Part 1 score: {result.Score} - {result.FinalPath.Actions.OfType<Rotate>().Count()} turns");
    var part2 = GetBestPositions(grid, result.Score);
    Console.WriteLine($"Part 2: {part2.Count}");

    static (int Score, Path FinalPath) GetMinScore(Grid grid)
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
                stack.Push(state.Step(nextPos, state.Direction, new Move(state.Direction, nextPos)));

            var d2 = Clockwise(state.Direction);
            stack.Push(state.Step(state.Position, d2, new Rotate(d2)));

            var d3 = CounterClockwise(state.Direction);
            stack.Push(state.Step(state.Position, d3, new Rotate(d3)));
        }

        return (root.Score ?? throw new InvalidOperationException("No path found from S to E"), root.FinalPath!);

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

    static HashSet<Position> GetBestPositions(Grid grid, int targetScore)
    {
        HashSet<Position> results = [grid.Start];
        CollectPositions(targetScore, grid, grid.Start, Direction.Right, new(), [], results);
        return results;

        static void CollectPositions(int targetScore, Grid grid, Position pos, Direction dir, Path path, Dictionary<(Position, Direction), Path> cache, HashSet<Position> best)
        {
            if (pos == grid.End && path.Cost == targetScore)
            {
                best.UnionWith(path.Positions);
                return;
            }

            if (grid[pos] == Tile.Wall) return;

            if (path.Cost > targetScore) return;

            // if we've been here before, with a better score, bail
            if (cache.TryGetValue((pos, dir), out var cached) && cached.Cost < path.Cost) return;

            cache[(pos, dir)] = path;
            CollectPositions(targetScore, grid, pos + dir, dir, path.AddAction(new Move(dir, pos + dir)), cache, best);
            CollectPositions(targetScore, grid, pos, Clockwise(dir), path.AddAction(new Rotate(Clockwise(dir))), cache, best);
            CollectPositions(targetScore, grid, pos, CounterClockwise(dir), path.AddAction(new Rotate(CounterClockwise(dir))), cache, best);
        }
    }
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
record Move(Direction Direction, Position NewPosition) : Action;
record Rotate(Direction NewDirection) : Action;
class Grid(Tile[][] chars)
{
    public int Rows => chars.Length;
    public int Cols => chars[0].Length;
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

    public IEnumerable<Position> Positions => Actions.OfType<Move>().Select(m => m.NewPosition);
}

record State(Position Position, Direction Direction, Path Path)
{
    public int? Score { get; private set; }
    public Path? FinalPath { get; private set; }
    public State? Parent { get; private set; }
    public State Step(Position pos, Direction dir, Action action)
        => new(pos, dir, Path.AddAction(action)) { Parent = this };
    public void SetScore(int score)
    {
        //Score = Score is null ? score : Math.Min(Score.Value, score);
        //Parent?.SetScore(score);
        Score = score;
        FinalPath = Path;
        Parent?.SetScore(this);
    }

    private void SetScore(State child)
    {
        if (Score is null || child.Score < Score)
        {
            Score = child.Score;
            FinalPath = child.FinalPath;
            Parent?.SetScore(this);
        }
    }
}

record Node(Position Position, Direction Direction);
