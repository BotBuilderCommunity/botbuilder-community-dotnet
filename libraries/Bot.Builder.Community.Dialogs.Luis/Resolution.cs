using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Bot.Builder.Community.Dialogs.Luis
{
    public abstract class Resolution
    {
        public override string ToString()
        {
            var builder = new StringBuilder();
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                if (value != null)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(",");
                    }

                    builder.Append(property.Name);
                    builder.Append("=");
                    builder.Append(value);
                }
            }

            return builder.ToString();
        }
    }
}