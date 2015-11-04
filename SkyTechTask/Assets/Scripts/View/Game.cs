using UnityEngine;
using UnityEngine.Networking;

using GUI;

using System.Collections;
using System.Collections.Generic;

namespace View
{
    /// <summary>
    /// Отвечает за настройки игры.
    /// </summary>
    [System.Serializable]
    public class GameConfig
    {
        /// <summary>
        /// Через каждый такой временной интервал увеличивается скорость падения шариков.
        /// </summary>
        [Range(10, 60)]
        public int incSpeedTime = 30;
        /// <summary>
        /// Скорость шариков в начале игры.
        /// </summary>
        public float startSpeedValue = 0.7f;
        /// <summary>
        /// На сколько увеличивается скорость шариков каждый временной интервал.
        /// </summary>
        public float incSpeedValue = 0.5f;

        /// <summary>
        /// Имеет ли шарик непрямую траекторию.
        /// </summary>
        public bool randomTrajectory;

        /// <summary>
        /// Максимальное количество шариков которые могут быть одновременно в игре.
        /// </summary>
        [Range(1, 10)]
        public int maxBallCount = 1;
        /// <summary>
        /// Вероятность того, что появится добавочный шарик.
        /// </summary>
        [Range(0f, 1f)]
        public float addBallsProbability = 0.5f;
        /// <summary>
        /// Время, через которое пробует появиться добавочный шарик.
        /// </summary>
        public int addBallTime = 5;
    }
    
    public class Game : NetworkBehaviour
    {
        [SerializeField]
        GameConfig gameConfig = new GameConfig();
        
        [HideInInspector]
        public int scores;
        
        [SerializeField]
        GameObject catcherPrefab;

        [SerializeField]
        GameObject ballPrefab;

        [SerializeField]
        Transform objectsHolder;

        [SerializeField]
        GameMenu gameMenu;

        List<GameObject> balls = new List<GameObject>();

        float time = 0f;
        float addBallTime = 0f;
        float speed;
        bool isOver = false;

        public override void OnStartServer()
        {
            base.OnStartServer();

            scores = 0;
            time = addBallTime = Time.time;
            speed = gameConfig.startSpeedValue;
            gameMenu.SetScores(scores);
            BallController.OnHit += OnBallHit;
            InitCatcher();
            CreateRandomBall();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            gameMenu.SetScores(scores);
        } 

        [ServerCallback]
        void Update()
        {
            if (time + gameConfig.incSpeedTime < Time.time)
            {
                time = Time.time;
                speed += gameConfig.incSpeedValue;
            }

            //В настройках игры возможно увеличивать количество шаров, одновременно находящихся на сцене 
            //(для большего разнообразия)
            if (balls.Count < gameConfig.maxBallCount && (addBallTime + gameConfig.addBallTime < Time.time))
            {
                addBallTime = Time.time;
                if(Random.value < gameConfig.addBallsProbability)
                    CreateRandomBall();
            }
        }

        void InitCatcher()
        {
            if (!isServer)
                return;

            GameObject catcher = Instantiate<GameObject>(catcherPrefab);
            catcher.transform.parent = objectsHolder;
            NetworkServer.Spawn(catcher);
        }

        void CreateRandomBall()
        {
            if (!isServer || isOver)
                return;

            GameObject ball = Instantiate<GameObject>(ballPrefab);
            ball.GetComponent<BallController>().Init(speed, gameConfig.randomTrajectory);
            NetworkServer.Spawn(ball);
            ball.transform.parent = objectsHolder;
            balls.Add(ball);
        }

        void OnBallHit(GameObject go, bool success)
        {
            if (!isServer)
                return;

            balls.Remove(go);
            NetworkServer.Destroy(go);

            if(!success)
            {
                BallController.OnHit -= OnBallHit;
                balls.ForEach(x => NetworkServer.Destroy(x));
                balls.Clear();
                isOver = true;
                gameMenu.ShowGameOver();
            }
            else
            {
                ++scores;
                gameMenu.SetScores(scores);
                if (balls.Count == 0 || (balls.Count < gameConfig.maxBallCount && Random.value < gameConfig.addBallsProbability))
                    CreateRandomBall();
            }
            RpcUpdateInfo(scores, isOver);
        }

        [ClientRpc(channel = 0)]
        void RpcUpdateInfo(int currScores, bool isGameOver)
        {
            if (isClient)
            {
                gameMenu.SetScores(currScores);
                if(isGameOver)
                    gameMenu.ShowGameOver();
            }
        }
    }
}
