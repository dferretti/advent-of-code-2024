var chars = File.ReadAllLines("input.txt").Select(line => line.ToCharArray()).ToArray();
var grid = chars
    .Select(line => line.Select(c => c switch { '#' => Tile.Obstacle, '>' or '<' or 'v' or '^' => Tile.Guard, _ => Tile.Empty }).ToArray())
    .ToArray();

var rows = grid.Length;
var cols = grid[0].Length;

var guardPos = grid
    .Index()
    .SelectMany(row => row.Item.Index().Select(col => (Pos: new Position(row.Index, col.Index), Tile: col.Item)))
    .Single(x => x.Tile is Tile.Guard)
    .Pos;

var guardDir = chars[guardPos.Row][guardPos.Col] switch
{
    '>' => Direction.Right,
    '<' => Direction.Left,
    'v' => Direction.Down,
    '^' => Direction.Up,
    _ => throw new InvalidOperationException(),
};

while (true)
{
    (guardPos, guardDir, var action) = Step(guardPos, guardDir);
    if (action == Action.Exit)
        break;
}

Console.WriteLine(Display(grid));

var guardPositions = grid.Sum(row => row.Count(tile => tile is Tile.Guard));
Console.WriteLine($"Count: {guardPositions}");

(Position pos, Direction dir, Action action) Step(Position pos, Direction dir)
{
    var nextPos = NextPosition(pos, dir);
    if (!IsInbounds(nextPos))
        return (default, default, Action.Exit);

    var tile = grid[nextPos.Row][nextPos.Col];
    if (tile == Tile.Obstacle)
        return (pos, NextDirection(dir), Action.Turn);

    grid[nextPos.Row][nextPos.Col] = Tile.Guard;
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

static Direction NextDirection(Direction dir) => dir switch
{
    Direction.Up => Direction.Right,
    Direction.Right => Direction.Down,
    Direction.Down => Direction.Left,
    Direction.Left => Direction.Up,
    _ => throw new InvalidOperationException(),
};

bool IsInbounds(Position pos) => pos.Row >= 0 && pos.Row < rows && pos.Col >= 0 && pos.Col < cols;

static string Display(Tile[][] grid)
    => string.Join('\n', grid.Select(row => new string(row.Select(tile => tile switch { Tile.Obstacle => '#', Tile.Guard => 'X', _ => '.' }).ToArray())));

enum Tile { Empty, Obstacle, Guard }

enum Direction { Up, Right, Down, Left }

enum Action { Move, Turn, Exit }

record struct Position(int Row, int Col);