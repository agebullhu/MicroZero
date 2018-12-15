var vue_option = {
    el: '#work_space',
    data: {
        ws_active: false
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
            location.href = "/Trace";
        },
        go_plan() {
            location.href = "/Plan";
        },
        go_doc() {
            location.href = "/Doc";
        },
        go_event() {
            location.href = "/MachineEvent";
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
ws_state = function (active) {
    vue_option.data.ws_active = active;
}