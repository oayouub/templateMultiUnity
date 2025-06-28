using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace SteamLobbyTutorial
{
    public class PlayerMovementHandler : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        
        void Update()
        {
            if (isLocalPlayer)
            {
                float horizontal = Input.GetAxis("Horizontal"); // A/D ou Flèches gauche/droite
                float vertical = Input.GetAxis("Vertical");     // W/S ou Flèches haut/bas
                
                // Mouvement 3D : X pour gauche/droite, Z pour avant/arrière, Y reste constant
                Vector3 playerMovement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
                transform.position = transform.position + playerMovement;
            }
        }
    }
}