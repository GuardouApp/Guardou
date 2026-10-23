import type {ReactNode} from 'react';
import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  description: ReactNode;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Easy to Use',
    description: (
      <>
        Built for non-technical users. Guardou follows familiar UI patterns,
        so it feels at home from the first click.
      </>
    ),
  },
  {
    title: 'Focused on What Matters',
    description: (
      <>
        No bloat. Guardou only ships features that are core to storing and
        organizing your files.
      </>
    ),
  },
  {
    title: 'Free and Open Source',
    description: (
      <>
        No ads. No telemetry. Free forever, licensed under the AGPL v3.
      </>
    ),
  },
];

function Feature({title, description}: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): ReactNode {
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
