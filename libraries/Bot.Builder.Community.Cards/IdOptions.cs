using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards
{
    public class IdOptions
    {
        private readonly Dictionary<IdType, string> _ids = new Dictionary<IdType, string>();

        public IdOptions(bool overwrite = false)
        {
            Overwrite = overwrite;
        }

        public IdOptions(IdType type, bool overwrite = false)
            : this(overwrite)
        {
            _ids.Add(type, null);
        }

        public IdOptions(IdType type, string id, bool overwrite = false)
            : this(overwrite)
        {
            _ids.Add(type, id);
        }

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

        public IdOptions(Dictionary<IdType, string> ids, bool overwrite = false)
            : this(overwrite)
        {
            if (ids is null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            _ids = new Dictionary<IdType, string>(ids);
        }

        public bool Overwrite { get; set; }

        public IdOptions Clone() => new IdOptions(_ids, Overwrite);

        public string GetId(IdType type) => _ids.TryGetValue(type, out var value) ? value : null;

        public Dictionary<IdType, string> GetIds() => new Dictionary<IdType, string>(_ids);

        public List<IdType> GetIdTypes() => new List<IdType>(_ids.Keys);

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
        public bool InitializeId(IdType type, string id = null)
        {
            return _ids.InitializeKey(type, id);
        }

        public string SetId(IdType type, string id = null)
        {
            return _ids[type] = id;
        }
    }
}