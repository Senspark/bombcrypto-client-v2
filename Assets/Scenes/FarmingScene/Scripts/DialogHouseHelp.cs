using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.FarmingScene.Scripts
{
    public class DialogHouseHelp : Dialog
    {
        public static UniTask<DialogHouseHelp> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHouseHelp>();
        }

        public void OnButtonCloseClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }

        public void OnButtonOkClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }
    }
}