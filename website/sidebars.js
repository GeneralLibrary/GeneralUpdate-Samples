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
};

export default sidebars;
