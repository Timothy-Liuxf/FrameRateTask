namespace TestBasic
{
    [TestClass]
    public class TestFrameRate
    {
        [TestMethod]
        public void TestInitialFrameRate()
        {
            var frt = new FrameRateTaskExecutor<int>
            (
                () => true,
                () => { },
                100,
                () => 0,
                maxTotalDuration: 2000
            );
            Assert.AreEqual(10u, frt.FrameRate);
        }

        [TestMethod]
        public void TestRunningFrameRate()
        {
            var frt = new FrameRateTaskExecutor<int>
            (
                () => true,
                () => { },
                100,
                () => 0,
                maxTotalDuration: 2000
            );
            Assert.AreEqual(10u, frt.FrameRate);

            int success = 1;
            new Thread
            (
                () =>
                {
                    while (!frt.Finished)
                    {
                        var frameRate = frt.FrameRate;
                        if (frameRate < 8 || frameRate > 12)
                        {
                            Interlocked.Exchange(ref success, 0);
                            break;
                        }
                        Thread.Sleep(500);
                    }
                }
            ).Start();
            frt.Start();

            Assert.IsTrue(Interlocked.CompareExchange(ref success, 0, 0) == 1);
        }
    }
}
