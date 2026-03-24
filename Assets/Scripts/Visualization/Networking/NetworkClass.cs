using AnimArch.Extensions;
using Unity.Netcode;
using UnityEngine;
using Visualization.UI;

namespace Visualization.Networking
{
    public class NetworkClass : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (UIEditorManager.Instance.active)
                GetComponentsInChildren<UnityEngine.UI.Button>(true)
                    .ForEach(x =>
                    {
                        if (UIEditorManager.ShouldAutoActivateButton(x))
                        {
                            x.gameObject.SetActive(true);
                        }
                    });
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePostionServerRpc(Vector3 postion)
        {
            if (IsClient && !IsHost)
                return;
            transform.position = postion;
        }
    }
}
