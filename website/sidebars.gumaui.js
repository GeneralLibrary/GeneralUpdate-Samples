/**
 * @ts-check
 *
 * GeneralUpdate.Maui documentation sidebar（独立 docs 实例，独立版本线）。
 * 快照命令：npm run docusaurus docs:version:gumaui <版本号>
 */

/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
const sidebars = {
  mauiSidebar: [
    'quickstart/Maui Android cookbook',
    'doc/GeneralUpdate.Maui.Android',
    'doc/Maui.Android-flow',
    {
      type: 'category',
      label: 'Release Log',
      collapsed: true,
      items: ['releaselog/GeneralUpdateMauiReleaselog'],
    },
  ],
};

export default sidebars;
