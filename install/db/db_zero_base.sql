/*
 Navicat Premium Data Transfer

 Source Server         : localhost
 Source Server Type    : MySQL
 Source Server Version : 50725
 Source Host           : 192.168.240.132:3306
 Source Schema         : db_zero_base

 Target Server Type    : MySQL
 Target Server Version : 50725
 File Encoding         : 65001

 Date: 02/04/2019 09:56:42
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for tb_app_app_info
-- ----------------------------
DROP TABLE IF EXISTS `tb_app_app_info`;
CREATE TABLE `tb_app_app_info`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `app_type` int(11) NOT NULL COMMENT '应用类型',
  `short_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '应用简称',
  `full_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '应用全称',
  `app_id` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '应用标识',
  `app_key` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '应用令牌',
  `memo` text CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '备注',
  `data_state_type` int(11) NOT NULL DEFAULT 0 COMMENT '数据状态枚举类型',
  `is_freeze` tinyint(1) NOT NULL DEFAULT 0 COMMENT '冻结更新',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '制作时间',
  `author_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '制作人',
  `last_reviser_id` bigint(20) NULL DEFAULT 0 COMMENT '最后修改者',
  `last_modify_date` datetime(0) NULL DEFAULT NULL COMMENT '最后修改日期',
  `data_state` int(11) NOT NULL COMMENT '数据状态',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '应用信息' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for tb_app_page_item
-- ----------------------------
DROP TABLE IF EXISTS `tb_app_page_item`;
CREATE TABLE `tb_app_page_item`  (
  `id` bigint(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `item_type` varchar(11) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '节点类型',
  `name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '名称',
  `caption` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '标题',
  `icon` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '图标',
  `url` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '链接地址',
  `memo` varchar(2000) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '备注',
  `parent_id` varchar(11) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '上级标识',
  `extend_value` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '扩展值',
  `index` varchar(11) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '序号',
  `json` longtext CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '扩展的JSON配置',
  `api_host` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'Api地址',
  `app_info_id` bigint(20) NULL DEFAULT NULL COMMENT '应用标识',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `ID_UNIQUE`(`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  INDEX `ItemType`(`item_type`) USING BTREE,
  INDEX `ParentId`(`parent_id`) USING BTREE,
  INDEX `Index`(`index`) USING BTREE,
  INDEX `Name_Index`(`name`) USING BTREE,
  INDEX `AppId_Index`(`app_info_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1484 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '页面节点' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_app_page_item
-- ----------------------------
INSERT INTO `tb_app_page_item` VALUES (3, '2', 'PageItemData', '页面配置', NULL, '/App/PageItem/index', '页面管理', '1411', NULL, '2', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (4, '2', 'RoleData', '角色管理', NULL, '/App/Role/index', '角色管理', '1411', NULL, '0', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (10, '0', 'Org', '组织机构', 'el-icon-info', NULL, '组织机构', '0', NULL, '4', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (11, '2', 'OrganizationData', '机构信息', NULL, '/Group/Organization/index', '机构', '10', NULL, '2', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (12, '2', 'OrganizePositionData', '职位信息', NULL, '/Group/OrganizePosition/index', '机构职位设置', '10', NULL, '3', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (13, '2', 'PositionPersonnelData', '人员职位', NULL, '/Group/PositionPersonnel/index', '人员职位设置', '10', NULL, '4', '{\"pid\":0,\"hide\":false,\"audit\":true,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":true}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (254, '3', '#btnDelete', '删除', NULL, NULL, '删除', '11', 'delete', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (255, '3', '#btnEdit', '详细', NULL, NULL, '详细', '11', 'details', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (256, '3', '#btnAdd', '新增', NULL, NULL, '新增', '11', 'addnew', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (265, '3', '#btnStateButtons', '状态按钮检查', NULL, NULL, '状态按钮检查', '3', 'normalbuttons', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (267, '3', '#btnFlushCache', '刷新系统缓存', NULL, NULL, '刷新系统缓存', '3', 'flushcache', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (268, '3', '#btnSetParent', '设置新分类', NULL, NULL, '设置新分类', '3', 'setparent', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (270, '3', '#btnEdit', '详细', NULL, NULL, '详细', '3', 'details', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (271, '3', '#btnAdd', '新增', NULL, NULL, '新增', '3', 'addnew', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (272, '3', '#btnDelete', '删除', NULL, NULL, '删除', '3', 'delete', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (568, '3', '#btnCreateSubjection', '启用所有修改', NULL, NULL, '启用所有修改', '12', 'createsubjection', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (569, '3', '#btnAdd', '新增', NULL, NULL, '新增', '12', 'addnew', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (570, '3', '#btnDelete', '删除', NULL, NULL, '删除', '12', 'delete', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (571, '3', '#btnEdit', '详细', NULL, NULL, '详细', '12', 'details', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (572, '3', '#btnCreateAll', '添加所有主管与办事员', NULL, NULL, '添加所有主管与办事员', '12', 'createall', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (592, '3', '#btnAdd', '新增', NULL, NULL, '新增', '5', 'addnew', '1', NULL, NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (593, '3', '#btnEdit', '详细', NULL, NULL, '详细', '5', 'details', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (597, '3', '#btnAdd', '新增', NULL, NULL, '新增', '4', 'addnew', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (598, '3', '#btnEdit', '详细', NULL, NULL, '详细', '4', 'details', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (633, '3', '#btnEdit', '详细', NULL, NULL, '详细', '13', 'details', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (635, '3', '#btnAdd', '新增', NULL, NULL, '新增', '13', 'addnew', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (637, '3', '#btnDelete', '删除', NULL, NULL, '删除', '13', 'delete', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (639, '3', '#btnSubmit', '提交审核', NULL, NULL, '提交审核', '13', 'submit', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (640, '3', '#btnAuditDeny', '审核(否决)', NULL, NULL, '审核(否决)', '13', 'deny', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (641, '3', '#btnAuditPass', '审核(通过)', NULL, NULL, '审核(通过)', '13', 'pass', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (642, '3', '#btnValidate', '数据校验', NULL, NULL, '数据校验', '13', 'validate', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (643, '3', '#btnAuditBack', '退回重做', NULL, NULL, '退回重做', '13', 'back', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (644, '3', '#btnReAudit', '反审核', NULL, NULL, '反审核', '13', 'reaudit', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (713, '3', '#btnBindType', '绑定类型', NULL, NULL, '绑定类型', '3', 'bindtype', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (724, '3', '#btnSave', '修改', NULL, NULL, '修改', '13', 'update', '0', '{\"type\":\"extend\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (730, '3', '#btnPullback', '拉回', NULL, NULL, '拉回', '13', 'pullback', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (731, '3', '#btnSave', '修改', NULL, NULL, '修改', '3', 'update', '0', '{\"type\":\"action\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (732, '3', '#btnValidate', '数据校验', NULL, NULL, '数据校验', '3', 'validate', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (734, '3', '#btnSave', '修改', NULL, NULL, '修改', '4', 'update', '0', '{\"type\":\"extend\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (735, '3', '#btnValidate', '数据校验', NULL, NULL, '数据校验', '4', 'validate', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (736, '3', '#btnDelete', '删除', NULL, NULL, '删除', '4', 'delete', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (750, '3', '#btnSave', '修改', NULL, NULL, '修改', '5', 'update', '0', '{\"type\":\"extend\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (751, '3', '#btnValidate', '数据校验', NULL, NULL, '数据校验', '5', 'validate', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (752, '3', '#btnDelete', '删除', NULL, NULL, '删除', '5', 'delete', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (865, '3', '#btnSave', '修改', NULL, NULL, '修改', '11', 'update', '0', '{\"type\":\"extend\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (879, '3', '#btnSave', '修改', NULL, NULL, '修改', '12', 'update', '0', '{\"type\":\"extend\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1061, '3', '#btnExport', '导出', NULL, NULL, '导出', '3', 'export', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1080, '3', '#btnExport', '导出', NULL, NULL, '导出', '13', 'export', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1089, '3', '#btnResetPwd', '重置密码', NULL, NULL, '重置密码', '5', 'reset_pwd', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1090, '3', '#btnExport', '导出', NULL, NULL, '导出', '5', 'export', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1136, '3', '#btnExport', '导出', NULL, NULL, '导出', '4', 'export', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1143, '3', '#btnExport', '导出', NULL, NULL, '导出', '11', 'export', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1144, '3', '#btnExport', '导出', NULL, NULL, '导出', '12', 'export', '0', '{\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1369, '4', 'normal_buttons', 'normal_buttons', NULL, NULL, NULL, '3', 'normal_buttons', '0', '{\"type\":\"action\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1374, '4', 'powers', 'powers', NULL, NULL, NULL, '4', 'powers', '0', '{\"type\":\"action\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1375, '4', 'savepowers', 'savepowers', NULL, NULL, NULL, '4', 'savepowers', '0', '{\"type\":\"action\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1396, '4', 'tree', 'tree', NULL, NULL, NULL, '3', 'tree', '0', '{\"type\":\"action\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1398, '4', 'set_parent', 'set_parent', NULL, NULL, NULL, '3', 'set_parent', '0', '{\"type\":\"action\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1399, '3', '#btnEnable', '启用', NULL, NULL, NULL, '5', 'disable', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1400, '3', '#btnDisable', '禁用', NULL, NULL, NULL, '5', 'enable', '0', '{\"type\":\"button\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1405, '3', '#btnAdd', '新增', NULL, NULL, '新增', '5', 'addnew', '0', '{\"type\":\"aaa\",\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1406, '3', '#btnExport', '导出', NULL, NULL, '导出', '5', 'export', '0', NULL, NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1411, '0', 'AppManage', '系统管理', 'el-icon-setting', NULL, '应用管理', '0', NULL, '6', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (1422, '0', 'User', '用户信息', NULL, NULL, '用户信息', '0', NULL, '1', '{\"pid\":0,\"hide\":true,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (1423, '2', 'Account', '登录账户', NULL, '/User/User/Account/index', '用于支持用户的账户名密码登录', '1411', NULL, NULL, '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1426, '2', 'Person', '个人信息', NULL, '/User/User/Person/index', '用户的个人信息', '1422', NULL, NULL, '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, NULL);
INSERT INTO `tb_app_page_item` VALUES (1428, '2', 'User', '注册信息', NULL, '/User/User/User/index', 'APP端用户信息表', '1422', NULL, '0', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (1431, '2', 'Wechat', '微信认证', NULL, '/User/User/Wechat/index', '微信联合认证关联的用户信息', '1422', NULL, '0', '{\"pid\":0,\"hide\":true,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (1481, '3', '编辑', '编辑', NULL, NULL, NULL, '1426', NULL, '0', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (1482, '3', '新增', '新增', NULL, NULL, NULL, '1426', NULL, '0', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);
INSERT INTO `tb_app_page_item` VALUES (1483, '3', '#btnAdd', '新增', NULL, NULL, '新增', '1423', NULL, '0', '{\"pid\":0,\"hide\":false,\"audit\":false,\"level_audit\":false,\"audit_page\":0,\"master_page\":0,\"data_state\":false,\"edit\":false}', NULL, 0);

-- ----------------------------
-- Table structure for tb_auth_login_log
-- ----------------------------
DROP TABLE IF EXISTS `tb_auth_login_log`;
CREATE TABLE `tb_auth_login_log`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '主键',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '加入时间',
  `user_id` bigint(20) NOT NULL COMMENT '用户Id',
  `device_id` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '设备识别码',
  `os` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '登录操作系统',
  `browser` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '浏览器',
  `channal` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '登录渠道码',
  `login_type` int(11) NOT NULL COMMENT '登录方式',
  `login_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '登录使用的名称',
  `success` tinyint(1) NOT NULL COMMENT '是否登录成功',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  INDEX `UserId_Index`(`user_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '用户登录日志' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for tb_auth_position_role
-- ----------------------------
DROP TABLE IF EXISTS `tb_auth_position_role`;
CREATE TABLE `tb_auth_position_role`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `app_id` bigint(20) NOT NULL COMMENT '应用标识',
  `position_id` bigint(20) NOT NULL COMMENT '职位标识',
  `role_id` bigint(20) NOT NULL COMMENT '职位标识',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '职位角色' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for tb_auth_role
-- ----------------------------
DROP TABLE IF EXISTS `tb_auth_role`;
CREATE TABLE `tb_auth_role`  (
  `id` bigint(11) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `role` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '角色',
  `memo` text CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '备注',
  `caption` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '标题',
  `is_freeze` tinyint(1) NOT NULL DEFAULT 0 COMMENT '冻结更新',
  `data_state` int(11) NOT NULL DEFAULT 0 COMMENT '数据状态',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '制作时间',
  `last_reviser_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '最后修改者',
  `last_modify_date` datetime(0) NULL DEFAULT NULL COMMENT '最后修改日期',
  `author_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '制作人',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  INDEX `Caption_Index`(`caption`) USING BTREE,
  INDEX `IsFreeze_Index`(`is_freeze`) USING BTREE,
  INDEX `DataState_Index`(`data_state`) USING BTREE,
  INDEX `AddDate_Index`(`add_date`) USING BTREE,
  INDEX `LastReviserId_Index`(`last_reviser_id`) USING BTREE,
  INDEX `LastModifyDate_Index`(`last_modify_date`) USING BTREE,
  INDEX `AuthorId_Index`(`author_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 6 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '角色' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_auth_role
-- ----------------------------
INSERT INTO `tb_auth_role` VALUES (1, 'developer', '开发专用', '开发者账户', 0, 0, NULL, 1, '2019-01-17 10:06:31', 0);
INSERT INTO `tb_auth_role` VALUES (2, '系统账号', '系统管理维护，账号及权限配置等', '系统账号', 0, 0, NULL, 1, '2019-01-17 10:06:24', 0);
INSERT INTO `tb_auth_role` VALUES (5, '编辑账号', NULL, '编辑账号', 0, 0, NULL, 1, '2018-12-24 19:31:56', 0);

-- ----------------------------
-- Table structure for tb_auth_role_power
-- ----------------------------
DROP TABLE IF EXISTS `tb_auth_role_power`;
CREATE TABLE `tb_auth_role_power`  (
  `id` bigint(11) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `page_item_id` bigint(11) NOT NULL COMMENT '页面标识',
  `role_id` bigint(11) NOT NULL COMMENT '角色标识',
  `power` int(11) NOT NULL COMMENT '权限',
  `data_scope` int(11) NOT NULL COMMENT '权限范围',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  INDEX `PageItemId_Index`(`page_item_id`) USING BTREE,
  INDEX `RoleId_Index`(`role_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 11497 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '角色权限' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_auth_role_power
-- ----------------------------
INSERT INTO `tb_auth_role_power` VALUES (1076, -4, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1077, 87, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1078, 88, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1079, 526, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1080, 527, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1081, 528, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1082, 529, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1083, 531, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1084, 538, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1085, 627, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1086, 790, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1087, 460, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1088, 796, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1089, 797, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1090, 798, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1091, 800, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1092, 801, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1093, 808, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1094, 809, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1095, 455, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1096, 599, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1097, 600, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1098, 601, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1099, 602, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1100, 606, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1101, 608, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1102, 630, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1103, 793, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1104, -3, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1105, 89, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1106, 461, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1107, 462, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1108, 463, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1109, 464, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1110, 465, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1111, 472, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1112, 488, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1113, 626, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1114, 862, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1115, 458, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1116, 474, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1117, 476, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1118, 478, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1119, 482, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1120, 485, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1121, 840, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1122, 841, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1123, 90, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1124, 814, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1125, 815, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1126, 816, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1127, 818, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1128, 819, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1129, 826, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1130, 827, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1131, 456, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1132, 495, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1133, 496, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1134, 499, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1135, 505, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1136, 507, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1137, 509, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1138, 510, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1139, 514, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1140, 516, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1141, 517, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1142, 519, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1143, 522, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1144, 524, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1145, 832, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1146, 833, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1147, 457, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1148, 540, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1149, 541, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1150, 543, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1151, 548, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1152, 552, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1153, 836, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1154, 837, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1155, 459, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1156, 844, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1157, 845, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1158, 846, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1159, 848, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1160, 849, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1161, 856, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1162, 857, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1163, -2, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1164, 99, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1165, 100, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1166, 171, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1167, 173, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1168, 174, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1169, 175, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1170, 176, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1171, 177, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1172, 893, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1173, 894, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1174, 901, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1175, 104, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1176, 906, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1177, 907, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1178, 908, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1179, 910, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1180, 911, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1181, 918, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1182, 919, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1183, 170, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1184, 920, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1185, 921, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1186, 922, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1187, 924, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1188, 925, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1189, 103, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1190, 105, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1191, 152, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1192, 153, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1193, 197, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1194, 198, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1195, 200, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1196, 264, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1197, 628, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1198, 714, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1199, 74, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1200, 179, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1201, 181, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1202, 182, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1203, 183, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1204, 190, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1205, 191, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1206, 192, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1207, 194, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1208, 279, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1209, 283, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1210, 284, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1211, 285, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1212, 290, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1213, 291, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1214, 715, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1215, 82, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1216, 215, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1217, 216, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1218, 217, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1219, 218, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1220, 224, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1221, 277, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1222, 716, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1223, 158, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1224, 203, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1225, 205, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1226, 206, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1227, 207, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1228, 213, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1229, 261, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1230, 717, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1231, 159, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1232, 227, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1233, 228, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1234, 229, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1235, 231, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1236, 237, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1237, 273, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1238, 718, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1239, 151, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1240, 160, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1241, 240, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1242, 241, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1243, 242, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1244, 244, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1245, 249, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1246, 307, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1247, 721, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1248, 76, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1249, 310, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1250, 312, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1251, 313, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1252, 314, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1253, 317, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1254, 370, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1255, 371, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1256, 372, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1257, 632, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1258, 936, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1259, 78, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1260, 320, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1261, 321, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1262, 322, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1263, 325, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1264, 330, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1265, 333, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1266, 939, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1267, 161, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1268, 335, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1269, 336, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1270, 339, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1271, 341, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1272, 345, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1273, 362, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1274, 365, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1275, 366, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1276, 942, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1277, 162, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1278, 347, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1279, 349, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1280, 350, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1281, 353, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1282, 357, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1283, 359, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1284, 945, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1285, 163, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1286, 292, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1287, 378, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1288, 380, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1289, 382, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1290, 387, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1291, 390, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1292, 957, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1293, 958, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1294, 164, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1295, 392, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1296, 393, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1297, 394, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1298, 396, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1299, 398, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1300, 433, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1301, 948, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1302, 166, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1303, 434, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1304, 435, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1305, 436, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1306, 438, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1307, 445, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1308, 447, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1309, 954, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1310, 165, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1311, 404, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1312, 405, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1313, 407, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1314, 408, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1315, 412, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1316, 451, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1317, 951, 4, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1827, -5, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1828, 91, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1829, 92, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1830, 254, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1831, 255, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1832, 257, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1833, 258, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1834, 867, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1835, 868, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1836, 869, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1837, 870, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1838, 871, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1839, 872, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1840, 875, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1841, 876, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1842, 877, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1843, 878, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1844, 93, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1845, 568, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1846, 570, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1847, 571, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1848, 572, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1849, 573, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1850, 881, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1851, 882, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1852, 883, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1853, 884, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1854, 885, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1855, 886, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1856, 889, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1857, 890, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1858, 891, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1859, 892, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1860, 96, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1861, 633, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1862, 634, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1863, 636, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1864, 637, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1865, 638, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1866, 640, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1867, 641, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1868, 643, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1869, 644, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1870, 725, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1871, 726, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1872, 727, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1873, 728, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1874, 729, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1875, 84, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1876, 77, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1877, 374, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1878, 375, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1879, 377, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1880, 668, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1881, 698, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1882, 699, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1883, 700, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1884, 701, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1885, 777, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1886, 778, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1887, 83, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1888, 574, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1889, 575, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1890, 576, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1891, 645, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1892, 86, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1893, 578, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1894, 580, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1895, 581, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1896, 582, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1897, 583, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1898, 584, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1899, 585, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1900, 629, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1901, 690, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1902, 783, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1903, 784, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1904, 785, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1905, 610, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1906, 612, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1907, 613, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1908, 614, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1909, 615, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1910, 616, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1911, 617, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1912, 618, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1913, 646, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1914, 788, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1915, 789, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1916, 68, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1917, 69, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1918, 619, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1919, 621, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1920, 622, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1921, 623, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1922, 624, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1923, 625, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1924, 765, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1925, 766, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1926, 767, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1927, 71, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1928, 107, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1929, 108, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1930, 109, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1931, 110, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1932, 111, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1933, 112, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1934, 115, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1935, 116, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1936, 117, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1937, 118, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1938, 251, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1939, 252, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1940, 769, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1941, 770, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1942, 167, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1943, 647, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1944, 648, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1945, 649, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1946, 650, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1947, 652, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1948, 654, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1949, 655, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1950, 656, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1951, 658, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1952, 676, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1953, 677, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1954, 772, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1955, 773, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1956, 774, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1957, 2, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1958, 3, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1959, 265, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1960, 266, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1961, 267, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1962, 268, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1963, 269, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1964, 270, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1965, 272, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1966, 713, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1967, 733, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1968, 4, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1969, 596, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1970, 598, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1971, 736, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1972, 737, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1973, 738, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1974, 739, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1975, 740, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1976, 741, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1977, 742, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1978, 743, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1979, 746, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1980, 747, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1981, 748, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1982, 749, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1983, 5, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1984, 591, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1985, 593, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1986, 594, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1987, 595, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1988, 659, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1989, 752, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1990, 753, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1991, 754, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1992, 755, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1993, 756, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1994, 759, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1995, 760, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1996, 761, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1997, 762, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1998, -1, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (1999, 168, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2000, 169, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2001, 416, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2002, 417, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2003, 418, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2004, 420, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2005, 421, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2006, 422, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2007, 423, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2008, 424, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2009, 427, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2010, 429, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2011, 430, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2012, 631, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2013, 962, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2014, 963, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2015, 719, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2016, 967, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2017, 968, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2018, 969, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2019, 970, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2020, 971, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2021, 972, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2022, 973, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2023, 974, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2024, 975, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2025, 978, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2026, 979, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2027, 980, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2028, 981, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2029, 720, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2030, 985, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2031, 986, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2032, 987, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2033, 988, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2034, 989, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2035, 990, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2036, 991, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2037, 992, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2038, 993, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2039, 996, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2040, 997, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2041, 998, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2042, 999, 6, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2044, -4, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2045, 87, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2046, 88, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2047, 526, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2048, 527, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2049, 532, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2050, 533, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2051, 535, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2052, 536, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2053, 460, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2054, 800, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2055, 801, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2056, 810, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2057, 811, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2058, 812, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2059, 813, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2060, 455, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2061, 491, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2062, 492, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2063, 493, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2064, 494, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2065, 600, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2066, 601, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2067, -3, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2068, 89, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2069, 461, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2070, 462, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2071, 465, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2072, 467, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2073, 469, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2074, 471, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2075, 473, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2076, 458, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2077, 474, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2078, 480, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2079, 481, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2080, 483, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2081, 484, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2082, 841, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2083, 90, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2084, 818, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2085, 819, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2086, 828, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2087, 829, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2088, 830, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2089, 831, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2090, 456, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2091, 495, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2092, 501, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2093, 502, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2094, 503, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2095, 504, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2096, 833, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2097, 457, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2098, 541, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2099, 546, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2100, 547, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2101, 549, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2102, 551, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2103, 837, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2104, 459, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2105, 848, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2106, 849, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2107, 858, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2108, 859, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2109, 860, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2110, 861, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2111, -2, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2112, 99, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2113, 100, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2114, 177, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2115, 894, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2116, 902, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2117, 903, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2118, 904, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2119, 905, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2120, 104, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2121, 147, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2122, 148, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2123, 149, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2124, 150, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2125, 910, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2126, 911, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2127, 103, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2128, 105, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2129, 154, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2130, 155, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2131, 156, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2132, 157, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2133, 197, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2134, 200, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2135, 74, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2136, 182, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2137, 183, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2138, 185, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2139, 187, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2140, 188, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2141, 189, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2142, 82, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2143, 216, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2144, 217, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2145, 222, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2146, 223, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2147, 225, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2148, 226, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2149, 158, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2150, 203, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2151, 206, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2152, 208, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2153, 210, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2154, 212, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2155, 214, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2156, 159, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2157, 229, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2158, 231, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2159, 234, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2160, 235, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2161, 236, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2162, 238, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2163, 76, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2164, 312, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2165, 314, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2166, 315, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2167, 316, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2168, 318, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2169, 319, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2170, 151, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2171, 160, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2172, 241, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2173, 242, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2174, 245, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2175, 247, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2176, 248, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2177, 250, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2178, 78, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2179, 321, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2180, 325, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2181, 327, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2182, 328, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2183, 329, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2184, 331, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2185, 161, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2186, 336, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2187, 339, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2188, 342, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2189, 343, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2190, 344, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2191, 346, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2192, 162, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2193, 347, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2194, 350, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2195, 354, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2196, 355, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2197, 356, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2198, 361, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2199, 163, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2200, 292, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2201, 378, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2202, 384, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2203, 385, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2204, 386, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2205, 389, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2206, 958, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2207, 164, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2208, 393, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2209, 394, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2210, 399, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2211, 400, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2212, 402, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2213, 403, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2214, 166, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2215, 434, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2216, 438, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2217, 441, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2218, 442, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2219, 443, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2220, 444, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2221, 165, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2222, 405, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2223, 408, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2224, 409, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2225, 410, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2226, 411, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (2227, 415, 3, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5744, 0, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5745, -2, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5746, 1246, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5747, 1252, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5748, 1172, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5749, 1173, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5750, 1174, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5751, 1175, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5752, 1176, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5753, 1178, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5754, 1179, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5755, 1273, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5756, 1274, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5757, 1275, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5758, 1276, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5759, 1277, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5760, 1278, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5761, 1329, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5762, 1169, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5763, 1198, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5764, 1245, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5765, 1264, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5766, 1263, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5767, 1224, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5768, 1225, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5769, 1226, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5770, 1227, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5771, 1228, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5772, 1230, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5773, 1231, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5774, 1238, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5775, 1239, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5776, 1240, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5777, 1241, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5778, 1242, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5779, 1243, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5780, 99, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5781, 100, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5782, 893, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5783, 1145, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5784, 1318, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5785, 1322, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5786, 1330, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5787, 1331, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5788, 1333, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5789, 1334, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5790, 1335, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5791, 104, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5792, 147, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5793, 148, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5794, 149, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5795, 150, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5796, 1337, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5797, 1370, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5798, 1170, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5799, 1254, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5800, 1255, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5801, 1256, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5802, 1257, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5803, 1258, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5804, 1259, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5805, 1260, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5806, 1261, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5807, 1262, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5808, 1287, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5809, 1338, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5810, 1340, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5811, 1341, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5812, 1342, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5813, 1344, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5814, 103, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5815, 105, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5816, 152, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5817, 153, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5818, 154, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5819, 155, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5820, 156, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5821, 157, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5822, 197, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5823, 198, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5824, 199, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5825, 264, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5826, 714, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5827, 1058, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5828, 1345, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5829, 74, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5830, 179, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5831, 180, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5832, 181, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5833, 183, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5834, 185, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5835, 187, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5836, 188, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5837, 189, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5838, 190, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5839, 279, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5840, 715, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5841, 1147, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5842, 1347, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5843, 82, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5844, 215, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5845, 217, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5846, 218, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5847, 219, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5848, 222, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5849, 223, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5850, 224, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5851, 225, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5852, 226, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5853, 277, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5854, 716, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5855, 1148, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5856, 1092, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5857, 1099, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5858, 1100, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5859, 1101, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5860, 1102, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5861, 1104, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5862, 1111, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5863, 1112, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5864, 1113, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5865, 1114, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5866, 1115, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5867, 1116, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5868, 1149, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5869, 1000, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5870, 158, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5871, 203, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5872, 204, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5873, 205, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5874, 207, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5875, 208, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5876, 210, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5877, 212, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5878, 213, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5879, 214, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5880, 261, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5881, 717, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5882, 1159, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5883, 159, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5884, 227, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5885, 228, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5886, 229, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5887, 230, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5888, 234, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5889, 235, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5890, 236, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5891, 237, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5892, 238, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5893, 273, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5894, 718, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5895, 1160, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5896, 76, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5897, 313, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5898, 315, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5899, 316, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5900, 317, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5901, 318, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5902, 319, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5903, 370, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5904, 936, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5905, 1158, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5906, 1211, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5907, 1212, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5908, 1213, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5909, 1003, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5910, 1021, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5911, 1022, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5912, 1023, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5913, 1028, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5914, 1030, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5915, 1031, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5916, 1032, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5917, 1033, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5918, 1034, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5919, 1035, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5920, 1125, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5921, 1161, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5922, 1129, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5923, 1130, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5924, 1131, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5925, 1132, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5926, 1133, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5927, 1135, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5928, 1162, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5929, 151, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5930, 160, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5931, 239, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5932, 240, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5933, 241, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5934, 244, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5935, 245, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5936, 247, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5937, 248, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5938, 249, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5939, 250, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5940, 307, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5941, 721, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5942, 1065, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5943, 78, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5944, 320, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5945, 321, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5946, 322, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5947, 326, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5948, 327, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5949, 328, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5950, 329, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5951, 330, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5952, 331, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5953, 333, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5954, 939, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5955, 1150, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5956, 161, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5957, 341, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5958, 342, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5959, 343, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5960, 344, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5961, 345, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5962, 346, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5963, 362, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5964, 942, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5965, 1151, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5966, 1217, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5967, 1218, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5968, 1219, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5969, 162, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5970, 347, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5971, 348, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5972, 349, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5973, 353, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5974, 354, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5975, 355, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5976, 356, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5977, 357, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5978, 359, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5979, 361, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5980, 945, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5981, 1067, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5982, 1004, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5983, 1006, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5984, 1007, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5985, 1008, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5986, 1013, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5987, 1014, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5988, 1016, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5989, 1017, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5990, 1018, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5991, 1019, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5992, 1020, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5993, 1117, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5994, 1152, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5995, 163, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5996, 292, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5997, 378, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5998, 380, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (5999, 381, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6000, 382, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6001, 384, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6002, 385, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6003, 386, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6004, 387, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6005, 389, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6006, 390, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6007, 957, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6008, 1156, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6009, 164, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6010, 392, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6011, 393, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6012, 395, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6013, 396, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6014, 398, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6015, 399, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6016, 400, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6017, 402, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6018, 403, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6019, 433, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6020, 948, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6021, 1153, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6022, 166, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6023, 434, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6024, 435, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6025, 436, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6026, 439, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6027, 441, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6028, 442, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6029, 443, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6030, 444, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6031, 445, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6032, 447, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6033, 954, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6034, 1155, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6035, 165, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6036, 404, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6037, 405, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6038, 406, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6039, 407, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6040, 409, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6041, 410, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6042, 411, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6043, 412, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6044, 415, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6045, 451, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6046, 951, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6047, 1154, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6048, 1005, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6049, 1041, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6050, 1043, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6051, 1044, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6052, 1048, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6053, 1049, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6054, 1050, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6055, 1051, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6056, 1052, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6057, 1053, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6058, 1054, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6059, 1121, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6060, 1157, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6061, -3, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6062, 1247, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6063, 89, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6064, 461, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6065, 462, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6066, 463, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6067, 464, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6068, 466, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6069, 467, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6070, 469, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6071, 471, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6072, 472, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6073, 473, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6074, 488, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6075, 862, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6076, 1077, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6077, 458, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6078, 474, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6079, 476, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6080, 477, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6081, 478, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6082, 480, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6083, 481, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6084, 482, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6085, 483, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6086, 484, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6087, 485, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6088, 840, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6089, 1070, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6090, 90, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6091, 814, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6092, 815, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6093, 816, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6094, 817, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6095, 819, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6096, 826, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6097, 827, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6098, 828, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6099, 829, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6100, 830, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6101, 831, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6102, 1141, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6103, 457, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6104, 540, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6105, 541, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6106, 542, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6107, 543, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6108, 546, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6109, 547, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6110, 548, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6111, 549, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6112, 551, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6113, 552, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6114, 836, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6115, 1066, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6116, 459, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6117, 844, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6118, 845, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6119, 846, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6120, 847, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6121, 849, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6122, 856, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6123, 857, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6124, 858, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6125, 859, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6126, 860, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6127, 861, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6128, 1142, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6129, -4, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6130, 1248, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6131, 87, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6132, 88, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6133, 525, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6134, 527, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6135, 528, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6136, 529, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6137, 531, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6138, 532, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6139, 533, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6140, 535, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6141, 536, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6142, 538, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6143, 790, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6144, 1078, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6145, 460, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6146, 796, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6147, 797, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6148, 798, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6149, 799, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6150, 801, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6151, 808, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6152, 809, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6153, 810, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6154, 811, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6155, 812, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6156, 813, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6157, 1140, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6158, 455, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6159, 491, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6160, 492, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6161, 493, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6162, 494, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6163, 599, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6164, 601, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6165, 602, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6166, 603, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6167, 606, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6168, 608, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6169, 793, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6170, 1079, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6171, -5, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6172, 1249, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6173, 1251, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6174, 1290, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6175, 1291, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6176, 1292, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6177, 1293, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6178, 1295, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6179, 1296, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6180, 91, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6181, 92, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6182, 254, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6183, 255, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6184, 256, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6185, 865, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6186, 1143, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6187, 93, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6188, 568, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6189, 569, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6190, 570, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6191, 571, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6192, 572, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6193, 879, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6194, 1144, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6195, 96, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6196, 633, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6197, 635, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6198, 637, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6199, 639, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6200, 640, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6201, 641, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6202, 642, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6203, 643, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6204, 644, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6205, 724, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6206, 730, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6207, 1080, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6208, 84, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6209, 77, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6210, 375, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6211, 376, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6212, 377, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6213, 775, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6214, 776, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6215, 1081, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6216, 83, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6217, 574, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6218, 575, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6219, 577, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6220, 779, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6221, 780, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6222, 1082, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6223, 86, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6224, 578, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6225, 579, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6226, 581, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6227, 583, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6228, 781, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6229, 782, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6230, 1083, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6231, 610, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6232, 611, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6233, 612, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6234, 613, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6235, 786, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6236, 787, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6237, 1084, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6238, 1091, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6239, 1093, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6240, 1094, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6241, 1095, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6242, 1096, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6243, 1098, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6244, 1139, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6245, 1203, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6246, 1204, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6247, 1205, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6248, 1206, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6249, 1207, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6250, 68, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6251, 69, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6252, 620, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6253, 621, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6254, 622, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6255, 763, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6256, 764, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6257, 1085, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6258, 71, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6259, 106, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6260, 107, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6261, 109, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6262, 113, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6263, 114, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6264, 115, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6265, 116, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6266, 117, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6267, 118, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6268, 253, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6269, 768, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6270, 1137, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6271, 2, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6272, 3, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6273, 265, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6274, 267, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6275, 268, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6276, 270, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6277, 271, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6278, 272, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6279, 713, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6280, 731, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6281, 732, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6282, 1061, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6283, 1369, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6284, 1379, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6285, 1381, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6286, 4, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6287, 597, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6288, 598, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6289, 734, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6290, 735, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6291, 736, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6292, 1136, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6293, 1374, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6294, 1375, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6295, 5, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6296, 592, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6297, 593, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6298, 750, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6299, 751, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6300, 752, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6301, 1089, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6302, 1090, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6303, -1, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6304, 1250, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6305, 168, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6306, 169, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6307, 416, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6308, 417, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6309, 419, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6310, 425, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6311, 961, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6312, 1063, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6313, 1383, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6314, 1384, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6315, 1385, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6316, 1386, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6317, 1387, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6318, 1388, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6319, 1389, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6320, 1391, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6321, 1392, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6322, 1393, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6323, 719, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6324, 964, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6325, 965, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6326, 966, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6327, 967, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6328, 969, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6329, 976, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6330, 977, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6331, 978, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6332, 979, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6333, 980, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6334, 981, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6335, 1072, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6336, 720, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6337, 982, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6338, 983, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6339, 984, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6340, 985, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6341, 987, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6342, 994, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6343, 995, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6344, 996, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6345, 997, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6346, 998, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6347, 999, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6348, 1074, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6349, 1075, 1, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6793, -2, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6794, 1169, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6795, 99, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6796, 100, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6797, 893, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6798, 1145, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6799, 104, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6800, 147, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6801, 148, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6802, 149, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6803, 150, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6804, 1170, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6805, 103, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6806, 105, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6807, 152, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6808, 153, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6809, 154, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6810, 155, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6811, 156, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6812, 157, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6813, 197, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6814, 198, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6815, 199, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6816, 264, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6817, 714, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6818, 1058, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6819, 74, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6820, 179, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6821, 180, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6822, 181, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6823, 183, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6824, 185, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6825, 187, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6826, 188, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6827, 189, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6828, 190, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6829, 279, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6830, 715, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6831, 1147, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6832, 82, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6833, 215, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6834, 217, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6835, 218, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6836, 219, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6837, 222, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6838, 223, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6839, 224, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6840, 225, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6841, 226, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6842, 277, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6843, 716, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6844, 1148, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6845, 1092, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6846, 1099, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6847, 1100, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6848, 1101, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6849, 1102, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6850, 1104, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6851, 1111, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6852, 1112, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6853, 1113, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6854, 1114, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6855, 1115, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6856, 1116, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6857, 1149, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6858, 1000, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6859, 158, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6860, 203, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6861, 204, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6862, 205, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6863, 207, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6864, 208, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6865, 210, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6866, 212, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6867, 213, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6868, 214, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6869, 261, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6870, 717, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6871, 1159, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6872, 159, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6873, 227, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6874, 228, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6875, 229, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6876, 230, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6877, 234, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6878, 235, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6879, 236, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6880, 237, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6881, 238, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6882, 273, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6883, 718, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6884, 1160, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6885, 76, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6886, 313, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6887, 315, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6888, 316, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6889, 317, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6890, 318, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6891, 319, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6892, 370, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6893, 936, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6894, 1158, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6895, 1003, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6896, 1021, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6897, 1022, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6898, 1023, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6899, 1028, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6900, 1030, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6901, 1031, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6902, 1032, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6903, 1033, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6904, 1034, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6905, 1035, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6906, 1125, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6907, 1161, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6908, 1129, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6909, 1130, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6910, 1131, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6911, 1132, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6912, 1133, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6913, 1135, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6914, 1162, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6915, 151, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6916, 160, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6917, 239, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6918, 240, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6919, 241, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6920, 244, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6921, 245, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6922, 247, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6923, 248, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6924, 249, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6925, 250, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6926, 307, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6927, 721, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6928, 1065, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6929, 78, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6930, 320, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6931, 321, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6932, 322, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6933, 326, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6934, 327, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6935, 328, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6936, 329, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6937, 330, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6938, 331, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6939, 333, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6940, 939, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6941, 1150, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6942, 161, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6943, 341, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6944, 342, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6945, 343, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6946, 344, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6947, 345, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6948, 346, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6949, 362, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6950, 942, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6951, 1151, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6952, 162, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6953, 347, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6954, 348, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6955, 349, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6956, 353, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6957, 354, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6958, 355, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6959, 356, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6960, 357, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6961, 359, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6962, 361, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6963, 945, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6964, 1067, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6965, 1004, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6966, 1006, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6967, 1007, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6968, 1008, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6969, 1013, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6970, 1014, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6971, 1016, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6972, 1017, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6973, 1018, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6974, 1019, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6975, 1020, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6976, 1117, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6977, 1152, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6978, 163, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6979, 292, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6980, 378, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6981, 380, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6982, 381, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6983, 382, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6984, 384, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6985, 385, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6986, 386, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6987, 387, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6988, 389, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6989, 390, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6990, 957, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6991, 1156, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6992, 164, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6993, 392, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6994, 393, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6995, 395, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6996, 396, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6997, 398, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6998, 399, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (6999, 400, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7000, 402, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7001, 403, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7002, 433, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7003, 948, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7004, 1153, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7005, 166, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7006, 434, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7007, 435, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7008, 436, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7009, 439, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7010, 441, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7011, 442, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7012, 443, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7013, 444, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7014, 445, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7015, 447, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7016, 954, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7017, 1155, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7018, 165, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7019, 404, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7020, 405, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7021, 406, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7022, 407, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7023, 409, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7024, 410, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7025, 411, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7026, 412, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7027, 415, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7028, 451, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7029, 951, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7030, 1154, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7031, 1005, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7032, 1041, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7033, 1043, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7034, 1044, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7035, 1048, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7036, 1049, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7037, 1050, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7038, 1051, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7039, 1052, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7040, 1053, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7041, 1054, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7042, 1121, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7043, 1157, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7044, 1251, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7045, 1290, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7046, 1291, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7047, 1292, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7048, 1293, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7049, 1295, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7050, 1296, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7051, -1, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7052, 1250, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7053, 168, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7054, 169, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7055, 416, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7056, 417, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7057, 419, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7058, 425, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7059, 961, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7060, 1063, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7061, 1383, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7062, 1384, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7063, 1385, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7064, 1386, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7065, 1387, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7066, 1388, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7067, 1389, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7068, 1391, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7069, 1392, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7070, 1393, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7071, 719, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7072, 964, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7073, 965, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7074, 966, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7075, 967, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7076, 969, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7077, 976, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7078, 977, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7079, 978, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7080, 979, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7081, 980, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7082, 981, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7083, 1072, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7084, 720, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7085, 982, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7086, 983, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7087, 984, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7088, 985, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7089, 987, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7090, 994, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7091, 995, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7092, 996, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7093, 997, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7094, 998, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7095, 999, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7096, 1074, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (7097, 1075, 5, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9255, 0, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9256, -2, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9257, 1246, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9258, 1252, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9259, 1172, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9260, 1173, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9261, 1174, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9262, 1175, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9263, 1176, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9264, 1178, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9265, 1179, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9266, 1273, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9267, 1274, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9268, 1275, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9269, 1276, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9270, 1277, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9271, 1278, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9272, 1329, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9273, 1169, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9274, 1198, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9275, 1245, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9276, 1264, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9277, 1263, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9278, 1224, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9279, 1225, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9280, 1226, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9281, 1227, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9282, 1228, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9283, 1230, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9284, 1231, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9285, 1238, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9286, 1239, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9287, 1240, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9288, 1241, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9289, 1242, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9290, 1243, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9291, 99, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9292, 100, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9293, 893, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9294, 1145, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9295, 1318, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9296, 1322, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9297, 1330, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9298, 1331, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9299, 1333, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9300, 1334, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9301, 1335, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9302, 104, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9303, 147, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9304, 148, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9305, 149, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9306, 150, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9307, 1337, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9308, 1370, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9309, 1170, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9310, 1254, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9311, 1255, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9312, 1256, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9313, 1257, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9314, 1258, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9315, 1259, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9316, 1260, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9317, 1261, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9318, 1262, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9319, 1287, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9320, 1338, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9321, 1340, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9322, 1341, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9323, 1342, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9324, 1344, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9325, 103, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9326, 82, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9327, 215, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9328, 217, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9329, 218, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9330, 219, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9331, 222, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9332, 223, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9333, 224, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9334, 225, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9335, 226, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9336, 277, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9337, 716, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9338, 1148, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9339, 1092, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9340, 1099, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9341, 1100, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9342, 1101, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9343, 1102, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9344, 1104, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9345, 1111, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9346, 1112, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9347, 1113, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9348, 1114, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9349, 1115, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9350, 1116, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9351, 1149, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9352, 1000, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9353, 158, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9354, 203, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9355, 204, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9356, 205, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9357, 207, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9358, 208, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9359, 210, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9360, 212, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9361, 213, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9362, 214, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9363, 261, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9364, 717, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9365, 1159, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9366, 159, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9367, 227, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9368, 228, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9369, 229, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9370, 230, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9371, 234, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9372, 235, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9373, 236, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9374, 237, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9375, 238, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9376, 273, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9377, 718, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9378, 1160, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9379, 76, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9380, 313, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9381, 315, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9382, 316, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9383, 317, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9384, 318, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9385, 319, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9386, 370, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9387, 936, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9388, 1158, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9389, 1211, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9390, 1212, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9391, 1213, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9392, 1003, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9393, 1021, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9394, 1022, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9395, 1023, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9396, 1028, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9397, 1030, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9398, 1031, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9399, 1032, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9400, 1033, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9401, 1034, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9402, 1035, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9403, 1125, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9404, 1161, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9405, 151, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9406, 160, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9407, 239, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9408, 240, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9409, 241, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9410, 244, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9411, 245, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9412, 247, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9413, 248, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9414, 249, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9415, 250, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9416, 307, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9417, 721, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9418, 1065, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9419, 78, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9420, 320, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9421, 321, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9422, 322, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9423, 326, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9424, 327, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9425, 328, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9426, 329, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9427, 330, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9428, 331, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9429, 333, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9430, 939, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9431, 1150, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9432, 161, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9433, 341, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9434, 342, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9435, 343, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9436, 344, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9437, 345, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9438, 346, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9439, 362, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9440, 942, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9441, 1151, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9442, 1217, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9443, 1218, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9444, 1219, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9445, 162, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9446, 347, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9447, 348, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9448, 349, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9449, 353, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9450, 354, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9451, 355, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9452, 356, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9453, 357, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9454, 359, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9455, 361, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9456, 945, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9457, 1067, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9458, 1004, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9459, 1006, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9460, 1007, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9461, 1008, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9462, 1013, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9463, 1014, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9464, 1016, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9465, 1017, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9466, 1018, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9467, 1019, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9468, 1020, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9469, 1117, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9470, 1152, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9471, 163, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9472, 292, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9473, 378, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9474, 380, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9475, 381, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9476, 382, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9477, 384, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9478, 385, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9479, 386, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9480, 387, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9481, 389, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9482, 390, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9483, 957, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9484, 1156, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9485, 164, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9486, 392, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9487, 393, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9488, 395, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9489, 396, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9490, 398, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9491, 399, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9492, 400, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9493, 402, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9494, 403, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9495, 433, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9496, 948, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9497, 1153, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9498, 166, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9499, 434, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9500, 435, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9501, 436, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9502, 439, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9503, 441, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9504, 442, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9505, 443, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9506, 444, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9507, 445, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9508, 447, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9509, 954, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9510, 1155, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9511, 165, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9512, 404, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9513, 405, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9514, 406, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9515, 407, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9516, 409, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9517, 410, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9518, 411, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9519, 412, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9520, 415, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9521, 451, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9522, 951, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9523, 1154, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9524, 1005, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9525, 1041, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9526, 1043, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9527, 1044, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9528, 1048, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9529, 1049, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9530, 1050, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9531, 1051, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9532, 1052, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9533, 1053, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9534, 1054, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9535, 1121, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9536, 1157, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9537, -3, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9538, 1247, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9539, 90, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9540, 814, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9541, 815, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9542, 816, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9543, 817, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9544, 819, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9545, 826, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9546, 827, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9547, 828, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9548, 829, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9549, 830, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9550, 831, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9551, 1141, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9552, -4, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9553, 1248, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9554, 88, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9555, 525, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9556, 527, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9557, 528, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9558, 529, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9559, 531, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9560, 532, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9561, 533, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9562, 535, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9563, 536, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9564, 538, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9565, 790, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9566, 1078, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9567, 455, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9568, 491, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9569, 492, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9570, 493, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9571, 494, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9572, 599, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9573, 601, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9574, 602, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9575, 603, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9576, 606, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9577, 608, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9578, 793, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9579, 1079, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9580, 1251, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9581, 1290, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9582, 1291, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9583, 1292, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9584, 1293, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9585, 1295, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9586, 1296, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9587, -1, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9588, 1250, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9589, 169, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9590, 416, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9591, 417, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9592, 419, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9593, 425, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9594, 961, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9595, 1063, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9596, 1383, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9597, 1384, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9598, 1385, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9599, 1386, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9600, 1387, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9601, 1388, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9602, 1389, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9603, 1391, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9604, 1392, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9605, 1393, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9606, 719, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9607, 964, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9608, 965, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9609, 966, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9610, 967, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9611, 969, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9612, 976, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9613, 977, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9614, 978, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9615, 979, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9616, 980, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9617, 981, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9618, 1072, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9619, 720, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9620, 982, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9621, 983, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9622, 984, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9623, 985, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9624, 987, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9625, 994, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9626, 995, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9627, 996, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9628, 997, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9629, 998, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9630, 999, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9631, 1074, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (9632, 1075, 10, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10898, 11, 9, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10899, 254, 9, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10900, 255, 9, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10901, 256, 9, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10902, 865, 9, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10903, 1143, 9, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10904, 1402, 9, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10982, 568, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10983, 572, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10984, 1374, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10985, 1375, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10986, 265, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10987, 267, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10988, 268, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10989, 713, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10990, 1369, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10991, 1396, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (10992, 1398, 11, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11126, 1451, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11127, 1456, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11128, 1457, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11129, 1458, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11130, 1459, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11131, 1460, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11132, 1462, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11133, 1463, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11134, 1464, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11135, 1467, 7, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11341, 0, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11343, 1422, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11344, 1423, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11345, 1426, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11346, 1428, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11347, 1435, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11349, 1451, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11350, 1456, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11351, 1457, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11352, 1469, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11353, 1470, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11354, 1471, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11355, 1472, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11356, 1474, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11357, 1458, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11358, 1459, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11359, 1460, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11360, 1462, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11361, 1463, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11362, 1464, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11363, 1467, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11364, 1475, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11365, 1476, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11366, 1477, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11367, 1478, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11368, 1479, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11369, 1480, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11371, 1466, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11372, 1468, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11374, 10, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11375, 11, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11376, 254, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11377, 255, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11378, 256, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11379, 865, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11380, 1143, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11381, 12, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11382, 568, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11383, 569, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11384, 570, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11385, 571, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11386, 572, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11387, 879, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11388, 1144, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11389, 13, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11390, 633, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11391, 635, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11392, 637, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11393, 639, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11394, 640, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11395, 641, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11396, 642, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11397, 643, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11398, 644, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11399, 724, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11400, 730, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11401, 1080, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11403, 2, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11404, 4, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11405, 597, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11406, 598, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11407, 734, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11408, 735, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11409, 736, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11410, 1136, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11411, 1374, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11412, 1375, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11414, 1411, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11415, 3, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11416, 265, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11417, 267, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11418, 268, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11419, 270, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11420, 271, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11421, 272, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11422, 713, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11423, 731, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11424, 732, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11425, 1061, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11426, 1369, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11427, 1396, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11428, 1398, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11430, 1412, 2, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11431, 1422, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11432, 1423, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11433, 1426, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11434, 1428, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11435, 1435, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11436, 1466, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11437, 10, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11438, 11, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11439, 254, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11440, 255, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11441, 256, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11442, 865, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11443, 1143, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11444, 12, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11445, 568, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11446, 569, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11447, 570, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11448, 571, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11449, 572, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11450, 879, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11451, 1144, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11452, 13, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11453, 633, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11454, 635, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11455, 637, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11456, 639, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11457, 640, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11458, 641, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11459, 642, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11460, 643, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11461, 644, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11462, 724, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11463, 730, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11464, 1080, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11465, 2, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11466, 4, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11467, 597, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11468, 598, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11469, 734, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11470, 735, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11471, 736, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11472, 1136, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11473, 1374, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11474, 1375, 13, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11475, 1451, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11476, 1456, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11477, 1457, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11478, 1469, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11479, 1470, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11480, 1471, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11481, 1472, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11482, 1473, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11483, 1474, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11484, 1458, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11485, 1459, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11486, 1460, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11487, 1462, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11488, 1463, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11489, 1464, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11490, 1467, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11491, 1475, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11492, 1476, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11493, 1477, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11494, 1478, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11495, 1479, 8, 1, 0);
INSERT INTO `tb_auth_role_power` VALUES (11496, 1480, 8, 1, 0);

-- ----------------------------
-- Table structure for tb_auth_user_device
-- ----------------------------
DROP TABLE IF EXISTS `tb_auth_user_device`;
CREATE TABLE `tb_auth_user_device`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `device_info` text CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '设备的详细信息',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '新增日期',
  `user_id` bigint(20) NOT NULL COMMENT '用户Id',
  `device_id` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '设备识别码',
  `os` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '设备操作系统',
  `app` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '浏览器',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  INDEX `DeviceId_Index`(`device_id`) USING BTREE,
  INDEX `UserId_Index`(`user_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 13 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '用户设备信息' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_auth_user_device
-- ----------------------------
INSERT INTO `tb_auth_user_device` VALUES (1, NULL, '2019-03-21 21:49:35', 0, '*X031VVUP_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (2, NULL, '2019-03-21 21:49:40', 0, '*QJLUN8MY_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (3, NULL, '2019-03-21 21:49:41', 0, '*PBZ3TYA2_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (4, NULL, '2019-03-21 21:49:44', 0, '*BCGVPWZD_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (5, NULL, '2019-03-21 21:52:03', 0, '*D0X1iVJP_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (6, NULL, '2019-03-21 21:52:17', 0, '*6XK69YC8_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (7, NULL, '2019-03-21 21:54:09', 0, '*R2VPiCGW_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (8, NULL, '2019-03-21 21:54:11', 0, '*8S1Z7HQQ_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (9, NULL, '2019-03-21 21:54:33', 0, '*N91HTGAD_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (10, NULL, '2019-03-28 15:59:57', 0, '*GDiN8UHK_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (11, NULL, '2019-03-28 20:47:08', 0, '*ZL4S2U8X_APP_LINUX', 'LINUX', 'APP');
INSERT INTO `tb_auth_user_device` VALUES (12, NULL, '2019-03-30 15:41:55', 0, '*9D7CPSQC_APP_LINUX', 'LINUX', 'APP');

-- ----------------------------
-- Table structure for tb_auth_user_token
-- ----------------------------
DROP TABLE IF EXISTS `tb_auth_user_token`;
CREATE TABLE `tb_auth_user_token`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `user_device_id` bigint(20) NOT NULL COMMENT 'UserDeviceId',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT 'AddDate',
  `user_id` bigint(20) NOT NULL COMMENT '用户标识',
  `device_id` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '设备标识',
  `access_token` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '访问令牌',
  `refresh_token` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '刷新令牌',
  `access_token_expires_time` datetime(0) NULL DEFAULT NULL COMMENT '访问令牌过期时间',
  `refresh_token_expires_time` datetime(0) NULL DEFAULT NULL COMMENT '刷新令牌过期时间',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  INDEX `UserId_Index`(`user_id`) USING BTREE,
  INDEX `AccessToken_Index`(`access_token`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 29 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '用户令牌' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_auth_user_token
-- ----------------------------
INSERT INTO `tb_auth_user_token` VALUES (1, 9, '2019-03-22 12:13:36', 1, '*N91HTGAD_APP_LINUX', '#59E446DA3F3149ACB2B3B532C8ADBFDF', '7E9BF63CEFA74FDBAA0D727DF21DD498', '2019-03-22 14:13:36', '2019-03-25 12:13:36');
INSERT INTO `tb_auth_user_token` VALUES (2, 9, '2019-03-22 14:30:45', 1, '*N91HTGAD_APP_LINUX', '#D127BCF5E1CE4FAFB9835363CF6B19B4', 'F3BD670CB19947A1A2FCC5A1443DF0CB', '2019-03-22 16:30:45', '2019-03-25 14:30:45');
INSERT INTO `tb_auth_user_token` VALUES (3, 10, '2019-03-28 15:59:57', 1, '*GDiN8UHK_APP_LINUX', '#5F4A4D79CC16409E8F15A9652C732982', '1821317711D9428190F9B6675D604FC6', '2019-03-28 17:59:57', '2019-03-31 15:59:57');
INSERT INTO `tb_auth_user_token` VALUES (4, 11, '2019-03-28 20:49:54', 1, '*ZL4S2U8X_APP_LINUX', '#AFF254C74B514A638DA442067760C6AD', '8BAF1CCB9192446DA01558070DA9D76D', '2019-03-28 22:49:54', '2019-03-31 20:49:54');
INSERT INTO `tb_auth_user_token` VALUES (5, 11, '2019-03-29 09:32:30', 1, '*ZL4S2U8X_APP_LINUX', '#7E767C5425B14D3790A22143BA0738C6', 'F17FBCB5085341928C069D91F8D1601E', '2019-03-29 11:32:30', '2019-04-01 09:32:30');
INSERT INTO `tb_auth_user_token` VALUES (6, 11, '2019-03-29 11:45:37', 1, '*ZL4S2U8X_APP_LINUX', '#FD3D5CC7F8144137B918EBB6030C71FD', 'F887499748854E3499DFA46610EE452F', '2019-03-29 13:45:37', '2019-04-01 11:45:37');
INSERT INTO `tb_auth_user_token` VALUES (7, 11, '2019-03-29 13:49:51', 1, '*ZL4S2U8X_APP_LINUX', '#1760E0A285014E7592ACD93C34D79DA5', 'CA10CF362C054E1C966F939F6F4E2C9C', '2019-03-29 15:49:51', '2019-04-01 13:49:51');
INSERT INTO `tb_auth_user_token` VALUES (8, 11, '2019-03-29 16:20:58', 1, '*ZL4S2U8X_APP_LINUX', '#1DF0C292810C4C22B89EA38698FF18B7', '51C6159978E746D48077478D20B85264', '2019-03-29 18:20:58', '2019-04-01 16:20:58');
INSERT INTO `tb_auth_user_token` VALUES (9, 11, '2019-03-30 09:49:08', 1, '*ZL4S2U8X_APP_LINUX', '#62C35799135444799AC2CCF10F03A22D', 'EC1331A406ED433DA759F8AA5C555916', '2019-03-30 11:49:08', '2019-04-02 09:49:08');
INSERT INTO `tb_auth_user_token` VALUES (10, 11, '2019-03-30 11:55:24', 1, '*ZL4S2U8X_APP_LINUX', '#F513C804CC7742F1ACBBF7FAC9E9BC0C', '890EAA96B1CF46B0BE88D6B65D43F559', '2019-03-30 13:55:24', '2019-04-02 11:55:24');
INSERT INTO `tb_auth_user_token` VALUES (11, 11, '2019-03-30 15:24:34', 1, '*ZL4S2U8X_APP_LINUX', '#1B46C9C722E84566BA6B712DA02D0263', 'F9BB8F1E109B4C36BFCFF09CF4248113', '2019-03-30 17:24:34', '2019-04-02 15:24:34');
INSERT INTO `tb_auth_user_token` VALUES (12, 11, '2019-03-30 15:24:57', 1, '*ZL4S2U8X_APP_LINUX', '#74A63BF5F6E84B7088339B6FA0E61D74', '4E844D62F1E149C28DEF2B729A8541E6', '2019-03-30 17:24:57', '2019-04-02 15:24:57');
INSERT INTO `tb_auth_user_token` VALUES (13, 12, '2019-03-30 15:41:59', 1, '*9D7CPSQC_APP_LINUX', '#61B65036716B401B8B72EF8C8012B4C1', '5800EC8C45DA4ED290D7C37C8949923F', '2019-03-30 17:41:59', '2019-04-02 15:41:59');
INSERT INTO `tb_auth_user_token` VALUES (14, 11, '2019-03-30 17:45:51', 1, '*ZL4S2U8X_APP_LINUX', '#A06804EDE624448AB9E05AD3C67ECF39', 'A8615ABEAC19441D991AF20F1F96224C', '2019-03-30 19:45:51', '2019-04-02 17:45:51');
INSERT INTO `tb_auth_user_token` VALUES (15, 11, '2019-03-30 20:15:16', 1, '*ZL4S2U8X_APP_LINUX', '#FA2321EE7B7D4D98A36572BA77A8F810', '830698869E9748F6AE15C350BB631AD5', '2019-03-30 22:15:16', '2019-04-02 20:15:16');
INSERT INTO `tb_auth_user_token` VALUES (16, 11, '2019-03-31 09:17:49', 1, '*ZL4S2U8X_APP_LINUX', '#6AABCB2C0AAA452FBBA6492186CA4811', '01225BBA8A1E4B00AAC2CFC69B7F53EA', '2019-03-31 11:17:49', '2019-04-03 09:17:49');
INSERT INTO `tb_auth_user_token` VALUES (17, 11, '2019-03-31 11:18:36', 1, '*ZL4S2U8X_APP_LINUX', '#A41A25EB999B42CCB56A59C3E0E36256', 'EF43C88CD94249DA95D3711E89353546', '2019-03-31 13:18:36', '2019-04-03 11:18:36');
INSERT INTO `tb_auth_user_token` VALUES (18, 11, '2019-03-31 12:13:31', 1, '*ZL4S2U8X_APP_LINUX', '#2BB830470EA0431A9E47A17480488814', 'BF29AB094B0F4F13B659868524F67483', '2019-03-31 14:13:31', '2019-04-03 12:13:31');
INSERT INTO `tb_auth_user_token` VALUES (19, 11, '2019-03-31 14:14:43', 1, '*ZL4S2U8X_APP_LINUX', '#7E7080DC66F044A0A84A1B0FBF973A3A', '623516A8FB5B4E89B446C0244F1575F0', '2019-03-31 16:14:43', '2019-04-03 14:14:43');
INSERT INTO `tb_auth_user_token` VALUES (20, 11, '2019-03-31 16:18:58', 1, '*ZL4S2U8X_APP_LINUX', '#DCD90851572D4EA9A4EE6AA3C39CE566', 'E7F2B9FAEDEA43C8AFF326A06BDEE2E6', '2019-03-31 18:18:58', '2019-04-03 16:18:58');
INSERT INTO `tb_auth_user_token` VALUES (21, 12, '2019-03-31 16:49:02', 1, '*9D7CPSQC_APP_LINUX', '#5C3734E23FD24CF5AED630402604D4E6', '1C74881226144DD28C996CCF5D0EF8E9', '2019-03-31 18:49:02', '2019-04-03 16:49:02');
INSERT INTO `tb_auth_user_token` VALUES (22, 11, '2019-03-31 19:10:34', 1, '*ZL4S2U8X_APP_LINUX', '#5213175DD37A47B49D4FA4FB16B05251', '61E8D8FF9A8145A4B861DB272D7FA7DF', '2019-03-31 21:10:34', '2019-04-03 19:10:34');
INSERT INTO `tb_auth_user_token` VALUES (23, 11, '2019-03-31 21:17:54', 1, '*ZL4S2U8X_APP_LINUX', '#50D9A5C10487424182242CD04807CCE9', '26FD1D52FA05465AA11750D15656B2EF', '2019-03-31 23:17:54', '2019-04-03 21:17:54');
INSERT INTO `tb_auth_user_token` VALUES (24, 11, '2019-04-01 09:21:57', 1, '*ZL4S2U8X_APP_LINUX', '#738257AD1F9149B0BD3F43A628341CAB', '2C2154A714CE4BD89F6A760AA1075577', '2019-04-01 11:21:57', '2019-04-04 09:21:57');
INSERT INTO `tb_auth_user_token` VALUES (25, 11, '2019-04-01 11:35:21', 1, '*ZL4S2U8X_APP_LINUX', '#10D484BA40BE40838FF98221F4E7C5AD', '2A08496E60014C728C02E3034E31C1DA', '2019-04-01 13:35:21', '2019-04-04 11:35:21');
INSERT INTO `tb_auth_user_token` VALUES (26, 11, '2019-04-01 14:03:13', 1, '*ZL4S2U8X_APP_LINUX', '#938AB20FA3B0446E91BB72141C4D1D98', 'E22B1B9C1DA54EC9AD286CFEAB71B967', '2019-04-01 16:03:13', '2019-04-04 14:03:13');
INSERT INTO `tb_auth_user_token` VALUES (27, 11, '2019-04-01 14:26:53', 1, '*ZL4S2U8X_APP_LINUX', '#8855412B9490436CAFBE2BFA6F152F6F', '0258E79E109F4AE2B02A8BAE5580E940', '2019-04-01 16:26:53', '2019-04-04 14:26:53');
INSERT INTO `tb_auth_user_token` VALUES (28, 11, '2019-04-01 16:39:27', 1, '*ZL4S2U8X_APP_LINUX', '#3010FC57924246FD88AA51E3DDD1E489', '6F14575EF40E40A7A21A605DC2292015', '2019-04-01 18:39:27', '2019-04-04 16:39:27');

-- ----------------------------
-- Table structure for tb_org_organization
-- ----------------------------
DROP TABLE IF EXISTS `tb_org_organization`;
CREATE TABLE `tb_org_organization`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `type` int(11) NOT NULL COMMENT '机构类型',
  `code` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '编码',
  `full_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '全称',
  `short_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '简称',
  `tree_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '树形名称',
  `org_level` int(11) NOT NULL COMMENT '级别',
  `level_index` int(11) NOT NULL DEFAULT 0 COMMENT '层级的序号',
  `parent_id` bigint(20) NOT NULL COMMENT '上级标识',
  `tel` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `memo` text CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '备注',
  `boundary_id` bigint(20) NULL DEFAULT 0 COMMENT '所属机构标识',
  `group_id` bigint(20) NULL DEFAULT NULL COMMENT '所在集团标识',
  `is_freeze` tinyint(1) NOT NULL DEFAULT 0 COMMENT '冻结更新',
  `data_state` int(11) NOT NULL DEFAULT 0 COMMENT '数据状态',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '制作时间',
  `last_reviser_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '最后修改者',
  `last_modify_date` datetime(0) NULL DEFAULT NULL COMMENT '最后修改日期',
  `author_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '制作人',
  `auditor_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '审核人',
  `audit_date` datetime(0) NULL DEFAULT NULL COMMENT '审核时间',
  `audit_state` int(11) NOT NULL DEFAULT 0 COMMENT '审核状态',
  `logo` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'Logo',
  `address` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'Address',
  `website` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'Website',
  `fax` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT 'Fax',
  `city_code` varchar(6) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '所在市级编码',
  `district_code` varchar(6) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '所在区县编码',
  `org_address` varchar(128) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '机构详细地址',
  `law_personname` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '机构负责人',
  `law_persontel` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '机构负责人电话',
  `contact_name` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '机构联系人',
  `contact_tel` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '机构联系人电话',
  `super_orgcode` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '上级机构代码',
  `manag_orgcode` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '注册管理机构代码',
  `manag_orgname` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '注册管理机构名称',
  `org_id` bigint(20) NULL DEFAULT 0,
  `area_id` bigint(20) NOT NULL COMMENT '行政区域外键',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  UNIQUE INDEX `id_2`(`id`) USING BTREE,
  INDEX `ShortName_Index`(`short_name`) USING BTREE,
  INDEX `IsFreeze_Index`(`is_freeze`) USING BTREE,
  INDEX `DataState_Index`(`data_state`) USING BTREE,
  INDEX `AddDate_Index`(`add_date`) USING BTREE,
  INDEX `LastReviserId_Index`(`last_reviser_id`) USING BTREE,
  INDEX `LastModifyDate_Index`(`last_modify_date`) USING BTREE,
  INDEX `AuthorId_Index`(`author_id`) USING BTREE,
  INDEX `AuditDate_Index`(`audit_date`) USING BTREE,
  INDEX `AuditState_Index`(`audit_state`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 45 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '机构' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_org_organization
-- ----------------------------
INSERT INTO `tb_org_organization` VALUES (44, 2, NULL, 'abc', 'abc', 'abc', 1, 0, 0, NULL, NULL, 44, NULL, 0, 0, '2019-03-03 01:02:59', 1, '2019-03-03 01:03:05', 1, 0, NULL, 0, NULL, NULL, NULL, NULL, '330301', '330301', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 2);

-- ----------------------------
-- Table structure for tb_org_organize_position
-- ----------------------------
DROP TABLE IF EXISTS `tb_org_organize_position`;
CREATE TABLE `tb_org_organize_position`  (
  `id` bigint(11) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `appellation` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '称谓',
  `department_id` bigint(11) NOT NULL COMMENT '机构标识',
  `role_id` bigint(20) NOT NULL COMMENT '角色外键',
  `memo` text CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '备注',
  `boundary_id` bigint(20) NOT NULL DEFAULT 0,
  `is_freeze` tinyint(1) NOT NULL DEFAULT 0 COMMENT '冻结更新',
  `data_state` int(11) NOT NULL DEFAULT 0 COMMENT '数据状态',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '制作时间',
  `last_reviser_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '最后修改者',
  `last_modify_date` datetime(0) NULL DEFAULT NULL COMMENT '最后修改日期',
  `author_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '制作人',
  `auditor_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '审核人',
  `audit_date` datetime(0) NULL DEFAULT NULL COMMENT '审核时间',
  `audit_state` int(11) NOT NULL DEFAULT 0 COMMENT '审核状态',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id_2`(`id`) USING BTREE,
  INDEX `id`(`id`) USING BTREE,
  INDEX `department_id`(`department_id`) USING BTREE,
  INDEX `is_freeze`(`is_freeze`, `data_state`) USING BTREE,
  INDEX `audit_state`(`audit_state`) USING BTREE,
  INDEX `Position_Index`(`appellation`) USING BTREE,
  INDEX `DepartmentId_Index`(`department_id`) USING BTREE,
  INDEX `RoleId_Index`(`role_id`) USING BTREE,
  INDEX `IsFreeze_Index`(`is_freeze`) USING BTREE,
  INDEX `DataState_Index`(`data_state`) USING BTREE,
  INDEX `AddDate_Index`(`add_date`) USING BTREE,
  INDEX `LastReviserId_Index`(`last_reviser_id`) USING BTREE,
  INDEX `LastModifyDate_Index`(`last_modify_date`) USING BTREE,
  INDEX `AuthorId_Index`(`author_id`) USING BTREE,
  INDEX `AuditDate_Index`(`audit_date`) USING BTREE,
  INDEX `AuditState_Index`(`audit_state`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '机构职位设置' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_org_organize_position
-- ----------------------------
INSERT INTO `tb_org_organize_position` VALUES (1, '系统管理员', 1, 2, NULL, 0, 0, 0, '2019-03-02 21:26:58', 1, '2019-03-02 21:50:28', 1, 0, NULL, 0);
INSERT INTO `tb_org_organize_position` VALUES (2, 'aaa', 44, 0, NULL, 0, 0, 0, '2019-03-03 01:17:55', 1, '2019-03-03 01:17:59', 1, 0, NULL, 0);
INSERT INTO `tb_org_organize_position` VALUES (3, 'c', 44, 0, NULL, 0, 0, 0, '2019-03-22 15:22:02', 1, '2019-03-22 15:22:01', 1, 0, NULL, 0);
INSERT INTO `tb_org_organize_position` VALUES (4, 'c', 44, 0, NULL, 0, 0, 0, '2019-03-22 16:07:39', 1, '2019-03-22 16:07:38', 1, 0, NULL, 0);

-- ----------------------------
-- Table structure for tb_org_position_personnel
-- ----------------------------
DROP TABLE IF EXISTS `tb_org_position_personnel`;
CREATE TABLE `tb_org_position_personnel`  (
  `id` bigint(11) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `appellation` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '称谓',
  `organize_position_id` bigint(11) NOT NULL COMMENT '职位标识',
  `user_id` bigint(11) NOT NULL COMMENT '员工标识',
  `role_id` bigint(20) NULL DEFAULT NULL,
  `memo` text CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '备注',
  `boundary_id` bigint(20) NOT NULL DEFAULT 0,
  `is_freeze` tinyint(1) NOT NULL DEFAULT 0 COMMENT '冻结更新',
  `data_state` int(11) NOT NULL DEFAULT 0 COMMENT '数据状态',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '制作时间',
  `last_reviser_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '最后修改者',
  `last_modify_date` datetime(0) NULL DEFAULT NULL COMMENT '最后修改日期',
  `author_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '制作人',
  `auditor_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '审核人',
  `audit_date` datetime(0) NULL DEFAULT NULL COMMENT '审核时间',
  `audit_state` int(11) NOT NULL DEFAULT 0 COMMENT '审核状态',
  `master_id` bigint(20) NULL DEFAULT NULL COMMENT '关联标识',
  `avatar_url` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL,
  `area_id` bigint(20) NOT NULL COMMENT '行政区域外键',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id_2`(`id`) USING BTREE,
  INDEX `id`(`id`) USING BTREE,
  INDEX `organize_position_id`(`organize_position_id`) USING BTREE,
  INDEX `user_id`(`user_id`) USING BTREE,
  INDEX `is_freeze`(`is_freeze`, `data_state`, `audit_state`) USING BTREE,
  INDEX `UserId_Index`(`user_id`) USING BTREE,
  INDEX `OrganizePositionId_Index`(`organize_position_id`) USING BTREE,
  INDEX `RoleId_Index`(`role_id`) USING BTREE,
  INDEX `IsFreeze_Index`(`is_freeze`) USING BTREE,
  INDEX `DataState_Index`(`data_state`) USING BTREE,
  INDEX `AddDate_Index`(`add_date`) USING BTREE,
  INDEX `LastReviserId_Index`(`last_reviser_id`) USING BTREE,
  INDEX `LastModifyDate_Index`(`last_modify_date`) USING BTREE,
  INDEX `AuthorId_Index`(`author_id`) USING BTREE,
  INDEX `AuditDate_Index`(`audit_date`) USING BTREE,
  INDEX `AuditState_Index`(`audit_state`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '人员职位设置' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_org_position_personnel
-- ----------------------------
INSERT INTO `tb_org_position_personnel` VALUES (1, NULL, 1, 867722619879428, 0, NULL, 0, 0, 255, '2019-03-02 22:49:25', 1, '2019-03-02 22:51:15', 1, 0, NULL, 0, NULL, NULL, 0);
INSERT INTO `tb_org_position_personnel` VALUES (2, NULL, 1, 867723303550981, 0, NULL, 0, 0, 0, '2019-03-02 22:50:59', 1, '2019-03-02 22:56:47', 1, 0, NULL, 0, NULL, NULL, 0);

-- ----------------------------
-- Table structure for tb_project_annex
-- ----------------------------
DROP TABLE IF EXISTS `tb_project_annex`;
CREATE TABLE `tb_project_annex`  (
  `id` int(11) NOT NULL AUTO_INCREMENT COMMENT '标识',
  `annex_type` int(11) NOT NULL COMMENT '附件类型',
  `entity_type` int(11) NOT NULL COMMENT '连接类型',
  `link_id` int(11) NOT NULL COMMENT '关联标识',
  `title` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '附件标题',
  `file_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '文件名称',
  `url` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '连接地址',
  `storage` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '存储地址',
  `memo` text CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '备注',
  `data_state` int(11) NOT NULL DEFAULT 0 COMMENT '数据状态',
  `is_freeze` tinyint(1) NOT NULL DEFAULT 0 COMMENT '数据是否已冻结',
  `is_public` tinyint(1) NULL DEFAULT 0,
  `author_id` int(11) NOT NULL DEFAULT 0 COMMENT '制作人',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '制作时间',
  `last_reviser_id` int(11) NOT NULL DEFAULT 0 COMMENT '最后修改者',
  `last_modify_date` datetime(0) NULL DEFAULT NULL COMMENT '最后修改日期',
  `area_id` int(11) NOT NULL COMMENT '区域标识',
  `department_id` int(11) NOT NULL DEFAULT 0 COMMENT '部门标识',
  `view_scope` int(11) NOT NULL DEFAULT 0 COMMENT '可视范围',
  `wx_media_id` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '微信的MediaId',
  `department_level` int(11) NOT NULL COMMENT '部门级别',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 82 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '附件' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_project_annex
-- ----------------------------
INSERT INTO `tb_project_annex` VALUES (1, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/YRJ9UVSD1B.bin', 'Anuexs\\None\\YRJ9UVSD1B.bin', NULL, 0, 0, 0, -2, '2019-03-29 10:15:12', -2, '2019-03-29 10:15:12', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (2, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/ELAGTW3QL2.bin', 'Anuexs\\None\\ELAGTW3QL2.bin', NULL, 0, 0, 0, -2, '2019-03-29 10:15:43', -2, '2019-03-29 10:15:43', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (3, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/Q8XVUWHYXL.bin', 'Anuexs\\None\\Q8XVUWHYXL.bin', NULL, 0, 0, 0, 1, '2019-03-29 10:21:34', 1, '2019-03-29 10:21:34', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (4, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/UZCE7SW8XX.bin', 'Anuexs\\None\\UZCE7SW8XX.bin', NULL, 0, 0, 0, 1, '2019-03-29 10:37:18', 1, '2019-03-29 10:37:18', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (5, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/N88ZMHZ21F.bin', 'Anuexs\\None\\N88ZMHZ21F.bin', NULL, 0, 0, 0, 1, '2019-03-29 10:43:13', 1, '2019-03-29 10:43:13', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (6, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/U7HGQVK1BT.bin', 'Anuexs\\None\\U7HGQVK1BT.bin', NULL, 0, 0, 0, 1, '2019-03-29 10:48:53', 1, '2019-03-29 10:48:53', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (7, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/7SS68WQURP.bin', 'Anuexs\\None\\7SS68WQURP.bin', NULL, 0, 0, 0, 1, '2019-03-29 12:15:39', 1, '2019-03-29 12:15:39', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (8, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/35ZD17Q65W.bin', 'Anuexs\\None\\35ZD17Q65W.bin', NULL, 0, 0, 0, 1, '2019-03-29 12:17:53', 1, '2019-03-29 12:17:53', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (9, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/D1N8BJVKQE.bin', 'Anuexs\\None\\D1N8BJVKQE.bin', NULL, 0, 0, 0, 1, '2019-03-29 12:18:46', 1, '2019-03-29 12:18:46', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (10, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/JMK4JLUBAH.bin', 'Anuexs\\None\\JMK4JLUBAH.bin', NULL, 0, 0, 0, 1, '2019-03-29 12:51:32', 1, '2019-03-29 12:51:32', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (11, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/JXiiJ7E6Z8.bin', 'Anuexs\\None\\JXiiJ7E6Z8.bin', NULL, 0, 0, 0, 1, '2019-03-29 12:55:14', 1, '2019-03-29 12:55:14', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (12, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/DXW0HHQSLT.bin', 'Anuexs\\None\\DXW0HHQSLT.bin', NULL, 0, 0, 0, 1, '2019-03-29 13:01:56', 1, '2019-03-29 13:01:56', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (13, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/SRGPKTUY39.bin', 'Anuexs\\None\\SRGPKTUY39.bin', NULL, 0, 0, 0, 1, '2019-03-29 14:03:11', 1, '2019-03-29 14:03:11', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (14, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/9ZHJBiAiL2.bin', 'Anuexs\\None\\9ZHJBiAiL2.bin', NULL, 0, 0, 0, 1, '2019-03-29 15:34:15', 1, '2019-03-29 15:34:15', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (15, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/0UA1JJ4YMU.bin', 'Anuexs\\None\\0UA1JJ4YMU.bin', NULL, 0, 0, 0, 1, '2019-03-29 17:58:37', 1, '2019-03-29 17:58:37', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (16, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/ZLNSR32CZH.bin', 'Anuexs\\None\\ZLNSR32CZH.bin', NULL, 0, 0, 0, 1, '2019-03-29 17:59:48', 1, '2019-03-29 17:59:48', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (17, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/BZEJ0iTiS2.bin', 'Anuexs\\None\\BZEJ0iTiS2.bin', NULL, 0, 0, 0, 1, '2019-03-29 18:00:04', 1, '2019-03-29 18:00:04', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (18, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/VW0YEZV5JX.bin', 'Anuexs\\None\\VW0YEZV5JX.bin', NULL, 0, 0, 0, 1, '2019-03-29 18:00:53', 1, '2019-03-29 18:00:53', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (19, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/444U5P0NB1.bin', 'Anuexs\\None\\444U5P0NB1.bin', NULL, 0, 0, 0, 1, '2019-03-30 09:56:23', 1, '2019-03-30 09:56:23', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (20, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/PU80BLDFi7.bin', 'Anuexs\\None\\PU80BLDFi7.bin', NULL, 0, 0, 0, 1, '2019-03-30 15:27:50', 1, '2019-03-30 15:27:50', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (21, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/0E513FJTR0.bin', 'Anuexs\\None\\0E513FJTR0.bin', NULL, 0, 0, 0, 1, '2019-03-30 15:31:57', 1, '2019-03-30 15:31:57', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (22, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/HQP2DEK3C7.bin', 'Anuexs\\None\\HQP2DEK3C7.bin', NULL, 0, 0, 0, -2, '2019-03-30 15:36:24', -2, '2019-03-30 15:36:24', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (23, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/DRC5Q05283.bin', 'Anuexs\\None\\DRC5Q05283.bin', NULL, 0, 0, 0, 1, '2019-03-30 15:46:07', 1, '2019-03-30 15:46:07', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (24, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/FUZFZE7NTP.bin', 'Anuexs\\None\\FUZFZE7NTP.bin', NULL, 0, 0, 0, 1, '2019-03-30 15:50:04', 1, '2019-03-30 15:50:04', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (25, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/3RVT77DMWG.bin', 'Anuexs\\None\\3RVT77DMWG.bin', NULL, 0, 0, 0, 1, '2019-03-30 17:47:45', 1, '2019-03-30 17:47:45', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (26, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/W7JLJWFY8V.bin', 'Anuexs\\None\\W7JLJWFY8V.bin', NULL, 0, 0, 0, 1, '2019-03-30 17:51:28', 1, '2019-03-30 17:51:28', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (27, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/KFF7BFPSCP.bin', 'Anuexs\\None\\KFF7BFPSCP.bin', NULL, 0, 0, 0, 1, '2019-03-30 17:52:46', 1, '2019-03-30 17:52:46', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (28, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/1B6P88MXCG.bin', 'Anuexs\\None\\1B6P88MXCG.bin', NULL, 0, 0, 0, 1, '2019-03-30 20:59:59', 1, '2019-03-30 20:59:59', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (29, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/UFKWB0CJB3.bin', 'Anuexs\\None\\UFKWB0CJB3.bin', NULL, 0, 0, 0, 1, '2019-03-30 21:09:08', 1, '2019-03-30 21:09:08', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (30, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/XZFL357XM4.bin', 'Anuexs\\None\\XZFL357XM4.bin', NULL, 0, 0, 0, 1, '2019-03-30 21:26:33', 1, '2019-03-30 21:26:33', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (31, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/VAiB5E7ZXZ.bin', 'Anuexs\\None\\VAiB5E7ZXZ.bin', NULL, 0, 0, 0, 1, '2019-03-30 21:27:44', 1, '2019-03-30 21:27:44', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (32, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/FiiDMLJMiE.bin', 'Anuexs\\None\\FiiDMLJMiE.bin', NULL, 0, 0, 0, 1, '2019-03-31 09:38:33', 1, '2019-03-31 09:38:33', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (33, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/URMVWZ191H.bin', 'Anuexs\\None\\URMVWZ191H.bin', NULL, 0, 0, 0, 1, '2019-03-31 09:45:28', 1, '2019-03-31 09:45:28', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (34, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/GWNF5KPiDX.bin', 'Anuexs\\None\\GWNF5KPiDX.bin', NULL, 0, 0, 0, 1, '2019-03-31 09:46:16', 1, '2019-03-31 09:46:16', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (35, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/817G7MG4V6.bin', 'Anuexs\\None\\817G7MG4V6.bin', NULL, 0, 0, 0, 1, '2019-03-31 09:51:33', 1, '2019-03-31 09:51:33', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (36, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/N1QF7HNXX0.bin', 'Anuexs\\None\\N1QF7HNXX0.bin', NULL, 0, 0, 0, 1, '2019-03-31 10:06:47', 1, '2019-03-31 10:06:47', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (37, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/VSiR0iRCEN.bin', 'Anuexs\\None\\VSiR0iRCEN.bin', NULL, 0, 0, 0, 1, '2019-03-31 10:27:26', 1, '2019-03-31 10:27:26', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (38, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/94iSTL6Fi1.bin', 'Anuexs\\None\\94iSTL6Fi1.bin', NULL, 0, 0, 0, 1, '2019-03-31 10:51:01', 1, '2019-03-31 10:51:01', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (39, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/MR16WR5EMN.bin', 'Anuexs\\None\\MR16WR5EMN.bin', NULL, 0, 0, 0, 1, '2019-03-31 11:59:04', 1, '2019-03-31 11:59:04', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (40, 0, 0, 0, 'sku_org_once.csv', 'sku_org_once.csv', '/Anuexs/None/E0QGL5Z628.bin', 'Anuexs\\None\\E0QGL5Z628.bin', NULL, 0, 0, 0, -2, '2019-03-31 12:02:23', -2, '2019-03-31 12:02:23', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (41, 0, 0, 0, 'sku_org_once.csv', 'sku_org_once.csv', '/Anuexs/None/TZDL9PXGLA.bin', 'Anuexs\\None\\TZDL9PXGLA.bin', NULL, 0, 0, 0, 1, '2019-03-31 12:07:29', 1, '2019-03-31 12:07:29', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (42, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/85KRZF34CM.bin', 'Anuexs\\None\\85KRZF34CM.bin', NULL, 0, 0, 0, 1, '2019-03-31 12:40:24', 1, '2019-03-31 12:40:24', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (43, 0, 0, 0, 'sku_org_once.csv', 'sku_org_once.csv', '/Anuexs/None/H7Wi1QL2QY.bin', 'Anuexs\\None\\H7Wi1QL2QY.bin', NULL, 0, 0, 0, 1, '2019-03-31 12:41:17', 1, '2019-03-31 12:41:17', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (44, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/7WSS1ZQNJA.bin', 'Anuexs\\None\\7WSS1ZQNJA.bin', NULL, 0, 0, 0, 1, '2019-03-31 12:44:02', 1, '2019-03-31 12:44:02', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (45, 0, 0, 0, 'sku_org_once.csv', 'sku_org_once.csv', '/Anuexs/None/YZBLCPTGPA.bin', 'Anuexs\\None\\YZBLCPTGPA.bin', NULL, 0, 0, 0, 1, '2019-03-31 12:46:26', 1, '2019-03-31 12:46:26', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (46, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/T5SWLWi4GM.bin', 'Anuexs\\None\\T5SWLWi4GM.bin', NULL, 0, 0, 0, 1, '2019-03-31 12:51:42', 1, '2019-03-31 12:51:42', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (47, 0, 0, 0, 'sku_org_once.csv', 'sku_org_once.csv', '/Anuexs/None/5UFDANR0WX.bin', 'Anuexs\\None\\5UFDANR0WX.bin', NULL, 0, 0, 0, 1, '2019-03-31 13:27:09', 1, '2019-03-31 13:27:09', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (48, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/Y8X31PGYZX.bin', 'Anuexs\\None\\Y8X31PGYZX.bin', NULL, 0, 0, 0, 1, '2019-03-31 14:15:08', 1, '2019-03-31 14:15:08', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (49, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/i3V8076ZZQ.bin', 'Anuexs\\None\\i3V8076ZZQ.bin', NULL, 0, 0, 0, 1, '2019-03-31 14:27:40', 1, '2019-03-31 14:27:40', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (50, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/JMZSJiWCHN.bin', 'Anuexs\\None\\JMZSJiWCHN.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:07:01', 1, '2019-03-31 15:07:01', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (51, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/PASC8X3iUU.bin', 'Anuexs\\None\\PASC8X3iUU.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:13:44', 1, '2019-03-31 15:13:44', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (52, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/998RH931HR.bin', 'Anuexs\\None\\998RH931HR.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:15:09', 1, '2019-03-31 15:15:09', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (53, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/VJ92DK9D77.bin', 'Anuexs\\None\\VJ92DK9D77.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:16:37', 1, '2019-03-31 15:16:37', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (54, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/CTU1ACA5iG.bin', 'Anuexs\\None\\CTU1ACA5iG.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:24:47', 1, '2019-03-31 15:24:47', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (55, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/iYKX0YKY9R.bin', 'Anuexs\\None\\iYKX0YKY9R.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:38:03', 1, '2019-03-31 15:38:03', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (56, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/0HTBYYND07.bin', 'Anuexs\\None\\0HTBYYND07.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:38:43', 1, '2019-03-31 15:38:43', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (57, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/DBGW84M5QF.bin', 'Anuexs\\None\\DBGW84M5QF.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:40:08', 1, '2019-03-31 15:40:08', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (58, 0, 0, 0, 'sku_org_once.csv', 'sku_org_once.csv', '/Anuexs/None/JX0UH85JT5.bin', 'Anuexs\\None\\JX0UH85JT5.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:41:06', 1, '2019-03-31 15:41:06', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (59, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/iSKXJRB5EN.bin', 'Anuexs\\None\\iSKXJRB5EN.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:52:08', 1, '2019-03-31 15:52:08', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (60, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/EP6ZiV797F.bin', 'Anuexs\\None\\EP6ZiV797F.bin', NULL, 0, 0, 0, 1, '2019-03-31 15:56:21', 1, '2019-03-31 15:56:21', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (61, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/NMW5A7iW6A.bin', 'Anuexs\\None\\NMW5A7iW6A.bin', NULL, 0, 0, 0, 1, '2019-03-31 16:03:45', 1, '2019-03-31 16:03:45', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (62, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/ZHTNK5AZNM.bin', 'Anuexs\\None\\ZHTNK5AZNM.bin', NULL, 0, 0, 0, 1, '2019-03-31 16:11:06', 1, '2019-03-31 16:11:06', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (63, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/UA31NN337Y.bin', 'Anuexs\\None\\UA31NN337Y.bin', NULL, 0, 0, 0, 1, '2019-03-31 16:12:11', 1, '2019-03-31 16:12:11', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (64, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/D2DTCJS9JN.bin', 'Anuexs\\None\\D2DTCJS9JN.bin', NULL, 0, 0, 0, 1, '2019-03-31 16:25:06', 1, '2019-03-31 16:25:06', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (65, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/PFWiVAZV9Y.bin', 'Anuexs\\None\\PFWiVAZV9Y.bin', NULL, 0, 0, 0, 1, '2019-03-31 16:52:29', 1, '2019-03-31 16:52:29', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (66, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/QTQKiBM86E.bin', 'Anuexs\\None\\QTQKiBM86E.bin', NULL, 0, 0, 0, 1, '2019-03-31 17:15:38', 1, '2019-03-31 17:15:38', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (67, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/XCF54FNVCW.bin', 'Anuexs\\None\\XCF54FNVCW.bin', NULL, 0, 0, 0, 1, '2019-03-31 19:11:38', 1, '2019-03-31 19:11:38', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (68, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/GZWLWPUG7A.bin', 'Anuexs\\None\\GZWLWPUG7A.bin', NULL, 0, 0, 0, 1, '2019-03-31 20:44:41', 1, '2019-03-31 20:44:41', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (69, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/ZQBZTKJZ1V.bin', 'Anuexs\\None\\ZQBZTKJZ1V.bin', NULL, 0, 0, 0, 1, '2019-03-31 20:47:21', 1, '2019-03-31 20:47:21', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (70, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/5RJQBM628M.bin', 'Anuexs\\None\\5RJQBM628M.bin', NULL, 0, 0, 0, 1, '2019-03-31 21:18:40', 1, '2019-03-31 21:18:40', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (71, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/RBNiD408PS.bin', 'Anuexs\\None\\RBNiD408PS.bin', NULL, 0, 0, 0, 1, '2019-03-31 21:20:53', 1, '2019-03-31 21:20:53', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (72, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/HGiUF0GW5A.bin', 'Anuexs\\None\\HGiUF0GW5A.bin', NULL, 0, 0, 0, 1, '2019-04-01 09:30:04', 1, '2019-04-01 09:30:04', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (73, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/TY6ZW2QU07.bin', 'Anuexs\\None\\TY6ZW2QU07.bin', NULL, 0, 0, 0, 1, '2019-04-01 13:15:33', 1, '2019-04-01 13:15:33', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (74, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/KEFNP7QFTB.bin', 'Anuexs\\None\\KEFNP7QFTB.bin', NULL, 0, 0, 0, 1, '2019-04-01 13:15:48', 1, '2019-04-01 13:15:48', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (75, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/1NH5ACJ8PA.bin', 'Anuexs\\None\\1NH5ACJ8PA.bin', NULL, 0, 0, 0, 1, '2019-04-01 13:16:42', 1, '2019-04-01 13:16:42', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (76, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/Q72EQR8WP5.bin', 'Anuexs\\None\\Q72EQR8WP5.bin', NULL, 0, 0, 0, 1, '2019-04-01 13:18:26', 1, '2019-04-01 13:18:26', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (77, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/MKUH5D5Si1.bin', 'Anuexs\\None\\MKUH5D5Si1.bin', NULL, 0, 0, 0, 1, '2019-04-01 14:06:23', 1, '2019-04-01 14:06:23', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (78, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/H2Y73iRLXP.bin', 'Anuexs\\None\\H2Y73iRLXP.bin', NULL, 0, 0, 0, 1, '2019-04-01 14:30:50', 1, '2019-04-01 14:30:50', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (79, 0, 0, 0, 'sku_org.csv', 'sku_org.csv', '/Anuexs/None/4M0MJ08W90.bin', 'Anuexs\\None\\4M0MJ08W90.bin', NULL, 0, 0, 0, 1, '2019-04-01 14:37:29', 1, '2019-04-01 14:37:29', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (80, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/Q7814QZ76T.bin', 'Anuexs\\None\\Q7814QZ76T.bin', NULL, 0, 0, 0, 1, '2019-04-01 15:44:11', 1, '2019-04-01 15:44:11', 0, 0, 0, NULL, 0);
INSERT INTO `tb_project_annex` VALUES (81, 0, 0, 0, 'sku_base.csv', 'sku_base.csv', '/Anuexs/None/LBLVZTW9i7.bin', 'Anuexs\\None\\LBLVZTW9i7.bin', NULL, 0, 0, 0, 1, '2019-04-01 15:47:55', 1, '2019-04-01 15:47:55', 0, 0, 0, NULL, 0);

-- ----------------------------
-- Table structure for tb_user_account
-- ----------------------------
DROP TABLE IF EXISTS `tb_user_account`;
CREATE TABLE `tb_user_account`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '标识主键',
  `user_id` bigint(20) NOT NULL COMMENT '用户Id',
  `account_type` int(11) NULL DEFAULT NULL COMMENT '账户类型',
  `account_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '账户名',
  `password` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '用户密码',
  `role_id` bigint(20) NULL DEFAULT NULL COMMENT '角色标识',
  `is_freeze` tinyint(1) NOT NULL DEFAULT 0 COMMENT '冻结更新',
  `data_state` int(11) NOT NULL DEFAULT 0 COMMENT '数据状态',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '制作时间',
  `last_reviser_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '最后修改者',
  `last_modify_date` datetime(0) NULL DEFAULT NULL COMMENT '最后修改日期',
  `author_id` bigint(20) NOT NULL DEFAULT 0 COMMENT '制作人',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  INDEX `UserId_Index`(`user_id`) USING BTREE,
  INDEX `RoleId_Index`(`role_id`) USING BTREE,
  INDEX `IsFreeze_Index`(`is_freeze`) USING BTREE,
  INDEX `DataState_Index`(`data_state`) USING BTREE,
  INDEX `AddDate_Index`(`add_date`) USING BTREE,
  INDEX `LastReviserId_Index`(`last_reviser_id`) USING BTREE,
  INDEX `LastModifyDate_Index`(`last_modify_date`) USING BTREE,
  INDEX `AuthorId_Index`(`author_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '登录账户' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_user_account
-- ----------------------------
INSERT INTO `tb_user_account` VALUES (1, 1, 1, 'root', 'root', 0, 0, 1, NULL, 1, '2018-11-12 09:21:11', 0);

-- ----------------------------
-- Table structure for tb_user_person
-- ----------------------------
DROP TABLE IF EXISTS `tb_user_person`;
CREATE TABLE `tb_user_person`  (
  `user_id` bigint(20) NOT NULL COMMENT '用户标识',
  `nick_name` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '昵称',
  `id_card` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '身份证号',
  `cert_type` int(11) NULL DEFAULT NULL COMMENT '证件类型',
  `real_name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '真实姓名',
  `sex` int(11) NULL DEFAULT NULL COMMENT '性别',
  `avatar_url` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '头像',
  `icon` longblob NULL COMMENT '头像照片',
  `phone_number` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '手机号',
  `birthday` datetime(0) NULL DEFAULT NULL COMMENT '生日',
  `nation` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '民族',
  `tel` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '电话',
  `email` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '电子邮件',
  `home_address` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '地址',
  `region_province` int(11) NULL DEFAULT NULL COMMENT '所在省',
  `region_city` int(11) NULL DEFAULT NULL COMMENT '所在市',
  `region_county` int(11) NULL DEFAULT NULL COMMENT '所在县',
  `is_freeze` tinyint(1) NOT NULL COMMENT '冻结更新',
  `data_state` int(11) NOT NULL COMMENT '数据状态',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '制作时间',
  `last_reviser_id` bigint(20) NULL DEFAULT NULL COMMENT '最后修改者',
  `last_modify_date` datetime(0) NULL DEFAULT NULL COMMENT '最后修改日期',
  `author_id` bigint(20) NOT NULL COMMENT '制作人',
  PRIMARY KEY (`user_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '个人信息' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_user_person
-- ----------------------------
INSERT INTO `tb_user_person` VALUES (1, 'root', NULL, NULL, 'root', 0, NULL, NULL, 'root', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 1, NULL, 0, NULL, 0);

-- ----------------------------
-- Table structure for tb_user_user
-- ----------------------------
DROP TABLE IF EXISTS `tb_user_user`;
CREATE TABLE `tb_user_user`  (
  `user_id` bigint(20) NOT NULL COMMENT '用户Id',
  `user_type` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '用户类型',
  `open_id` varchar(16) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '用户代码',
  `status` int(11) NOT NULL COMMENT '用户状态',
  `authorize_screen` int(11) NOT NULL COMMENT '已认证场景',
  `regist_screen` int(11) NOT NULL COMMENT '注册来源',
  `os` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '注册来源操作系统',
  `app` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '注册时的应用',
  `device_id` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '注册时设备识别码',
  `channel` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '注册来源渠道码',
  `trace_mark` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '注册来源活动跟踪码',
  `is_freeze` tinyint(1) NOT NULL COMMENT '冻结更新',
  `data_state` int(11) NOT NULL COMMENT '数据状态',
  `last_reviser_id` bigint(20) NULL DEFAULT NULL COMMENT '最后修改者',
  `last_modify_date` datetime(0) NULL DEFAULT NULL COMMENT '最后修改日期',
  `add_date` datetime(0) NULL DEFAULT NULL COMMENT '制作时间',
  `author_id` bigint(20) NOT NULL COMMENT '制作人',
  PRIMARY KEY (`user_id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '用户信息' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of tb_user_user
-- ----------------------------
INSERT INTO `tb_user_user` VALUES (1, '3', 'root', 1, 0, 0, '', NULL, '%', NULL, NULL, 1, 1, 0, NULL, NULL, 0);

-- ----------------------------
-- View structure for view_org_organization
-- ----------------------------
DROP VIEW IF EXISTS `view_org_organization`;
CREATE ALGORITHM = UNDEFINED DEFINER = `root`@`%` SQL SECURITY DEFINER VIEW `view_org_organization` AS select `tb_org_organization`.`id` AS `id`,`tb_org_organization`.`type` AS `type`,`tb_org_organization`.`code` AS `code`,`tb_org_organization`.`full_name` AS `full_name`,`tb_org_organization`.`short_name` AS `short_name`,`tb_org_organization`.`tree_name` AS `tree_name`,`tb_org_organization`.`org_level` AS `org_level`,`tb_org_organization`.`level_index` AS `level_index`,`tb_org_organization`.`parent_id` AS `parent_id`,`tb_org_organization`.`boundary_id` AS `boundary_id`,`tb_org_organization`.`memo` AS `memo`,`tb_org_organization`.`is_freeze` AS `is_freeze`,`tb_org_organization`.`data_state` AS `data_state`,`tb_org_organization`.`add_date` AS `add_date`,`tb_org_organization`.`last_reviser_id` AS `last_reviser_id`,`tb_org_organization`.`last_modify_date` AS `last_modify_date`,`tb_org_organization`.`author_id` AS `author_id`,`tb_org_organization`.`auditor_id` AS `auditor_id`,`tb_org_organization`.`audit_date` AS `audit_date`,`tb_org_organization`.`audit_state` AS `audit_state`,`tb_org_organization`.`super_orgcode` AS `super_orgcode`,`tb_org_organization`.`manag_orgcode` AS `manag_orgcode`,`tb_org_organization`.`manag_orgname` AS `manag_orgname`,`tb_org_organization`.`city_code` AS `city_code`,`tb_org_organization`.`district_code` AS `district_code`,`tb_org_organization`.`org_address` AS `org_address`,`tb_org_organization`.`law_personname` AS `law_personname`,`tb_org_organization`.`law_persontel` AS `law_persontel`,`tb_org_organization`.`contact_name` AS `contact_name`,`tb_org_organization`.`contact_tel` AS `contact_tel`,`tb_org_organization`.`area_id` AS `area_id`,`governmentarea`.`full_name` AS `area` from (`tb_org_organization` left join `tb_org_area` `governmentarea` on((`tb_org_organization`.`area_id` = `governmentarea`.`id`)));

-- ----------------------------
-- View structure for view_org_organize_position
-- ----------------------------
DROP VIEW IF EXISTS `view_org_organize_position`;
CREATE ALGORITHM = UNDEFINED DEFINER = `root`@`localhost` SQL SECURITY DEFINER VIEW `view_org_organize_position` AS select `op`.`id` AS `id`,`op`.`appellation` AS `appellation`,`org`.`group_id` AS `group_id`,`org`.`boundary_id` AS `org_id`,`org`.`org_level` AS `org_level`,`op`.`department_id` AS `department_id`,`org`.`tree_name` AS `department`,`org`.`tree_name` AS `organization`,`op`.`role_id` AS `role_id`,`role`.`caption` AS `role`,`op`.`memo` AS `memo`,`op`.`boundary_id` AS `boundary_id`,`op`.`is_freeze` AS `is_freeze`,`op`.`data_state` AS `data_state`,`op`.`add_date` AS `add_date`,`op`.`last_reviser_id` AS `last_reviser_id`,`op`.`last_modify_date` AS `last_modify_date`,`op`.`author_id` AS `author_id`,`op`.`auditor_id` AS `auditor_id`,`op`.`audit_date` AS `audit_date`,`op`.`audit_state` AS `audit_state` from ((`tb_org_organize_position` `op` left join `tb_org_organization` `org` on((`op`.`department_id` = `org`.`id`))) left join `tb_auth_role` `role` on((`op`.`role_id` = `role`.`id`)));

-- ----------------------------
-- View structure for view_org_position_personnel
-- ----------------------------
DROP VIEW IF EXISTS `view_org_position_personnel`;
CREATE ALGORITHM = UNDEFINED DEFINER = `root`@`localhost` SQL SECURITY DEFINER VIEW `view_org_position_personnel` AS select ifnull(`pp`.`id`,(0 - `pers`.`user_id`)) AS `id`,`pp`.`area_id` AS `area_id`,`pers`.`user_id` AS `user_id`,`pers`.`real_name` AS `real_name`,`pers`.`sex` AS `sex`,`pers`.`birthday` AS `birthday`,`pers`.`phone_number` AS `mobile`,`pers`.`phone_number` AS `phone_number`,`pers`.`tel` AS `tel`,`pp`.`appellation` AS `appellation`,`position`.`appellation` AS `position`,ifnull(`pp`.`organize_position_id`,0) AS `organize_position_id`,`org`.`group_id` AS `group_id`,`org`.`id` AS `organization_id`,ifnull(`position`.`department_id`,-(1)) AS `department_id`,`org`.`tree_name` AS `organization`,`org`.`short_name` AS `department`,`org`.`org_level` AS `org_level`,ifnull(`pp`.`role_id`,`position`.`role_id`) AS `role_id`,`role`.`caption` AS `role`,`pp`.`memo` AS `memo`,`pp`.`is_freeze` AS `is_freeze`,ifnull(`pp`.`data_state`,0) AS `data_state`,`pp`.`add_date` AS `add_date`,`pp`.`last_reviser_id` AS `last_reviser_id`,`pp`.`last_modify_date` AS `last_modify_date`,`pp`.`author_id` AS `author_id`,`pp`.`auditor_id` AS `auditor_id`,`pp`.`audit_date` AS `audit_date`,ifnull(`pp`.`audit_state`,0) AS `audit_state`,`pers`.`avatar_url` AS `avatar_url` from ((((`tb_user_person` `pers` left join `tb_org_position_personnel` `pp` on((`pp`.`user_id` = `pers`.`user_id`))) left join `tb_org_organize_position` `position` on((`pp`.`organize_position_id` = `position`.`id`))) left join `tb_org_organization` `org` on((`position`.`department_id` = `org`.`id`))) left join `tb_auth_role` `role` on((`position`.`role_id` = `role`.`id`))) where ((`pp`.`id` is not null) or (`pers`.`data_state` = 0) or (`pers`.`data_state` = 2));

-- ----------------------------
-- View structure for view_user_account
-- ----------------------------
DROP VIEW IF EXISTS `view_user_account`;
CREATE ALGORITHM = UNDEFINED DEFINER = `root`@`%` SQL SECURITY DEFINER VIEW `view_user_account` AS select `tb_user_account`.`id` AS `id`,`tb_user_account`.`role_id` AS `role_id`,`role`.`role` AS `role`,`tb_user_account`.`user_id` AS `user_id`,`person`.`nick_name` AS `nick_name`,`person`.`id_card` AS `id_card`,`person`.`phone_number` AS `phone_number`,`person`.`real_name` AS `real_name`,`tb_user_account`.`account_name` AS `account_name`,`tb_user_account`.`password` AS `password`,`tb_user_account`.`is_freeze` AS `is_freeze`,`tb_user_account`.`data_state` AS `data_state`,`tb_user_account`.`add_date` AS `add_date`,`tb_user_account`.`last_reviser_id` AS `last_reviser_id`,`tb_user_account`.`last_modify_date` AS `last_modify_date`,`tb_user_account`.`author_id` AS `author_id` from ((`tb_user_account` left join `tb_auth_role` `role` on((`tb_user_account`.`role_id` = `role`.`id`))) left join `tb_user_person` `person` on((`tb_user_account`.`user_id` = `person`.`user_id`)));

-- ----------------------------
-- View structure for view_user_user
-- ----------------------------
DROP VIEW IF EXISTS `view_user_user`;
CREATE ALGORITHM = UNDEFINED DEFINER = `root`@`%` SQL SECURITY DEFINER VIEW `view_user_user` AS select `tb_user_user`.`user_id` AS `user_id`,`tb_user_user`.`user_type` AS `user_type`,`tb_user_user`.`open_id` AS `open_id`,`tb_user_user`.`status` AS `status`,`tb_user_user`.`authorize_screen` AS `authorize_screen`,`tb_user_user`.`regist_screen` AS `regist_screen`,`tb_user_user`.`os` AS `os`,`tb_user_user`.`app` AS `app`,`tb_user_user`.`device_id` AS `device_id`,`tb_user_user`.`channel` AS `channel`,`tb_user_user`.`trace_mark` AS `trace_mark`,`tb_user_user`.`is_freeze` AS `is_freeze`,`tb_user_user`.`data_state` AS `data_state`,`tb_user_user`.`last_reviser_id` AS `last_reviser_id`,`tb_user_user`.`last_modify_date` AS `last_modify_date`,`tb_user_user`.`add_date` AS `add_date`,`tb_user_user`.`author_id` AS `author_id`,`person`.`phone_number` AS `phone_number`,`person`.`real_name` AS `real_name` from (`tb_user_user` left join `tb_user_person` `person` on((`tb_user_user`.`user_id` = `person`.`user_id`)));

SET FOREIGN_KEY_CHECKS = 1;
