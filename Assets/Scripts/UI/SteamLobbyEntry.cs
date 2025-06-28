using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

namespace SteamLobbyTutorial
{
    public class SteamLobbyEntry : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI lobbyNameText;
        public Button joinButton;
        
        private CSteamID lobbyID;
        private SteamLobbiesList lobbiesList;
        
        public void Initialize(CSteamID steamLobbyID, SteamLobbiesList parentList)
        {
            lobbyID = steamLobbyID;
            lobbiesList = parentList;
            
            // Configuration du bouton join
            if (joinButton != null)
            {
                joinButton.onClick.RemoveAllListeners();
                joinButton.onClick.AddListener(JoinLobby);
            }
            
            UpdateLobbyInfo();
        }
        
        private void UpdateLobbyInfo()
        {
            if (lobbyID.IsValid())
            {
                // Récupérer les informations du lobby
                int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
                int memberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
                
                // Essayer d'abord avec les nouvelles métadonnées
                string hostName = SteamMatchmaking.GetLobbyData(lobbyID, "host_name");
                string displayText = "Lobby";
                
                // Si pas de nom d'host dans les métadonnées, essayer l'ancienne méthode
                if (string.IsNullOrEmpty(hostName))
                {
                    string hostAddress = SteamMatchmaking.GetLobbyData(lobbyID, "HostAddress");
                    
                    if (!string.IsNullOrEmpty(hostAddress))
                    {
                        try
                        {
                            CSteamID hostID = new CSteamID(ulong.Parse(hostAddress));
                            hostName = SteamFriends.GetFriendPersonaName(hostID);
                        }
                        catch (System.Exception)
                        {
                            // Si l'analyse échoue, utiliser le texte par défaut
                            hostName = "";
                        }
                    }
                }
                
                // Construire le nom d'affichage
                if (!string.IsNullOrEmpty(hostName))
                {
                    displayText = $"Lobby de {hostName}";
                }
                
                // Ajouter le compteur de joueurs
                if (memberLimit > 0)
                {
                    displayText += $" ({memberCount}/{memberLimit})";
                }
                else
                {
                    displayText += $" ({memberCount} joueurs)";
                }
                
                if (lobbyNameText != null)
                {
                    lobbyNameText.text = displayText;
                }
                
                // Activer/désactiver le bouton selon la disponibilité
                if (joinButton != null)
                {
                    bool canJoin = memberLimit == 0 || memberCount < memberLimit;
                    joinButton.interactable = canJoin;
                    
                    // Changer le texte du bouton si le lobby est plein
                    var buttonText = joinButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = canJoin ? "Rejoindre" : "Plein";
                    }
                }
                
                // Debug: log des métadonnées du lobby pour diagnostic
                string gameType = SteamMatchmaking.GetLobbyData(lobbyID, "game_type");
                string version = SteamMatchmaking.GetLobbyData(lobbyID, "version");
                Debug.Log($"Lobby affiché - Host: {hostName}, Joueurs: {memberCount}/{memberLimit}, Type: {gameType}, Version: {version}");
            }
        }
        
        private void JoinLobby()
        {
            if (lobbyID.IsValid() && lobbiesList != null)
            {
                lobbiesList.JoinLobby(lobbyID);
            }
        }
        
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
} 