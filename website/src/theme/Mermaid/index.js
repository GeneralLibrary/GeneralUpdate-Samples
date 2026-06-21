import React, { useEffect, useRef, useState, useCallback } from 'react';
import ErrorBoundary from '@docusaurus/ErrorBoundary';
import { ErrorBoundaryErrorMessageFallback } from '@docusaurus/theme-common';
import {
  MermaidContainerClassName,
  useMermaidRenderResult,
} from '@docusaurus/theme-mermaid/client';
import panzoom from '@panzoom/panzoom';

import styles from './styles.module.css';

function MermaidRenderResult({ renderResult }) {
  const ref = useRef(null);
  const [modalOpen, setModalOpen] = useState(false);
  const modalRef = useRef(null);
  const zoomRef = useRef(null);
  const instanceRef = useRef(null);

  useEffect(() => {
    const div = ref.current;
    renderResult.bindFunctions?.(div);
  }, [renderResult]);

  // Handle click on the mermaid diagram to open modal
  const handleClick = useCallback(() => {
    setModalOpen(true);
  }, []);

  // Initialize panzoom when modal opens
  useEffect(() => {
    if (!modalOpen || !modalRef.current) return;

    const modalEl = modalRef.current;
    const svgEl = modalEl.querySelector('svg');
    if (!svgEl) return;

    // Clean up any previous instance
    if (instanceRef.current) {
      instanceRef.current.destroy();
    }

    const instance = panzoom(svgEl, {
      maxScale: 10,
      minScale: 0.3,
      step: 0.3,
      contain: 'outside',
      pinchAndPan: true,
    });
    instanceRef.current = instance;

    // Reset zoom on open
    // Use a small delay to let the modal render finish
    const resetTimer = setTimeout(() => {
      instance.reset({ animate: false });
    }, 50);

    // Keyboard handlers
    const handleKeyDown = (e) => {
      if (e.key === 'Escape') {
        setModalOpen(false);
      }
      // Zoom with +/-
      if (e.key === '+' || e.key === '=') {
        e.preventDefault();
        instance.zoomIn();
      }
      if (e.key === '-') {
        e.preventDefault();
        instance.zoomOut();
      }
    };

    window.addEventListener('keydown', handleKeyDown);

    return () => {
      clearTimeout(resetTimer);
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
                instanceRef.current?.reset({ animate: true });
              }}
              title="Reset"
            >
              ↺
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
              Scroll to zoom · Drag to pan
            </span>
          </div>
          <div
            className={styles.modalBody}
            onClick={(e) => e.stopPropagation()}
            role="presentation"
          >
            <div
              ref={modalRef}
              className={styles.zoomContainer}
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
