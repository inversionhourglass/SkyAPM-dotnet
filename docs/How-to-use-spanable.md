# How to use spanable

## 如何替换官方包
解决方案全局替换，使用正则进行替换，查找规则: `[Ii]nclude="SkyAPM\.([^"]+?)" *[Vv]ersion=".+"`，替换为：`Include="SpanableSkyAPM.$1" Version="<version>"`，注意把替换值里的`<version>`改为目标版本，替换完之后执行`dotnet restore`还原NuGet。如果你使用Agent自动注入SkyAPM，那么需要把环境变量`ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`从`SkyAPM.Agent.AspNetCore`修改为`SpanableSkyAPM.Agent.AspNetCore`。

## 如何启用spanable
默认情况下还是使用官方的方式生成trace数据，该项目在官方项目的配置文件下新增了一些配置用于启用spanable以及对spanable功能进行配置
```json
// skyapm.json
{
  "SkyWalking": {
    // ...
    // 官方配置省略
    // ...
	
	// 启用spanable
    "Scale": "span",
    "Spanable": {
	  // 未完成的异步span最大等待时间，当前版本推荐使用0(默认值)，不等待异步span完成
      "DelaySeconds": 0,
	  // 存在未完成异步span的segment队列延迟发送检测间隔，单位毫秒，默认3000
	  "DelayInspectInterval": 3000,
	  // 超过DelaySeconds依旧未完成的异步span将如何处理
	  // prefix（默认值）: 在该span的OperationName前面增加前缀，值取IncompletePrefix配置值
	  // error: 将该span的状态设置为失败
	  // tag: 仅在span的tags中增加一个key为incomplete，值为true的tag，其他选项同样会加该tag
      "IncompleteSymbol": "prefix",
	  // 当IncompleteSymbol值为prefix时，将使用IncompletePrefix作为span的OperationName前缀，默认[incomplete]
	  "IncompletePrefix": "[incomplete]"
    }
  }
}
```