import { observer } from "mobx-react-lite";

const ServerDashboard = () => {
  return (
    <header className="App-header">
      <img src={"/assets/logo.svg"} className="App-logo" alt="logo" />
      <p>
        Edit <code>src/App.tsx</code> and save to reload.
      </p>
      <a
        className="App-link"
        href="https://reactjs.org"
        target="_blank"
        rel="noopener noreferrer"
      >
        Learn React
      </a>
    </header>
  );
};

export default observer(ServerDashboard);
