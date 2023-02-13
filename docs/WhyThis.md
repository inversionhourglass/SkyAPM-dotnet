# Why This

> If you use English, please use the translation tool to translate first, the English version will be available later

**下面的说明都是基于我目前对SkyWalking的理解，如果与实际存在偏差，希望能够指出，谢谢。**

大家如果对SkyWalking的基础概念比较清楚，应该对`trace`, `segment`, `span`这三个名词比较熟悉了。一条调用链是一个`trace`可跨多个服务；一个`trace`包含一个或多个`segment`，简单场景中一个服务的单次API处理就是一个`segment`，实际设计中跨线程任务便会产生一个新的`segment`，所以一个服务的单次API处理如果包含跨线程任务，那么就会产生多个`segment`；一个`segment`包含一个或多个`span`，`span`是SkyWalking的基本单位，简单理解一次方法调用、一次IO处理都是最基本的`span`。

除了上面说的三个名词，这里再介绍一个名词`tracingContext`，`tracingContext`与`segment`一对一关联，`tracingContext`是关联的`segment`的管理入口，为`segment`新增`span`、`segment`之间的关联都是通过`tracingContext`进行的，一个服务必然是需要同时处理多个请求的，所以同一时间就会存在多个`tracingContext`，在java中可以简单的使用`ThreadLocal`来存储这多个`tracingContext`，但是在.NET中却无法如此实现，下面介绍.NET所面临的问题。

在现在的.NET编程中，异步编程已经成为了基本的编程技巧，同时我们知道异步编程中一次`await`IO操作一般都是会切换线程的
```csharp
// thread 1
await client.ConnectAsync();
// thread 2
```
而这就是无法使用`ThreadLocal`的原因，.NET异步编程带来的线程切换是无处不在的，一个请求处理下来可能已经切换了数个线程，如果使用`ThreadLocal`存储`tracingContext`，那么就会出现一个新请求进来后发现当前`tracingContext`已存在，以及`span`在结束时无法找到对应的`tracingContext`或找到错误的`tracingContext`。对于这种情况，在以前的framework版本(具体版本忘了，懒得查).NET提供了`CallContext`，在现在的版本中可以使用`AsyncLocal`。事情到这里并没有结束，虽然解决了`tracingContext`的存储问题（但其实并没有完全解决，考虑下面代码的第四个场景，这里暂时不做说明），但是异步编程带来的问题却并不仅仅如此，我们在异步编程的过程中可能会出现以下几种使用场景：
```csharp
// 基本的使用方式，每次都直接await
async Task M1() {
  await O1();
  await O2();
  await O3();
}

// 异步任务并行执行
async Task M2() {
  var t1 = O1();
  var t2 = O2();
  var t3 = O3();
  await Task.WhenAll(t1, t2, t3);
}

// 异步任务不等待完成
async Task M3() {
  await O1();
  await O2();
  O3();
}

// 手动创建线程不等待完成
async Task M4() {
  await O1();
  await O2();
  Task.Run(() => {
    SyncExecution();
  });
}
```
上述的四种种情况在日常编程中都是常用的方式，所以在实现SkyAPM agent的时候也是必须考虑的，上述四种情况中的第三种和第四种是最令人头疼的，因为`O3`和`M4`中手动创建的线程可能在整个请求结束之后再结束，在这里暂时不再深入了，如果有兴趣可以到 [再聊聊](#再聊聊) 中看看。针对于.NET异步编程的特点，官方的实现中通过一种简单却很聪明的方式进行解决，在最开始有介绍到一个`segment`包含一个或多个`span`，官方设计中.NET的一个`segment`仅包含一个`span`，这种方式是能完美解决所有场景的，但从官方的设计来看（我以为的）却有些不妥，官方设计中`segment`可以并且一般是包含多个`span`的，.NET中一个`segment`仅包含一个`span`就导致.NET丢失了一个维度，目前能够想到的所带来的问题是数据的冗余导致的数据量的增长，以ES存储为例，ES在存储trace数据时是以`segment`维度来存储的，一个`segment`对应一个`document`，原本属于一个`segment`的多个`span`将存储为一个`document`，在.NET实现中一个`span`将存储为一个`document`，同时`segment`的基础信息也将被每个`span`拥有一份，`document`的数量以及实际的数据量都将增加。另外在oap-server端在对采集数据进行处理时的压力也将增加，还有一个未知影响（未来）是oap-server在对数据进行分析时是按五种数据类型进行分析的`segment, entrySpan, localSpan, exitSpan, firstSpan`，由于.NET中一个`segment`仅包含一个`span`，那么每个`span`都将作为`firstSpan`进行分析，目前（9.3.0）`firstSpan`只是在存储部分进行分析，不确定以后会不会使用`firstSpan`做其他数据分析。

当前库目的就是尽量按照官方设计（我以为的）来还原`segment`和`span`的关联关系，官方的.NET实现在我看来最小单位是`segment`，所以该库取名为`spanable`。**需要提前说明的是，该库并不能像官方库那样完美的解决前面代码给出的三种场景**，目前能够完美解决前两种场景，对于第三种和第四种不等待任务完成的场景需要手动接入`SkyAPM`埋点来解决，这种侵入性强的当前`2.0.1.1`版本不做介绍。如果你想尝试该库，可以查看 [How to use spanable](How-to-use-spanable.md)

## Q&A
```text
- Q: 为什么要新建分支项目来做，怎么不提PR
  A: 尝试过，很遗憾因为一些原因未能成功，暂时这么来吧
  
- Q: BUG会不会很多，降级成本会不会很高
  A: 难免会有些BUG，考虑到降级成本问题，目前使用该项目默认还是官方的实现方式，需要通过配置启用 spanable 功能，降级也是通过配置进行
  
- Q: 这个项目后续是独立开发还是会和官方同步
  A: 官方的实现还是很好的，该项目也只是fork下来稍微改改，后续官方有新功能更新，这里也会尽量同步更新
  
- Q: 四位版本号是什么意思，怎么选择版本
  A: 前三位版本号是官方版本号，表示当前版本是从官方的某个版本修改而来，最后一个版本号就是该项目的自增版本号，选择版本根据官方版本内容来选择即可
```

## 再聊聊
懒，下次再写