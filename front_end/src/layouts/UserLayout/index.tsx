import React from 'react';
import './UserLayout.less';
import logo from '../../assets/logo.svg';
import { Link } from "react-router-dom";
import { Layout, Menu, Dropdown, Icon, Button  } from 'antd';
const { Header, Footer, Content } = Layout;

const menu = (
  <Menu>
    <Menu.Item>
      <a target="_blank" rel="noopener noreferrer" href="http://www.alipay.com/">
        中文
      </a>
    </Menu.Item>
    <Menu.Item>
      <a target="_blank" rel="noopener noreferrer" href="http://www.taobao.com/">
        English
      </a>
    </Menu.Item>
  </Menu>
);

export const UserLayout: React.FC = (props) => {
  return (
    <Layout className='user-layout-container' >
      <Header className='header'>
        <div className='lang'>
          <Dropdown overlay={menu} >
            <Button> Choose language <Icon type="down" /></Button>
          </Dropdown>
        </div>
      </Header>
      <Content className='content'>
        <div className='top'>
          <div className='content-header'>
            <Link to="/">
              <img alt="logo" className='logo' src={logo} /> 
              <span className='title'>InstaTrip</span>
            </Link>
          </div>
          {props.children}
        </div>
      </Content>
    </Layout>
  )
}
