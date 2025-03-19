import Sidebar from '../partials/Sidebar';
import Header from '../partials/Header';
import React, { useState, useEffect, useRef } from 'react';
import { Button, List, ListItem, ListItemText, TextField } from '@mui/material';
import Grid2 from '@mui/material/Grid2';
import Typography from '@mui/material/Typography';

import { useAuth } from "react-oidc-context";
import WebSocketConnections from '../partials/dashboard/WebSocketConnections';
import agent from '../agent';
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';

function HTTP() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const [deviceName, setDeviceName] = useState();
  const [text, setText] = useState();
  const [messages, setMessages] = useState([]);
  const messagesRef = useRef(messages);

  const hubConnectionString = process.env.NODE_ENV === 'development' ? import.meta.env.VITE_API_URL + '/hubs/testhttp' : '/hubs/testhttp';
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

    connection.on('TemperatureChanged', async (message) => {
      if (messagesRef.current.length === 10) {
        messagesRef.current.pop();
      }

      setMessages([message, ...messagesRef.current]);
    });

    setHubConnection(connection);
  }

  useEffect(() => {
    messagesRef.current = messages;
  }, [messages])

  const stopHubConnection = () => {
    hubConnection?.stop();
  }


  useEffect(() => {
    agent.TestHTTP.listTop10().then(result => setMessages(result));
    createHubConnection();

    return () => {
      stopHubConnection();
    }
  }, []);

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
                <h1 className="text-2xl md:text-3xl text-gray-800 dark:text-gray-100 font-bold">HTTP</h1>
              </div>

            </div>

            <div style={{  marginBottom: 20, fontSize: '10pt' }}>
                <code>
                  <p>curl --location 'https://domain.ru/api/testhttp?temperature=10'</p><br/>

                  <p>curl --location 'https://domain.ru/api/testhttp/secured?temperature=12' \<br/>
                  --header 'X-API-Key: 'secret'
                  </p>
                </code>
            </div>


            <div className="col-span-full xl:col-span-8 bg-white dark:bg-gray-800 shadow-xs rounded-xl" style={{ marginBottom: '20px' }}>
              <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60">
                <h2 className="font-semibold text-gray-800 dark:text-gray-100">Входящие данные</h2>
              </header>
              <div className="p-3">
                {/* Table */}
                <div className="overflow-x-auto">

                  <Table aria-label="simple table">
                    <TableHead>
                      <TableRow>
                        <TableCell align='left'>Дата</TableCell>
                        <TableCell align='left'>Устройство</TableCell>
                        <TableCell align='left'>Значение</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {messages.map((message) => (
                        <TableRow
                          key={message.date}
                          sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                        >
                          <TableCell component="th" scope="row" align='left'>
                            {message.date}
                          </TableCell>
                          <TableCell align='left'>{message.deviceName}</TableCell>
                          <TableCell align='left'>{message.value}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </div>
            </div>

          </div>
        </main>
      </div>
    </div>
  );

}

export default HTTP;