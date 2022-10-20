using System.Collections.Concurrent;

namespace TestConcurrency
{
    [TestClass]
    public class TestConcurrency
    {
        [TestMethod]
        public void TestMethod()
        {
            const int endVal = 2;
            int cnt = 0;
            var result = new BlockingCollection<int>();
            var threadQueue = new BlockingCollection<Thread>();

            FrameRateTaskExecutor<int> executor = new FrameRateTaskExecutor<int>(
                () => cnt < endVal,
                () =>
                {
                    result.Add(cnt);
                    ++cnt;
                },
                1,
                () => 8888
            )
            { AllowTimeExceed = true };

            Action action = () =>
            {
                for (int i = 0; i < 10; ++i)
                {
                    var thrd = new Thread(() =>
                    {
                        if (executor.TryStart())
                        {
                            result.Add(executor.Result);
                        }
                    });
                    threadQueue.Add(thrd);
                    thrd.Start();
                };
            };
            for (int i = 0; i < 20; ++i)
            {
                var thrd = new Thread(() => action());
                threadQueue.Add(thrd);
                thrd.Start();
            }

            while (threadQueue.Count > 0)
            {
                threadQueue.TryTake(out Thread? thrd);
                if (thrd is not null)
                {
                    thrd.Join();
                }
            }
            for (int i = 0; i < endVal; ++i)
            {
                Assert.IsTrue(result.TryTake(out int seq), "Get no number");
                Assert.AreEqual(i, seq, $"Get error number: {seq}");
            }
            Assert.IsTrue(result.TryTake(out int res), "Get no result!");
            Assert.AreEqual(8888, res, $"Get error result: {res}");
        }
    }
}
