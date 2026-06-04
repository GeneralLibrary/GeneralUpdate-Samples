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
          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h3">{isEn ? 'QQ Group' : 'QQ 群'}</Heading>
            <p style={{fontSize: '1.25rem', fontWeight: 'bold', margin: 0}}>
              748744489
            </p>
            <p style={{color: 'var(--ifm-color-secondary-darkest)', marginTop: '0.5rem'}}>
              {isEn
                ? 'Join our QQ group for community discussion and real-time support.'
                : '加入 QQ 群参与社区讨论，获取实时技术支持。'}
            </p>
          </div>

          <div style={{
            border: '1px solid var(--ifm-color-emphasis-300)',
            borderRadius: '8px',
            padding: '1.5rem',
          }}>
            <Heading as="h3">{isEn ? 'Email' : '邮箱'}</Heading>
            <p style={{fontSize: '1.25rem', margin: 0}}>
              <a href="mailto:zhuzhen723723@outlook.com">zhuzhen723723@outlook.com</a>
            </p>
            <p style={{color: 'var(--ifm-color-secondary-darkest)', marginTop: '0.5rem'}}>
              {isEn
                ? 'Send us an email for business inquiries or private questions.'
                : '发送邮件咨询商务合作或私密问题。'}
            </p>
          </div>
        </div>
      </main>
    </Layout>
  );
}
