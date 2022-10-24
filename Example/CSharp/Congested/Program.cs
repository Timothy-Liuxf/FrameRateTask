using Timothy.FrameRateTask;

Random r = new Random();
new FrameRateTaskExecutor<int>
    (
        () => true,
        () =>
        {
            Console.WriteLine("Hello!");
            Thread.Sleep(r.Next(0, 100)); // Time consuming, cannot finish the task.
        },
        50,
        () => 0,
        maxTotalDuration: 2000
    )
{
    AllowTimeExceed = true,         // It will not throw exceptions.
    MaxTolerantTimeExceedCount = 2,
    TimeExceedAction = b => { Console.WriteLine(b ? "Reset!!!" : "Exceed!"); }
}
.Start();

Console.WriteLine("\n==========\n");

try
{
    new FrameRateTaskExecutor<int>
    (
        () => true,
        () =>
        {
            Console.WriteLine("Hello!");
            Thread.Sleep(r.Next(0, 100));
        },
        50,
        () => 0
    )
    {
        AllowTimeExceed = false,        // It will throw exceptions if time excees too many times in a series (greater than MaxTolerantTimeExceedCount).
        MaxTolerantTimeExceedCount = 2,
        TimeExceedAction = b => { Console.WriteLine(b ? "Reset!!!" : "exceed once"); }
    }
    .Start();
}
catch (Exception e)
{
    Console.WriteLine("The task cannot be finished!!!\n" + e.Message);
}
