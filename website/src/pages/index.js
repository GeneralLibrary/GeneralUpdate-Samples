import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';
import styles from './index.module.css';

function CosmicHero() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <section className={styles.cosmicHero}>
      <div className={styles.cosmicBackground}>
        <div className={styles.stars}></div>
        <div className={styles.planets}></div>
      </div>
      <div className={styles.heroContent}>
        <div className={styles.pixelLogo}>
          <div className={styles.logoPixel}></div>
        </div>
        <Heading as="h1" className={styles.cosmicTitle}>
          {siteConfig.title}
        </Heading>
        <p className={styles.cosmicSubtitle}>
          🚀 跨平台自动更新框架 · 极简 · 高效 · 开源
        </p>
        <div className={styles.actionButtons}>
          <Link className={styles.primaryBtn} to="/docs/doc/GeneralSpacestation">
            <span className={styles.btnIcon}>▶</span> 开始探索
          </Link>
          <a className={styles.secondaryBtn} href="https://github.com/GeneralLibrary" target="_blank" rel="noopener noreferrer">
            <span className={styles.btnIcon}>★</span> GitHub
          </a>
        </div>
      </div>
    </section>
  );
}

function FeatureCard({ icon, title, description, link }) {
  return (
    <Link to={link} className={styles.featureCard}>
      <div className={styles.cardIcon}>{icon}</div>
      <h3 className={styles.cardTitle}>{title}</h3>
      <p className={styles.cardDesc}>{description}</p>
      <div className={styles.cardArrow}>→</div>
    </Link>
  );
}

function Features() {
  return (
    <section className={styles.featuresSection}>
      <div className={styles.container}>
        <div className={styles.sectionHeader}>
          <Heading as="h2" className={styles.sectionTitle}>
            核心组件
          </Heading>
          <div className={styles.titleUnderline}></div>
        </div>
        <div className={styles.featureGrid}>
          <FeatureCard
            icon="🚀"
            title="GeneralUpdate"
            description="轻量级跨平台自动更新客户端"
            link="/docs/doc/GeneralSpacestation"
          />
          <FeatureCard
            icon="🛠️"
            title="Update Tools"
            description="自动化补丁包生成与发布工具"
            link="/docs/doc/GeneralSpacestation"
          />
          <FeatureCard
            icon="💡"
            title="Quick Start"
            description="快速上手示例与最佳实践"
            link="/docs/doc/GeneralSpacestation"
          />
        </div>
      </div>
    </section>
  );
}

function TechStack() {
  const techs = [
    { name: '.NET', color: '#512bd4' },
    { name: 'WPF', color: '#0078d4' },
    { name: 'Avalonia', color: '#8b5cf6' },
    { name: 'MAUI', color: '#3498db' },
    { name: 'Console', color: '#2ecc71' },
  ];

  return (
    <section className={styles.techSection}>
      <div className={styles.container}>
        <Heading as="h3" className={styles.techTitle}>
          支持平台
        </Heading>
        <div className={styles.techGrid}>
          {techs.map((tech, idx) => (
            <div key={idx} className={styles.techBadge} style={{ borderColor: tech.color }}>
              <span className={styles.techDot} style={{ backgroundColor: tech.color }}></span>
              {tech.name}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

export default function Home() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={`${siteConfig.title} - 跨平台自动更新框架`}
      description="GeneralUpdate - 轻量级、跨平台、易用的 .NET 自动更新框架">
      <CosmicHero />
      <Features />
      <TechStack />
    </Layout>
  );
}
