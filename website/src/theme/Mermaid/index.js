import React, { useEffect, useRef, useState, useCallback } from 'react';
import ErrorBoundary from '@docusaurus/ErrorBoundary';
import { ErrorBoundaryErrorMessageFallback } from '@docusaurus/theme-common';
import {
  MermaidContainerClassName,
  useMermaidRenderResult,
} from '@docusaurus/theme-mermaid/client';

const LEVELS = [0.25, 0.33, 0.5, 0.67, 0.8, 1, 1.25, 1.5, 2, 2.5, 3, 4];

function MermaidRenderResult({ renderResult }) {
  const inlineRef = useRef(null);
  const viewerRef = useRef(null);
  const [open, setOpen] = useState(false);
  const [scale, setScale] = useState(1);
  const [nat, setNat] = useState({ w: 800, h: 600 });
  const dragRef = useRef({ on: false, x: 0, y: 0, sx: 0, sy: 0 });

  useEffect(() => {
    const div = inlineRef.current;
    if (div) renderResult.bindFunctions?.(div);
  }, [renderResult]);

  const onOpen = useCallback(() => {
    setScale(1);
    setOpen(true);
    // measure the inline SVG (already rendered in page DOM)
    const svg = inlineRef.current?.querySelector('svg');
    if (svg) {
      setNat({
        w: svg.viewBox?.baseVal?.width || svg.width?.baseVal?.value || 800,
        h: svg.viewBox?.baseVal?.height || svg.height?.baseVal?.value || 600,
      });
    }
  }, []);
  const onClose = useCallback(() => setOpen(false), []);

  const zoomIn  = useCallback(() => setScale((s) => { const i = LEVELS.findIndex((l) => l > s); return i >= 0 ? LEVELS[i] : s; }), []);
  const zoomOut = useCallback(() => setScale((s) => { const i = [...LEVELS].reverse().findIndex((l) => l < s); return i >= 0 ? LEVELS[LEVELS.length - 1 - i] : s; }), []);

  // Drag → scroll viewer
  const onMouseDown = useCallback((e) => {
    const v = viewerRef.current;
    dragRef.current = { on: true, x: e.clientX, y: e.clientY, sx: v?.scrollLeft || 0, sy: v?.scrollTop || 0 };
    e.preventDefault();
  }, []);

  useEffect(() => {
    if (!open) return;
    const move = (e) => {
      if (!dragRef.current.on || !viewerRef.current) return;
      viewerRef.current.scrollLeft = dragRef.current.sx - (e.clientX - dragRef.current.x);
      viewerRef.current.scrollTop  = dragRef.current.sy - (e.clientY - dragRef.current.y);
    };
    const up  = () => { dragRef.current.on = false; };
    const key = (e) => { if (e.key === 'Escape') setOpen(false); };
    document.addEventListener('mousemove', move);
    document.addEventListener('mouseup', up);
    document.addEventListener('keydown', key);
    document.body.style.overflow = 'hidden';
    return () => {
      document.removeEventListener('mousemove', move);
      document.removeEventListener('mouseup', up);
      document.removeEventListener('keydown', key);
      document.body.style.overflow = '';
      dragRef.current.on = false;
    };
  }, [open]);

  // Wrapper: explicit px = nat × scale → viewer overflow sees full zoomed size
  // Inner div: transform: scale() renders the SVG at the zoomed size
  const pw = Math.round(nat.w * scale);
  const ph = Math.round(nat.h * scale);

  return (
    <>
      {/* Inline */}
      <div
        ref={inlineRef}
        className={`${MermaidContainerClassName} gu-mermaid-inline`}
        onClick={onOpen}
        role="button" tabIndex={0}
        onKeyDown={(e) => { if (e.key === 'Enter') onOpen(); }}
        title="Click to view fullscreen"
        dangerouslySetInnerHTML={{ __html: renderResult.svg }}
      />

      {/* Modal */}
      {open && (
        <div className="gu-backdrop" onClick={onClose}>
          <div className="gu-toolbar" onClick={(e) => e.stopPropagation()}>
            <button className="gu-tb-btn" onClick={zoomIn}  title="Zoom in">＋</button>
            <button className="gu-tb-btn" onClick={zoomOut} title="Zoom out">−</button>
            <span className="gu-tb-pct">{Math.round(scale * 100)}%</span>
            <button className="gu-tb-btn" onClick={() => setScale(1)}>1:1</button>
            <button className="gu-tb-btn" onClick={onClose} title="Esc">✕</button>
          </div>
          <div ref={viewerRef} className="gu-viewer"
            onClick={(e) => e.stopPropagation()} onMouseDown={onMouseDown}>
            {/* Wrapper: real px size → viewer overflow tracks it */}
            <div className="gu-wrap-outer" style={{ width: pw, height: ph }}>
              {/* Inner: transforms to visually scale while outer handles scroll range */}
              <div className="gu-wrap-inner"
                style={{
                  width: nat.w, height: nat.h,
                  transform: `scale(${scale})`,
                  transformOrigin: '0 0',
                }}>
                <div dangerouslySetInnerHTML={{ __html: renderResult.svg }} />
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function MermaidRenderer({ value }) {
  const result = useMermaidRenderResult({ text: value });
  if (result === null) return null;
  return <MermaidRenderResult renderResult={result} />;
}

export default function Mermaid(props) {
  return (
    <ErrorBoundary fallback={(params) => <ErrorBoundaryErrorMessageFallback {...params} />}>
      <MermaidRenderer {...props} />
    </ErrorBoundary>
  );
}
