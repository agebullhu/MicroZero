function htmlEncode(str) {
    if (!str)
        return "";
    var s = str.replace(/&/g, "&amp;");
    s = s.replace(/</g, "&lt;");
    s = s.replace(/>/g, "&gt;");
    s = s.replace(/ /g, "&nbsp;");
    s = s.replace(/\'/g, "&#39;");
    s = s.replace(/\"/g, "&quot;");
    return s;
}
function checkArray(cls) {
    var idxx = cls.type.indexOf('[');
    if (idxx > 0) {
        cls.isArray = true;
        cls.type = cls.type.substring(0, idxx);
    }
    idxx = cls.type.indexOf('IEnumerable<');
    if (idxx < 0)
        idxx = cls.type.indexOf('List<');
    if (idxx < 0)
        idxx = cls.type.indexOf('Collection<');
    if (idxx >= 0) {
        idxx = cls.type.indexOf('<');
        cls.isArray = true;
        cls.type = cls.type.substring(idxx + 1);
    }
}
function toCSharp(cls) {
    if (!cls || !cls.fields)
        return '';
    checkArray(cls);
    var sb = "";
    var first = true;
    if (cls.isArray)
        sb += `
<li><span class='bool'>new</span>&nbsp;<span class='class'>List&lt;${htmlEncode(cls.type)}&gt;</span></li>
<li>{</li>
<li>
<ul>`;

    sb += `
<li><span class='bool'>new</span>&nbsp;<span class='class'>${htmlEncode(cls.type)}</span></li>
<li>{</li>
<li>
<ul>`;
    for (var name in cls.fields) {
        if (cls.fields.hasOwnProperty(name)) {
            var field = cls.fields[name];
            if (first) {
                first = false;
            } else
                sb += ",</li>";
            if (field.caption && field.caption !== field.name)
                sb += `<li><span class='class'>//</span> <span class='null'>${field.caption}</span></li>`;
            sb += `<li><span class='name'></span>${field.name} = `;
            if (field.enum) {
                if (field.example) {
                    sb += `<span class='class'>${htmlEncode(field.class)}</span>.<span class='num'>${field.example}</span>`;
                } else if (field.fields) {
                    for (var en in field.fields) {
                        if (field.fields.hasOwnProperty(en)) {
                            var item = field.fields[en];
                            sb += `<span class='class'>${htmlEncode(field.class)}</span>.<span class='num'>${item.name}</span>`;
                            break;
                        }
                    }
                } else {
                    sb += `<span class='class'>${htmlEncode(field.class)}</span>.<span class='num'>None</span>`;
                }
                continue;
            }
            if (field.fields) {
                sb += toCSharp(field);
                continue;
            }

            var ex = field.example;
            var tp = "str";
            switch (field.type) {
                case "string":
                    if (!ex)
                        ex = field.caption ? field.caption : field.name;
                    sb += `<span class='${tp}'>"${ex}"</span>`;
                    break;
                case "DateTime":
                    if (!field.example)
                        ex = `new</span> <span class='class'>DateTime</span>(<span class='num'>2018</span>,<span class='num'>1</span>,<span class='num'>1</span>,<span class='num'>12</span>,<span class='num'>0</span>,<span class='num'>0</span>)`;
                    else
                        ex = `<span class='class'>DateTime.Parse</span>("${field.example}")`;
                    sb += `<span class='${tp}'>'${ex}'</span>`;
                    break;
                case "bool":
                    if (!ex)
                        ex = "false";
                    sb += `<span class='${tp}'>${ex}</span>`;
                    break;
                default:
                    if (!ex)
                        ex = "0";
                    sb += `<span class='${tp}'>${ex}</span>`;
                    break;
            }
        }
    }
    if (!first)
        sb += "</li>";
    if (cls.isArray)
        sb += `</ul><li>}`;
    sb += `</ul><li>}`;
    return sb;
}

function classCSharp(cls) {
    if (!cls || !cls.type || !cls.fields) {
        return "";
    }
    checkArray(cls);
    var sb = `<ul>`;
    if (cls.caption)
        sb += `
<li><span class='class'>/// &lt;summary&gt;</span>${cls.caption}<span class='class'>&lt;/summary&gt;</span></li>`;
    sb += `
<li>[JsonObject]</li>
<li><span class='bool'>public&nbsp;class</span>&nbsp;<span class='class'>${htmlEncode(cls.type)}</span></li>
<li>{</li><li><ul>`;

    var name;
    var field;
    for (name in cls.fields) {
        if (cls.fields.hasOwnProperty(name)) {
            field = cls.fields[name];
            checkArray(field);
            if (field.caption && field.name !== field.caption)
                sb += `<li><span class='class'>/// &lt;summary&gt;</span>${field.caption}<span class='class'>&lt;/summary&gt;</span></li>`;
            sb += `<li>[JsonProperty`;
            if (field.jsonName && field.name !== field.jsonName)
                sb += `"<span class='str'>${field.jsonName}</span>")`;
            sb += `]</li>
<li><span class='class'>public&nbsp;${htmlEncode(field.class)} </span><span class='name'>${field.name} </span>{&nbsp;get;&nbsp;set;&nbsp;}</li><br/>`;
        }
    }
    sb += `</ul></li><li>}</li>
</ul>`;
    for (name in cls.fields) {
        if (cls.fields.hasOwnProperty(name)) {
            field = cls.fields[name];
            if (!field.fields) {
                continue;
            }
            if (field.enum) {
                sb += enumCSharp(field, sb);
            } else {
                sb += classCSharp(field, sb);
            }
        }
    }
    return sb;
}

function enumCSharp(cls) {
    if (!cls || !cls.type || !cls.fields) {
        return "";
    }
    checkArray(cls);
    var sb = `<ul>`;
    if (cls.caption)
        sb += `
<li><span class='class'>/// &lt;summary&gt;</span>${cls.caption}<span class='class'>&lt;/summary&gt;</span></li>`;
    sb += `
<li>[JsonObject]</li>
<li><span class='bool'>public&nbsp;enum</span>&nbsp;<span class='class'>${htmlEncode(cls.type)}</span></li>
<li>{</li><li><ul>`;

    var name;
    var field;
    for (name in cls.fields) {
        if (cls.fields.hasOwnProperty(name)) {
            field = cls.fields[name];
            checkArray(field);
            if (field.caption && field.name !== field.caption)
                sb += `<li><span class='class'>/// &lt;summary&gt;</span>${field.caption}<span class='class'>&lt;/summary&gt;</span></li>`;
            sb += `<li><span class='name'>${field.name}</span>&nbsp;=&nbsp;span class='number'>${field.example},</span></li>`;
        }
    }
    sb += `</ul></li><li>}</li>
</ul>`;
    return sb;
}

function toJson(cls) {
    if (!cls || !cls.fields)
        return '';
    checkArray(cls);
    var sb = "";
    for (var name in cls.fields) {
        if (cls.fields.hasOwnProperty(name)) {
            var field = cls.fields[name];
            checkArray(field);
            if (sb)
                sb += ",</li>";
            sb += `<li><span class='name'>"${field.jsonName}"</span> : `;
            if (field.enum) {
                sb += `<span class='num'>0</span>`;
                continue;
            }
            if (!field.fields) {
                var ex = field.example;
                var tp = "str";
                switch (field.type) {
                    case "string":
                        if (!ex)
                            ex = field.caption ? field.caption : field.name;
                        sb += `<span class='${tp}'>"${ex}"</span>`;
                        break;
                    case "DateTime":
                        if (!ex)
                            ex = "2018-1-1 12:00:00:00";
                        sb += `<span class='${tp}'>"${ex}"</span>`;
                        break;
                    case "bool":
                        if (!ex)
                            ex = "false";
                        sb += `<span class='${tp}'>${ex}</span>`;
                        break;
                    default:
                        if (!ex)
                            ex = "0";
                        sb += `<span class='${tp}'>${ex}</span>`;
                        break;
                }
                continue;
            }
            if (field.isArray)
                sb += "[";
            sb += "{</li><li><ul>";
            sb += toJson(field);
            sb += "</ul></li><li>}";
            if (field.isArray)
                sb += "]";
        }
    }
    if (!sb)
        return '';
    return `${sb}</li>`;
}

function toApiTags(cls, head) {
    if (!cls || !cls.fields)
        return '';
    checkArray(cls);
    var sb = "";
    for (var name in cls.fields) {
        if (cls.fields.hasOwnProperty(name)) {
            var field = cls.fields[name];
            checkArray(field);
            sb += "<tr>";
            if (field.enum || field.fields) {
                sb += `<td>${head}<span class='class'>${field.name}</span></td>
                       <td>${head}<span class='class'>${field.jsonName ? field.jsonName : field.name}</span></td>`;
            } else {
                sb += `<td>${head}${field.name}</td>
                       <td>${head}${field.jsonName ? field.jsonName : field.name}</td>`;
            }
            if (!field.fields) {
                sb += `<td>${htmlEncode(field.class)}</td>`;
            } else if (field.enum) {
                sb += `<td>${htmlEncode(field.class)}<span class='name'>(Enum)</span></td>`;
            } else {
                sb += `<td>object<span class='name'>(${htmlEncode(field.class)})</span></td>`;
            }
            if (field.enum || field.fields) {
                sb += `<td>${head}<span class='class'>${field.caption ? field.caption : ''}</span></td>`;
            } else {
                sb += `<td>${head}${field.caption ? field.caption : ''}</td>`;
            }
            sb += `<td>${field.description ? field.description + '<br/>' : ''}${field.value ? field.value : ''}</td>`;
            if (field.example && (!field.fields || field.enum)) {
                sb += `<td>${field.example}</td>`;
            } else {
                sb += "<td></td>";
            }
            sb += `</tr>`;
            sb += toApiTags(field, head + "&nbsp;&nbsp;&nbsp;&nbsp;");
        }
    }
    return sb;
}

extend_methods({
    menu_select: function (index) {
        if (index === '_update')
            location.href = "/Doc/Update/" + this.name;
        else
            location.href = "/Doc/Index/" + index;
    }
});
extend_filter({
    toMark(val) {
        return '#' + toMarkId(val, "");
    },
    toMarkApi(val) {
        return toMarkId(val, "");
    },
    toMark_arg_cs(val) {
        return toMarkId(val, "_arg_cs");
    },
    toMark_arg_js(val) {
        return toMarkId(val, "_arg_js");
    },
    toMark_res_js(val) {
        return toMarkId(val, "_res_js");
    },
    toMark_res_cs(val) {
        return toMarkId(val, "_res_cs");
    },
    toCapition(row) {
        if (row.caption)
            return row.caption;
        return row.name;
    },
    outputTags(field) {
        return toApiTags(field, "");
    },
    outputJson(field) {
        return "<li>{</li><li><ul>" + toJson(field) + "</ul></li><li>}</li>";
    },
    outputCSharp(field) {
        var sb = classCSharp(field);
        sb += "<ul>";
        sb += toCSharp(field);
        sb += "</ul>";
        return sb;
    }
});

function show(head, last) {
    if (last === "_cs") {
        show1(head + "_cs");
        hide1(head + "_js");
    } else {
        show1(head + "_js");
        hide1(head + "_cs");
    }
}

function hide1(el) {
    var elm = document.getElementById(el);
    if (!elm)
        return;
    elm.style.display = "none";
}

function show1(el) {
    var elm = document.getElementById(el);
    if (!elm)
        return;
    elm.style.display = "block";
}


function toMarkId(val, last) {
    return val ? `${val.replace(new RegExp("/", "gm"), "_")}${last}` : "";
}

function prepareApi() {
    try {
        for (var idx = 0; idx < arg.group.length; idx++) {
            for (var jdx = 0; jdx < arg.group[idx].apis.length; jdx++) {
                var api = arg.group[idx].apis[jdx];
                api.argHtml = toApiTags(api.argument, "");
                api.argCs = `${classCSharp(api.argument)}<ul>${toCSharp(api.argument)}</ul>`;
                api.argJs = "<li>{</li><li><ul>" + toJson(api.argument) + "</ul></li><li>}</li>";
                api.resHtml = toApiTags(api.result, "");
                api.resCs = `${classCSharp(api.result)}<ul>${toCSharp(api.result)}</ul>`;
                api.resJs = "<li>{</li><li><ul>" + toJson(api.result) + "</ul></li><li>}</li>";
            }
        }
    } catch (e) {
        console.error(e);
    }
}
