import React from 'react';
import logo from '../../assets/logo.svg';
import './Header.less'
import { useHistory, Link } from 'react-router-dom';
import { Layout, Menu, Typography, Input, Button, Dropdown, Icon } from 'antd';
const { Title, Text } = Typography;
const { Search } = Input;
const { Group } = Button;

const menu = (
  <Menu>
    <Menu.Item>中文</Menu.Item>
    <Menu.Item>英语</Menu.Item>
  </Menu>
);

export const Header: React.FC = (props) => {

  const history = useHistory();

  const onSearch = (keywords) => {
    history.push('/search/'+keywords);
  }

  return (
    <div className='header'>
      <div className="top-header" >
        <div style={{float: 'right'}}>
          <Text>让旅行更幸福</Text>
          <Dropdown.Button
            style={{marginLeft: 15}}
            overlay={menu} 
            icon={<Icon type="global" />}
          >
            Language
          </Dropdown.Button>
        </div>
      </div>
      <Layout.Header className="main-header">
        <Link to="/"><img src={logo} className="logo" alt="logo" /></Link>
        <Link to="/"><Title level={3} style={{lineHeight: '64px', display: "inline"}}>慕课旅游网</Title></Link>
        <Menu
          mode="horizontal"
          style={{ lineHeight: '64px', float:"right", marginLeft: 15 }}
        >
          <Group>
            <Button><Link to="/Register">Register</Link></Button>
            <Button><Link to="/SignIn">Sign in</Link></Button>
          </Group>
        </Menu>
        <Search
          placeholder="Destination"
          onSearch={(keywords)=>onSearch(keywords)}
          style={{ width: 400, lineHeight: '64px', float:"right" }}
        />
      </Layout.Header>
      <div className='menu-header'>
        <Menu
          mode="horizontal"
          style={{ paddingLeft: '4%', paddingRight: '4%' }}
        >
          <Menu.Item key="1"> Travel front page </Menu.Item>
          <Menu.Item key="2"> 1-3 days trip</Menu.Item>
          <Menu.Item key="3"> Group tour </Menu.Item>
          <Menu.Item key="4"> Backpacking trip </Menu.Item>
          <Menu.Item key="5"> Family trip </Menu.Item>
          <Menu.Item key="6"> Ferry </Menu.Item>
          <Menu.Item key="7"> Hotel </Menu.Item>
          <Menu.Item key="8"> Local Scenic spot</Menu.Item>
          <Menu.Item key="9"> Travel theme </Menu.Item>
          <Menu.Item key="10"> Customize trip </Menu.Item>
          <Menu.Item key="11"> Exchange program </Menu.Item>
          <Menu.Item key="12"> Visa </Menu.Item>
          <Menu.Item key="13"> Company trip </Menu.Item>
          <Menu.Item key="14"> High end trip </Menu.Item>
          <Menu.Item key="15"> Outdoor </Menu.Item>
          <Menu.Item key="16"> Insurance </Menu.Item>
        </Menu>
      </div>
    </div>  
  )
}
