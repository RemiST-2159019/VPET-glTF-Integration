using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace vpet
{
    public class VPETOnBoardingModule : MonoBehaviour
    {
        public TMP_InputField ipInput;
        public TMP_InputField portInput;

        //left side context sensitive action bar with icon buttons
            //select camera
        //checkbox load from server
        //checkbox load textures?
        //dropdown choose roles
        //button connect/start
        
        private Core m_vpet;
        private CanvasGroup canvas;

        public void Awake()
        {
            m_vpet = GameObject.Find("VPET").GetComponent<Core>();
            canvas = GetComponent<CanvasGroup>();
            
            ipInput.text = "127.0.0.1";
            portInput.text = "5555";
        }
        
        public void send()
        {
            SceneManager sceneManager = m_vpet.getManager<SceneManager>();
            NetworkManager networkManager = m_vpet.getManager<NetworkManager>();

            SceneParserModule sceneParserModule = sceneManager.getModule<SceneParserModule>();
            SceneSenderModule sceneSenderModule = networkManager.getModule<SceneSenderModule>();

            sceneParserModule.ParseScene();
            sceneSenderModule.sendScene(ipInput.text, portInput.text);

            canvas.alpha = 0f;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
        }

        public void receive()
        {
            SceneManager sceneManager = m_vpet.getManager<SceneManager>();
            NetworkManager networkManager = m_vpet.getManager<NetworkManager>();

            SceneReceiverModule sceneReceiverModule = networkManager.getModule<SceneReceiverModule>();
            SceneCreatorModule sceneCreatorModule = sceneManager.getModule<SceneCreatorModule>();

            sceneReceiverModule.receiveScene("172.18.1.177", "5555");
        }
    }
}