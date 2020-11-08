/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;

namespace GameCanvas
{
    readonly struct GcMinMaxBox3D : System.IEquatable<GcMinMaxBox3D>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public readonly float MaxX;
        public readonly float MaxY;
        public readonly float MaxZ;
        public readonly float MinX;
        public readonly float MinY;
        public readonly float MinZ;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GcMinMaxBox3D(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            MinX = Mathf.Min(x1, x2);
            MinY = Mathf.Min(y1, y2);
            MinZ = Mathf.Min(z1, z2);
            MaxX = Mathf.Max(x1, x2);
            MaxY = Mathf.Max(y1, y2);
            MaxZ = Mathf.Max(z1, z2);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GcMinMaxBox3D(Vector3 p1, Vector3 p2) : this(p1.x, p1.y, p1.z, p2.x, p2.y, p2.z) { }

        /// <summary>
        /// 中心
        /// </summary>
        public Vector3 Center => new Vector3(CenterX, CenterY, CenterZ);

        public float CenterX => (MinX + MaxX) * 0.5f;

        public float CenterY => (MinY + MaxY) * 0.5f;

        public float CenterZ => (MinZ + MaxZ) * 0.5f;

        /// <summary>
        /// 奥行き
        /// </summary>
        public float Depth => (MaxZ - MinZ);

        /// <summary>
        /// 高さ
        /// </summary>
        public float Height => (MaxY - MinY);

        public Vector3 Max => new Vector3(MaxX, MaxY, MaxZ);

        public Vector3 Min => new Vector3(MinX, MinY, MinZ);

        /// <summary>
        /// 大きさ
        /// </summary>
        public Vector3 Size => new Vector3(Width, Height, Depth);

        /// <summary>
        /// 幅
        /// </summary>
        public float Width => (MaxX - MinX);

        public static bool operator !=(GcMinMaxBox3D lh, GcMinMaxBox3D rh) => !lh.Equals(rh);

        public static bool operator ==(GcMinMaxBox3D lh, GcMinMaxBox3D rh) => lh.Equals(rh);

        public bool Equals(GcMinMaxBox3D other)
            => MinX == other.MinX && MinY == other.MinY && MinZ == other.MinZ
            && MaxX == other.MaxX && MaxY == other.MaxY && MaxZ == other.MaxZ;

        public override bool Equals(object obj)
            => (obj is GcMinMaxBox3D other) && Equals(other);

        public override int GetHashCode()
            => MinX.GetHashCode() ^ MinY.GetHashCode() ^ MinZ.GetHashCode()
            & MaxX.GetHashCode() ^ MaxY.GetHashCode() ^ MaxZ.GetHashCode();

        public override string ToString()
            => string.Format("{{x:[{0:f3}, {1:f3}], y:[{2:f3}, {3:f3}], z:[{4:f3}, {5:f3}]}}", MinX, MaxX, MinY, MaxY, MinZ, MaxZ);
        #endregion
    }
}
