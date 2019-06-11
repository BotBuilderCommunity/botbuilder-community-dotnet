namespace Luis_Dialog_Sample.Models
{
    using System;

    /// <summary>
    /// This is the Note class.  Notes are stored in a static dictionary on the SimpleNoteDialog.
    /// </summary>
    public sealed class Note : IEquatable<Note>
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public override string ToString()
        {
            return $"[{this.Title} : {this.Text}]";
        }

        public bool Equals(Note other)
        {
            return other != null
                && this.Text == other.Text
                && this.Title == other.Title;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as Note);
        }

        public override int GetHashCode()
        {
            return this.Title.GetHashCode();
        }
    }
}
