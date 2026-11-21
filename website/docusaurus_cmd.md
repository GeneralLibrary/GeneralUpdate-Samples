```shell
//docusaurus 清理
npm run clear
//生成
npm run build
//启动所有语言预览
npm run serve
//启动默认语言的预览
npm run start

//i18n写入配置文件，.js文件
yarn run write-translations -- --locale en
yarn run write-translations -- --locale zh

//拷贝en语言的md文件
cp -r docs/** i18n/en/docusaurus-plugin-content-docs/current

//拷贝en语言的md文件
Copy-Item -Path docs -Destination i18n/en/docusaurus-plugin-content-docs/current -Recurse -Force
Copy-Item -Path docs -Destination i18n/zh/docusaurus-plugin-content-docs/current -Recurse -Force

mkdir -p i18n/en/docusaurus-plugin-content-blog

https://www.youtube.com/watch?v=2Arz1j5n2u0

...\website\i18n\en\docusaurus-plugin-content-docs\current\docs\doc
...\website\i18n\zh\docusaurus-plugin-content-docs\current\docs\doc
```

## 文档版本化（多实例）

站点按产品拆分为 6 个 docs 实例，各自独立版本线。导航栏左侧是单个 **"文档 ▾" 分组下拉**：产品名做分组标题，已发布版本号做菜单项（悬停/点开才显示，隐藏未发布的 Next 子项）——由自定义导航项 `src/theme/NavbarItem/ProductsVersionsNavbarItem` 实现（经 `src/theme/NavbarItem/ComponentTypes` swizzle 注册为 `custom-productsVersions` 类型；`custom-` 前缀是官方 schema 允许的自定义类型白名单）。版本列表随 `docs:version` 自动更新，发布新版无需改导航配置。

| 实例 | 内容目录 | 路由 | 版本化 | 版本命令 |
|------|----------|------|--------|----------|
| GeneralUpdate | `docs/` | `/docs`（稳定版）、`/docs/next`（开发中） | ✓ | `npm run docusaurus docs:version <版本号>` |
| GeneralZip | `generalzip/` | `/zip`、`/zip/next` | ✓ | `npm run docusaurus docs:version:generalzip <版本号>` |
| GeneralUpdate.Maui | `maui/` | `/maui`、`/maui/next` | ✓ | `npm run docusaurus docs:version:gumaui <版本号>` |
| GeneralUpdate.Avalonia | `avalonia/` | `/avalonia`、`/avalonia/next` | ✓ | `npm run docusaurus docs:version:guavalonia <版本号>` |
| Agent Skills | `agent-skills/` | `/agent-skills` | 暂不 | — |
| GeneralSpacestation | `spacestation/` | `/spacestation` | 不 | — |

注意：GeneralUpdate.Maui / GeneralUpdate.Avalonia 是独立仓库（GeneralLibrary/GeneralUpdate.Maui、GeneralLibrary/GeneralUpdate.Avalonia），版本线独立于 GeneralUpdate 主组件。

### 发布新版本时（例：GeneralUpdate 11.0.0）

1. 在 `docs/`（和 `generalzip/`、`maui/`、`avalonia/`）里完成新 API 的文档内容——此时 `/*/next` 路由展示开发中内容，带"尚未发行"横幅
2. 正式发布后冻结快照：
   ```shell
   npm run docusaurus docs:version 11.0.0
   ```
   命令自动完成：
   - 复制 `docs/` → `versioned_docs/version-11.0.0/`
   - 复制手写 sidebar → `versioned_sidebars/version-11.0.0-sidebars.json`
   - 复制 en 翻译 `i18n/en/docusaurus-plugin-content-docs/current` → `.../version-11.0.0`
   - 版本号插入 `versions.json` 首位 → `/docs` 立即指向 11.0.0，原 10.5.0 移到 `/docs/10.5.0`
3. 提交所有新生成的版本目录（`versioned_docs/`、`versions.json` 等需入库，不是构建产物）

其他实例同理：`docs:version:generalzip 2.1.0`（产物 `generalzip_versioned_docs/`）、`docs:version:gumaui 0.0.2`、`docs:version:guavalonia 0.0.2`。

### 修改历史版本内容

- 修 bug/补文档：直接改 `versioned_docs/version-<版本号>/`（en 同步改 `i18n/en/.../version-<版本号>/`），不要回灌 `docs/`
- 新内容永远写在 `docs/`（= Next），等下个版本发布时冻结

### 导航栏"文档"分组下拉（swizzle 组件）

- 组件：`src/theme/NavbarItem/ProductsVersionsNavbarItem/index.js`；类型注册：`src/theme/NavbarItem/ComponentTypes/index.js`（swizzle 官方 ComponentTypes，新增 `custom-productsVersions` 类型）
- navbar 配置：`groups` 里维护产品名、docsPluginId 与 basePath（移动端自动展开用）；`hideCurrentVersion: true` 隐藏 Next
- 新增产品实例时：在 `groups` 数组加一行即可
- 升级 Docusaurus 版本后需与官方 `ComponentTypes.js`、`DropdownNavbarItem` 源码比对一次，同步官方改动

### 版本化后各目录的职责

- `docs/` — GU 当前开发中内容（Next）
- `versioned_docs/version-*/` — GU 已发布版本快照（`docs:version` 生成，勿手工建）
- `generalzip/` + `generalzip_versioned_docs/` — Zip 同理
- `maui/` + `gumaui_versioned_docs/`、`avalonia/` + `guavalonia_versioned_docs/` — Maui/Avalonia 同理
- `i18n/en/docusaurus-plugin-content-docs[-<实例id>]/current` — 各实例 en 翻译（手动拷贝维护）
- `i18n/en/.../version-*/` — 各版本 en 翻译快照（`docs:version` 自动复制）
- `i18n/zh-Hans/docusaurus-plugin-content-docs/current` — 默认语言的部分镜像，只覆盖 current 版本页面，不进入版本快照


