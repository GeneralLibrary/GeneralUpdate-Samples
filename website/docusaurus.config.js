// @ts-check
// `@type` JSDoc annotations allow editor autocompletion and type checking
// (when paired with `@ts-check`).
// There are various equivalent ways to declare your Docusaurus config.
// See: https://docusaurus.io/docs/api/docusaurus-config

import {themes as prismThemes} from 'prism-react-renderer';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'GeneralLibrary',
  //tagline: 'This is a technology from an alien planet.',
  favicon: 'img/general.ico',

  // Set the production url of your site here
  // 部署目标：自有 Azure VM nginx 静态站（见 .github/workflows/deploy-docs.yml 与 ServerInfo 仓库）。
  // 此值用于 canonical/og:url 与 sitemap 生成，必须与实际域名一致。
  url: 'https://www.justerzhu.cn',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'GeneralLibrary', // Usually your GitHub org/user name.
  projectName: 'GeneralLibrary', // Usually your repo name.

  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'zh-Hans',
    locales: ['zh-Hans','en'],
    localeConfigs: {
      "zh-Hans": {
        label: '中文',
      },
      "en": {
        label: 'English',
      }
    }
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: './sidebars.js',
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
            'https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/website',
          // 多实例版本化：docs/ 为 GeneralUpdate 实例（默认插件），
          // versions.json 里第一个版本即 lastVersion，挂在 /docs 基础路由；
          // 未冻结的 docs/ 为 current（"Next"），挂在 /docs/next。
          // 发布新版本时执行 npm run docusaurus docs:version <新版本号> 冻结快照。
          versions: {
            current: {
              label: 'Next',
            },
          },
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      }),
    ],
  ],

  plugins: [
    [
      '@easyops-cn/docusaurus-search-local',
      {
        // 索引文件名加 hash，避免发布后浏览器缓存旧索引
        hashed: true,
        // 启用英文与中文分词（中文基于 nodejieba）
        language: ['en', 'zh'],
        // 覆盖所有 docs 实例的路由（默认只索引 "docs"）
        docsRouteBasePath: ['docs', 'zip', 'agent-skills', 'spacestation'],
        // 仅索引 docs（默认），不索引独立页面
        indexPages: false,
      },
    ],
    // ── GeneralZip 独立 docs 实例（独立版本线）──────────────────────
    [
      '@docusaurus/plugin-content-docs',
      {
        id: 'generalzip',
        path: 'generalzip',
        routeBasePath: 'zip',
        sidebarPath: './sidebars.generalzip.js',
        editUrl:
          'https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/website',
        versions: {
          current: {
            label: 'Next',
          },
        },
      },
    ],
    // ── Agent Skills 独立 docs 实例（暂不版本化）────────────────────
    [
      '@docusaurus/plugin-content-docs',
      {
        id: 'agent-skills',
        path: 'agent-skills',
        routeBasePath: 'agent-skills',
        sidebarPath: './sidebars.agent-skills.js',
      },
    ],
    // ── GeneralSpacestation 独立 docs 实例（单页，不版本化）─────────
    [
      '@docusaurus/plugin-content-docs',
      {
        id: 'spacestation',
        path: 'spacestation',
        routeBasePath: 'spacestation',
        sidebarPath: false,
      },
    ],
  ],

  markdown: {
    mermaid: true,
  },

  themes: ['@docusaurus/theme-mermaid'],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      // Replace with your project's social card
      image: 'img/docusaurus-social-card.jpg',
      navbar: {
        title: 'GeneralLibrary',
        logo: {
          alt: 'GeneralLibrary Logo',
          src: 'img/general.png',
        },
        items: [
          {
            // 产品名做下拉标签，悬停/点开显示已发布版本号（隐藏 Next）
            // 见 src/theme/NavbarItem/DocsVersionDropdownNavbarItem
            type: 'docsVersionDropdown',
            position: 'left',
            label: 'GeneralUpdate',
            hideCurrentVersion: true,
          },
          {
            type: 'docsVersionDropdown',
            docsPluginId: 'generalzip',
            position: 'left',
            label: 'GeneralZip',
            hideCurrentVersion: true,
          },
          {
            type: 'docSidebar',
            sidebarId: 'agentSkillsSidebar',
            docsPluginId: 'agent-skills',
            position: 'left',
            label: 'Agent Skills',
          },
          {
            type: 'dropdown',
            label: '定价',
            position: 'left',
            items: [
              {
                type: 'doc',
                docId: 'GeneralSpacestation',
                docsPluginId: 'spacestation',
                label: 'GeneralSpacestation',
              },
              {
                to: '/outsourcing',
                label: '软件外包服务',
              },
              {
                to: '/paid-consulting',
                label: '付费咨询',
              },
            ],
          },
          {
            type: 'dropdown',
            label: '更多',
            position: 'right',
            items: [
              {
                to: '/contact',
                label: '联系方式',
              },
              {
                to: '/corporate-wall',
                label: '企业墙',
              },
              {
                to: '/oss-partners',
                label: '开源生态伙伴',
              },
            ],
          },
          {
            type: 'dropdown',
            label: '源码平台',
            position: 'right',
            items: [
              {
                href: 'https://github.com/GeneralLibrary/GeneralUpdate',
                label: 'GitHub',
              },
              {
                href: 'https://gitee.com/GeneralLibrary/GeneralUpdate',
                label: 'Gitee',
              },
              {
                href: 'https://gitcode.com/GeneralLibrary/GeneralUpdate',
                label: 'GitCode',
              },
            ],
          },
          {
            type: 'localeDropdown',
            position: 'right',
          }
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Doc',
            items: [
              {
                label: 'GeneralLibrary',
                to: '/docs/doc/Component Introduction',
              },
            ],
          },
          {
            title: 'Community',
            items: [
              {
                label: 'GitHub',
                href: 'https://github.com/GeneralLibrary/GeneralUpdate',
              },
              {
                label: 'Gitee',
                href: 'https://gitee.com/GeneralLibrary/GeneralUpdate',
              },
              {
                label: 'GitCode',
                href: 'https://gitcode.com/GeneralLibrary/GeneralUpdate',
              },
            ],
          },
          {
            title: 'More',
            items: [
              {
                label: 'GitHub',
                href: 'https://github.com/GeneralLibrary/GeneralUpdate',
              },
              {
                label: 'Gitee',
                href: 'https://gitee.com/GeneralLibrary/GeneralUpdate',
              },
              {
                label: 'GitCode',
                href: 'https://gitcode.com/GeneralLibrary/GeneralUpdate',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} Juster zhu Project, Inc.`,
      },
      prism: {
        theme: prismThemes.github,
        darkTheme: prismThemes.dracula,
        additionalLanguages: ['csharp', 'bash', 'json'],
      },
    }),
};

export default config;
