import { action, configure, observable, runInAction } from "mobx";
import agent from "../api/agent";
import { IAdminUser, IAdminUserForm } from "../models/user";
import { RootStore } from "./rootStore";
import { history } from "../..";

configure({ enforceActions: "always" });

export default class UserStore {
  rootStore: RootStore;

  @observable isLoggedIn: boolean = false;
  @observable adminUser: IAdminUser | null = null;

  constructor(rootStore: RootStore) {
    this.rootStore = rootStore;
  }

  @action login = async (userForm: IAdminUserForm) => {
    try {
      const adminUser = await agent.User.login(userForm);
      runInAction(() => {
        this.adminUser = adminUser;
      });

      this.rootStore.commonStore.setToken(adminUser.token);
      history.push("/dashboard");
    } catch (error) {
      throw error;
    }
  };

  @action logout = () => {
    this.rootStore.commonStore.setToken(null);
    this.adminUser = null;
    history.push("/");
  };
}
