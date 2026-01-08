using Animation;

using App;

using Engine.Components;
using Engine.Entities;
using Engine.Utils;
using UnityEngine;

namespace StoryMode.UI
{
    public class EnemyInMenu : MonoBehaviour {
        [SerializeField]
        private bool isBoss;
        
        [SerializeField]
        private RectTransform bodyTransform;

        [SerializeField]
        private ImageAnimation imageAnimation;

        [SerializeField]
        private AnimationResource resource;

        private EnemyType _enemyType;
        private Vector3 _originPosition = Vector3.zero;
        private Vector3 _changedPosition = Vector3.zero;
        
        public void ChangeImage(IEnemyDetails enemy) {
           _enemyType = (EnemyType) enemy.Skin;

           if (!isBoss) {
               return;
           }

           var size = _enemyType switch {
                EnemyType.CandyKing => 180f,
                EnemyType.BeetlesKing => 160f,
                EnemyType.LordPirates => 160f,
                EnemyType.DumplingsMaster => 200f,
                _ => 120f
            };
            SetSize(size);
            
            var anchorPosition = _enemyType switch {
                EnemyType.CandyKing => (-104f, 30f),
                EnemyType.LordPirates => (-104f, 20f),
                EnemyType.DumplingsMaster => (-84f, 10f),
                _ => (-104f, 0f )
                };
            SetAnchorPosition(anchorPosition);
        }

        public void SetAnimation(FaceDirection face) {
            var sprites = resource.GetSpriteMoving(_enemyType, face);
            imageAnimation.StartLoop(sprites, face == FaceDirection.Left);
        }

        private void SetAnchorPosition((float, float) xy) {
            var anchoredPosition = bodyTransform.anchoredPosition;
            anchoredPosition.x = xy.Item1;
            anchoredPosition.y = xy.Item2;
            bodyTransform.anchoredPosition = anchoredPosition;
        }
        
        private void SetSize(float size) {
            bodyTransform.sizeDelta = new Vector2(size, size);
        }
    }
}
