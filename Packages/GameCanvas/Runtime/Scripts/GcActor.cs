/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
namespace GameCanvas
{
    /// <summary>
    /// アクター基底
    /// </summary>
    public abstract class GcActor : IActor, System.IComparable<GcActor>
    {
        internal GcScene m_Scene = null;
        internal int m_Priority = 0;
        internal uint m_Index = 0;

        // ---

        /// <inheritdoc/>
        public virtual int Priority { get { return m_Priority; } set { m_Priority = value; } }

        /// <inheritdoc/>
        public virtual void Update() { }

        /// <inheritdoc/>
        public virtual void Draw() { }

        /// <inheritdoc/>
        public virtual void AfterDraw() { }

#pragma warning disable IDE1006
        protected static GcProxy gc { get; private set; }
#pragma warning restore IDE1006

        internal static void Inject(GcProxy proxy) { gc = proxy; }

        public int CompareTo(GcActor other)
        {
            if (m_Priority == other.Priority)
            {
                return (int)(m_Index - other.m_Index);
            }
            return m_Priority - other.Priority;
        }
    }
}
