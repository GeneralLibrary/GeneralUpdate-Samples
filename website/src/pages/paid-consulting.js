import React from 'react';
import Layout from '@theme/Layout';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Heading from '@theme/Heading';
import contactImg from '@site/docs/doc/imgs/contact.png';

export default function PaidConsulting() {
  const {i18n} = useDocusaurusContext();
  const isEn = i18n.currentLocale === 'en';

  const title = isEn ? 'Paid Consultation' : '付费咨询';
  const description = isEn
    ? 'One-on-one technical support and consulting services for GeneralUpdate and .NET'
    : '针对 GeneralUpdate 与 .NET 技术的一对一技术支持与咨询服务';

  return (
    <Layout title={title} description={description}>
      <main style={{maxWidth: 800, margin: '0 auto', padding: '4rem 2rem'}}>
        <Heading as="h1">{title}</Heading>
        <p style={{color: 'var(--ifm-color-secondary-darkest)', marginBottom: '2rem'}}>
          {isEn
            ? 'We offer one-on-one paid consultation services to help you get the most out of GeneralUpdate and solve complex .NET technical challenges. Due to the large number of individual inquiries, paid consultation ensures you receive dedicated, timely support.'
            : '我们提供一对一付费咨询服务，帮助您充分发挥 GeneralUpdate 的价值，解决复杂的 .NET 技术难题。由于个人咨询量较大，付费咨询确保您获得专注、及时的技术支持。'}
        </p>

        <div style={{display: 'flex', flexDirection: 'column', gap: '2rem'}}>

          {/* ── GeneralUpdate 组件使用咨询 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h2">
              {isEn ? 'GeneralUpdate Consulting' : 'GeneralUpdate 组件使用咨询'}
            </Heading>
            <p style={{margin: 0, color: 'var(--ifm-color-secondary-darkest)'}}>
              {isEn
                ? 'From basic integration to advanced customization — get expert guidance on integrating GeneralUpdate into your applications. We cover configuration, differential updates, pipeline design, driver adaptation, security hardening, and best practices for production deployment.'
                : '从基础集成到高级定制——获取将 GeneralUpdate 集成到您应用中的专家指导。涵盖配置调优、差量更新、管线设计、驱动适配、安全加固以及生产环境部署的最佳实践。'}
            </p>
          </div>

          {/* ── .NET 技术咨询 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h2">
              {isEn ? '.NET Technical Consulting' : '.NET 技术咨询'}
            </Heading>
            <p style={{margin: 0, color: 'var(--ifm-color-secondary-darkest)'}}>
              {isEn
                ? 'Deep expertise across the .NET ecosystem — WPF, Avalonia, MAUI, ASP.NET Core, and more. Whether you need architecture review, performance optimization, migration planning, or troubleshooting, our team is ready to help.'
                : '深厚的 .NET 生态技术积累——WPF、Avalonia、MAUI、ASP.NET Core 等。无论您需要架构评审、性能优化、迁移规划还是疑难排错，我们的团队随时为您提供支持。'}
            </p>
          </div>

          {/* ── 方案咨询 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h2">
              {isEn ? 'Solution Consulting' : '方案咨询'}
            </Heading>
            <p style={{margin: 0, color: 'var(--ifm-color-secondary-darkest)'}}>
              {isEn
                ? 'Need a tailored solution for your project? We provide solution design and consulting services covering auto-update architecture, CI/CD integration with automated packaging, cross-platform update strategies, and general .NET application architecture. Tell us your requirements and we\'ll design a solution that fits.'
                : '需要为您的项目量身定制方案？我们提供方案设计与咨询服务，涵盖自动更新架构规划、CI/CD 自动打包集成、跨平台更新策略以及通用 .NET 应用架构设计。告诉我们您的需求，我们将为您设计合适的方案。'}
            </p>
          </div>

          {/* ── 联系方式 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
            background: 'var(--ifm-color-warning-contrast-background)',
          }}>
            <Heading as="h2">
              {isEn ? 'Get in Touch' : '联系方式'}
            </Heading>
            <p style={{color: 'var(--ifm-color-secondary-darkest)', marginBottom: '1rem'}}>
              {isEn
                ? 'Scan the QR code below to contact us. Please state your purpose when adding (WeChat recommended).'
                : '扫描下方二维码联系我们，加好友请注明来意（推荐微信）。'}
            </p>
            <div style={{textAlign: 'center'}}>
              <img src={contactImg} alt={isEn ? 'Contact QR Code' : '联系方式二维码'} style={{maxWidth: '100%', borderRadius: '8px'}} />
            </div>
          </div>

        </div>
      </main>
    </Layout>
  );
}
