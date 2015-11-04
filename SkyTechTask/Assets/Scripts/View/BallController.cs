using UnityEngine;
using UnityEngine.Networking;
using Rand = UnityEngine.Random;

using Utils;

using System;
using System.Collections;
using System.Collections.Generic;

namespace View
{
    public class BallController : NetworkBehaviour
    {
        public static Action<GameObject, bool> OnHit = delegate{};

        float speed;
        float time = 0f;
        float totalTime;
        bool randomTrajectory;

        const float edgeY = 0.05f;
        const float edgeX = 0.02f;

        const string catcherTag = "catcher";
        const string borderTag = "border";

        List<Vector3> positions = new List<Vector3>();
        
        public void Init(float speed, bool randomTrajectory)
        {
            this.speed = speed;
            this.randomTrajectory = randomTrajectory;
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(Rand.Range(edgeX, 1 - edgeX), 1f + edgeY, -Camera.main.transform.position.z));
            positions.Add(transform.position);
            if (randomTrajectory)
                GenerateTrajectory();
        }

        [ServerCallback]
        void Update()
        {
            if (!isServer)
                return;

            if (!randomTrajectory)
            {
                transform.Translate(Vector3.down * speed * Time.deltaTime);
            }
            else
            {
                if(time < totalTime)
                {
                    transform.position = CurveHelper.InterpBezierCube(positions[0], positions[1], 
                                         positions[2], positions[3], Mathf.Clamp01(time / totalTime));
                    time += Time.deltaTime;
                }
            }
            RpcUpdateBallPosition(transform.position);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == catcherTag)
                OnHit(gameObject, true);
            else if (other.tag == borderTag)
                OnHit(gameObject, false);
        }

        void GenerateTrajectory()
        {
            float z = - Camera.main.transform.position.z;
            positions.Add(Camera.main.ViewportToWorldPoint(new Vector3(Rand.Range(edgeX, 1 - edgeX), 0.65f, z)));
            positions.Add(Camera.main.ViewportToWorldPoint(new Vector3(Rand.Range(edgeX, 1 - edgeX), 0.33f, z)));
            positions.Add(Camera.main.ViewportToWorldPoint(new Vector3(Rand.Range(edgeX, 1 - edgeX), -edgeY, z)));
            float distance = CurveHelper.GetPathLength(positions.ToArray());
            time = 0;
            totalTime = distance / speed;
        }

        [ClientRpc]
        void RpcUpdateBallPosition(Vector3 pos)
        {
            if (isClient)
            {
                transform.position = pos;
            }
        }
    }
}
