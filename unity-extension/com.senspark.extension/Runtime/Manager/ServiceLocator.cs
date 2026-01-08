using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace Senspark {
    public class ServiceLocator : IServiceLocator {
        private static ServiceLocator _sharedInstance;

        /// <summary>
        /// Gets the global instance.
        /// </summary>
        public static IServiceLocator Instance => _sharedInstance ??= new ServiceLocator();

        private readonly Dictionary<string, object> _services = new();
        private readonly Dictionary<Type, string> _serviceNames = new();

        [NotNull]
        private string GetServiceName([NotNull] Type type) {
            if (_serviceNames.TryGetValue(type, out var result)) {
                return result;
            }
            var interfaces = type.GetInterfaces().ToList();
            if (type.IsInterface) {
                interfaces.Add(type);
            }
            foreach (var item in interfaces) {
                var attribute = Attribute.GetCustomAttribute(item, typeof(ServiceAttribute));
                if (attribute is ServiceAttribute serviceAttribute) {
                    var name = serviceAttribute.Name;
                    _serviceNames.Add(type, name);
                    return name;
                }
            }
            throw new Exception($"The requested service is not registered: {type.Name}");
        }

        public void Provide<T>(T service) {
            var type = service.GetType();
            var name = GetServiceName(type);
            _services.Remove(name);
            _services.Add(name, service);
        }

        public T Resolve<T>() {
            return (T) Resolve(typeof(T));
        }

        [NotNull]
        private object Resolve([NotNull] Type type) {
            var name = GetServiceName(type);
            if (_services.TryGetValue(name, out var item)) {
                return item;
            }
            throw new Exception($"Cannot find the requested service: {name}");
        }

        public void ResolveInjection<T>(T value) {
            var type = typeof(T);
            type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(it => it.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                .ToList()
                .ForEach(it => { //
                    it.SetValue(value, Resolve(it.PropertyType));
                });
            type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(it => it.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                .ToList()
                .ForEach(it => { //
                    it.SetValue(value, Resolve(it.FieldType));
                });
        }
    }
}