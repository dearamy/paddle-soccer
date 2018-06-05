// Copyright 2017 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Game;
using UnityEngine;
using UnityEngine.Networking;

namespace Server.Controllers {
    /// <summary>
    /// Create a ball. Once there is a Goal, create a new
    /// ball at the given time frame.
    /// </summary>
    public class BallController : NetworkBehaviour {
        [SerializeField] [Tooltip("The soccer ball prefab")]
        GameObject prefabBall;

        [SerializeField] [Tooltip("Goal #1")] private GameObject goal1;
        [SerializeField] [Tooltip("Goal #2")] private GameObject goal2;

        /// <summary>
        /// The current instance of the ball.
        /// </summary>
        GameObject currentBall;

        /// <summary>
        /// property to ensure we don't try and create a ball while creating a ball
        /// </summary>
        bool isGoal;

        // --- Messages ---

        /// <summary>
        /// Make sure there is a ball prefab
        /// </summary>
        /// <exception cref="Exception">If the prefab is null, throws an exception</exception>
        void OnValidate() {
            if (prefabBall == null) {
                throw new Exception("[Ball Controller] Ball prefab needs to be populated");
            }
        }

        /// <summary>
        /// Call when two players have joined the game
        /// </summary>
        void Start() {
            Debug.Log("[Ball Controller] Initialising...");
            isGoal = false;

            goal1.GetComponent<TriggerObservable>().TriggerEnter += OnGoal;
            goal2.GetComponent<TriggerObservable>().TriggerEnter += OnGoal;

            GameServer.OnGameReady += CreateBall;
        }

        // --- Functions ---

        /// <summary>
        /// Create a ball after 5 seconds. Removes the old one if there is one.
        /// </summary>
        void OnGoal(Collider _) {
            if (!isGoal) {
                isGoal = true;
                Invoke("CreateBall", 5);
            }
        }

        /// <summary>
        /// Creates the ball
        /// </summary>
        void CreateBall() {
            Debug.Log("[Ball Controller] Creating a ball");

            if (currentBall != null) {
                Destroy(currentBall);
            }

            currentBall = Instantiate(prefabBall);
            currentBall.name = Ball.Name;

            NetworkServer.Spawn(currentBall);

            isGoal = false;
        }
    }
}
