# 用 VS Code Claude Code + DeepSeek V4 Pro 写代码的一些记录

## 一、背景

Claude Code 的 VS Code 插件出来之后我一直想试试，但这东西默认走 Anthropic 自己的 API，Opus 4.6 输出价格 $25/百万 token，高强度用一个月账单不太好看。后来发现 DeepSeek 官方提供了兼容 Anthropic 协议的 API 端点，输出价格 $0.87/百万 token，差了一个数量级。于是就搭起来用了段时间，这篇文章是把整个过程和感受记下来，给想试的人一个参考。

---

## 二、环境搭建

### 2.1 装插件

VS Code 里按 `Ctrl+Shift+X` 搜 "Claude Code"，发布者是 Anthropic，点安装就行。装完右上角会出现一个闪电图标，那是打开 Claude Code 面板的入口。

没什么特别的，跟装其他扩展一样。

### 2.2 配置 DeepSeek 的 API 密钥

去 [platform.deepseek.com](https://platform.deepseek.com) 注册账号，在 API Keys 页面创建一个 Key。注意关掉弹窗就看不到了，提前存好。

然后在 VS Code 里按 `Ctrl+Shift+P`，打开 `Open User Settings (JSON)`，加这段配置：

```json
"claudeCode.environmentVariables": [
    { "name": "ANTHROPIC_BASE_URL", "value": "https://api.deepseek.com/anthropic" },
    { "name": "ANTHROPIC_AUTH_TOKEN", "value": "sk-你的DeepSeek_API_Key" },
    { "name": "ANTHROPIC_MODEL", "value": "deepseek-v4-pro[1m]" },
    { "name": "ANTHROPIC_DEFAULT_OPUS_MODEL", "value": "deepseek-v4-pro[1m]" },
    { "name": "ANTHROPIC_DEFAULT_SONNET_MODEL", "value": "deepseek-v4-pro[1m]" },
    { "name": "ANTHROPIC_DEFAULT_HAIKU_MODEL", "value": "deepseek-v4-flash" },
    { "name": "CLAUDE_CODE_SUBAGENT_MODEL", "value": "deepseek-v4-flash" },
    { "name": "CLAUDE_CODE_DISABLE_NONESSENTIAL_TRAFFIC", "value": "1" },
    { "name": "CLAUDE_CODE_EFFORT_LEVEL", "value": "max" },
    { "name": "API_TIMEOUT_MS", "value": "600000" }
]
```

说几个注意点：

- **`[1m]` 后缀加上**，否则默认上下文只有 64K，Claude Code 跑 agent 模式几个来回就超了。
- **SUBAGENT_MODEL 我设成了 Flash**，文件搜索、索引这些辅助任务没必要上 Pro，省点钱。
- **`DISABLE_NONESSENTIAL_TRAFFIC` 设 1**，DeepSeek 的兼容端点对某些预检请求会返回 404，设了这个直接跳过，省得报错刷屏。

配置完重启 VS Code，打开 Claude Code 面板输入 `/model`，返回 `deepseek-v4-pro` 就说明通了。

另外这些环境变量配置其实不用自己手敲——有其他 AI Agent 的话可以代劳，比如 OpenClaw 里通过 MCP 工具直接写 settings.json，一句话搞定。

### 2.3 实际用起来的感受

快捷键：`Ctrl+Shift+I` 打开面板，`Ctrl+Esc` 切换焦点，`Ctrl+N` 新建对话。

几个场景的感受：

**写代码。** 需求明确的时候它干得不错。比如"写一个支持断点续传的 HTTP 上传客户端，用 asyncio，带进度回调"——生成的代码结构清晰，分片逻辑、md5 校验、重试机制都带了，基本改改就能用。

**重构。** 选中一段老代码让它拆函数加类型注解，它能理解意图，但有个毛病——容易过度工程化。50 行的函数它能给你拆成 5 个文件 8 个类，你得自己往回拉。

**Debug。** 报错堆栈丢给它定位方向还行，但深层的逻辑漏洞它经常"自圆其说"，给一个看似合理但实际错误的解释。最终还是靠自己。

---

## 三、一些心得

### 3.1 智能程度：能干活，但得有人带着

SWE Verified 上 V4 Pro 是 80.6%，Opus 4.6 Max 是 80.8%，数字看不出什么差距。实际用下来，**需求明确、路径清晰的任务**——比如"实现一个 Redis 分布式锁"、"写 JWT 中间件"——它完成度很高。但需求模糊、需要自己判断决策的地方，它就容易写出看似正确但细节不对的东西。

我的感受是：把它当成一个效率放大器比较合适。**你自己心里得有清晰的思路，知道每一步要什么，它能帮你把想法快速落地。** 反过来，如果你自己都不知道该怎么写，指望它给你一个完整的架构设计，那大概率会失望。

### 3.2 测试：帮得上忙，但核心流程还是得人走

写单元测试、集成测试这种套路固定的工作，它做得不错。给个函数它能快速生成 edge case 覆盖、mock 依赖、assert 断言，比自己手写快不少。搭测试环境、写 Dockerfile、配 CI 这类事也基本零失误。

但"帮我写测试看看能不能测出 bug"——这件事它做不好。生成的测试大概率只走 happy path，隐藏的并发竞争条件、异常状态流转它发现不了。**找 bug 还是靠人的直觉和对代码的理解。**

### 3.3 性价比：划算，但得看场景

价格上 DeepSeek V4 Pro 确实便宜很多：输入 $0.435、输出 $0.87（每百万 token）。Opus 4.6 是 $5 和 $25。差了将近 29 倍。

但这里有个反直觉的事。我之前遇到一个依赖注入容器的生命周期问题，涉及多个框架层的交互，比较复杂。用 DeepSeek V4 Pro 调了大半天，token 烧了将近 1 亿，来回改了好几轮都没解决。后来切回 Opus 4.6，20 分钟定位问题，10 分钟修完。算下来 DeepSeek 花了大概 $80 加大半天时间，Opus 花了大概 $50 加半小时。

**贵的模型不一定更贵。** 如果任务难度高，更强的模型虽然单价贵，但总成本（算上你的时间）反而更低。如果任务简单明确——写 CRUD、写测试、写配置——那 DeepSeek V4 Pro 确实划算。

我现在是**日常开发用 DeepSeek，遇到疑难杂症切回 Opus**。成本能控住，关键时刻效率也不掉。

---

## 四、总结

VS Code Claude Code + DeepSeek V4 Pro 这套组合，对于预算有限的个人开发者和中小团队来说是个实惠的选择。配置简单、日常编码够用、成本低。缺点是复杂推理不如顶级闭源模型、偶尔过度工程化、没有多模态能力。

适合的场景是"你心里有清晰的代码蓝图，需要一个人快速把代码落地"。它不是自动驾驶，是一个能提效的辅助工具。

**模型选型看场景，日常省钱、关键时刻舍得用好模型，互补着来比较实际。**
