
using Unity.Collections;
using UnityEngine;

namespace Engine.Components
{
    [DisallowMultipleComponent]
    public class Poolable : MonoBehaviour
    {
        [SerializeField]
        private string poolId;

        /// <summary>
        /// Gets or sets the pool identifier.
        /// </summary>
        /// <value>The pool identifier.</value>
        public string PoolId => poolId;

        public bool InstantiatedFromPool { get; set; }
    }
}