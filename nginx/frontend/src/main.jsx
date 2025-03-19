import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter as Router } from 'react-router-dom';
import ThemeProvider from './utils/ThemeContext';
import App from './App';
import Spinner from './components/Spinner'

import { AuthProvider } from "react-oidc-context";
import { WebStorageStateStore } from 'oidc-client-ts';
import OidcWrapper from './OidcWrapper';

const oidcConfig = {
  authority: import.meta.env.VITE_AUTHORITY,
  client_id: import.meta.env.VITE_CLIENT_ID,
  redirect_uri: import.meta.env.VITE_CLIENT_URL,
  response_type: 'code',
  scope: 'openid profile iotapi color offline_access',
  userStore: new WebStorageStateStore({ store: window.localStorage }), // где хранить токены

  loadUserInfo: true, // Автоматически получает информацию из userendpoint и ее можно получить по auth.user?.profile
  automaticSilentRenew: true, // Продливать ли токен автоматически

  onSigninCallback() {
    window.history.replaceState(
      {},
      document.title,
      window.location.pathname
    )
  },
}

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <AuthProvider {...oidcConfig}>
      <OidcWrapper spinner={<Spinner/>}>
        <Router>
          <ThemeProvider>
            <App/>
          </ThemeProvider>
        </Router>
      </OidcWrapper>
    </AuthProvider>
  </React.StrictMode>
);
