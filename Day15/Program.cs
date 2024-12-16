var lines = File.ReadLines("input.txt");

var tiles = lines
    .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
    .Select(l => l.ToCharArray().Select(ParseTile).ToArray())
    .ToArray();
var grid = new Grid(tiles);
var robot = tiles
    .SelectMany((row, r) => row.Select((tile, c) => (Tile: tile, Position: new Position(r, c))))
    .Single(x => x.Tile == Tile.Robot)
    .Position;

var moves = lines
    .SkipWhile(line => !string.IsNullOrWhiteSpace(line))
    .SelectMany(line => line.ToCharArray())
    .Select(ParseMove);

grid.Print();

var step = 0;
foreach (var move in moves)
{
    step++;
    var canMove = true;
    var numBoxes = 0;
    var cursor = robot;
    while (true)
    {
        cursor += move;

        if (!grid.IsInbounds(cursor) || grid[cursor] == Tile.Wall)
        {
            canMove = false;
            break;
        } else if (grid[cursor] == Tile.Empty)
        {
            break;
        }

        numBoxes++;
    }

    if (!canMove) continue;

    grid[robot + move * (numBoxes + 1)] = Tile.Box;
    grid[robot + move] = Tile.Robot;
    grid[robot] = Tile.Empty;
    robot += move;

    Console.WriteLine($"Step {step}: {move.Display}");
    //grid.Print();
}

Console.WriteLine($"Part 1 sum: {grid.Part1Sum()}");

static Move ParseMove(char c) => c switch
{
    '^' => new(-1, 0, '^'),
    'v' => new(1, 0, 'v'),
    '<' => new(0, -1, '<'),
    '>' => new(0, 1, '>'),
    _ => throw new InvalidOperationException($"Unexpected move: {c}"),
};

static Tile ParseTile(char c) => c switch
{
    '.' => Tile.Empty,
    '#' => Tile.Wall,
    'O' => Tile.Box,
    '@' => Tile.Robot,
    _ => throw new InvalidOperationException($"Unexpected tile: {c}"),
};

enum Tile { Empty, Wall, Box, Robot }
record Position(int R, int C)
{
    public static Position operator +(Position p, Move m) => new(p.R + m.R, p.C + m.C);
}
record Move(int R, int C, char Display)
{
    public static Move operator *(Move m, int n) => new(m.R * n, m.C * n, m.Display);
}
class Grid(Tile[][] tiles)
{
    public Tile this[Position p]
    {
        get => tiles[p.R][p.C];
        set => tiles[p.R][p.C] = value;
    }

    public bool IsInbounds(Position p) =>
        p.R >= 0 && p.R < tiles.Length && p.C >= 0 && p.C < tiles[0].Length;

    public void Print()
    {
        foreach (var row in tiles)
        {
            foreach (var tile in row)
            {
                Console.Write(tile switch
                {
                    Tile.Empty => '.',
                    Tile.Wall => '#',
                    Tile.Box => 'O',
                    Tile.Robot => '@',
                    _ => throw new InvalidOperationException(),
                });
            }
            Console.WriteLine();
        }
    }

    public int Part1Sum() => tiles
        .SelectMany((row, r) => row.Select((tile, c) => (r, c, tile)))
        .Sum(x => x.tile == Tile.Box ? (100 * x.r) + x.c : 0);
}
