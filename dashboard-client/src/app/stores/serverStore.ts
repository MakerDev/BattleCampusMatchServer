import { action, observable } from "mobx";
import { IGameServer } from "../models/gameServer";
import { RootStore } from "./rootStore";

export default class ServerStore {
  rootStore: RootStore;
  @observable servers: IGameServer[] = [];

  constructor(rootStore: RootStore) {
    this.rootStore = rootStore;
  }

  @action loadServers = () => {
      
  }
}
