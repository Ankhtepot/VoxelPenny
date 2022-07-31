using UnityEngine;

namespace DefaultNamespace.Extensions
{
    public static class Vector3Extensions
    {
        private static Vector3Int vector3Int;

        static Vector3Extensions()
        {
            vector3Int = new Vector3Int();
        }
        
        public static Vector3Int ToVector3Int(this Vector3 source)
        {
            vector3Int.x = (int) source.x;
            vector3Int.y = (int) source.y;
            vector3Int.z = (int) source.z;

            return vector3Int;
        }
    }
}