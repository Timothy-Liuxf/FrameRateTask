# FrameRateTask

---

## 介绍

`FrameRateTask` 是一款帧率稳定器，用于以固定帧率执行任务。

本项目旨在构建一个可以在单线程中重复、高频率执行一个任务的引擎，并且每两次执行的时间间隔有精确和稳定的需求。并且，本引擎还可以提供实时的帧率。目前本引擎仅支持的两次执行的时间间隔小于一秒钟。本引擎可以用来控制游戏帧数、网络通信帧率，等等。  

源代码在 FrameRateTask 项目中，使用示例代码在 Test 项目中。

本项目的编写最初是为了 [THUAI4](https://github.com/eesast/THUAI4) 来实现某些特定功能。  

## 获取此 Nuget 包

要获取此 Nuget 包，请访问 Nuget 页面：[https://www.nuget.org/packages/FrameRateTask/](https://www.nuget.org/packages/FrameRateTask/)

或者使用 .NET 命令行工具：  

```shell
$ dotnet add package FrameRateTask
```

## 作者

作者：Timothy-LiuXuefeng

Copyright (C) 2022 Timothy-LiuXuefeng

## 开源协议

[MIT license](https://github.com/Timothy-LiuXuefeng/FrameRateTask/blob/master/LICENSE.txt)

## 向本仓库贡献代码

在贡献代码前，请认真阅读 [CONTRIBUTING](./CONTRIBUTING.zh-CN.md)。

## 接口

### CSharp  

#### `class FrameRateTaskExecutor<TResult>`

`TResult`：任务的返回值。  

+ 构造方法：

  + `FrameRateTaskExecutor(Func<bool> loopCondition, Func<bool> loopToDo, long timeInterval, Func<TResult> finallyReturn, long maxTotalDuration = long.MaxValue)`

    如果该对象使用此构造方法构造，那么当调用该对象的 `Start` 或 `TryStart` 方法时，程序等价于下面的代码：  

    ```c#
    while (loopCondition && time <= maxTotalDuration)
    {
     if (!loopToDo()) break;
     /* 延迟直到下一帧到来 */
    }
    return finallyReturn;
    ```

    + `loopCondition`：是否继续循环的判断条件。
    + `loopToDo`：循环体。如果返回 `false`，跳出循环。
    + `timeInterval`：两次循环体执行的时间间隔，单位是毫秒。
    + `finallyReturn`：指定最后要做的事情和返回值。
    + `maxTotalDuration`：整个任务执行的最长时间，单位是毫秒，默认值为 `long.MaxValue`。

  + `FrameRateTaskExecutor(Func<bool> loopCondition, Action loopToDo, long timeInterval, Func<TResult> finallyReturn, long maxTotalDuration = long.MaxValue)`

    与上一个构造函数唯一的不同是，`loopToDo` 没有返回值，因此你不能通过它跳出循环体。

+ `public void Start()`

  开始执行任务。如果任务已经开始，将会抛出 `Timothy.FrameRateTask.TaskStartedMoreThanOnceException` 异常。否则开始执行任务，执行完毕后返回。

+ `public bool TryStart()`

  尝试开始执行任务。如果任务已经开始，返回 `false`，否则开始执行该任务。任务执行完毕后返回 `true` 。

+ `public uint FrameRate { get; }`

  任务执行的实时帧率，初始化为期待的帧率，会在任务执行时被改变。  

+ `public bool Finished { get; }`

  任务是否已经完成。

+ `public bool HasExecuted { get; }`

  任务是否已经开始执行。

+ `public TResult Result { get; }`

  任务的返回值。如果任务未执行完毕，将会抛出 `Timothy.FrameRateTask.TaskNotFinishedException` 异常。

+ `public bool AllowTimeExceed { get; init; }`

  是否允许执行超时，默认为 `true`。详情参见 `MaxTolerantTimeExceedCount` 的说明。

+ `public Action<bool> TimeExceedAction { get; init; }`

  将在超时后被调用。详情参见 `MaxTolerantTimeExceedCount` 的说明。

+ `public ulong MaxTolerantTimeExceedCount { get; init; }`

  允许连续超时的最大次数，默认为 `5`。一旦某次循环执行超时，如果连续超时次数不超过 `MaxTolerantTimeExceedCount`，`TimeExceedAction` 会被调用，且参数为 `false`。否则，如果 `AllowTimeExceed` 为 `true`，`TimeExceedAction` 会被调用，且参数为 `true`；如果 `AllowTimeExceed` 为 `false`，将会抛出 `Timothy.FrameRateTask.TimeExceedException` 异常。

  一旦连续超时的次数大于 `MaxTolerantTimeExceedCount`，未完成的循环将会被舍弃，并重新进行循环计数。样例 `TemporaryCongestion `就是关于本条的样例代码。


