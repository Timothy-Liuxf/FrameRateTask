/*

 * FrameRateTask

 * Copyright © 2022 Timothy-LiuXuefeng

 * MIT LICENSE

 */

using System;

namespace Timothy.FrameRateTask
{
	/// <summary>
	/// This exception will be thrown when the task hasn't finished but is asked to get the return value.
	/// </summary>
	public class TaskNotFinishedException : Exception
	{
		///
		public override string Message => "The task has not finished!";
	}

	/// <summary>
	/// This exception will be thrown when the time interval specified is invalid.
	/// </summary>
	public class IllegalTimeIntervalException : Exception
	{
		///
		public override string Message => "The time interval should be positive and no more than 1000ms!";
	}

	/// <summary>
	/// This exception will be thrown when time exceeds but time exceeding is not allowed.
	/// </summary>
	public class TimeExceedException : Exception
	{
		///
		public override string Message => "The loop runs too slow that it cannot finish the task in the given time!";
	}

	/// <summary>
	/// This exception will be thrown when the task has started but is asked to execute again.
	/// </summary>
	public class TaskStartedMoreThanOnceException : Exception
	{
		///
		public override string Message => "The task has started more than once!";
	}

}
