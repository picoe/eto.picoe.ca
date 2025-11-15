import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

const FeatureList = [
  {
    title: 'Easy to Use',
    Svg: require('@site/static/img/eto-easy.svg').default,
    description: (
      <>
        Eto.Forms makes building cross-platform desktop apps simple. 
        With a clean API and intuitive design, you can create powerful UIs in just a few lines of C#.
      </>
    ),
  },
  {
    title: 'Native on Every Platform',
    Svg: require('@site/static/img/eto-native.svg').default,
    description: (
      <>
        Write your UI once, and Eto.Forms makes it look and feel right at home on Windows, macOS, and Linux — using each platform's native widgets.
      </>
    ),
  },
  {
    title: 'Flexible and Extensible',
    Svg: require('@site/static/img/eto-flexible.svg').default,
    description: (
      <>
        Eto.Forms is built for customization. Extend controls, integrate with existing libraries, or design your own - all while keeping your core code portable.
      </>
    ),
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
