using NUnit.Framework;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.CoreTest.GeneraterTest;

[TestFixture]
public class LlmWordRankGeneraterTest
{
    [Test]
    public void TestParseRank()
    {
        var generater = new LlmWordRankGenerater();
        var json = @"{
  ""choices"": [
    {
      ""message"": {
        ""content"": ""{\""苹果\"": 850000}""
      }
    }
  ]
}";
        var rank = generater.ParseRank(json);
        Assert.That(rank, Is.EqualTo(850000));
    }

    [Test]
    public void TestParseRankRegex()
    {
        var generater = new LlmWordRankGenerater();
        var json = @"{
  ""choices"": [
    {
      ""message"": {
        ""content"": ""\\\""苹果\\\"": 12345""
      }
    }
  ]
}";
        var rank = generater.ParseRank(json);
        Assert.That(rank, Is.EqualTo(12345));
    }

    [Test]
    public void TestParseRanksJson()
    {
        var generater = new LlmWordRankGenerater();
        var json = @"{
  ""choices"": [
    {
      ""message"": {
        ""content"": ""{\""苹果\"": 850000, \""香蕉\"": 700000}""
      }
    }
  ]
}";
        var ranks = generater.ParseRanks(json);
        Assert.That(ranks["苹果"], Is.EqualTo(850000));
        Assert.That(ranks["香蕉"], Is.EqualTo(700000));
    }

    [Test]
    public void TestParseRanksRegex()
    {
        var generater = new LlmWordRankGenerater();
        var json = @"{
  ""choices"": [
    {
      ""message"": {
        ""content"": ""以下是词频：\n\""苹果\"": 850000\n\""香蕉\"": 700000""
      }
    }
  ]
}";
        var ranks = generater.ParseRanks(json);
        Assert.That(ranks["苹果"], Is.EqualTo(850000));
        Assert.That(ranks["香蕉"], Is.EqualTo(700000));
    }
}
