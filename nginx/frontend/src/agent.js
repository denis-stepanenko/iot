import axios from "axios";
import { User } from "oidc-client-ts"

function getUser() {
    const oidcStorage = localStorage.getItem(`oidc.user:${import.meta.env.VITE_AUTHORITY}:${import.meta.env.VITE_CLIENT_ID}`)
    if (!oidcStorage) {
        return null;
    }

    return User.fromStorageString(oidcStorage);
}

axios.defaults.baseURL = process.env.NODE_ENV === 'development' ? import.meta.env.VITE_API_URL + '/api' : '/api';

axios.interceptors.request.use(config => {
    const user = getUser();
    const token = user?.access_token;

    if (token)
        config.headers.Authorization = `Bearer ${token}`;

    return config;
})

const responseBody = (response) => response.data;

const requests = {
    get: (url) => axios.get(url).then(responseBody),
    post: (url, body) => axios.post(url, body).then(responseBody),
    put: (url, body) => axios.put(url, body).then(responseBody),
    del: (url) => axios.delete(url).then(responseBody),
}

const Components = {
    list: () => requests.get(`/component`),
    add: (item) => requests.post('/component', item),
    delete: (topic) => requests.del(`/component?id=${topic}`),
}

const RetainedMessages = {
    list: () => requests.get(`/retainedmessage`),
}

const Clients = {
    list: () => requests.get(`/client`),
}

const TestWS = {
    send: (item) => requests.post('/TestWS/SendCommandToClient', item),
    listDevices: () => requests.get(`/TestWS/GetDevices`)
}

const TestHTTP = {
    listTop10: () => requests.get(`/TestHTTP/GetTop10`),
}

const agent = {
    Components,
    RetainedMessages,
    Clients,
    TestWS,
    TestHTTP
}

export default agent;