using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class PayloadIdOptions
    {
        private readonly IDictionary<string, string> _ids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public PayloadIdOptions(bool overwrite = false) => Overwrite = overwrite;

        public PayloadIdOptions(string type, bool overwrite = false)
            : this(overwrite) => _ids.Add(type, null);

        public PayloadIdOptions(IEnumerable<string> types, bool overwrite = false)
            : this(overwrite)
        {
            if (types is null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            foreach (var type in types.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                _ids.Add(type, null);
            }
        }

        private PayloadIdOptions(IDictionary<string, string> ids, bool overwrite = false)
            : this(overwrite) => _ids = new Dictionary<string, string>(ids, StringComparer.OrdinalIgnoreCase);

        public bool Overwrite { get; set; }

        public PayloadIdOptions Clone() => new PayloadIdOptions(_ids, Overwrite);

        public IEnumerable<string> GetIdTypes() => new HashSet<string>(_ids.Keys);

        public bool HasIdType(string type) => _ids.ContainsKey(type);

        /// <summary>
        /// Assigns an ID only if the ID type is not already assigned.
        /// </summary>
        /// <param name="type">The ID type to potentially assign.</param>
        /// <param name="id">The ID to potentially assign.</param>
        /// <returns>
        /// The ID.
        /// </returns>
        public string InitializeId(string type, string id = null) => _ids.InitializeKey(type, id);

        public bool SetExistingId(string type, string id = null) => _ids.SetExistingValue(type, id);

        public string Get(string type) => _ids.TryGetValue(type, out var value) ? value : null;

        public string Set(string type, string id = null) => _ids[type] = id;

        public IDictionary<string, string> GetIds() => new Dictionary<string, string>(_ids);

        internal PayloadIdOptions ReplaceNullWithGeneratedId(string type)
        {
            var options = Clone();

            if (HasIdType(type))
            {
                var id = Get(type);

                if (id is null)
                {
                    options.Set(type, PayloadIdTypes.GenerateId(type));
                }
            }

            return options;
        }
    }
}