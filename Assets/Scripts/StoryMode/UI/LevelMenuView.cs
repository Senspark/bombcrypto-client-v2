using App;

using CreativeSpore.SuperTilemapEditor;

using Engine.Manager;

using UnityEngine.SceneManagement;
using UnityEngine;

namespace StoryMode.UI {
    public class LevelMenuView : MonoBehaviour {
        [SerializeField]
        private Tileset[] tileSets;

        [SerializeField]
        private TilemapGroup tilemapBackground;

        [SerializeField]
        private TilemapGroup tileMapGroup;

        private IEntityManager _entityManager;

        public void Initialize(int stage) {
            var scene = SceneManager.GetActiveScene();
            var physicsScene = scene.GetPhysicsScene2D();

            const int col = 60;
            const int row = 11;
            var manager = new DefaultEntityManager(tileMapGroup.gameObject, physicsScene, true) {
                LevelManager = null
            };
            var mapInfo = new MapInfo() {
                GameModeType = GameModeType.StoryMode,
                Col = col,
                Row = row,
            };
            manager.MapManager = new DefaultMapManagerV2(stage, tileSets[stage], manager, tilemapBackground, null, tileMapGroup,
                mapInfo);
            _entityManager = manager;
        }
    }
}