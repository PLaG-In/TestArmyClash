using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ArmyClash.Utilities
{
    /// <summary>
    /// Extension methods for common Unity operations
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get a random element from a list
        /// </summary>
        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
            
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Shuffle a list in place using Fisher-Yates algorithm
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Check if a transform is within a certain distance of another
        /// </summary>
        public static bool IsWithinDistance(this Transform transform, Transform other, float distance)
        {
            return Vector3.Distance(transform.position, other.position) <= distance;
        }

        /// <summary>
        /// Set alpha of a Color
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Get 2D distance (ignoring Y axis)
        /// </summary>
        public static float Distance2D(this Vector3 from, Vector3 to)
        {
            Vector2 from2D = new Vector2(from.x, from.z);
            Vector2 to2D = new Vector2(to.x, to.z);
            return Vector2.Distance(from2D, to2D);
        }

        /// <summary>
        /// Clamp a vector within min and max bounds
        /// </summary>
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(vector.x, min.x, max.x),
                Mathf.Clamp(vector.y, min.y, max.y),
                Mathf.Clamp(vector.z, min.z, max.z)
            );
        }

        /// <summary>
        /// Reset transform to default values
        /// </summary>
        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Destroy all children of a transform
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Get all components in children including inactive
        /// </summary>
        public static T[] GetComponentsInChildrenIncludingInactive<T>(this GameObject go) where T : Component
        {
            return go.GetComponentsInChildren<T>(true);
        }

        /// <summary>
        /// Safe destroy - works in editor and runtime
        /// </summary>
        public static void SafeDestroy(this Object obj)
        {
            if (obj == null) return;

#if UNITY_EDITOR
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
#else
            Object.Destroy(obj);
#endif
        }

        /// <summary>
        /// Convert enum to list of values
        /// </summary>
        public static List<T> GetEnumValues<T>() where T : System.Enum
        {
            return System.Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        /// <summary>
        /// Remap value from one range to another
        /// </summary>
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        /// <summary>
        /// Check if layer is in layer mask
        /// </summary>
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
    }
}
