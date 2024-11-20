import React from 'react';
// 导入原始的 Layout 组件
import OriginalLayout from '@theme-original/Layout';

export default function Layout(props) {
  return (
    <>
      <OriginalLayout {...props} />
      <script
        async
        src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-3450324887026774"
        crossorigin="anonymous"
      ></script>
    </>
  );
}