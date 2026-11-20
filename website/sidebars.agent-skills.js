/**
 * @ts-check
 *
 * Agent Skills documentation sidebar（独立 docs 实例，暂不版本化）。
 * 文档平铺在 agent-skills/ 目录下，id 无子目录前缀。
 */

/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
const sidebars = {
  agentSkillsSidebar: [
    'overview',
    'generalupdate-init',
    'generalupdate-ui',
    'generalupdate-strategy',
    'generalupdate-advanced',
    'generalupdate-troubleshoot',
    'generalupdate-migration',
    'generalupdate-security-audit',
  ],
};

export default sidebars;
