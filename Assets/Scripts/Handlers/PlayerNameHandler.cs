using UnityEngine;
using Mirror;
using TMPro;
using Steamworks;

namespace SteamLobbyTutorial
{
    public class PlayerNameHandler : NetworkBehaviour
    {
        [Header("UI Components")]
        public TextMeshPro playerNameText;
        public Canvas nameCanvas;
        
        [Header("Settings")]
        public float nameOffset = 2f;
        public float textSize = 0.5f;
        
        [SyncVar(hook = nameof(OnPlayerNameChanged))]
        public string playerName = "";
        
        private Camera mainCamera;
        
        void Start()
        {
            SetupNameDisplay();
        }
        
        void SetupNameDisplay()
        {
            // Créer un Canvas pour le nom si il n'existe pas
            if (nameCanvas == null)
            {
                GameObject canvasObj = new GameObject("PlayerNameCanvas");
                canvasObj.transform.SetParent(transform);
                canvasObj.transform.localPosition = Vector3.up * nameOffset;
                
                nameCanvas = canvasObj.AddComponent<Canvas>();
                nameCanvas.renderMode = RenderMode.WorldSpace;
                nameCanvas.worldCamera = Camera.main;
                
                // Taille du canvas
                RectTransform canvasRect = nameCanvas.GetComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(3, 1);
                nameCanvas.transform.localScale = Vector3.one * textSize;
            }
            
            // Créer le TextMeshPro si il n'existe pas
            if (playerNameText == null)
            {
                GameObject textObj = new GameObject("PlayerNameText");
                textObj.transform.SetParent(nameCanvas.transform);
                textObj.transform.localPosition = Vector3.zero;
                textObj.transform.localRotation = Quaternion.identity;
                textObj.transform.localScale = Vector3.one;
                
                playerNameText = textObj.AddComponent<TextMeshPro>();
                playerNameText.text = "";
                playerNameText.fontSize = 4;
                playerNameText.color = Color.white;
                playerNameText.alignment = TextAlignmentOptions.Center;
                playerNameText.sortingOrder = 10;
                
                // Configuration du RectTransform
                RectTransform textRect = playerNameText.GetComponent<RectTransform>();
                textRect.sizeDelta = new Vector2(10, 2);
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
            
            mainCamera = Camera.main;
        }
        
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            
            // Le joueur local récupère son nom Steam et l'envoie au serveur
            if (SteamManager.Initialized)
            {
                string steamName = SteamFriends.GetPersonaName();
                CmdSetPlayerName(steamName);
            }
            else
            {
                CmdSetPlayerName("Player");
            }
        }
        
        [Command]
        void CmdSetPlayerName(string name)
        {
            playerName = name;
        }
        
        void OnPlayerNameChanged(string oldName, string newName)
        {
            if (playerNameText != null)
            {
                playerNameText.text = newName;
            }
        }
        
        void LateUpdate()
        {
            // Faire en sorte que le nom regarde toujours la caméra
            if (nameCanvas != null && mainCamera != null)
            {
                nameCanvas.transform.LookAt(mainCamera.transform);
                nameCanvas.transform.Rotate(0, 180, 0); // Inverser pour que le texte soit lisible
            }
        }
    }
} 