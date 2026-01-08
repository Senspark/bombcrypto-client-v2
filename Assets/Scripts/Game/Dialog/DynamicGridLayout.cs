using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog
{
    public class DynamicGridLayout : MonoBehaviour
    {
        public float cellSize;

        private RectTransform myTransform;
        private GridLayoutGroup layoutGroup;
        private RectOffset padding;

        private Vector2 newScale;

        private void Start()
        {
            myTransform = GetComponent<RectTransform>();
            layoutGroup = GetComponent<GridLayoutGroup>();
            padding = layoutGroup.padding;

        }

        private void OnGUI()
        {
            newScale = myTransform.sizeDelta;

            var fixedColumn = 5;
            var row = transform.childCount / fixedColumn;
            if (transform.childCount > row * fixedColumn) {
                row += 1;
            }

            newScale.y = padding.top + padding.bottom + ((cellSize + layoutGroup.spacing.y) * row);

            myTransform.sizeDelta = newScale;
        }
    }
}
