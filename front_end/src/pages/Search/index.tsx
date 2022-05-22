import React, { useState, useEffect } from 'react';
import { useParams, useLocation } from 'react-router-dom';
import { Divider, Spin } from 'antd';
import { Filter, ProductList } from '../../components'
import { MainLayout } from '../../layouts'

export const Search: React.FC = () => {

  const { keywords }  = useParams() as any;
  const location = useLocation();
  
  const [loading, setLoading] = useState(true);
  const [productList, setProductList] = useState([]);
  
  useEffect(() => {
    setLoading(true);
    const fetchData = async () => {
      const response = await fetch(`https://localhost:3001/api/touristRoutes/?keyword=${keywords}`);
      const result = await response.text()
      setProductList(JSON. parse(result))
      setLoading(false);
    };

    fetchData();
  }, [location]);

  if(loading) {
    return <Spin />
  }

  return (
    <MainLayout
      breadcrumbItems={['front page', "search", keywords]}
    >
        {/* filter*/}
        <div style={{padding: 20, marginTop: 20, background: 'white'}}>
          <Filter title='Departure city' tags={["Beijing", "Shanghai", "Guangzhou", "Shenzhen"]}/>
          <Divider dashed />
          <Filter title='Number of days' tags={["2days", "3days", "4days", "5days", "6days"]}/>
          <Divider dashed />
          <Filter title='Trip type' tags={["Group tour", "Backpacking", "Driving", "Customize"]}/>
          <Divider dashed />
          <Filter title='Departure time' tags={["Christmas", "Easter", "Summer"]}/>
        </div>

        <div style={{padding: 20, marginTop: 20, background: 'white'}}>
          <ProductList data={productList}/>
        </div>
    </MainLayout>
  )
}
