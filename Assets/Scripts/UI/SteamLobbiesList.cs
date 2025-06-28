using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using Mirror;

namespace SteamLobbyTutorial
{
    public class SteamLobbiesList : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject lobbyEntryPrefab; // Le prefab LobbyEntryItem
        public Transform lobbyListContent; // Le Content du ScrollView
        public Button refreshButton;
        public Button backButton;
        public TextMeshProUGUI statusText; // Pour afficher les messages de statut
        public GameObject loadingIndicator; // Optionnel : indicateur de chargement
        
        [Header("Settings")]
        public int maxLobbiesCount = 50;
        
        // Callback Steam pour la liste des lobbies
        protected Callback<LobbyMatchList_t> lobbyMatchListCallback;
        
        // Liste des entrées de lobby créées
        private List<SteamLobbyEntry> lobbyEntries = new List<SteamLobbyEntry>();
        
        // Référence au PanelSwapper pour navigation
        private PanelSwapper panelSwapper;
        
        void Start()
        {
            // Initialisation des callbacks Steam
            if (SteamManager.Initialized)
            {
                lobbyMatchListCallback = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
            }
            
            // Configuration des boutons
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(RefreshLobbyList);
            }
            
            if (backButton != null)
            {
                backButton.onClick.AddListener(GoBack);
            }
            
            // Trouver le PanelSwapper
            panelSwapper = FindFirstObjectByType<PanelSwapper>();
            
            // Affichage initial
            UpdateStatusText("Click on refresh button to see lobbies");
        }
        
        void OnEnable()
        {
            // Rafraîchir automatiquement la liste quand le panel devient actif
            StartCoroutine(DelayedRefresh());
        }
        
        private IEnumerator DelayedRefresh()
        {
            yield return new WaitForSeconds(0.1f); // Petit délai pour s'assurer que tout est initialisé
            RefreshLobbyList();
        }
        
        public void RefreshLobbyList()
        {
            if (!SteamManager.Initialized)
            {
                UpdateStatusText("Steam is not initialized");
                return;
            }
            
            UpdateStatusText("Searching for lobbies...");
            ShowLoadingIndicator(true);
            
            // Nettoyer la liste actuelle
            ClearLobbyList();
            
            // Configurer les filtres de recherche
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.AddRequestLobbyListResultCountFilter(maxLobbiesCount);
            
            // Filtrer seulement les lobbies avec des slots disponibles
            SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(1);
            
            // IMPORTANT : Filtrer par App ID (votre jeu spécifique)
            // Cela évite de récupérer les lobbies d'autres jeux
            SteamMatchmaking.AddRequestLobbyListStringFilter("appid", SteamUtils.GetAppID().ToString(), ELobbyComparison.k_ELobbyComparisonEqual);
            
            // Filtrer par un tag spécifique à votre jeu pour plus de sécurité
            SteamMatchmaking.AddRequestLobbyListStringFilter("game_type", "mirror_steam_lobby", ELobbyComparison.k_ELobbyComparisonEqual);
            
            // Optionnel : Filtrer seulement les lobbies publics
            SteamMatchmaking.AddRequestLobbyListStringFilter("lobby_type", "public", ELobbyComparison.k_ELobbyComparisonEqual);
            
            // Demander la liste des lobbies à Steam
            SteamAPICall_t apiCall = SteamMatchmaking.RequestLobbyList();
            
            if (apiCall == SteamAPICall_t.Invalid)
            {
                UpdateStatusText("Error while requesting lobbies");
                ShowLoadingIndicator(false);
            }
        }
        
        private void OnLobbyMatchList(LobbyMatchList_t callback)
        {
            ShowLoadingIndicator(false);
            
            uint lobbiesFound = callback.m_nLobbiesMatching;
            
            if (lobbiesFound == 0)
            {
                UpdateStatusText("No lobby found");
                return;
            }
            
            UpdateStatusText($"{lobbiesFound} lobby(s) found");
            
            // Créer les entrées pour chaque lobby trouvé
            for (int i = 0; i < lobbiesFound; i++)
            {
                CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                CreateLobbyEntry(lobbyID);
            }
        }
        
        private void CreateLobbyEntry(CSteamID lobbyID)
        {
            if (lobbyEntryPrefab == null || lobbyListContent == null)
            {
                Debug.LogError("LobbyEntryPrefab or LobbyListContent is not assigned");
                return;
            }
            
            // Instantier le prefab
            GameObject lobbyEntryObj = Instantiate(lobbyEntryPrefab, lobbyListContent);
            
            // Récupérer le composant SteamLobbyEntry ou l'ajouter
            SteamLobbyEntry lobbyEntry = lobbyEntryObj.GetComponent<SteamLobbyEntry>();
            if (lobbyEntry == null)
            {
                lobbyEntry = lobbyEntryObj.AddComponent<SteamLobbyEntry>();
                
                // Connecter les références UI si elles ne sont pas déjà connectées
                SetupLobbyEntryReferences(lobbyEntry, lobbyEntryObj);
            }
            
            // Initialiser l'entrée avec les données du lobby
            lobbyEntry.Initialize(lobbyID, this);
            
            // Ajouter à la liste
            lobbyEntries.Add(lobbyEntry);
        }
        
        private void SetupLobbyEntryReferences(SteamLobbyEntry lobbyEntry, GameObject lobbyEntryObj)
        {
            // Chercher le TextMeshPro spécifiquement nommé "LobbyName"
            Transform lobbyNameTransform = lobbyEntryObj.transform.Find("LobbyName");
            if (lobbyNameTransform != null)
            {
                lobbyEntry.lobbyNameText = lobbyNameTransform.GetComponent<TextMeshProUGUI>();
            }
            
            // Si pas trouvé, prendre le premier TextMeshPro
            if (lobbyEntry.lobbyNameText == null)
            {
                lobbyEntry.lobbyNameText = lobbyEntryObj.GetComponentInChildren<TextMeshProUGUI>();
            }
            
            // Chercher le bouton spécifiquement nommé "JoinButton"
            Transform joinButtonTransform = lobbyEntryObj.transform.Find("JoinButton");
            if (joinButtonTransform != null)
            {
                lobbyEntry.joinButton = joinButtonTransform.GetComponent<Button>();
            }
            
            // Si pas trouvé, chercher un bouton avec "join" dans le nom
            if (lobbyEntry.joinButton == null)
            {
                Button[] buttons = lobbyEntryObj.GetComponentsInChildren<Button>();
                foreach (Button button in buttons)
                {
                    if (button.name.ToLower().Contains("join"))
                    {
                        lobbyEntry.joinButton = button;
                        break;
                    }
                }
            }
        }
        
        public void JoinLobby(CSteamID lobbyID)
        {
            if (!lobbyID.IsValid())
            {
                UpdateStatusText("Lobby ID is invalid");
                return;
            }
            
            UpdateStatusText("Trying to join lobby...");
            
            // Vérifier si nous sommes déjà connectés et nous déconnecter si nécessaire
            if (NetworkClient.isConnected || NetworkClient.active)
            {
                NetworkManager.singleton.StopClient();
            }
            
            // Rejoindre le lobby Steam
            SteamAPICall_t apiCall = SteamMatchmaking.JoinLobby(lobbyID);
            
            if (apiCall == SteamAPICall_t.Invalid)
            {
                UpdateStatusText("Error while joining lobby");
            }
        }
        
        private void ClearLobbyList()
        {
            // Détruire toutes les entrées existantes
            foreach (SteamLobbyEntry entry in lobbyEntries)
            {
                if (entry != null && entry.gameObject != null)
                {
                    Destroy(entry.gameObject);
                }
            }
            
            lobbyEntries.Clear();
        }
        
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            
            Debug.Log($"[SteamLobbiesList] {message}");
        }
        
        private void ShowLoadingIndicator(bool show)
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(show);
            }
        }
        
        private void GoBack()
        {
            if (panelSwapper != null)
            {
                panelSwapper.SwapPanel("MainPanel");
            }
        }
        
        void OnDestroy()
        {
            // Nettoyer les callbacks
            if (lobbyMatchListCallback != null)
            {
                lobbyMatchListCallback.Dispose();
            }
        }
    }
} 