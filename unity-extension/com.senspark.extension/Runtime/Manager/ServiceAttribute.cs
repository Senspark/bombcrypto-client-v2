using System;

using JetBrains.Annotations;

using UnityEngine.Assertions;

namespace Senspark {
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceAttribute : Attribute {
        /// <summary>
        /// Gets the registered name of this service.
        /// </summary>
        [NotNull]
        public string Name { get; }

        public ServiceAttribute([NotNull] string name) {
            Name = name;
        }

        public ServiceAttribute([NotNull] Type type) {
            Assert.IsNotNull(type.FullName);
            Name = type.FullName;
        }
    }
}