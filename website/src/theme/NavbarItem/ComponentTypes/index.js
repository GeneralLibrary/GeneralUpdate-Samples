/**
 * Raw swizzle of @theme/NavbarItem/ComponentTypes
 * （基于 Docusaurus 3.1.1 官方文件，注册自定义导航项类型）
 *
 * 新增类型：
 * - productsVersions: "文档"分组下拉（见 ProductsVersionsNavbarItem），
 *   一个导航项容纳全部产品及其已发布版本号，避免横向溢出。
 */
import DefaultNavbarItem from '@theme/NavbarItem/DefaultNavbarItem';
import DropdownNavbarItem from '@theme/NavbarItem/DropdownNavbarItem';
import LocaleDropdownNavbarItem from '@theme/NavbarItem/LocaleDropdownNavbarItem';
import SearchNavbarItem from '@theme/NavbarItem/SearchNavbarItem';
import HtmlNavbarItem from '@theme/NavbarItem/HtmlNavbarItem';
import DocNavbarItem from '@theme/NavbarItem/DocNavbarItem';
import DocSidebarNavbarItem from '@theme/NavbarItem/DocSidebarNavbarItem';
import DocsVersionNavbarItem from '@theme/NavbarItem/DocsVersionNavbarItem';
import DocsVersionDropdownNavbarItem from '@theme/NavbarItem/DocsVersionDropdownNavbarItem';
import ProductsVersionsNavbarItem from '@theme/NavbarItem/ProductsVersionsNavbarItem';

const ComponentTypes = {
  default: DefaultNavbarItem,
  localeDropdown: LocaleDropdownNavbarItem,
  search: SearchNavbarItem,
  dropdown: DropdownNavbarItem,
  html: HtmlNavbarItem,
  doc: DocNavbarItem,
  docSidebar: DocSidebarNavbarItem,
  docsVersion: DocsVersionNavbarItem,
  docsVersionDropdown: DocsVersionDropdownNavbarItem,
  // 类型名须匹配官方 schema 的 /custom-.*/ 白名单（见 theme-classic options.js CustomNavbarItemSchema）
  'custom-productsVersions': ProductsVersionsNavbarItem,
};

export default ComponentTypes;
