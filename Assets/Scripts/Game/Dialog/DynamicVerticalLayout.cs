using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog
{
    public class DynamicVerticalLayout : MonoBehaviour
    {
        public float cellSize;

        private RectTransform myTransform;
        private VerticalLayoutGroup layoutGroup;
        private RectOffset padding;

        private Vector2 newScale;

        private void Start()
        {
            myTransform = GetComponent<RectTransform>();
            layoutGroup = GetComponent<VerticalLayoutGroup>();
            padding = layoutGroup.padding;

        }

        private void OnGUI()
        {
            newScale = myTransform.sizeDelta;

            newScale.y = padding.top + padding.bottom + ((cellSize + layoutGroup.spacing) * transform.childCount);

            myTransform.sizeDelta = newScale;
        }
    }
}
