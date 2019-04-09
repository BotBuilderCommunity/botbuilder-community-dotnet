using System;
using Microsoft.EntityFrameworkCore;

namespace Bot.Builder.Community.Storage.EntityFramework
{
    /// <summary>
    /// DbContext for BotDataEntitys
    /// </summary>
    public class BotDataContext : DbContext
    {
        private string _connectionString;

        /// <summary>
        /// Constructor for BotDataContext receiving connectionString
        /// </summary>
        /// <param name="connectionString">Connection string to use when configuring the options during <see cref="OnConfiguring"/></param>
        public BotDataContext(string connectionString)
            : base()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        /// <summary>
        /// Constructor for BotDataContext receiving DBContextOptions
        /// </summary>
        /// <param name="options">Options to use for configuration.</param>
        public BotDataContext(DbContextOptions<BotDataContext> options)
            : base(options)
        { }

        /// <summary>
        /// BotDataEntity records
        /// </summary>
        public virtual DbSet<BotDataEntity> BotDataEntity { get; set; }

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
            builder.Entity<BotDataEntity>(entity =>
            {
                entity.ToTable(nameof(BotDataEntity));
                entity.HasIndex(e => e.RealId);
                entity.HasKey(e => e.Id);
            });
        }

    }
}
