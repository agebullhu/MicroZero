
extend_data({
    plans: [],
    plan: {
        plan_type: "0",
        plan_value: 0,
        plan_repet: 0,
        description: null,
        no_skip: 0,
        plan_time: null,
        station: null,
        skip_set: 0,
        command: null,
        context: null,
        argument: null
    },
    zeroPlan: {
        visible: false,
        loading: false,
        labelWidth: 100,
        form: {
            plan_type: "0",
            plan_value: 0,
            plan_repet: 0,
            description: null,
            no_skip: 0,
            plan_time: null,
            station: null,
            skip_set: 0,
            command: null,
            context: null,
            argument: null
        },
        rules: {
            'description': [
                { required: true, message: '请输入计划说明', trigger: 'blur' }
            ],
            'station': [
                { required: true, message: '请输入站点', trigger: 'blur' }
            ],
            'command': [
                { required: true, message: '请输入调用的API', trigger: 'blur' }
            ],
            'plan_type': [
                { required: true, message: '请输入调用的API', trigger: 'blur' },
                { min: 1, max: 7, message: '请选择正确计划类型', trigger: 'blur' }
            ]
        }
    }
});


extend_filter({
    format_plan_date_type(type) {
        switch (type) {
            default:
                return 'none';
            case 1:
                return 'time';
            case 2:
                return 'second';
            case 3:
                return 'minute';
            case 4:
                return 'hour';
            case 5:
                return 'day';
            case 6:
                return 'week';
            case 7:
                return 'month';
        }
    },
    format_plan_state(state) {
        switch (state) {
            default:
                return 'none';
            case 1:
                return 'queue';
            case 2:
                return 'execute';
            case 3:
                return 'retry';
            case 4:
                return 'skip';
            case 5:
                return 'pause';
            case 6:
                return 'error';
            case 7:
                return 'close';
        }
    },
    format_exec_state(state) {
        return exec_state_text(state);
    },
    format_plan(plan) {
        return  format_plan(plan);
    },
    format_skip(plan) {
        if (plan.skip_set === 0) {
            if (plan.no_skip)
                return `Error(${plan.skip_num})`;
            return `Overdue(${plan.skip_num})`;
        }
        if (plan.skip_set === -2)
            return `Retry(${plan.skip_num}/3)`;
        if (plan.skip_set === -1)
            return `Pause(${plan.skip_num})`;
        return `Skip(${plan.skip_num}/${plan.skip_set})`;
    },
    format_plan_type(plan_type) {
        switch (plan_type) {
            case "2":
                return "秒";
            case "3":
                return "分钟";
            case "4":
                return "小时";
            case "5":
                return "天";
        }
        return "";
    },
    format_plan_value(vl) {
        if (vue_option.data.zeroPlan.form.plan_type !== 7) {
            return "";
        }
        if (vl > 0)
            return "号";

        return "倒数第" + Math.abs(vl) + "天";
    }
});
extend_methods({
    new_plan() {
        this.zeroPlan.form = {
            plan_type: "0",
            plan_value: 0,
            plan_repet: 0,
            description: null,
            no_skip: 0,
            plan_time: null,
            station: null,
            skip_set: 0,
            command: null,
            context: null,
            argument: null
        };
        this.zeroPlan.visible = true;
    },
    save_zeroPlan() {
        var that = this;
        var data = vue_option.data.zeroPlan;
        if (format_plan(data.form) === '错误' || !data.form.command) {
            that.$message.error('内容不合理');
            return false;
        }
        this.$refs['zeroPlanForm'].validate((valid) => {
            if (!valid) {
                that.$message.error('内容不合理');
                return false;
            }
            data.loading = true;
            data.form.plan_time1 = formatDate(data.form.plan_time);
            $.post('Plan/Add', data.form, function (result) {
                data.loading = false;
                if (result.success) {
                    that.$message({
                        message: '操作成功',
                        type: 'success'
                    });
                    data.visible = false;
                }
                else {
                    that.$message.error('操作失败:' + result.status.msg);
                }
                data.loading = false;
            }).error(function () {
                data.loading = false;
                data.visible = false;
                that.$message.error('更新失败');
            });
            return true;
        });
        return true;
    },
    plan_remove(plan) {
        do_sync_get(`/Plan/Remove/${plan.plan_id}`, `删除计划任务:${plan.description}`);
    },
    plan_reset(plan) {
        do_sync_get(`/Plan/Reset/${plan.plan_id}`, `恢复计划任务:${plan.description}`);
    },
    plan_pause(plan) {
        do_sync_get(`/Plan/Pause/${plan.plan_id}`, `暂停计划任务:${plan.description}`);
    },
    plan_close(plan) {
        do_sync_get(`/Plan/Close/${plan.plan_id}`, `关闭计划任务:${plan.description}`);
    },
    format_frame(frames) {
        if (!frames || frames.length === 0)
            return '*';
        if (frames.length === 1)
            return frames[0];
        var desc = eval(`(${frames[1]})`);
        var msg = `<div><div class='el-form-item el-form-item--small' style='inline-block'><label class='el-form-item__label' style='width:100px'>\
                           <i class='el-icon-date'>Caller</i></label><div class='el-form-item__content'><span>${frames[0]}</span></div></div>`;
        var idx;
        if (desc) {
            for (idx = 2; idx < frames.length; idx++) {
                msg += `<div class='el-form-item el-form-item--small' style='inline-block'><label class='el-form-item__label' style='width:100px'>\
                                <i class='el-icon-star-on'>${desc.frames[idx]}</i></label><div class='el-form-item__content'><span>${frames[idx]}</span></div></div>`;
                if ((idx % 2) === 0)
                    msg += "</div><div>";
            }
        } else {
            for (idx = 1; idx < frames.length; idx++) {
                msg += `<i class='el-icon-star-on'>${frames[idx]}</i>`;
                if (idx % 2 !== 0)
                    msg += "</div><div>";
            }
        }
        msg += "</div>";
        return msg;
    },

    menu_select: function (index, indexPath) {
        switch (index) {
            case "operator-add":
                this.new_plan();
                return;
            case "operator-flush":
                this.plans = [];
                do_sync_get("/Plan/Flush", "刷新数据");
                return;
            case "type-once":
                do_sync_get("/Plan/Filter/none", "单次任务", res => {
                    vue_option.data.plans = res.data;
                });
                return;
            case "type-time":
                do_sync_get("/Plan/Filter/time", "定时任务", res => {
                    vue_option.data.plans = res.data;
                });
                return;
            case "type-delay":
                do_sync_get("/Plan/Filter/delay", "延时任务", res => {
                    vue_option.data.plans = res.data;
                });
                return;
            case "type-week":
                do_sync_get("/Plan/Filter/week", "每周任务", res => {
                    vue_option.data.plans = res.data;
                });
                return;
            case "type-month":
                do_sync_get("/Plan/Filter/month", "每月任务", res => {
                    vue_option.data.plans = res.data;
                });
                return;
            case "type-active":
                do_sync_get("/Plan/Active", "活动任务", res => {
                    vue_option.data.plans = res.data;
                });
                return;
            case "type-history":
                do_sync_get("/Plan/History", "历史任务", res => {
                    vue_option.data.plans = res.data;
                });
                return;
            default:
                do_sync_get("/Plan/Station/" + index, this, "站点数据", res => {
                    vue_option.data.plans = res.data;
                });
                return;
        }
    }
});

function format_plan(plan) {
    var txt = "";
    var type = plan.plan_type ? parseInt(plan.plan_type) : 0;
    if (type < 1 || type > 7)
        return "错误";
    var date;
    if (type === 1) {
        if (!plan.plan_time)
            return "立即执行一次，不重复";
        date = new Date(plan.plan_time);
        if (plan.no_skip)
            return "在" + formatDate(date, 'MM-dd hh:mm:ss') + "执行，如果时间已过则立即执行，不重复";
        else
            return "仅在" + formatDate(date, 'MM-dd hh:mm:ss') + "没有过去的情况下执行一次，不重复";
    }
    var value = plan.plan_value ? parseInt(plan.plan_value) : 0;
    var skip = plan.skip_set ? parseInt(plan.skip_set) : 0;
    var repet = plan.plan_repet ? parseInt(plan.plan_repet) : 0;
    var sk = "";
    if (repet === 0 || value === 0)
        return "错误";
    if (type < 6) {
        if (value <= 0 || value > 32767)
            return "错误";

        txt = "从";
        if (!plan.plan_time)
            txt += "现在起";
        else {

            date = new Date(plan.plan_time);
            txt += formatDate(date, 'MM-dd hh:mm:ss') + "起";
        }
        if (skip > 0) {
            txt += "，跳过" + (skip * value);
            switch (type) {
                case 2:
                    txt += "秒";
                    break;
                case 3:
                    txt += "分钟";
                    break;
                case 4:
                    txt += "小时";
                    break;
                case 5:
                    txt += "天";
                    break;
            }
            txt += "后，";
        }
        txt += "每" + value;

        switch (type) {
            case 2:
                txt += "秒";
                break;
            case 3:
                txt += "分钟";
                break;
            case 4:
                txt += "小时";
                break;
            case 5:
                txt += "天";
                break;
        }
        if (plan.no_skip)
            sk = "计算执行时间时,过去的时间不会跳过。";
        else
            sk = "计算执行时间时,如果时间已经过去将直接跳过(执行次数也会步进)。";

    }
    if (type === 6) {
        if (value <= 0 || value > 7)
            return "错误";
        if (skip > 0) {
            txt += "跳过前" + skip + "周后，";
        }
        txt += "每周" + (value == 7 ? "日" : value.toString());
        if (!plan.plan_time)
            txt += "零点";
        else {
            date = new Date(plan.plan_time);
            txt += "的" + formatDate(date, 'hh:mm:ss');
        }
        sk = "不计算过去的时间。";
    }
    if (type === 7) {
        if (skip > 0) {
            txt += "跳过前" + skip + "个月后，";
        }
        txt += "每月";
        if (value > 0)
            txt += value + "号";
        else
            txt += "倒数第" + Math.abs(value) + "天";
        if (!plan.plan_time)
            txt += "零点";
        else {
            date = new Date(plan.plan_time);
            txt += formatDate(date, 'hh:mm:ss');
        }
        sk = "不计算过去的时间。";
    }
    if (repet < 0)
        txt += "执行一次，永久重复执行。";
    else {
        txt += "执行一次，重复" + repet + "次后结束。";
    }
    txt += sk;
    return txt;
}
var socket_status = new ws({
    address: `ws://${location.host}/plan_notify`,
    sub: null,
    onmessage: function (item) {
        var plans = vue_option.data.plans;
        for (var idx = 0; idx < plans.length; idx++) {
            var old = plans[idx];
            if (old.plan_id === item.plan_id) {
                if (item.plan_state === 8) {
                    vm.$notify({
                        title: "消息推送",
                        message: old.description + '已删除',
                        type: 'warning',
                        position: 'bottom-left',
                        duration: 2000
                    });
                    Vue.delete(plans, idx);
                }
                else {
                    old.exec_time = item.exec_time;
                    old.exec_state = item.exec_state;
                    old.plan_state = item.plan_state;
                    old.plan_time = item.plan_time;
                    old.real_repet = item.real_repet;
                    old.skip_set = item.skip_set;
                    old.skip_num = item.skip_num;
                }
                return;
            }
        }
        if (item.plan_state !== 8)
            plans.push(item);
    }
});


function close_open() {
    vue_option.data.isCollapse = !vue_option.data.isCollapse;
    if (vue_option.data.isCollapse)
        $('#main_menu').width('auto');
    else
        $('#main_menu').width(vue_option.data.menu_wid);
}

function start() {
    if (!vue_option.data.menu_wid)
        vue_option.data.menu_wid = '300px';
    vue_obj = new Vue(vue_option);
    if (vue_option.data.isCollapse)
        $('#main_menu').width('auto');
    else
        $('#main_menu').width(vue_option.data.menu_wid);

    socket_status.open();
}

start();