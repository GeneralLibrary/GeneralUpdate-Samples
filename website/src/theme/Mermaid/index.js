import React, { useEffect, useRef, useState, useCallback } from 'react';
import ErrorBoundary from '@docusaurus/ErrorBoundary';
import { ErrorBoundaryErrorMessageFallback } from '@docusaurus/theme-common';
import {
  MermaidContainerClassName,
  useMermaidRenderResult,
} from '@docusaurus/theme-mermaid/client';
import panzoom from '@panzoom/panzoom';

import styles from './styles.module.css';

const PADDING = 60;

function MermaidRenderResult({ renderResult }) {
  const ref = useRef(null);
  const [modalOpen, setModalOpen] = useState(false);
  const modalRef = useRef(null);
  const instanceRef = useRef(null);

  useEffect(() => {
    const div = ref.current;
    if (div) renderResult.bindFunctions?.(div);
  }, [renderResult]);

  const handleClick = useCallback(() => {
    setModalOpen(true);
  }, []);

  // ── Modal: panzoom init + auto-fit ──────────────────
  useEffect(() => {
    if (!modalOpen || !modalRef.current) return;

    const body = modalRef.current;
    const svg = body.querySelector('svg');
    if (!svg) return;

    // Clean up previous instance
    if (instanceRef.current) {
      instanceRef.current.destroy();
    }

    // 1. Create panzoom instance
    const instance = panzoom(svg, {
      maxScale: 12,
      minScale: 0.05,
      step: 0.2,
      contain: false,
      pinchAndPan: true,
      // Let SVG keep its native size initially; we'll zoom below
      cursor: 'grab',
    });
    instanceRef.current = instance;

    // 2. Fit to screen – retry with exponential backoff
    let fitAttempts = 0;
    const maxAttempts = 8;

    const fitToScreen = () => {
      const svg = body.querySelector('svg');
      if (!svg) return;

      const w = body.clientWidth;
      const h = body.clientHeight;
      if (!w || !h) return;

      // SVG native dimensions
      let svgW = svg.viewBox?.baseVal?.width;
      let svgH = svg.viewBox?.baseVal?.height;

      // Fallback to width/height attributes
      if (!svgW) svgW = svg.width?.baseVal?.value;
      if (!svgH) svgH = svg.height?.baseVal?.value;

      // Fallback to computed bounding rect
      if (!svgW || !svgH) {
        const rect = svg.getBoundingClientRect();
        svgW = rect.width;
        svgH = rect.height;
      }

      // If still zero and we have retries left, try again
      if ((!svgW || !svgH) && fitAttempts < maxAttempts) {
        fitAttempts++;
        setTimeout(fitToScreen, 100 * fitAttempts);
        return;
      }

      if (!svgW || !svgH) return;

      const availW = w - PADDING * 2;
      const availH = h - PADDING * 2;
      const scale = Math.min(availW / svgW, availH / svgH, 3);

      if (scale > 0) {
        instance.zoom(scale, { animate: false });
        instance.pan(
          (w - svgW * scale) / 2,
          (h - svgH * scale) / 2,
          { animate: false }
        );
      }
    };

    // Kick off fit with first attempt
    const fitTimer = setTimeout(fitToScreen, 100);

    // 3. Wheel zoom on the body
    const handleWheel = (e) => {
      e.preventDefault();
      const svgEl = body.querySelector('svg');
      if (!svgEl) return;
      instance.zoomWithWheel(e, { step: 0.3, minScale: 0.05, maxScale: 12 });
    };
    body.addEventListener('wheel', handleWheel, { passive: false });

    // 4. Resize listener
    window.addEventListener('resize', fitToScreen);

    // 5. Keyboard shortcuts
    const handleKeyDown = (e) => {
      if (e.key === 'Escape') { setModalOpen(false); return; }
      if (e.key === '+' || e.key === '=') { e.preventDefault(); instance.zoomIn(); }
      if (e.key === '-') { e.preventDefault(); instance.zoomOut(); }
      if (e.key === '0') { e.preventDefault(); fitToScreen(); }
    };
    window.addEventListener('keydown', handleKeyDown);

    return () => {
      clearTimeout(fitTimer);
      body.removeEventListener('wheel', handleWheel);
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
      {/* ── Inline diagram ─────────────────────────── */}
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

      {/* ── Fullscreen modal ─────────────────────────── */}
      {modalOpen && (
        <div
          className={styles.overlay}
          onClick={() => setModalOpen(false)}
          role="presentation"
        >
          {/* Toolbar */}
          <div className={styles.toolbar}>
            <button className={styles.toolBtn}
              onClick={(e) => { e.stopPropagation(); instanceRef.current?.zoomIn(); }}
              title="Zoom in (+)">＋</button>
            <button className={styles.toolBtn}
              onClick={(e) => { e.stopPropagation(); instanceRef.current?.zoomOut(); }}
              title="Zoom out (-)">−</button>
            <button className={styles.toolBtn}
              onClick={(e) => {
                e.stopPropagation();
                const body = modalRef.current;
                if (!body || !instanceRef.current) return;
                const svg = body.querySelector('svg');
                if (!svg) return;
                const w = body.clientWidth, h = body.clientHeight;
                let svgW = svg.viewBox?.baseVal?.width || svg.width?.baseVal?.value || svg.getBoundingClientRect().width;
                let svgH = svg.viewBox?.baseVal?.height || svg.height?.baseVal?.value || svg.getBoundingClientRect().height;
                if (!svgW || !svgH) return;
                const availW = w - PADDING * 2, availH = h - PADDING * 2;
                const scale = Math.min(availW / svgW, availH / svgH, 3);
                if (scale > 0) {
                  instanceRef.current.zoom(scale, { animate: true });
                  instanceRef.current.pan((w - svgW * scale) / 2, (h - svgH * scale) / 2, { animate: true });
                }
              }}
              title="Fit to screen (0)">⊡</button>
            <button className={styles.toolBtn}
              onClick={(e) => { e.stopPropagation(); setModalOpen(false); }}
              title="Close (Esc)">✕</button>
            <span className={styles.toolHint}>Scroll zoom · Drag pan · 0 fit</span>
          </div>

          {/* Body – scrollable fallback so content is never hidden */}
          <div
            ref={modalRef}
            className={styles.modalBody}
            onClick={(e) => e.stopPropagation()}
            role="presentation"
          >
            <div
              className={styles.svgWrapper}
              dangerouslySetInnerHTML={{ __html: renderResult.svg }}
            />
          </div>
        </div>
      )}
    </>
  );
}

function MermaidRenderer({ value }) {
  const renderResult = useMermaidRenderResult({ text: value });
  if (renderResult === null) return null;
  return <MermaidRenderResult renderResult={renderResult} />;
}

export default function Mermaid(props) {
  return (
    <ErrorBoundary
      fallback={(params) => <ErrorBoundaryErrorMessageFallback {...params} />}>
      <MermaidRenderer {...props} />
    </ErrorBoundary>
  );
}
