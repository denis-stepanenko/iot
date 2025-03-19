import React from 'react';
import ListItemAvatar from '@mui/material/ListItemAvatar';
import Avatar from '@mui/material/Avatar';
import DevicesIcon from '@mui/icons-material/Devices';

function WebSocketConnections({ items }) {
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
              </tr>
            </thead>
            {/* Table body */}
            <tbody className="text-sm font-medium divide-y divide-gray-100 dark:divide-gray-700/60">

              {
                items.map(device =>
                  <tr key={'connection-' + device.name}>
                    <td className="p-2">
                      <div className="flex items-center">

                        <ListItemAvatar>
                          <Avatar>
                            <DevicesIcon />
                          </Avatar>
                        </ListItemAvatar>
                        <div className="text-gray-800 dark:text-gray-100">{device.name}</div>
                      </div>
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

export default WebSocketConnections;
