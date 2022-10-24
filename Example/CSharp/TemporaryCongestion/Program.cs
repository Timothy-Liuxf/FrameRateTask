using Timothy.FrameRateTask;

{
    Random r = new Random();
    int tm = 0;
    int cnt = 0;
    FrameRateTaskExecutor<int> executor = new FrameRateTaskExecutor<int>
        (
            () => true,
            () =>
            {
                tm = r.Next(0, cnt > 50 ? 2 : 200);    // Congested at first, but then clear.
                Console.WriteLine($"Loop once! Sleep time: {tm}; At tickcount: {Environment.TickCount64}");
                Thread.Sleep(tm);
                ++cnt;
            },
            50,
            () => 0,
            maxTotalDuration: 12000
        )
    {
        AllowTimeExceed = true,
        MaxTolerantTimeExceedCount = 5,     // It will drop frame once time exceeds over five times in a series. You can see the frame will be stable once it goes out of congestion.
        TimeExceedAction = b => { if (b) Console.WriteLine("Reset!"); }
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
}
Console.WriteLine("You can see it is stable after it becomes clear.");

Console.WriteLine("\n==========\n");

{
    Random r = new Random();
    int tm = 0;
    int cnt = 0;
    FrameRateTaskExecutor<int> executor = new FrameRateTaskExecutor<int>
        (
            () => true,
            () =>
            {
                tm = r.Next(0, cnt > 50 ? 2 : 200);    // Congested at first, but then clear.
                Console.WriteLine($"Loop once! Sleep time: {tm}; At tickcount: {Environment.TickCount64}");
                Thread.Sleep(tm);
                ++cnt;
            },
            50,
            () => 0,
            maxTotalDuration: 8000
        )
    {
        AllowTimeExceed = true,
        MaxTolerantTimeExceedCount = ulong.MaxValue,     // It will not drop frame at any time! You can see the frame will NOT be stable after it goes out of congestion to make up for the congestion before!
        TimeExceedAction = b => { if (b) Console.WriteLine("Reset!"); }
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
    Console.WriteLine("You can see it is NOT stable after it becomes clear.");
}
