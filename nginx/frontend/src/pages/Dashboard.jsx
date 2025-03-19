import { useEffect, useState, useRef } from 'react'
import Sidebar from '../partials/Sidebar';
import Header from '../partials/Header';
import Devices from '../partials/dashboard/Devices'
import Indicator from '../partials/dashboard/Indicator';
import Toggle from '../partials/dashboard/Toggle';
import Range from '../partials/dashboard/Range';
import agent from '../agent';
import NewIndicator from '../partials/dashboard/NewIndicator';
import NewToggleDialog from '../partials/dashboard/NewToggleDialog';
import NewRangeDialog from '../partials/dashboard/NewRangeDialog';
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useAuth } from 'react-oidc-context';
import mqtt from 'mqtt'
import CircularProgress from '@mui/material/CircularProgress';
import Fade from '@mui/material/Fade';

function Dashboard() {

  const [sidebarOpen, setSidebarOpen] = useState(false);


  const [loading, setLoading] = useState(true);
  const [reconnectig, setReconnectig] = useState(false)
  const [connectionError, setConnectionError] = useState('')
  const host = import.meta.env.VITE_MQTT_URL
  const clientId = 'mqttjs_' + Math.random().toString(16).substr(2, 8)
  const [client, setClient] = useState(null);
  const clientRef = useRef(client)

  const options = {
    keepalive: 60,
    clientId: clientId,
    username: 'firstpart',
    password: 'secondpart',
    protocolId: 'MQTT',
    protocolVersion: 4,
    clean: true,
    reconnectPeriod: 1000,
    connectTimeout: 30 * 1000,
    will: {
      topic: 'WillMsg',
      payload: 'Connection Closed abnormally..!',
      qos: 0,
      retain: false
    },
  }

  const readingHubConnectionString = process.env.NODE_ENV === 'development' ? import.meta.env.VITE_API_URL + '/hubs/readings' : '/hubs/readings';
  const [readingHubConnection, setReadingHubConnection] = useState();

  const startReadingHubConnection = async () => {

    let connection = new HubConnectionBuilder()
      .withUrl(readingHubConnectionString, {
        accessTokenFactory:
          () => auth.user.access_token
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.None)
      .build();

    await connection.start();

    connection.on('ReadingsChanged', async (topic, reading) => {
      let indicator = componentsRef.current.filter(component => component.$type === 'indicator' && component.topic === topic)[0];
      addReading(reading, indicator.readings);
    });

    setReadingHubConnection(connection);
  }

  const stopReadingHubConnection = () => {
    readingHubConnection?.stop();
  }

  const addReading = (value, data) => {
    if(data.length === 10) {
      data.shift();
    }
    
    data.push(value);
  }



  const hubConnectionString = process.env.NODE_ENV === 'development' ? import.meta.env.VITE_API_URL + '/hubs/clients' : '/hubs/clients';
  const auth = useAuth();
  const [hubConnection, setHubConnection] = useState();
  const [devices, setDevices] = useState([])

  const startHubConnection = async () => {

    let connection = new HubConnectionBuilder()
      .withUrl(hubConnectionString, {
        accessTokenFactory:
          () => auth.user.access_token
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.None)
      .build();

    await connection.start();

    connection.on('ClientsChanged', async (devices) => {

      setDevices(devices.filter(x => !x.id.includes('mqttjs_')));
    });

    connection.on('GetAll', async (devices) => {
      setDevices(devices.filter(x => !x.id.includes('mqttjs_')));
    });

    await connection?.invoke('GetAll');

    setHubConnection(connection);
  }

  const stopHubConnection = () => {
    hubConnection?.stop();
  }


  const [components, setComponents] = useState([])
  const componentsRef = useRef(components)

  async function getComponents() {

    const result = await agent.Components.list();

    result.forEach(e => {
      e.subscribed = false;

      if (e.$type === 'toggle')
        e.value = false;

      if (e.$type === 'range')
        e.value = 0;

      if (e.$type === 'indicator')
        e.value = 0;
    });

    const messages = await agent.RetainedMessages.list();

    var retainedMessages = messages.map(function (o) {
      return { topic: o.topic, payload: atob(o.payload) }
    });

    result.forEach(e => {
      let message = retainedMessages.filter(x => x.topic === e.topic)[0];

      if (message)
        e.value = convertForUI(e.$type, message.payload)
    });

    return result;
  }

  const addComponent = async (item) => {

    if (components.some(x => x.topic === item.topic || x.title === item.title))
      return;

    await agent.Components.add(item);

    setComponents([...components, item]);
  }

  const removeComponent = async (topic) => {

    await agent.Components.delete(topic);

    setComponents(components.filter(x => x.topic != topic));

    client.publish(topic, '', { qos: 2, retain: true }, (err, p) => {
      if (!err) {
        client.unsubscribe(topic);
        setComponents(components.filter(x => x.topic != topic));
      }
    })
  }

  const convertForBroker = (type, value) => {
    switch (type) {
      case 'toggle':
        return value ? '1' : '0';
      case 'range':
        return String(value);
      case 'indicator':
        return String(value);
    }
  }

  const convertForUI = (type, message) => {
    switch (type) {
      case 'toggle':
        return message == '1';
      case 'range':
        return Number(message);
      case 'indicator':
        return Number(message);
    }
  }

  function getComponentValue(topic) {
    return components.filter(x => x.topic === topic)[0].value;
  }

  function setComponentValue(topic, value) {
    setComponents(prevSquares => prevSquares.map((square) =>
      square.topic === topic
        ? { ...square, value: value, initialized: true }
        : square
    ));
  }

  const handleRangeChange = (topic, newValue) => {
    setComponentValue(topic, newValue);
  };

  const handleRangeChangeCommitted = (name, newValue) => {
    client.publish(name, convertForBroker('range', newValue), { qos: 2, retain: true })
  }

  const handleToggleChange = (name, value) => {

    setComponentValue(name, value);

    client.publish(name, convertForBroker('toggle', value), { qos: 2, retain: true })
  };

  useEffect(() => {
    getComponents().then(result => setComponents(result));

    startHubConnection();

    startReadingHubConnection();

    const mqttClient = mqtt.connect(host, options);
    setClient(mqttClient);
    clientRef.current = mqttClient;

    return () => {
      stopHubConnection();
      stopReadingHubConnection();
      clientRef.current.end();
    }
  }, [])

  useEffect(() => {
    if (client) {
      client.on('connect', () => {
        setLoading(false);
        setReconnectig(false)
      })

      client.on('reconnect', () => {
        setLoading(false)
        setReconnectig(true)
      })

      client.on('error', (err) => {
        console.log(err)
        setLoading(false);
        setConnectionError(err)
        client.end()
      })

      client.on('message', (topic, message) => {
        let item = componentsRef.current.filter(x => x.topic === topic)[0];

        setComponentValue(topic, convertForUI(item.$type, message));
      })
    }
  }, [client]);

  useEffect(() => {
    // Subscribe
    if (client)
      components.filter(x => !x.subscribed).forEach(x => {

        setComponents(prevComponents => prevComponents.map((component) =>
          component.topic === x.topic
            ? { ...component, subscribed: true }
            : component
        ));

        componentsRef.current = components;
        client.subscribe(x.topic, { qos: 2 })
      })
  }, [components])

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
                <h1 className="text-2xl md:text-3xl text-gray-800 dark:text-gray-100 font-bold">MQTT</h1>
              </div>



            </div>

            {loading && <div style={{width: '100%', textAlign: 'center', marginBottom: '40px'}}><Fade in={true} style={{ transitionDelay: '800ms'}} unmountOnExit><CircularProgress /></Fade></div>}
            {reconnectig && <div style={{width: '100%', textAlign: 'center', marginBottom: '40px'}}><Fade in={true} style={{ transitionDelay: '800ms'}} unmountOnExit><CircularProgress /></Fade></div>}
            {connectionError && <div>Ошибка подключения: {connectionError.toString()}</div>}

            {/* Cards */}
            <div className="grid grid-cols-12 gap-6">

            {!loading && !reconnectig && !connectionError && <>
              {/* Индикаторы */}
              <div className="flex flex-col col-span-full">
                <NewIndicator items={components} onAdd={addComponent} />
              </div>

              {
                components.filter(component => component.$type === 'indicator')
                  .map(component => {
                    return (
                      <Indicator key={'component' + component.topic}
                        title={component.title}
                        topic={component.topic}
                        data={component.readings}
                        onRemove={_ => removeComponent(component.topic)} />
                    )
                  })
              }

              {/* Кнопки */}
              <div className="flex flex-col col-span-full">
                <NewToggleDialog items={components} onAdd={addComponent} />
                <NewRangeDialog items={components} onAdd={addComponent} />
              </div>

              {
                components.map(component => {
                  if (component.$type === 'toggle') {
                    return (
                      <Toggle key={'component' + component.topic}
                        title={component.title}
                        topic={component.topic}
                        checked={getComponentValue(component.topic)}
                        onChange={(_, value) => handleToggleChange(component.topic, value)}
                        onRemove={_ => removeComponent(component.topic)} />
                    );
                  }

                  if (component.$type === 'range') {
                    return (
                      <Range key={'component' + component.topic}
                        title={component.title}
                        topic={component.topic}
                        step={component.step}
                        min={component.min}
                        max={component.max}
                        value={getComponentValue(component.topic)}
                        onRemove={_ => removeComponent(component.topic)}
                        onChange={(_, value) => handleRangeChange(component.topic, value)}
                        onChangeCommitted={(_, value) => handleRangeChangeCommitted(component.topic, value)} />
                    );
                  }
                })
              }
              </>}

              <Devices items={devices} />
            </div>

          </div>
        </main>

      </div>
    </div>
  );
}

export default Dashboard;