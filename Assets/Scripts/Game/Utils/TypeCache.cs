using System;
using System.Collections.Generic;

using Engine.Entities;

namespace Engine.Utils
{
    public class TypeCache
    {
        private readonly Dictionary<Type, string> typeNames = new Dictionary<Type, string>();
        private readonly Dictionary<Type, List<Type>> entityTrees = new Dictionary<Type, List<Type>>();

        public string GetName(Type type)
        {
            if (typeNames.TryGetValue(type, out var result))
            {
                return result;
            }
            var name = type.Name;
            typeNames.Add(type, name);
            return name;
        }

        public List<Type> GetEntityTree(Type type)
        {
            if (entityTrees.TryGetValue(type, out var result))
            {
                return result;
            }
            var tree = new List<Type>();
            for (var currentType = type; currentType != typeof(Entity); currentType = currentType.BaseType)
            {
                tree.Add(currentType);
            }
            tree.Reverse();
            entityTrees.Add(type, tree);
            return tree;
        }
    }
}