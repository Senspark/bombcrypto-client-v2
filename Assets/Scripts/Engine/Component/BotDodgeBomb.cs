using UnityEngine;

namespace Engine.Components
{
    public class BotDodgeBomb: MonoBehaviour
    {
        private BotManager botManager;
        private BotMove botMove;
        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            botManager = GetComponent<BotManager>();
            botMove = GetComponent<BotMove>();
        }

        //public void DodgeBomb()
        //{
        //    if (botManager.safeLocationList.Count == 0)
        //    {
        //        Debug.Log("dodgebomb: reachable = 0 - " + gameObject.name);
        //        botMove.Waiting();
        //        return;
        //    }
        //    var location = botManager.GetNearestSafeLocation();
        //    botManager.targetLocation = location;
        //}
    }
}