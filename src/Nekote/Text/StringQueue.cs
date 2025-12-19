namespace Nekote.Text;

/// <summary>
/// A queue for materializing and storing ReadOnlySpan&lt;char&gt; as strings in sequential order.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Problem this solves:</strong> When processing text with ReadOnlySpan&lt;char&gt;,
/// spans are ephemeral - they may point to temporary buffers that get overwritten or to
/// memory that becomes invalid after the current iteration. If you need to preserve span
/// content for later sequential access, you must materialize it to strings.
/// </para>
/// <para>
/// <strong>Why a queue:</strong> This class is optimized for the common pattern of sequential
/// processing where spans are encountered, buffered, and then consumed in FIFO order.
/// Unlike List&lt;string&gt; with RemoveAt(0), Queue&lt;string&gt; provides O(1) dequeue operations.
/// </para>
/// <para>
/// <strong>Use cases:</strong>
/// - Lookahead scenarios: buffer spans until you determine how to process them
/// - Blank line handling: queue blank lines until you know if they're consecutive or trailing
/// - Token buffering: queue parsed tokens as strings for later processing
/// - Any scenario where spans must be preserved for sequential future use
/// </para>
/// <para>
/// <strong>Example - Blank Line Handling:</strong>
/// <code>
/// var queue = new StringQueue();
///
/// foreach (var line in text.EnumerateLines())
/// {
///     if (line.IsWhiteSpace())
///     {
///         // Don't know yet if this is trailing - buffer it
///         queue.Enqueue(line);
///     }
///     else
///     {
///         // Content found - queued blanks were consecutive, not trailing
///         while (queue.TryDequeue(out var blank))
///         {
///             ProcessBlankLine(blank);
///         }
///         ProcessContentLine(line);
///     }
/// }
///
/// // EOF - any remaining queued blanks were trailing
/// if (shouldPreserveTrailing)
/// {
///     while (queue.TryDequeue(out var blank))
///         ProcessBlankLine(blank);
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class StringQueue : IEnumerable<string>
{
    private readonly Queue<string> _queue;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringQueue"/> class that is empty.
    /// </summary>
    public StringQueue()
    {
        _queue = new Queue<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringQueue"/> class with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the queue can contain.</param>
    /// <exception cref="ArgumentOutOfRangeException">Capacity is less than zero.</exception>
    public StringQueue(int capacity)
    {
        _queue = new Queue<string>(capacity);
    }

    /// <summary>
    /// Gets the number of strings currently in the queue.
    /// </summary>
    public int Count => _queue.Count;

    /// <summary>
    /// Gets a value indicating whether the queue is empty.
    /// </summary>
    public bool IsEmpty => _queue.Count == 0;

    /// <summary>
    /// Enqueues a span by materializing it to a string.
    /// </summary>
    /// <param name="span">The span to materialize and enqueue.</param>
    /// <remarks>
    /// The span is immediately converted to a string via <see cref="ReadOnlySpan{T}.ToString()"/>,
    /// which performs a copy. This ensures the content is preserved even if the span's underlying
    /// memory is reused or becomes invalid.
    /// </remarks>
    public void Enqueue(ReadOnlySpan<char> span)
    {
        _queue.Enqueue(span.ToString());
    }

    /// <summary>
    /// Enqueues a string directly.
    /// </summary>
    /// <param name="value">The string to enqueue.</param>
    /// <exception cref="ArgumentNullException">Value is null.</exception>
    public void Enqueue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _queue.Enqueue(value);
    }

    /// <summary>
    /// Removes and returns the string at the beginning of the queue.
    /// </summary>
    /// <returns>The string that is removed from the beginning of the queue.</returns>
    /// <exception cref="InvalidOperationException">The queue is empty.</exception>
    public string Dequeue()
    {
        return _queue.Dequeue();
    }

    /// <summary>
    /// Returns the string at the beginning of the queue without removing it.
    /// </summary>
    /// <returns>The string at the beginning of the queue.</returns>
    /// <exception cref="InvalidOperationException">The queue is empty.</exception>
    public string Peek()
    {
        return _queue.Peek();
    }

    /// <summary>
    /// Attempts to peek at the string at the beginning of the queue without removing it.
    /// </summary>
    /// <param name="value">
    /// When this method returns true, contains the string at the beginning of the queue;
    /// otherwise, null.
    /// </param>
    /// <returns>
    /// <c>true</c> if a string was successfully retrieved;
    /// <c>false</c> if the queue is empty.
    /// </returns>
    public bool TryPeek(out string? value)
    {
        return _queue.TryPeek(out value);
    }

    /// <summary>
    /// Attempts to dequeue the next string from the queue.
    /// </summary>
    /// <param name="value">
    /// When this method returns true, contains the next string in the queue;
    /// otherwise, null.
    /// </param>
    /// <returns>
    /// <c>true</c> if a string was successfully dequeued;
    /// <c>false</c> if the queue is empty.
    /// </returns>
    public bool TryDequeue(out string? value)
    {
        if (_queue.Count == 0)
        {
            value = null;
            return false;
        }

        value = _queue.Dequeue();
        return true;
    }

    /// <summary>
    /// Removes all strings from the queue.
    /// </summary>
    public void Clear()
    {
        _queue.Clear();
    }

    /// <summary>
    /// Ensures that the queue can hold at least the specified number of elements without resizing.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <remarks>
    /// If the current capacity is less than the specified capacity, the internal capacity is increased.
    /// This can improve performance when you know how many items you'll be adding.
    /// </remarks>
    public void EnsureCapacity(int capacity)
    {
        _queue.EnsureCapacity(capacity);
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the queue if that number is less than 90% of current capacity.
    /// </summary>
    /// <remarks>
    /// This method can be used to minimize a queue's memory overhead if no new elements will be added.
    /// </remarks>
    public void TrimExcess()
    {
        _queue.TrimExcess();
    }

    /// <summary>
    /// Copies the queue elements to a new array.
    /// </summary>
    /// <returns>An array containing copies of the strings in the queue.</returns>
    public string[] ToArray()
    {
        return _queue.ToArray();
    }

    /// <summary>
    /// Copies the queue elements to an existing array, starting at the specified array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">Array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Array index is less than zero.</exception>
    /// <exception cref="ArgumentException">
    /// The number of elements in the source queue is greater than the available space from arrayIndex to the end of the destination array.
    /// </exception>
    public void CopyTo(string[] array, int arrayIndex)
    {
        _queue.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the queued strings.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    /// <remarks>
    /// The enumerator does not remove items from the queue. Use <see cref="Dequeue"/>
    /// or <see cref="TryDequeue"/> to consume items.
    /// </remarks>
    public IEnumerator<string> GetEnumerator()
    {
        return _queue.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the queued strings.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
