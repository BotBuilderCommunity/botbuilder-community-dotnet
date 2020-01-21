using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards
{
    public class PayloadIdOptions
    {
        private readonly IDictionary<PayloadIdType, string> _ids = new Dictionary<PayloadIdType, string>();

        public PayloadIdOptions(bool overwrite = false) => Overwrite = overwrite;

        public PayloadIdOptions(PayloadIdType type, bool overwrite = false)
            : this(overwrite) => _ids.Add(type, null);

        public PayloadIdOptions(IEnumerable<PayloadIdType> types, bool overwrite = false)
            : this(overwrite)
        {
            if (types is null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            foreach (var type in types.Distinct())
            {
                _ids.Add(type, null);
            }
        }

        private PayloadIdOptions(IDictionary<PayloadIdType, string> ids, bool overwrite = false)
            : this(overwrite) => _ids = new Dictionary<PayloadIdType, string>(ids);

        public bool Overwrite { get; set; }

        public PayloadIdOptions Clone() => new PayloadIdOptions(_ids, Overwrite);

        public IEnumerable<PayloadIdType> GetIdTypes() => new HashSet<PayloadIdType>(_ids.Keys);

        public bool HasIdType(PayloadIdType type) => _ids.ContainsKey(type);

        /// <summary>
        /// Assigns an ID only if the ID type is not already assigned.
        /// </summary>
        /// <param name="type">The ID type to potentially assign.</param>
        /// <param name="id">The ID to potentially assign.</param>
        /// <returns>
        /// The ID.
        /// </returns>
        public string InitializeId(PayloadIdType type, string id = null) => _ids.InitializeKey(type, id);

        public bool SetExistingId(PayloadIdType type, string id = null) => _ids.SetExistingValue(type, id);

        public string Get(PayloadIdType type) => _ids.TryGetValue(type, out var value) ? value : null;

        public string Set(PayloadIdType type, string id = null) => _ids[type] = id;

        public IDictionary<PayloadIdType, string> GetIds() => new Dictionary<PayloadIdType, string>(_ids);
    }
}