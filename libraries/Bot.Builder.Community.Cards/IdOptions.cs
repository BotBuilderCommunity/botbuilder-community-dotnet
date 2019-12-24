using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards
{
    public class IdOptions
    {
        private readonly IDictionary<IdType, string> _ids = new Dictionary<IdType, string>();

        public IdOptions(bool overwrite = false) => Overwrite = overwrite;

        public IdOptions(IdType type, bool overwrite = false)
            : this(overwrite) => _ids.Add(type, null);

        public IdOptions(IEnumerable<IdType> types, bool overwrite = false)
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

        private IdOptions(IDictionary<IdType, string> ids, bool overwrite = false)
            : this(overwrite) => _ids = new Dictionary<IdType, string>(ids);

        public bool Overwrite { get; set; }

        public IdOptions Clone() => new IdOptions(_ids, Overwrite);

        public IEnumerable<IdType> GetIdTypes() => new List<IdType>(_ids.Keys);

        public bool HasIdType(IdType type) => _ids.ContainsKey(type);

        /// <summary>
        /// Assigns an ID only if the ID type is not already assigned.
        /// </summary>
        /// <param name="type">The ID type to potentially assign.</param>
        /// <param name="id">The ID to potentially assign.</param>
        /// <returns>
        /// True if the ID was assigned because the ID type was not present.
        /// False if the ID was not assigned because the ID type was present.
        /// </returns>
        public bool InitializeId(IdType type, string id = null) => _ids.InitializeKey(type, id);

        public bool SetExistingId(IdType type, string id = null) => _ids.SetExistingValue(type, id);

        public string Get(IdType type) => _ids.TryGetValue(type, out var value) ? value : null;

        public string Set(IdType type, string id = null) => _ids[type] = id;

        public IDictionary<IdType, string> GetIds() => new Dictionary<IdType, string>(_ids);
    }
}