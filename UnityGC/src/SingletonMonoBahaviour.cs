using UnityEngine;

namespace GameCanvas
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        protected static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        var gameObject = new GameObject(typeof(T).ToString());
                        if (gameObject == null)
                        {
                            Debug.LogError(typeof(T) + " is nothing");
                            return null;
                        }

                        instance = gameObject.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        protected void Awake()
        {
            CheckInstance();
        }

        protected bool CheckInstance()
        {
            if (instance == null)
            {
                instance = (T)this;
                return true;
            }
            else if (Instance == this)
            {
                return true;
            }

            DestroyImmediate(this);
            return false;
        }
    }
}
