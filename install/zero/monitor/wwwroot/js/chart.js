
Highcharts.setOptions({
    global: {
        useUTC: false
    },
    exporting: {
        enabled: false
    },
    credits: {
        enabled: false
    }
});


function activeLastPointToolip(chart) {
    var points = chart.series[0].points;
    chart.tooltip.refresh(points[points.length - 1]);
}


function update_chart(chart, value) {
    var point = chart.series[0].points[0];
    point.update(value);
}

function gauge_chart_option(max, option) {
    var def = {
        credits: {
            enabled: false
        },
        chart: {
            type: 'gauge',
            plotBackgroundColor: null,
            plotBackgroundImage: null,
            plotBorderWidth: 0,
            plotShadow: false
        },
        pane: {
            startAngle: -150,
            endAngle: 150,
            background: [
                {
                    backgroundColor: {
                        linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
                        stops: [
                            [0, '#FFF'],
                            [1, '#333']
                        ]
                    },
                    borderWidth: 0,
                    outerRadius: '109%'
                }, {
                    backgroundColor: {
                        linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
                        stops: [
                            [0, '#333'],
                            [1, '#FFF']
                        ]
                    },
                    borderWidth: 1,
                    outerRadius: '107%'
                }, {
                    // default background

                }, {
                    backgroundColor: '#DDD',
                    borderWidth: 0,
                    outerRadius: '105%',
                    innerRadius: '103%'
                }
            ]
        },
        yAxis: {
            min: 0,
            max: max,
            minorTickInterval: 'auto',
            minorTickWidth: 1,
            minorTickLength: 10,
            minorTickPosition: 'inside',
            minorTickColor: '#666',
            tickPixelInterval: 30,
            tickWidth: 2,
            tickPosition: 'inside',
            tickLength: 10,
            tickColor: '#666',
            labels: {
                step: 2,
                rotation: 'auto'
            },
            plotBands: [
                {
                    from: 0,
                    to: max * 0.4,
                    color: '#55BF3B' // green
                }, {
                    from: max * 0.4,
                    to: max * 0.8,
                    color: '#DDDF0D' // yellow
                }, {
                    from: max * 0.8,
                    to: max,
                    color: '#DF5353' // red
                }
            ]
        }
    };
    return $.extend(option, def);
}

function solidgauge_chart_option(max, option) {
    var def = {
        credits: {
            enabled: false
        },
        chart: {
            type: 'solidgauge'
        },
        pane: {
            center: ['50%', '85%'],
            size: '140%',
            startAngle: -90,
            endAngle: 90,
            background: {
                backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || '#EEE',
                innerRadius: '60%',
                outerRadius: '100%',
                shape: 'arc'
            }
        },
        yAxis: {
            min: 0,
            max: max,
            stops: [
                [0.5, '#55BF3B'], // green
                [0.75, '#DDDF0D'], // yellow
                [0.9, '#DF5353'] // red
            ],
            lineWidth: 0,
            minorTickInterval: null,
            tickPixelInterval: 400,
            tickWidth: 0,
            formatter: function () {
                return this.value.toFixed(1);
            }
        },
        xAxis: {
            formatter: function () {
                return this.value.toFixed(1);
            }
        },
        plotOptions: {
            solidgauge: {
                dataLabels: {
                    y: 5,
                    borderWidth: 0,
                    useHTML: true
                }
            }
        }
    };
    return $.extend(option, def);
}


function line_chart_option(option) {
    var def = {
        credits: {
            enabled: false
        },
        chart: {
            type: 'spline',
            marginRight: 10
        },
        xAxis: {
            type: 'datetime',
            tickPixelInterval: 150
        },
        plotOptions: {
            spline: {
                lineWidth: 3,
                states: {
                    hover: {
                        lineWidth: 5
                    }
                },
                marker: {
                    enabled: false
                }
            }
        },
        tooltip: {
            formatter: function () {
                return '<b>' +
                    this.series.name +
                    '</b><br/>' +
                    Highcharts.dateFormat('%Y-%m-%d %H:%M:%S', this.x) +
                    '<br/>' +
                    Highcharts.numberFormat(this.y, 2);
            }
        },
        legend: {
            enabled: false
        }
    };
    return $.extend(option, def);
}