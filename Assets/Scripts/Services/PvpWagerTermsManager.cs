using Senspark;
using UnityEngine;

namespace App
{
    [Service(nameof(IPvpWagerTermsManager))]
    public interface IPvpWagerTermsManager : IService
    {
        bool IsWagerDisclaimerAccepted();
        void SetWagerDisclaimerAccepted(bool accepted);
    }

    public class PvpWagerTermsManager : IPvpWagerTermsManager
    {
        private const string WagerDisclaimerKey = "PvpWagerDisclaimerAccepted";

        public System.Threading.Tasks.Task<bool> Initialize()
        {
            return System.Threading.Tasks.Task.FromResult(true);
        }

        public void Destroy()
        {
        }

        public bool IsWagerDisclaimerAccepted()
        {
            return PlayerPrefs.GetInt(WagerDisclaimerKey, 0) == 1;
        }

        public void SetWagerDisclaimerAccepted(bool accepted)
        {
            PlayerPrefs.SetInt(WagerDisclaimerKey, accepted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
