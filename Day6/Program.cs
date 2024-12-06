var chars = File.ReadAllLines("example.txt").Select(line => line.ToCharArray()).ToArray();
var grid = new Grid(chars);

var part1Positions = new HashSet<Position> { grid.StartPos };
var guardPos = grid.StartPos;
var guardDir = grid.StartDir;
while (true)
{
    (guardPos, guardDir, var action) = Step(guardPos, guardDir, grid);
    if (action == Action.Exit)
        break;

    if (action == Action.Move)
        part1Positions.Add(guardPos);
}

Console.WriteLine(grid.Display());

var guardPositions = part1Positions.Count;
Console.WriteLine($"Part 1 count: {guardPositions}");

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
    private readonly Tile[][] _tiles;

    public Position StartPos { get; }

    public Direction StartDir { get; }

    public int Rows => _tiles.Length;

    public int Cols => _tiles[0].Length;

    public Grid(char[][] chars)
    {
        _tiles = chars
            .Select(row => row.Select(c => c switch { '#' => Tile.Obstacle, _ => Tile.Empty }).ToArray())
            .ToArray();

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

    public bool IsInbounds(Position pos) => pos.Row >= 0 && pos.Row < Rows && pos.Col >= 0 && pos.Col < Cols;

    public Tile this[Position pos] => _tiles[pos.Row][pos.Col];

    public string Display()
        => string.Join('\n', _tiles.Select(row => new string(row.Select(tile => tile switch { Tile.Obstacle => '#', _ => '.' }).ToArray())));
}