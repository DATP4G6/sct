grammar Sct;

start: (function | class_def)* EOF;

// Functions
function:
	FUNCTION ID LPAREN args_def RPAREN RIGHT_ARROW type LCURLY statement_list RCURLY;

// All argument definitions are here
args_def: (type ID (COMMA type ID)*)?;
args_agent: (ID COLON expression (COMMA ID COLON expression)*)?;
args_call: (expression (COMMA expression)*)?;

// Statements
statement_list: statement*;
statement:
	expression SEMI	# ExpressionStatement
	| declaration	# StatementTag
	| assignment	# StatementTag
	| if			# StatementTag
	| while			# StatementTag
	| enter			# StatementTag
	| return		# StatementTag
	| create		# StatementTag
	| destroy		# StatementTag
	| exit			# StatementTag
	| break			# StatementTag
	| continue		# StatementTag;

declaration:
	type ID ASSIGN expression SEMI # variableDeclaration;
assignment: ID ASSIGN expression SEMI;
if:
	IF LPAREN expression RPAREN LCURLY statement_list RCURLY (
		elseif
		| else
	)?;
elseif:
	ELSE IF LPAREN expression RPAREN LCURLY statement_list RCURLY (
		elseif
		| else
	)?;
else: ELSE LCURLY statement_list RCURLY;
while:
	WHILE LPAREN expression RPAREN LCURLY statement_list RCURLY;
enter: ENTER ID SEMI;
return: RETURN expression? SEMI;
create: CREATE agent_create SEMI;
destroy: DESTROY SEMI;
exit: EXIT SEMI;
break: BREAK SEMI;
continue: CONTINUE SEMI;

// Expressions
expression:
	LIT									# LiteralExpression
	| ID								# IDExpression
	| LPAREN expression RPAREN			# ParenthesisExpression
	| LPAREN type RPAREN expression		# TypecastExpression
	| ID LPAREN args_call RPAREN		# CallExpression
	| unary_minus = MINUS expression	# UnaryMinusExpression
	| not = NOT expression				# LogicalNotExpression
	| expression op = MULT expression	# BinaryExpression
	| expression op = DIV expression	# BinaryExpression
	| expression op = MOD expression	# BinaryExpression
	| expression op = PLUS expression	# BinaryExpression
	| expression op = MINUS expression	# BinaryExpression
	| expression op = GT expression		# BooleanExpression
	| expression op = LT expression		# BooleanExpression
	| expression op = GTE expression	# BooleanExpression
	| expression op = LTE expression	# BooleanExpression
	| expression op = EQ expression		# BooleanExpression
	| expression op = NEQ expression	# BooleanExpression
	| expression op = AND expression	# BooleanExpression
	| expression op = OR expression		# BooleanExpression
	| agent_predicate					# AgentPredicateExpression;

// Class
class_def:
	CLASS ID LPAREN args_def RPAREN LCURLY class_body RCURLY;
class_body: (state | function | decorator)*;

decorator: DECORATOR ID LCURLY statement_list RCURLY;
state: (state_decorator)* STATE ID LCURLY statement_list RCURLY;
state_decorator: AT ID;

// Not using literals probably allows WS arround '::'
agent_create: ID DBL_COLON ID LPAREN args_agent RPAREN;
agent_predicate:
	ID DBL_COLON (ID | QUESTION) LPAREN args_agent RPAREN;

FUNCTION: 'function';
RIGHT_ARROW: '->';

type: T_INT | T_FLOAT | T_VOID;
T_INT: 'int';
T_FLOAT: 'float';
T_VOID: 'void';

ASSIGN: '=';
IF: 'if';
ELSE: 'else';
WHILE: 'while';
RETURN: 'return';
ENTER: 'enter';
CREATE: 'create';
DESTROY: 'destroy';
EXIT: 'exit';
CONTINUE: 'continue';
BREAK: 'break';

MULT: '*';
DIV: '/';
PLUS: '+';
MINUS: '-';
MOD: '%';
AND: '&&';
OR: '||';
EQ: '==';
NEQ: '!=';
GT: '>';
LT: '<';
GTE: '>=';
LTE: '<=';
NOT: '!';

CLASS: 'class';
STATE: 'state';
DECORATOR: 'decorator';
AT: '@';

WS: [ \t\r\n]+ -> skip;
COMMENT: '//' ~[\r\n]* -> skip;

QUESTION: '?';
LPAREN: '(';
RPAREN: ')';
LCURLY: '{';
RCURLY: '}';
SEMI: ';';
COMMA: ',';
COLON: ':';
DBL_COLON: '::';
ID: [a-zA-Z_][a-zA-Z_0-9]*;
LIT: INT | FLOAT;
INT: [0-9]+;
FLOAT: [0-9]+ '.' [0-9]+;