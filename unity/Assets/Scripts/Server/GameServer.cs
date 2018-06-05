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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace Server {
    /// <summary>
    /// Game erver logic for the standalone server.
    /// You really only want one of these.
    /// </summary>
    public class GameServer {
        static readonly int defaultMinPort = 7000;
        static readonly int defaultMaxPort = 8000;

        /// <summary>
        /// Minimum port in range server can start on
        /// </summary>
        public const string MinPortEnv = "MIN_PORT";

        /// <summary>
        /// Maximum port in range the server can start on
        /// </summary>
        public const string MaxPortEnv = "MAX_PORT";

        /// <summary>
        /// Environment variable for where the session services
        /// is. Set by Kubernetes.
        /// </summary>
        public const string SessionsServiceEnv = "SESSIONS_SERVICE_HOST";

        /// <summary>
        /// Environment or the session name. Provided by K8s downward API
        /// </summary>
        public const string SessionNameEnv = "SESSION_NAME";


        /// <summary>
        /// How many times to retry for an open port
        /// </summary>
        private const int maxStartRetries = 10;

        private const int playersNeededForGame = 2;

        /// <summary>
        /// Delegate for when two players have joined the game
        /// </summary>
        public delegate void Ready();

        /// <summary>
        /// Event when two players have joined the game
        /// and we are ready to start
        /// </summary>
        public static event Ready OnGameReady;

        static GameServer s_instance;

        // local member variables

        List<GameObject> _players;
        int _connCount;
        Random _rnd;
        int _port;

        readonly IUnityServer _server;

        /// <summary>
        /// Returns the singleton instances if it exists.
        /// </summary>
        /// <exception cref="Exception">When Start() has not been called yet, this is thrown</exception>
        static GameServer s_Instance {
            get {
                if (s_instance == null) {
                    throw new Exception(string.Format("{0} has not had Start() called.",
                                                      typeof(GameServer).FullName));
                }

                return s_instance;
            }
        }

        /// <summary>
        /// Constructor, sets the server
        /// </summary>
        /// <param name="server"></param>
        public GameServer(IUnityServer server) {
            _rnd = new Random();
            _server = server;
            _players = new List<GameObject>();
        }

        /// <summary>
        /// Starts the server with the dependencies this singleton needs.
        /// This should only ever be called once,
        /// as it will throw an exception if it get called again.
        /// </summary>
        public static void Start(IUnityServer server) {
            if (s_instance == null) {
                s_instance = new GameServer(server);

                for (var i = 0; i < maxStartRetries; i++) {
                    s_instance.SelectPort();
                    if (s_instance._server.StartServer()) {
                        s_instance.Register();
                        return;
                    }
                }

                Stop();
                throw new Exception(string.Format("Error starting server after {0} retries", maxStartRetries));
            }

            throw new Exception(string.Format("{0} Can only be started once!",
                                              typeof(GameServer).FullName));
        }

        /// <summary>
        /// Sets the port to a random
        /// </summary>
        void SelectPort() {
            var minPort = defaultMinPort;
            var maxPort = defaultMaxPort;
            var minPortStr = Environment.GetEnvironmentVariable(MinPortEnv);
            var maxPortStr = Environment.GetEnvironmentVariable(MaxPortEnv);

            if (minPortStr != null) {
                minPort = int.Parse(minPortStr);
            }

            if (maxPortStr != null) {
                maxPort = int.Parse(maxPortStr);
            }

            _port = _rnd.Next(minPort, maxPort);
            Debug.LogFormat("[GameServer] Attempting to start server on port: {0}", _port);
            _server.SetPort(_port);
        }

        /// <summary>
        /// Register this server
        /// </summary>
        void Register() {
            var registry = Environment.GetEnvironmentVariable(SessionsServiceEnv);

            if (registry == null) {
                Debug.LogFormat("[GameNetwork] No Session Registry environment variable set. Skipping.");
                return;
            }

            Debug.Log("Registering Server...");

            var session = new Session {
                                          id = Environment.GetEnvironmentVariable(SessionNameEnv),
                                          port = _port
                                      };

            var host = "http://" + registry + "/register";
            _server.PostHTTP(host, JsonUtility.ToJson(session));
        }

        /// <summary>
        /// Stops the server, and lets go of all the resources.
        /// </summary>
        public static void Stop() {
            s_instance._server.ShutdownNetwork();
            OnGameReady = null;
            s_instance = null;
        }

        /// <summary>
        /// Should be called when the server recieves a connection
        /// </summary>
        /// <param name="conn">The connection</param>
        public static void OnServerConnect(NetworkConnection conn) {
            s_Instance._connCount++;
            Debug.LogFormat("[GameNetwork] Client #{0} Connected", s_Instance._connCount);

            // only two players are allowed
            if (s_Instance._connCount > playersNeededForGame) {
                conn.Disconnect();
            }
        }

        /// <summary>
        /// Should be called when the server has a player added
        /// </summary>
        /// <param name="player"></param>
        public static void OnServerAddPlayer(GameObject player) {
            Debug.LogFormat("[GameNetwork] Adding Player {0}. Count: {1}", player, s_Instance._players.Count);
            s_Instance._players.Add(player);

            if (s_Instance._players.Count == playersNeededForGame && OnGameReady != null) {
                Debug.Log("[GameNetwork] Firing on game ready!");
                OnGameReady();
            }
        }

        /// <summary>
        /// List of players that are currently connected to the game,
        /// In the order that they connected in.
        /// </summary>
        /// <returns></returns>
        public static List<GameObject> GetPlayers() { return s_Instance._players; }
    }
}
