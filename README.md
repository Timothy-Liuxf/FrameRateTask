# FrameRateTask

---

## Introduction

This project is initially written for [THUAI4](https://github.com/eesast/THUAI4) to support some functions.

This project intends to build an engine to support to execute tasks which need to be execute repeatedly and frequently, and the time interval between two executing need to be accurately stable. And this engine can also provide the real-time frame rate. Up to now, this time interval should be no more than 1 second. It can be used to control the framerate of a game, and control the frame rate of communication through network, etc. 

The source code of the dll is in the project FrameRateTask, and the example of usage is in the project Test.

>  本项目的编写最初是为了 [THUAI4](https://github.com/eesast/THUAI4) 来实现某些特定功能。  
>
>  本项目旨在构建一个可以重复、高频率执行一个任务的引擎，并且每两次执行的时间间隔有精确和稳定的需求。并且，本引擎还可以提供实时的帧率。目前本引擎仅支持的两次执行的时间间隔小于一秒钟。本引擎可以用来控制游戏帧数、网络通信帧率，等等。  
>
>  dll 的源代码在 FrameRateTask 项目中，使用示例代码在 Test 项目中。



## Get This Package

To get this package, please enter nuget package page: [https://www.nuget.org/packages/FrameRateTask/](https://www.nuget.org/packages/FrameRateTask/)

or use .NET CLI:  

```shell
$ dotnet add package FrameRateTask
```



## Author

Autor: Timothy-LiuXuefeng

Job: Undergraduate in THU, major in EE

Copyright (C) 2022 Timothy-LiuXuefeng



## LICENSE

[MIT license](./LICENSE.txt)



## Interfaces  

### CSharp  

#### `class FrameRateTaskExecutor<TResult>`

`TResult`: The return value of the task.

+ Constructor:

  + `FrameRateTaskExecutor(Func<bool> loopCondition, Func<bool> loopToDo, long timeInterval, Func<TResult> finallyReturn, long maxTotalDuration = long.MaxValue)`

    > ```c#
    > while (loopCondition && time <= maxTotalDuration)
    > {
    >     if (!loopToDo()) break;
    >     /*Do something to delay.*/
    > }
    > return finallyReturn;
    > ```
    >
    > + `loopCondition`: The condition to judge if the loop will continue. 是否继续循环的判断条件。
    > + `loopToDo`: The loop body. If it returns false, jump out the loop. 循环体。如果返回 `false`，跳出循环。
    > + `timeInterval`: Time interval between two executing in milliseconds. 两次循环体执行的时间间隔，单位是毫秒。
    > + `finallyReturn`: Specify the last thing to do and the return value. 指定最后要做的事情和返回值。
    > + The maximum time in total for this task in milliseconds, `long.MaxValue` for default. 整个任务执行的最长时间，单位是毫秒。

  + `FrameRateTaskExecutor(Func<bool> loopCondition, Action loopToDo, long timeInterval, Func<TResult> finallyReturn, long maxTotalDuration = long.MaxValue)`

    > The only thing that is different from the last constructor is that `loopToDo` has no return value so you cannot jump out the loop through `loopToDo`.
    >
    > 与上一个构造函数唯一的不同是，`loopToDo` 没有返回值，因此你不能通过它跳出循环体。

+ `public uint FrameRate { get; }`

  > The real-time framerate, initialized with expected framerate, changed when the task is running. 任务执行的实时帧率，初始化为期待的帧率，会在任务执行时被改变。  

+ `public bool Finished { get; }`

+ `public bool HasExecuted { get; }`

+ `public TResult Result { get; }`

  > The return value of the task. It will throw an exception if the task hasn't finished. 
  >
  > 任务的返回值。如果任务未执行完毕，将会抛出异常。

+ `public bool AllowTimeExceed { get; init; }`

  > Whether the engine allow time exceeding. `true` fir default. Details are under `MaxTolerantTimeExceedCount`.
  >
  > 是否允许执行超时。默认为 `true`。详情参见 `MaxTolerantTimeExceedCount`。

+ `public Action<bool> TimeExceedAction { get; init; }`

  > It will be called when time exceeds. Details are under `MaxTolerantTimeExceedCount`.
  >
  > 将在超时后被调用。详情参见 `MaxTolerantTimeExceedCount`。

+ `public ulong MaxTolerantTimeExceedCount { get; init; }`

  > The maximum number of time exceeding in a series. `5` for default. Once time exceeds, if the number of time exceeding in a series is no more than `MaxTolerantTimeExceedCount`, `TimeExceedAction` will be called with argument `false`, otherwise, if `AllowTimeExceed` is set `true`, `TimeExceedAction` will be called with argument `true`, otherwise (`AllowTimeExceed` is set `false`), it will throw an exception.
  >
  > Once more than `MaxTolerantTimeExceedCount`, it will automatically abandon unfinished loops, and reset the loop counter. There is an example about this in the project Test, that is, `Demo5` method.
  >
  > 允许连续超时的最大次数。默认为 `5`。一旦某次循环执行超时，如果连续超时次数不超过 `MaxTolerantTimeExceedCount`，`TimeExceedAction` 会被调用，且参数为 `false`；否则，如果 `AllowTimeExceed` 为 `true`，`TimeExceedAction` 会被调用，且参数为 `true`；如果 `AllowTimeExceed` 为 `false`，将会抛出异常。
  >
  > 一旦连续超时的次数大于 `MaxTolerantTimeExceedCount`，未完成的循环将会被舍弃，并重新进行循环计数。在 Test 项目中有一个关于本条的例子，即 `Demo5` 方法。

