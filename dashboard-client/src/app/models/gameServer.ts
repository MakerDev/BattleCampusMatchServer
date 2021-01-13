import { IIpPortInfo } from "./ipPortInfo";

export enum ServerState
{
    RUNNING,
    OFF,
}

export interface IGameServer
{
    id: string,
    name: string,
    maxMatches: number,
    ipPortInfo: IIpPortInfo
    state: ServerState
}