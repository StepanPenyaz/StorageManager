using Storage.Services.Services;

namespace Storage.Tests;

public class StorageUpdateFileParserTests
{
    private static string CreateTempXmlFile(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Parse_NoItems_ReturnsEmptyCollection()
    {
        var path = CreateTempXmlFile("<Inventory></Inventory>");
        var parser = new StorageUpdateFileParser();

        try
        {
            var result = parser.Parse(path);

            Assert.Empty(result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Parse_ValidItems_ReturnsCorrectValues()
    {
        var xml = """
            <Inventory>
              <Item>
                <ItemID>ITEM-001</ItemID>
                <Qty>20</Qty>
                <Remarks>#3</Remarks>
              </Item>
              <Item>
                <ItemID>ITEM-002</ItemID>
                <Qty>5</Qty>
                <Remarks>#8</Remarks>
              </Item>
            </Inventory>
            """;
        var path = CreateTempXmlFile(xml);
        var parser = new StorageUpdateFileParser();

        try
        {
            var result = parser.Parse(path).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("ITEM-001", result[0].ItemId);
            Assert.Equal(20, result[0].Qty);
            Assert.Equal("#3", result[0].Remarks);
            Assert.Equal("ITEM-002", result[1].ItemId);
            Assert.Equal(5, result[1].Qty);
            Assert.Equal("#8", result[1].Remarks);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Parse_ItemMissingItemId_IsSkipped()
    {
        var xml = """
            <Inventory>
              <Item>
                <Qty>10</Qty>
                <Remarks>#1</Remarks>
              </Item>
            </Inventory>
            """;
        var path = CreateTempXmlFile(xml);
        var parser = new StorageUpdateFileParser();

        try
        {
            var result = parser.Parse(path);

            Assert.Empty(result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Parse_ItemMissingQty_IsSkipped()
    {
        var xml = """
            <Inventory>
              <Item>
                <ItemID>ITEM-X</ItemID>
                <Remarks>#1</Remarks>
              </Item>
            </Inventory>
            """;
        var path = CreateTempXmlFile(xml);
        var parser = new StorageUpdateFileParser();

        try
        {
            var result = parser.Parse(path);

            Assert.Empty(result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Parse_ItemWithoutRemarks_ReturnsEmptyStringRemarks()
    {
        var xml = """
            <Inventory>
              <Item>
                <ItemID>ITEM-Y</ItemID>
                <Qty>1</Qty>
              </Item>
            </Inventory>
            """;
        var path = CreateTempXmlFile(xml);
        var parser = new StorageUpdateFileParser();

        try
        {
            var result = parser.Parse(path).ToList();

            Assert.Single(result);
            Assert.Equal(string.Empty, result[0].Remarks);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
