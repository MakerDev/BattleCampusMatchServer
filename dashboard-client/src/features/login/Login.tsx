import { observer } from "mobx-react-lite";
import React, { useContext, useState } from "react";
import { Button, Container, Form, Header } from "semantic-ui-react";
import { RootStoreContext } from "../../app/stores/rootStore";
import { IAdminUserForm } from "../../app/models/user";
import { FormEvent } from "react";

const Login = () => {
  const [userForm, setUserForm] = useState<IAdminUserForm>({
    id: "",
    password: "",
  });

  const rootStore = useContext(RootStoreContext);

  const handleSubmit = () => {
    rootStore.userStore.login(userForm);
  };

  const handleInputChange = (
    event: FormEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = event.currentTarget;
    setUserForm({ ...userForm, [name]: value });
  };

  return (
    <Container style={{ marginTop: "5rem" }}>
      <Header as="h2">Login as admin</Header>

      <Form onSubmit={handleSubmit}>
        <Form.Input
          onChange={handleInputChange}
          name="id"
          placeholder="id"
          value={userForm.id}
        />
        <Form.Input
          onChange={handleInputChange}
          name="password"
          placeholder="password"
          type="password"
          value={userForm.password}
        />
        <Button type="submit">Submit</Button>
      </Form>
    </Container>
  );
};

export default observer(Login);
