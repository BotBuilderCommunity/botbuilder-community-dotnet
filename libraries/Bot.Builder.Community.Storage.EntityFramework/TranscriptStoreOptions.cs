using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Storage.EntityFramework;


public class TranscriptStoreOptions
{

    /// <summary>
    /// Gets or sets the connection string to use while creating TranscriptContext.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the total records to return from the store per page.
    /// </summary>
    /// <remarks>
    /// Default 20
    /// </remarks>
    public int PageSize => 20;

}
