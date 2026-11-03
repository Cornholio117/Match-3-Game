using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Game.Tests.EditMode.Study
{
    // Study exercise (week 1, Thursday): an async loader that can be cancelled.
    // Plain Task is used so it can be tested without Play mode; in the game this
    // would be an Awaitable that listens to destroyCancellationToken instead.
    public sealed class FakeLevelLoader
    {
        public async Task<int> LoadAsync(int levelIndex, CancellationToken token)
        {
            await Task.Delay(50, token);
            return levelIndex * 10;
        }
    }

    public sealed class AsyncLoaderTests
    {
        [Test]
        public async Task LoadAsync_NotCancelled_ReturnsLevelData()
        {
            var loader = new FakeLevelLoader();

            var data = await loader.LoadAsync(3, CancellationToken.None);

            Assert.That(data, Is.EqualTo(30));
        }

        [Test]
        public void LoadAsync_CancelledWhileLoading_ThrowsOperationCanceled()
        {
            var loader = new FakeLevelLoader();
            var source = new CancellationTokenSource();

            var loading = loader.LoadAsync(3, source.Token);
            source.Cancel();

            Assert.CatchAsync<System.OperationCanceledException>(async () => await loading);
        }
    }
}
