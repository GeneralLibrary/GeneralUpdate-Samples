import React from 'react';
import Layout from '@theme/Layout';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Heading from '@theme/Heading';
import styles from './corporate-wall.module.css';

function CompanyCard({ name, description, descriptionEn }) {
  const {i18n} = useDocusaurusContext();
  const isEn = i18n.currentLocale === 'en';
  return (
    <div className={styles.card}>
      <div className={styles.cardHeader}>
        <h3 className={styles.cardName}>{name}</h3>
      </div>
      <p className={styles.cardDesc}>
        {isEn ? descriptionEn : description}
      </p>
    </div>
  );
}

export default function CorporateWall() {
  const {i18n} = useDocusaurusContext();
  const isEn = i18n.currentLocale === 'en';

  const companies = [
    {
      name: '天津云度科技',
      description: '专注于智慧城市应用技术与产品服务，在大数据、AI 技术服务、平台运营及系统集成等领域为政府及企业提供一站式信息化解决方案。',
      descriptionEn: 'Focused on smart city application technology and products, providing one-stop digital solutions for governments and enterprises in big data, AI services, platform operations, and system integration.',
    },
    {
      name: '上海铱泓科技',
      description: '致力于开源 UI 组件库研发，旗下 Semi.Avalonia 和 Ursa.Avalonia 等优秀开源项目被广泛使用。',
      descriptionEn: 'Dedicated to open-source UI component library development, with excellent projects like Semi.Avalonia and Ursa.Avalonia widely adopted.',
    },
    {
      name: '天津****化工工程有限公司',
      description: '将 GeneralUpdate 应用于工业自动化系统的客户端升级管理。',
      descriptionEn: 'Using GeneralUpdate for client upgrade management in industrial automation systems.',
    },
    {
      name: '沈阳**汽车科技有限公司',
      description: '在车载智能终端软件更新中采用 GeneralUpdate 解决方案。',
      descriptionEn: 'Adopting GeneralUpdate solutions for software updates in automotive intelligent terminals.',
    },
    {
      name: '杭州猿通信息科技有限责任公司',
      description: '专注于 AI 原生应用与数据处理技术研发，旗下核心产品"决策链（DecisionLinnc）"定位为下一代智能体操作系统，致力于为金融、医疗、科研等领域提供智能化解决方案。',
      descriptionEn: 'Specializing in AI-native application and data processing technologies, with the core product "DecisionLinnc" positioned as the next-generation agent operating system, providing intelligent solutions for finance, healthcare, and scientific research.',
    },
    {
      name: '上海**导航技术股份有限公司',
      description: '将 GeneralUpdate 应用于高精度卫星导航定位系统的客户端软件升级与远程运维管理。',
      descriptionEn: 'Applying GeneralUpdate for client software update and remote operations management in high-precision satellite navigation and positioning systems.',
    },
  ];

  const title = isEn ? 'Corporate Wall' : '企业墙';
  const description = isEn
    ? 'Companies using GeneralUpdate in production'
    : '在生产环境中使用 GeneralUpdate 的企业';

  return (
    <Layout title={title} description={description}>
      <main className={styles.page}>
        <Heading as="h1" className={styles.pageTitle}>
          {title}
        </Heading>
        <p className={styles.pageSubtitle}>
          {isEn
            ? 'Special thanks to the following companies using GeneralUpdate in production.'
            : '感谢以下企业在生产环境中使用 GeneralUpdate。'}
        </p>
        <div className={styles.cardGrid}>
          {companies.map((c, i) => <CompanyCard key={i} {...c} />)}
        </div>
      </main>
    </Layout>
  );
}
