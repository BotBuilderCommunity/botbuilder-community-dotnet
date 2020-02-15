using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Cards.Management
{
    internal class PayloadIdComparer : EqualityComparer<PayloadId>
    {
        internal static PayloadIdComparer Instance { get; } = new PayloadIdComparer();

        public override bool Equals(PayloadId x, PayloadId y)
        {
            return x.Id == y.Id && x.Type == y.Type;
        }

        public override int GetHashCode(PayloadId obj)
        {
            return obj.Id.GetHashCode() + (int)obj.Type;
        }
    }
}
