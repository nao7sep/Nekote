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

    #region ValidateKeyValueFileKey Tests

    [Fact]
    public void ValidateKeyValueFileKeyValueFileKey_ValidKey_DoesNotThrow()
    {
        StringValidator.ValidateKeyValueFileKey("validkey");
        StringValidator.ValidateKeyValueFileKey("valid-key");
        StringValidator.ValidateKeyValueFileKey("valid_key");
        StringValidator.ValidateKeyValueFileKey("ValidKey123");
    }

    [Fact]
    public void ValidateKeyValueFileKey_NullKey_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey(null!));
        Assert.Contains("cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_EmptyKey_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey(""));
        Assert.Contains("cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_WhitespaceOnlyKey_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey("   "));
        Assert.Contains("cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_LeadingWhitespace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey(" key"));
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_TrailingWhitespace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey("key "));
        Assert.Contains("cannot end with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_ContainsColon_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey("key:value"));
        Assert.Contains("contains invalid character ':'", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_ContainsNewline_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey("key\nvalue"));
        Assert.Contains("contains line breaks", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_ContainsCarriageReturn_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey("key\rvalue"));
        Assert.Contains("contains line breaks", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_StartsWithHash_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey("#comment"));
        Assert.Contains("starts with '#'", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_StartsWithDoubleSlash_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey("//comment"));
        Assert.Contains("starts with '//'", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_StartsWithBracket_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey("[section]"));
        Assert.Contains("starts with '['", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_StartsWithAtSign_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKeyValueFileKey("@section"));
        Assert.Contains("starts with '@'", ex.Message);
    }

    [Fact]
    public void ValidateKeyValueFileKey_ContainsHashNotAtStart_DoesNotThrow()
    {
        StringValidator.ValidateKeyValueFileKey("key#value");
    }

    [Fact]
    public void ValidateKeyValueFileKey_ContainsSlashesNotAtStart_DoesNotThrow()
    {
        StringValidator.ValidateKeyValueFileKey("key//value");
    }

    #endregion
}
