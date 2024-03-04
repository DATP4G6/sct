// Generated from /home/bliztle/uni/4/project/sct/sct.g4 by ANTLR 4.13.1
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast", "CheckReturnValue"})
public class sctParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.13.1", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14, T__14=15, T__15=16, T__16=17, 
		T__17=18, T__18=19, T__19=20, T__20=21, T__21=22, T__22=23, T__23=24, 
		T__24=25, T__25=26, T__26=27, T__27=28, T__28=29, T__29=30, T__30=31, 
		T__31=32, T__32=33, T__33=34, T__34=35, T__35=36, T__36=37, T__37=38, 
		ID=39, ID_p=40, LIT=41, INT=42, FLOAT=43, WS=44, COMMENT=45;
	public static final int
		RULE_program = 0, RULE_function = 1, RULE_args_def = 2, RULE_args_def_p = 3, 
		RULE_args_entity = 4, RULE_args_entity_p = 5, RULE_args_entity_pp = 6, 
		RULE_args_call = 7, RULE_args_call_p = 8, RULE_args_call_pp = 9, RULE_type = 10, 
		RULE_statement = 11, RULE_statement_pp = 12, RULE_statement_p = 13, RULE_declaration = 14, 
		RULE_assignment = 15, RULE_if = 16, RULE_else = 17, RULE_while = 18, RULE_enter = 19, 
		RULE_return = 20, RULE_return_p = 21, RULE_create = 22, RULE_destroy = 23, 
		RULE_exit = 24, RULE_expression = 25, RULE_exp = 26, RULE_call = 27, RULE_agent_def = 28, 
		RULE_agent_body = 29, RULE_state = 30, RULE_entity_create = 31, RULE_entity_predicate = 32;
	private static String[] makeRuleNames() {
		return new String[] {
			"program", "function", "args_def", "args_def_p", "args_entity", "args_entity_p", 
			"args_entity_pp", "args_call", "args_call_p", "args_call_pp", "type", 
			"statement", "statement_pp", "statement_p", "declaration", "assignment", 
			"if", "else", "while", "enter", "return", "return_p", "create", "destroy", 
			"exit", "expression", "exp", "call", "agent_def", "agent_body", "state", 
			"entity_create", "entity_predicate"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'function '", "'('", "') ->'", "' {'", "'}'", "', '", "':'", "','", 
			"'int'", "'float'", "'void'", "';'", "'='", "'if'", "')'", "'{'", "'else'", 
			"'while'", "'enter'", "'return'", "'create'", "'destroy;'", "'exit;'", 
			"'*'", "'/'", "'+'", "'-'", "'=='", "'!='", "'>'", "'<'", "'>='", "'<='", 
			"'!'", "'agent'", "') {'", "'state'", "'::'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null, 
			null, null, null, null, null, null, null, null, null, null, null, null, 
			null, null, null, null, null, null, null, null, null, null, null, null, 
			null, null, null, "ID", "ID_p", "LIT", "INT", "FLOAT", "WS", "COMMENT"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}

	@Override
	public String getGrammarFileName() { return "sct.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public sctParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ProgramContext extends ParserRuleContext {
		public FunctionContext function() {
			return getRuleContext(FunctionContext.class,0);
		}
		public ProgramContext program() {
			return getRuleContext(ProgramContext.class,0);
		}
		public Agent_defContext agent_def() {
			return getRuleContext(Agent_defContext.class,0);
		}
		public TerminalNode EOF() { return getToken(sctParser.EOF, 0); }
		public ProgramContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_program; }
	}

	public final ProgramContext program() throws RecognitionException {
		ProgramContext _localctx = new ProgramContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_program);
		try {
			setState(73);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__0:
				enterOuterAlt(_localctx, 1);
				{
				setState(66);
				function();
				setState(67);
				program();
				}
				break;
			case T__34:
				enterOuterAlt(_localctx, 2);
				{
				setState(69);
				agent_def();
				setState(70);
				program();
				}
				break;
			case EOF:
				enterOuterAlt(_localctx, 3);
				{
				setState(72);
				match(EOF);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class FunctionContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public Args_defContext args_def() {
			return getRuleContext(Args_defContext.class,0);
		}
		public TypeContext type() {
			return getRuleContext(TypeContext.class,0);
		}
		public StatementContext statement() {
			return getRuleContext(StatementContext.class,0);
		}
		public FunctionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_function; }
	}

	public final FunctionContext function() throws RecognitionException {
		FunctionContext _localctx = new FunctionContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_function);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(75);
			match(T__0);
			setState(76);
			match(ID);
			setState(77);
			match(T__1);
			setState(78);
			args_def();
			setState(79);
			match(T__2);
			setState(80);
			type();
			setState(81);
			match(T__3);
			setState(82);
			statement();
			setState(83);
			match(T__4);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Args_defContext extends ParserRuleContext {
		public Args_def_pContext args_def_p() {
			return getRuleContext(Args_def_pContext.class,0);
		}
		public Args_defContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_args_def; }
	}

	public final Args_defContext args_def() throws RecognitionException {
		Args_defContext _localctx = new Args_defContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_args_def);
		try {
			setState(87);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__8:
			case T__9:
			case T__10:
				enterOuterAlt(_localctx, 1);
				{
				setState(85);
				args_def_p();
				}
				break;
			case T__2:
			case T__35:
				enterOuterAlt(_localctx, 2);
				{
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Args_def_pContext extends ParserRuleContext {
		public TypeContext type() {
			return getRuleContext(TypeContext.class,0);
		}
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public Args_def_pContext args_def_p() {
			return getRuleContext(Args_def_pContext.class,0);
		}
		public Args_def_pContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_args_def_p; }
	}

	public final Args_def_pContext args_def_p() throws RecognitionException {
		Args_def_pContext _localctx = new Args_def_pContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_args_def_p);
		try {
			setState(97);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,2,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(89);
				type();
				setState(90);
				match(ID);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(92);
				type();
				setState(93);
				match(ID);
				setState(94);
				match(T__5);
				setState(95);
				args_def_p();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Args_entityContext extends ParserRuleContext {
		public Args_entity_pContext args_entity_p() {
			return getRuleContext(Args_entity_pContext.class,0);
		}
		public Args_entityContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_args_entity; }
	}

	public final Args_entityContext args_entity() throws RecognitionException {
		Args_entityContext _localctx = new Args_entityContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_args_entity);
		try {
			setState(101);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case ID:
				enterOuterAlt(_localctx, 1);
				{
				setState(99);
				args_entity_p();
				}
				break;
			case T__14:
				enterOuterAlt(_localctx, 2);
				{
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Args_entity_pContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public Args_entity_ppContext args_entity_pp() {
			return getRuleContext(Args_entity_ppContext.class,0);
		}
		public Args_entity_pContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_args_entity_p; }
	}

	public final Args_entity_pContext args_entity_p() throws RecognitionException {
		Args_entity_pContext _localctx = new Args_entity_pContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_args_entity_p);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(103);
			match(ID);
			setState(104);
			match(T__6);
			setState(105);
			expression();
			setState(106);
			args_entity_pp();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Args_entity_ppContext extends ParserRuleContext {
		public Args_entity_pContext args_entity_p() {
			return getRuleContext(Args_entity_pContext.class,0);
		}
		public Args_entity_ppContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_args_entity_pp; }
	}

	public final Args_entity_ppContext args_entity_pp() throws RecognitionException {
		Args_entity_ppContext _localctx = new Args_entity_ppContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_args_entity_pp);
		try {
			setState(111);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__7:
				enterOuterAlt(_localctx, 1);
				{
				setState(108);
				match(T__7);
				setState(109);
				args_entity_p();
				}
				break;
			case T__14:
				enterOuterAlt(_localctx, 2);
				{
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Args_callContext extends ParserRuleContext {
		public Args_call_pContext args_call_p() {
			return getRuleContext(Args_call_pContext.class,0);
		}
		public Args_callContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_args_call; }
	}

	public final Args_callContext args_call() throws RecognitionException {
		Args_callContext _localctx = new Args_callContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_args_call);
		try {
			setState(115);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__1:
			case T__26:
			case T__33:
			case ID:
			case LIT:
				enterOuterAlt(_localctx, 1);
				{
				setState(113);
				args_call_p();
				}
				break;
			case T__14:
				enterOuterAlt(_localctx, 2);
				{
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Args_call_pContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public Args_call_ppContext args_call_pp() {
			return getRuleContext(Args_call_ppContext.class,0);
		}
		public Args_call_pContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_args_call_p; }
	}

	public final Args_call_pContext args_call_p() throws RecognitionException {
		Args_call_pContext _localctx = new Args_call_pContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_args_call_p);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(117);
			expression();
			setState(118);
			args_call_pp();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Args_call_ppContext extends ParserRuleContext {
		public Args_call_pContext args_call_p() {
			return getRuleContext(Args_call_pContext.class,0);
		}
		public Args_call_ppContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_args_call_pp; }
	}

	public final Args_call_ppContext args_call_pp() throws RecognitionException {
		Args_call_ppContext _localctx = new Args_call_ppContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_args_call_pp);
		try {
			setState(123);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__7:
				enterOuterAlt(_localctx, 1);
				{
				setState(120);
				match(T__7);
				setState(121);
				args_call_p();
				}
				break;
			case T__14:
				enterOuterAlt(_localctx, 2);
				{
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class TypeContext extends ParserRuleContext {
		public TypeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_type; }
	}

	public final TypeContext type() throws RecognitionException {
		TypeContext _localctx = new TypeContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_type);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(125);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & 3584L) != 0)) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class StatementContext extends ParserRuleContext {
		public Statement_pContext statement_p() {
			return getRuleContext(Statement_pContext.class,0);
		}
		public Statement_ppContext statement_pp() {
			return getRuleContext(Statement_ppContext.class,0);
		}
		public StatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statement; }
	}

	public final StatementContext statement() throws RecognitionException {
		StatementContext _localctx = new StatementContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_statement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(127);
			statement_p();
			setState(128);
			statement_pp();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Statement_ppContext extends ParserRuleContext {
		public StatementContext statement() {
			return getRuleContext(StatementContext.class,0);
		}
		public Statement_ppContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statement_pp; }
	}

	public final Statement_ppContext statement_pp() throws RecognitionException {
		Statement_ppContext _localctx = new Statement_ppContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_statement_pp);
		try {
			setState(132);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__1:
			case T__8:
			case T__9:
			case T__10:
			case T__13:
			case T__17:
			case T__18:
			case T__19:
			case T__20:
			case T__21:
			case T__22:
			case T__26:
			case T__33:
			case ID:
			case LIT:
				enterOuterAlt(_localctx, 1);
				{
				setState(130);
				statement();
				}
				break;
			case T__4:
				enterOuterAlt(_localctx, 2);
				{
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Statement_pContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public DeclarationContext declaration() {
			return getRuleContext(DeclarationContext.class,0);
		}
		public AssignmentContext assignment() {
			return getRuleContext(AssignmentContext.class,0);
		}
		public IfContext if_() {
			return getRuleContext(IfContext.class,0);
		}
		public WhileContext while_() {
			return getRuleContext(WhileContext.class,0);
		}
		public EnterContext enter() {
			return getRuleContext(EnterContext.class,0);
		}
		public ReturnContext return_() {
			return getRuleContext(ReturnContext.class,0);
		}
		public CreateContext create() {
			return getRuleContext(CreateContext.class,0);
		}
		public DestroyContext destroy() {
			return getRuleContext(DestroyContext.class,0);
		}
		public ExitContext exit() {
			return getRuleContext(ExitContext.class,0);
		}
		public Statement_pContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statement_p; }
	}

	public final Statement_pContext statement_p() throws RecognitionException {
		Statement_pContext _localctx = new Statement_pContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_statement_p);
		try {
			setState(146);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,8,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(134);
				expression();
				setState(135);
				match(T__11);
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(137);
				declaration();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(138);
				assignment();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(139);
				if_();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(140);
				while_();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(141);
				enter();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(142);
				return_();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(143);
				create();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(144);
				destroy();
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(145);
				exit();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class DeclarationContext extends ParserRuleContext {
		public TypeContext type() {
			return getRuleContext(TypeContext.class,0);
		}
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public DeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_declaration; }
	}

	public final DeclarationContext declaration() throws RecognitionException {
		DeclarationContext _localctx = new DeclarationContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_declaration);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(148);
			type();
			setState(149);
			match(ID);
			setState(150);
			match(T__12);
			setState(151);
			expression();
			setState(152);
			match(T__11);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class AssignmentContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public AssignmentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_assignment; }
	}

	public final AssignmentContext assignment() throws RecognitionException {
		AssignmentContext _localctx = new AssignmentContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_assignment);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(154);
			match(ID);
			setState(155);
			match(T__12);
			setState(156);
			expression();
			setState(157);
			match(T__11);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class IfContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public StatementContext statement() {
			return getRuleContext(StatementContext.class,0);
		}
		public ElseContext else_() {
			return getRuleContext(ElseContext.class,0);
		}
		public IfContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_if; }
	}

	public final IfContext if_() throws RecognitionException {
		IfContext _localctx = new IfContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_if);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(159);
			match(T__13);
			setState(160);
			match(T__1);
			setState(161);
			expression();
			setState(162);
			match(T__14);
			setState(163);
			match(T__15);
			setState(164);
			statement();
			setState(165);
			match(T__4);
			setState(166);
			else_();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ElseContext extends ParserRuleContext {
		public StatementContext statement() {
			return getRuleContext(StatementContext.class,0);
		}
		public ElseContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_else; }
	}

	public final ElseContext else_() throws RecognitionException {
		ElseContext _localctx = new ElseContext(_ctx, getState());
		enterRule(_localctx, 34, RULE_else);
		try {
			setState(174);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__16:
				enterOuterAlt(_localctx, 1);
				{
				setState(168);
				match(T__16);
				setState(169);
				match(T__15);
				setState(170);
				statement();
				setState(171);
				match(T__4);
				}
				break;
			case T__1:
			case T__4:
			case T__8:
			case T__9:
			case T__10:
			case T__13:
			case T__17:
			case T__18:
			case T__19:
			case T__20:
			case T__21:
			case T__22:
			case T__26:
			case T__33:
			case ID:
			case LIT:
				enterOuterAlt(_localctx, 2);
				{
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class WhileContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public StatementContext statement() {
			return getRuleContext(StatementContext.class,0);
		}
		public WhileContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_while; }
	}

	public final WhileContext while_() throws RecognitionException {
		WhileContext _localctx = new WhileContext(_ctx, getState());
		enterRule(_localctx, 36, RULE_while);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(176);
			match(T__17);
			setState(177);
			match(T__1);
			setState(178);
			expression();
			setState(179);
			match(T__14);
			setState(180);
			match(T__15);
			setState(181);
			statement();
			setState(182);
			match(T__4);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class EnterContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public EnterContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_enter; }
	}

	public final EnterContext enter() throws RecognitionException {
		EnterContext _localctx = new EnterContext(_ctx, getState());
		enterRule(_localctx, 38, RULE_enter);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(184);
			match(T__18);
			setState(185);
			match(ID);
			setState(186);
			match(T__11);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ReturnContext extends ParserRuleContext {
		public Return_pContext return_p() {
			return getRuleContext(Return_pContext.class,0);
		}
		public ReturnContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_return; }
	}

	public final ReturnContext return_() throws RecognitionException {
		ReturnContext _localctx = new ReturnContext(_ctx, getState());
		enterRule(_localctx, 40, RULE_return);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(188);
			match(T__19);
			setState(189);
			return_p();
			setState(190);
			match(T__11);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Return_pContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public Return_pContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_return_p; }
	}

	public final Return_pContext return_p() throws RecognitionException {
		Return_pContext _localctx = new Return_pContext(_ctx, getState());
		enterRule(_localctx, 42, RULE_return_p);
		try {
			setState(194);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__1:
			case T__26:
			case T__33:
			case ID:
			case LIT:
				enterOuterAlt(_localctx, 1);
				{
				setState(192);
				expression();
				}
				break;
			case T__11:
				enterOuterAlt(_localctx, 2);
				{
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class CreateContext extends ParserRuleContext {
		public Entity_createContext entity_create() {
			return getRuleContext(Entity_createContext.class,0);
		}
		public CreateContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_create; }
	}

	public final CreateContext create() throws RecognitionException {
		CreateContext _localctx = new CreateContext(_ctx, getState());
		enterRule(_localctx, 44, RULE_create);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(196);
			match(T__20);
			setState(197);
			entity_create();
			setState(198);
			match(T__11);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class DestroyContext extends ParserRuleContext {
		public DestroyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_destroy; }
	}

	public final DestroyContext destroy() throws RecognitionException {
		DestroyContext _localctx = new DestroyContext(_ctx, getState());
		enterRule(_localctx, 46, RULE_destroy);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(200);
			match(T__21);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ExitContext extends ParserRuleContext {
		public ExitContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_exit; }
	}

	public final ExitContext exit() throws RecognitionException {
		ExitContext _localctx = new ExitContext(_ctx, getState());
		enterRule(_localctx, 48, RULE_exit);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(202);
			match(T__22);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ExpressionContext extends ParserRuleContext {
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public ExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_expression; }
	}

	public final ExpressionContext expression() throws RecognitionException {
		ExpressionContext _localctx = new ExpressionContext(_ctx, getState());
		enterRule(_localctx, 50, RULE_expression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(204);
			exp(0);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ExpContext extends ParserRuleContext {
		public Token unary_minus;
		public Token not;
		public Token mult;
		public Token div;
		public Token plus;
		public Token minus;
		public Token eq;
		public Token neq;
		public Token gt;
		public Token lt;
		public Token gte;
		public Token lte;
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public CallContext call() {
			return getRuleContext(CallContext.class,0);
		}
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public TerminalNode LIT() { return getToken(sctParser.LIT, 0); }
		public ExpContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_exp; }
	}

	public final ExpContext exp() throws RecognitionException {
		return exp(0);
	}

	private ExpContext exp(int _p) throws RecognitionException {
		ParserRuleContext _parentctx = _ctx;
		int _parentState = getState();
		ExpContext _localctx = new ExpContext(_ctx, _parentState);
		ExpContext _prevctx = _localctx;
		int _startState = 52;
		enterRecursionRule(_localctx, 52, RULE_exp, _p);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(218);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,11,_ctx) ) {
			case 1:
				{
				setState(207);
				((ExpContext)_localctx).unary_minus = match(T__26);
				setState(208);
				exp(6);
				}
				break;
			case 2:
				{
				setState(209);
				((ExpContext)_localctx).not = match(T__33);
				setState(210);
				exp(5);
				}
				break;
			case 3:
				{
				setState(211);
				call();
				}
				break;
			case 4:
				{
				setState(212);
				match(T__1);
				setState(213);
				exp(0);
				setState(214);
				match(T__14);
				}
				break;
			case 5:
				{
				setState(216);
				match(ID);
				}
				break;
			case 6:
				{
				setState(217);
				match(LIT);
				}
				break;
			}
			_ctx.stop = _input.LT(-1);
			setState(252);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,13,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					setState(250);
					_errHandler.sync(this);
					switch ( getInterpreter().adaptivePredict(_input,12,_ctx) ) {
					case 1:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(220);
						if (!(precpred(_ctx, 16))) throw new FailedPredicateException(this, "precpred(_ctx, 16)");
						setState(221);
						((ExpContext)_localctx).mult = match(T__23);
						setState(222);
						exp(17);
						}
						break;
					case 2:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(223);
						if (!(precpred(_ctx, 15))) throw new FailedPredicateException(this, "precpred(_ctx, 15)");
						setState(224);
						((ExpContext)_localctx).div = match(T__24);
						setState(225);
						exp(16);
						}
						break;
					case 3:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(226);
						if (!(precpred(_ctx, 14))) throw new FailedPredicateException(this, "precpred(_ctx, 14)");
						setState(227);
						((ExpContext)_localctx).plus = match(T__25);
						setState(228);
						exp(15);
						}
						break;
					case 4:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(229);
						if (!(precpred(_ctx, 13))) throw new FailedPredicateException(this, "precpred(_ctx, 13)");
						setState(230);
						((ExpContext)_localctx).minus = match(T__26);
						setState(231);
						exp(14);
						}
						break;
					case 5:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(232);
						if (!(precpred(_ctx, 12))) throw new FailedPredicateException(this, "precpred(_ctx, 12)");
						setState(233);
						((ExpContext)_localctx).eq = match(T__27);
						setState(234);
						exp(13);
						}
						break;
					case 6:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(235);
						if (!(precpred(_ctx, 11))) throw new FailedPredicateException(this, "precpred(_ctx, 11)");
						setState(236);
						((ExpContext)_localctx).neq = match(T__28);
						setState(237);
						exp(12);
						}
						break;
					case 7:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(238);
						if (!(precpred(_ctx, 10))) throw new FailedPredicateException(this, "precpred(_ctx, 10)");
						setState(239);
						((ExpContext)_localctx).gt = match(T__29);
						setState(240);
						exp(11);
						}
						break;
					case 8:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(241);
						if (!(precpred(_ctx, 9))) throw new FailedPredicateException(this, "precpred(_ctx, 9)");
						setState(242);
						((ExpContext)_localctx).lt = match(T__30);
						setState(243);
						exp(10);
						}
						break;
					case 9:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(244);
						if (!(precpred(_ctx, 8))) throw new FailedPredicateException(this, "precpred(_ctx, 8)");
						setState(245);
						((ExpContext)_localctx).gte = match(T__31);
						setState(246);
						exp(9);
						}
						break;
					case 10:
						{
						_localctx = new ExpContext(_parentctx, _parentState);
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(247);
						if (!(precpred(_ctx, 7))) throw new FailedPredicateException(this, "precpred(_ctx, 7)");
						setState(248);
						((ExpContext)_localctx).lte = match(T__32);
						setState(249);
						exp(8);
						}
						break;
					}
					} 
				}
				setState(254);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,13,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			unrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class CallContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public Args_callContext args_call() {
			return getRuleContext(Args_callContext.class,0);
		}
		public CallContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_call; }
	}

	public final CallContext call() throws RecognitionException {
		CallContext _localctx = new CallContext(_ctx, getState());
		enterRule(_localctx, 54, RULE_call);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(255);
			match(ID);
			setState(256);
			match(T__1);
			setState(257);
			args_call();
			setState(258);
			match(T__14);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Agent_defContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public Args_defContext args_def() {
			return getRuleContext(Args_defContext.class,0);
		}
		public Agent_bodyContext agent_body() {
			return getRuleContext(Agent_bodyContext.class,0);
		}
		public Agent_defContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_agent_def; }
	}

	public final Agent_defContext agent_def() throws RecognitionException {
		Agent_defContext _localctx = new Agent_defContext(_ctx, getState());
		enterRule(_localctx, 56, RULE_agent_def);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(260);
			match(T__34);
			setState(261);
			match(ID);
			setState(262);
			match(T__1);
			setState(263);
			args_def();
			setState(264);
			match(T__35);
			setState(265);
			agent_body();
			setState(266);
			match(T__4);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Agent_bodyContext extends ParserRuleContext {
		public StateContext state() {
			return getRuleContext(StateContext.class,0);
		}
		public Agent_bodyContext agent_body() {
			return getRuleContext(Agent_bodyContext.class,0);
		}
		public FunctionContext function() {
			return getRuleContext(FunctionContext.class,0);
		}
		public Agent_bodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_agent_body; }
	}

	public final Agent_bodyContext agent_body() throws RecognitionException {
		Agent_bodyContext _localctx = new Agent_bodyContext(_ctx, getState());
		enterRule(_localctx, 58, RULE_agent_body);
		try {
			setState(275);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__36:
				enterOuterAlt(_localctx, 1);
				{
				setState(268);
				state();
				setState(269);
				agent_body();
				}
				break;
			case T__0:
				enterOuterAlt(_localctx, 2);
				{
				setState(271);
				function();
				setState(272);
				agent_body();
				}
				break;
			case T__4:
				enterOuterAlt(_localctx, 3);
				{
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class StateContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public StatementContext statement() {
			return getRuleContext(StatementContext.class,0);
		}
		public StateContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_state; }
	}

	public final StateContext state() throws RecognitionException {
		StateContext _localctx = new StateContext(_ctx, getState());
		enterRule(_localctx, 60, RULE_state);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(277);
			match(T__36);
			setState(278);
			match(ID);
			setState(279);
			match(T__15);
			setState(280);
			statement();
			setState(281);
			match(T__4);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Entity_createContext extends ParserRuleContext {
		public List<TerminalNode> ID() { return getTokens(sctParser.ID); }
		public TerminalNode ID(int i) {
			return getToken(sctParser.ID, i);
		}
		public Args_entityContext args_entity() {
			return getRuleContext(Args_entityContext.class,0);
		}
		public Entity_createContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_entity_create; }
	}

	public final Entity_createContext entity_create() throws RecognitionException {
		Entity_createContext _localctx = new Entity_createContext(_ctx, getState());
		enterRule(_localctx, 62, RULE_entity_create);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(283);
			match(ID);
			setState(284);
			match(T__37);
			setState(285);
			match(ID);
			setState(286);
			match(T__1);
			setState(287);
			args_entity();
			setState(288);
			match(T__14);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class Entity_predicateContext extends ParserRuleContext {
		public TerminalNode ID() { return getToken(sctParser.ID, 0); }
		public TerminalNode ID_p() { return getToken(sctParser.ID_p, 0); }
		public Args_entityContext args_entity() {
			return getRuleContext(Args_entityContext.class,0);
		}
		public Entity_predicateContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_entity_predicate; }
	}

	public final Entity_predicateContext entity_predicate() throws RecognitionException {
		Entity_predicateContext _localctx = new Entity_predicateContext(_ctx, getState());
		enterRule(_localctx, 64, RULE_entity_predicate);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(290);
			match(ID);
			setState(291);
			match(T__37);
			setState(292);
			match(ID_p);
			setState(293);
			match(T__1);
			setState(294);
			args_entity();
			setState(295);
			match(T__14);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 26:
			return exp_sempred((ExpContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean exp_sempred(ExpContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return precpred(_ctx, 16);
		case 1:
			return precpred(_ctx, 15);
		case 2:
			return precpred(_ctx, 14);
		case 3:
			return precpred(_ctx, 13);
		case 4:
			return precpred(_ctx, 12);
		case 5:
			return precpred(_ctx, 11);
		case 6:
			return precpred(_ctx, 10);
		case 7:
			return precpred(_ctx, 9);
		case 8:
			return precpred(_ctx, 8);
		case 9:
			return precpred(_ctx, 7);
		}
		return true;
	}

	public static final String _serializedATN =
		"\u0004\u0001-\u012a\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
		"\u0002\u0007\u0002\u0002\u0003\u0007\u0003\u0002\u0004\u0007\u0004\u0002"+
		"\u0005\u0007\u0005\u0002\u0006\u0007\u0006\u0002\u0007\u0007\u0007\u0002"+
		"\b\u0007\b\u0002\t\u0007\t\u0002\n\u0007\n\u0002\u000b\u0007\u000b\u0002"+
		"\f\u0007\f\u0002\r\u0007\r\u0002\u000e\u0007\u000e\u0002\u000f\u0007\u000f"+
		"\u0002\u0010\u0007\u0010\u0002\u0011\u0007\u0011\u0002\u0012\u0007\u0012"+
		"\u0002\u0013\u0007\u0013\u0002\u0014\u0007\u0014\u0002\u0015\u0007\u0015"+
		"\u0002\u0016\u0007\u0016\u0002\u0017\u0007\u0017\u0002\u0018\u0007\u0018"+
		"\u0002\u0019\u0007\u0019\u0002\u001a\u0007\u001a\u0002\u001b\u0007\u001b"+
		"\u0002\u001c\u0007\u001c\u0002\u001d\u0007\u001d\u0002\u001e\u0007\u001e"+
		"\u0002\u001f\u0007\u001f\u0002 \u0007 \u0001\u0000\u0001\u0000\u0001\u0000"+
		"\u0001\u0000\u0001\u0000\u0001\u0000\u0001\u0000\u0003\u0000J\b\u0000"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0002\u0001\u0002"+
		"\u0003\u0002X\b\u0002\u0001\u0003\u0001\u0003\u0001\u0003\u0001\u0003"+
		"\u0001\u0003\u0001\u0003\u0001\u0003\u0001\u0003\u0003\u0003b\b\u0003"+
		"\u0001\u0004\u0001\u0004\u0003\u0004f\b\u0004\u0001\u0005\u0001\u0005"+
		"\u0001\u0005\u0001\u0005\u0001\u0005\u0001\u0006\u0001\u0006\u0001\u0006"+
		"\u0003\u0006p\b\u0006\u0001\u0007\u0001\u0007\u0003\u0007t\b\u0007\u0001"+
		"\b\u0001\b\u0001\b\u0001\t\u0001\t\u0001\t\u0003\t|\b\t\u0001\n\u0001"+
		"\n\u0001\u000b\u0001\u000b\u0001\u000b\u0001\f\u0001\f\u0003\f\u0085\b"+
		"\f\u0001\r\u0001\r\u0001\r\u0001\r\u0001\r\u0001\r\u0001\r\u0001\r\u0001"+
		"\r\u0001\r\u0001\r\u0001\r\u0003\r\u0093\b\r\u0001\u000e\u0001\u000e\u0001"+
		"\u000e\u0001\u000e\u0001\u000e\u0001\u000e\u0001\u000f\u0001\u000f\u0001"+
		"\u000f\u0001\u000f\u0001\u000f\u0001\u0010\u0001\u0010\u0001\u0010\u0001"+
		"\u0010\u0001\u0010\u0001\u0010\u0001\u0010\u0001\u0010\u0001\u0010\u0001"+
		"\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0001\u0011\u0003"+
		"\u0011\u00af\b\u0011\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001"+
		"\u0012\u0001\u0012\u0001\u0012\u0001\u0012\u0001\u0013\u0001\u0013\u0001"+
		"\u0013\u0001\u0013\u0001\u0014\u0001\u0014\u0001\u0014\u0001\u0014\u0001"+
		"\u0015\u0001\u0015\u0003\u0015\u00c3\b\u0015\u0001\u0016\u0001\u0016\u0001"+
		"\u0016\u0001\u0016\u0001\u0017\u0001\u0017\u0001\u0018\u0001\u0018\u0001"+
		"\u0019\u0001\u0019\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001"+
		"\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001"+
		"\u001a\u0001\u001a\u0003\u001a\u00db\b\u001a\u0001\u001a\u0001\u001a\u0001"+
		"\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001"+
		"\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001"+
		"\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001"+
		"\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0001"+
		"\u001a\u0001\u001a\u0001\u001a\u0001\u001a\u0005\u001a\u00fb\b\u001a\n"+
		"\u001a\f\u001a\u00fe\t\u001a\u0001\u001b\u0001\u001b\u0001\u001b\u0001"+
		"\u001b\u0001\u001b\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001c\u0001"+
		"\u001c\u0001\u001c\u0001\u001c\u0001\u001c\u0001\u001d\u0001\u001d\u0001"+
		"\u001d\u0001\u001d\u0001\u001d\u0001\u001d\u0001\u001d\u0003\u001d\u0114"+
		"\b\u001d\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001\u001e\u0001"+
		"\u001e\u0001\u001f\u0001\u001f\u0001\u001f\u0001\u001f\u0001\u001f\u0001"+
		"\u001f\u0001\u001f\u0001 \u0001 \u0001 \u0001 \u0001 \u0001 \u0001 \u0001"+
		" \u0000\u00014!\u0000\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012\u0014"+
		"\u0016\u0018\u001a\u001c\u001e \"$&(*,.02468:<>@\u0000\u0001\u0001\u0000"+
		"\t\u000b\u012d\u0000I\u0001\u0000\u0000\u0000\u0002K\u0001\u0000\u0000"+
		"\u0000\u0004W\u0001\u0000\u0000\u0000\u0006a\u0001\u0000\u0000\u0000\b"+
		"e\u0001\u0000\u0000\u0000\ng\u0001\u0000\u0000\u0000\fo\u0001\u0000\u0000"+
		"\u0000\u000es\u0001\u0000\u0000\u0000\u0010u\u0001\u0000\u0000\u0000\u0012"+
		"{\u0001\u0000\u0000\u0000\u0014}\u0001\u0000\u0000\u0000\u0016\u007f\u0001"+
		"\u0000\u0000\u0000\u0018\u0084\u0001\u0000\u0000\u0000\u001a\u0092\u0001"+
		"\u0000\u0000\u0000\u001c\u0094\u0001\u0000\u0000\u0000\u001e\u009a\u0001"+
		"\u0000\u0000\u0000 \u009f\u0001\u0000\u0000\u0000\"\u00ae\u0001\u0000"+
		"\u0000\u0000$\u00b0\u0001\u0000\u0000\u0000&\u00b8\u0001\u0000\u0000\u0000"+
		"(\u00bc\u0001\u0000\u0000\u0000*\u00c2\u0001\u0000\u0000\u0000,\u00c4"+
		"\u0001\u0000\u0000\u0000.\u00c8\u0001\u0000\u0000\u00000\u00ca\u0001\u0000"+
		"\u0000\u00002\u00cc\u0001\u0000\u0000\u00004\u00da\u0001\u0000\u0000\u0000"+
		"6\u00ff\u0001\u0000\u0000\u00008\u0104\u0001\u0000\u0000\u0000:\u0113"+
		"\u0001\u0000\u0000\u0000<\u0115\u0001\u0000\u0000\u0000>\u011b\u0001\u0000"+
		"\u0000\u0000@\u0122\u0001\u0000\u0000\u0000BC\u0003\u0002\u0001\u0000"+
		"CD\u0003\u0000\u0000\u0000DJ\u0001\u0000\u0000\u0000EF\u00038\u001c\u0000"+
		"FG\u0003\u0000\u0000\u0000GJ\u0001\u0000\u0000\u0000HJ\u0005\u0000\u0000"+
		"\u0001IB\u0001\u0000\u0000\u0000IE\u0001\u0000\u0000\u0000IH\u0001\u0000"+
		"\u0000\u0000J\u0001\u0001\u0000\u0000\u0000KL\u0005\u0001\u0000\u0000"+
		"LM\u0005\'\u0000\u0000MN\u0005\u0002\u0000\u0000NO\u0003\u0004\u0002\u0000"+
		"OP\u0005\u0003\u0000\u0000PQ\u0003\u0014\n\u0000QR\u0005\u0004\u0000\u0000"+
		"RS\u0003\u0016\u000b\u0000ST\u0005\u0005\u0000\u0000T\u0003\u0001\u0000"+
		"\u0000\u0000UX\u0003\u0006\u0003\u0000VX\u0001\u0000\u0000\u0000WU\u0001"+
		"\u0000\u0000\u0000WV\u0001\u0000\u0000\u0000X\u0005\u0001\u0000\u0000"+
		"\u0000YZ\u0003\u0014\n\u0000Z[\u0005\'\u0000\u0000[b\u0001\u0000\u0000"+
		"\u0000\\]\u0003\u0014\n\u0000]^\u0005\'\u0000\u0000^_\u0005\u0006\u0000"+
		"\u0000_`\u0003\u0006\u0003\u0000`b\u0001\u0000\u0000\u0000aY\u0001\u0000"+
		"\u0000\u0000a\\\u0001\u0000\u0000\u0000b\u0007\u0001\u0000\u0000\u0000"+
		"cf\u0003\n\u0005\u0000df\u0001\u0000\u0000\u0000ec\u0001\u0000\u0000\u0000"+
		"ed\u0001\u0000\u0000\u0000f\t\u0001\u0000\u0000\u0000gh\u0005\'\u0000"+
		"\u0000hi\u0005\u0007\u0000\u0000ij\u00032\u0019\u0000jk\u0003\f\u0006"+
		"\u0000k\u000b\u0001\u0000\u0000\u0000lm\u0005\b\u0000\u0000mp\u0003\n"+
		"\u0005\u0000np\u0001\u0000\u0000\u0000ol\u0001\u0000\u0000\u0000on\u0001"+
		"\u0000\u0000\u0000p\r\u0001\u0000\u0000\u0000qt\u0003\u0010\b\u0000rt"+
		"\u0001\u0000\u0000\u0000sq\u0001\u0000\u0000\u0000sr\u0001\u0000\u0000"+
		"\u0000t\u000f\u0001\u0000\u0000\u0000uv\u00032\u0019\u0000vw\u0003\u0012"+
		"\t\u0000w\u0011\u0001\u0000\u0000\u0000xy\u0005\b\u0000\u0000y|\u0003"+
		"\u0010\b\u0000z|\u0001\u0000\u0000\u0000{x\u0001\u0000\u0000\u0000{z\u0001"+
		"\u0000\u0000\u0000|\u0013\u0001\u0000\u0000\u0000}~\u0007\u0000\u0000"+
		"\u0000~\u0015\u0001\u0000\u0000\u0000\u007f\u0080\u0003\u001a\r\u0000"+
		"\u0080\u0081\u0003\u0018\f\u0000\u0081\u0017\u0001\u0000\u0000\u0000\u0082"+
		"\u0085\u0003\u0016\u000b\u0000\u0083\u0085\u0001\u0000\u0000\u0000\u0084"+
		"\u0082\u0001\u0000\u0000\u0000\u0084\u0083\u0001\u0000\u0000\u0000\u0085"+
		"\u0019\u0001\u0000\u0000\u0000\u0086\u0087\u00032\u0019\u0000\u0087\u0088"+
		"\u0005\f\u0000\u0000\u0088\u0093\u0001\u0000\u0000\u0000\u0089\u0093\u0003"+
		"\u001c\u000e\u0000\u008a\u0093\u0003\u001e\u000f\u0000\u008b\u0093\u0003"+
		" \u0010\u0000\u008c\u0093\u0003$\u0012\u0000\u008d\u0093\u0003&\u0013"+
		"\u0000\u008e\u0093\u0003(\u0014\u0000\u008f\u0093\u0003,\u0016\u0000\u0090"+
		"\u0093\u0003.\u0017\u0000\u0091\u0093\u00030\u0018\u0000\u0092\u0086\u0001"+
		"\u0000\u0000\u0000\u0092\u0089\u0001\u0000\u0000\u0000\u0092\u008a\u0001"+
		"\u0000\u0000\u0000\u0092\u008b\u0001\u0000\u0000\u0000\u0092\u008c\u0001"+
		"\u0000\u0000\u0000\u0092\u008d\u0001\u0000\u0000\u0000\u0092\u008e\u0001"+
		"\u0000\u0000\u0000\u0092\u008f\u0001\u0000\u0000\u0000\u0092\u0090\u0001"+
		"\u0000\u0000\u0000\u0092\u0091\u0001\u0000\u0000\u0000\u0093\u001b\u0001"+
		"\u0000\u0000\u0000\u0094\u0095\u0003\u0014\n\u0000\u0095\u0096\u0005\'"+
		"\u0000\u0000\u0096\u0097\u0005\r\u0000\u0000\u0097\u0098\u00032\u0019"+
		"\u0000\u0098\u0099\u0005\f\u0000\u0000\u0099\u001d\u0001\u0000\u0000\u0000"+
		"\u009a\u009b\u0005\'\u0000\u0000\u009b\u009c\u0005\r\u0000\u0000\u009c"+
		"\u009d\u00032\u0019\u0000\u009d\u009e\u0005\f\u0000\u0000\u009e\u001f"+
		"\u0001\u0000\u0000\u0000\u009f\u00a0\u0005\u000e\u0000\u0000\u00a0\u00a1"+
		"\u0005\u0002\u0000\u0000\u00a1\u00a2\u00032\u0019\u0000\u00a2\u00a3\u0005"+
		"\u000f\u0000\u0000\u00a3\u00a4\u0005\u0010\u0000\u0000\u00a4\u00a5\u0003"+
		"\u0016\u000b\u0000\u00a5\u00a6\u0005\u0005\u0000\u0000\u00a6\u00a7\u0003"+
		"\"\u0011\u0000\u00a7!\u0001\u0000\u0000\u0000\u00a8\u00a9\u0005\u0011"+
		"\u0000\u0000\u00a9\u00aa\u0005\u0010\u0000\u0000\u00aa\u00ab\u0003\u0016"+
		"\u000b\u0000\u00ab\u00ac\u0005\u0005\u0000\u0000\u00ac\u00af\u0001\u0000"+
		"\u0000\u0000\u00ad\u00af\u0001\u0000\u0000\u0000\u00ae\u00a8\u0001\u0000"+
		"\u0000\u0000\u00ae\u00ad\u0001\u0000\u0000\u0000\u00af#\u0001\u0000\u0000"+
		"\u0000\u00b0\u00b1\u0005\u0012\u0000\u0000\u00b1\u00b2\u0005\u0002\u0000"+
		"\u0000\u00b2\u00b3\u00032\u0019\u0000\u00b3\u00b4\u0005\u000f\u0000\u0000"+
		"\u00b4\u00b5\u0005\u0010\u0000\u0000\u00b5\u00b6\u0003\u0016\u000b\u0000"+
		"\u00b6\u00b7\u0005\u0005\u0000\u0000\u00b7%\u0001\u0000\u0000\u0000\u00b8"+
		"\u00b9\u0005\u0013\u0000\u0000\u00b9\u00ba\u0005\'\u0000\u0000\u00ba\u00bb"+
		"\u0005\f\u0000\u0000\u00bb\'\u0001\u0000\u0000\u0000\u00bc\u00bd\u0005"+
		"\u0014\u0000\u0000\u00bd\u00be\u0003*\u0015\u0000\u00be\u00bf\u0005\f"+
		"\u0000\u0000\u00bf)\u0001\u0000\u0000\u0000\u00c0\u00c3\u00032\u0019\u0000"+
		"\u00c1\u00c3\u0001\u0000\u0000\u0000\u00c2\u00c0\u0001\u0000\u0000\u0000"+
		"\u00c2\u00c1\u0001\u0000\u0000\u0000\u00c3+\u0001\u0000\u0000\u0000\u00c4"+
		"\u00c5\u0005\u0015\u0000\u0000\u00c5\u00c6\u0003>\u001f\u0000\u00c6\u00c7"+
		"\u0005\f\u0000\u0000\u00c7-\u0001\u0000\u0000\u0000\u00c8\u00c9\u0005"+
		"\u0016\u0000\u0000\u00c9/\u0001\u0000\u0000\u0000\u00ca\u00cb\u0005\u0017"+
		"\u0000\u0000\u00cb1\u0001\u0000\u0000\u0000\u00cc\u00cd\u00034\u001a\u0000"+
		"\u00cd3\u0001\u0000\u0000\u0000\u00ce\u00cf\u0006\u001a\uffff\uffff\u0000"+
		"\u00cf\u00d0\u0005\u001b\u0000\u0000\u00d0\u00db\u00034\u001a\u0006\u00d1"+
		"\u00d2\u0005\"\u0000\u0000\u00d2\u00db\u00034\u001a\u0005\u00d3\u00db"+
		"\u00036\u001b\u0000\u00d4\u00d5\u0005\u0002\u0000\u0000\u00d5\u00d6\u0003"+
		"4\u001a\u0000\u00d6\u00d7\u0005\u000f\u0000\u0000\u00d7\u00db\u0001\u0000"+
		"\u0000\u0000\u00d8\u00db\u0005\'\u0000\u0000\u00d9\u00db\u0005)\u0000"+
		"\u0000\u00da\u00ce\u0001\u0000\u0000\u0000\u00da\u00d1\u0001\u0000\u0000"+
		"\u0000\u00da\u00d3\u0001\u0000\u0000\u0000\u00da\u00d4\u0001\u0000\u0000"+
		"\u0000\u00da\u00d8\u0001\u0000\u0000\u0000\u00da\u00d9\u0001\u0000\u0000"+
		"\u0000\u00db\u00fc\u0001\u0000\u0000\u0000\u00dc\u00dd\n\u0010\u0000\u0000"+
		"\u00dd\u00de\u0005\u0018\u0000\u0000\u00de\u00fb\u00034\u001a\u0011\u00df"+
		"\u00e0\n\u000f\u0000\u0000\u00e0\u00e1\u0005\u0019\u0000\u0000\u00e1\u00fb"+
		"\u00034\u001a\u0010\u00e2\u00e3\n\u000e\u0000\u0000\u00e3\u00e4\u0005"+
		"\u001a\u0000\u0000\u00e4\u00fb\u00034\u001a\u000f\u00e5\u00e6\n\r\u0000"+
		"\u0000\u00e6\u00e7\u0005\u001b\u0000\u0000\u00e7\u00fb\u00034\u001a\u000e"+
		"\u00e8\u00e9\n\f\u0000\u0000\u00e9\u00ea\u0005\u001c\u0000\u0000\u00ea"+
		"\u00fb\u00034\u001a\r\u00eb\u00ec\n\u000b\u0000\u0000\u00ec\u00ed\u0005"+
		"\u001d\u0000\u0000\u00ed\u00fb\u00034\u001a\f\u00ee\u00ef\n\n\u0000\u0000"+
		"\u00ef\u00f0\u0005\u001e\u0000\u0000\u00f0\u00fb\u00034\u001a\u000b\u00f1"+
		"\u00f2\n\t\u0000\u0000\u00f2\u00f3\u0005\u001f\u0000\u0000\u00f3\u00fb"+
		"\u00034\u001a\n\u00f4\u00f5\n\b\u0000\u0000\u00f5\u00f6\u0005 \u0000\u0000"+
		"\u00f6\u00fb\u00034\u001a\t\u00f7\u00f8\n\u0007\u0000\u0000\u00f8\u00f9"+
		"\u0005!\u0000\u0000\u00f9\u00fb\u00034\u001a\b\u00fa\u00dc\u0001\u0000"+
		"\u0000\u0000\u00fa\u00df\u0001\u0000\u0000\u0000\u00fa\u00e2\u0001\u0000"+
		"\u0000\u0000\u00fa\u00e5\u0001\u0000\u0000\u0000\u00fa\u00e8\u0001\u0000"+
		"\u0000\u0000\u00fa\u00eb\u0001\u0000\u0000\u0000\u00fa\u00ee\u0001\u0000"+
		"\u0000\u0000\u00fa\u00f1\u0001\u0000\u0000\u0000\u00fa\u00f4\u0001\u0000"+
		"\u0000\u0000\u00fa\u00f7\u0001\u0000\u0000\u0000\u00fb\u00fe\u0001\u0000"+
		"\u0000\u0000\u00fc\u00fa\u0001\u0000\u0000\u0000\u00fc\u00fd\u0001\u0000"+
		"\u0000\u0000\u00fd5\u0001\u0000\u0000\u0000\u00fe\u00fc\u0001\u0000\u0000"+
		"\u0000\u00ff\u0100\u0005\'\u0000\u0000\u0100\u0101\u0005\u0002\u0000\u0000"+
		"\u0101\u0102\u0003\u000e\u0007\u0000\u0102\u0103\u0005\u000f\u0000\u0000"+
		"\u01037\u0001\u0000\u0000\u0000\u0104\u0105\u0005#\u0000\u0000\u0105\u0106"+
		"\u0005\'\u0000\u0000\u0106\u0107\u0005\u0002\u0000\u0000\u0107\u0108\u0003"+
		"\u0004\u0002\u0000\u0108\u0109\u0005$\u0000\u0000\u0109\u010a\u0003:\u001d"+
		"\u0000\u010a\u010b\u0005\u0005\u0000\u0000\u010b9\u0001\u0000\u0000\u0000"+
		"\u010c\u010d\u0003<\u001e\u0000\u010d\u010e\u0003:\u001d\u0000\u010e\u0114"+
		"\u0001\u0000\u0000\u0000\u010f\u0110\u0003\u0002\u0001\u0000\u0110\u0111"+
		"\u0003:\u001d\u0000\u0111\u0114\u0001\u0000\u0000\u0000\u0112\u0114\u0001"+
		"\u0000\u0000\u0000\u0113\u010c\u0001\u0000\u0000\u0000\u0113\u010f\u0001"+
		"\u0000\u0000\u0000\u0113\u0112\u0001\u0000\u0000\u0000\u0114;\u0001\u0000"+
		"\u0000\u0000\u0115\u0116\u0005%\u0000\u0000\u0116\u0117\u0005\'\u0000"+
		"\u0000\u0117\u0118\u0005\u0010\u0000\u0000\u0118\u0119\u0003\u0016\u000b"+
		"\u0000\u0119\u011a\u0005\u0005\u0000\u0000\u011a=\u0001\u0000\u0000\u0000"+
		"\u011b\u011c\u0005\'\u0000\u0000\u011c\u011d\u0005&\u0000\u0000\u011d"+
		"\u011e\u0005\'\u0000\u0000\u011e\u011f\u0005\u0002\u0000\u0000\u011f\u0120"+
		"\u0003\b\u0004\u0000\u0120\u0121\u0005\u000f\u0000\u0000\u0121?\u0001"+
		"\u0000\u0000\u0000\u0122\u0123\u0005\'\u0000\u0000\u0123\u0124\u0005&"+
		"\u0000\u0000\u0124\u0125\u0005(\u0000\u0000\u0125\u0126\u0005\u0002\u0000"+
		"\u0000\u0126\u0127\u0003\b\u0004\u0000\u0127\u0128\u0005\u000f\u0000\u0000"+
		"\u0128A\u0001\u0000\u0000\u0000\u000fIWaeos{\u0084\u0092\u00ae\u00c2\u00da"+
		"\u00fa\u00fc\u0113";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}