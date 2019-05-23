using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Dialogs.Luis
{
    public struct Range<T> : IEquatable<Range<T>>, IComparable<Range<T>>
        where T : IEquatable<T>, IComparable<T>
    {
        public Range(T start, T after)
        {
            this.Start = start;
            this.After = after;
        }

        public T Start { get; }

        public T After { get; }

        public override bool Equals(object other)
        {
            return other is Range<T> && this.Equals((Range<T>)other);
        }

        public override int GetHashCode()
        {
            return this.Start.GetHashCode() ^ this.After.GetHashCode();
        }

        public override string ToString()
        {
            return $"[{this.Start}, {this.After})";
        }

        public bool Equals(Range<T> other)
        {
            return this.Start.Equals(other.Start) && this.After.Equals(other.After);
        }

        public int CompareTo(Range<T> other)
        {
            if (this.After.CompareTo(other.Start) < 0)
            {
                return -1;
            }
            else if (other.After.CompareTo(this.Start) > 0)
            {
                return +1;
            }
            else
            {
                return 0;
            }
        }
    }

    public static partial class Range
    {
        public static Range<T> From<T>(T start, T after)
             where T : IEquatable<T>, IComparable<T>
        {
            return new Range<T>(start, after);
        }
    }
}
