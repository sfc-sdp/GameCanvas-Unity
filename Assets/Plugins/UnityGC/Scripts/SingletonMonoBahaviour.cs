/*------------------------------------------------------------*/
/// <summary>GameCanvas for Unity</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2017 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/

using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// シングルトン版MonoBehaviour
    /// </summary>
    /// <typeparam name="T">シングルトンにしたい型</typeparam>
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        protected static T instance;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

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

        /// <summary>
        /// 生成処理
        /// </summary>
        protected virtual void Awake()
        {
            Validate();
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (this == instance)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 自身がシングルトンインスタンスかどうかを検証する。既にインスタンスがある場合は即時破棄される
        /// </summary>
        protected bool Validate()
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
