import Sidebar from '../partials/Sidebar';
import Header from '../partials/Header';
import React, { useState, useEffect, useRef } from 'react';
import { Button, TextField } from '@mui/material';
import Grid2 from '@mui/material/Grid2';
import Typography from '@mui/material/Typography';

import { useAuth } from "react-oidc-context";
import WebSocketConnections from '../partials/dashboard/WebSocketConnections';
import agent from '../agent';
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

function WebSocket() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const [deviceName, setDeviceName] = useState();
  const [text, setText] = useState();
  const [devices, setDevices] = useState([]);
  const devicesRef = useRef(devices)


  const hubConnectionString = process.env.NODE_ENV === 'development' ? import.meta.env.VITE_API_URL + '/hubs/testws' : '/hubs/testws';
  const [hubConnection, setHubConnection] = useState();
  const auth = useAuth();

  const createHubConnection = async () => {

    let connection = new HubConnectionBuilder()
      .withUrl(hubConnectionString, {
        accessTokenFactory:
          () => auth.user.access_token
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.None)
      .build();

    await connection.start();

    connection.on('Connected', async (device) => {
      setDevices([device, ...devicesRef.current]);
    });

    connection.on('Disconnected', async (device) => {
      setDevices(devices.filter((item) => item.name !== device.name));
    });

    setHubConnection(connection);
  }

  const stopHubConnection = () => {
    hubConnection?.stop();
  }


  useEffect(() => {
    agent.TestWS.listDevices().then(result => setDevices(result));
    createHubConnection();

    return () => {
      stopHubConnection();
    }
  }, []);

  useEffect(() => {
    devicesRef.current = devices;
  }, [devices])

  return (
    <div className="flex h-screen overflow-hidden">

      {/* Sidebar */}
      <Sidebar sidebarOpen={sidebarOpen} setSidebarOpen={setSidebarOpen} />

      {/* Content area */}
      <div className="relative flex flex-col flex-1 overflow-y-auto overflow-x-hidden">

        {/*  Site header */}
        <Header sidebarOpen={sidebarOpen} setSidebarOpen={setSidebarOpen} />

        <main className="grow">
          <div className="px-4 sm:px-6 lg:px-8 py-8 w-full max-w-9xl mx-auto">

            {/* Dashboard actions */}
            <div className="sm:flex sm:justify-between sm:items-center mb-8">

              {/* Left: Title */}
              <div className="mb-4 sm:mb-0">
                <h1 className="text-2xl md:text-3xl text-gray-800 dark:text-gray-100 font-bold">WebSocket (RFC 6455)</h1>
              </div>

            </div>

            <div style={{ marginBottom: 20, fontSize: '10pt' }}>
              <code>
                <p>wss://domain.ru/api/testws?deviceName=test</p><br />
              </code>
            </div>

            <div className="col-span-full xl:col-span-8 bg-white dark:bg-gray-800 shadow-xs rounded-xl" style={{ marginBottom: '20px' }}>
              <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60">
                <h2 className="font-semibold text-gray-800 dark:text-gray-100">Отправить команду</h2>
              </header>
              <div className="p-3">
                {/* Table */}
                <div className="overflow-x-auto">

                  <div style={{ width: '80%', padding: 10 }}>

                    <TextField label="Устройство" variant="outlined" onChange={e => setDeviceName(e.target.value)} />
                    <TextField label="Команда" variant="outlined" onChange={e => setText(e.target.value)} />
                    <Button variant="contained" style={{ height: '55px' }} onClick={() => agent.TestWS.send({ deviceName: deviceName, text: text })}>Отправить</Button>
                  </div>

                </div>
              </div>
            </div>



            <WebSocketConnections items={devices} />

          </div>
        </main>
      </div>
    </div>
  );

}

export default WebSocket;