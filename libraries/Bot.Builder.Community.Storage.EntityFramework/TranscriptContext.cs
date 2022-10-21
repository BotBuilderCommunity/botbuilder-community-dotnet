using Bot.Builder.Community.Storage.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Storage.EntityFramework;

public class TranscriptContext : DbContext
{
    private string _connectionString;

    /// <summary>
    /// Constructor for TranscriptContext receiving connectionString
    /// </summary>
    /// <param name="connectionString">Connection string to use when configuring the options during <see cref="OnConfiguring"/></param>
    public TranscriptContext(string connectionString)
        : base()
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    /// <summary>
    /// TranscriptEntity records
    /// </summary>
    public DbSet<TranscriptEntity>? Transcript { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TranscriptEntity>(entity =>
        {
            entity.ToTable(nameof(TranscriptEntity));
            entity.HasIndex(e => e.Conversation);
            entity.HasIndex(e => e.Channel);
            entity.HasIndex(e => new { e.Channel, e.Conversation });
            entity.HasKey(e => e.Id);
        });
    }



}
