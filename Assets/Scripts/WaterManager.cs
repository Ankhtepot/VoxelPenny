using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class WaterManager : MonoBehaviour
    {
        public GameObject player;

        private void Update()
        {
            Vector3 pos = player.transform.position;
            pos.y = 0;
            gameObject.transform.position = pos;
        }
    }
}