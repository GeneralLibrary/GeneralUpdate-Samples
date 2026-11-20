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

站点按产品拆分为 4 个 docs 实例，各自独立版本线。导航栏上 GeneralUpdate / GeneralZip 是**产品名下拉框**（不显示版本号），悬停/点开才列出已发布的版本号（隐藏未发布的 Next 子项）——由自定义组件 `src/theme/NavbarItem/DocsVersionDropdownNavbarItem`（raw swizzle，基于 3.1.1 官方组件修改）实现，版本列表随 `docs:version` 自动更新，发布新版无需改导航配置。

| 实例 | 内容目录 | 路由 | 版本化 | 版本命令 |
|------|----------|------|--------|----------|
| GeneralUpdate | `docs/` | `/docs`（稳定版）、`/docs/next`（开发中） | ✓ | `npm run docusaurus docs:version <版本号>` |
| GeneralZip | `generalzip/` | `/zip`、`/zip/next` | ✓ | `npm run docusaurus docs:version:generalzip <版本号>` |
| Agent Skills | `agent-skills/` | `/agent-skills` | 暂不 | — |
| GeneralSpacestation | `spacestation/` | `/spacestation` | 不 | — |

### 发布新版本时（例：GeneralUpdate 11.0.0）

1. 在 `docs/`（和 `generalzip/`）里完成新 API 的文档内容——此时 `/docs/next` 展示开发中内容，带"尚未发行"横幅
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

GeneralZip 同理：`npm run docusaurus docs:version:generalzip 2.1.0`，产物为 `generalzip_versioned_docs/`、`generalzip_versions.json`。

### 修改历史版本内容

- 修 bug/补文档：直接改 `versioned_docs/version-<版本号>/`（en 同步改 `i18n/en/.../version-<版本号>/`），不要回灌 `docs/`
- 新内容永远写在 `docs/`（= Next），等下个版本发布时冻结

### 导航栏版本下拉（swizzle 组件）

- 位置：`src/theme/NavbarItem/DocsVersionDropdownNavbarItem/index.js`，navbar 配置里用 `label`（产品名）+ `hideCurrentVersion: true`（隐藏 Next 子项）
- 升级 Docusaurus 版本后需与官方 `DocsVersionDropdownNavbarItem` 源码比对一次，同步官方改动

### 版本化后各目录的职责

- `docs/` — GU 当前开发中内容（Next）
- `versioned_docs/version-*/` — GU 已发布版本快照（`docs:version` 生成，勿手工建）
- `generalzip/` + `generalzip_versioned_docs/` — Zip 同理
- `i18n/en/docusaurus-plugin-content-docs[-<实例id>]/current` — 各实例 en 翻译（手动拷贝维护）
- `i18n/en/.../version-*/` — 各版本 en 翻译快照（`docs:version` 自动复制）
- `i18n/zh-Hans/docusaurus-plugin-content-docs/current` — 默认语言的部分镜像，只覆盖 current 版本页面，不进入版本快照


