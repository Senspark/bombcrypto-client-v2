using UnityEngine;

namespace pooling
{
	public abstract class PoolingObject : MonoBehaviour, IPooling
    {
		public virtual string ObjectName => "";

        public bool IsUsing { get; set; }

        public virtual void OnCollect()
        {
			IsUsing = true;
            gameObject.SetActive(true);
        }

        public virtual void OnRelease()
        {
			IsUsing = false;
            gameObject.SetActive(false);
        }
    }
}