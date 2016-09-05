using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// 永続化可能な連想配列（ディレクショナリー）
    /// </summary>
    /// <typeparam name="TKey">キーの型</typeparam>
    /// <typeparam name="TValue">値の型</typeparam>
    [Serializable]
    class SerializableDictionary<TKey, TValue>
    {
        public List<TKey> Keys;
        public List<TValue> Values;

        /// <summary>
        /// 格納されているデータの数
        /// </summary>
        public int Count { get { return _dict.Count; } }

        private Dictionary<TKey, TValue> _dict;


        /* ---------------------------------------------------------------------------------------------------- */

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SerializableDictionary()
        {
            _dict  = new Dictionary<TKey, TValue>();
            Keys   = new List<TKey>();
            Values = new List<TValue>();
        }

        /// <summary>
        /// コンストラクタ。既存のDictionaryインスタンスを元にデータを初期化します
        /// </summary>
        public SerializableDictionary(Dictionary<TKey, TValue> dictionary)
        {
            FromDictionary(dictionary);
        }

        /// <summary>
        /// コンストラクタ。Json記法の文字列を元にデータを復元します
        /// </summary>
        public SerializableDictionary(string json)
        {
            FromJson(json);
        }


        /* ---------------------------------------------------------------------------------------------------- */

        /// <summary>
        /// Dictionary型に変換します
        /// </summary>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            return _dict;
        }

        /// <summary>
        /// Json記法で永続化します
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// 既存のDictionaryインスタンスで自身を上書きします
        /// </summary>
        /// <param name="dictionary"></param>
        public void FromDictionary(Dictionary<TKey, TValue> dictionary)
        {
            _dict  = dictionary;
            Keys   = new List<TKey>(_dict.Keys);
            Values = new List<TValue>(_dict.Values);
        }

        /// <summary>
        /// Json記法の文字列から復元したデータで自身を上書きします
        /// </summary>
        /// <param name="json"></param>
        public void FromJson(string json)
        {
            _dict = JsonUtility.FromJson< Dictionary<TKey, TValue> >(json);
            if (_dict != null)
            {
                var num = Math.Min(Keys.Count, Values.Count);
                _dict   = new Dictionary<TKey, TValue>(num);
                for (var i = 0; i < num; ++i)
                {
                    _dict.Add(Keys[i], Values[i]);
                }
            }
            else
            {
                _dict  = new Dictionary<TKey, TValue>();
                Keys   = new List<TKey>();
                Values = new List<TValue>();
            }
        }


        /* ---------------------------------------------------------------------------------------------------- */

        /// <summary>
        /// データを追加します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        /// <param name="overwrite">上書きを許すかどうか</param>
        public SerializableDictionary<TKey, TValue> Add(TKey key, TValue value, bool overwrite = false)
        {
            if (!_dict.ContainsKey(key))
            {
                _dict.Add(key, value);
                Keys.Add(key);
                Values.Add(value);
            }
            else if (overwrite)
            {
                _dict[key] = value;
                Values = new List<TValue>(_dict.Values);
            }
            return this;
        }

        /// <summary>
        /// データを初期化します
        /// </summary>
        public SerializableDictionary<TKey, TValue> Clear()
        {
            _dict.Clear();
            Keys.Clear();
            Values.Clear();
            return this;
        }

        /// <summary>
        /// キーで検索し該当するデータを削除します
        /// </summary>
        /// <param name="key">キー</param>
        public SerializableDictionary<TKey, TValue> Remove(TKey key)
        {
            _dict.Remove(key);
            Keys.Remove(key);
            Values = new List<TValue>(_dict.Values);
            return this;
        }

        /// <summary>
        /// キーで検索し該当するデータを返します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値（存在した場合）</param>
        /// <returns>キーが存在したかどうか</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }
    }
}
