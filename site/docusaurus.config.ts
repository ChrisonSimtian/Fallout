import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'Fallout',
  tagline: 'Build automation for C#/.NET — the hard-fork successor to NUKE',
  favicon: 'img/favicon.ico',

  future: {
    v4: true,
  },

  url: 'https://docs.fallout.build',
  baseUrl: '/',

  organizationName: 'ChrisonSimtian',
  projectName: 'Fallout',

  onBrokenLinks: 'warn',
  onBrokenMarkdownLinks: 'warn',

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          path: '../docs',
          routeBasePath: '/docs',
          sidebarPath: './sidebars.ts',
          editUrl: 'https://github.com/ChrisonSimtian/Fallout/tree/main/',
          exclude: [
            '**/rebrand-plan.md',
            '**/roadmap.md',
            '**/dependencies.md',
            '**/cli-tools/**',
          ],
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    colorMode: {
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'Fallout',
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docsSidebar',
          position: 'left',
          label: 'Docs',
        },
        {
          href: 'https://github.com/ChrisonSimtian/Fallout',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {label: 'Introduction', to: '/docs/introduction'},
            {label: 'Getting started', to: '/docs/01-getting-started/01-installation'},
          ],
        },
        {
          title: 'Project',
          items: [
            {label: 'GitHub', href: 'https://github.com/ChrisonSimtian/Fallout'},
            {label: 'Issues', href: 'https://github.com/ChrisonSimtian/Fallout/issues'},
            {label: 'Releases', href: 'https://github.com/ChrisonSimtian/Fallout/releases'},
          ],
        },
        {
          title: 'Origin',
          items: [
            {label: 'Originally NUKE', href: 'https://github.com/nuke-build/nuke'},
            {label: 'Migration guide', to: '/docs/migration/from-nuke'},
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Maintainers of Fallout. Originally based on NUKE by Matthias Koch and contributors. Distributed under the MIT License.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'powershell', 'bash', 'yaml', 'json'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
