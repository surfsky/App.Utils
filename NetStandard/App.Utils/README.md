
## 使用

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddNewtonsoftJson();       // 支持 NewtonsoftJson
}
```




## History

3.0.0
    迁移到 netstandard, 并拆分为 App.Utils 和 App.Web 两个项目
    去除功能
        /NetHelper.GetServerOrNetworkImage
        /TypeBuilder
        /ExcelHelper 拆分为 ExcelHelper（项目App.Utils） 和 ExcelExporter（项目 App.Web）
    重构Cache
        重构 IO.Cache，支持 null 值
        新增 Cacher 类
    修正 JsEvaluator, CsEvaluate, PinYin

3.0.1
    Add Reflector.ExpressOf()

3.0.2
    *IO.WriteFile
    +IO.ReadFileText
    +IO.ReadFileBytes
