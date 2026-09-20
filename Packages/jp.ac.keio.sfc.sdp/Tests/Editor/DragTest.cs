#nullable enable
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
namespace GameCanvas.Editor.Tests
{
    public class DragTest
    {
        static GcPointer P(int id, float x, float y, bool down = false, bool up = false, bool cancel = false)
            => new(id, GcPointerType.Touch, true, down, !up && !cancel, up, cancel, true,
                new GcPoint(x,y), new GcPoint(20,30), default, .01);
        [Test] public void FollowsCapturedId_UsesReleasePosition_AndDoesNotJumpToOtherFinger()
        {
            var rect = new GcRect(10,20,50,50); var drag = new GcDrag();
            var values = new List<GcPointer>{P(7,25,35,true),P(8,90,90,true)};
            var list = new GcReadOnlyList<GcPointer>(values);
            drag.Update(list, ref rect, GcAnchor.UpperLeft, GcAffine.Identity);
            Assert.That(drag.PointerId,Is.EqualTo(7)); Assert.That(rect.X,Is.EqualTo(15)); Assert.That(drag.Started,Is.True);
            values.Clear(); values.Add(P(8,90,90)); values.Add(P(7,70,80,up:true));
            drag.Update(list, ref rect, GcAnchor.UpperLeft, GcAffine.Identity);
            Assert.That(rect.X,Is.EqualTo(60)); Assert.That(rect.Y,Is.EqualTo(70)); Assert.That(drag.Ended,Is.True); Assert.That(drag.Active,Is.False);
            values.Clear(); values.Add(P(8,95,95)); drag.Update(list,ref rect,GcAnchor.UpperLeft,GcAffine.Identity);
            Assert.That(drag.Active,Is.False); Assert.That(rect.X,Is.EqualTo(60));
        }
        [Test] public void SameFrameReleaseWorks_CancellationAndMissingContactRestoreOrigin()
        {
            var rect = new GcRect(10,20,50,50); var drag = new GcDrag();
            var values = new List<GcPointer>{P(7,40,50,true,true)}; var list = new GcReadOnlyList<GcPointer>(values);
            drag.Update(list,ref rect,GcAnchor.UpperLeft,GcAffine.Identity);
            Assert.That(drag.Started && drag.Ended,Is.True); Assert.That(rect.X,Is.EqualTo(30));
            rect = new GcRect(10,20,50,50); values[0]=P(7,40,50,true);
            drag.Update(list,ref rect,GcAnchor.UpperLeft,GcAffine.Identity);
            values[0]=P(7,90,90,cancel:true); drag.Update(list,ref rect,GcAnchor.UpperLeft,GcAffine.Identity);
            Assert.That(drag.Cancelled,Is.True); Assert.That(rect.X,Is.EqualTo(10));
            values[0]=P(7,40,50,true); drag.Update(list,ref rect,GcAnchor.UpperLeft,GcAffine.Identity);
            values.Clear(); drag.Update(list,ref rect,GcAnchor.UpperLeft,GcAffine.Identity);
            Assert.That(drag.Cancelled,Is.True); Assert.That(rect.Y,Is.EqualTo(20));
        }
        [Test] public void ContainsMatchesDrawingTransformForEveryAnchorAndRotation()
        {
            foreach (GcAnchor anchor in Enum.GetValues(typeof(GcAnchor)))
            foreach (float rotation in new[]{0f,30f,90f,180f})
            foreach (float width in new[]{100f,-100f})
            {
                var rect=GcRect.FromDegrees(150,180,width,50,rotation);
                var coordinate = GcAffine.FromTRS(new float2(.1f,.2f),.2f,new float2(.8f,1.3f));
                var matrix=GcAffine.FromTRS(rect.Position,rect.Radian,rect.Size).Mul(coordinate);
                var offset=new float2((int)anchor%3,(int)anchor/3)*-.5f;
                foreach (float2 local in new[]{new float2(.1f,.1f),new float2(.9f,.9f),new float2(-.1f,.5f),new float2(1.1f,.5f)})
                {
                    var world=matrix.Mul(local+offset);
                    Assert.That(GcHitTest.Contains(rect,new GcPoint(world.x,world.y),anchor,coordinate),Is.EqualTo(local.x>0&&local.x<1));
                }
            }
            var bounds=new GcRect(10,20,50,60);
            Assert.That(bounds.Contains(10,20),Is.True); Assert.That(bounds.Contains(60,20),Is.False);
            Assert.That(bounds.Contains(10,80),Is.False); Assert.That(bounds.Contains(float.NaN,30),Is.False);
            Assert.That(new GcRect(0,0,0,10).Contains(0,0),Is.False);
        }
        [Test] public void WarmDragFramesAllocateNoManagedMemory()
        {
            var rect=new GcRect(10,20,50,50); var drag=new GcDrag();
            var values=new List<GcPointer>{P(7,25,35,true)}; var list=new GcReadOnlyList<GcPointer>(values);
            drag.Update(list,ref rect,GcAnchor.UpperLeft,GcAffine.Identity); values[0]=P(7,30,40);
            for(int i=0;i<10;i++)drag.Update(list,ref rect,GcAnchor.UpperLeft,GcAffine.Identity);
            long before=GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<1000;i++)drag.Update(list,ref rect,GcAnchor.UpperLeft,GcAffine.Identity);
            long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
            Assert.That(bytes,Is.Zero);
        }
    }
}
