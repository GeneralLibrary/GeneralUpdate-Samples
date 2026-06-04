import React from 'react';
import Layout from '@theme/Layout';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Heading from '@theme/Heading';

export default function Contact() {
  const {i18n} = useDocusaurusContext();
  const isEn = i18n.currentLocale === 'en';

  const title = isEn ? 'Contact Us' : '联系方式';
  const description = isEn
    ? 'Get in touch with the GeneralUpdate team'
    : '与 GeneralUpdate 团队取得联系';

  return (
    <Layout title={title} description={description}>
      <main style={{maxWidth: 800, margin: '0 auto', padding: '4rem 2rem'}}>
        <Heading as="h1">{title}</Heading>
        <p style={{color: 'var(--ifm-color-secondary-darkest)', marginBottom: '2rem'}}>
          {isEn
            ? 'Have questions, suggestions, or need help? Reach out through the channels below.'
            : '有问题、建议或需要帮助？通过以下方式联系我们。'}
        </p>

        <div style={{display: 'flex', flexDirection: 'column', gap: '2rem'}}>

          {/* ── QQ 群 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h3">{isEn ? 'QQ Groups' : 'QQ 群'}</Heading>
            <div style={{display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: '0.75rem'}}>
              <div>
                <span style={{fontWeight: 'bold', fontSize: '1.1rem'}}>
                  {isEn ? 'GeneralUpdate Discussion Group' : 'GeneralUpdate 交流群'}：
                </span>
                <code style={{fontSize: '1.1rem', marginLeft: '0.5rem'}}>748744489</code>
              </div>
              <div>
                <span style={{fontWeight: 'bold', fontSize: '1.1rem'}}>
                  {isEn ? '.NET Technical Exchange Group' : '.NET 技术交流群'}：
                </span>
                <code style={{fontSize: '1.1rem', marginLeft: '0.5rem'}}>341349660</code>
              </div>
            </div>
            <p style={{color: 'var(--ifm-color-secondary-darkest)', marginTop: '0.75rem', fontSize: '0.9rem'}}>
              {isEn
                ? 'Join our QQ groups for community discussion and real-time support. Questions in discussion groups are visible to everyone to avoid repetitive answers.'
                : '加入 QQ 群参与社区讨论，获取实时技术支持。讨论群提问大家都能看到避免重复回答问题。'}
            </p>
          </div>

          {/* ── GitHub Issues ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h3">GitHub Issues</Heading>
            <p style={{margin: 0}}>
              <a href="https://github.com/GeneralLibrary/GeneralUpdate/issues" target="_blank" rel="noopener">
                github.com/GeneralLibrary/GeneralUpdate/issues
              </a>
            </p>
            <p style={{color: 'var(--ifm-color-secondary-darkest)', marginTop: '0.5rem'}}>
              {isEn
                ? 'Submit bug reports, feature requests, or ask questions on GitHub. Issues are publicly visible and help everyone.'
                : '提交 Bug 报告、功能建议或问题答疑。Issue 公开可见，帮助所有人。'}
            </p>
          </div>

          {/* ── Email ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h3">{isEn ? 'Email' : '邮箱'}</Heading>
            <p style={{margin: 0}}>
              <a href="mailto:zhuzhen723723@outlook.com">zhuzhen723723@outlook.com</a>
            </p>
            <p style={{color: 'var(--ifm-color-secondary-darkest)', marginTop: '0.5rem'}}>
              {isEn
                ? 'For business inquiries, private questions, or one-on-one paid consultations.'
                : '商务合作、私密问题或一对一付费咨询。'}
            </p>
          </div>

          {/* ── 付费咨询 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
            background: 'var(--ifm-color-warning-contrast-background)',
          }}>
            <Heading as="h3">{isEn ? 'Paid Consultation' : '付费咨询'}</Heading>
            <p style={{margin: 0, color: 'var(--ifm-color-secondary-darkest)'}}>
              {isEn
                ? 'Due to the large number of individual communications, the author\'s time and energy are limited. One-on-one technical support and customized secondary development require paid consultation. Please state your purpose when adding (WeChat recommended, idle chats are refused).'
                : '由于单独沟通人数过多作者时间精力有限，一对一技术支持和定制化二次开发需付费咨询。加好友请注明来意（推荐微信，拒绝闲聊）。'}
            </p>
          </div>

          {/* ── 企业墙 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h3">{isEn ? 'Corporate Wall' : '企业墙'}</Heading>
            <p style={{margin: 0, color: 'var(--ifm-color-secondary-darkest)'}}>
              {isEn
                ? 'The open-source project is building a corporate wall on the official website. If your company is using GeneralUpdate in production and would like free promotion, please contact the author.'
                : '本开源项目在官网上建立企业墙，如果有企业在项目中有使用本项目并且想上墙进行免费宣传可以联系作者。'}
            </p>
          </div>

          {/* ── 赞助 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h3">{isEn ? 'Sponsorship' : '赞助一下'}</Heading>
            <p style={{color: 'var(--ifm-color-secondary-darkest)'}}>
              {isEn
                ? 'All community donations will be used for the development of open-source projects and to reward code contributors. Sponsors\' company logos will be displayed on the project website. Your sponsorship will help us improve quality, add features, and provide a better user experience.'
                : '所有的社区捐赠将用于开源项目的发展建设和奖励代码贡献者。赞助者的公司标志将展示在项目网站上。您的赞助将帮助我们提高项目质量、增加更多功能、提供更好的用户体验。'}
            </p>
            <p style={{color: 'var(--ifm-color-secondary-darkest)', fontSize: '0.9rem', fontStyle: 'italic'}}>
              {isEn
                ? 'Code contributors: please contact the email above to claim your rewards.'
                : '代码贡献者请联系上方邮箱领取奖励。'}
            </p>
          </div>

        </div>
      </main>
    </Layout>
  );
}
