using System;
using Game.Core.Events;
using NUnit.Framework;

namespace Game.Tests.EditMode.Events
{
    public sealed class EventBusTests
    {
        private struct LevelWon
        {
            public int Stars;
        }

        private struct CoinsChanged
        {
            public int Total;
        }

        [Test]
        public void Publish_SubscribedListener_ReceivesNotice()
        {
            var bus = new EventBus();
            var stars = -1;
            bus.Subscribe<LevelWon>(notice => stars = notice.Stars);

            bus.Publish(new LevelWon { Stars = 3 });

            Assert.That(stars, Is.EqualTo(3));
        }

        [Test]
        public void Publish_NoSubscribers_DoesNotThrow()
        {
            var bus = new EventBus();

            Assert.DoesNotThrow(() => bus.Publish(new CoinsChanged { Total = 10 }));
        }

        [Test]
        public void Publish_OtherNoticeType_ListenerIsNotCalled()
        {
            var bus = new EventBus();
            var calls = 0;
            bus.Subscribe<LevelWon>(notice => calls++);

            bus.Publish(new CoinsChanged { Total = 10 });

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void Publish_AfterUnsubscribe_ListenerIsNotCalled()
        {
            var bus = new EventBus();
            var calls = 0;
            Action<LevelWon> listener = notice => calls++;
            bus.Subscribe(listener);
            bus.Unsubscribe(listener);

            bus.Publish(new LevelWon { Stars = 1 });

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void Unsubscribe_NeverSubscribed_DoesNotThrow()
        {
            var bus = new EventBus();

            Assert.DoesNotThrow(() => bus.Unsubscribe<LevelWon>(notice => { }));
        }
    }
}
