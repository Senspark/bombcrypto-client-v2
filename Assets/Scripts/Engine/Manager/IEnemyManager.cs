using App;

using Cysharp.Threading.Tasks;

using Engine.Entities;

using UnityEngine;

namespace Engine.Manager {
    public interface IEnemyManager {
        int Count { get; }
        IBossSkillDetails GetBossSkillDetails();
        UniTask<Enemy> CreateEnemy(IEnemyDetails enemy, Vector2Int location);
        void CreateEnemies(IEnemyDetails[] enemies);
        void CreateEnemies(IEnemyDetails[] enemies, Vector2Int location);
        void RemoveEnemy(Enemy enemy);
        Enemy GetEnemyById(int id);
    }
}
