#include "main.h"
#include <iostream>
#include <string>
#include <sstream>
#include <vector>
#include <list>
#include <cstdio>

#include "sqlite3.h"
//#include "common.h"

// 参考：http://blog.csdn.net/majiakun1/article/details/46607163

static const char* kDatabaseName = "test.db";

//--------------------------------------------------------------

static void TestTimer();

//--------------------------------------------------------------

static bool PrepareDB(sqlite3** db, bool create_table);

static void CreateTable();

static void ClearTable();

static void TestExec(bool turn_off_synchronous = false);

static void TestNoSynchronous();

static void TestTransactionExec();

static void TestStep(bool turn_on_transaction = true);

static void PerformanceTest();

//--------------------------------------------------------------

int main() {
	//TestTimer();
	//ClearTable();
	CreateTable();

	PerformanceTest();

	return 0;
}

//--------------------------------------------------------------

static void PerformanceTest() {
	//TestExec(false);
	//TestExec(true);
	//TestNoSynchronous();
	//TestTransactionExec();
	TestStep(true);
	//TestStep(false);
}

// 1.直接执行sqlite3_exec。
static void TestExec(bool turn_off_synchronous) {
	sqlite3* db = NULL;
	if (!PrepareDB(&db, false)) {
		return;
	}

	if (turn_off_synchronous) {
		std::cout << "1.关闭写同步执行sqlite3_exec : " << std::endl;
	}
	else {
		std::cout << "2.直接执行sqlite3_exec : " << std::endl;
	}
	boost::posix_time::ptime start_time = boost::posix_time::microsec_clock::local_time();
	std::stringstream sstream(std::stringstream::out);

	const int kDataCount = 1000;
	const char* kTurnOffSynchronous = "PRAGMA synchronous = OFF;";

	if (turn_off_synchronous) {
		sqlite3_exec(db, kTurnOffSynchronous, NULL, NULL, NULL);
	}

	// Insert.
	start_time = boost::posix_time::microsec_clock::local_time();
	for (int i = 0; i < kDataCount; ++i) {
		sstream << "INSERT INTO PERFORMANCE_TEST VALUES("
			<< i << "," << i << "," << i << "," << i << ");";

		sqlite3_exec(db, sstream.str().c_str(), NULL, NULL, NULL);
		sstream.str("");
	}

	double rate = kDataCount / (boost::posix_time::microsec_clock::local_time() - start_time).total_milliseconds() * 1000.0;
	std::cout << "插入数据： " << rate << "条/秒" << std::endl;

	// Delete.
	start_time = boost::posix_time::microsec_clock::local_time();
	for (int i = 0; i < kDataCount; ++i) {
		sstream << "DELETE FROM PERFORMANCE_TEST WHERE ID1 = " << i;
		sqlite3_exec(db, sstream.str().c_str(), NULL, NULL, NULL);
		sstream.str("");
	}
	rate = kDataCount / (boost::posix_time::microsec_clock::local_time() - start_time).total_milliseconds() * 1000.0;
	std::cout << "删除数据： " << rate << "条/秒\n" << std::endl;

	sqlite3_close(db);
}

// 2.显式开启事务，执行sqlite3_exec。
// (A)所谓”事务“就是指一组SQL命令，这些命令要么一起执行，要么都不被执行。
// (B)在SQLite中，每调用一次sqlite3_exec()函数，就会隐式地开启了一个事务，如果插入一条数据，就调用该函数一次，事务就会被反复地开启、关闭，会增大IO量。
// (C)如果在插入数据前显式开启事务，插入后再一起提交，则会大大提高IO效率，进而加数据快插入速度。
static void TestTransactionExec() {
	sqlite3* db = NULL;
	if (!PrepareDB(&db, false)) {
		return;
	}

	std::cout << "3.显式开启事务执行sqlite3_exec : " << std::endl;

	boost::posix_time::ptime start_time = boost::posix_time::microsec_clock::local_time();
	std::stringstream sstream(std::stringstream::out);

	const int kDataCount = 100000;
	const char* kBeginTransaction = "begin";
	const char* kCommitTransaction = "commit";

	// Insert.
	start_time = boost::posix_time::microsec_clock::local_time();
	sqlite3_exec(db, kBeginTransaction, NULL, NULL, NULL);
	for (int i = 0; i < kDataCount; ++i) {
		sstream << "INSERT INTO PERFORMANCE_TEST VALUES("
			<< i << "," << i << "," << i << "," << i << ");";

		sqlite3_exec(db, sstream.str().c_str(), NULL, NULL, NULL);
		sstream.str("");
	}
	sqlite3_exec(db, kCommitTransaction, NULL, NULL, NULL);

	double rate = kDataCount / (boost::posix_time::microsec_clock::local_time() - start_time).total_milliseconds() * 1000.0;
	std::cout << "插入数据： " << rate << "条/秒" << std::endl;

	// Delete.
	start_time = boost::posix_time::microsec_clock::local_time();
	sqlite3_exec(db, kBeginTransaction, NULL, NULL, NULL);
	for (int i = 0; i < kDataCount; ++i) {
		sstream << "DELETE FROM PERFORMANCE_TEST WHERE ID1 = " << i;
		sqlite3_exec(db, sstream.str().c_str(), NULL, NULL, NULL);
		sstream.str("");
	}
	sqlite3_exec(db, kCommitTransaction, NULL, NULL, NULL);

	rate = kDataCount / (boost::posix_time::microsec_clock::local_time() - start_time).total_milliseconds() * 1000.0;
	std::cout << "删除数据： " << rate << "条/秒\n" << std::endl;

	sqlite3_close(db);
}

// 3.关闭写同步且显式开启事务执行sqlite3_exec。
// (A)在SQLite中，数据库配置的参数都由编译指示（pragma）来实现的。
// (B)synchronous选项有三种可选状态，分别是full、normal、off。
// 当synchronous设置为FULL,SQLite数据库引擎在紧急时刻会暂停以确定数据已经写入磁盘。这使系统崩溃或电源出问题时能确保数据库在重起后不会损坏。FULL synchronous很安全但很慢。
// 当synchronous设置为NORMAL, SQLite数据库引擎在大部分紧急时刻会暂停，但不像FULL模式下那么频繁。 NORMAL模式下有很小的几率(但不是不存在)发生电源故障导致数据库损坏的情况。
// 但实际上，在这种情况 下很可能你的硬盘已经不能使用，或者发生了其他的不可恢复的硬件错误。
// 当为synchronous OFF时，SQLite在传递数据给系统以后直接继续而不暂停。若运行SQLite的应用程序崩溃， 数据不会损伤，但在系统崩溃或写入数据时意外断电。
// (C)SQLite3中，该选项的默认值就是full，如果我们再插入数据前将其改为off，则会提高效率。
static void TestNoSynchronous() {
	sqlite3* db = NULL;
	if (!PrepareDB(&db, false)) {
		return;
	}

	std::cout << "4.关闭写同步且显式开启事务执行sqlite3_exec : " << std::endl;

	boost::posix_time::ptime start_time = boost::posix_time::microsec_clock::local_time();
	std::stringstream sstream(std::stringstream::out);

	const int kDataCount = 100000;
	const char* kTurnOffSynchronous = "PRAGMA synchronous = OFF;";
	const char* kBeginTransaction = "begin";
	const char* kCommitTransaction = "commit";

	sqlite3_exec(db, kTurnOffSynchronous, NULL, NULL, NULL);

	// Insert.
	start_time = boost::posix_time::microsec_clock::local_time();
	sqlite3_exec(db, kBeginTransaction, NULL, NULL, NULL);
	for (int i = 0; i < kDataCount; ++i) {
		sstream << "INSERT INTO PERFORMANCE_TEST VALUES("
			<< i << "," << i << "," << i << "," << i << ");";

		sqlite3_exec(db, sstream.str().c_str(), NULL, NULL, NULL);
		sstream.str("");
	}
	sqlite3_exec(db, kCommitTransaction, NULL, NULL, NULL);

	double rate = kDataCount / (boost::posix_time::microsec_clock::local_time() - start_time).total_milliseconds() * 1000.0;
	std::cout << "插入数据： " << rate << "条/秒" << std::endl;

	// Delete.
	start_time = boost::posix_time::microsec_clock::local_time();
	sqlite3_exec(db, kBeginTransaction, NULL, NULL, NULL);
	for (int i = 0; i < kDataCount; ++i) {
		sstream << "DELETE FROM PERFORMANCE_TEST WHERE ID1 = " << i;
		sqlite3_exec(db, sstream.str().c_str(), NULL, NULL, NULL);
		sstream.str("");
	}
	sqlite3_exec(db, kCommitTransaction, NULL, NULL, NULL);

	rate = kDataCount / (boost::posix_time::microsec_clock::local_time() - start_time).total_milliseconds() * 1000.0;
	std::cout << "删除数据： " << rate << "条/秒\n" << std::endl;

	sqlite3_close(db);
}

// 4. 使用sqlite3_step执行。
// (A)SQLite执行SQL语句的时候，有两种方式：一种是使用前文提到的函数sqlite3_exec()，该函数直接调用包含SQL语句的字符串；
// 另一种方法就是“执行准备”（类似于存储过程）操作，即先将SQL语句编译好，然后再一步一步（或一行一行）地执行。
// (B)如果采用前者的话，就算开起了事务，SQLite仍然要对循环中每一句SQL语句进行“词法分析”和“语法分析”。
// (C)“执行准备”主要分为三大步骤：并且声明一个指向sqlite3_stmt对象的指针，该函数对参数化的SQL语句zSql进行编译，将编译后的状态存入ppStmt中。
// 调用函数 sqlite3_step() ，这个函数就是执行一步（本例中就是插入一行），如果函数返回的是SQLite_ROW则说明仍在继续执行，否则则说明已经执行完所有操作。
// 调用函数 sqlite3_finalize()，关闭语句。
// (D)综上所述啊，SQLite插入数据效率最快的方式就是：事务+关闭写同步+执行准备（存储过程），如果对数据库安全性有要求的话，就开启写同步。

static void TestStep(bool turn_on_transaction) {
	sqlite3* db = NULL;
	if (!PrepareDB(&db, false)) {
		return;
	}

	std::string title = "5.直接执行sqlite3_step : ";
	if (turn_on_transaction) {
		title = "6.显式开启事务执行sqlite3_step : ";
	}

	std::cout << title << std::endl;

	boost::posix_time::ptime start_time = boost::posix_time::microsec_clock::local_time();

	int data_count = 10000000;
	//if (turn_on_transaction) {
	//	data_count = 1000000;
	//}

	const char* kBeginTransaction = "begin";
	const char* kCommitTransaction = "commit";

	// Insert.
	start_time = boost::posix_time::microsec_clock::local_time();
	if (turn_on_transaction) {
		sqlite3_exec(db, kBeginTransaction, NULL, NULL, NULL);
	}

	const char* kInsertSql = "INSERT INTO PERFORMANCE_TEST VALUES(?,?,?,?);";

	sqlite3_stmt* stmt = NULL;
	sqlite3_prepare_v2(db, kInsertSql, strlen(kInsertSql), &stmt, NULL);
	for (int i = 0; i < data_count; ++i) {
		sqlite3_reset(stmt);

		sqlite3_bind_int(stmt, 1, i);
		sqlite3_bind_int(stmt, 2, i);
		sqlite3_bind_int(stmt, 3, i);
		sqlite3_bind_int(stmt, 4, i);
		sqlite3_step(stmt);
	}

	if (turn_on_transaction) {
		sqlite3_exec(db, kCommitTransaction, NULL, NULL, NULL);
	}

	sqlite3_finalize(stmt);

	double rate = data_count / (boost::posix_time::microsec_clock::local_time() - start_time).total_milliseconds() * 1000.0;
	std::cout << "插入数据： " << rate << "条/秒" << std::endl;

	// Delete.
	start_time = boost::posix_time::microsec_clock::local_time();
	if (turn_on_transaction) {
		sqlite3_exec(db, kBeginTransaction, NULL, NULL, NULL);
	}

	const char* kDeleteSql = "DELETE FROM PERFORMANCE_TEST WHERE ID1 = ?;";
	sqlite3_prepare_v2(db, kDeleteSql, strlen(kDeleteSql), &stmt, NULL);
	for (int i = 0; i < data_count; ++i) {
		sqlite3_reset(stmt);

		sqlite3_bind_int(stmt, 1, i);
		sqlite3_step(stmt);
	}

	if (turn_on_transaction) {
		sqlite3_exec(db, kCommitTransaction, NULL, NULL, NULL);
	}

	sqlite3_finalize(stmt);

	rate = data_count / (boost::posix_time::microsec_clock::local_time() - start_time).total_milliseconds() * 1000.0;
	std::cout << "删除数据： " << rate << "条/秒\n" << std::endl;

	sqlite3_close(db);
}

static bool PrepareDB(sqlite3** db, bool create_table) {
	int rc = sqlite3_open(kDatabaseName, db);
	if (rc != SQLITE_OK) {
		std::cout << "Failed to open " << kDatabaseName << std::endl;
		std::cout << "Error msg: " << sqlite3_errmsg(*db) << std::endl;
		return false;
	}

	if (!create_table) {
		return true;
	}

	const char* kCreateTableSql = "CREATE TABLE PERFORMANCE_TEST(\
                                                                 ID1 INT, ID2 INT, ID3 INT, ID4 INT\
                                                                                                  );";
	char* error_msg = NULL;
	rc = sqlite3_exec(*db, kCreateTableSql, NULL, NULL, &error_msg);
	if (rc != SQLITE_OK) {
		std::cout << "Failed to create table PERFORMANCE_TEST." << std::endl;
		std::cout << "Error msg: " << error_msg << std::endl;
		sqlite3_free(error_msg);
		return false;
	}

	return true;
}

static void CreateTable() {
	sqlite3* db = NULL;
	PrepareDB(&db, true);
	sqlite3_close(db);
}

static void ClearTable() {
	sqlite3* db = NULL;
	PrepareDB(&db, false);

	const char* kClrearTableSql = "DELETE FROM PERFORMANCE_TEST;";
	char* error_msg = NULL;

	int rc = sqlite3_exec(db, kClrearTableSql, NULL, NULL, &error_msg);
	if (rc != SQLITE_OK) {
		std::cout << "Failed to clear table!" << std::endl;
		std::cout << "Error msg: " << error_msg << std::endl;
		sqlite3_free(error_msg);
	}

	sqlite3_close(db);
}