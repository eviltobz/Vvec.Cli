using Vvec.Cli.Config;

namespace Vvec.Cli.Tests.Config;

[TestFixture]
public class ConfigUpdaterTests
{
    public class TestConfig
    {
        public string? StringValue { get; init; }

        public int IntValue { get; init; }

        public int IV { get; init; }

        public bool BoolValue { get; init; }

        public FolderPath? AFolder { get; init; }

        public FilePath? AFile { get; init; }

        public Url? Url { get; init; }
    }

    //private ConfigUpdater<TestConfig> _sut;

    private TestConfig _config;

    [SetUp]
    public void Setup()
    {
        _config = new TestConfig();
    }

    [TestCase("StringValue", 1)]
    [TestCase("stringvalue", 1)]
    [TestCase("STRINGVALUE", 1)]
    [TestCase("sv", 1)]
    [TestCase("Url", 1)]
    [TestCase("u", 1)]
    [TestCase("aFile", 1)]
    [TestCase("iv", 1)] // Need to pay attention to this in setting test to favour full name over initials for IntValue
    [TestCase("af", 2)] // can't distinguish between folder or file path
    [TestCase("NonExistentField", 0)]
    public void MatchingFieldCount_ReturnsCorrectCountForFieldName(string fieldName, int expected)
    {
        var _sut = new ConfigUpdater<TestConfig>(_config, fieldName, "");

        var actual = _sut.MatchingFieldCount;

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void MatchingFields_ReturnsEmptyForNoMatch()
    {
        var _sut = new ConfigUpdater<TestConfig>(_config, "Non existent field name", "");

        var actual = _sut.MatchingFields;

        CollectionAssert.AreEquivalent(actual, new string[0]);
    }

    [Test]
    public void MatchingFields_ReturnsMultipleForNonUniqueInitials()
    {
        var _sut = new ConfigUpdater<TestConfig>(_config, "af", "");

        var actual = _sut.MatchingFields.ToArray();

        Assert.AreEqual("AFolder", actual[0].Name);
        Assert.AreEqual("FolderPath", actual[0].Type);
        Assert.AreEqual("AFile", actual[1].Name);
        Assert.AreEqual("FilePath", actual[1].Type);
    }

    [TestCase("StringValue", "")]
    [TestCase("StringValue", "some content")]
    [TestCase("StringValue", "!$$&*()blah")]
    [TestCase("IntValue", 0)]
    [TestCase("IntValue", int.MinValue)]
    [TestCase("IntValue", int.MaxValue)]
    [TestCase("BoolValue", "true")]
    [TestCase("BoolValue", "False")]
    [TestCase("AFile",@"C:\Windows\win.ini")]
    [TestCase("AFolder",@"C:\Windows")]
    public void FailedValidation_ValidContent_ReturnsFalse(string fieldName, object content)
    {
        var _sut = new ConfigUpdater<TestConfig>(_config, fieldName, content.ToString());

        var actual = _sut.FailedValidation;

        Assert.IsFalse(actual);
    }

    [TestCase("IntValue", "not an int", "Invalid Int")]
    [TestCase("IntValue", "", "Invalid Int")]
    [TestCase("IntValue", "9999999999", "Invalid Int")] // larger than int
    [TestCase("AFile", @"C:\I-dont-exist\invalid.filename", "Invalid FilePath")]
    [TestCase("AFile", @"not even close to a path", "Invalid FilePath")]
    [TestCase("AFolder", @"C:\Windows\win.ini", "Invalid FolderPath")]
    [TestCase("AFolder", @"C:\Not a folder\", "Invalid FolderPath")]
    [TestCase("AFolder", @"Not even slightly a folder", "Invalid FolderPath")]
    [TestCase("AFolder", @"", "Invalid FolderPath")]
    public void FailedValidation_InvalidContent_ReturnsTrue(string fieldName, string content, string expectedError)
    {
        var _sut = new ConfigUpdater<TestConfig>(_config, fieldName, content);

        var actual = _sut.FailedValidation;

        Assert.True(actual);
    }

    [Test]
    public void UpdateConfig_StringValue_UpdatesConfig()
    {
        const string content = "some content";
        var _sut = new ConfigUpdater<TestConfig>(_config, "StringValue", content);
        Assert.AreNotEqual(content, _config.StringValue);

        _ = _sut.FailedValidation;
        _sut.UpdateConfig();

        Assert.AreEqual(content, _config.StringValue);
    }

    [Test]
    public void UpdateConfig_IntValue_UpdatesConfig()
    {
        const int content = 23;
        var _sut = new ConfigUpdater<TestConfig>(_config, "IntValue", content.ToString());
        Assert.AreNotEqual(content, _config.IntValue);

        _ = _sut.FailedValidation;
        _sut.UpdateConfig();

        Assert.AreEqual(content, _config.IntValue);
    }

    [Test]
    public void UpdateConfig_BoolValue_UpdatesConfig()
    {
        const bool content = true;
        var _sut = new ConfigUpdater<TestConfig>(_config, "BoolValue", content.ToString());
        Assert.AreNotEqual(content, _config.BoolValue);

        _ = _sut.FailedValidation;
        _sut.UpdateConfig();

        Assert.AreEqual(content, _config.BoolValue);
    }

    [Test]
    public void UpdateConfig_AFile_UpdatesConfig()
    {
        const string content = @"C:\Windows\win.ini";
        var _sut = new ConfigUpdater<TestConfig>(_config, "AFile", content);
        Assert.Null(_config.AFile);

        _ = _sut.FailedValidation;
        _sut.UpdateConfig();

        Assert.AreEqual(content, _config.AFile.Value);
    }

    [Test]
    public void UpdateConfig_AFolder_UpdatesConfig()
    {
        const string content = @"C:\Windows\";
        var _sut = new ConfigUpdater<TestConfig>(_config, "AFolder", content);
        Assert.Null(_config.AFolder);

        _ = _sut.FailedValidation;
        _sut.UpdateConfig();

        Assert.AreEqual(content, _config.AFolder.Value);
    }
}

