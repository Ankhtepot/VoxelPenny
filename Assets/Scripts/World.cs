﻿using System;
using System.Collections;
using Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class World : MonoBehaviour
    {
        public static Vector3 worldDimensions = new(5, 3, 5);
        public static Vector3 chunkDimensions = new(10, 10, 10);
        public GameObject chunkPrefab;
        public GameObject mCamera;
        public GameObject fpc;
        public Slider loadingBar;
        [Header("Layers")]
        public PerlinNoiseSettings surfacePerlinNoise;
        public static PerlinNoiseSettings SurfacePerlinNoise;
        public PerlinNoiseSettings stonesPerlinNoise;
        public static PerlinNoiseSettings StonesPerlinNoise;

        private void Awake()
        {
            SurfacePerlinNoise = surfacePerlinNoise;
            StonesPerlinNoise = stonesPerlinNoise;
        }

        private void Start()
        {
            loadingBar.maxValue = worldDimensions.x * worldDimensions.y * worldDimensions.z;
            loadingBar.value = 0;
            StartCoroutine(BuildWorld());
        }

        private IEnumerator BuildWorld()
        {
            mCamera.SetActive(true);
            loadingBar.gameObject.SetActive(true);
            fpc.SetActive(false);
            
            for (int z = 0; z < worldDimensions.z; z++)
            {
                for (int y = 0; y < worldDimensions.y; y++)
                {
                    for (int x = 0; x < worldDimensions.x; x++)
                    {
                        GameObject chunk = Instantiate(chunkPrefab, transform, true);
                        Vector3 position = new(chunkDimensions.x * x, chunkDimensions.y * y, chunkDimensions.z * z);
                        Chunk chunkComponent = chunk.GetComponent<Chunk>();
                        chunkComponent.CreateChunk(chunkDimensions, position);
                        loadingBar.value += 1;
                        yield return null;
                    }
                }
            }
            
            mCamera.SetActive(false);
            loadingBar.gameObject.SetActive(false);

            float xpos = (chunkDimensions.x * worldDimensions.x) / 2f;
            float zpos = (chunkDimensions.z * worldDimensions.z) / 2f;
            float ypos = MeshUtils.fBM(xpos, zpos, surfacePerlinNoise) + 1;

            fpc.transform.position = new Vector3(xpos, ypos, zpos);
            fpc.SetActive(true);
        }
    }
}