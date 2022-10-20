using Timothy.FrameRateTask;

Random r = new Random();
int tm = 0;
FrameRateTaskExecutor<int> executor = new FrameRateTaskExecutor<int>
    (
        () => true,
        () =>
        {
            tm = r.Next(0, 280);    // Time consuming, but can finish the task at most time.
            Console.WriteLine($"Loop once! Sleep time: {tm}; At tickcount: {Environment.TickCount64}");
            Thread.Sleep(tm);
        },
        200,
        () => 0,
        maxTotalDuration: 10000
    )
{
    AllowTimeExceed = true,
    MaxTolerantTimeExceedCount = ulong.MaxValue,
    TimeExceedAction = b =>
    {
        if (b)
        {
            Console.WriteLine("Reset!");
        }
    }
};
Task.Run
    (
        () =>
        {
            while (!executor.Finished)
            {
                Console.WriteLine($"Now framerate: {executor.FrameRate}");
                Thread.Sleep(1000);
            }
        }
    );
executor.Start();
