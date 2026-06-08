using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using OpenHome3D.Core.Models;

namespace OpenHome3D.Core.IO;

public static class Sh3dIO
{
    public static Sh3dFile Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{filePath}' does not exist.");
        }

        using var archive = ZipFile.OpenRead(filePath);
        
        var homeEntry = archive.Entries.FirstOrDefault(e => e.FullName == "Home.xml") ?? archive.Entries.FirstOrDefault(e => e.Name == "Home");
        if (homeEntry == null)
        {
            throw new InvalidDataException("The .sh3d file does not contain a 'Home' or 'Home.xml' entry.");
        }

        string xmlContent;
        using (var reader = new StreamReader(homeEntry.Open()))
        {
            xmlContent = reader.ReadToEnd();
        }

        var home = HomeXmlParser.Parse(xmlContent);

        var content = new Dictionary<string, byte[]>();
        foreach (var entry in archive.Entries)
        {
            if (entry.Name != "Home" && entry.Name != "Home.xml")
            {
                using var stream = entry.Open();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                content[entry.FullName] = memoryStream.ToArray();
            }
        }

        return new Sh3dFile(home, content);
    }

    public static void Save(Home home, IReadOnlyDictionary<string, byte[]> content, string filePath)
    {
        using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
        
        var xmlContent = HomeXmlExporter.Export(home);
        var entry = archive.CreateEntry("Home.xml");
        using (var stream = entry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write(xmlContent);
        }

        foreach (var kvp in content)
        {
            var contentEntry = archive.CreateEntry(kvp.Key);
            using var stream = contentEntry.Open();
            stream.Write(kvp.Value, 0, kvp.Value.Length);
        }
    }
}

public record Sh3dFile(Home Home, IReadOnlyDictionary<string, byte[]> Content);
