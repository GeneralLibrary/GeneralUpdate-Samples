/**
 * @ts-check
 *
 * GeneralZip documentation sidebar（独立 docs 实例，独立版本线）。
 * 快照命令：npm run docusaurus docs:version:generalzip <版本号>
 */

/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
const sidebars = {
  zipSidebar: [
    'doc/GeneralZip',
    {
      type: 'category',
      label: 'Release Log',
      collapsed: true,
      items: ['releaselog/GeneralZipReleaselog'],
    },
  ],
};

export default sidebars;
