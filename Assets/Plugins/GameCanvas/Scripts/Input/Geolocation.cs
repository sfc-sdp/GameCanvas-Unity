/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2019 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas.Input
{
    using UnityEngine;

    public sealed class Geolocation
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private float mLastAltitude;
        private float mLastLatitude;
        private float mLastLongitude;
        private double mLastTimestamp;

        private bool mIsActive;
        private bool mHasUpdate;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Geolocation()
        {
            // TODO
        }

        public void OnBeforeUpdate()
        {
            mHasUpdate = false;

            if (mIsActive && HasPermission && Status == LocationServiceStatus.Running)
            {
                var data = Input.location.lastData;
                if (mLastTimestamp != data.timestamp)
                {
                    mLastAltitude = data.altitude;
                    mLastLatitude = data.latitude;
                    mLastLongitude = data.longitude;
                    mLastTimestamp = data.timestamp;

                    mHasUpdate = true;
                }
            }
        }

        public bool HasPermission => Input.location.isEnabledByUser;
        public LocationServiceStatus Status => Input.location.status;
        public bool IsRunning => HasPermission && (Status == LocationServiceStatus.Running || Status == LocationServiceStatus.Initializing);
        public bool HasUpdate => mHasUpdate;

        public float LastAltitude => mLastAltitude;
        public float LastLatitude => mLastLatitude;
        public float LastLongitude => mLastLongitude;
        public long LastTimestamp => (long)mLastTimestamp;
        public System.DateTimeOffset LastTime => Engine.Time.cUnixZero.AddSeconds(mLastTimestamp);

        public void StartService()
        {
            if (!mIsActive && Status == LocationServiceStatus.Stopped)
            {
                Input.location.Start();
                mIsActive = true;
            }
        }

        public void StopService()
        {
            if (mIsActive || Status != LocationServiceStatus.Stopped)
            {
                Input.location.Stop();
                mIsActive = false;
            }
        }

        #endregion
    }
}
