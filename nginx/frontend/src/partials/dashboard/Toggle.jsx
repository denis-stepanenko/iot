import React, { useState, useEffect } from 'react';
import Tooltip from '../../components/Tooltip';
import { chartAreaGradient } from '../../charts/ChartjsConfig';
import RealtimeChart from '../../charts/RealtimeChart';
import { Link } from 'react-router-dom';
import { Typography, Switch, FormControlLabel, Slider, IconButton } from '@mui/material'

// Import utilities
import { adjustColorOpacity, getCssVariable } from '../../utils/Utils';
import EditMenu from '../../components/DropdownEditMenu';

function Toggle({ title, topic, onRemove, checked, onChange }) {


  return (
    <div className="flex flex-col col-span-full sm:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">

      <div className="px-5 pt-4">
        <header className="flex justify-between items-start">
          <h2 className="text-lg font-semibold text-gray-800 dark:text-gray-100">{title}
            <Tooltip align="left" className="ml-2 relative inline-flex">
              <div className="text-xs text-center whitespace-nowrap">{topic}</div>
            </Tooltip>
          </h2>
          {/* Menu button */}
          <EditMenu align="right" className="relative inline-flex">
            <li>
              <Link className="font-medium text-sm text-red-500 hover:text-red-600 flex py-1 px-3" onClick={x => onRemove()}>
              Удалить
              </Link>
            </li>
          </EditMenu>
        </header>


      </div>


      {/* Chart built with Chart.js 3 */}
      {/* Change the height attribute to adjust the chart height */}
      {/* <RealtimeChart data={chartData} width={595} height={248} /> */}

      <FormControlLabel sx={{ ml: 2, mb: 2 }}
        control={<Switch
          checked={checked}
          onChange={onChange}
        />}
      />
    </div>
  );
}

export default Toggle;
