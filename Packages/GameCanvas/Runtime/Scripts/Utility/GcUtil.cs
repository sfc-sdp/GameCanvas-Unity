/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace GameCanvas
{
    public static class GcUtil
    {
        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T a, ref T b)
        {
            var temp = a;
            a = b;
            b = temp;
        }

        public static void Memcpy<T>(in NativeList<T> from, NativeList<T> to)
            where T : struct
        {
            to.Length = from.Length;
            if (from.Length == 0) return;

            unsafe
            {
                var src = from.GetUnsafeReadOnlyPtr();
                var dst = to.GetUnsafePtr();
                var size = to.Length * UnsafeUtility.SizeOf<T>();
                UnsafeUtility.MemCpy(dst, src, size);
            }
        }

        #endregion
    }
}
