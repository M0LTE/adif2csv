# adif2csv
A .NET 6 command line utility to take ADIF files and convert them to the CSV format required for OARC's 12 days of OARCmas event.

Tested with Cloudlog and N1MM+ export files.

## Prerequisites
.NET 6 SDK for your platform from https://dotnet.microsoft.com/en-us/download

## Building

```
git clone https://github.com/M0LTE/adif2csv.git
dotnet build 
```

## Usage

```
cd bin\Debug\net6.0
adif2csv [infile] [outfile]
```

outfile can be a hyphen, in which case output will be to stdout.

outfile can also exist, in which case it will be appended to.

