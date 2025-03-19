import Sidebar from '../partials/Sidebar';
import Header from '../partials/Header';
import React, { useState } from 'react';

import { useAuth } from "react-oidc-context";

function UserProfile() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const auth = useAuth();

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
                <h1 className="text-2xl md:text-3xl text-gray-800 dark:text-gray-100 font-bold">Настройки</h1>
              </div>

            </div>

            <div style={{wordWrap: 'break-word', width: '80%'}}>
              <p><b>Access token: </b>{auth.user.access_token}</p><br/>
              <p><b>Refresh token: </b>{auth.user.refresh_token}</p><br/>
              <p><b>Expires At: </b>{new Date(auth.user.expires_at * 1000).toLocaleString()}</p><br/>
            </div>

          </div>
        </main>
      </div>
    </div>
  );

}

export default UserProfile;