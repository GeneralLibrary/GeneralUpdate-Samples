/**
 * 自定义导航栏类型 productsVersions（经 src/theme/NavbarItem/ComponentTypes 注册）
 *
 * "文档"分组下拉：groups 里每个产品一个小节——产品名做分组标题，
 * 已发布版本号做菜单项。悬停/点开菜单后才显示版本号，未发布的
 * current（"Next"）版本始终隐藏。
 *
 * 版本列表随各实例的 docs:version 自动更新，发布新版无需改导航配置。
 * 切换版本时优先跳转到当前页面在目标版本下的对应文档（保留 ?search#hash），
 * 做不到时回退到该版本的主文档。
 */
import React, {useState, useRef, useEffect} from 'react';
import clsx from 'clsx';
import {useCollapsible, Collapsible} from '@docusaurus/theme-common';
import {useLocalPathname} from '@docusaurus/theme-common/internal';
import {useLocation} from '@docusaurus/router';
import {
  useVersions,
  useActiveDocContext,
} from '@docusaurus/plugin-content-docs/client';
import {useDocsPreferredVersion} from '@docusaurus/theme-common';
import NavbarNavLink from '@theme/NavbarItem/NavbarNavLink';
import NavbarItem from '@theme/NavbarItem';

const getVersionMainDoc = (version) =>
  version.docs.find((doc) => doc.id === version.mainDocId);

/** 一个产品小节：产品名标题（html 项）+ 已发布版本号链接列表 */
function GroupSection({group, hideCurrentVersion = true, mobile, search, hash, onClose}) {
  const versions = useVersions(group.docsPluginId);
  const activeDocContext = useActiveDocContext(group.docsPluginId);
  const {savePreferredVersionName} = useDocsPreferredVersion(group.docsPluginId);
  const visibleVersions = hideCurrentVersion
    ? versions.filter((version) => version.name !== 'current')
    : versions;
  return (
    <>
      <NavbarItem
        isDropdownItem
        mobile={mobile}
        type="html"
        value={`<span class="navbar__menu-group">${group.label}</span>`}
      />
      {visibleVersions.map((version) => {
        // 尽量跳转到当前页面在该版本下的对应文档；做不到时回退到版本主文档
        const versionDoc =
          activeDocContext.alternateDocVersions[version.name] ??
          getVersionMainDoc(version);
        return (
          <NavbarItem
            key={version.name}
            mobile={mobile}
            isDropdownItem
            activeClassName="dropdown__link--active"
            label={version.label}
            to={`${versionDoc.path}${search}${hash}`}
            onClick={() => {
              savePreferredVersionName(version.name);
              onClose?.();
            }}
          />
        );
      })}
    </>
  );
}

/** 桌面端：悬停/点开展开的 navbar 下拉（结构对齐官方 DropdownNavbarItem） */
function DesktopLayout({label, position, className, groups, hideCurrentVersion, ...props}) {
  const {search, hash} = useLocation();
  const dropdownRef = useRef(null);
  const [showDropdown, setShowDropdown] = useState(false);
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (!dropdownRef.current || dropdownRef.current.contains(event.target)) {
        return;
      }
      setShowDropdown(false);
    };
    document.addEventListener('mousedown', handleClickOutside);
    document.addEventListener('touchstart', handleClickOutside);
    document.addEventListener('focusin', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
      document.removeEventListener('touchstart', handleClickOutside);
      document.removeEventListener('focusin', handleClickOutside);
    };
  }, [dropdownRef]);
  return (
    <div
      ref={dropdownRef}
      className={clsx('navbar__item', 'dropdown', 'dropdown--hoverable', {
        'dropdown--right': position === 'right',
        'dropdown--show': showDropdown,
      })}>
      <NavbarNavLink
        aria-haspopup="true"
        aria-expanded={showDropdown}
        role="button"
        href={props.to ? undefined : '#'}
        className={clsx('navbar__link', className)}
        {...props}
        label={label}
        onClick={props.to ? undefined : (e) => e.preventDefault()}
        onKeyDown={(e) => {
          if (e.key === 'Enter') {
            e.preventDefault();
            setShowDropdown(!showDropdown);
          }
        }}
      />
      <ul className="dropdown__menu">
        {groups.map((group) => (
          <GroupSection
            key={group.docsPluginId}
            group={group}
            hideCurrentVersion={hideCurrentVersion}
            search={search}
            hash={hash}
            onClose={() => setShowDropdown(false)}
          />
        ))}
      </ul>
    </div>
  );
}

/** 移动端：抽屉里的可折叠子菜单（结构对齐官方 DropdownNavbarItemMobile） */
function MobileLayout({label, className, groups, hideCurrentVersion, ...props}) {
  const {search, hash} = useLocation();
  const localPathname = useLocalPathname();
  // 当前页面属于某个产品的文档时，自动展开该下拉
  const containsActive = groups.some(
    (group) => group.basePath && localPathname.startsWith(group.basePath),
  );
  const {collapsed, toggleCollapsed, setCollapsed} = useCollapsible({
    initialState: () => !containsActive,
  });
  useEffect(() => {
    if (containsActive) {
      setCollapsed(!containsActive);
    }
  }, [localPathname, containsActive, setCollapsed]);
  return (
    <li
      className={clsx('menu__list-item', {
        'menu__list-item--collapsed': collapsed,
      })}>
      <NavbarNavLink
        role="button"
        className={clsx(
          'menu__link menu__link--sublist menu__link--sublist-caret',
          className,
        )}
        {...props}
        label={label}
        onClick={(e) => {
          e.preventDefault();
          toggleCollapsed();
        }}
      />
      <Collapsible lazy as="ul" className="menu__list" collapsed={collapsed}>
        {groups.map((group) => (
          <GroupSection
            key={group.docsPluginId}
            group={group}
            hideCurrentVersion={hideCurrentVersion}
            mobile
            search={search}
            hash={hash}
          />
        ))}
      </Collapsible>
    </li>
  );
}

export default function ProductsVersionsNavbarItem({mobile = false, ...props}) {
  const Comp = mobile ? MobileLayout : DesktopLayout;
  return <Comp {...props} />;
}
