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
      description: '专注于云计算与数字化转型，在多条产品线中集成 GeneralUpdate 实现客户端自动更新。',
      descriptionEn: 'Focused on cloud computing and digital transformation, integrating GeneralUpdate across multiple product lines for automatic client updates.',
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
