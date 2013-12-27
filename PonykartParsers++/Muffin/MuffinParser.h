#ifndef MUFFINPARSER_H_INCLUDED
#define MUFFINPARSER_H_INCLUDED

#include <stack>
#include <string>
#include <unordered_map>
#include <vector>

namespace PonykartParsers
{
namespace MuffinParser
{
	enum NodeType
	{
		Tok_EOF,
		Tok_Assign,
		Tok_Comma,
		Tok_LBrace,
		Tok_RBrace,
		Tok_Name,
		Tok_KeyFalse,
		Tok_KeyTrue,
		Tok_StringLiteral,
		Tok_FloatLiteral,
		Tok_IntLiteral,
		Tok_SingleLineComment,
		Tok_MultiLineComment,
		Tok_Whitespace,
		Rule_Start,
		Rule_Property,
		Rule_EnumProperty,
		Rule_QuatProperty,
		Rule_Vec3Property,
		Rule_NumericProperty,
		Rule_StringProperty,
		Rule_BoolProperty,
		Rule_Block,
		Rule_AnyName
	};

	std::string NodeTypeMap[] = {"Tok_EOF", "Tok_Assign", "Tok_Comma", "Tok_LBrace", "Tok_RBrace",
	"Tok_Name,Tok_KeyFalse", "Tok_KeyTrue", "Tok_StringLiteral", "Tok_FloatLiteral", "Tok_IntLiteral",
	"Tok_SingleLineComment", "Tok_MultiLineComment", "Tok_Whitespace", "Rule_Start", "Rule_Property",
	"Rule_EnumProperty", "Rule_QuatProperty", "Rule_Vec3Property", "Rule_NumericProperty", "Rule_StringProperty",
	"Rule_BoolProperty", "Rule_Block", "Rule_AnyName"};

	struct Node // Abstract
	{
		Node(NodeType Type) : type(Type) {};
		static std::string TypeName(NodeType Type)
		{
			std::string s = NodeTypeMap[Type];
			return s.substr(s.find('_') + 1);
		}

		const NodeType type;
	};

	/// Represents an inner node
	struct RuleInstance : Node
	{
		RuleInstance(NodeType type, const Node** Children) : Node(type), children(Children) {};

		const Node** children;
	};

	/// Represents a leaf node
	class Token : Node
	{
	public:
		Token();
		Token(Token** PrecedingFillerTokens, NodeType Type, const std::string& Image, int LineNr, int CharNr);
	private:
		static NodeType specializeType(NodeType Type, const std::string& Image);
	public:
		const Token** precedingFillerTokens;
		const std::string image;
		const int lineNr, charNr;
	private:
		static std::unordered_map<std::string, NodeType> specForTok_Name;
	};

	class Parser
	{
	public:
		Parser() = default;
		RuleInstance* parse(std::string source);
	private:
		Token* nextToken();
		Token* fetchToken(int offset);
		Token** getFillerTokens();
		Token* nextToken(bool useFetched);
		RuleInstance* matchStart();
		RuleInstance* matchProperty();
		RuleInstance* matchEnumProperty();
		RuleInstance* matchQuatProperty();
		RuleInstance* matchVec3Property();
		RuleInstance* matchNumericProperty();
		RuleInstance* matchStringProperty();
		RuleInstance* matchBoolProperty();
		RuleInstance* matchBlock();
		RuleInstance* matchAnyName();
		void lookaheadEnumProperty();
		void lookaheadNumericProperty();
		void lookaheadBoolProperty();
		void lookaheadAnyName();
	private:
		std::string source;
		int index, length, currLine, currChar, laOffset;
		const std::vector<Token*> fetchedTokens;
		const std::stack<bool> laSuccess;
		const std::stack<int> laOffsets;
		const std::stack<bool> onceOrMoreB;
		const std::vector<Token*> tokens;
		const std::stack<int> indices, currLines, currChars;
	};
} // MuffinParser
} // PonykartParsers

#endif // MUFFINPARSER_H_INCLUDED
