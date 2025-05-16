using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    [System.Serializable]
    public partial class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        internal List<SerializedKeyValuePair<TKey, TValue>> _serializedList = new List<SerializedKeyValuePair<TKey, TValue>>();

#if UNITY_EDITOR
        internal IKeyable LookupTable
        {
            get
            {
                if (_lookupTable == null)
                    _lookupTable = new DictionaryLookupTable<TKey, TValue>(this);
                return _lookupTable;
            }
        }

        private DictionaryLookupTable<TKey, TValue> _lookupTable;
#endif

        public void OnAfterDeserialize()
        {
            Clear();

            foreach (var kvp in _serializedList)
            {
#if UNITY_EDITOR
                if (!ContainsKey(kvp.Key))
                    Add(kvp.Key, kvp.Value);
#else
                Add(kvp.Key, kvp.Value);
#endif
            }

#if UNITY_EDITOR
            LookupTable.RecalculateOccurences();
#else
            _serializedList.Clear();
#endif
        }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
                LookupTable.RemoveDuplicates();
#endif
        }

        public void Clone(IDictionary<TKey, TValue> original)
        {
            _serializedList.Clear();

            foreach (var kvp in original)
            {
                var entry = new SerializedKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
                _serializedList.Add(entry);
            }

            OnAfterDeserialize();
        }

        public void AddSerialized(TKey key, TValue value)
        {
#if UNITY_EDITOR
            if (_serializedList.Find(x => x.Key.Equals(key)).Equals(default))
            {
                throw new ArgumentException($"An element with the same key already exists in collection.");
            }

            var entry = new SerializedKeyValuePair<TKey, TValue>(key, value);
            _serializedList.Add(entry);
#endif
        }

        public void RemoveSerialized(TKey key)
        {
#if UNITY_EDITOR
            _serializedList.RemoveAll(x => x.Key.Equals(key));
#endif
        }

        public void SetSerialized(TKey key, TValue value)
        {
#if UNITY_EDITOR
            var entry = _serializedList.Find(x => x.Key.Equals(key));
            if (entry.Equals(default))
            {
                throw new ArgumentNullException();
            }

            entry.Value = value;
#endif
        }
    }
}
