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

    #region ValidateKey Tests

    [Fact]
    public void ValidateKey_ValidKey_DoesNotThrow()
    {
        StringValidator.ValidateKey("validkey");
        StringValidator.ValidateKey("valid-key");
        StringValidator.ValidateKey("valid_key");
        StringValidator.ValidateKey("ValidKey123");
    }

    [Fact]
    public void ValidateKey_NullKey_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey(null!));
        Assert.Contains("cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKey_EmptyKey_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey(""));
        Assert.Contains("cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKey_WhitespaceOnlyKey_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey("   "));
        Assert.Contains("cannot be null or whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKey_LeadingWhitespace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey(" key"));
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKey_TrailingWhitespace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey("key "));
        Assert.Contains("cannot end with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateKey_ContainsColon_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey("key:value"));
        Assert.Contains("contains invalid character ':'", ex.Message);
    }

    [Fact]
    public void ValidateKey_ContainsNewline_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey("key\nvalue"));
        Assert.Contains("contains line breaks", ex.Message);
    }

    [Fact]
    public void ValidateKey_ContainsCarriageReturn_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey("key\rvalue"));
        Assert.Contains("contains line breaks", ex.Message);
    }

    [Fact]
    public void ValidateKey_StartsWithHash_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey("#comment"));
        Assert.Contains("starts with '#'", ex.Message);
    }

    [Fact]
    public void ValidateKey_StartsWithDoubleSlash_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey("//comment"));
        Assert.Contains("starts with '//'", ex.Message);
    }

    [Fact]
    public void ValidateKey_StartsWithBracket_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey("[section]"));
        Assert.Contains("starts with '['", ex.Message);
    }

    [Fact]
    public void ValidateKey_StartsWithAtSign_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateKey("@section"));
        Assert.Contains("starts with '@'", ex.Message);
    }

    [Fact]
    public void ValidateKey_ContainsHashNotAtStart_DoesNotThrow()
    {
        StringValidator.ValidateKey("key#value");
    }

    [Fact]
    public void ValidateKey_ContainsSlashesNotAtStart_DoesNotThrow()
    {
        StringValidator.ValidateKey("key//value");
    }

    #endregion

    #region ValidateSectionName Tests

    [Fact]
    public void ValidateSectionName_ValidName_DoesNotThrow()
    {
        StringValidator.ValidateSectionName("Section");
        StringValidator.ValidateSectionName("Section Name");
        StringValidator.ValidateSectionName("Section-123");
    }

    [Fact]
    public void ValidateSectionName_EmptyString_DoesNotThrow()
    {
        // Empty string is valid (represents preamble)
        StringValidator.ValidateSectionName("");
    }

    [Fact]
    public void ValidateSectionName_WhitespaceOnly_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateSectionName("   "));
        Assert.Contains("cannot be whitespace only", ex.Message);
    }

    [Fact]
    public void ValidateSectionName_LeadingWhitespace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateSectionName(" Section"));
        Assert.Contains("cannot start with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateSectionName_TrailingWhitespace_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringValidator.ValidateSectionName("Section "));
        Assert.Contains("cannot end with whitespace", ex.Message);
    }

    [Fact]
    public void ValidateSectionName_InternalWhitespace_DoesNotThrow()
    {
        StringValidator.ValidateSectionName("Section Name");
        StringValidator.ValidateSectionName("Multi Word Section");
    }

    #endregion
}
