using Engine.Entities;
using UnityEngine;

namespace Engine.Components
{
    public class BotAttackPlayer: MonoBehaviour
    {
        private BotManager botManager;
        private Player player;
        public bool isMovingToTarget;
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            botManager = GetComponent<BotManager>();
        }

        public void FindPlayerLocation()
        {
            //if (isMovingToTarget)
            //{
            //    return;
            //}
            ////player = EntityManager.PlayerManager.Player;
            //if (player == null)
            //{
            //    botManager.ChooseNextAction();
            //    return;
            //}
            
            //var start = botManager.currentLocation;
            //var end = player.GetTileLocation();

            //end.x += Random.Range(-1, 1);
            //end.y += Random.Range(-1, 1);

            //var result = botManager.mapManager.ShortestPath(start, end, botManager.isThroughBrick);
            //if (result == null)
            //{
            //    return;
            //}
            //botManager.targetLocation = end;
            //isMovingToTarget = true;
        }
    }
}