/**
 * @ts-check
 *
 * GeneralUpdate documentation sidebar.
 *
 * Pages are grouped by role:
 *   1. Quick Start          — Beginner cookbook & Tools reference
 *   2. Components           — non-firmware component reference (Core, Bowl,
 *                              Differential, Drivelution, Extension)
 *   3. Help                 — operational / platform-specific guides
 *   4. Release Log          — changelog and roadmap
 */

/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
const sidebars = {
  tutorialSidebar: [
    // ── 1. Quick Start ──────────────────────────────────────────────
    {
      type: 'category',
      label: 'Quick Start',
      collapsed: false,
      items: [
        'quickstart/Beginner cookbook',
        'quickstart/GeneralUpdate.PacketTool',
        'quickstart/Avalonia Android cookbook',
        'quickstart/Maui Android cookbook',
      ],
    },

    // ── 2. Components ───────────────────────────────────────────────
    {
      type: 'category',
      label: 'Components',
      collapsed: false,
      items: [
        'doc/Component Introduction',
        'doc/GeneralUpdate.Core',
        'doc/Core-flow',
        'doc/GeneralUpdate.Bowl',
        'doc/GeneralUpdate.Differential',
        'doc/GeneralUpdate.Drivelution',
        'doc/GeneralUpdate.Extension',
        'doc/GeneralUpdate.Avalonia.Android',
        'doc/GeneralUpdate.Maui.Android',
      ],
    },

    // ── 3. Release Log ──────────────────────────────────────────────
    {
      type: 'category',
      label: 'Release Log',
      collapsed: true,
      items: [
        'releaselog/GeneralUpdateReleaselog',
      ],
    },
  ],

  // ── Agent Skills Sidebar (独立侧边栏) ────────────────────────────
  agentSkillsSidebar: [
    'agent-skills/overview',
    'agent-skills/generalupdate-init',
    'agent-skills/generalupdate-ui',
    'agent-skills/generalupdate-strategy',
    'agent-skills/generalupdate-advanced',
    'agent-skills/generalupdate-troubleshoot',
    'agent-skills/generalupdate-migration',
    'agent-skills/generalupdate-security-audit',
  ],
};

export default sidebars;
