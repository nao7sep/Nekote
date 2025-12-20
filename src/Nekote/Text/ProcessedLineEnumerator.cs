using System.Collections.Generic;

namespace Nekote.Text;

/// <summary>
/// Enumerates lines with configurable processing (trimming, whitespace handling, blank line handling).
/// </summary>
/// <remarks>
/// This enumerator wraps a <see cref="LineEnumerator"/> and applies processing according to
/// <see cref="LineProcessingOptions"/>. Uses <see cref="CharBuffer"/> for inline whitespace
/// transformations and <see cref="Queue{T}"/> for blank line lookahead/buffering.
/// <para>
/// The returned spans are only valid until the next call to <see cref="MoveNext"/>. If data
/// needs to persist, call <c>ToString()</c> on the span to materialize it as a string.
/// </para>
/// </remarks>
public ref struct ProcessedLineEnumerator
{
    private LineEnumerator _lines;
    private readonly LineProcessingOptions _options;
    private CharBuffer _charBuffer;
    private Queue<string>? _blankQueue;
    private ReadOnlySpan<char> _current;

    // State for blank line handling
    private bool _hasEmittedContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessedLineEnumerator"/> struct.
    /// </summary>
    /// <param name="lines">The line enumerator to process.</param>
    /// <param name="options">The processing options to apply.</param>
    public ProcessedLineEnumerator(LineEnumerator lines, LineProcessingOptions options)
    {
        _lines = lines;
        _options = options;
        _charBuffer = new CharBuffer();
        _blankQueue = null;
        _current = default;
        _hasEmittedContent = false;
    }

    /// <summary>
    /// Gets the current processed line as a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <remarks>
    /// The returned span is only valid until the next call to <see cref="MoveNext"/>.
    /// If inline whitespace processing is active, the span points to a buffer that is
    /// reused across iterations.
    /// </remarks>
    public ReadOnlySpan<char> Current => _current;

    /// <summary>
    /// Advances the enumerator to the next processed line.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the enumerator successfully advanced to the next line;
    /// <c>false</c> if the enumerator has reached the end of the text.
    /// </returns>
    public bool MoveNext()
    {
        // First, check if we have queued lines to emit (blanks or content buffered during lookahead)
        if (_blankQueue != null && _blankQueue.TryDequeue(out string? queuedLine))
        {
            _current = queuedLine.AsSpan();
            return true;
        }

        while (true)
        {
            // Get next raw line
            if (!_lines.MoveNext())
            {
                // EOF - no more lines
                _current = default;
                return false;
            }

            ReadOnlySpan<char> line = _lines.Current;

            // Apply line processing
            line = ProcessLine(line);

            // Determine if line is blank
            bool isBlank = line.IsEmpty || line.IsWhiteSpace();

            // Handle leading blank lines (before first visible content)
            if (isBlank && !_hasEmittedContent)
            {
                if (_options.LeadingBlanks == LeadingBlankLinesMode.Omit)
                {
                    continue; // Skip leading blanks
                }
                // Leading blank, preserve mode - emit it
                _current = line;
                return true;
            }

            // Handle blank line after first visible content - ALWAYS queue and read ahead
            if (isBlank && _hasEmittedContent)
            {
                // We can't know if this is consecutive or trailing until we read ahead
                _blankQueue ??= new Queue<string>();
                _blankQueue.Enqueue(line.ToString());

                // Read ahead until we find content or EOF
                while (_lines.MoveNext())
                {
                    ReadOnlySpan<char> nextLine = _lines.Current;
                    nextLine = ProcessLine(nextLine);
                    bool nextIsBlank = nextLine.IsEmpty || nextLine.IsWhiteSpace();

                    if (nextIsBlank)
                    {
                        // Another blank - queue it
                        _blankQueue.Enqueue(nextLine.ToString());
                    }
                    else
                    {
                        // Found content after blank(s) - these are consecutive blanks, not trailing
                        // Handle based on consecutive blanks mode
                        if (_options.ConsecutiveBlanks == ConsecutiveBlankLinesMode.Omit)
                        {
                            // Discard all queued blanks, emit the content
                            _blankQueue.Clear();
                            _current = nextLine;
                            return true;
                        }
                        else if (_options.ConsecutiveBlanks == ConsecutiveBlankLinesMode.Collapse)
                        {
                            // Keep only one blank, emit it, queue content for next iteration
                            string? singleBlank = null;
                            if (_blankQueue.TryDequeue(out singleBlank))
                            {
                                _blankQueue.Clear(); // Discard remaining blanks
                                _blankQueue.Enqueue(nextLine.ToString()); // Queue content line
                                _current = singleBlank.AsSpan();
                                return true;
                            }
                        }
                        else // Preserve mode
                        {
                            // Keep all queued blanks, queue content for later
                            _blankQueue.Enqueue(nextLine.ToString());
                            // Will emit from queue on next MoveNext call
                            if (_blankQueue.TryDequeue(out string? firstBlank))
                            {
                                _current = firstBlank.AsSpan();
                                return true;
                            }
                        }
                    }
                }

                // EOF reached while reading ahead - queued blanks are trailing blanks
                if (_options.TrailingBlanks == TrailingBlankLinesMode.Preserve)
                {
                    // Emit queued blanks
                    if (_blankQueue.TryDequeue(out string? trailingBlank))
                    {
                        _current = trailingBlank.AsSpan();
                        return true;
                    }
                }
                else // Omit trailing blanks
                {
                    // Discard queued blanks
                    _blankQueue.Clear();
                }

                // No more content
                _current = default;
                return false;
            }

            // Non-blank line (visible content)
            if (!isBlank)
            {
                _hasEmittedContent = true;
                _current = line;
                return true;
            }
        }
    }

    /// <summary>
    /// Returns this enumerator instance (enables foreach support).
    /// </summary>
    /// <returns>This enumerator instance.</returns>
    public ProcessedLineEnumerator GetEnumerator() => this;

    /// <summary>
    /// Releases resources used by this enumerator, including the CharBuffer.
    /// </summary>
    public void Dispose()
    {
        _charBuffer.Dispose();
    }

    private ReadOnlySpan<char> ProcessLine(ReadOnlySpan<char> line)
    {
        // Apply line start whitespace handling
        if (_options.LineStart == LineStartWhitespaceMode.Trim)
        {
            line = line.TrimStart();
        }

        // Apply inline whitespace handling
        if (_options.InlineWhitespace != InlineWhitespaceMode.Preserve)
        {
            line = ProcessInlineWhitespace(line);
        }

        // Apply line end whitespace handling
        if (_options.LineEnd == LineEndWhitespaceMode.Trim)
        {
            line = line.TrimEnd();
        }

        return line;
    }

    private ReadOnlySpan<char> ProcessInlineWhitespace(ReadOnlySpan<char> line)
    {
        if (line.IsEmpty)
            return line;

        // Clear buffer from any previous use
        _charBuffer.Clear();

        if (_options.InlineWhitespace == InlineWhitespaceMode.Collapse)
        {
            bool inWhitespace = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (char.IsWhiteSpace(c))
                {
                    if (!inWhitespace)
                    {
                        // First whitespace in sequence - emit single ASCII space
                        _charBuffer.Append(' ');
                        inWhitespace = true;
                    }
                    // Skip additional whitespace characters
                }
                else
                {
                    _charBuffer.Append(c);
                    inWhitespace = false;
                }
            }
        }
        else if (_options.InlineWhitespace == InlineWhitespaceMode.Strip)
        {
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (!char.IsWhiteSpace(c))
                {
                    _charBuffer.Append(c);
                }
            }
        }

        return _charBuffer.AsSpan();
    }
}
