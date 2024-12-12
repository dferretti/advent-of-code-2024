var chars = File.ReadAllLines("input.txt").Select(line => line.ToCharArray()).ToArray();

var regions = new List<Region>();
var regionMap = chars.Select(row => new Region[row.Length]).ToArray();

var outside = new Region('-');
for (var r = 0; r < chars.Length; r++)
{
    for (var c = 0; c < chars.Length; c++)
    {
        var plot = new Plot(r, c, FenceCount(chars, r, c));
        var type = chars[r][c];
        var northRegion = r > 0 ? regionMap[r - 1][c] : outside;
        var westRegion = c > 0 ? regionMap[r][c - 1] : outside;

        if (northRegion.Type == type && westRegion.Type == type)
        {
            northRegion.AddPlot(plot);

            // if 2 regions are joined by this new corner plot, merge them and remove the west region
            if (northRegion != westRegion)
            {
                foreach (var p in westRegion.Plots) regionMap[p.R][p.C] = northRegion;
                regions.Remove(westRegion);
                northRegion.Merge(westRegion);
            }

            regionMap[r][c] = northRegion;
        }
        else if (northRegion.Type == type)
        {
            northRegion.AddPlot(plot);
            regionMap[r][c] = northRegion;
        }
        else if (westRegion.Type == type)
        {
            westRegion.AddPlot(plot);
            regionMap[r][c] = westRegion;
        }
        else
        {
            var region = new Region(type);
            region.AddPlot(plot);
            regions.Add(region);
            regionMap[r][c] = region;
        }
    }
}

// foreach (var region in regions) Console.WriteLine($"Region {region.Type}: {region.Area} plots, {region.Permiter} fences, {region.Price} price");
Console.WriteLine($"Part 1 total price: {regions.Sum(region => region.Price)}");

static int FenceCount(char[][] chars, int r, int c)
{
    var count = 0;
    var type = chars[r][c];
    if (r <= 0 || chars[r - 1][c] != type) count++;
    if (r >= chars.Length - 1 || chars[r + 1][c] != type) count++;
    if (c <= 0 || chars[r][c - 1] != type) count++;
    if (c >= chars[r].Length - 1 || chars[r][c + 1] != type) count++;
    return count;
}

record Plot(int R, int C, int Fences);

class Region(char type)
{
    private readonly HashSet<Plot> _plots = [];

    public char Type => type;

    public int Area => _plots.Count;

    public int Permiter => _plots.Sum(plot => plot.Fences);

    public int Price => Area * Permiter;

    public void AddPlot(Plot plot) => _plots.Add(plot);

    public void Merge(Region region)
    {
        _plots.UnionWith(region._plots);
        region._plots.Clear();
    }

    public IEnumerable<Plot> Plots => _plots;
}