import React from 'react';
import "./SignInForm.less"
import { Link } from 'react-router-dom';
import { Form, Icon, Input, Button, Checkbox } from 'antd';
import { FormComponentProps } from "antd/lib/form/Form";

const SignInFormComponent: React.FC<FormComponentProps> = (props) => {

  const handleSubmit = e => {
    e.preventDefault();
    props.form.validateFields((err, values) => {
      if (!err) {
        console.log('Received values of form: ', values);
      }
    });
  };

  const { getFieldDecorator } = props.form;

  return (
    <Form onSubmit={handleSubmit} className="login-form"> 
      <Form.Item>
        {getFieldDecorator('username', {
          rules: [{ required: true, message: 'Please input username!' }],
        })(
          <Input
            prefix={<Icon type="user" style={{ color: 'rgba(0,0,0,.25)' }} />}
            placeholder="Username"
          />
        )}
      </Form.Item>
      <Form.Item>
        {getFieldDecorator('password', {
          rules: [{ required: true, message: 'Please input password!' }],
        })( 
          <Input
            prefix={<Icon type="lock" style={{ color: 'rgba(0,0,0,.25)' }} />}
            type="password"
            placeholder="password"
          />
        )}
      </Form.Item>
      <Form.Item>
        {getFieldDecorator('remember', {
          valuePropName: 'checked',
          initialValue: true,
        })(<Checkbox className='login-form-remember'>Remember me</Checkbox>)}
        <Link className="login-form-forgot" to="/">
          Forgot password
        </Link>
        <Button type="primary" htmlType="submit" className="login-form-button">
          Log in
        </Button>
        Or <Link to="/Register">register now!</Link>
      </Form.Item>
    </Form>
  )
}

export default Form.create({ name: 'normal_login' })(SignInFormComponent);


