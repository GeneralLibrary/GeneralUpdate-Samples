import React from 'react';
import styles from './styles.module.css';

export default function SpaceAnimation() {
  return (
    <div className={styles.animationContainer}>
      {/* Space Background with Portal Effect */}
      <div className={styles.spacePortal}>
        <div className={styles.portalRing1}></div>
        <div className={styles.portalRing2}></div>
        <div className={styles.portalRing3}></div>
        <div className={styles.portalCore}></div>
      </div>

      {/* Astronaut Character - More Detailed */}
      <div className={styles.astronaut}>
        {/* Helmet with detailed visor */}
        <div className={styles.helmet}>
          <div className={styles.helmetTop}></div>
          <div className={styles.helmetBottom}></div>
          <div className={styles.visor}>
            <div className={styles.visorReflection}></div>
            <div className={styles.visorStars}></div>
          </div>
          <div className={styles.helmetRing}></div>
        </div>
        
        {/* Upper Body with detailed suit */}
        <div className={styles.torso}>
          <div className={styles.chestPanel}></div>
          <div className={styles.shoulderLeft}></div>
          <div className={styles.shoulderRight}></div>
          <div className={styles.backpack}>
            <div className={styles.backpackLight}></div>
          </div>
        </div>
        
        {/* Right Arm - Raised and waving */}
        <div className={styles.armRight}>
          <div className={styles.upperArmRight}></div>
          <div className={styles.elbowRight}></div>
          <div className={styles.forearmRight}></div>
          <div className={styles.gloveRight}>
            <div className={styles.finger}></div>
            <div className={styles.finger}></div>
            <div className={styles.thumb}></div>
          </div>
        </div>
        
        {/* Left Arm - Operating wrist device */}
        <div className={styles.armLeft}>
          <div className={styles.upperArmLeft}></div>
          <div className={styles.elbowLeft}></div>
          <div className={styles.forearmLeft}>
            <div className={styles.wristDevice}>
              <div className={styles.deviceFrame}></div>
              <div className={styles.deviceScreen}>
                <div className={styles.screenGlow}></div>
              </div>
              <div className={styles.deviceButton}></div>
              <div className={styles.deviceStraps}></div>
            </div>
          </div>
          <div className={styles.gloveLeft}></div>
        </div>
        
        {/* Lower Body */}
        <div className={styles.waist}></div>
        <div className={styles.legs}>
          <div className={styles.legLeft}>
            <div className={styles.thighLeft}></div>
            <div className={styles.kneeLeft}></div>
            <div className={styles.shinLeft}></div>
            <div className={styles.bootLeft}></div>
          </div>
          <div className={styles.legRight}>
            <div className={styles.thighRight}></div>
            <div className={styles.kneeRight}></div>
            <div className={styles.shinRight}></div>
            <div className={styles.bootRight}></div>
          </div>
        </div>
      </div>

      {/* Orange Cat - More Realistic */}
      <div className={styles.cat}>
        {/* Cat Head with detailed features */}
        <div className={styles.catHead}>
          <div className={styles.catSkull}>
            <div className={styles.catEarLeft}>
              <div className={styles.earInner}></div>
            </div>
            <div className={styles.catEarRight}>
              <div className={styles.earInner}></div>
            </div>
            <div className={styles.catForehead}></div>
          </div>
          
          <div className={styles.catFace}>
            <div className={styles.catEyeLeft}>
              <div className={styles.eyePupil}></div>
              <div className={styles.eyeGleam}></div>
            </div>
            <div className={styles.catEyeRight}>
              <div className={styles.eyePupil}></div>
              <div className={styles.eyeGleam}></div>
            </div>
            <div className={styles.catMuzzle}>
              <div className={styles.catNose}></div>
              <div className={styles.catMouth}></div>
            </div>
            <div className={styles.catWhiskers}>
              <div className={styles.whisker}></div>
              <div className={styles.whisker}></div>
              <div className={styles.whisker}></div>
            </div>
          </div>
        </div>
        
        {/* Cat Body with realistic proportions */}
        <div className={styles.catBody}>
          <div className={styles.catChest}></div>
          <div className={styles.catBelly}></div>
          <div className={styles.catFur}></div>
        </div>
        
        {/* Cat Legs */}
        <div className={styles.catFrontLegs}>
          <div className={styles.catLegFrontLeft}>
            <div className={styles.catPaw}></div>
          </div>
          <div className={styles.catLegFrontRight}>
            <div className={styles.catPaw}></div>
          </div>
        </div>
        <div className={styles.catBackLegs}>
          <div className={styles.catLegBackLeft}>
            <div className={styles.catPaw}></div>
          </div>
          <div className={styles.catLegBackRight}>
            <div className={styles.catPaw}></div>
          </div>
        </div>
        
        {/* Cat Tail with realistic curve */}
        <div className={styles.catTail}>
          <div className={styles.tailSegment}></div>
          <div className={styles.tailSegment}></div>
          <div className={styles.tailSegment}></div>
        </div>
        
        {/* Transformation Light Effect */}
        <div className={styles.transformEffect}>
          <div className={styles.lightRing1}></div>
          <div className={styles.lightRing2}></div>
          <div className={styles.lightRing3}></div>
          <div className={styles.energyFlash}>
            <div className={styles.flashCore}></div>
            <div className={styles.flashRays}></div>
          </div>
        </div>
        
        {/* Cat Spacesuit - Appears after transformation */}
        <div className={styles.catSpacesuit}>
          <div className={styles.catHelmet}>
            <div className={styles.catHelmetGlass}>
              <div className={styles.catVisorReflection}></div>
            </div>
            <div className={styles.catHelmetFrame}></div>
            <div className={styles.catHelmetSeal}></div>
          </div>
          <div className={styles.catSuitTorso}>
            <div className={styles.catSuitPanel}></div>
            <div className={styles.catSuitLights}></div>
          </div>
          <div className={styles.catSuitPack}>
            <div className={styles.packTank}></div>
            <div className={styles.packIndicator}></div>
          </div>
        </div>
      </div>

      {/* Floating Energy Particles */}
      <div className={styles.particles}>
        {[...Array(8)].map((_, i) => (
          <div key={i} className={styles.particle} style={{
            '--delay': `${i * 0.3}s`,
            '--x': `${(i % 4) * 25}%`,
            '--y': `${Math.floor(i / 4) * 50}%`
          }}></div>
        ))}
      </div>
    </div>
  );
}
