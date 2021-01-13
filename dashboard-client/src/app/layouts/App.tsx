import React, { Fragment } from "react";
import "./App.css";
import {
  BrowserRouter as Router,
  Switch,
  Route,
  withRouter,
  RouteComponentProps,
} from "react-router-dom";
import DashboardLayout from "../../features/dashboard/DashboardLayout";
import Login from "../../features/login/Login";
import { observer } from "mobx-react-lite";

const App: React.FC<RouteComponentProps> = ({ location }) => {
  return (
    <Fragment>
      <Route exact path="/">
        <Login />
      </Route>
      <Switch>
        <Route path="/dashboard">
          <DashboardLayout />
        </Route>
      </Switch>
    </Fragment>
  );
};

export default withRouter(observer(App));
