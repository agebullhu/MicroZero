
var steps = [];
var tips = [];
var tip = '';
var hei = 0;
var empty = '';
var flowHtml = '';
var lines = [];
var names = [];
function showInfo() {
    $('#stepPanel').html(tip);
    $('#flowPanel').html(flowHtml);
    $('#flowPanel').width((lines.length * 172 + 21) + "px");
}
function showTip(step, type) {
    tip = '';
    for (var idx = 0; idx < steps.length; idx++) {
        if (steps[idx] === step) {
            tip = tips[idx][type];
            break;
        }
    }
    $('#stepPanel').html(tip);
}
function showFlow(rid) {
    try {
        hei = 0;
        steps = [];
        tips = [];
        empty = '';
        lines = [];
        names = [];
        tip = '';
        flowHtml = '';
        var idx;
        var root = null;
        for (idx = 0; idx < vue_option.data.flows.length; idx++) {
            if (vue_option.data.flows[idx].rid === rid) {
                root = vue_option.data.flows[idx];
                break;
            }
        }
        if (root) {
            showFlowItem(root.start, null, -1);

            hei = hei * 85 + 80;
            for (idx = 0; idx < lines.length; idx++) {
                if (lines[idx]) {
                    if (idx === 0)
                        flowHtml += "<div class='station1' style='height:" + hei + "px'>" + lines[idx] + "</div>";
                    else
                        flowHtml += "<div class='station' style='height:" + hei + "px'>" + lines[idx] + "</div>";
                }
            }
        }
        showInfo();
    } catch (e) {
        alert(e);
    }
}
function showLine(start, end, back, begin, htm) {
    hei++;
    empty += "<div class='empty'><div class='toempty'></div></div>";
    var i;
    lines[start] += '<div class="range">';
    if (start > end) {
        if (begin)
            lines[start] += '<div class="point_left"></div>' + htm + '<div class="point_right"></div>';
        else
            lines[start] += '<div class="point_left"><i class="el-icon-arrow-right"></i></div>' + htm + '<div class="point_right"></div>';
        for (i = 0; i < names.length; i++) {
            if (start === i) {
                continue;
            } else if (i === end) {
                lines[i] += back
                    ? "<div class='empty'><div class='toright2'></div></div>"
                    : "<div class='empty'><div class='toright'></div></div>";
            } else if (i > end && i < start) {
                lines[i] += back
                    ? "<div class='empty'><div class='toline2'></div></div>"
                    : "<div class='empty'><div class='toline'></div></div>";
            } else {
                lines[i] += "<div class='empty'></div>";
            }
        }
    } else {
        if (begin)
            lines[start] += '<div class="point_left"></div>' + htm + '<div class="point_right"></div>';
        else
            lines[start] += '<div class="point_left"></div>' + htm + '<div class="point_right "><i class="el-icon-arrow-left"></i></div>';
        for (i = 0; i < names.length; i++) {
            if (start === i) {
                continue;
            } else if (i === end) {
                lines[i] += back
                    ? "<div class='empty'><div class='toleft2'></div></div>"
                    : "<div class='empty'><div class='toleft'></div></div>";
            } else if (i > start && i < end) {
                lines[i] += back
                    ? "<div class='empty'><div class='toline2'></div></div>"
                    : "<div class='empty'><div class='toline'></div></div>";
            } else {
                lines[i] += "<div class='empty'></div>";
            }
        }
    }
    lines[start] += '</div>';
}
function showFlowItem(flow, step, pre) {
    var row = -1;
    for (var i = 0; i < names.length; i++) {
        if (names[i] === flow.station) {
            row = i;
            break;
        }
    }
    if (row === -1) {
        names.push(flow.station);
        lines.push(`<div class='title'>${flow.station}</div>${empty}`);
        row = lines.length - 1;
    }
    if (step != null) {
        steps.push(step);
        tips.push([
            `<div><i class='step'> ${step}</i><br/>${flow.station}/${flow.cmd}<br/><div>${flow.steps["Request"]}</div></div>`,
            `<div><i class='step'> ${step}</i><br/>${flow.station}/${flow.cmd}<br/><div>${flow.steps["Result"]}</div></div>`,
            `<div><i class='step'> ${step}</i><br/>${flow.station}/${flow.cmd}<br/><div>${flow.info}</div></div>`
        ]);

        showLine(row, pre, false, false, `<div class='point join' onclick='showTip(\"${step}\",0)'><i class='step'>${step}</i><br/>${flow.cmd}</div>`);
    } else {
        showLine(row, pre, false, true, `<div class='point call'><br><i class='step'>Begin</i></div>`);
    }

    var title = step == null
        ? "Step "
        : step;
    for (var idx = 0; idx < flow.child.length; idx++) {
        showFlowItem(flow.child[idx], title + (idx + 1).toString() + ".", row);
    }

    var cls;
    switch (flow.state) {
        case "General":
        case "Publish":
        case "Runing":
            cls = 'run';
            break;
        case "Ok":
            cls = 'ok';
            break;
        default:
            cls = 'failed';
            break;
    }
    if (step == null) {
        lines[pre] += `<div class='point ${cls}'><i class='step'>End</i><br/>${flow.state}</div>`;//<br/>${flow.station}<br/>${flow.cmd}
    } else {
        showLine(pre, row, true, false, `<div class='point ${cls}' onclick='showTip(\"${step}\",1)'><i class='step'>${step}</i><br/>${flow.state}</div>`);
    }
}
var ws_api;

function modify_sub(title, callback) {
    vue_obj.$prompt(title, '流程跟踪', {
        confirmButtonText: '确定',
        cancelButtonText: '取消'
    }).then(({ value }) => {
        if (callback)
            callback(value);
        else {
            ws_api.change_sub(value);
            if (!value)
                value = '没有任何订阅，您将收不到任何数据';
            else
                value = '订阅标签已修改为' + value;
            vue_obj.$message({
                type: 'info',
                message: value
            });
        }
    }).catch(() => {
        if (callback)
            callback('');
    });
}
extend_data({
    menu_wid: "360px",
    ws_action: '关闭',
    ws_icon: 'el-icon-circle-close',
    info: {
        show: false,
        title: '显示信息栏',
        width: '1px',
        icon: 'el-icon-arrow-left'
    },
    flows: []
});
extend_methods({
    menu_select: function (index, indexPath) {
        if (!index)
            return;
        if (index == 'menu-show') {
            var info = vue_option.data.info;
            info.show = !info.show;
            if (!info.show) {
                info.title = '显示信息栏';
                info.width = '1px';
                info.icon = 'el-icon-arrow-left';
            }
            else {
                info.title = '收起信息栏';
                info.width = '600px';
                info.icon = 'el-icon-arrow-right';
            }
            return;
        }
        else if (index == 'menu-msg-clear') {
            vue_option.data.flows = [];
            return;
        }
        else if (index == 'menu-msg-close') {
            if (vue_option.data.ws_action === '关闭') {
                vue_option.data.ws_action = '打开';
                vue_option.data.ws_icon = 'el-icon-circle-check';
                ws_api.close();
            } else {
                vue_option.data.ws_action = '关闭';
                vue_option.data.ws_icon = 'el-icon-circle-close';
                ws_api.open();
            }
            return;
        }
        else if (index == 'menu-msg-search') {
            modify_sub('请输入查询标签', function (val) {
                if (!val) {
                    vue_obj.$message({
                        type: 'info',
                        message: '查询标签为空,未做改变'
                    });
                    return;
                }
                if (val[0] === '#')
                    val = val.substring(1);
                if (!val) {
                    vue_obj.$message({
                        type: 'info',
                        message: '查询标签为空,未做改变'
                    });
                    return;
                }
                vue_obj.$message({
                    type: 'info',
                    message: '正在查询...'
                });
                ws_api.change_sub('~' + val);
                vue_option.data.flows = [];
                do_sync_get('/Flow/Query/' + encodeURI(val), '查询');
            });
            return;
        }
        else if (index == 'menu-msg-sub') {
            modify_sub('请输入订阅标签');
            return;
        }
        showFlow(index);
    }
});

function create_ws(val) {
    ws_api = new ws({
        address: `ws://${location.host}/trace_flow`,
        sub: val,
        onmessage: function (flow) {
            if (vue_option.data.flows.length > 300)
                return;
            console.log(JSON.stringify(flow));
            vue_update_array(vue_option.data.flows, flow, 'rid');
        }
    });
}