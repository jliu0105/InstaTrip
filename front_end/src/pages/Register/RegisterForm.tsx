import React, { useState } from 'react';
import "./RegisterForm.less"
import { Link } from 'react-router-dom';
import { Form, Input, Tooltip, Icon, Checkbox, Button } from 'antd';
import { FormComponentProps } from "antd/lib/form/Form";

const formItemLayout = {
  labelCol: {
    xs: { span: 24 },
    sm: { span: 8 },
  },
  wrapperCol: {
    xs: { span: 24 },
    sm: { span: 16 },
  },
};

const tailFormItemLayout = {
  wrapperCol: {
    xs: {
      span: 24,
      offset: 0,
    },
    sm: {
      span: 16,
      offset: 8,
    },
  },
};

const RegisterFormComponent: React.FC<FormComponentProps> = (props) => {

  const [confirmDirty, setConfirmDirty] = useState(false);
  const { getFieldDecorator } = props.form;

  const handleSubmit = e => {
    e.preventDefault();
    props.form.validateFields((err, values) => {
      if (!err) {
        console.log('Received values of form: ', values);
      }
    });
  };

  const handleConfirmBlur = e => {
    const { value } = e.target;
    setConfirmDirty(confirmDirty || !!value)
  };

  const validateToNextPassword = (rule, value, callback) => {
    const { form } = props;
    if (value && confirmDirty) {
      form.validateFields(['confirm'], { force: true });
    }
    callback();
  };

  const compareToFirstPassword = (rule, value, callback) => {
    if (value && value !== props.form.getFieldValue('password')) {
      callback('Password is not the same!');
    } else {
      callback();
    }
  };

  return (
    <Form {...formItemLayout} onSubmit={handleSubmit} className="register-form">
      {/* 昵称 */}
      <Form.Item
        label={
          <span>
            nickname&nbsp;
            <Tooltip title="What is your name?"><Icon type="question-circle-o" /></Tooltip>
          </span>
        }
      >
        {getFieldDecorator('Nickname', {
          rules: [{ required: true, message: 'Please input your nickname', whitespace: true }],
        })(<Input />)}
      </Form.Item>

      {/* E-mail */}
      <Form.Item label="E-mail">
        {getFieldDecorator('email', {
          rules: [
            {
              type: 'email',
              message: 'please input correct E-mail!',
            },
            {
              required: true,
              message: 'please input your E-mail!',
            },
          ],
        })(<Input />)}
      </Form.Item>

      {/* Password */}
      <Form.Item label="Password" hasFeedback>
        {getFieldDecorator('password', {
          rules: [
            {
              required: true,
              message: 'Please input your password!',
            },
            {
              validator: validateToNextPassword,
            },
          ],
        })(<Input.Password />)}
      </Form.Item>

      {/* Password confirm */}
      <Form.Item label="Password confirm" hasFeedback>
        {getFieldDecorator('confirm', {
          rules: [
            {
              required: true,
              message: 'Please check your password!',
            },
            {
              validator: compareToFirstPassword,
            },
          ],
        })(<Input.Password onBlur={handleConfirmBlur} />)}
      </Form.Item>
        
      {/* Agreement */}
      <Form.Item {...tailFormItemLayout}>
        {getFieldDecorator('agreed', {
          valuePropName: 'checked',
        })(
          <Checkbox> I have read and agreed to <Link to="/">document</Link></Checkbox>,
        )}
      </Form.Item>

      {/* register */}
      <Form.Item {...tailFormItemLayout}>
        <Button type="primary" htmlType="submit">register</Button>
      </Form.Item>
    </Form>
  )
}

export default Form.create({ name: 'normal_login' })(RegisterFormComponent);


