#nullable enable
using NUnit.Framework;
namespace GameCanvas.Editor.Tests
{
    public class ActorSnapshotTest
    {
        sealed class Scene : GcScene { }
        sealed class Actor : GcActor { }
        [Test] public void TypedSnapshotsContainDerivedActorsAndRemainStable()
        {
            var scene = new Scene(); var first = scene.CreateActor<Actor>();
            Assert.That(scene.TryGetActorAll<Actor>(out var snapshot));
            Assert.That(snapshot[0], Is.SameAs(first));
            scene.CreateActor<Actor>();
            Assert.That(snapshot.Length, Is.EqualTo(1));
            Assert.That(scene.TryGetActorAll<Actor>(out var next)); Assert.That(next.Length, Is.EqualTo(2));
            scene.RemoveActorAll(); Assert.That(snapshot[0], Is.SameAs(first));
            Assert.That(scene.TryGetActorAll<Actor>(out _), Is.False);
        }
        [Test] public void RemovingAnActorBeforeTheFirstUpdateClearsItsTypeIndex()
        {
            var scene = new Scene(); var actor = scene.CreateActor<Actor>();
            Assert.That(scene.TryRemoveActor(actor));
            Assert.That(scene.GetActorCount<Actor>(), Is.Zero);
            Assert.That(scene.TryGetActorAll<Actor>(out var actors), Is.False);
            Assert.That(actors.Length, Is.Zero);
        }
    }
}
