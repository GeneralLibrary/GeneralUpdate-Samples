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

