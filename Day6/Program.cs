using System.Collections.Immutable;

var chars = File.ReadAllLines("input.txt").Select(line => line.ToCharArray()).ToArray();
var grid = new Grid(chars);

var part1Positions = new HashSet<Position> { grid.StartPos };
var pos = grid.StartPos;
var dir = grid.StartDir;
while (true)
{
    (pos, dir, var action) = Step(pos, dir, grid);
    if (action == Action.Exit)
        break;

    if (action == Action.Move)
        part1Positions.Add(pos);
}

Console.WriteLine(grid.Display());
Console.WriteLine($"Part 1 count: {part1Positions.Count}");

var part2Count = 0;
for (int row = 0; row < grid.Rows; row++)
{
    for (int col = 0; col < grid.Cols; col++)
    {
        var newObstacle = new Position(row, col);
        if (WouldCauseLoop(newObstacle, grid))
        {
            Console.WriteLine($"Would cause loop: {newObstacle}");
            part2Count++;
        }
    }
}

Console.WriteLine($"Part 2 count: {part2Count}");

static bool WouldCauseLoop(Position newObstacle, Grid original)
{
    if (original[newObstacle] == Tile.Obstacle)
        return false; // if it is alreay an obstacle

    if (newObstacle == original.StartPos)
        return false; // can't place new obstacle on start position

    var grid = original.WithNewObstacle(newObstacle);

    var history = new HashSet<(Position, Direction)> { (grid.StartPos, grid.StartDir) };
    var pos = grid.StartPos;
    var dir = grid.StartDir;
    while (true)
    {
        (pos, dir, var action) = Step(pos, dir, grid);
        if (action == Action.Exit)
            return false;

        // if we have been in this exact position and direction before, we are in a loop
        if (action == Action.Move && !history.Add((pos, dir)))
            return true;
    }
}

static (Position pos, Direction dir, Action action) Step(Position pos, Direction dir, Grid grid)
{
    var nextPos = NextPosition(pos, dir);
    if (!grid.IsInbounds(nextPos))
        return (default, default, Action.Exit);

    var tile = grid[nextPos];
    if (tile == Tile.Obstacle)
        return (pos, TurnRight(dir), Action.Turn);

    return (nextPos, dir, Action.Move);
}

static Position NextPosition(Position pos, Direction dir) => dir switch
{
    Direction.Up => new Position(pos.Row - 1, pos.Col),
    Direction.Right => new Position(pos.Row, pos.Col + 1),
    Direction.Down => new Position(pos.Row + 1, pos.Col),
    Direction.Left => new Position(pos.Row, pos.Col - 1),
    _ => throw new InvalidOperationException(),
};

static Direction TurnRight(Direction dir) => dir switch
{
    Direction.Up => Direction.Right,
    Direction.Right => Direction.Down,
    Direction.Down => Direction.Left,
    Direction.Left => Direction.Up,
    _ => throw new InvalidOperationException(),
};

enum Tile { Empty, Obstacle }

enum Direction { Up, Right, Down, Left }

enum Action { Move, Turn, Exit }

record struct Position(int Row, int Col);

class Grid
{
    private readonly ImmutableArray<ImmutableArray<Tile>> _tiles;

    public Position StartPos { get; }

    public Direction StartDir { get; }

    public int Rows => _tiles.Length;

    public int Cols => _tiles[0].Length;

    public Grid(char[][] chars)
    {
        _tiles = chars
            .Select(row => row.Select(c => c switch { '#' => Tile.Obstacle, _ => Tile.Empty }).ToImmutableArray())
            .ToImmutableArray();

        StartPos = chars
            .Index()
            .SelectMany(row => row.Item.Index().Select(col => (Pos: new Position(row.Index, col.Index), Char: col.Item)))
            .Single(x => x.Char is '^' or '>' or 'v' or '<')
            .Pos;

        StartDir = chars[StartPos.Row][StartPos.Col] switch
        {
            '>' => Direction.Right,
            '<' => Direction.Left,
            'v' => Direction.Down,
            '^' => Direction.Up,
            _ => throw new InvalidOperationException(),
        };
    }

    private Grid(ImmutableArray<ImmutableArray<Tile>> tiles, Position startPos, Direction startDir)
    {
        _tiles = tiles;
        StartPos = startPos;
        StartDir = startDir;
    }

    public bool IsInbounds(Position pos) => pos.Row >= 0 && pos.Row < Rows && pos.Col >= 0 && pos.Col < Cols;

    public Tile this[Position pos] => _tiles[pos.Row][pos.Col];

    public string Display()
        => string.Join('\n', _tiles.Select(row => new string(row.Select(tile => tile switch { Tile.Obstacle => '#', _ => '.' }).ToArray())));

    public Grid WithNewObstacle(Position pos)
        => new(
            _tiles.SetItem(pos.Row, _tiles[pos.Row].SetItem(pos.Col, Tile.Obstacle)),
            StartPos,
            StartDir);
}