import React, { useState, useEffect } from 'react';
import Tooltip from '../../components/Tooltip';
import { chartAreaGradient } from '../../charts/ChartjsConfig';
import RealtimeChart from '../../charts/RealtimeChart';
import { Link } from 'react-router-dom';

// Import utilities
import { adjustColorOpacity, getCssVariable } from '../../utils/Utils';
import EditMenu from '../../components/DropdownEditMenu';

function Indicator({ title, topic, data, onRemove }) {

  const chartData = {
    labels: data ? data.map(function (o) { return new Date(o.date) }) : [],
    datasets: [
      // Indigo line
      {
        data: data ? data.map(function (o) { return o.value }) : [],
        fill: false,
        backgroundColor: function (context) {
          const chart = context.chart;
          const { ctx, chartArea } = chart;
          return chartAreaGradient(ctx, chartArea, [
            { stop: 0, color: adjustColorOpacity(getCssVariable('--color-violet-500'), 0) },
            { stop: 1, color: adjustColorOpacity(getCssVariable('--color-violet-500'), 0.2) }
          ]);
        },
        borderColor: getCssVariable('--color-violet-500'),
        borderWidth: 2,
        pointRadius: 0,
        pointHoverRadius: 3,
        pointBackgroundColor: getCssVariable('--color-violet-500'),
        pointHoverBackgroundColor: getCssVariable('--color-violet-500'),
        pointBorderWidth: 0,
        pointHoverBorderWidth: 0,
        clip: 20,
        tension: 0.2,
      },
    ],
  };

  return (
    <div className="flex flex-col col-span-full sm:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">

      <div className="px-5 pt-5">
        <header className="flex justify-between items-start mb-2">
          <h2 className="text-lg font-semibold text-gray-800 dark:text-gray-100 mb-2">{title}
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
      <RealtimeChart data={chartData} width={595} height={248} />
    </div>
  );
}

export default Indicator;
