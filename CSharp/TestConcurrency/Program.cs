using System;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Concurrent;
using Timothy.FrameRateTask;

namespace TestConcurrency
{
	internal class Program
	{
		static int Main(string[] args)
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
				if (!result.TryTake(out int seq) || seq != i)
				{
					Console.WriteLine();
					if (result.Count > 0)
					{
						Console.Error.WriteLine($"Get error number: { seq }");
					}
					else
					{
						Console.Error.WriteLine("Get no number!");
					}
					return 1;
				}
				Console.Write(i.ToString() + ' ');
			}
			Console.WriteLine();
			if (!result.TryTake(out int res) || res != 8888)
			{
				Console.Error.WriteLine("Get error result!");
				return 1;
			}
			Console.WriteLine(res);
			return 0;
		}
	}
}
