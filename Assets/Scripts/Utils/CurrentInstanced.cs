using UnityEngine;

namespace Utils
{
    public class CurrentInstanced<T> : MonoBehaviour where T : CurrentInstanced<T>
    {
        public static T Instance;

        protected virtual void Awake()
        {
            Instance = (T) this;
        }
    }
}