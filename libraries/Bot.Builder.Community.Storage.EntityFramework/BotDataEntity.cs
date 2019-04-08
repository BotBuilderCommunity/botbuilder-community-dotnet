using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.Builder.Community.Storage.EntityFramework
{
    /// <summary>
    /// BotDataEntity representing one bot data record.
    /// </summary>
    [Table("BotDataEntity")]
    public class BotDataEntity
    {
        /// <summary>
        /// Constructor for BotDataEntity
        /// </summary>
        /// <remarks>
        /// Sets Timestamp to DateTimeOffset.UtfcNow
        /// </remarks>
        public BotDataEntity()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Gets or sets the auto-generated Id/Key.
        /// </summary>
        /// <value>
        /// The database generated Id/Key.
        /// </value>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the un-sanitized Id/Key.
        /// </summary>
        /// <value>
        /// The un-sanitized Id/Key.
        /// </value>
        [MaxLength(1024)]
        public string RealId { get; set; }

        /// <summary>
        /// Gets or sets the persisted object's state.
        /// </summary>
        /// <value>
        /// The persisted object's state.
        /// </value>
        [Column(TypeName = "nvarchar(MAX)")]
        public string Document { get; set; }

        /// <summary>
        /// Gets or sets the current timestamp.
        /// </summary>
        /// <value>
        /// The current timestamp.
        /// </value>
        [Required]
        [Timestamp]
        public DateTimeOffset Timestamp { get; set; }
    }

}
