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
        private readonly Dictionary<Type, List<MemberInfo>> _declaredInjectableMembers = new();
        private readonly Dictionary<Type, List<MemberInfo>> _allInjectableMembers = new();

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
            var members = GetAllInjectableMembers(value.GetType());
            foreach (var member in members) {
                switch (member) {
                    case PropertyInfo property:
                        property.SetValue(value, Resolve(property.PropertyType));
                        break;
                    case FieldInfo field:
                        field.SetValue(value, Resolve(field.FieldType));
                        break;
                }
            }
        }

        private List<MemberInfo> GetAllInjectableMembers(Type runtimeType) {
            if (_allInjectableMembers.TryGetValue(runtimeType, out var cached)) {
                return cached;
            }
            var members = new List<MemberInfo>();
            var type = runtimeType;
            while (type != null && type != typeof(UnityEngine.MonoBehaviour)) {
                members.AddRange(GetDeclaredInjectableMembers(type));
                type = type.BaseType;
            }
            _allInjectableMembers.Add(runtimeType, members);
            return members;
        }

        public void Dispose() {
            foreach (var service in _services.Values.Reverse()) {
                try {
                    if (service is IService svc) {
                        svc.Destroy();
                    }
                    if (service is IDisposable disposable) {
                        disposable.Dispose();
                    }
                } catch {
                }
            }
            _services.Clear();
        }

        private List<MemberInfo> GetDeclaredInjectableMembers(Type type) {
            if (_declaredInjectableMembers.TryGetValue(type, out var cached)) {
                return cached;
            }
            var members = new List<MemberInfo>();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            foreach (var property in type.GetProperties(flags)) {
                if (property.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0) {
                    members.Add(property);
                }
            }
            foreach (var field in type.GetFields(flags)) {
                if (field.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0) {
                    members.Add(field);
                }
            }
            _declaredInjectableMembers.Add(type, members);
            return members;
        }
    }
}