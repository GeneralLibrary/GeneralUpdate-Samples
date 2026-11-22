/**
 * @ts-check
 *
 * GeneralUpdate documentation sidebar.
 *
 * Pages are grouped by role:
 *   1. Quick Start          — Beginner cookbook & Tools reference
 *   2. Components           — non-firmware component reference (Core, Bowl,
 *                              Differential, Drivelution, Extension, GeneralZip)
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
        'doc/GeneralUpdate.Bowl',
        'doc/GeneralUpdate.Differential',
        'doc/GeneralUpdate.Drivelution',
        'doc/GeneralUpdate.Extension',
        {
          type: 'category',
          label: '执行流程详解',
          collapsible: true,
          collapsed: true,
          items: [
            'doc/Core-flow',
            'doc/Bowl-flow',
            'doc/Drivelution-flow',
            'doc/Extension-flow',
          ],
        },
      ],
    },

    // ── 3. Release Log ──────────────────────────────────────────────
    {
      type: 'category',
      label: 'Release Log',
      collapsed: true,
      items: [
        'releaselog/GeneralUpdateReleaselog',
        'releaselog/GeneralUpdateToolsReleaselog',
      ],
    },
  ],
};

export default sidebars;
