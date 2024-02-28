using System;
using System.Collections.Generic;
using UnityEngine;

namespace DesertImage.Assets
{
    [Serializable]
    public struct LibraryNode<TId, TItem>
    {
        public TId Id;
        public TItem Value;
    }

    public abstract class AbstractScriptableLibrary<TId, TItem> : ScriptableObject, ILibrary<TId, TItem>
    {
        public Dictionary<TId, TItem>.ValueCollection Values => _storage.Values;
        
        public List<LibraryNode<TId, TItem>> Nodes;

        private readonly Dictionary<TId, TItem> _storage = new Dictionary<TId, TItem>();

        public void Initialize()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                Register(node.Id, node.Value);
            }
        }

        public void Register(TId id, TItem item)
        {
            if (_storage.ContainsKey(id))
            {
#if DEBUG
                Debug.LogWarning($"Library {name} already contains item with id {id}. Old item will be replaced");
#endif
                _storage[id] = item;
            }

            _storage.Add(id, item);
        }

        public TItem Get(TId id)
        {
            if (_storage.TryGetValue(id, out var item)) return item;

            throw new Exception("Library has not item with id {id");
        }
    }
}