import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

const FeatureList = [
  {
    title: 'GeneralUpdate',
    Svg: require('@site/static/img/upgrade.svg').default,
    //description: (
    //  <>
    //    帮助你的客户端应用以最快最小的资源的占用完成自动升级！
    //  </>
    //),
  },
  {
    title: 'GeneralUpdate Tools',
    Svg: require('@site/static/img/packet.svg').default,
    //description: (
    //  <>
    //    打包工具帮助您发布更新补丁包文件！
    //  </>
    //),
  },
  {
    title: 'GeneralUpdate Samples',
    Svg: require('@site/static/img/samples.svg').default,
    //description: (
    //  <>
    //    快速启动，更快的了解项目如何使用！
    //  </>
    //),
  },
];

function Feature({Svg, title, description}) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
