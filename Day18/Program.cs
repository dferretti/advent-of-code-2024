var example = false;
var bytes = File.ReadAllLines(example ? "example.txt" : "input.txt").Select(Parse).ToArray();
var start = new Coord(0, 0);
var exit = example ? new Coord(6, 6) : new Coord(70, 70);
var steps = example ? 12 : 1024;

var grid = new bool[exit.Y + 1][];
for (var y = 0; y < grid.Length; y++)
    grid[y] = new bool[exit.X + 1];

foreach (var coord in bytes.Take(steps))
    grid[coord.Y][coord.X] = true;

var pq = new PriorityQueue<Coord, int>();
var source = start;
var dist = new Dictionary<Coord, int> { [source] = 0 };
var prev = new Dictionary<Coord, Coord>();
pq.Enqueue(source, 0);
for (var y = 0; y < grid.Length; y++)
{
    for (var x = 0; x < grid[y].Length; x++)
    {
        var coord = new Coord(x, y);
        if (coord == source) continue;
        dist[coord] = int.MaxValue;
        pq.Enqueue(coord, int.MaxValue);
    }
}

while (pq.TryDequeue(out var node, out var priority))
{
    if (node == exit) break;
    foreach (var neighbor in GetNeighbors(grid, node))
    {
        var alt = dist[node] + 1;
        if (alt < dist[neighbor])
        {
            dist[neighbor] = alt;
            prev[neighbor] = node;
            pq.Remove(neighbor, out _, out _);
            pq.Enqueue(neighbor, alt);
        }
    }
}

var shortestDistance = dist[exit];
Console.WriteLine($"Part 1 distance: {shortestDistance}");

static IEnumerable<Coord> GetNeighbors(bool[][] grid, Coord coord)
{
    var (x, y) = coord;
    if (x > 0 && !grid[y][x - 1]) yield return new(x - 1, y);
    if (x < grid[y].Length - 1 && !grid[y][x + 1]) yield return new(x + 1, y);
    if (y > 0 && !grid[y - 1][x]) yield return new(x, y - 1);
    if (y < grid.Length - 1 && !grid[y + 1][x]) yield return new(x, y + 1);
}

static Coord Parse(string line)
{
    var parts = line.Split(',');
    return new(int.Parse(parts[0]), int.Parse(parts[1]));
}

record Coord(int X, int Y);