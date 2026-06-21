import React, { useEffect, useRef, useState, useCallback } from 'react';
import ErrorBoundary from '@docusaurus/ErrorBoundary';
import { ErrorBoundaryErrorMessageFallback } from '@docusaurus/theme-common';
import {
  MermaidContainerClassName,
  useMermaidRenderResult,
} from '@docusaurus/theme-mermaid/client';
import panzoom from '@panzoom/panzoom';

import styles from './styles.module.css';

const PADDING = 80; // pixels of padding inside modal

function MermaidRenderResult({ renderResult }) {
  const ref = useRef(null);
  const [modalOpen, setModalOpen] = useState(false);
  const modalRef = useRef(null);
  const instanceRef = useRef(null);

  useEffect(() => {
    const div = ref.current;
    renderResult.bindFunctions?.(div);
  }, [renderResult]);

  const handleClick = useCallback(() => {
    setModalOpen(true);
  }, []);

  // Initialize panzoom and auto-fit SVG when modal opens
  useEffect(() => {
    if (!modalOpen || !modalRef.current) return;

    const modalEl = modalRef.current;
    const svgEl = modalEl.querySelector('svg');
    if (!svgEl) return;

    // Clean up previous
    if (instanceRef.current) {
      instanceRef.current.destroy();
    }

    const instance = panzoom(svgEl, {
      maxScale: 10,
      minScale: 0.1,
      step: 0.15,
      contain: 'outside',
      pinchAndPan: true,
    });
    instanceRef.current = instance;

    // Auto-fit: scale the SVG to fill the modal while keeping aspect ratio
    const fitToScreen = () => {
      const parent = modalRef.current;
      if (!parent) return;
      const svg = parent.querySelector('svg');
      if (!svg) return;

      const containerW = parent.clientWidth;
      const containerH = parent.clientHeight;
      const svgW = svg.viewBox?.baseVal?.width || svg.width?.baseVal?.value || svg.getBoundingClientRect().width;
      const svgH = svg.viewBox?.baseVal?.height || svg.height?.baseVal?.value || svg.getBoundingClientRect().height;

      if (!svgW || !svgH) return;

      const availableW = containerW - PADDING;
      const availableH = containerH - PADDING;
      const scale = Math.min(availableW / svgW, availableH / svgH, 3); // cap at 3x

      if (scale > 0) {
        instance.zoom(scale, { animate: false });
        // Center the SVG
        const scaledW = svgW * scale;
        const scaledH = svgH * scale;
        const offsetX = (containerW - scaledW) / 2;
        const offsetY = (containerH - scaledH) / 2;
        instance.pan(offsetX, offsetY, { animate: false });
      }
    };

    // Wait for layout, then fit
    const fitTimer = setTimeout(fitToScreen, 80);

    // Re-fit on resize
    window.addEventListener('resize', fitToScreen);

    const handleKeyDown = (e) => {
      if (e.key === 'Escape') {
        setModalOpen(false);
      }
      if (e.key === '+' || e.key === '=') {
        e.preventDefault();
        instance.zoomIn();
      }
      if (e.key === '-') {
        e.preventDefault();
        instance.zoomOut();
      }
      if (e.key === '0') {
        e.preventDefault();
        fitToScreen(); // reset to fit
      }
    };

    window.addEventListener('keydown', handleKeyDown);

    return () => {
      clearTimeout(fitTimer);
      window.removeEventListener('resize', fitToScreen);
      window.removeEventListener('keydown', handleKeyDown);
      if (instanceRef.current) {
        instanceRef.current.destroy();
        instanceRef.current = null;
      }
    };
  }, [modalOpen]);

  return (
    <>
      <div
        ref={ref}
        className={`${MermaidContainerClassName} ${styles.container}`}
        dangerouslySetInnerHTML={{ __html: renderResult.svg }}
        onClick={handleClick}
        role="button"
        tabIndex={0}
        onKeyDown={(e) => { if (e.key === 'Enter') handleClick(); }}
        title="Click to zoom"
      />

      {modalOpen && (
        <div
          className={styles.overlay}
          onClick={() => setModalOpen(false)}
          role="presentation"
        >
          <div className={styles.toolbar}>
            <button
              className={styles.toolBtn}
              onClick={(e) => {
                e.stopPropagation();
                instanceRef.current?.zoomIn();
              }}
              title="Zoom in (+)"
            >
              ＋
            </button>
            <button
              className={styles.toolBtn}
              onClick={(e) => {
                e.stopPropagation();
                instanceRef.current?.zoomOut();
              }}
              title="Zoom out (-)"
            >
              −
            </button>
            <button
              className={styles.toolBtn}
              onClick={(e) => {
                e.stopPropagation();
                // Re-fit to screen
                const parent = modalRef.current;
                const svg = parent?.querySelector('svg');
                if (!svg || !instanceRef.current) return;
                const containerW = parent.clientWidth;
                const containerH = parent.clientHeight;
                const svgW = svg.viewBox?.baseVal?.width || svg.width?.baseVal?.value || svg.getBoundingClientRect().width;
                const svgH = svg.viewBox?.baseVal?.height || svg.height?.baseVal?.value || svg.getBoundingClientRect().height;
                if (!svgW || !svgH) return;
                const availableW = containerW - PADDING;
                const availableH = containerH - PADDING;
                const scale = Math.min(availableW / svgW, availableH / svgH, 3);
                if (scale > 0) {
                  instanceRef.current.zoom(scale, { animate: true });
                  const scaledW = svgW * scale;
                  const scaledH = svgH * scale;
                  instanceRef.current.pan(
                    (containerW - scaledW) / 2,
                    (containerH - scaledH) / 2,
                    { animate: true }
                  );
                }
              }}
              title="Fit to screen (0)"
            >
              ⊡
            </button>
            <button
              className={styles.toolBtn}
              onClick={(e) => {
                e.stopPropagation();
                setModalOpen(false);
              }}
              title="Close (Esc)"
            >
              ✕
            </button>
            <span className={styles.toolHint}>
              Scroll zoom · Drag pan · 0 fit
            </span>
          </div>
          <div
            ref={modalRef}
            className={styles.modalBody}
            onClick={(e) => e.stopPropagation()}
            role="presentation"
            dangerouslySetInnerHTML={{ __html: renderResult.svg }}
          />
        </div>
      )}
    </>
  );
}

function MermaidRenderer({ value }) {
  const renderResult = useMermaidRenderResult({ text: value });
  if (renderResult === null) {
    return null;
  }
  return <MermaidRenderResult renderResult={renderResult} />;
}

export default function Mermaid(props) {
  return (
    <ErrorBoundary
      fallback={(params) => <ErrorBoundaryErrorMessageFallback {...params} />}
    >
      <MermaidRenderer {...props} />
    </ErrorBoundary>
  );
}
