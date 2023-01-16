/* global Chart:false */

$(function () {
    'use strict'

    var ticksStyle = {
        fontColor: '#495057',
        fontStyle: 'bold'
    }
    //debugger;

    var mode = 'index'
    var intersect = true

    //avgtime - chart
    var $avgtimeChart = $('#avgtime-chart');
    var formattedavgtimebarDataset = JSON.parse($('#avgtimebardiv').text())
    console.log(formattedavgtimebarDataset);

    var avgtimeChart = new Chart($avgtimeChart, {
        type: 'bar',
        data: formattedavgtimebarDataset,
        options: {
            maintainAspectRatio: false,
            tooltips: {
                mode: mode,
                intersect: intersect
            },
            hover: {
                mode: mode,
                intersect: intersect
            },
            legend: {
                display: false
            },
            scales: {
                yAxes: [{
                    // display: false,
                    gridLines: {
                        display: true,
                        lineWidth: '4px',
                        color: 'rgba(0, 0, 0, .2)',
                        zeroLineColor: 'transparent'
                    },
                    ticks: $.extend({
                        beginAtZero: true,

                        // Include a dollar sign in the ticks
                        //callback: function (value) {
                        //    if (value >= 1000) {
                        //        value /= 1000
                        //        value += 'k'
                        //    }

                        //    return '$' + value
                        //}
                    }, ticksStyle)
                }],
                xAxes: [{
                    display: true,
                    gridLines: {
                        display: false
                    },
                    ticks: ticksStyle
                }]
            }
        }
    })

    //bar chart
    var $salesChart = $('#sales-chart');
    var formattedbarDataset = JSON.parse($('#bardiv').text())

    // eslint-disable-next-line no-unused-vars
    var salesChart = new Chart($salesChart, {
        type: 'bar',
        data: formattedbarDataset,
        //data: {
        //    //labels: ['JUN', 'JUL', 'AUG', 'SEP', 'OCT', 'NOV', 'DEC'],


        //    //[
        //    //    {
        //    //        backgroundColor: '#007bff',
        //    //        borderColor: '#007bff',
        //    //        data: [1000, 2000, 3000, 2500, 2700, 2500, 3000]
        //    //    },
        //    //    {
        //    //        backgroundColor: '#ced4da',
        //    //        borderColor: '#ced4da',
        //    //        data: [700, 1700, 2700, 2000, 1800, 1500, 2000]
        //    //    }
        //    //]
        //},
        options: {
            maintainAspectRatio: false,
            tooltips: {
                mode: mode,
                intersect: intersect
            },
            hover: {
                mode: mode,
                intersect: intersect
            },
            legend: {
                display: false
            },
            scales: {
                yAxes: [{
                    // display: false,
                    gridLines: {
                        display: true,
                        lineWidth: '4px',
                        color: 'rgba(0, 0, 0, .2)',
                        zeroLineColor: 'transparent'
                    },
                    ticks: $.extend({
                        beginAtZero: true,

                        // Include a dollar sign in the ticks
                        //callback: function (value) {
                        //    if (value >= 1000) {
                        //        value /= 1000
                        //        value += 'k'
                        //    }

                        //    return '$' + value
                        //}
                    }, ticksStyle)
                }],
                xAxes: [{
                    display: true,
                    gridLines: {
                        display: false
                    },
                    ticks: ticksStyle
                }]
            }
        }
    })

    //Line Chart
    var $visitorsChart = $('#visitors-chart')
    // eslint-disable-next-line no-unused-vars
    var formattedDataset = JSON.parse($('#linediv').text())
    var visitorsChart = new Chart($visitorsChart, {
        data:

            //{ "labels": ["11/20/2022", "11/21/2022", "11/22/2022", "11/23/2022", "11/24/2022", "11/25/2022", "11/26/2022", "11/27/2022", "11/28/2022", "11/29/2022", "11/30/2022", "12/1/2022", "12/2/2022", "12/3/2022", "12/4/2022", "12/7/2022", "12/19/2022"], "datasets": [{ "type": "line", "pointBorderColor": "#007bff", "pointBackgroundColor": "#007bff", "fill": false, "backgroundColor": "#007bff", "borderColor": "#007bff", "data": [23, 68, 74, 107, 50, 4, 2, 49, 71, 72, 59, 82, 2, 2, 30, 4, 2] }, { "type": "line", "pointBorderColor": "#ced4da", "pointBackgroundColor": "#ced4da", "fill": false, "backgroundColor": "#ced4da", "borderColor": "#ced4da", "data": [11, 72, 73, 108, 46, 2, 4, 48, 76, 65, 56, 78, 6, 1, 16] }] }

            formattedDataset
        //{
        //    labels: ['18th', '20th', '22nd', '24th', '26th', '28th', '30th'],
        //    datasets: [{
        //        type: 'line',
        //        data: [100, 120, 170, 167, 180, 177, 160],
        //        backgroundColor: 'transparent',
        //        borderColor: '#007bff',
        //        pointBorderColor: '#007bff',
        //        pointBackgroundColor: '#007bff',
        //        fill: false
        //        // pointHoverBackgroundColor: '#007bff',
        //        // pointHoverBorderColor    : '#007bff'
        //    },
        //    {
        //        type: 'line',
        //        data: [60, 80, 70, 67, 80, 77, 100],
        //        backgroundColor: 'tansparent',
        //        borderColor: '#ced4da',
        //        pointBorderColor: '#ced4da',
        //        pointBackgroundColor: '#ced4da',
        //        fill: false
        //        // pointHoverBackgroundColor: '#ced4da',
        //        // pointHoverBorderColor    : '#ced4da'
        //    }]
        //}
        ,
        options: {
            maintainAspectRatio: false,
            tooltips: {
                mode: mode,
                intersect: intersect
            },
            hover: {
                mode: mode,
                intersect: intersect
            },
            legend: {
                display: false
            },
            scales: {
                yAxes: [{
                    // display: false,
                    gridLines: {
                        display: true,
                        lineWidth: '4px',
                        color: 'rgba(0, 0, 0, .2)',
                        zeroLineColor: 'transparent'
                    },
                    ticks: $.extend({
                        beginAtZero: true,
                        suggestedMax: 200
                    }, ticksStyle)
                }],
                xAxes: [{
                    display: true,
                    gridLines: {
                        display: false
                    },
                    ticks: ticksStyle
                }]
            }
        }
    })
})

// lgtm [js/unused-local-variable]
