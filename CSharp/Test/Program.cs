using System;
using System.Threading;
using System.Threading.Tasks;
using Timothy.FrameRateTask;

namespace Test
{
	class Program
	{
		static void Demo1()			// The most common circumstance
		{
			Random r = new Random();
			int cnt = 0;
			FrameRateTaskExecutor<int> executor = new FrameRateTaskExecutor<int>
				(
					() => true,
					() =>
					{
						Console.WriteLine($"The {++cnt}-th frame! At TickCount: {Environment.TickCount64}");
					},
					50,
					() => { return 0; },
					maxTotalDuration: 2000
				)
			{ 
				AllowTimeExceed = true,
				TimeExceedAction = b => { Console.WriteLine(b ? "Reset time!" : "Time exceed for once!"); }, 
				MaxTolerantTimeExceedCount = 5		// If time exceeds
			};
			Task.Run
				(
					() =>
					{
						while (!executor.Finished)
						{
							Console.WriteLine($"Now framerate: { executor.FrameRate }");
							Thread.Sleep(1000);
						}
					}
				);
			executor.Start();
		}

		static void Demo2()			// The most common circumstance.
		{
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
							return false;		//  return false to break out.
						}
						return true;
					},
					200,    // 200 milliseconds to calculate once.
					() => sum
				)
			{
				MaxTolerantTimeExceedCount = ulong.MaxValue			// Set it to ulong.MaxValue in case time exceeding could cause the result incorrect.
			};
			executor.Start();
			Console.WriteLine($"result: {executor.Result}");
		}
		static void Demo3()
		{
			Random r = new Random();
			new FrameRateTaskExecutor<int>
				(
					() => true,
					() => { Thread.Sleep(r.Next(0, 100)); },        // Time consuming, cannot finish the task.
					50,
					() => 0,
					maxTotalDuration: 2000
				)
			{ 
				AllowTimeExceed = true,			// It will not throw exceptions.
				MaxTolerantTimeExceedCount = 2, 
				TimeExceedAction = b => { Console.WriteLine(b ? "Reset!!!" : "Exceed!"); } }
			.Start();

			try
			{
				new FrameRateTaskExecutor<int>
				(
					() => true,
					() => { Thread.Sleep(r.Next(0, 100)); },        // Time consuming, cannot finish the task.
					50,
					() => 0
				)
				{ 
					AllowTimeExceed = false,        // It will throw exceptions if time excees too many times in a series (greater than MaxTolerantTimeExceedCount).
					MaxTolerantTimeExceedCount = 2, 
					TimeExceedAction = b => { Console.WriteLine(b ? "Reset!!!" : "exceed once"); } }
				.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine("The task cannot be finished!!!\n" + e.Message);
			}
		}

		static void Demo4()
		{
			Random r = new Random();
			int tm = 0;
			FrameRateTaskExecutor<int> executor = new FrameRateTaskExecutor<int>
				(
					() => true,
					() => 
					{
						tm = r.Next(0, 280);	// Time consuming, but can finish the task at most time.
						Console.WriteLine($"Loop once! Sleep time: {tm}; At tickcount: {Environment.TickCount64}");
						Thread.Sleep(tm);
					},
					200,
					() => 0,
					maxTotalDuration: 10000
				)
			{ AllowTimeExceed = true, MaxTolerantTimeExceedCount = ulong.MaxValue, TimeExceedAction = b => { if (b) Console.WriteLine("Reset!"); } };
			Task.Run
				(
					() =>
					{
						while (!executor.Finished)
						{
							Console.WriteLine($"Now framerate: { executor.FrameRate }");
							Thread.Sleep(1000);
						}
					}
				);
			executor.Start();
		}

		static void Demo5()
		{
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
								Console.WriteLine($"Now framerate: { executor.FrameRate }");
								Thread.Sleep(1000);
							}
						}
					);
				executor.Start();
			}
			Console.WriteLine("You can see it is stable after it becomes clear.");

			Console.WriteLine("\n::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
			Console.WriteLine("====================================================================");
			Console.WriteLine("::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::\n");

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
								Console.WriteLine($"Now framerate: { executor.FrameRate }");
								Thread.Sleep(1000);
							}
						}
					);
				executor.Start();
				Console.WriteLine("You can see it is NOT stable after it becomes clear.");
			}
		}

		static void Main(string[] args)
		{
			Console.WriteLine("\n============================= Demo1 =============================\n");
			Demo1();
			Console.WriteLine("\n============================= Demo2 =============================\n");
			Demo2();
			Console.WriteLine("\n============================= Demo3 =============================\n");
			Demo3();
			Console.WriteLine("\n============================= Demo4 =============================\n");
			Demo4();
			Console.WriteLine("\n============================= Demo5 =============================\n");
			Demo5();
		}
	}
}
