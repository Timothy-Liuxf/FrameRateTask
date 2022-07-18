/*

 * FrameRateTask

 * Copyright © 2022 Timothy-LiuXuefeng

 * MIT LICENSE

 */

using System;
using System.Threading;

namespace Timothy.FrameRateTask
{

	/// <summary>
	/// The class intends to execute a task which needs to be executed repeatedly every less than one second accurately.
	/// </summary>
	/// <typeparam name="TResult">The type of the return value of the task.</typeparam>
	public class FrameRateTaskExecutor<TResult>
	{
		/// <summary>
		/// The current actual frame rate.
		/// </summary>
		public uint FrameRate
		{
			get => (uint)Interlocked.CompareExchange(ref frameRate, 0, 0);
			private set => Interlocked.Exchange(ref frameRate, value);
		}
		private long frameRate;

		/// <summary>
		/// Gets a value indicating whether or not the task has finished.
		/// </summary>
		/// <returns>
		/// true if the task has finished; otherwise, false.
		/// </returns>
		public bool Finished
		{
			get => Interlocked.CompareExchange(ref finished, 0, 0) != 0;
			set => Interlocked.Exchange(ref finished, value ? 1 : 0);
		}
		private int finished = 0;

		/// <summary>
		/// Gets a value indicating whether or not the task has started.
		/// </summary>
		/// <returns>
		/// true if the task has started; otherwise, false.
		/// </returns>
		public bool HasExecuted { get => Interlocked.CompareExchange(ref hasExecuted, 0, 0) != 0; }
		private int hasExecuted = 0;
		private bool TrySetExecute()
		{
			if (Interlocked.Exchange(ref hasExecuted, 1) != 0)
			{
				return false;
			}
			return true;
		}

		private TResult result;
		/// <summary>
		/// Get the return value of the task.
		/// </summary>
		/// <exception cref="TaskNotFinishedException">
		///		The task hasn't finished.
		/// </exception>
		public TResult Result
		{
			get
			{
				if (!Finished) throw new TaskNotFinishedException();
				return result;
			}
			private set => result = value;
		}

		/// <summary>
		/// Gets or sets whether it allows time exceeding.
		/// </summary>
		/// <remarks>
		/// If it is set false, the task will throw Timothy.FrameRateTask.TimeExceedException when the task cannot finish in the given time.
		/// The default value is true.
		/// </remarks>
		public bool AllowTimeExceed
		{
			get;
#if NET5_0_OR_GREATER
            init;
#else
			set;
#endif
		} = true;

		/// <summary>
		/// It will be called once time exceeds.
		/// </summary>
		/// <remarks>
		/// parameter bool: If it is called because of the number of time exceeding is greater than MaxTolerantTimeExceedCount, the argument is true; if it is called because of exceeding once, the argument is false.
		/// </remarks>
		public Action<bool> TimeExceedAction
		{
			get;
#if NET5_0_OR_GREATER
            init;
#else
			set;
#endif
		} = callByExceed => { };

		/// <summary>
		/// Gets or sets the maximum count of time exceeding continuously.
		/// </summary>
		/// <remarks>
		/// The value is 5 for default.
		/// </remarks>
		public ulong MaxTolerantTimeExceedCount
		{
			get;
#if NET5_0_OR_GREATER
            init;
#else
			set;
#endif
		} = 5;

		/// <summary>
		/// Start this task synchronously.
		/// </summary>
		/// <exception cref="TaskStartedMoreThanOnceException">
		/// the task has started.
		/// </exception>
		public void Start()
		{
			if (!TryStart()) throw new TaskStartedMoreThanOnceException();
		}
		/// <summary>
		/// Try to start this task synchronously.
		/// </summary>
		/// <returns>
		/// true if the task starts successfully; false if the task has started.
		/// </returns>
		public bool TryStart()
		{
			if (!TrySetExecute()) return false;
			loopFunc();
			return true;
		}

		private Action loopFunc;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="loopCondition">If you want to continue to loop, return true; otherwise, return false.</param>
		/// <param name="loopToDo">If you want to break out, return false; otherwise, return true.</param>
		/// <param name="timeInterval">The time interval between two execution.</param>
		/// <param name="finallyReturn">Used to set the return value. It will be called after the loop.</param>
		/// <param name="maxTotalDuration">The maximum time for the loop to run.</param>
		public FrameRateTaskExecutor
			(
				Func<bool> loopCondition,
				Func<bool> loopToDo,
				long timeInterval,
				Func<TResult> finallyReturn,
				long maxTotalDuration = long.MaxValue
			)
		{

			if (timeInterval <= 0L && timeInterval > 1000L)
			{
				throw new IllegalTimeIntervalException();
			}
			FrameRate = (uint)(1000L / timeInterval);

			loopFunc = () =>
			{
				ulong timeExceedCount = 0UL;
				long lastLoopEndingTickCount, beginTickCount;

				var nextTime = (lastLoopEndingTickCount = beginTickCount = Environment.TickCount64) + timeInterval;
				var endTime = beginTickCount < long.MaxValue - maxTotalDuration ? beginTickCount + maxTotalDuration : long.MaxValue;

				uint loopCnt = 0;
				var nextCntTime = beginTickCount + 1000L;

				while (loopCondition() && nextTime <= endTime)
				{
					if (!loopToDo()) break;

					var nowTime = Environment.TickCount64;
					if (nextTime >= nowTime)
					{
						timeExceedCount = 0UL;
						Thread.Sleep((int)(nextTime - nowTime));
					}
					else
					{
						++timeExceedCount;
						if (timeExceedCount > MaxTolerantTimeExceedCount)
						{
							if (AllowTimeExceed)
							{
								TimeExceedAction(true);
								timeExceedCount = 0UL;
								nextTime = Environment.TickCount64;
							}
							else
							{
								throw new TimeExceedException();
							}
						}
						else if (AllowTimeExceed) TimeExceedAction(false);
					}

					lastLoopEndingTickCount = nextTime;
					nextTime += timeInterval;
					++loopCnt;
					if (Environment.TickCount64 >= nextCntTime)
					{
						nextCntTime = Environment.TickCount64 + 1000L;
						FrameRate = loopCnt;
						loopCnt = 0;
					}
				}

				result = finallyReturn();
				Interlocked.MemoryBarrierProcessWide();
				Finished = true;
			};
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="loopCondition">If you want to continue to loop, return true; otherwise, return false.</param>
		/// <param name="loopToDo">Loop to do.</param>
		/// <param name="timeInterval">The time interval between two execution.</param>
		/// <param name="finallyReturn">Used to set the return value. It will be called after the loop.</param>
		/// <param name="maxTotalDuration">The maximum time for the loop to run.</param>
		public FrameRateTaskExecutor
			(
				Func<bool> loopCondition,
				Action loopToDo,
				long timeInterval,
				Func<TResult> finallyReturn,
				long maxTotalDuration = long.MaxValue
			) : this(loopCondition, () => { loopToDo(); return true; }, timeInterval, finallyReturn, maxTotalDuration) { }
	}
}
