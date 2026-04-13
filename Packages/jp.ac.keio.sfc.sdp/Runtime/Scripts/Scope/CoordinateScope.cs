/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2026 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable

namespace GameCanvas
{
    public readonly struct CoordinateScope : System.IDisposable
    {
        readonly IGraphics m_Graphics;

        internal CoordinateScope(in IGraphics graphics)
        {
            m_Graphics = graphics;
            m_Graphics.PushCoordinate();
        }

        public void Dispose()
        {
            m_Graphics?.PopCoordinate();
        }
    }

    /// <summary>
    /// 旧名称 (typo)。v8.0 で除去予定。
    /// </summary>
    [System.Obsolete("Use CoordinateScope instead. Will be removed in v8.0.")]
    public readonly struct CoordianteScope : System.IDisposable
    {
        readonly CoordinateScope m_Scope;

        internal CoordianteScope(in IGraphics graphics)
        {
            m_Scope = new CoordinateScope(graphics);
        }

        // CoordinateScope.CoordinateScope プロパティの戻り値型を CoordinateScope に
        // 変更したことによる後方互換用の暗黙変換。既存の
        // `CoordianteScope scope = gc.CoordinateScope;` を引き続き動作させる。
        // CoordinateScope は既に Push 済みのため、Push を再度呼ばずに wrap する。
        private CoordianteScope(in CoordinateScope scope)
        {
            m_Scope = scope;
        }

        public static implicit operator CoordianteScope(in CoordinateScope scope)
            => new(scope);

        public void Dispose()
        {
            m_Scope.Dispose();
        }
    }
}
