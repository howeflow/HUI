# HUI

**HUI** 是一个轻量级的 Unity UI 管理框架，提供完整的 UI 生命周期管理、分组层级控制、队列弹窗系统、事件通知机制以及组件化 UI 架构。

它旨在解决 Unity 项目中常见的 UI 管理问题：

- UI 打开/关闭逻辑分散
- 弹窗顺序难以控制
- UI 层级与排序复杂
- UI 状态变化难以监听
- 子视图管理困难

通过统一的 UI 管理入口、清晰的生命周期状态机以及灵活的事件系统，让 UI 逻辑更加清晰、可维护且易于扩展。

---

## 特性

- **生命周期管理** — 完整的 UI 状态机 (`Load → Open → Show → Shown → Hide → Hidden → Close`)
- **分组层级** — 多 Canvas 分组管理，组间按 `depth` 排序，组内按 `Priority` 排序
- **队列系统** — 支持多队列顺序弹窗，自动排队/插队/暂停/恢复
- **子组件化** — 子组件系统，支持 UI 内嵌套子视图的独立管理
- **动画调度** — UI状态动画过渡，开关可全局配置
- **参数传递** — 泛型参数支持，类型安全的数据传递
- **事件系统** — 全局 & 单 UI 粒度的状态变更事件通知
- **编辑器集成** — Editor 工具全局管理UI, 一键生成 UI / Component 模板代码

---

## 快速开始

### 1. 创建Settings/UIRoot

在 `Resources` 文件夹下通过菜单 **Create → HUI → Create UISettings Assets** 创建 `UISettings` 资产，配置：

| 字段              | 说明                       | 默认值       |
|-------------------|----------------------------|--------------|
| `prefabPath`      | 预制体默认路径            | `Assets`     |
| `scriptPath`      | 脚本默认路径               | `Assets`     |
| `skipAnimation`   | 是否跳过过渡动画           | `false`       |
| `dontDestroyOnLoad`        | UI Root 是否 DontDestroyOnLoad | `true`       |
| `groups`          | UI 分组列表（名称 + 层级深度） | `Default(0)` |

在工具栏中选择 **GameObject → HUI → Root**，会在 `Hierarchy` 下创建一个 `UIRoot` 预制体。


### 2. 初始化

```csharp
// 创建UI加载器（DefaultUILoader/YooAssetsUILoader/AddressblesUILoader) 或根据项目需求自定义加载器实现 IUILoader 接口
var loader = new DefaultUILoader();

//使用默认UIRoot
UIKit.Initialize(loader);

//使用自定义UIRoot
var root = GameObject.Find("UIRoot");
UIKit.Initialize(loader,root);

// 可监听初始化完成回调
UIKit.Initialized += () => Debug.Log("UI 系统就绪");
```

### 3. 创建 UI

继承 `BaseUI` 并重写生命周期方法：

```csharp
public class TestUI : BaseUI
{
    protected override void OnOpen()   { /* 首次加载完成 */ }
    protected override void OnShow()   { /* 显示开始 */ }
    protected override void OnShown()  { /* 显示动画结束 */ }
    protected override void OnHide()   { /* 隐藏开始 */ }
    protected override void OnHidden() { /* 隐藏动画结束 */ }
    protected override void OnClose()  { /* 销毁前 */ }
    protected override void OnRefresh(){ /* 手动刷新 */ }
}
```


或者继承 `BaseUI<T>` 并重写生命周期方法：

```csharp
public class MyParamUI : BaseUI<string>
{
    protected override void OnShow()
    {
        Debug.Log(Parameter); // 获取传入的参数
    }
}
```


### 4. 打开 / 关闭 UI

```csharp
// 打开（自动加载 + 显示）
UIKit.OpenUI<TestUI>();

//打开携带参数的UI
UIKit.OpenUI<MyParamUI, string>("Hello World");

// 关闭并销毁
UIKit.CloseUI<TestUI>();

// 关闭但保留实例（隐藏）
UIKit.CloseUI<TestUI>(destroy: false);
```
---

## 事件系统
### UI 事件监听

`OpenUI` 返回 `BaseUI`，通过扩展方法可链式注册事件：

```csharp
UIKit.OpenUI<TestUI>()
    .OnShow(h => Debug.Log("显示"))
    .OnHidden(h => Debug.Log("隐藏"));
```

### 全局事件

```csharp
UIKit.Manager.Event.OnOpen += ui => Debug.Log($"{ui.Name} 打开了");
```


## 队列系统

队列模式可让多个 UI 按顺序依次显示，前一个关闭后自动弹出下一个，适用于弹窗、引导等场景。

```csharp
// 添加到队列（默认队列 ID = 0）
UIKit.OpenQueueUI<TipA>();
UIKit.OpenQueueUI<TipB>();
UIKit.OpenQueueUI<TipC>();

// 指定队列 ID
UIKit.OpenQueueUI<TipA>(queueId: 1);

// 插入到队列指定位置
UIKit.InsertQueueUI<TipD>(index: 0, queueId: 0);

// 带参数的队列 UI
UIKit.OpenQueueUI<TipPanel, string>("内容", queueId: 0);
```

队列管理：

```csharp
UIKit.Manager.Queue.Pause();   // 暂停队列
UIKit.Manager.Queue.Resume();  // 恢复队列
UIKit.Manager.Queue.Clear();   // 清空队列
UIKit.Manager.Queue.ClearAll(); // 清空所有队列
```

---

## 分组系统

分组用于管理 UI 的层级渲染顺序。每个分组对应一个独立的 `Canvas`，通过 `sortingOrder` 控制组间的前后关系；组内的 UI 再按 `Priority` 排序。

### 配置分组

在 `UISettings`（ScriptableObject）中定义分组列表，每个分组包含名称和深度值：

```csharp
// UISettings.groups 默认配置
new List<UIGroupInfo> {
    new UIGroupInfo { name = "Default", depth = 0 }
};
```

### 指定 UI 所属分组

在 Prefab 的 `BaseView` 组件上设置 `ViewSettings`：

```csharp
[Serializable]
public struct ViewSettings
{
    public int group;         // 分组索引（对应 UISettings.groups 列表下标）
    public Priority priority; // 组内排序优先级
}
```

- **group** — 分组索引，对应 `UISettings.groups` 的下标
- **priority** — 同一分组内的显示顺序，值越大越靠前

分组管理：

```csharp
// 通过 BaseUI 访问所属分组
var ui = UIKit.OpenUI<TestUI>();
// ui.Group 即为该 UI 所在的 UIGroup

// 遍历分组内的所有 UI
foreach (var ui in ui.Group)
{
    Debug.Log(ui.Name);
}

// 遍历所有分组
foreach (var group in UIKit.Manager.Groups)
{
    Debug.Log($"{group.Info.name} : {group.Count} UIs");
}

// 获取最高/最低层级的分组
var top = UIKit.Manager.Groups.Max;
var bottom = UIKit.Manager.Groups.Min;
```

---

## 子组件 (Component & UIContainer)

在一个 UI 内部管理多个子视图组件：

```csharp
public class TestComponent : BaseComponent
{
    protected override void OnAdded()   { /* 添加时 */ }
    protected override void OnRemoved() { /* 移除时 */ }
    protected override void OnShow()    { /* 显示时 */ }
    protected override void OnHide()    { /* 隐藏时 */ }
}

public class TestUI : BaseUI
{
    protected override void OnOpen()
    {
        // 添加子组件，绑定对应的 BaseView
        Container.Add<TestComponent>(someView);

        // 带参数添加
        Container.Add<TestComponent, string>(someView, "param");

        // 显示 / 隐藏
        Container.Show<TestComponent>();
        Container.Hide<TestComponent>();

        // 批量操作
        Container.ShowAll();
        Container.HideAll();
        Container.RefreshAll();

        // 移除
        Container.Remove<TestComponent>();
        Container.RemoveAll();
    }
}
```

子组件同样支持泛型参数 `BaseComponent<P>` 和强类型 View `BaseViewComponent<V>` / `BaseViewComponent<V, P>`。

---

## 视图与动画

`BaseView` 是挂载在 Prefab 上的 MonoBehaviour，负责 GameObject 的显示/隐藏与动画播放。

### ViewSettings

```csharp
[Serializable]
public struct ViewSettings
{
    public int group;         // 分组 ID
    public Priority priority; // 层级优先级
}
```

`Priority` 枚举值：`Lowest(-2000)` / `Low(-1000)` / `Normal(0)` / `Middle(1000)` / `High(2000)` / `Highest(3000)`

### AnimatorUIAnimation 

实现了IUIAnimation接口基于 Animator 的 UI 过渡动画。

### 自定义动画

实现 `IUIAnimation` 接口并挂载到 View 的 GameObject 上：

```csharp
public class MyAnimation : MonoBehaviour, IUIAnimation
{
    public IEnumerator Show() { /* 显示动画 */ yield break; }
    public IEnumerator Hide() { /* 隐藏动画 */ yield break; }
}
```

动画由 `UIScheduler` 统一调度，可通过 `UISettings` 全局开关动画。

---


## UI 路径特性

使用 `UIPathAttribute` 指定资源加载路径（默认使用类名作为路径）,可在代码被混淆时确定预制体路径：

```csharp
[UIPath("TestUI")]
public class TestUI : BaseUI { }
```

---

## UI 生命周期

```
Initialize ──► Load ──► Open ──► Show ──► Shown
                                              │
                          ┌───────────────────┘
                          ▼
                        Hide ──► Hidden ──► Close(销毁)
                                    │
                                    └──► Show(再次显示，未销毁时)
```


### 状态枚举

| 状态 | 说明 |
|------|------|
| `None` | 初始状态 |
| `Load` | 资源加载中 |
| `Open` | 加载完成，实例已创建 |
| `Show` | 显示开始（动画播放中） |
| `Shown` | 显示完成 |
| `Hide` | 隐藏开始（动画播放中） |
| `Hidden` | 隐藏完成 |
| `Close` | 已关闭并销毁 |

---

## 类型速查

### BaseUI

| 类型 | 说明 |
|------|------|
| `BaseUI` | 基础 UI 逻辑类 |
| `BaseUI<P>` | 带参数的 UI（通过 `Parameter` 属性访问） |
| `BaseViewUI<V>` | 强类型 View 的 UI（`View` 属性返回具体类型） |
| `BaseViewUI<V, P>` | 强类型 View + 参数 |

### BaseComponent

| 类型 | 说明 |
|------|------|
| `BaseComponent` | 基础子组件 |
| `BaseComponent<P>` | 带参数的子组件 |
| `BaseViewComponent<V>` | 强类型 View 的子组件 |
| `BaseViewComponent<V, P>` | 强类型 View + 参数 |

---

## 安装

#### git安装
需要一个支持git包路径查询参数的Unity版本. 

你可以通过PackageManager `Add package from git URL`，添加 `https://github.com/howeflow/HUI.git?path=Assets/HUI`

或者在项目文件`Packages/manifest.json`，添加 `"com.howe.hui" : "https://github.com/howeflow/HUI.git?path=Assets/HUI"`。

#### 手动安装
1. 下载或克隆本仓库。
2. 将 `Assets/HUI` 文件夹复制到你的 Unity 项目的 `Assets` 目录下。
