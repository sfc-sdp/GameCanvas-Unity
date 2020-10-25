using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace GameCanvas.Tests
{
    public class PlaymodeTest
    {
        [Test]
        public void Empty()
        {
            //
        }

        [UnityTest]
        public IEnumerator EmptyCoroutine()
        {
            yield return null;
        }
    }
}
