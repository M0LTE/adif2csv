using M0LTE.AdifLib;
using MaidenheadLib;

var (ok, infile, outfile) = ParseArgs();
if (!ok)
{
    Console.WriteLine("Usage: adif2csv [infile] [outfile]");
    return -1;
}

(bool ok, string infile, string outfile) ParseArgs()
{
    if (args.Length != 2)
    {
        return default;
    }

    if (!File.Exists(args[0]))
    {
        Console.WriteLine($"Could not find file {args[0]}");
        return default;
    }

    return (true, args[0], args[1]);
}

string data;
try
{
    data = File.ReadAllText(infile);
}
catch (Exception ex)
{
    Console.WriteLine($"Could not read {infile}: {ex.Message}");
    return -1;
}

if (!AdifFile.TryParse(data, out var adifFile, out var reason))
{
    Console.WriteLine($"Could not parse {infile}: {reason}");
    return -1;
}

const string dateformat = "dd/MM/yyyy HH:mm:ss";

List<string> lines = new();

foreach (var record in adifFile.Records)
{
    var mycall = record.StationCallsign ?? record.Operator;

    List<string?> rowItems = new() {
        DateTime.UtcNow.ToString(dateformat),
        record.QsoStart.ToString(dateformat),
        mycall?.ToUpper()?.Trim(),
        record.Call?.ToUpper()?.Trim(),
        record.Band?.ToUpper()?.Trim(),
        record.Mode?.ToUpper()?.Trim(),
        record.MyGridSquare?.ToUpper()?.Trim(),
        record.GridSquare?.ToUpper()?.Trim(),
        string.Empty
    };

    rowItems.AddRange(BuildGeoColumns(record));

    var line = string.Join(",", rowItems.Select(i => IsNumeric(i) ? i : $"\"{i}\""));

    lines.Add(line);
}

static bool IsNumeric(string? i)
{
    if (i == null) return true;
    return i.All(c => char.IsNumber(c) || c == '.' || c == '-');
}

const string Header = "Timestamp, QSO Datetime (UTC), Your Callsign, Their Callsign, Band, Mode, Locator Sent - Optional, Locator Received - Optional, Any other details - Optional, Their latitude, Their longitude, Your latitude, Your longitude, Distance (km)";

if (outfile == "-")
{
    Console.WriteLine(Header);
    foreach (var line in lines)
    {
        Console.WriteLine(line);
    }
}
else
{
    if (!File.Exists(outfile))
    {
        File.WriteAllText(outfile, Header + Environment.NewLine);
    }
    else
    {
        if (!File.ReadAllText(infile).EndsWith(Environment.NewLine))
        {
            File.AppendAllText(outfile, Environment.NewLine);
        }
    }

    try
    {
        File.AppendAllLines(outfile, lines);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        return -1;
    }
}

return 0;

static List<string> BuildGeoColumns(AdifContactRecord record)
{
    List<string> rowItems = new();

    if (!string.IsNullOrWhiteSpace(record.GridSquare))
    {
        try
        {
            var (lat, lon) = MaidenheadLocator.LocatorToLatLng(record.GridSquare);
            if (lat != default && lon != default)
            {
                rowItems.Add(lat.ToString("0.000000"));
                rowItems.Add(lon.ToString("0.000000"));
            }
        }
        catch (Exception)
        {
            rowItems.Add("");
            rowItems.Add("");
        }
    }
    else
    {
        rowItems.Add("");
        rowItems.Add("");
    }

    if (!string.IsNullOrWhiteSpace(record.MyGridSquare))
    {
        try
        {
            var (lat, lon) = MaidenheadLocator.LocatorToLatLng(record.MyGridSquare);
            if (lat != default && lon != default)
            {
                rowItems.Add(lat.ToString("0.000000"));
                rowItems.Add(lon.ToString("0.000000"));
            }
        }
        catch (Exception)
        {
            rowItems.Add("");
            rowItems.Add("");
        }
    }
    else
    {
        rowItems.Add("");
        rowItems.Add("");
    }

    if (!string.IsNullOrWhiteSpace(record.GridSquare) && !string.IsNullOrWhiteSpace(record.MyGridSquare))
    {
        try
        {
            rowItems.Add(MaidenheadLocator.Distance(record.GridSquare, record.MyGridSquare).ToString("0"));
        }
        catch (Exception)
        {
            rowItems.Add("");
        }
    }
    else
    {
        rowItems.Add("");
    }

    return rowItems;
}