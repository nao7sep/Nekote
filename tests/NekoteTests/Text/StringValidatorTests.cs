using Nekote.Text;

namespace Nekote.Tests.Text;

public class StringValidatorTests
{
    #region ValidateNoLeadingOrTrailingWhitespace Tests

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_ValidString_DoesNotThrow()
    {
        StringValidator.ValidateNoLeadingOrTrailingWhitespace("valid", "test");
        StringValidator.ValidateNoLeadingOrTrailingWhitespace("valid string", "test");
        StringValidator.ValidateNoLeadingOrTrailingWhitespace("a", "test");
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_EmptyString_DoesNotThrow()
    {
        StringValidator.ValidateNoLeadingOrTrailingWhitespace("", "test");
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_NullString_DoesNotThrow()
    {
        StringValidator.ValidateNoLeadingOrTrailingWhitespace(null!, "test");
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_LeadingSpace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            StringValidator.ValidateNoLeadingOrTrailingWhitespace(" leading", "TestParam"));
        Assert.Contains("TestParam", ex.Message);
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_TrailingSpace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            StringValidator.ValidateNoLeadingOrTrailingWhitespace("trailing ", "TestParam"));
        Assert.Contains("TestParam", ex.Message);
        Assert.Contains("cannot end with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_LeadingTab_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            StringValidator.ValidateNoLeadingOrTrailingWhitespace("\tleading", "TestParam"));
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_TrailingTab_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            StringValidator.ValidateNoLeadingOrTrailingWhitespace("trailing\t", "TestParam"));
        Assert.Contains("cannot end with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_LeadingNewline_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            StringValidator.ValidateNoLeadingOrTrailingWhitespace("\nleading", "TestParam"));
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_TrailingNewline_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            StringValidator.ValidateNoLeadingOrTrailingWhitespace("trailing\n", "TestParam"));
        Assert.Contains("cannot end with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_BothLeadingAndTrailing_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            StringValidator.ValidateNoLeadingOrTrailingWhitespace(" both ", "TestParam"));
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNoLeadingOrTrailingWhitespace_InternalWhitespace_DoesNotThrow()
    {
        StringValidator.ValidateNoLeadingOrTrailingWhitespace("internal whitespace", "test");
        StringValidator.ValidateNoLeadingOrTrailingWhitespace("multiple  spaces", "test");
    }

    #endregion

    #region ValidateNiniKey Tests

    [Fact]
    public void ValidateNiniKeyValueFileKey_ValidKey_DoesNotThrow()
    {
        StringValidator.ValidateNiniKey("validkey");
        StringValidator.ValidateNiniKey("valid-key");
        StringValidator.ValidateNiniKey("valid_key");
        StringValidator.ValidateNiniKey("ValidKey123");
    }

    [Fact]
    public void ValidateNiniKey_NullKey_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey(null!));
        Assert.Contains("cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_EmptyKey_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey(""));
        Assert.Contains("cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_WhitespaceOnlyKey_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey("   "));
        Assert.Contains("cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_LeadingWhitespace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey(" key"));
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_TrailingWhitespace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey("key "));
        Assert.Contains("cannot end with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_ContainsColon_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey("key:value"));
        Assert.Contains("contains invalid character ':'", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_ContainsNewline_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey("key\nvalue"));
        Assert.Contains("contains line breaks", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_ContainsCarriageReturn_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey("key\rvalue"));
        Assert.Contains("contains line breaks", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_StartsWithHash_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey("#comment"));
        Assert.Contains("starts with '#'", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_StartsWithDoubleSlash_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey("//comment"));
        Assert.Contains("starts with '//'", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_StartsWithBracket_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey("[section]"));
        Assert.Contains("starts with '['", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_StartsWithAtSign_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateNiniKey("@section"));
        Assert.Contains("starts with '@'", ex.Message);
    }

    [Fact]
    public void ValidateNiniKey_ContainsHashNotAtStart_DoesNotThrow()
    {
        StringValidator.ValidateNiniKey("key#value");
    }

    [Fact]
    public void ValidateNiniKey_ContainsSlashesNotAtStart_DoesNotThrow()
    {
        StringValidator.ValidateNiniKey("key//value");
    }

    #endregion

    #region Surrogate Pair Tests

    [Fact]
    public void StringValidator_HandlesSurrogatePairsInKeys()
    {
        // Keys can contain emojis, just not control chars
        const string Emoji = "😀";
        string key = "Key" + Emoji;
        var exception = Record.Exception(() => StringValidator.ValidateNiniKey(key));
        Assert.Null(exception);
    }

    #endregion
}

