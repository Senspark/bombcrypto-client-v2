using System;
using System.Collections.Generic;
using System.Linq;

namespace BLPvpMode.Engine.Manager {
    public class ManagerNameCache {
        private readonly Dictionary<Type, string> _serviceNames = new();

        public string GetName<T>() where T : IManager {
            return GetName(typeof(T));
        }

        public string GetName(Type type) {
            if (_serviceNames.TryGetValue(type, out var result)) {
                return result;
            }
            var interfaces = type.GetInterfaces().ToList();
            if (type.IsInterface) {
                interfaces.Add(type);
            }
            foreach (var item in interfaces) {
                var attribute = Attribute.GetCustomAttribute(item, typeof(ManagerAttribute));
                if (attribute is ManagerAttribute managerAttribute) {
                    var name = managerAttribute.Name;
                    _serviceNames.Add(type, name);
                    return name;
                }
            }
            throw new Exception($"The requested manager is not registered: {type.Name}");
        }
    }
}