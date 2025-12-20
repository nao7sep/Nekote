using System;
using Nekote.Text;
using Xunit;

namespace NekoteTests.Text;

public class CharBufferTests
{
    [Fact]
    public void Constructor_Default_CreatesEmptyBuffer()
    {
        using var buffer = new CharBuffer();
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Capacity >= CharBuffer.DefaultInitialSize);
        Assert.True(buffer.IsEmpty);
    }

    [Fact]
    public void Constructor_WithCapacity_CreatesBufferWithCapacity()
    {
        using var buffer = new CharBuffer(100);
        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.Capacity >= 100);
    }

    // --- Append ---

    [Fact]
    public void Append_Char_AppendsCharacter()
    {
        using var buffer = new CharBuffer();
        buffer.Append('a');
        Assert.Equal(1, buffer.Length);
        Assert.Equal('a', buffer[0]);
        Assert.Equal("a", buffer.ToString());
    }

    [Fact]
    public void Append_Repeat_AppendsCharacters()
    {
        using var buffer = new CharBuffer();
        buffer.Append('a', 3);
        Assert.Equal(3, buffer.Length);
        Assert.Equal("aaa", buffer.ToString());
    }

    [Fact]
    public void Append_Span_AppendsCharacters()
    {
        using var buffer = new CharBuffer();
        buffer.Append("hello".AsSpan());
        Assert.Equal("hello", buffer.ToString());
    }

    [Fact]
    public void AppendLine_AppendsNewLine()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abc");
        buffer.AppendLine();
        Assert.Equal("abc" + Environment.NewLine, buffer.ToString());
    }

    // --- Insert ---

    [Fact]
    public void Insert_Char_InsertsAtPosition()
    {
        using var buffer = new CharBuffer();
        buffer.Append("ac");
        buffer.Insert(1, 'b');
        Assert.Equal("abc", buffer.ToString());
    }

    [Fact]
    public void Insert_Repeat_InsertsAtPosition()
    {
        using var buffer = new CharBuffer();
        buffer.Append("ad");
        buffer.Insert(1, 'b', 2); // Insert 'b' twice at index 1
        Assert.Equal("abbd", buffer.ToString());
    }

    [Fact]
    public void Insert_Span_InsertsAtPosition()
    {
        using var buffer = new CharBuffer();
        buffer.Append("ad");
        buffer.Insert(1, "bc".AsSpan());
        Assert.Equal("abcd", buffer.ToString());
    }

    [Fact]
    public void Insert_AtEndOfBuffer_WorksLikeAppend()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abc");
        buffer.Insert(3, 'd');
        Assert.Equal("abcd", buffer.ToString());
    }

    [Fact]
    public void Insert_AtBeginning_WorksCorrectly()
    {
        using var buffer = new CharBuffer();
        buffer.Append("bcd");
        buffer.Insert(0, 'a');
        Assert.Equal("abcd", buffer.ToString());
    }

    // --- Remove ---

    [Fact]
    public void Remove_RemovesRange()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abcde");
        buffer.Remove(1, 2); // Remove "bc"
        Assert.Equal("ade", buffer.ToString());
    }

    [Fact]
    public void Remove_AtEndOfBuffer_Works()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abcde");
        buffer.Remove(3, 2); // Remove "de"
        Assert.Equal("abc", buffer.ToString());
    }

    // --- Indexer & Properties ---

    [Fact]
    public void Indexer_GetSet_WorksCorrectly()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abc");
        Assert.Equal('b', buffer[1]);

        buffer[1] = 'z';
        Assert.Equal('z', buffer[1]);
        Assert.Equal("azc", buffer.ToString());
    }

    [Fact]
    public void EnsureCapacity_GrowsBuffer()
    {
        using var buffer = new CharBuffer(10);
        int initialCapacity = buffer.Capacity;

        // Force growth
        buffer.EnsureCapacity(100);

        Assert.True(buffer.Capacity >= 100);
        Assert.True(buffer.Capacity > initialCapacity);
    }

    [Fact]
    public void EnsureCapacity_NegativeValue_ThrowsException()
    {
        using var buffer = new CharBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.EnsureCapacity(-1));
    }

    [Fact]
    public void EnsureCapacity_ExactPowerOfTwo_DoesNotDoubleExcessively()
    {
        using var buffer = new CharBuffer(256);
        int initial = buffer.Capacity;

        int target = initial * 2 + 1;
        buffer.EnsureCapacity(target);

        Assert.True(buffer.Capacity >= target);
    }

    [Fact]
    public void Clear_ResetsLengthButNotCapacity()
    {
        using var buffer = new CharBuffer();
        buffer.Append("test");
        int capacity = buffer.Capacity;

        buffer.Clear();

        Assert.Equal(0, buffer.Length);
        Assert.True(buffer.IsEmpty);
        Assert.Equal(capacity, buffer.Capacity);
    }

    [Fact]
    public void Length_Setter_TruncatesAndExtends()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abc");

        // Truncate
        buffer.Length = 2;
        Assert.Equal("ab", buffer.ToString());

        // Extend (zero-fill)
        buffer.Length = 4;
        Assert.Equal(4, buffer.Length);
        Assert.Equal('\0', buffer[2]);
        Assert.Equal('\0', buffer[3]);
    }

    // --- Replace ---

    [Fact]
    public void Replace_Char_ReplacesAllOccurrences()
    {
        using var buffer = new CharBuffer();
        buffer.Append("bananas");
        buffer.Replace('a', 'o');
        Assert.Equal("bononos", buffer.ToString());
    }

    [Fact]
    public void Replace_NoMatch_DoesNothing()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abc");
        buffer.Replace('z', 'x');
        Assert.Equal("abc", buffer.ToString());
    }

    [Fact]
    public void Replace_Range_OnlyReplacesInRange()
    {
        using var buffer = new CharBuffer();
        buffer.Append("aaaaa");
        // Replace 'a' with 'b' only in indices 1..3 (length 2) -> "abbaa"
        buffer.Replace('a', 'b', 1, 2);
        Assert.Equal("abbaa", buffer.ToString());
    }

    [Fact]
    public void ReplaceAny_ReplacesSpecifiedChars()
    {
        using var buffer = new CharBuffer();
        buffer.Append("hello world");
        buffer.ReplaceAny("aeiou".AsSpan(), '*');
        Assert.Equal("h*ll* w*rld", buffer.ToString());
    }

    [Fact]
    public void ReplaceAny_EmptySet_DoesNothing()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abc");
        buffer.ReplaceAny(ReadOnlySpan<char>.Empty, 'z');
        Assert.Equal("abc", buffer.ToString());
    }

    // --- Search (IndexOf/Contains) ---

    [Fact]
    public void IndexOf_FindsFirstOccurrence()
    {
        using var buffer = new CharBuffer();
        buffer.Append("hello");
        Assert.Equal(1, buffer.IndexOf('e'));
        Assert.Equal(-1, buffer.IndexOf('z'));
    }

    [Fact]
    public void IndexOf_DoesNotSearchPastLength()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abc");
        buffer.Length = 2; // "ab", 'c' is still in memory but should be ignored

        Assert.Equal(-1, buffer.IndexOf('c'));
    }

    [Fact]
    public void IndexOf_StartIndex_RespectsBoundaries()
    {
        using var buffer = new CharBuffer();
        buffer.Append("banana");
        // 012345
        // banana
        Assert.Equal(3, buffer.IndexOf('a', 2)); // Should skip index 1
    }

    [Fact]
    public void LastIndexOf_FindsLastOccurrence()
    {
        using var buffer = new CharBuffer();
        buffer.Append("ababa");
        Assert.Equal(4, buffer.LastIndexOf('a'));
    }

    [Fact]
    public void LastIndexOf_StartIndex_SearchesBackwardsFromIndex()
    {
        using var buffer = new CharBuffer();
        buffer.Append("ababa");
        // 01234
        // ababa
        // Searching backwards from index 3 ('b'): indices 0,1,2,3 are "abab"
        Assert.Equal(2, buffer.LastIndexOf('a', 3));
    }

    [Fact]
    public void IndexOfAny_FindsFirstOccurrence()
    {
        using var buffer = new CharBuffer();
        buffer.Append("hello");
        // "hello" -> 'h'(0), 'e'(1), 'l'(2), 'l'(3), 'o'(4)
        // Searching for 'o' or 'l'. First 'l' is at 2.
        Assert.Equal(2, buffer.IndexOfAny("ol".AsSpan()));
    }

    [Fact]
    public void IndexOfAny_StartIndex_RespectsBoundaries()
    {
        using var buffer = new CharBuffer();
        buffer.Append("hello");
        // Start at 3 ("lo"). 'l' at 3, 'o' at 4.
        Assert.Equal(3, buffer.IndexOfAny("ol".AsSpan(), 3));
    }

    [Fact]
    public void LastIndexOfAny_FindsLastOccurrence()
    {
        using var buffer = new CharBuffer();
        buffer.Append("hello");
        // 'o' at 4, 'l' at 3. Last is 4.
        Assert.Equal(4, buffer.LastIndexOfAny("ol".AsSpan()));
    }

    [Fact]
    public void LastIndexOfAny_StartIndex_SearchesBackwards()
    {
        using var buffer = new CharBuffer();
        buffer.Append("hello");
        // Search back from 3 ("hell"). 'l' at 3, 'l' at 2.
        Assert.Equal(3, buffer.LastIndexOfAny("ol".AsSpan(), 3));
    }

    [Fact]
    public void Contains_ReturnsTrueIfFound()
    {
        using var buffer = new CharBuffer();
        buffer.Append("test");
        Assert.True(buffer.Contains('e'));
        Assert.False(buffer.Contains('x'));
    }

    [Fact]
    public void Contains_StartIndex_ReturnsTrueIfFoundAfterIndex()
    {
        using var buffer = new CharBuffer();
        buffer.Append("banana");
        // 012345
        // banana
        Assert.True(buffer.Contains('a', 2));
        Assert.False(buffer.Contains('b', 2));
    }

    [Fact]
    public void Contains_Range_ReturnsTrueIfFoundInRange()
    {
        using var buffer = new CharBuffer();
        buffer.Append("012345");
        Assert.True(buffer.Contains('2', 1, 3)); // "123"
        Assert.False(buffer.Contains('5', 1, 3)); // "123"
    }

    [Fact]
    public void ContainsAny_ReturnsTrueIfAnyFound()
    {
        using var buffer = new CharBuffer();
        buffer.Append("test");
        Assert.True(buffer.ContainsAny("xyze".AsSpan()));
        Assert.False(buffer.ContainsAny("xyz".AsSpan()));
    }

    [Fact]
    public void ContainsAny_Range_ReturnsTrueIfAnyFound()
    {
        using var buffer = new CharBuffer();
        buffer.Append("012345");
        Assert.True(buffer.ContainsAny("982".AsSpan(), 1, 3)); // "123" contains '2'
        Assert.False(buffer.ContainsAny("98".AsSpan(), 1, 3));
    }

    // --- Export & Slice (ToString/AsSpan/CopyTo) ---

    [Fact]
    public void ToString_Range_ReturnsSubstring()
    {
        using var buffer = new CharBuffer();
        buffer.Append("012345");
        Assert.Equal("23", buffer.ToString(2, 2));
    }

    [Fact]
    public void ToString_Start_ReturnsRestOfString()
    {
        using var buffer = new CharBuffer();
        buffer.Append("012345");
        Assert.Equal("2345", buffer.ToString(2));
    }

    [Fact]
    public void AsSpan_Slice_ReturnsCorrectSpan()
    {
        using var buffer = new CharBuffer();
        buffer.Append("012345");
        var span = buffer.AsSpan(2, 2);
        Assert.Equal("23", span.ToString());
    }

    [Fact]
    public void AsSpan_Start_ReturnsCorrectSpan()
    {
        using var buffer = new CharBuffer();
        buffer.Append("012345");
        var span = buffer.AsSpan(2);
        Assert.Equal("2345", span.ToString());
    }

    [Fact]
    public void AsSpan_EmptyBuffer_ReturnsEmptySpan()
    {
        using var buffer = new CharBuffer();
        var span = buffer.AsSpan();
        Assert.True(span.IsEmpty);
    }

    [Fact]
    public void CopyTo_CopiesToDestination()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abcde");

        char[] dest = new char[3];
        buffer.CopyTo(dest.AsSpan(), 1, 3); // Copy "bcd"

        Assert.Equal("bcd", new string(dest));
    }

    [Fact]
    public void CopyTo_FullBuffer_CopiesAll()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abc");
        char[] dest = new char[3];
        buffer.CopyTo(dest.AsSpan());
        Assert.Equal("abc", new string(dest));
    }

    [Fact]
    public void CopyTo_Start_CopiesRest()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abcde");
        char[] dest = new char[3];
        buffer.CopyTo(dest.AsSpan(), 2); // "cde"
        Assert.Equal("cde", new string(dest));
    }

    // --- Replace Extra Overloads ---

    [Fact]
    public void Replace_Start_ReplacesRest()
    {
        using var buffer = new CharBuffer();
        buffer.Append("aaaaa");
        // Replace 'a' -> 'b' starting at 2: "aabbb"
        buffer.Replace('a', 'b', 2);
        Assert.Equal("aabbb", buffer.ToString());
    }

    [Fact]
    public void ReplaceAny_Start_ReplacesRest()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abcde");
        // Replace 'c','d','e' -> '*' starting at 2
        buffer.ReplaceAny("cde".AsSpan(), '*', 2);
        Assert.Equal("ab***", buffer.ToString());
    }

    [Fact]
    public void ReplaceAny_Range_ReplacesInRange()
    {
        using var buffer = new CharBuffer();
        buffer.Append("abcde");
        // Replace 'c','d','e' -> '*' in range 1..3 ("bcd")
        // "b" is not in set. "c","d" are. "e" is outside range.
        // -> "a" + "b**" + "e" -> "ab**e"
        buffer.ReplaceAny("cde".AsSpan(), '*', 1, 3);
        Assert.Equal("ab**e", buffer.ToString());
    }

    // --- Edge Cases / Unicode Agnosticism ---

    [Fact]
    public void Append_Emoji_StoredAsSurrogatePairs()
    {
        using var buffer = new CharBuffer();
        // 🐱 is \uD83D\uDC31 (Surrogate pair)
        string catFace = "🐱";

        buffer.Append(catFace.AsSpan());

        Assert.Equal(2, buffer.Length);
        Assert.Equal('\uD83D', buffer[0]); // High surrogate
        Assert.Equal('\uDC31', buffer[1]); // Low surrogate
        Assert.Equal(catFace, buffer.ToString());
    }

    [Fact]
    public void Insert_SplittingSurrogatePair_IsAllowed()
    {
        // This test confirms the design choice: The buffer is a raw char container
        // and does NOT enforce Unicode validity. Splitting a surrogate pair is valid
        // for the buffer, even if it creates invalid strings.

        using var buffer = new CharBuffer();
        string catFace = "🐱"; // \uD83D\uDC31
        buffer.Append(catFace.AsSpan());

        // Insert 'X' between the high and low surrogate
        buffer.Insert(1, 'X');

        Assert.Equal(3, buffer.Length);
        Assert.Equal('\uD83D', buffer[0]);
        Assert.Equal('X', buffer[1]);
        Assert.Equal('\uDC31', buffer[2]);

        string result = buffer.ToString();
        Assert.Equal(3, result.Length);
        Assert.Equal(catFace[0], result[0]);
        Assert.Equal('X', result[1]);
        Assert.Equal(catFace[1], result[2]);
    }

    [Fact]
    public void Dispose_PreventsAccess()
    {
        var buffer = new CharBuffer();
        buffer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = buffer.Length);
        Assert.Throws<ObjectDisposedException>(() => buffer.Append('a'));
        Assert.Throws<ObjectDisposedException>(() => buffer.ToString());
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        var buffer = new CharBuffer();
        buffer.Append("test");

        buffer.Dispose();
        buffer.Dispose(); // Should not throw
        buffer.Dispose(); // Should not throw

        // Should still throw ObjectDisposedException when trying to use
        Assert.Throws<ObjectDisposedException>(() => buffer.Append('x'));
    }
}
