using Storage.Services.Services;

namespace Storage.Tests;

public class OrderFileParserTests
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
        var parser = new OrderFileParser();

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
                <LotID>42</LotID>
                <Qty>10</Qty>
                <Remarks>#5</Remarks>
              </Item>
              <Item>
                <LotID>99</LotID>
                <Qty>3</Qty>
                <Remarks>#7</Remarks>
              </Item>
            </Inventory>
            """;
        var path = CreateTempXmlFile(xml);
        var parser = new OrderFileParser();

        try
        {
            var result = parser.Parse(path).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(42, result[0].LotId);
            Assert.Equal(10, result[0].Qty);
            Assert.Equal("#5", result[0].Remarks);
            Assert.Equal(99, result[1].LotId);
            Assert.Equal(3, result[1].Qty);
            Assert.Equal("#7", result[1].Remarks);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Parse_ItemMissingLotId_IsSkipped()
    {
        var xml = """
            <Inventory>
              <Item>
                <Qty>5</Qty>
                <Remarks>#1</Remarks>
              </Item>
            </Inventory>
            """;
        var path = CreateTempXmlFile(xml);
        var parser = new OrderFileParser();

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
                <LotID>10</LotID>
                <Remarks>#1</Remarks>
              </Item>
            </Inventory>
            """;
        var path = CreateTempXmlFile(xml);
        var parser = new OrderFileParser();

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
                <LotID>7</LotID>
                <Qty>2</Qty>
              </Item>
            </Inventory>
            """;
        var path = CreateTempXmlFile(xml);
        var parser = new OrderFileParser();

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
