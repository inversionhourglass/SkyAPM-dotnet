# Why This

> If you use English, please use the translation tool to translate first, the English version will be available later

**下面的说明都是基于我目前对SkyWalking的理解，如果与实际存在偏差，希望能够指出，谢谢。**

大家如果对SkyWalking的基础概念比较清楚，应该对`trace`, `segment`, `span`这三个名词比较熟悉了。一条调用链是一个`trace`可跨多个服务；一个`trace`包含一个或多个`segment`，简单场景中一个服务的单次API处理就是一个`segment`，实际设计中跨线程任务便会产生一个新的`segment`，所以一个服务的单次API处理如果包含跨线程任务，那么就应该产生多个`segment`；一个`segment`包含一个或多个`span`，`span`是SkyWalking的基本单位，简单理解为一次方法调用、一次IO处理都是最基本的`span`。

除了上面说的三个名词，这里再介绍一个名词`tracingContext`，`tracingContext`与`segment`一对一关联，`tracingContext`是关联的`segment`的管理入口，为`segment`新增`span`以及`segment`之间的关联关系都是通过`tracingContext`进行的。一个服务必然是需要同时处理多个请求的，所以同一时间就会存在多个`tracingContext`，在java中可以简单的使用`ThreadLocal`来存储这多个`tracingContext`，但是在.NET中却无法如此实现，下面介绍.NET所面临的问题。

在现在的.NET编程中，异步编程已经成为了基本的编程技巧，同时我们知道异步编程中一次`await`IO操作一般都是会切换线程的
```csharp
// thread 1
await client.ConnectAsync();
// thread 2
```
而这就是无法使用`ThreadLocal`的原因，.NET异步编程带来的线程切换是无处不在的，一个请求处理下来可能已经切换了数个线程，如果使用`ThreadLocal`存储`tracingContext`，那么就会出现一个新请求进来后发现当前`tracingContext`已存在，以及`span`在结束时无法找到对应的`tracingContext`或找到错误的`tracingContext`。对于这种情况，在以前的framework版本(具体版本忘了，懒得查).NET提供了`CallContext`，在现在的版本中可以使用`AsyncLocal`。但事情到这里并没有结束，虽然解决了`tracingContext`的存储问题（但其实并没有完全解决，考虑下面代码的第四个场景，这里暂时不做说明），但是异步编程带来的问题却并不仅仅如此，我们在异步编程的过程中可能会出现以下几种使用场景：
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
上述的四种种情况在日常编程中都是常用的方式，所以在实现SkyAPM agent的时候也是必须考虑的，上述四种情况中的第三种和第四种是最令人头疼的，因为`O3`以及`M4`中手动创建的线程可能在整个请求结束之后再结束，也就是`segment`已完成但子`span`还没有完成（这里暂时不再深入了，如果有兴趣可以到 [再聊聊](#再聊聊) 中看看）。针对于.NET异步编程的特点，官方的实现中通过一种简单却很聪明的方式进行解决，在最开始有介绍到一个`segment`包含一个或多个`span`，官方设计中.NET的一个`segment`仅包含一个`span`，这种方式是能完美解决所有场景的，但从官方的设计来看（我以为的）却有些不妥，官方设计中`segment`可以并且一般是包含多个`span`的，.NET中一个`segment`仅包含一个`span`就导致.NET变相丢失了一个`span`这个维度，.NET Agent的这个设定目前能够想到的所带来的问题有：
- `segment`数据冗余，原本多个`span`共享同一个`segment`数据，现在每个`span`都有各自的`segment`，那么`segment`上的公共信息必然产生冗余
- 影响存储索引性能，以ES存储为例，ES在存储trace数据时是以`segment`维度来存储的，一个`segment`对应一个`document`，原本属于一个`segment`的多个`span`将存储为一个`document`，在.NET实现中每个`span`都将存储为一个`document`，这就等同于以前存储的基本单位是`segment`现在是`span`
- oap-server的分析压力存在未知影响，对oap-server的整体分析逻辑还不是那么了解，感觉上是有影响的，具体影响有多大还是未知数
- oap-server分析结果的差异，oap-server在对数据进行分析时是按五种数据类型进行分析的`segment, entrySpan, localSpan, exitSpan, firstSpan`，由于.NET中一个`segment`仅包含一个`span`，那么每个`span`都将作为`firstSpan`进行分析，目前（9.3.0）`firstSpan`只是在存储部分进行分析，不确定以后会不会使用`firstSpan`做其他数据分析
- 界面查询差异，如果贵公司的技术体系是多语言的，那么仅从界面使用上可能就会发现一些差异，比如Java语言统计的Endpoint都是该应用提供的API路径，但是.NET的Endpoint包含被调用服务的URL，这其实也是第一点中说到的，每个`span`都是一个`segment`，因为Endpoint数据是从`segment`中读取的，Endpoint值就是`segment`的first span的OperationName，由于每个`segment`只有一个`span`，所以每个`span`都是first span

当前库目的就是尽量按照官方设计（我以为的）来还原`segment`和`span`的关联关系，官方的.NET实现在我看来最小单位是`segment`，因为一个`segment`仅包含一个`span`，该库将还原`span`作为基本单位，所以该库取名为`spanable`。**需要提前说明的是，该库并不能像官方库那样完美的解决前面代码给出的所有场景**，目前能够完美解决前两种场景，对于第三种和第四种不等待任务完成的场景需要手动接入`SkyAPM`埋点才能完美解决，这种侵入性强的当前`2.0.1.1`版本不做介绍，默认不进行手动埋点的情况下，第三第四种情况的未完成`span`会被截断。如果你想尝试该库，可以查看 [How to use spanable](How-to-use-spanable.md)

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
在前面有提到该库在不手动添加埋点的情况下无法完美处理第三和第四种场景，主要原因在于前两种场景下无论是串行执行还是并行执行，所有的子任务都会在顶级任务完成之前完成，而第三第四种场景会出现子任务可能在顶级任务结束后依旧在执行的情况。在spanable开始的设计中，对于这种情况会对该`segment`延迟一段时间(`SkyWalking:Spanable:DelaySeconds`)再尝试提交，给后台任务一定的执行时间，如果超过了延迟时间依旧未完成，那么就会强制截断提交，为什么这么做后面再聊。在后面的尝试中发现，这种延迟设计会导致`segment`总体耗时不正确，其主要原因是官方的oap-server对segment耗时的计算方式与我想象中不一样，如下面代码所示，官方计算segment耗时的方式是取segment中所有span的最小开始时间和所有span的最大结束时间之差，那么在这里所有span的最大结束时间取的就是后台span结束的时间，这个耗时其实是不符预期的，顶级span的开始结束时间之差才是真正的耗时，https://github.com/apache/skywalking/blob/49594c4db1973c20790f55581f02245ad50a9b2f/oap-server/analyzer/agent-analyzer/src/main/java/org/apache/skywalking/oap/server/analyzer/provider/trace/parser/listener/SegmentAnalysisListener.java#L127-L138
```java
segmentObject.getSpansList().forEach(span -> {
	if (startTimestamp == 0 || startTimestamp > span.getStartTime()) {
		startTimestamp = span.getStartTime();
	}
	if (span.getEndTime() > endTimestamp) {
		endTimestamp = span.getEndTime();
	}
	isError = isError || segmentStatusAnalyzer.isError(span);
	appendSearchableTags(span);
});
final long accurateDuration = endTimestamp - startTimestamp;
duration = accurateDuration > Integer.MAX_VALUE ? Integer.MAX_VALUE : (int) accurateDuration;
```
对于这里的耗时逻辑也提有discuss询问官方，不过官方表示并不会对代码逻辑进行解释，这个也可以理解，如果每个人都去问代码逻辑，确实忙不过来。那么因为这段耗时计算逻辑，所以spanable一开始设计的延迟发送就不再使用，但是目前只是把延迟发送的时间的默认值改为了0表示不延迟，并没有直接移除这个逻辑，因为始终觉得这个设计是好的，后续应该会fork一份oap-server单独修改来启用这个逻辑。

既然有设计延迟发送的逻辑，为什么不像官方的`AsyncSpan`那样等到所有的`AsyncSpan`完成后再发送呢。这里主要考虑到.NET的特殊性，最前面有介绍到.NET无法像Java那样直接用`ThreadLocal`来存储`tracingContext`，.NET使用的是`CallContext/AsyncLocal`（后面都用`AsyncLocal`说明），`AsyncLocal`的内部实现大家可能大概知道，简单来说就是在线程切换的时候就会把`AsyncLocal`中存储的数据复制到下一个线程中，在前面有说到await 异步IO操作会进行线程切换，同样的`Task/Thread/Timer`等都可以创建线程并进行线程切换，这些同样会复制`AsyncLocal`数据，并且这个操作是自动的。对于这些切换线程的后台任务，有时候我们可能只是在执行玩一个业务后调用了一个接口发送一个通知或推送一个消息，我们并不希望这个操作阻塞主逻辑，所以不等待这个操作执行完毕，对于这种场景一般异步任务的执行时间也不会很久，我们会希望尽可能完整记录下这段异步任务，所以延迟发送就在这种场景下用上了；而另一种情况我们在代码中新建了一个线程跑一个长任务，此时我们并不会希望等待这个长任务执行完毕，那么延迟时间不进行无限等待也就在这种场景下派上用场了，当然其实这种场景最合适的是为这个长任务新建一个segment，但在.NET中难以通过Agent直接实现，一般需要手动埋点。

## 如何选择
从通用性来说，官方库的通用性更好，所有场景都能适用，spanable的优势以及目标主要就在于压缩数据量，所以如何选择推荐从以下几点考虑：
|官方库|spanable|
|:--:|:--:|
|代码写法多样，常有异步不等待完成任务<br/>数据总量较小，采样率保持拉满无压力|异步不等待完成任务较少，或能够接受少量手动埋点<br/>不在意异步未完成任务的trace数据被截断<br/>采样数据压力大，期望减小数据压力<br/>减少.NET采样数据与其他采样数据的差异，以及界面显示的差异|