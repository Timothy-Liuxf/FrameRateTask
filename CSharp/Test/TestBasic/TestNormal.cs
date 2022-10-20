namespace TestBasic
{
    [TestClass]
    public class TestNormal
    {
        [TestMethod]
        public void TestExecutionCount()
        {
            const int maxCnt = 8;
            int cnt = 0;
            new FrameRateTaskExecutor<int>
            (
                () => cnt < maxCnt,
                () =>
                {
                    ++cnt;
                },
                10,
                () => 0
            )
            {
                AllowTimeExceed = true,
                MaxTolerantTimeExceedCount = ulong.MaxValue,
            }.Start();
            Assert.AreEqual(cnt, maxCnt);
        }

        [TestMethod]
        public void TestExecuted()
        {
            var task = new FrameRateTaskExecutor<int>
            (
                () => true,
                () => false,
                10,
                () => 0
            )
            {
                AllowTimeExceed = true,
                MaxTolerantTimeExceedCount = ulong.MaxValue,
            };
            Assert.IsFalse(task.HasExecuted);
            task.Start();
            Assert.IsTrue(task.HasExecuted);
        }

        [TestMethod]
        public void TestFinished()
        {
            var frt = new FrameRateTaskExecutor<int>
            (
                () => true,
                () => false,
                10,
                () => 0
            )
            {
                AllowTimeExceed = true,
                MaxTolerantTimeExceedCount = ulong.MaxValue,
            };
            frt.Start();
            Assert.IsTrue(frt.Finished);
        }

        [TestMethod]
        public void TestCondition()
        {
            bool cond = true;
            int cnt = 0;
            new FrameRateTaskExecutor<int>
            (
                () => cond,
                () =>
                {
                    Assert.IsTrue(cond);
                    Assert.IsTrue(cnt < 5);
                    if (cnt == 4)
                    {
                        cond = false;
                    }
                    ++cnt;
                },
                50,
                () => 0
            )
            {
                AllowTimeExceed = true,
                MaxTolerantTimeExceedCount = ulong.MaxValue,
            }.Start();
            Assert.AreEqual(5, cnt);
            Assert.IsFalse(cond);
        }

        [TestMethod]
        public void TestBreakOut()
        {
            int cnt = 0;
            new FrameRateTaskExecutor<int>
            (
                () => true,
                () =>
                {
                    try
                    {
                        Assert.IsTrue(cnt < 5);
                        if (cnt == 4)
                        {
                            return false;
                        }
                        return true;
                    }
                    finally
                    {
                        ++cnt;
                    }
                },
                50,
                () => 0
            )
            {
                AllowTimeExceed = true,
                MaxTolerantTimeExceedCount = ulong.MaxValue,
            }.Start();
            Assert.AreEqual(5, cnt);
        }

        [TestMethod]
        public void TestValueTypeResult()
        {
            const int result = 8888;
            var frt = new FrameRateTaskExecutor<int>
            (
                () => true,
                () => false,
                10,
                () => result
            );
            frt.Start();
            Assert.IsTrue(frt.Finished);
            Assert.AreEqual(frt.Result, result);
        }

        [TestMethod]
        public void TestReferenceTypeResult()
        {
            object result = new object();
            var frt = new FrameRateTaskExecutor<object>
            (
                () => true,
                () => false,
                10,
                () => result
            );
            frt.Start();
            Assert.IsTrue(frt.Finished);
            Assert.ReferenceEquals(result, frt.Result);  // frt.Result and result should reference to the same object
        }
    }
}
