
var vue_obj;

var vue_option = {
    el: '#work_space',
    data: {
        ws_active: false,
        isCollapse: false
    },
    filters: {
        formatDate(time) {
            var date = new Date(time);
            return formatDate(date, 'MM-dd hh:mm:ss');
        },
        formatUnixDate(unix) {
            if (unix === 0)
                return "*";
            var date = new Date(unix * 1000);
            return formatDate(date, 'MM-dd hh:mm:ss');
        },
        formatNumber(number) {
            if (number) {
                return number.toFixed(4);
            } else {
                return "0.0";
            }
        },
        thousandsNumber(number) {
            if (number) {
                return toThousandsInt(number);
            } else {
                return "0";
            }
        },
        formatNumber1(number) {
            if (number) {
                return number.toFixed(4);
            } else {
                return "0.0";
            }
        },
        formatNumber0(number) {
            if (number) {
                return number.toFixed(0);
            } else {
                return "0";
            }
        },
        formatHex(number) {
            if (number) {
                return number.toString(16).toUpperCase();
            } else {
                return "-";
            }
        }
    },
    methods: {
        go_home() {
            location.href = "/Home";
        },
        go_monitor() {
            location.href = "/Monitor";
        },
        go_trace() {
            location.href = "/Flow";
        },
        go_plan() {
            location.href = "/Plan";
        },
        go_doc() {
            location.href = "/Doc";
        },
        go_event() {
            location.href = "/MachineEvent";
        },
        go_github() {
            location.href = "https://github.com/agebullhu/ZeroNet";
        }
    }
};
function extend_data(data) {
    vue_option.data = $.extend(vue_option.data, data);
}
function extend_filter(filters) {
    vue_option.filters = $.extend(vue_option.filters, filters);
}
function extend_methods(methods) {
    vue_option.methods = $.extend(vue_option.methods, methods);
}

function ws_state(active) {
    vue_option.data.ws_active = active;
};

/**
 * 更新到vue数组
 * @param {any} array 数组
 * @param {any} item 节点
 * @param {any} key 主键
 */
function vue_update_array(array, item, key) {
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
 * @param {any} job 标题
 * @param {any} callback 回调
 */
function do_sync_get(url, job, callback) {
    console.log(url);
    $.get(url, function (data) {
        console.log(data);
        var msg = null;
        if (data.status)
            msg = data.status.msg;
        if (!msg)
            msg = data.success ? '操作成功' : '操作异常';
        var type = data.success ? 'success' : 'warning';
        vue_obj.$notify({
            title: job,
            message: msg,
            type: type,
            position: 'bottom-right',
            duration: 2000
        });
        if (data.success && callback) {
            callback(data);
        }
    }).error(function (e) {
        vue_obj.$notify.error({
            title: job,
            message: '网络异常',
            position: 'bottom-right',
            duration: 2000
        });
    });
}


function formatDate(date, fmt) {
    if (!date || isNaN(date.getMonth()))
        return '-';
    if (!fmt)
        fmt = 'yyyy-MM-dd hh:mm:ss';
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



/**
 * /千分符小数表示法
 * @param {any} num 数字
 * @return {string} 格式化好的文本
 */
function toThousands(num) {
    if (!num)
        return "0.00";
    num = num.toString();
    if (!num)
        return "0.00";

    var result;

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
 * /千分符整数表示法
 * @param {any} num 数字
 * @return {string} 格式化好的文本
 */

function toThousandsInt(num) {
    if (!num)
        return "0";
    num = num.toString();
    if (!num)
        return "0";

    var result;

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