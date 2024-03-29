// @ts-check
// `@type` JSDoc annotations allow editor autocompletion and type checking
// (when paired with `@ts-check`).
// There are various equivalent ways to declare your Docusaurus config.
// See: https://docusaurus.io/docs/api/docusaurus-config

import {themes as prismThemes} from 'prism-react-renderer';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'GeneralLibrary',
  tagline: 'This is a technology from an alien planet.',
  favicon: 'img/general.ico',

  // Set the production url of your site here
  url: 'https://your-docusaurus-site.example.com',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'facebook', // Usually your GitHub org/user name.
  projectName: 'docusaurus', // Usually your repo name.

  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'zh',
    locales: ['zh','en'],
    localeConfigs: {
      zh: {
        htmlLang: 'zh-GB',
      },
      en: {
        htmlLang: 'en-GB',
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
            'https://github.com/GeneralLibrary/GeneralUpdate-Samples/tree/main/website/doc',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      }),
    ],
  ],

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
            type: 'docSidebar',
            sidebarId: 'tutorialSidebar',
            position: 'left',
            label: '文档',
          },
          {
            label:'生态伙伴',
            href:'/EcologicalPartners',
            position: 'left'
          },
          {
            label:'企业合作',
            href:'/CooperativeEnterprises',
            position: 'left'
          },
          {
            label:'关于',
            href:'/About',
            position: 'left'
          },
          {
            href: 'https://github.com/GeneralLibrary',
            label: 'GitHub',
            position: 'right',
          },{
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
                label: 'Stack Overflow',
                href: 'https://github.com/GeneralLibrary',
              },
              {
                label: 'Discord',
                href: 'https://github.com/GeneralLibrary',
              },
              {
                label: 'Twitter',
                href: 'https://github.com/GeneralLibrary',
              },
            ],
          },
          {
            title: 'More',
            items: [
              {
                label: 'GitHub',
                href: 'https://github.com/GeneralLibrary',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} Juster zhu Project, Inc.`,
      },
      prism: {
        theme: prismThemes.github,
        darkTheme: prismThemes.dracula,
      },
    }),
};

export default config;
