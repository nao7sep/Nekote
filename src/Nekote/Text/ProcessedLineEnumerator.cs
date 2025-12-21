using System;
using System.Text;

namespace Nekote.Text;

/// <summary>
/// Supports iterating over and processing lines in a text span according to whitespace and blank line handling options.
/// </summary>
public ref struct ProcessedLineEnumerator
{
    private enum Phase
    {
        Leading,
        Content,
        Trailing,
        Done
    }

    private Phase _phase;
    private ReadOnlySpan<char> _remaining; // Current slice being iterated in the current phase
    
    private readonly ReadOnlySpan<char> _contentSpan;
    private readonly ReadOnlySpan<char> _trailingSpan;
    
    private readonly LineProcessingOptions _options;
    private readonly StringBuilder _builder;
    private ReadOnlySpan<char> _current;

    // Tracks if we have already yielded a blank line in the current sequence of blank lines.
    // Used for InnerBlankLineHandling.Collapse.
    private bool _hasYieldedBlankInSequence;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessedLineEnumerator"/> struct.
    /// </summary>
    /// <param name="text">The text to iterate over.</param>
    /// <param name="options">The options defining how whitespace and blank lines should be handled.</param>
    /// <param name="builder">A StringBuilder used as a buffer for line processing. If null, a new one is created.</param>
    public ProcessedLineEnumerator(ReadOnlySpan<char> text, LineProcessingOptions options, StringBuilder? builder)
    {
        _options = options;
        _builder = builder ?? new StringBuilder();
        _current = default;
        _hasYieldedBlankInSequence = false;

        // 1. Analyze Structure
        LineProcessor.SplitIntoSections(text, out var leading, out _contentSpan, out var trailing);

        // 2. Setup Trailing Phase (Optimization: clear it now if we know we'll ignore it)
        if (_options.TrailingBlankLineHandling == TrailingBlankLineHandling.Remove)
        {
            _trailingSpan = ReadOnlySpan<char>.Empty;
        }
        else
        {
            _trailingSpan = trailing;
        }

        // 3. Initialize Starting Phase
        if (_options.LeadingBlankLineHandling == LeadingBlankLineHandling.Remove)
        {
            // Skip leading, start with Content
            _phase = Phase.Content;
            _remaining = _contentSpan;
        }
        else
        {
            _phase = Phase.Leading;
            _remaining = leading;
        }
    }

    /// <summary>
    /// Gets the processed line at the current position of the enumerator.
    /// </summary>
    public ReadOnlySpan<char> Current => _current;

    /// <summary>
    /// Returns the enumerator itself.
    /// </summary>
    /// <returns>The enumerator instance.</returns>
    public ProcessedLineEnumerator GetEnumerator() => this;

    /// <summary>
    /// Advances the enumerator to the next line, handling blank line logic and whitespace processing.
    /// </summary>
    /// <returns><c>true</c> if the enumerator was successfully advanced; <c>false</c> if no more lines remain.</returns>
    public bool MoveNext()
    {
        while (_phase != Phase.Done)
        {
            // Try to read a line from the current phase
            if (LineProcessor.TryReadLine(_remaining, out var rawLine, out var nextRemaining))
            {
                // Advance internal pointer
                _remaining = nextRemaining;

                // Process the line based on the current phase
                switch (_phase)
                {
                    case Phase.Leading:
                        // In Leading phase, we only have blank lines (by definition of SplitIntoSections).
                        // If we are here, LeadingBlankLineHandling is Preserve (Remove is handled in ctor).
                        _current = rawLine; // Return raw blank line
                        return true;

                    case Phase.Content:
                        bool isBlank = LineProcessor.IsBlank(rawLine);

                        if (isBlank)
                        {
                            // Inner Blank Line Logic
                            if (_options.InnerBlankLineHandling == InnerBlankLineHandling.Remove)
                            {
                                continue; // Skip and read next
                            }
                            else if (_options.InnerBlankLineHandling == InnerBlankLineHandling.Collapse)
                            {
                                if (_hasYieldedBlankInSequence)
                                {
                                    continue; // Already yielded one, skip this one
                                }
                                _hasYieldedBlankInSequence = true;
                                _current = rawLine; // Yield blank
                                return true;
                            }
                            else // Preserve
                            {
                                _current = rawLine;
                                return true;
                            }
                        }
                        else
                        {
                            // Visible Line Logic
                            _hasYieldedBlankInSequence = false; // Reset collapse tracker
                            _current = LineProcessor.ProcessLine(rawLine, _options, _builder);
                            return true;
                        }

                    case Phase.Trailing:
                        // In Trailing phase, we only have blank lines.
                        // If we are here, TrailingBlankLineHandling is Preserve.
                        _current = rawLine;
                        return true;
                }
            }
            else
            {
                // Current phase exhausted. Transition to next phase.
                AdvancePhase();
            }
        }

        return false;
    }

    private void AdvancePhase()
    {
        switch (_phase)
        {
            case Phase.Leading:
                _phase = Phase.Content;
                _remaining = _contentSpan;
                break;
            case Phase.Content:
                _phase = Phase.Trailing;
                _remaining = _trailingSpan;
                break;
            case Phase.Trailing:
                _phase = Phase.Done;
                _remaining = default;
                break;
        }
    }
}