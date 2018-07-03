// Write your Javascript code.
/**
 * 更新到vue数组
 * @param {any} array 数组
 * @param {any} item 节点
 * @param {any} key 主键
 */
function vue_update_array(array, item,key) {
    for (var idx = 0; idx < array.length; idx++) {
        var old = array[idx];
        if (old[key] === item[key]) {
            Vue.set(array, idx, item);
            return;
        }
    }
    array.push(item);
}
/**
 * ajax get 操作
 * @param {any} url 地址
 * @param {any} vue_obj vue快捷对象
 */
function do_sync_get(url, vue_obj, job, callback) {
    $.get(url, function (data) {
        console.log(data);
        if (data.success) {
            vue_obj.$notify({
                title: job,
                message: '操作成功',
                type: 'success',
                position: 'bottom-left',
                duration: 2000
            });
            if (callback)
                callback(data);
        }
        else {
            vue_obj.$notify({
                title: job,
                message: data.status.msg,
                type: 'warning',
                position: 'bottom-left',
                duration: 2000
            });
        }
    }).error(function (e) {
        vue_obj.$notify.error({
            title: job,
            message: '操作异常',
            position: 'bottom-left',
            duration: 2000
        });
    });
}

function formatDate(date, fmt) {
    if (isNaN(date.getMonth()))
        return date;
    if (/(y+)/.test(fmt)) {
        fmt = fmt.replace(RegExp.$1, (date.getFullYear() + '').substr(4 - RegExp.$1.length));
    }
    var o = {
        'M+': date.getMonth() + 1,
        'd+': date.getDate(),
        'h+': date.getHours(),
        'm+': date.getMinutes(),
        's+': date.getSeconds()
    };
    for (var k in o) {
        if (o.hasOwnProperty(k)) {
            if (new RegExp(`(${k})`).test(fmt)) {
                var str = o[k] + '';
                fmt = fmt.replace(RegExp.$1, (RegExp.$1.length === 1) ? str : padLeftZero(str));
            }
        }
    }
    return fmt;
}
function padLeftZero(str) {
    return ('00' + str).substr(str.length);
}


Highcharts.setOptions({
    global: {
        useUTC: false
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

/**
 * /千分符数字表示法
 */
function toThousands(num) {
    if (!num || num == "")
        return "0.00";
    num = num.toString();
    if (num == "")
        return "0.00";

    var result;

    var ls;
    var p = num.indexOf(".");
    if (p >= 0) {
        result = num.substr(p, num.length - p);
        num = num.substring(0, p);
    } else {
        result = ".00";
    }
    while (num.length > 3) {
        result = "," + num.slice(-3) + result;
        num = num.slice(0, num.length - 3);
    }
    if (num) {
        result = num + result;
    }
    return result;
}

/**
 * /千分符数字表示法
 */
function toThousandsInt(num) {
    if (!num || num == "")
        return "0";
    num = num.toString();
    if (num == "")
        return "0";

    var result;

    var ls;
    var p = num.indexOf(".");
    if (p >= 0) {
        result = num.substr(p, num.length - p);
        num = num.substring(0, p);
    } else {
        result = "";
    }
    while (num.length > 3) {
        result = "," + num.slice(-3) + result;
        num = num.slice(0, num.length - 3);
    }
    if (num) {
        result = num + result;
    }
    return result;
}