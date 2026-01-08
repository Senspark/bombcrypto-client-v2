using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace Engine.Utils
{
    public class ComponentPoolIndexer : MonoBehaviour
    {
        public int Index { get; set; } = -1;
    }

    public class ComponentPool<T> : IObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly List<T> activeList = new List<T>();
        private readonly List<T> inactiveList = new List<T>();

        public int Capacity => activeList.Count + inactiveList.Count;

        public ComponentPool(T prefab)
        {
            this.prefab = prefab;
        }

        public ComponentPool(string path)
        {
            prefab = Resources.Load<T>(path);
        }

        public void Reserve(int capacity)
        {
            var currentCapacity = Capacity;
            for (var i = currentCapacity; i < capacity; ++i)
            {
                inactiveList.Add(Clone());
            }
        }

        public T Instantiate()
        {
            if (inactiveList.Count == 0)
            {
                inactiveList.Add(Clone());
            }

            var instance = inactiveList[inactiveList.Count - 1];
            inactiveList.RemoveAt(inactiveList.Count - 1);

            var indexer = instance.GetComponent<ComponentPoolIndexer>();
            Assert.IsTrue(indexer.Index == -1);
            indexer.Index = activeList.Count;
            activeList.Add(instance);

            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Destroy(T instance)
        {
            var indexer = instance.GetComponent<ComponentPoolIndexer>();
            var index = indexer.Index;

            Assert.IsTrue(0 <= index && index < activeList.Count);
            Assert.IsFalse(inactiveList.Contains(instance));
            inactiveList.Add(instance);

            indexer.Index = -1;
            if (index < activeList.Count - 1)
            {
                activeList[index] = activeList[activeList.Count - 1];
                activeList[index].GetComponent<ComponentPoolIndexer>().Index = index;
            }
            activeList.RemoveAt(activeList.Count - 1);

            instance.gameObject.SetActive(false);
        }

        private T Clone()
        {
            var instance = Object.Instantiate(prefab);
            instance.gameObject.AddComponent<ComponentPoolIndexer>();
            instance.gameObject.SetActive(false);
            return instance;
        }
    }
}