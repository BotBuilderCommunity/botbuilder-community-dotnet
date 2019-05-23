using System;

namespace Bot.Builder.Community.Dialogs.Luis
{
    [Serializable]
    public abstract class AttributeString : Attribute, IEquatable<AttributeString>
    {
        protected abstract string Text { get; }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Text})";
        }

        bool IEquatable<AttributeString>.Equals(AttributeString other)
        {
            return other != null
                && object.Equals(this.Text, other.Text);
        }

        public override bool Equals(object other)
        {
            return base.Equals(other as AttributeString);
        }

        public override int GetHashCode()
        {
            return this.Text.GetHashCode();
        }
    }
}
