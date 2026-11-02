using Game.Core.Events;
using NUnit.Framework;

namespace Game.Tests.EditMode.Events
{
    public sealed class ChannelTests
    {
        private struct LevelWon
        {
            public int Stars;
        }

        [Test]
        public void Publish_OneListener_ReceivesNotice()
        {
            var channel = new Channel<LevelWon>();
            var stars = -1;
            channel.Subscribe(notice => stars = notice.Stars);

            channel.Publish(new LevelWon { Stars = 3 });

            Assert.That(stars, Is.EqualTo(3));
        }

        [Test]
        public void Publish_TwoListeners_BothReceiveNotice()
        {
            var channel = new Channel<LevelWon>();
            var calls = 0;
            channel.Subscribe(notice => calls++);
            channel.Subscribe(notice => calls++);

            channel.Publish(new LevelWon { Stars = 2 });

            Assert.That(calls, Is.EqualTo(2));
        }

        [Test]
        public void Publish_AfterUnsubscribe_ListenerIsNotCalled()
        {
            var channel = new Channel<LevelWon>();
            var calls = 0;
            System.Action<LevelWon> listener = notice => calls++;
            channel.Subscribe(listener);
            channel.Unsubscribe(listener);

            channel.Publish(new LevelWon { Stars = 1 });

            Assert.That(calls, Is.EqualTo(0));
        }
    }
}
