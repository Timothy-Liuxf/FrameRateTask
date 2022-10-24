using Timothy.FrameRateTask;

int i = 1, sum = 0;
Random r = new Random();
FrameRateTaskExecutor<int> executor = new FrameRateTaskExecutor<int>
    (
        () => i <= 10,
        () =>
        {
            Console.WriteLine($"Calculate once: {sum += i++}");
            if (r.Next(0, 5) > 3)
            {
                Console.WriteLine("Break out early!");
                return false;       //  return false to break out.
            }
            return true;
        },
        200,    // 200 milliseconds to calculate once.
        () => sum
    )
{
    MaxTolerantTimeExceedCount = ulong.MaxValue         // Set it to ulong.MaxValue in case time exceeding could cause the result incorrect.
};
executor.Start();
Console.WriteLine($"result: {executor.Result}");
