using System;
using System.Collections.Generic;

namespace BLPvpMode.Engine {
    public class ComponentContainer {
        private readonly Dictionary<Type, IEntityComponent> _components;

        public ComponentContainer() {
            _components = new Dictionary<Type, IEntityComponent>();
        }

        public void AddComponent(IEntityComponent component) {
            _components[component.GetType()] = component;
        }

        public T GetComponent<T>() where T : IEntityComponent {
            return (T) _components[typeof(T)];
        }
    }
}