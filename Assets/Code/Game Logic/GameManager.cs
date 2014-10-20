﻿using UnityEngine;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using GameLogic;
using Helpers;

using UnityDebug  = UnityEngine.Debug;
using UnityRandom = UnityEngine.Random;

namespace GameLogic
{
    public class GameManager : MonoBehaviour
    {
        public GameMode InitialMode;
        private GameMode Mode;

        private List<GameEntityCacheEntry> GameEntityCache;
        public string[] StartupAssets;
        private Dictionary<string, object> ResourceCache;
        private SpawnPointCollection SpawnPoints;

        public bool AssetsLoaded
        {
            get { return _AssetsLoaded; }
            set { _AssetsLoaded = value; }
        }
        public bool InternalsInitialized
        {
            get
            {
                return _InternalsInitialized;
            }
        }
        private bool _InternalsInitialized;
        public int GameEntityCacheCount
        {
            get
            {
                return GameEntityCache.Count;
            }
        }
        private bool _AssetsLoaded;
        public bool GameReadyToStart
        {
            get
            {
                if (_AssetsLoaded && _InternalsInitialized)
                    return true;

                return false;
            }
        }

        private const string PrefabsDirectory = "Prefabs/";
        
        void Start()
        {
            InitializeInternals();

            SpawnPlayer();
        }

        void Update()
        {

        }

        private void UpdateState()
        {

        }

        /// <summary>
        /// Spawns player at a random spawn point.
        /// </summary>
        private void SpawnPlayer()
        {
            GameObject player = GetResourceCacheItemByName("Player") as GameObject;
            Vector3 SpawnPointPosition = SpawnPoints[UnityRandom.Range(0, SpawnPoints.Count)].transform.position;
            SpawnPointPosition.y += 5;

            GameObject GamePlayerObject = GameObject.Instantiate(player, SpawnPointPosition, this.transform.rotation) as GameObject;
            GamePlayerObject.name = "Player";
            AddGameEntityCacheEntry(GamePlayerObject);
        }

        /// <summary>
        /// Initializes all objects/resources that GameManager needs to use
        /// </summary>
        private void InitializeInternals()
        {
            ResourceCache = new Dictionary<string, object>();
            GameEntityCache = new List<GameEntityCacheEntry>();

            SpawnPoints = new SpawnPointCollection();
            SpawnPoints.CacheAll();

            LoadAssets();

            _InternalsInitialized = true;
        }

        /// <summary>
        /// Loads all assets that are critical and required before gameplay can start
        /// </summary>
        private void LoadAssets()
        {
            if(StartupAssets.Length > 0)
            {
                for(int i = 0; i < StartupAssets.Length; ++i)
                {
                    string AssetPath = PrefabsDirectory + StartupAssets[i];
                    LoadGameObject(AssetPath, StartupAssets[i]);
                }
            }
            _AssetsLoaded = true;
        }

        public void AddObjectToResourceCache(string ObjectHandleName, object Handle)
        {
            ResourceCache.Add(ObjectHandleName, Handle);
        }


        public void LoadGameObject(string GameObjectFile,string GameObjectHandle)
        {
            if(ResourceCache.ContainsKey(GameObjectHandle))
            {
                UnityDebug.Log("GameManager is attempting to load: " + GameObjectHandle + " Twice. Blocked.");
                return;
            }
            GameObject LoadedObject = Resources.Load(GameObjectFile) as GameObject;
            AddObjectToResourceCache(GameObjectHandle, LoadedObject);
        }

        public bool ResourceCacheContains(string Handle)
        {
            return ResourceCache.ContainsKey(Handle);
        }
        public int ResourceCacheItemCount
        {
            get
            {
                return ResourceCache.Count;
            }
        }
        public object GetResourceCacheItemByName(string Name)
        {
            return ResourceCache[Name];
        }

        private void AddGameEntityCacheEntry(GameObject Entity)
        {
            GameEntityCacheEntry NewCacheEntry = new GameEntityCacheEntry(Entity);
            GameEntityCache.Add(NewCacheEntry);
        }
        private void RemoveGameEntityCacheEntry(GameEntityCacheEntry Entry)
        {
            GameEntityCache.Remove(Entry);
        }
        private void RemoveGameEntityCacheEntry(int EntityHash)
        {
            for(int i = 0; i < GameEntityCache.Count; ++i)
            {
                if(GameEntityCache[i].EntityHash == EntityHash)
                {
                    GameEntityCache.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Caches all game entities in the world
        /// </summary>
        private void CacheAllGameEntities()
        {
            GameObject[] Entities = GameObject.FindGameObjectsWithTag("GameEntity");

            if(Entities.Length == 0)
            {
                Debug.LogError("GAME MANAGER MESSAGE: FATAL ERROR: NO GAME ENTITIES IN SCENE. HALTING");
                Debug.Break();
                return;
            }

            for (int i = 0; i < Entities.Length; ++i )
            {
                AddGameEntityCacheEntry(Entities[i]);
            }
        }
    }
}