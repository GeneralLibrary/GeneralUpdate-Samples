/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
const sidebars = {
  toolsSidebar: [
    'quickstart/GeneralUpdate.PacketTool',
    {
      type: 'category',
      label: 'Release Log',
      collapsed: true,
      items: ['releaselog/GeneralUpdateToolsReleaselog'],
    },
  ],
};

export default sidebars;
