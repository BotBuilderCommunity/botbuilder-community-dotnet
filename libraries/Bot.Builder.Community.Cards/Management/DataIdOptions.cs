using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class DataIdOptions
    {
        private readonly IDictionary<string, string> _ids = new Dictionary<string, string>();

        public DataIdOptions(bool overwrite = false) => Overwrite = overwrite;

        public DataIdOptions(string scope, bool overwrite = false)
            : this(overwrite) => _ids.Add(scope, null);

        public DataIdOptions(IEnumerable<string> scopes, bool overwrite = false)
            : this(overwrite)
        {
            if (scopes is null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            foreach (var scope in scopes.Distinct())
            {
                _ids.Add(scope, null);
            }
        }

        private DataIdOptions(IDictionary<string, string> ids, bool overwrite = false)
            : this(overwrite) => _ids = new Dictionary<string, string>(ids);

        public bool Overwrite { get; set; }

        public DataIdOptions Clone() => new DataIdOptions(_ids, Overwrite);

        public IEnumerable<string> GetIdTypes() => new HashSet<string>(_ids.Keys);

        public bool HasIdScope(string scope) => _ids.ContainsKey(scope);

        public string Get(string scope) => _ids.TryGetValue(scope, out var value) ? value : null;

        public DataIdOptions Set(string scope, string id = null)
        {
            _ids[scope] = id;

            return this;
        }

        public IDictionary<string, string> GetIds() => new Dictionary<string, string>(_ids);
    }
}