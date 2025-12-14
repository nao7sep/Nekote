using System.Text;
using Nekote.Text;

namespace NekoteTests.Text;

public class TextEncodingTests
{
    [Fact]
    public void Utf8NoBom_HasNoBom()
    {
        var preamble = TextEncoding.Utf8NoBom.GetPreamble();
        Assert.Empty(preamble);
    }

    [Fact]
    public void Utf8WithBom_HasBom()
    {
        var preamble = TextEncoding.Utf8WithBom.GetPreamble();
        Assert.Equal(3, preamble.Length);
        Assert.Equal(0xEF, preamble[0]);
        Assert.Equal(0xBB, preamble[1]);
        Assert.Equal(0xBF, preamble[2]);
    }

    [Fact]
    public async Task SectionedKeyValueFile_SaveLoad_UsesUtf8NoBomByDefault()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var file = new SectionedKeyValueFile();
            file.SetValue("Section1", "Key1", "Value with 日本語 and emoji 🎉");
            file.Save(tempFile);

            // Read raw bytes
            var bytes = await File.ReadAllBytesAsync(tempFile);

            // Should not start with BOM (EF BB BF)
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);

            // Should load correctly
            var loaded = SectionedKeyValueFile.Load(tempFile);
            Assert.Equal("Value with 日本語 and emoji 🎉", loaded.GetString("Section1", "Key1"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SectionedKeyValueFile_SaveWithBom_LoadWithBom()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var file = new SectionedKeyValueFile();
            file.SetValue("Section1", "Key1", "Value with 日本語");
            file.Save(tempFile, TextEncoding.Utf8WithBom);

            // Read raw bytes
            var bytes = await File.ReadAllBytesAsync(tempFile);

            // Should start with BOM (EF BB BF)
            Assert.True(bytes.Length >= 3);
            Assert.Equal(0xEF, bytes[0]);
            Assert.Equal(0xBB, bytes[1]);
            Assert.Equal(0xBF, bytes[2]);

            // Should load correctly (File.ReadAllText handles BOM automatically)
            var loaded = SectionedKeyValueFile.Load(tempFile, encoding: TextEncoding.Utf8WithBom);
            Assert.Equal("Value with 日本語", loaded.GetString("Section1", "Key1"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task KeyValueParser_ParseFileAsync_HandlesUtf8()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "Key: Value with 日本語\n", TextEncoding.Utf8NoBom);

            var result = await KeyValueParser.ParseFileAsync(tempFile);

            Assert.Equal("Value with 日本語", result["Key"]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LineParser_ToLinesFromFileAsync_HandlesUtf8()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "Line1 日本語\nLine2 🎉", TextEncoding.Utf8NoBom);

            var lines = await LineParser.ToLinesFromFileAsync(tempFile);

            Assert.Equal(2, lines.Length);
            Assert.Equal("Line1 日本語", lines[0]);
            Assert.Equal("Line2 🎉", lines[1]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LineParser_FromLinesToFileAsync_UsesUtf8NoBomByDefault()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var lines = new[] { "Line1 日本語", "Line2 🎉" };
            await LineParser.FromLinesToFileAsync(tempFile, lines);

            // Read raw bytes
            var bytes = await File.ReadAllBytesAsync(tempFile);

            // Should not start with BOM
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);

            // Should load correctly
            var loaded = await LineParser.ToLinesFromFileAsync(tempFile);
            Assert.Equal("Line1 日本語", loaded[0]);
            Assert.Equal("Line2 🎉", loaded[1]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParagraphParser_ParseFileAsync_HandlesUtf8()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "Para1 日本語\n\nPara2 🎉\n", TextEncoding.Utf8NoBom);

            var paragraphs = await ParagraphParser.ParseFileAsync(tempFile);

            Assert.Equal(2, paragraphs.Length);
            Assert.Equal("Para1 日本語", paragraphs[0]);
            Assert.Equal("Para2 🎉", paragraphs[1]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
