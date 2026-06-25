import React from 'react';
import Layout from '@theme/Layout';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Heading from '@theme/Heading';
import styles from './oss-partners.module.css';

function PartnerCard({ name, url, description, descriptionEn }) {
  const {i18n} = useDocusaurusContext();
  const isEn = i18n.currentLocale === 'en';
  return (
    <div className={styles.card}>
      <div className={styles.cardHeader}>
        <h3 className={styles.cardName}>{name}</h3>
        <a href={url} className={styles.cardLink} target="_blank" rel="noopener noreferrer">
          {url}
        </a>
      </div>
      <p className={styles.cardDesc}>
        {isEn ? descriptionEn : description}
      </p>
    </div>
  );
}

export default function OssPartners() {
  const {i18n} = useDocusaurusContext();
  const isEn = i18n.currentLocale === 'en';

  const partners = [
    {
      name: 'Semi.Avalonia',
      url: 'https://github.com/irihitech/Semi.Avalonia',
      description: '一套 Semi Design 风格的 Avalonia UI 主题库，帮助开发者快速构建现代、美观的跨平台桌面应用。',
      descriptionEn: 'A Semi Design-inspired Avalonia UI theme library for building modern, beautiful cross-platform desktop applications.',
    },
    {
      name: 'Ursa.Avalonia',
      url: 'https://github.com/irihitech/Ursa.Avalonia',
      description: '一套包含丰富自定义控件的 Avalonia UI 组件库，为开发者提供开箱即用的高级 UI 组件。',
      descriptionEn: 'A rich Avalonia UI component library with custom controls, providing developers with ready-to-use advanced UI components.',
    },
    {
      name: 'WPFDevelopers',
      url: 'https://github.com/WPFDevelopersOrg/WPFDevelopers',
      description: 'WPFDevelopers 是一个开源 WPF UI 控件库，提供截图、动画、主题切换等高级功能，覆盖 .NET 4.0 至 10.0 全系列版本。核心价值：提升开发效率，降低造轮子成本，帮助 .NET 桌面开发者快速构建现代化应用。',
      descriptionEn: 'An open-source project collection focused on WPF development, featuring a wealth of practical WPF controls, templates, and resources.',
    },
    {
      name: 'Layui-WPF',
      url: 'https://github.com/Layui-WPF-Team/Layui-WPF',
      description: '将经典 Layui 风格带入 WPF 的 UI 框架，让 WPF 开发者也能享受到 Layui 简洁优雅的设计语言。',
      descriptionEn: 'A UI framework bringing the classic Layui design style to WPF, allowing WPF developers to enjoy Layui\'s clean and elegant design language.',
    },
    {
      name: 'AntdUI',
      url: 'https://github.com/AntdUI/AntdUI',
      description: '基于 Ant Design 风格的 .NET UI 控件库，为 WinForms/WPF 桌面开发提供企业级 UI 解决方案。',
      descriptionEn: 'A .NET UI control library based on Ant Design style, providing enterprise-grade UI solutions for WinForms and WPF desktop development.',
    },
    {
      name: 'routin.ai',
      url: 'https://routin.ai/',
      description: '人工智能与自动化平台，通过 AI 技术赋能企业业务流程智能化升级。',
      descriptionEn: 'An AI and automation platform that empowers enterprise business process upgrades through artificial intelligence technology.',
    },
    {
      name: 'OpenCowork',
      url: 'https://github.com/AIDotNet/OpenCowork',
      description: '开源协同办公平台，提供团队协作、文档管理、项目管理等企业级协同功能。',
      descriptionEn: 'An open-source collaborative office platform providing enterprise-grade collaboration features including team workspace, document management, and project management.',
    },
    {
      name: 'Gradio.Net',
      url: 'https://github.com/feiyun0112/Gradio.Net',
      description: 'Gradio 的 .NET 移植版，无需前端代码即可为机器学习模型、API 或任意函数快速构建演示或 Web 应用。',
      descriptionEn: 'A .NET port of Gradio, allowing you to quickly build demos or web applications for ML models, APIs, or any functions without frontend code.',
    },
    {
      name: '3C-demo',
      url: 'https://github.com/LessIsMoreInSZ/3C-demo',
      description: '抵制无良培训班，开源 3C 电商 Demo 项目，助力初学者学习真实项目开发。',
      descriptionEn: 'Open-source 3C e-commerce demo project, helping beginners learn real-world project development.',
    },
    {
      name: 'HagiCode',
      url: 'https://hagicode.com/',
      description: '全球不唯一，但是超级好用的 Agentic Coding 软件，集 AI 编程、游戏化工作空间于一体的全栈开发平台。',
      descriptionEn: 'Not the only one in the world, but a super useful Agentic Coding software — an all-in-one AI-powered development workspace with gamification.',
    },
    {
      name: 'TouchSocket',
      url: 'https://touchsocket.net/',
      description: '一个轻量级、高性能、易用的 .NET 网络通信框架，支持 TCP、UDP、HTTP 等多种协议。',
      descriptionEn: 'A lightweight, high-performance, easy-to-use .NET networking framework supporting TCP, UDP, HTTP and more.',
    },
  ];

  const title = isEn ? 'Open Source Partners' : '开源生态伙伴';
  const description = isEn
    ? 'Open source projects and communities in the .NET ecosystem'
    : '.NET 生态中的开源项目与社区';

  return (
    <Layout title={title} description={description}>
      <main className={styles.page}>
        <Heading as="h1" className={styles.pageTitle}>
          {title}
        </Heading>
        <p className={styles.pageSubtitle}>
          {isEn
            ? 'Thank you to the following open-source projects and communities for their contributions to the .NET ecosystem.'
            : '感谢以下开源项目与社区对 .NET 生态的贡献。'}
        </p>
        <div className={styles.cardGrid}>
          {partners.map((p, i) => <PartnerCard key={i} {...p} />)}
        </div>
      </main>
    </Layout>
  );
}
