namespace TestBasic
{
    [TestClass]
    public class TestCongestion
    {
        [TestMethod]
        public void TestDroppingFrame()
        {
            int cnt = 0;
            int dropCnt = 0;
            var frt = new FrameRateTaskExecutor<int>
            (
                () => true,
                () =>
                {
                    if (cnt < 4)
                    {
                        Thread.Sleep(200);
                    }
                    ++cnt;
                },
                50,
                () => 0,
                maxTotalDuration: 1600
            )
            {
                AllowTimeExceed = true,
                MaxTolerantTimeExceedCount = 1,
                TimeExceedAction = b =>
                {
                    if (b)
                    {
                        ++dropCnt;
                    }
                }
            };
            frt.Start();
            Assert.IsTrue(cnt > 800 / 50 && cnt < (800 / 50 + 4) + 4, $"The value of cnt: {cnt}");
            Assert.AreEqual(2, dropCnt);
        }

        [TestMethod]
        public void TestTimeExceed()
        {
            int cnt = 0;
            try
            {
                new FrameRateTaskExecutor<int>
                (
                    () => true,
                    () =>
                    {
                        Thread.Sleep(200);
                        ++cnt;
                    },
                    10,
                    () => 0
                )
                {
                    AllowTimeExceed = false,
                    MaxTolerantTimeExceedCount = 4,
                }.Start();
                Assert.Fail();
            }
            catch (TimeExceedException)
            {
                Assert.AreEqual(5, cnt);
            }
        }
    }
}
