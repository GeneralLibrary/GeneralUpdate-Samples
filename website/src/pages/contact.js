import React from 'react';
import Layout from '@theme/Layout';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Heading from '@theme/Heading';
import contactImg from '@site/docs/doc/imgs/contact.png';

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

          {/* ── 联系方式 ── */}
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h3">{isEn ? 'Contact' : '联系方式'}</Heading>
            <p style={{color: 'var(--ifm-color-secondary-darkest)', marginBottom: '1rem'}}>
              {isEn
                ? 'Scan the QR code below to contact us.'
                : '扫描下方二维码联系我们。'}
            </p>
            <div style={{textAlign: 'center'}}>
              <img src={contactImg} alt={isEn ? 'Contact QR Code' : '联系方式二维码'} style={{maxWidth: '100%', borderRadius: '8px'}} />
            </div>
          </div>

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

        </div>
      </main>
    </Layout>
  );
}
