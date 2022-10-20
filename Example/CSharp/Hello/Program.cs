using Timothy.FrameRateTask;

Random r = new Random();
int cnt = 0;
FrameRateTaskExecutor<int> executor = new FrameRateTaskExecutor<int>
    (
        () => true,
        () =>
        {
            Console.WriteLine($"Hello! (The {++cnt}-th frame. At TickCount: {Environment.TickCount64})");
        },
        50,
        () => { return 0; },
        maxTotalDuration: 2000
    )
{
    AllowTimeExceed = true,
    TimeExceedAction = b => { Console.WriteLine(b ? "Reset time!" : "Time exceed for once!"); },
    MaxTolerantTimeExceedCount = 5      // If time exceeds
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
