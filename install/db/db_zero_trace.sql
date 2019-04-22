/*
 Navicat Premium Data Transfer

 Source Server         : zero.yizuanbao.cn
 Source Server Type    : MySQL
 Source Server Version : 50724
 Source Host           : zero.yizuanbao.cn:3306
 Source Schema         : db_zero_trace

 Target Server Type    : MySQL
 Target Server Version : 50724
 File Encoding         : 65001

 Date: 02/04/2019 00:57:24
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for tb_zt_flow_log
-- ----------------------------
DROP TABLE IF EXISTS `tb_zt_flow_log`;
CREATE TABLE `tb_zt_flow_log`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '流水号',
  `request_id` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '请求标识',
  `root_station` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '根站点',
  `root_command` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '根命令',
  `record_date` datetime(0) NULL DEFAULT NULL COMMENT '记录时间',
  `flow_json` longtext CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '流程内容的Json表示',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  INDEX `RequestId_Index`(`request_id`) USING BTREE,
  INDEX `RootStation_Index`(`root_station`) USING BTREE,
  INDEX `RootCommand_Index`(`root_command`) USING BTREE,
  INDEX `RecordDate_Index`(`record_date`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 30 CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = '流程日志' ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
