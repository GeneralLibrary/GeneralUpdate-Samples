import React from 'react';
import Layout from '@theme/Layout';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Heading from '@theme/Heading';
import contactImg from '@site/docs/doc/imgs/contact.png';

export default function Outsourcing() {
  const {i18n} = useDocusaurusContext();
  const isEn = i18n.currentLocale === 'en';

  const title = isEn ? 'Software Outsourcing Services' : '软件外包服务';
  const description = isEn
    ? 'Professional enterprise-grade software outsourcing services'
    : '专业的企业级软件外包服务';

  return (
    <Layout title={title} description={description}>
      <main style={{maxWidth: 800, margin: '0 auto', padding: '4rem 2rem'}}>
        <Heading as="h1">{title}</Heading>
        <p style={{color: 'var(--ifm-color-secondary-darkest)', marginBottom: '2rem'}}>
          {isEn
            ? 'We offer professional enterprise-grade software outsourcing services backed by years of .NET ecosystem development experience. No matter the size of your project, we deliver high-quality, reliable solutions.'
            : '我们提供专业的企业级软件外包服务，基于多年 .NET 生态开发经验，无论项目大小，我们都能为您交付高质量、可靠的解决方案。'}
        </p>

        <div style={{display: 'flex', flexDirection: 'column', gap: '2rem'}}>

          {/* ── 关于我们 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h2">
              {isEn ? 'Who We Are' : '关于我们'}
            </Heading>
            <p style={{margin: 0, color: 'var(--ifm-color-secondary-darkest)'}}>
              {isEn
                ? 'Our core team has deep expertise in the .NET technology stack, with extensive experience in WPF, Avalonia, MAUI, and other desktop/mobile frameworks. We provide end-to-end services from requirements analysis and architecture design to development and delivery. We have delivered production-grade auto-update solutions and enterprise applications for companies across multiple industries.'
                : '核心团队长期深耕 .NET 技术栈，在 WPF、Avalonia、MAUI 等桌面/移动框架方面有深厚积累。我们提供从需求分析、架构设计到开发交付的全流程服务，已为多家跨行业企业交付生产级自动更新解决方案与企业级应用。'}
            </p>
          </div>

          {/* ── 服务范围 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h2">
              {isEn ? 'Our Services' : '服务范围'}
            </Heading>
            <p style={{margin: 0, color: 'var(--ifm-color-secondary-darkest)'}}>
              {isEn
                ? 'Including but not limited to: enterprise desktop application development (WPF / Avalonia / MAUI), auto-update system integration and customization, third-party system integration, legacy system migration and modernization, performance optimization, and technical consulting.'
                : '包括但不限于：企业级桌面应用开发（WPF / Avalonia / MAUI）、自动更新系统集成与定制、第三方系统对接、遗留系统迁移与现代化改造、性能优化与技术咨询。'}
            </p>
          </div>

          {/* ── 合作方式 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h2">
              {isEn ? 'How to Engage' : '合作方式'}
            </Heading>
            <p style={{color: 'var(--ifm-color-secondary-darkest)', marginBottom: '1rem'}}>
              {isEn
                ? 'Scan the QR code below to contact us with your project requirements. We will get back to you within 1–2 business days to discuss your needs and propose a tailored solution.'
                : '扫描下方二维码联系我们，附上项目需求说明。我们会在 1–2 个工作日内与您取得联系，讨论您的需求并提供定制化方案。'}
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
