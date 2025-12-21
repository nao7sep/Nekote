using Nekote.Text;

namespace Nekote.Tests.Text;

public class CharOrStringTests
{
    [Fact]
    public void FromChar_CreatesCharInstance()
    {
        var cos = CharOrString.FromChar('x');

        Assert.True(cos.IsChar);
        Assert.False(cos.IsString);
        Assert.Equal('x', cos.AsChar());
        Assert.Equal(1, cos.Length);
    }

    [Fact]
    public void FromString_CreatesStringInstance()
    {
        var cos = CharOrString.FromString("hello");

        Assert.False(cos.IsChar);
        Assert.True(cos.IsString);
        Assert.Equal("hello", cos.AsString());
        Assert.Equal(5, cos.Length);
    }

    [Fact]
    public void FromString_NullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => CharOrString.FromString(null!));
    }

    [Fact]
    public void ImplicitConversion_FromChar()
    {
        CharOrString cos = 'x';

        Assert.True(cos.IsChar);
        Assert.Equal('x', cos.AsChar());
    }

    [Fact]
    public void ImplicitConversion_FromString()
    {
        CharOrString cos = "hello";

        Assert.True(cos.IsString);
        Assert.Equal("hello", cos.AsString());
    }

    [Fact]
    public void ImplicitConversion_NullStringThrows()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            CharOrString cos = (string)null!;
        });
    }

    [Fact]
    public void AsChar_ThrowsWhenContainsString()
    {
        var cos = CharOrString.FromString("hello");

        var ex = Assert.Throws<InvalidOperationException>(() => cos.AsChar());
        Assert.Contains("does not contain a character", ex.Message);
    }

    [Fact]
    public void AsString_ThrowsWhenContainsChar()
    {
        var cos = CharOrString.FromChar('x');

        var ex = Assert.Throws<InvalidOperationException>(() => cos.AsString());
        Assert.Contains("does not contain a string", ex.Message);
    }

    [Fact]
    public void AsSpan_ReturnsStringSpan()
    {
        var cos = CharOrString.FromString("hello");
        var span = cos.AsSpan();

        Assert.Equal(5, span.Length);
        Assert.Equal("hello", new string(span));
    }

    [Fact]
    public void AsSpan_ThrowsWhenContainsChar()
    {
        var cos = CharOrString.FromChar('x');

        var ex = Assert.Throws<InvalidOperationException>(() => cos.AsSpan());
        Assert.Contains("does not contain a string", ex.Message);
    }

    [Theory]
    [InlineData('a', 'a', true)]
    [InlineData('a', 'b', false)]
    public void Equals_Chars(char left, char right, bool expected)
    {
        var cos1 = CharOrString.FromChar(left);
        var cos2 = CharOrString.FromChar(right);

        Assert.Equal(expected, cos1.Equals(cos2));
        Assert.Equal(expected, cos1 == cos2);
        Assert.Equal(!expected, cos1 != cos2);
    }

    [Theory]
    [InlineData("hello", "hello", true)]
    [InlineData("hello", "world", false)]
    [InlineData("Hello", "hello", false)] // Case-sensitive
    public void Equals_Strings(string left, string right, bool expected)
    {
        var cos1 = CharOrString.FromString(left);
        var cos2 = CharOrString.FromString(right);

        Assert.Equal(expected, cos1.Equals(cos2));
        Assert.Equal(expected, cos1 == cos2);
        Assert.Equal(!expected, cos1 != cos2);
    }

    [Fact]
    public void Equals_DifferentTypes_ReturnsFalse()
    {
        var charCos = CharOrString.FromChar('x');
        var stringCos = CharOrString.FromString("x");

        Assert.False(charCos.Equals(stringCos));
        Assert.False(charCos == stringCos);
        Assert.True(charCos != stringCos);
    }

    [Fact]
    public void GetHashCode_SameChars_SameHash()
    {
        var cos1 = CharOrString.FromChar('x');
        var cos2 = CharOrString.FromChar('x');

        Assert.Equal(cos1.GetHashCode(), cos2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_SameStrings_SameHash()
    {
        var cos1 = CharOrString.FromString("hello");
        var cos2 = CharOrString.FromString("hello");

        Assert.Equal(cos1.GetHashCode(), cos2.GetHashCode());
    }

    [Fact]
    public void ToString_Char()
    {
        var cos = CharOrString.FromChar('x');
        Assert.Equal("x", cos.ToString());
    }

    [Fact]
    public void ToString_String()
    {
        var cos = CharOrString.FromString("hello");
        Assert.Equal("hello", cos.ToString());
    }

    [Fact]
    public void WriteTo_Char_Success()
    {
        var cos = CharOrString.FromChar('x');
        Span<char> destination = stackalloc char[5];

        int written = cos.WriteTo(destination);

        Assert.Equal(1, written);
        Assert.Equal('x', destination[0]);
    }

    [Fact]
    public void WriteTo_Char_TooSmall_Throws()
    {
        var cos = CharOrString.FromChar('x');
        Span<char> destination = new char[0];

        ArgumentException ex = null!;
        try
        {
            cos.WriteTo(destination);
        }
        catch (ArgumentException e)
        {
            ex = e;
        }

        Assert.NotNull(ex);
        Assert.Contains("too small", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WriteTo_String_Success()
    {
        var cos = CharOrString.FromString("hello");
        Span<char> destination = stackalloc char[10];

        int written = cos.WriteTo(destination);

        Assert.Equal(5, written);
        Assert.Equal("hello", new string(destination.Slice(0, written)));
    }

    [Fact]
    public void WriteTo_String_TooSmall_Throws()
    {
        var cos = CharOrString.FromString("hello");
        Span<char> destination = new char[3];

        ArgumentException ex = null!;
        try
        {
            cos.WriteTo(destination);
        }
        catch (ArgumentException e)
        {
            ex = e;
        }

        Assert.NotNull(ex);
        Assert.Contains("too small", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmptyString_HasZeroLength()
    {
        var cos = CharOrString.FromString("");
        Assert.Equal(0, cos.Length);
    }

    #region Edge Cases - Unicode

    [Fact]
    public void String_WithEmoji_CorrectLength()
    {
        var emoji = "😀"; // Single emoji, but 2 UTF-16 code units (surrogate pair)
        var cos = CharOrString.FromString(emoji);

        Assert.Equal(2, cos.Length); // Length is in char units, not Unicode scalar values
        Assert.Equal(emoji, cos.AsString());
    }

    [Fact]
    public void String_WithMultipleEmojis()
    {
        var text = "Hello 😀 World 🌍";
        var cos = CharOrString.FromString(text);

        Assert.Equal(text, cos.AsString());
        Assert.Equal(text.Length, cos.Length);
    }

    [Fact]
    public void String_WithZeroWidthCharacters()
    {
        var text = "Hello\u200BWorld"; // Contains zero-width space
        var cos = CharOrString.FromString(text);

        Assert.Equal(text, cos.AsString());
        Assert.Equal(11, cos.Length);
    }

    [Fact]
    public void String_WithCombiningCharacters()
    {
        var text = "e\u0301"; // e with combining acute accent
        var cos = CharOrString.FromString(text);

        Assert.Equal(text, cos.AsString());
        Assert.Equal(2, cos.Length);
    }

    [Fact]
    public void Equals_EmojiStrings()
    {
        var cos1 = CharOrString.FromString("😀");
        var cos2 = CharOrString.FromString("😀");
        var cos3 = CharOrString.FromString("😁");

        Assert.True(cos1.Equals(cos2));
        Assert.False(cos1.Equals(cos3));
    }

    [Fact]
    public void WriteTo_Emoji_Success()
    {
        var emoji = "😀";
        var cos = CharOrString.FromString(emoji);
        Span<char> destination = stackalloc char[10];

        int written = cos.WriteTo(destination);

        Assert.Equal(2, written);
        Assert.Equal(emoji, new string(destination.Slice(0, written)));
    }

    [Fact]
    public void String_VeryLong()
    {
        var longString = new string('x', 10000);
        var cos = CharOrString.FromString(longString);

        Assert.Equal(10000, cos.Length);
        Assert.Equal(longString, cos.AsString());
    }

    [Fact]
    public void DefaultValue_Behavior()
    {
        // Extreme Edge Case: The default(struct) state
        // This state has neither char nor string (both are 0/null).
        CharOrString def = default;

        Assert.False(def.IsChar, "Default should not be Char");
        Assert.False(def.IsString, "Default should not be String");

        // Accessing values should throw
        Assert.Throws<InvalidOperationException>(() => def.AsChar());
        Assert.Throws<InvalidOperationException>(() => def.AsString());
        Assert.Throws<InvalidOperationException>(() => def.AsSpan());

        // ToString() on default struct returns null
        Assert.Null(def.ToString());

        // Length throws NullReferenceException because internal string is null
        Assert.Throws<NullReferenceException>(() => _ = def.Length);
    }

    [Fact]
    public void WriteTo_ExactBufferSize()
    {
        // Edge Case: Buffer is EXACTLY the size needed.
        // Checks for off-by-one errors in bounds checking.

        // Char
        var charCos = CharOrString.FromChar('A');
        Span<char> charDest = stackalloc char[1]; // Exact size
        int charWritten = charCos.WriteTo(charDest);
        Assert.Equal(1, charWritten);
        Assert.Equal('A', charDest[0]);

        // String
        var strCos = CharOrString.FromString("ABC");
        Span<char> strDest = stackalloc char[3]; // Exact size
        int strWritten = strCos.WriteTo(strDest);
        Assert.Equal(3, strWritten);
        Assert.Equal("ABC", new string(strDest));
    }

    #endregion
}
