var ZERO_STATUS_OK_ID = (0x1);
var ZERO_STATUS_PLAN_ID = (0x2);
var ZERO_STATUS_RUNING_ID = (0x3);
var ZERO_STATUS_BYE_ID = (0x4);
var ZERO_STATUS_WECOME_ID = (0x5);
var ZERO_STATUS_WAIT_ID = (0x6);

var ZERO_STATUS_VOTE_SENDED_ID = (0x70);
var ZERO_STATUS_VOTE_BYE_ID = (0x71);
var ZERO_STATUS_VOTE_WAITING_ID = (0x72);
var ZERO_STATUS_VOTE_START_ID = (0x73);
var ZERO_STATUS_VOTE_END_ID = (0x74);
var ZERO_STATUS_VOTE_CLOSED_ID = (0x75);

var ZERO_STATUS_FAILED_ID = (0x80);

var ZERO_STATUS_BUG_ID = (0xD0);
var ZERO_STATUS_FRAME_INVALID_ID = (0xD1);
var ZERO_STATUS_ARG_INVALID_ID = (0xD2);

var ZERO_STATUS_ERROR_ID = (0xF0);
var ZERO_STATUS_NOT_FIND_ID = (0xF1);
var ZERO_STATUS_NOT_WORKER_ID = (0xF2);
var ZERO_STATUS_NOT_SUPPORT_ID = (0xF3);
var ZERO_STATUS_TIMEOUT_ID = (0xF4);
var ZERO_STATUS_NET_ERROR_ID = (0xF5);
var ZERO_STATUS_PLAN_ERROR_ID = (0xF6);
var ZERO_STATUS_SEND_ERROR_ID = (0xF7);
var ZERO_STATUS_RECV_ERROR_ID = (0xF8);


var ZERO_STATUS_SUCCESS = '+';
var ZERO_STATUS_OK = '+ok';
var ZERO_STATUS_PLAN = '+plan';
var ZERO_STATUS_RUNING = '+runing';
var ZERO_STATUS_BYE = '+bye';
var ZERO_STATUS_WECOME = '+wecome';
var ZERO_STATUS_WAITING = '+waiting';
var ZERO_STATUS_VOTE_SENDED = '+send';
var ZERO_STATUS_VOTE_CLOSED = '+close';
var ZERO_STATUS_VOTE_BYE = '+bye';
var ZERO_STATUS_VOTE_START = '+start';
var ZERO_STATUS_VOTE_END = '+end';

/*!
* ´íÎó×´Ì¬
*/
var ZERO_STATUS_BAD = '-';
var ZERO_STATUS_FAILED = '-failed';
var ZERO_STATUS_ERROR = '-error';
var ZERO_STATUS_NOT_SUPPORT = '-not support';
var ZERO_STATUS_NOT_FIND = '-not find';
var ZERO_STATUS_NOT_WORKER = '-not work';
var ZERO_STATUS_FRAME_INVALID = '-invalid frame';
var ZERO_STATUS_ARG_INVALID = '-invalid argument';
var ZERO_STATUS_TIMEOUT = '-time out';
var ZERO_STATUS_NET_ERROR = '-net error';
var ZERO_STATUS_PLAN_INVALID = '-plan invalid';
var ZERO_STATUS_PLAN_ERROR = '-plan error';

function exec_state_text(state) {
    switch (state) {
        case ZERO_STATUS_OK_ID: //!(0x1)
            return (ZERO_STATUS_OK);
            break;
        case ZERO_STATUS_PLAN_ID: //!(0x2)
            return (ZERO_STATUS_PLAN);
            break;
        case ZERO_STATUS_RUNING_ID: //!(0x3)
            return (ZERO_STATUS_RUNING);
            break;
        case ZERO_STATUS_BYE_ID: //!(0x4)
            return (ZERO_STATUS_BYE);
            break;
        case ZERO_STATUS_WECOME_ID: //!(0x5)
            return (ZERO_STATUS_WECOME);
            break;
        case ZERO_STATUS_VOTE_SENDED_ID: //!(0x20)
            return (ZERO_STATUS_VOTE_SENDED);
            break;
        case ZERO_STATUS_VOTE_BYE_ID: //!(0x21)
            return (ZERO_STATUS_VOTE_BYE);
            break;
        case ZERO_STATUS_WAIT_ID: //!(0x22)
            return (ZERO_STATUS_WAITING);
            break;
        case ZERO_STATUS_VOTE_WAITING_ID: //!(0x22)
            return (ZERO_STATUS_WAITING);
            break;
        case ZERO_STATUS_VOTE_START_ID: //!(0x23)
            return (ZERO_STATUS_VOTE_START);
            break;
        case ZERO_STATUS_VOTE_END_ID: //!(0x24)
            return (ZERO_STATUS_VOTE_END);
            break;
        case ZERO_STATUS_VOTE_CLOSED_ID: //!(0x25)
            return (ZERO_STATUS_VOTE_CLOSED);
        case ZERO_STATUS_ERROR_ID: //!(0x81)
            return (ZERO_STATUS_ERROR);
            break;
        case ZERO_STATUS_FAILED_ID: //!(0x82)
            return (ZERO_STATUS_FAILED);
            break;
        case ZERO_STATUS_NOT_FIND_ID: //!(0x83)
            return (ZERO_STATUS_NOT_FIND);
            break;
        case ZERO_STATUS_NOT_SUPPORT_ID: //!(0x84)
            return (ZERO_STATUS_NOT_SUPPORT);
            break;
        case ZERO_STATUS_FRAME_INVALID_ID: //!(0x85)
            return (ZERO_STATUS_FRAME_INVALID);
            break;
        case ZERO_STATUS_ARG_INVALID_ID: //!(0x85)
            return (ZERO_STATUS_ARG_INVALID);
            break;
        case ZERO_STATUS_TIMEOUT_ID: //!(0x86)
            return (ZERO_STATUS_TIMEOUT);
            break;
        case ZERO_STATUS_NET_ERROR_ID: //!(0x87)
            return (ZERO_STATUS_NET_ERROR);
            break;
        case ZERO_STATUS_NOT_WORKER_ID: //!(0x88)
            return (ZERO_STATUS_NOT_WORKER);
            break;
        case ZERO_STATUS_PLAN_ERROR_ID: //!(0x8B)
            return (ZERO_STATUS_PLAN_ERROR);
            break;
        default:
            return "*";
    }
}