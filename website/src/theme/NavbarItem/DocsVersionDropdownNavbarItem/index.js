/**
 * Raw swizzle of @theme/NavbarItem/DocsVersionDropdownNavbarItem
 * （基于 Docusaurus 3.1.1 源码修改）
 *
 * 与官方组件的差异：
 * 1. 支持 `label` 配置项：下拉按钮显示产品名（如 "GeneralUpdate"），
 *    版本号只出现在展开的菜单里，不占用导航栏空间。
 * 2. 支持 `hideCurrentVersion` 配置项：菜单中隐藏未发布的 current
 *    （"Next"）版本，只列出已通过 docs:version 冻结的版本。
 * 3. 提供自定义 label 时，即使只有一个版本也渲染为下拉菜单。
 */
import React from 'react';
import {
  useVersions,
  useActiveDocContext,
} from '@docusaurus/plugin-content-docs/client';
import {useDocsPreferredVersion} from '@docusaurus/theme-common';
import {useDocsVersionCandidates} from '@docusaurus/theme-common/internal';
import {translate} from '@docusaurus/Translate';
import {useLocation} from '@docusaurus/router';
import DefaultNavbarItem from '@theme/NavbarItem/DefaultNavbarItem';
import DropdownNavbarItem from '@theme/NavbarItem/DropdownNavbarItem';
const getVersionMainDoc = (version) =>
  version.docs.find((doc) => doc.id === version.mainDocId);
export default function DocsVersionDropdownNavbarItem({
  mobile,
  docsPluginId,
  dropdownActiveClassDisabled,
  dropdownItemsBefore,
  dropdownItemsAfter,
  hideCurrentVersion,
  ...props
}) {
  const {search, hash} = useLocation();
  const activeDocContext = useActiveDocContext(docsPluginId);
  const versions = useVersions(docsPluginId);
  const {savePreferredVersionName} = useDocsPreferredVersion(docsPluginId);
  const visibleVersions = hideCurrentVersion
    ? versions.filter((version) => version.name !== 'current')
    : versions;
  const versionLinks = visibleVersions.map((version) => {
    // 尽量跳转到同一篇文档的其他版本；做不到时回退到该版本的主文档
    const versionDoc =
      activeDocContext.alternateDocVersions[version.name] ??
      getVersionMainDoc(version);
    return {
      label: version.label,
      // 版本切换时保留 ?search#hash
      to: `${versionDoc.path}${search}${hash}`,
      isActive: () => version === activeDocContext.activeVersion,
      onClick: () => savePreferredVersionName(version.name),
    };
  });
  const items = [
    ...dropdownItemsBefore,
    ...versionLinks,
    ...dropdownItemsAfter,
  ];
  const dropdownVersion = useDocsVersionCandidates(docsPluginId)[0];
  // 自定义 label（产品名）优先；否则沿用官方行为：
  // 移动端多版本显示 "Versions"，桌面端显示当前版本号
  const dropdownLabel =
    props.label ??
    (mobile && items.length > 1
      ? translate({
          id: 'theme.navbar.mobileVersionsDropdown.label',
          message: 'Versions',
          description:
            'The label for the navbar versions dropdown on mobile view',
        })
      : dropdownVersion.label);
  const dropdownTo =
    mobile && items.length > 1 && !props.label
      ? undefined
      : getVersionMainDoc(dropdownVersion).path;
  // 官方行为：仅一个版本且未自定义 label 时渲染为普通按钮
  if (items.length <= 1 && !props.label) {
    return (
      <DefaultNavbarItem
        {...props}
        mobile={mobile}
        label={dropdownLabel}
        to={dropdownTo}
        isActive={dropdownActiveClassDisabled ? () => false : undefined}
      />
    );
  }
  return (
    <DropdownNavbarItem
      {...props}
      mobile={mobile}
      label={dropdownLabel}
      to={dropdownTo}
      items={items}
      isActive={dropdownActiveClassDisabled ? () => false : undefined}
    />
  );
}
