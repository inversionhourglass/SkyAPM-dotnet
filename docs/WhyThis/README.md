# Why This

> If you use English, please use the translation tool to translate first, the English version will be available later

## 优劣势
- 主要差异：数据结构差异，官方仓库一个`Segment`仅包含一个`Span`，spanable一个`Segment`可包含多个`Span`
- 主要优势：同样的数据采集，spanable将产生数据将更小，因为多个`Span`将共用同一个`Segment`信息，而官方库每个`Span`都将包含一份`Segment`信息
- 主要劣势：.NET的异步/多线程写法多种多样，官方仓库支持所有编码方式，spanable对部分编码方式产生的数据需要进行特殊处理，特殊处理产出的数据也将会有些差异（后续介绍）
- 其他差异：
  - 存储差异：以ES为例，ES存储trace以`Segment`为基础单位进行存储，所以同样的trace结构，spanable中的一个`Segment`存储为一个document，而官方仓库将存储为多个document
  - 分析差异：官方仓库将产生更多的segment，所以oap-server的分析压力也将更大，同时官方仓库的`Span`分散在`Segment`中相关数据更加分散，spanable将更加聚合
  - 界面差异：目前UI（9.3.0）上的Trace查询界面，能够查询的Endpoint实际是segment first span的OperationName，所以官方仓库在Trace界面上可查询的Endpoint将更多，而spanable将类似java版本那样更少（主要是接收的请求的URL Path）

## 场景表现对比
### 组件场景分类
**列表中的简写名称将用于[代码场景分类](#代码场景分类)**
|简写名称|描述|
|:-----:|:---|
|官方仓库|使用官方仓库，其他配置不影响实际表现|
|spanable|启用spanable模式，其他spanable相关配置默认|
|spanable手动埋点|启用spanable模式，对于后台执行的任务增加手动埋点处理|
|spanable延迟发送|启用spanable模式，同时设置有未完成的后台span延迟发送，并且更新oap-server以支持延迟发送|

### 代码场景分类
**下面展示列表中代码场景所使用的公共代码部分：**
- Timing方法，传入参数`Func<Task>`，返回`Task<string>`，记录传入委托的执行时间并将执行时间以`string`格式返回
https://github.com/inversionhourglass/SkyAPM-dotnet/blob/eb4d7f7d2376eda16ee9086755335d55411d81b6/sample/SkyApm.Sample.Frontend/Controllers/TestController.cs#L131-L139

- BackendDelayAsync，传入参数`int`，返回`Task<string>`，内部调用backend服务`/api/delay/{delay}`接口，该接口模拟耗时服务，传入参数为毫秒，服务端等待指定毫秒后返回
https://github.com/inversionhourglass/SkyAPM-dotnet/blob/eb4d7f7d2376eda16ee9086755335d55411d81b6/sample/SkyApm.Sample.Frontend/Controllers/TestController.cs#L141-L146

***注意下表中代码场景中注释的代码都是`spanable手动埋点`场景所用代码，该场景将使用注释部分代码并且注释前一行代码***  

|代码场景|代码场景描述|官方仓库|spanable|spanable手动埋点|spanable延迟发送|
|:------|:----------|:------|:-------|:---------------|:--------------|
|https://github.com/inversionhourglass/SkyAPM-dotnet/blob/eb4d7f7d2376eda16ee9086755335d55411d81b6/sample/SkyApm.Sample.Frontend/Controllers/TestController.cs#L23-L31|普通异步场景，每次异步任务都会直接await|segment数量：[7](./normal_official_es.jpg)<br>界面展示：![](./normal_official.jpg)|segment数量：[4](./normal_spanable_es.jpg)<br>界面展示：![](./normal_spanable_es.jpg)|不需要手动埋点，效果及数据同spanable测试，跳过该测试|没有延迟完成的span，不需要延迟发送，效果及数据同spanable测试，跳过该测试|
|https://github.com/inversionhourglass/SkyAPM-dotnet/blob/eb4d7f7d2376eda16ee9086755335d55411d81b6/sample/SkyApm.Sample.Frontend/Controllers/TestController.cs#L33-L43|多任务并行处理，最终`Task.WhenAll`等待全部执行完毕|segment数量：[7](./parallel_official_es.jpg)<br>界面展示：![](./parallel_official.jpg)|segment数量：[4](./parallel_spanable_es.jpg)<br>界面展示：![](./parallel_spanable.jpg)|不需要手动埋点，效果及数据同spanable测试，跳过该测试|没有延迟完成的span，不需要延迟发送，效果及数据同spanable测试，跳过该测试|
|https://github.com/inversionhourglass/SkyAPM-dotnet/blob/eb4d7f7d2376eda16ee9086755335d55411d81b6/sample/SkyApm.Sample.Frontend/Controllers/TestController.cs#L45-L54|异步任务不等待完成直接返回，注释部分是spanable手动埋点场景代码|segment数量：[7](./background_official_es.jpg)<br>界面展示：![](./background_official.jpg)|segment数量：[4](./background_spanable_es.jpg)<br>界面展示：![](./background_spanable.jpg)|segment数量：[5](./background_spanable_manual_es.jpg)<br>界面展示：![](./background_spanable_manual.jpg)|segment数量：[4](./background_spanable_delay_es.jpg)<br>界面展示：![](./background_spanable_delay.jpg)|
|https://github.com/inversionhourglass/SkyAPM-dotnet/blob/eb4d7f7d2376eda16ee9086755335d55411d81b6/sample/SkyApm.Sample.Frontend/Controllers/TestController.cs#L56-L65|与上一个代码场景基本相同，主要区别在于开启延迟发送功能后，当前代码场景异步不等待完成的span会在延迟发送超时后才完成，主要与上一个代码场景在延迟发送场景下进行对比，注释部分是spanable手动埋点场景代码|segment数量：[7](./backgroundtimeout_official_es.jpg)<br>界面展示：![](./backgroundtimeout_official.jpg)|segment数量：[4](./backgroundtimeout_spanable_es.jpg)<br>界面展示：![](./backgroundtimeout_spanable.jpg)|segment数量：[5](./backgroundtimeout_spanable_manual_es.jpg)<br>界面展示：![](./backgroundtimeout_spanable_manual.jpg)|segment数量：[4](./backgroundtimeout_spanable_delay_es.jpg)<br>界面展示：![](./backgroundtimeout_spanable_delay.jpg)|
|https://github.com/inversionhourglass/SkyAPM-dotnet/blob/eb4d7f7d2376eda16ee9086755335d55411d81b6/sample/SkyApm.Sample.Frontend/Controllers/TestController.cs#L67-L76|手动创建`Task`且不等待其完成，注释部分是spanable手动埋点场景代码|segment数量：[7](./bgtask_official_es.jpg)<br>界面展示：![](./bgtask_official.jpg)|segment数量：[4](./bgtask_spanable_es.jpg)<br>界面展示：![](./bgtask_spanable.jpg)|segment数量：[5](./bgtask_spanable_manual_es.jpg)<br>界面展示：![](./bgtask_spanable_manual.jpg)|segment数量：[4](./bgtask_spanable_delay_es.jpg)<br>界面展示：![](./bgtask_spanable_delay.jpg)|
|https://github.com/inversionhourglass/SkyAPM-dotnet/blob/eb4d7f7d2376eda16ee9086755335d55411d81b6/sample/SkyApm.Sample.Frontend/Controllers/TestController.cs#L78-L87|手动创建`Thread`且不等待其完成，注释部分是spanable手动埋点场景代码|segment数量：[7](./bgthread_official_es.jpg)<br>界面展示：![](./bgthread_official.jpg)|segment数量：[4](./bgthread_spanable_es.jpg)<br>界面展示：![](./bgthread_spanable.jpg)|segment数量：[5](./bgthread_spanable_manual_es.jpg)<br>界面展示：![](./bgthread_spanable_manual.jpg)|segment数量：[4](./bgthread_spanable_delay_es.jpg)<br>界面展示：![](./bgthread_spanable_delay.jpg)|
|https://github.com/inversionhourglass/SkyAPM-dotnet/blob/eb4d7f7d2376eda16ee9086755335d55411d81b6/sample/SkyApm.Sample.Frontend/Controllers/TestController.cs#L112-L129|父级Segment已提交，之后再产生新的Span，主要测试spanable对这种情况的处理，注释部分是spanable手动埋点场景代码|segment数量：[7](./delaystart_official_es.jpg)<br>界面展示：![](./delaystart_official.jpg)|segment数量：[5](./delaystart_spanable_es.jpg)<br>界面展示：![](./delaystart_spanable.jpg)|segment数量：[5](./delaystart_spanable_manual_es.jpg)<br>界面展示：![](./delaystart_spanable_manual.jpg)|segment数量：[5](./delaystart_spanable_delay_es.jpg)<br>界面展示：![](./delaystart_spanable_delay.jpg)|

## Q&A
```text
- Q: 为什么要新建分支项目来做，怎么不提PR
  A: 尝试过，很遗憾因为一些原因未能成功，目前已分支项目单独做吧
  
- Q: BUG会不会很多，降级成本会不会很高
  A: 难免会有些BUG，考虑到降级成本问题，目前使用该项目默认还是官方的实现方式，需要通过配置启用 spanable 功能，降级也是通过配置进行
  
- Q: 这个项目后续是独立开发还是会和官方同步
  A: 官方的实现还是很好的，该项目也只是fork下来稍微改改，后续官方有新功能更新，这里也会尽量同步更新，同样的，如果有比较公共的功能，也会向官方项目提交PR
  
- Q: 四位版本号是什么意思，怎么选择版本
  A: 前三位版本号是官方版本号，表示当前版本是从官方的某个版本修改而来，最后一个版本号就是该项目的自增版本号，选择版本根据官方版本内容来选择即可
```