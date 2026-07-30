using System.Buffers;
using System.Runtime.CompilerServices;

namespace FFCEI.DotNetStack.Framework;

public struct MultipleStringsReadOnlySpan
    : IDisposable, IEquatable<MultipleStringsReadOnlySpan>
{
    private sealed class BuffersStoage
        : IDisposable
    {
        private ReadOnlyMemory<char>[]? _blocks;
        private int[]? _starts;
        private char[][]? _rentedBuffers;
        private int _rentedBuffersCount;
        private int _capacity;
        private int _cursorBlock;
        private bool _disposed;

        public int Length { get; private set; }
        public int Count { get; private set; }

        public void Add(ReadOnlyMemory<char> memory)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (Count >= _capacity)
            {
                EnsureCapacity();
            }

            _starts![Count] = Length;
            _blocks![Count] = memory;
            Length += memory.Length;
            Count++;
        }

        public void AddPooledCopy(ReadOnlySpan<char> block)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var pooled = ArrayPool<char>.Shared.Rent(block.Length);

            block.CopyTo(pooled);

            TrackRentedBuffer(pooled);

            Add(pooled.AsMemory(0, block.Length));
        }

        public char CharAt(int index)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var starts = _starts!;
            var blocks = _blocks!;
            var count = Count;
            var cursor = _cursorBlock;

            if ((uint)cursor < (uint)count)
            {
                var start = starts[cursor];
                var end = (cursor + 1 < count) ? starts[cursor + 1] : Length;

                if ((uint)(index - start) < (uint)(end - start))
                {
                    return blocks[cursor].Span[index - start];
                }

                var next = cursor + 1;

                if (next < count)
                {
                    var nStart = starts[next];
                    var nEnd = (next + 1 < count) ? starts[next + 1] : Length;

                    if ((uint)(index - nStart) < (uint)(nEnd - nStart))
                    {
                        _cursorBlock = next;

                        return blocks[next].Span[index - nStart];
                    }
                }
            }

            var blockIndex = FindStringBlockIndex(index);

            _cursorBlock = blockIndex;

            return blocks[blockIndex].Span[index - starts[blockIndex]];
        }

        public void CopyTo(Span<char> destination)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var blocks = _blocks!;
            var count = Count;
            var offset = 0;

            for (var i = 0; i < count; i++)
            {
                var span = blocks[i].Span;

                span.CopyTo(destination[offset..]);

                offset += span.Length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindStringBlockIndex(int index)
        {
            var count = Count;

            if (count == 1)
            {
                return 0;
            }

            var starts = _starts!;
            var low = 0;
            var high = count - 1;
            var result = 0;

            while (low <= high)
            {
                var mid = (int)(((uint)low + (uint)high) >> 1);

                if (starts[mid] <= index)
                {
                    result = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return result;
        }

        private void EnsureCapacity()
        {
            var oldBlocks = _blocks;
            var oldStarts = _starts;

            var newSize = _capacity == 0 ? 4 : _capacity * 2;
            var newBlocks = ArrayPool<ReadOnlyMemory<char>>.Shared.Rent(newSize);
            var newStarts = ArrayPool<int>.Shared.Rent(newSize);

            if (Count > 0)
            {
                Array.Copy(oldBlocks!, newBlocks, Count);
                Array.Copy(oldStarts!, newStarts, Count);
            }

            if (oldBlocks is not null)
            {
                ArrayPool<ReadOnlyMemory<char>>.Shared.Return(oldBlocks, clearArray: true);
            }

            if (oldStarts is not null)
            {
                ArrayPool<int>.Shared.Return(oldStarts);
            }

            _blocks = newBlocks;
            _starts = newStarts;
            _capacity = Math.Min(newBlocks.Length, newStarts.Length);
        }

        private void TrackRentedBuffer(char[] buffer)
        {
            var arr = _rentedBuffers;

            if (arr is null || _rentedBuffersCount >= arr.Length)
            {
                var oldCap = arr?.Length ?? 0;
                var newSize = oldCap == 0 ? 4 : oldCap * 2;
                var newArr = ArrayPool<char[]>.Shared.Rent(newSize);

                if (_rentedBuffersCount > 0)
                {
                    Array.Copy(arr!, newArr, _rentedBuffersCount);
                }

                if (arr is not null)
                {
                    ArrayPool<char[]>.Shared.Return(arr, clearArray: true);
                }

                _rentedBuffers = newArr;

                arr = newArr;
            }

            arr[_rentedBuffersCount++] = buffer;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            var rented = _rentedBuffers;

            if (rented is not null)
            {
                var n = _rentedBuffersCount;

                for (var i = 0; i < n; i++)
                {
                    var b = rented[i];

                    if (b is not null)
                    {
                        ArrayPool<char>.Shared.Return(b);
                    }
                }

                ArrayPool<char[]>.Shared.Return(rented, clearArray: true);

                _rentedBuffers = null;
                _rentedBuffersCount = 0;
            }

            if (_blocks is not null)
            {
                ArrayPool<ReadOnlyMemory<char>>.Shared.Return(_blocks, clearArray: true);

                _blocks = null;
            }

            if (_starts is not null)
            {
                ArrayPool<int>.Shared.Return(_starts);

                _starts = null;
            }

            _capacity = 0;
            _cursorBlock = 0;

            Count = 0;
            Length = 0;

            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }

    private BuffersStoage? _storage;
    private bool _disposed;

    public readonly int Length => _storage?.Length ?? 0;
    public readonly int Count => _storage?.Count ?? 0;

    private BuffersStoage EnsureStorage()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _storage ??= new BuffersStoage();
    }

    public void Add(string? block)
    {
        var storage = EnsureStorage();

        if (string.IsNullOrEmpty(block))
        {
            return;
        }

        storage.Add(block.AsMemory());
    }

    public void Add(ReadOnlyMemory<char> block)
    {
        var storage = EnsureStorage();

        if (block.IsEmpty)
        {
            return;
        }

        storage.Add(block);
    }

    public void Add(char[]? block, bool copy = true)
    {
        var storage = EnsureStorage();

        if (block is null || block.Length == 0)
        {
            return;
        }

        if (copy)
        {
            storage.AddPooledCopy(block);
        }
        else
        {
            storage.Add(block.AsMemory());
        }
    }

    public void Add(ReadOnlySpan<char> block)
    {
        var storage = EnsureStorage();

        if (block.IsEmpty)
        {
            return;
        }

        storage.AddPooledCopy(block);
    }

    public readonly char this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if ((uint)index >= (uint)Length)
            {
                ThrowIndexOutOfRangeException();
            }

            return _storage!.CharAt(index);
        }
    }

    public readonly void CopyTo(Span<char> destination)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (Length == 0)
        {
            return;
        }

        if (destination.Length < Length)
        {
            throw new ArgumentException("Destination span is too short.", nameof(destination));
        }

        _storage!.CopyTo(destination);
    }

    public override readonly string ToString()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (Length == 0)
        {
            return string.Empty;
        }

        return string.Create(Length, this, static (span, state) => state.CopyTo(span));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _storage.DisposeSafelyIfNeeded();
        _storage = null;

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowIndexOutOfRangeException()
#pragma warning disable CA2201 // Do not raise reserved exception types
        => throw new IndexOutOfRangeException();
#pragma warning restore CA2201 // Do not raise reserved exception types

    public readonly bool Equals(MultipleStringsReadOnlySpan other)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(other._disposed, other);

        if (Length != other.Length)
        {
            return false;
        }

        for (var index = 0; index < Length; index++)
        {
            if (this[index] != other[index])
            {
                return false;
            }
        }

        return true;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is MultipleStringsReadOnlySpan span && Equals(span);
    }

    public override readonly int GetHashCode()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var hash = new HashCode();

        for (var index = 0; index < Length; index++)
        {
            hash.Add(this[index]);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(MultipleStringsReadOnlySpan left, MultipleStringsReadOnlySpan right)
    {
        ObjectDisposedException.ThrowIf(left._disposed, left);
        ObjectDisposedException.ThrowIf(right._disposed, right);

        return left.Equals(right);
    }

    public static bool operator !=(MultipleStringsReadOnlySpan left, MultipleStringsReadOnlySpan right)
    {
        ObjectDisposedException.ThrowIf(left._disposed, left);
        ObjectDisposedException.ThrowIf(right._disposed, right);

        return !(left == right);
    }
}
