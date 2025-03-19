import React from 'react';
import ListItemAvatar from '@mui/material/ListItemAvatar';
import Avatar from '@mui/material/Avatar';
import DevicesIcon from '@mui/icons-material/Devices';

function Devices({ items }) {
  return (
    <div className="col-span-full xl:col-span-8 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Устройства</h2>
      </header>
      <div className="p-3">
        {/* Table */}
        <div className="overflow-x-auto">
          <table className="table-auto w-full dark:text-gray-300">
            {/* Table header */}
            <thead className="text-xs uppercase text-gray-400 dark:text-gray-500 bg-gray-50 dark:bg-gray-700 dark:bg-opacity-50 rounded-xs">
              <tr>
                <th className="p-2">
                  <div className="font-semibold text-left">Наименование</div>
                </th>
                <th className="p-2">
                  <div className="font-semibold text-center">Дата подключения</div>
                </th>
              </tr>
            </thead>
            {/* Table body */}
            <tbody className="text-sm font-medium divide-y divide-gray-100 dark:divide-gray-700/60">

              {
                items.map(device =>
                  <tr key={'device-' + device.id}>
                    <td className="p-2">
                      <div className="flex items-center">
                        {/* <svg className="shrink-0 mr-2 sm:mr-3" width="36" height="36" viewBox="0 0 36 36">
                        <circle fill="#24292E" cx="18" cy="18" r="18" />
                        <path
                          d="M18 10.2c-4.4 0-8 3.6-8 8 0 3.5 2.3 6.5 5.5 7.6.4.1.5-.2.5-.4V24c-2.2.5-2.7-1-2.7-1-.4-.9-.9-1.2-.9-1.2-.7-.5.1-.5.1-.5.8.1 1.2.8 1.2.8.7 1.3 1.9.9 2.3.7.1-.5.3-.9.5-1.1-1.8-.2-3.6-.9-3.6-4 0-.9.3-1.6.8-2.1-.1-.2-.4-1 .1-2.1 0 0 .7-.2 2.2.8.6-.2 1.3-.3 2-.3s1.4.1 2 .3c1.5-1 2.2-.8 2.2-.8.4 1.1.2 1.9.1 2.1.5.6.8 1.3.8 2.1 0 3.1-1.9 3.7-3.7 3.9.3.4.6.9.6 1.6v2.2c0 .2.1.5.6.4 3.2-1.1 5.5-4.1 5.5-7.6-.1-4.4-3.7-8-8.1-8z"
                          fill="#FFF"
                        />
                      </svg> */}
                        <ListItemAvatar>
                          <Avatar>
                            <DevicesIcon />
                          </Avatar>
                        </ListItemAvatar>
                        <div className="text-gray-800 dark:text-gray-100">{device.id}</div>
                      </div>
                    </td>
                    <td className="p-2">
                      <div className="text-center">{new Date(device.connectedTimestamp).toLocaleString('ru-RU', { timeZoneName: 'short' })}</div>
                    </td>
                  </tr>)
              }

            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default Devices;
