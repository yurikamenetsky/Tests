using UnityEngine;
using UnityEngine.Networking;

using System.Collections;

namespace View
{
    public class CatcherController : NetworkBehaviour
    {
        [SerializeField]
        float velocity = 10f;

        [SerializeField]
        float scale = 1f;

        void Awake()
        {
            transform.localScale = scale * transform.localScale;
        }

        void FixedUpdate()
        {
            if (!isServer)
                return;

            float currX = Input.GetAxis("Horizontal") * velocity * Time.fixedDeltaTime;
            if (Mathf.Abs(currX) >= float.Epsilon)
            {
                transform.Translate(currX, 0, 0);

                Bounds bounds = GetComponent<Renderer>().bounds;
                Vector3 maxPos = Camera.main.WorldToViewportPoint(new Vector3(bounds.max.x, 0f, 0f));
                Vector3 minPos = Camera.main.WorldToViewportPoint(new Vector3(bounds.min.x, 0f, 0f));
                maxPos.x = Mathf.Clamp01(maxPos.x);
                minPos.x = Mathf.Clamp01(minPos.x);
                Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
                pos.x = (maxPos.x + minPos.x) * 0.5f;
                transform.position = Camera.main.ViewportToWorldPoint(pos);
                
                RpcUpdateCatcherPosition(transform.position);
            }
        }

        [ClientRpc(channel = 0)]
        void RpcUpdateCatcherPosition(Vector3 pos)
        {
            if (isClient)
            {
                transform.position = pos;
            }
        }
    }
}
