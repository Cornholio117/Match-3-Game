using Game.Core.Services;
using NUnit.Framework;

namespace Game.Tests.EditMode.Services
{
    public sealed class NullServicesTests
    {
        [Test]
        public void NullSaveService_TryLoad_ReturnsFalseAndNullJson()
        {
            ISaveService save = new NullSaveService();

            var found = save.TryLoad("progress", out var json);

            Assert.That(found, Is.False);
            Assert.That(json, Is.Null);
        }

        [Test]
        public void NullSaveService_SaveThenLoad_StillFindsNothing()
        {
            ISaveService save = new NullSaveService();

            save.Save("progress", "{}");

            Assert.That(save.TryLoad("progress", out _), Is.False);
        }

        [Test]
        public void NullAudioService_AllCalls_DoNotThrow()
        {
            IAudioService audio = new NullAudioService();

            Assert.DoesNotThrow(() =>
            {
                audio.PlaySfx("match");
                audio.PlayMusic("menu");
                audio.StopMusic();
            });
        }

        [Test]
        public void NullAnalyticsService_LogEvent_DoesNotThrow()
        {
            IAnalyticsService analytics = new NullAnalyticsService();

            Assert.DoesNotThrow(() => analytics.LogEvent("level_won"));
        }
    }
}
