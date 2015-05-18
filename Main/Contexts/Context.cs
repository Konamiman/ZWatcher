using System.Collections;
using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZWatcher.Contexts
{
    public abstract class Context : IContext
    {
        public ushort Address { get; internal set; }
        public IZ80Processor Z80 { get; }
        public long TimesReached { get; set; }
        public IDictionary<string, ushort> Symbols { get; }

        internal Context(IZ80Processor z80, IDictionary<string, ushort> symbols)
        {
            this.Z80 = z80;
            this.Symbols = new SymbolsDictionary(symbols);
        }

        #region SymbolsDictionary class

        private class SymbolsDictionary : IDictionary<string, ushort>
        {
            private readonly IDictionary<string, ushort> inner;
            
            public SymbolsDictionary(IDictionary<string, ushort> inner)
            {
                this.inner = inner;
            }

            public ushort this[string key]
            {
                get
                {
                    if(!Keys.Contains(key))
                        throw new KeyNotFoundException($"\"{key}\" not found in the symbols dictionary");

                    return inner[key];
                }
                set { inner[key] = value; }
            }

            public IEnumerator<KeyValuePair<string, ushort>> GetEnumerator() => inner.GetEnumerator();

            public void Add(KeyValuePair<string, ushort> item) => inner.Add(item);

            public void Clear() => inner.Clear();

            public bool Contains(KeyValuePair<string, ushort> item) => inner.Contains(item);

            public void CopyTo(KeyValuePair<string, ushort>[] array, int arrayIndex) => inner.CopyTo(array, arrayIndex);

            public int Count => inner.Count;

            public bool Remove(KeyValuePair<string, ushort> item) => inner.Remove(item);

            public bool IsReadOnly => inner.IsReadOnly;

            public bool ContainsKey(string key) => inner.ContainsKey(key);

            public void Add(string key, ushort value) => inner.Add(key, value);

            public bool Remove(string key) => inner.Remove(key);

            public bool TryGetValue(string key, out ushort value) => inner.TryGetValue(key, out value);

            public ICollection<string> Keys => inner.Keys;

            public ICollection<ushort> Values => inner.Values;

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        #endregion
    }
}
