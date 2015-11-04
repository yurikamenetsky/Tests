using UnityEngine;
using UnityEngine.Networking;

using System.Collections;

namespace GUI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.UI.Text ipAddressText;

        [SerializeField]
        GameObject errorPanel;
        
        public void OnStart()
        {
            NetworkManager.singleton.StartHost();
        }

        public void OnConnect()
        {
            NetworkManager.singleton.networkAddress = ipAddressText.text;
            NetworkManager.singleton.StartClient();
            NetworkManager.singleton.client.RegisterHandler(MsgType.Error, OnError);
        }

        void OnError(NetworkMessage msg)
        {
            errorPanel.SetActive(true);
        }

        public void OnExit()
        {
            Application.Quit();
        }
    }
}
