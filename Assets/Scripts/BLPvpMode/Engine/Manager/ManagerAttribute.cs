using System;

namespace BLPvpMode.Engine.Manager {
    [AttributeUsage(AttributeTargets.Interface)]
    public class ManagerAttribute : Attribute {
        /// <summary>
        /// Gets the registered name of this service.
        /// </summary>
        public string Name { get; }

        public ManagerAttribute(string name) {
            Name = name;
        }
    }
}