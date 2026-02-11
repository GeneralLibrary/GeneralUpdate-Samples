import React from 'react';
import styles from './styles.module.css';

export default function SpaceAnimation() {
  return (
    <div className={styles.animationContainer}>
      {/* Spaceship Background */}
      <div className={styles.spaceship}>
        <div className={styles.spaceshipWindow}></div>
        <div className={styles.spaceshipPanel}></div>
      </div>

      {/* Astronaut Character */}
      <div className={styles.astronaut}>
        {/* Helmet */}
        <div className={styles.helmet}>
          <div className={styles.visor}></div>
          <div className={styles.visorGlare}></div>
        </div>
        {/* Body */}
        <div className={styles.body}>
          <div className={styles.chestPack}></div>
        </div>
        {/* Right Arm (raised) */}
        <div className={styles.armRight}>
          <div className={styles.handRight}></div>
        </div>
        {/* Left Arm (operating device) */}
        <div className={styles.armLeft}>
          <div className={styles.handLeft}></div>
          <div className={styles.wristDevice}>
            <div className={styles.deviceScreen}></div>
            <div className={styles.deviceButton}></div>
          </div>
        </div>
        {/* Legs */}
        <div className={styles.legs}>
          <div className={styles.legLeft}></div>
          <div className={styles.legRight}></div>
        </div>
      </div>

      {/* Orange Cat */}
      <div className={styles.cat}>
        {/* Cat Head */}
        <div className={styles.catHead}>
          <div className={styles.catEarLeft}></div>
          <div className={styles.catEarRight}></div>
          <div className={styles.catFace}>
            <div className={styles.catEyeLeft}></div>
            <div className={styles.catEyeRight}></div>
            <div className={styles.catNose}></div>
          </div>
        </div>
        {/* Cat Body */}
        <div className={styles.catBody}></div>
        {/* Cat Legs */}
        <div className={styles.catLegs}>
          <div className={styles.catLegLeft}></div>
          <div className={styles.catLegRight}></div>
        </div>
        {/* Cat Tail */}
        <div className={styles.catTail}></div>
        
        {/* Sci-fi Light Effect */}
        <div className={styles.lightEffect}>
          <div className={styles.lightRing1}></div>
          <div className={styles.lightRing2}></div>
          <div className={styles.lightRing3}></div>
          <div className={styles.pixelFlash}></div>
        </div>
        
        {/* Cat Spacesuit (appears after animation) */}
        <div className={styles.catSpacesuit}>
          <div className={styles.catHelmet}>
            <div className={styles.catVisor}></div>
          </div>
          <div className={styles.catSuitBody}></div>
          <div className={styles.catSuitPack}></div>
        </div>
      </div>

      {/* Floating Particles */}
      <div className={styles.particles}>
        <div className={styles.particle}></div>
        <div className={styles.particle}></div>
        <div className={styles.particle}></div>
        <div className={styles.particle}></div>
        <div className={styles.particle}></div>
      </div>
    </div>
  );
}
